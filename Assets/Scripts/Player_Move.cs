using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("体力設定")]
    public float maxHealth = 100f;
    [SerializeField]
    private float currentHealth;

    [Header("移動設定")]
    public float moveSpeed = 5.0f;
    public float sprintSpeed = 10.0f;
    public float rotationSpeed = 10.0f;

    [Header("ジャンプ設定")]
    public float jumpForce = 7.0f;

    [Header("接地判定")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("特殊アクション（Shiftキー）")]
    public GameObject shiftObject;
    public float shiftDelay = 0.3f;

    [Header("回復能力設定（Noddy）")]
    public float abilityHealAmount = 8f;
    public float noddyAbilityDisableDuration = 2.5f;
    public Color noddyAbilityColor = Color.magenta;

    [Header("爆発能力設定（Bomber）")]
    public float bomberAbilityDamage = 15f;
    public float bomberAbilityRadius = 2f;
    public float bomberAbilityDisableDuration = 2.0f;
    public Color bomberAbilityColor = Color.black;
    public GameObject bomberAbilityEffectPrefab;
    public float bomberAbilityEffectDuration = 1.0f;

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

        if ((hasNoddyAbility || hasBomberAbility) && Input.GetMouseButtonDown(1))
        {
            DiscardAnyAbility();
        }

        // 移動入力（カメラの向きを基準にする）
        float moveX = Input.GetAxisRaw("Horizontal"); // A, Dキー
        float moveZ = Input.GetAxisRaw("Vertical");   // W, Sキー
        // カメラの前方ベクトル（Y軸を無視して水平にする）
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        // カメラの向きを基準に、移動方向を計算
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOverScreen();
        }
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