using System.Collections;
using UnityEngine;

// Rigidbodyコンポーネントが必須であることを示す
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

    [Header("Noddy能力設定")]
    [Tooltip("能力発動時の回復量")]
    public float abilityHealAmount = 8f;
    [Tooltip("能力発動中の操作不能時間")]
    public float abilityDisableDuration = 2.5f;
    [Tooltip("能力獲得時の色")]
    public Color abilityColor = Color.magenta;

    // --- 内部で使用する変数 ---
    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;
    private Coroutine showObjectCoroutine;

    // Noddy能力関連の内部変数
    private Renderer playerRenderer;
    private Color originalColor;
    private bool hasNoddyAbility = false;
    private bool controlsDisabled = false;

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
        if (shiftObject != null)
        {
            shiftObject.SetActive(false);
        }
        currentHealth = maxHealth;
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
        // Noddy能力の入力
        if (hasNoddyAbility && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ActivateNoddyAbility());
            return;
        }
        if (hasNoddyAbility && Input.GetMouseButtonDown(1))
        {
            DiscardNoddyAbility();
        }

        // 移動入力（ワールド座標基準）
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(moveX, 0f, moveZ).normalized;

        // ジャンプ入力
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // Shiftキー入力
        HandleShiftObjectWithDelay();

        // デバッグ用ダメージキー
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10);
        }
    }

    private void MoveCharacter()
    {
        // 移動
        float currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        Vector3 newVelocity = moveInput * currentMoveSpeed;
        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;

        // 回転
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

    // --- Noddy能力関連のメソッド ---
    public void GainNoddyAbility()
    {
        if (hasNoddyAbility) return;
        hasNoddyAbility = true;
        if (playerRenderer != null) playerRenderer.material.color = abilityColor;
        Debug.Log("Noddyの能力を獲得した！ 左クリックで発動、右クリックで破棄できます。");
    }

    private IEnumerator ActivateNoddyAbility()
    {
        hasNoddyAbility = false;
        controlsDisabled = true;
        rb.linearVelocity = Vector3.zero;
        Debug.Log("能力発動！ " + abilityDisableDuration + "秒間、操作不能になります...");
        yield return new WaitForSeconds(abilityDisableDuration);
        Heal(abilityHealAmount);
        if (playerRenderer != null) playerRenderer.material.color = originalColor;
        controlsDisabled = false;
        Debug.Log("HPが回復し、操作可能になりました。能力は失われました。");
    }

    private void DiscardNoddyAbility()
    {
        hasNoddyAbility = false;
        if (playerRenderer != null) playerRenderer.material.color = originalColor;
        Debug.Log("Noddyの能力を捨て、元の状態に戻りました。");
    }

    // --- HP・ダメージ関連のメソッド ---
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        Debug.Log(amount + " 回復した！ 現在のHP: " + currentHealth);
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        Debug.Log(gameObject.name + " が " + damageAmount + " のダメージを受けた！ 残りHP: " + currentHealth);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " は力尽きた...");
        gameObject.SetActive(false);
    }

    // --- Shiftキー関連のメソッド ---
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