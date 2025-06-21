using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("体力設定")]
    [Tooltip("最大HP")]
    public float maxHealth = 100f;
    [Tooltip("現在のHP")]
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
    [Tooltip("地面を判定するためのタグ名")]
    public string groundTag = "Ground";

    [Header("特殊アクション")]
    [Tooltip("Shiftキーで表示/非表示を切り替える子オブジェクト")]
    public GameObject shiftObject;
    [Tooltip("Shiftキーを押してからオブジェクトが表示されるまでの時間(秒)")]
    public float shiftDelay = 0.3f;

    private bool isGrounded = false;
    private Rigidbody rb;
    private Vector3 moveInput;
    private Coroutine showObjectCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
        // 移動入力
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveDirection = cameraForward * moveZ + Camera.main.transform.right * moveX;
        moveInput = moveDirection.normalized;

        // ジャンプ入力
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // 回転処理
        RotateCharacter();

        // Shiftキー処理
        HandleShiftObjectWithDelay();

        // デバッグ用ダメージ処理
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10);
        }
    }

    void FixedUpdate()
    {
        float currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        Vector3 newVelocity = moveInput * currentMoveSpeed;
        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        Debug.Log(gameObject.name + " が " + damageAmount + " のダメージを受けた！ 残りHP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " は力尽きた...");
        gameObject.SetActive(false);
    }

    private void Jump()
    {
        isGrounded = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void RotateCharacter()
    {
        if (moveInput == Vector3.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(moveInput);
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleShiftObjectWithDelay()
    {
        if (shiftObject == null) return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (showObjectCoroutine != null)
            {
                StopCoroutine(showObjectCoroutine);
            }
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

    // ★★★ エラーの原因箇所 ★★★
    // 正しいコードはこちらです
    private IEnumerator ShowObjectAfterDelay()
    {
        yield return new WaitForSeconds(shiftDelay);
        shiftObject.SetActive(true);
        showObjectCoroutine = null;
    }
    // ★★★★★★★★★★★★★★★

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = false;
        }
    }
}