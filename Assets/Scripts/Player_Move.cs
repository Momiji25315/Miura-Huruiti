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

    // ゲームオブジェクトが生成された直後に一度だけ呼ばれる
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            // ゲーム開始時のマテリアルの色を保存
            originalColor = playerRenderer.material.color;
        }
    }

    // 最初のフレーム更新の前に一度だけ呼ばれる
    void Start()
    {
        // Shiftキーオブジェクトが設定されていれば、最初は非表示にする
        if (shiftObject != null)
        {
            shiftObject.SetActive(false);
        }
        // HPを最大値に設定
        currentHealth = maxHealth;
    }

    // 毎フレーム呼ばれる
    void Update()
    {
        // 1. 接地判定を毎フレーム行う
        CheckIfGrounded();

        // 2. 操作不能状態なら、以降の入力を受け付けない
        if (controlsDisabled) return;

        // 3. 入力受付
        HandleInput();
    }

    // 固定フレームレートで呼び出される（物理演算はこちら）
    void FixedUpdate()
    {
        // 操作不能状態なら、移動処理を行わない
        if (controlsDisabled) return;

        // 4. 物理的な移動処理
        MoveCharacter();
    }

    /// <summary>
    /// 接地しているかどうかの判定
    /// </summary>
    private void CheckIfGrounded()
    {
        // Physics.CheckSphereを使って、足元に球状の判定領域を作り、地面レイヤーと接触しているかチェック
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    /// <summary>
    /// キーボードやマウスからの入力をまとめて処理
    /// </summary>
    private void HandleInput()
    {
        // Noddy能力の発動（左クリック）
        if (hasNoddyAbility && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ActivateNoddyAbility());
            return; // 能力を使ったフレームでは他の操作はしない
        }

        // Noddy能力の破棄（右クリック）
        if (hasNoddyAbility && Input.GetMouseButtonDown(1))
        {
            DiscardNoddyAbility();
        }

        // 移動入力
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveDirection = cameraForward * moveZ + Camera.main.transform.right * moveX;
        moveInput = moveDirection.normalized;

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

    /// <summary>
    /// Rigidbodyを使ったキャラクターの移動と回転
    /// </summary>
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

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    private void Jump()
    {
        // Y軸の速度を一度リセットしてから力を加えることで、安定したジャンプ力を得られる
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
        rb.linearVelocity = Vector3.zero; // 移動を停止
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