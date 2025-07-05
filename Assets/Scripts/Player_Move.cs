using System.Collections;
using UnityEngine;

// このスクリプトがアタッチされたオブジェクトにはRigidbodyが必須であることを示す
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("体力設定")]
    [Tooltip("最大HP")]
    public float maxHealth = 100f;
    [Tooltip("現在のHP（実行中に確認用）")]
    [SerializeField]
    private float currentHealth;

    [Header("移動設定")]
    [Tooltip("キャラクターの通常移動速度")]
    public float moveSpeed = 5.0f;
    [Tooltip("Shiftキーを押している間の移動速度")]
    public float sprintSpeed = 10.0f;
    [Tooltip("キャラクターの回転速度")]
    public float rotationSpeed = 10.0f;

    [Header("ジャンプ設定")]
    [Tooltip("ジャンプの強さ")]
    public float jumpForce = 7.0f;

    [Header("接地判定")]
    [Tooltip("地面と認識するレイヤー")]
    public LayerMask groundLayer;
    [Tooltip("接地判定の中心点（プレイヤーの足元に配置）")]
    public Transform groundCheck;
    [Tooltip("接地判定の球体の半径")]
    public float groundCheckRadius = 0.2f;

    [Header("特殊アクション（Shiftキー）")]
    [Tooltip("Shiftキーで表示/非表示を切り替える子オブジェクト")]
    public GameObject shiftObject;
    [Tooltip("Shiftキーを押してからオブジェクトが表示されるまでの時間(秒)")]
    public float shiftDelay = 0.3f;

    [Header("回復能力設定（Noddy）")]
    [Tooltip("能力発動時の回復量")]
    public float abilityHealAmount = 8f;
    [Tooltip("操作不能時間")]
    public float noddyAbilityDisableDuration = 2.5f;
    [Tooltip("能力獲得時の色")]
    public Color noddyAbilityColor = Color.magenta;

    [Header("爆発能力設定（Bomber）")]
    [Tooltip("爆発ダメージ")]
    public float bomberAbilityDamage = 15f;
    [Tooltip("爆発範囲")]
    public float bomberAbilityRadius = 2f;
    [Tooltip("操作不能時間")]
    public float bomberAbilityDisableDuration = 2.0f;
    [Tooltip("能力獲得時の色")]
    public Color bomberAbilityColor = Color.black;
    [Tooltip("爆発能力使用時のエフェクトプレハブ")]
    public GameObject bomberAbilityEffectPrefab;
    [Tooltip("爆発エフェクトの表示時間")]
    public float bomberAbilityEffectDuration = 1.0f;

    // --- 内部変数 ---
    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;
    private Coroutine showObjectCoroutine;

    private Renderer playerRenderer;
    private Color originalColor;
    private bool hasNoddyAbility = false;
    private bool hasBomberAbility = false;
    private bool controlsDisabled = false;

    private UIManager uiManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
    }

    void Start()
    {
        // シーン内のUIManagerを探して取得（新しいFindAnyObjectByTypeを使用）
        uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManagerが見つかりません！");
        }

        currentHealth = maxHealth;
        if (uiManager != null)
        {
            uiManager.SetMaxHealth(maxHealth);
        }

        if (shiftObject != null) shiftObject.SetActive(false);
    }

    void Update()
    {
        CheckIfGrounded();
        if (controlsDisabled) return;
        HandleInput();
    }

    void FixedUpdate()
    {
        if (controlsDisabled) return;
        MoveCharacter();
    }

    private void CheckIfGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleInput()
    {
        // 能力発動（左クリック）
        if (hasNoddyAbility && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ActivateNoddyAbility());
            return;
        }
        else if (hasBomberAbility && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ActivateBomberAbility());
            return;
        }

        // 能力破棄（右クリック）
        if ((hasNoddyAbility || hasBomberAbility) && Input.GetMouseButtonDown(1))
        {
            DiscardAnyAbility();
        }

        // 移動入力（カメラ基準）
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveDirection = cameraForward * moveZ + Camera.main.transform.right * moveX;
        moveInput = moveDirection.normalized;

        if (isGrounded && Input.GetKeyDown(KeyCode.Space)) Jump();
        HandleShiftObjectWithDelay();
        if (Input.GetKeyDown(KeyCode.K)) TakeDamage(10);
    }

    private void MoveCharacter()
    {
        float currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        Vector3 newVelocity = moveInput * currentMoveSpeed;
        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;

        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void GainNoddyAbility()
    {
        ResetAbilityState();
        hasNoddyAbility = true;
        if (playerRenderer != null) playerRenderer.material.color = noddyAbilityColor;
        Debug.Log("回復能力を獲得！");
    }

    public void GainBomberAbility()
    {
        ResetAbilityState();
        hasBomberAbility = true;
        if (playerRenderer != null) playerRenderer.material.color = bomberAbilityColor;
        Debug.Log("爆発能力を獲得！");
    }

    private IEnumerator ActivateNoddyAbility()
    {
        controlsDisabled = true;
        rb.linearVelocity = Vector3.zero;
        Debug.Log("回復能力発動！ " + noddyAbilityDisableDuration + "秒間、操作不能...");
        yield return new WaitForSeconds(noddyAbilityDisableDuration);
        Heal(abilityHealAmount);
        ResetAbilityState();
        controlsDisabled = false;
        Debug.Log("HPが回復し、操作可能になりました。");
    }

    private IEnumerator ActivateBomberAbility()
    {
        controlsDisabled = true;
        rb.linearVelocity = Vector3.zero;
        Debug.Log("爆発能力発動！ " + bomberAbilityDisableDuration + "秒間、操作不能...");
        ExplodeWithAbility();
        yield return new WaitForSeconds(bomberAbilityDisableDuration);
        ResetAbilityState();
        controlsDisabled = false;
        Debug.Log("操作可能になりました。");
    }

    private void ExplodeWithAbility()
    {
        Debug.Log("BOOM! 範囲内の特定の敵にダメージを与えます。");

        if (bomberAbilityEffectPrefab != null)
        {
            GameObject effect = Instantiate(bomberAbilityEffectPrefab, transform.position, Quaternion.identity);
            float diameter = bomberAbilityRadius * 2f;
            effect.transform.localScale = new Vector3(diameter, diameter, diameter);
            Destroy(effect, bomberAbilityEffectDuration);
        }

        Collider[] collidersToDamage = Physics.OverlapSphere(transform.position, bomberAbilityRadius);
        foreach (var col in collidersToDamage)
        {
            if (col.CompareTag("Noddy") || col.CompareTag("Bomber"))
            {
                if (col.TryGetComponent<EnemyAI>(out var enemy1))
                {
                    enemy1.TakeDamage(bomberAbilityDamage);
                }
                else if (col.TryGetComponent<FallingBomberAI>(out var enemy2))
                {
                    enemy2.TakeDamage(bomberAbilityDamage);
                }
            }
        }
    }

    private void DiscardAnyAbility()
    {
        ResetAbilityState();
        Debug.Log("能力を捨て、元の状態に戻りました。");
    }

    private void ResetAbilityState()
    {
        hasNoddyAbility = false;
        hasBomberAbility = false;
        if (playerRenderer != null)
        {
            playerRenderer.material.color = originalColor;
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth);
        }

        Debug.Log(amount + " 回復した！ 現在のHP: " + currentHealth);
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth);
        }

        Debug.Log(gameObject.name + " が " + damageAmount + " のダメージを受けた！ 残りHP: " + currentHealth);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " は力尽きた...");

        // GameManagerにゲームオーバーを通知する
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOverScreen();
        }

        // プレイヤーオブジェクトを非表示にする
        gameObject.SetActive(false);
    }

    private void HandleShiftObjectWithDelay()
    {
        if (shiftObject == null) return;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (showObjectCoroutine != null) StopCoroutine(showObjectCoroutine);
            showObjectCoroutine = StartCoroutine(ShowObjectAfterDelay());
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            if (showObjectCoroutine != null)
            {
                StopCoroutine(showObjectCoroutine);
                showObjectCoroutine = null;
            }
            shiftObject.SetActive(false);
        }
    }

    private IEnumerator ShowObjectAfterDelay()
    {
        yield return new WaitForSeconds(shiftDelay);
        shiftObject.SetActive(true);
        showObjectCoroutine = null;
    }
}