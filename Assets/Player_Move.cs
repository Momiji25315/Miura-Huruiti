using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("キャラクターの移動速度")]
    public float moveSpeed = 5.0f;

    [Header("ジャンプ設定")]
    [Tooltip("ジャンプの強さ")]
    public float jumpForce = 7.0f;
    [Tooltip("地面を判定するためのタグ名")]
    public string groundTag = "Ground";

    // 接地しているかどうかを判定するフラグ
    private bool isGrounded = false;

    private Rigidbody rb;
    private Vector3 moveInput;

    void Awake()
    {
        // Rigidbodyコンポーネントを取得
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // --- 1. 移動入力の受付 ---
        float moveX = Input.GetAxisRaw("Horizontal"); // A, Dキー
        float moveZ = Input.GetAxisRaw("Vertical");   // W, Sキー

        moveInput = (transform.forward * moveZ + transform.right * moveX).normalized;

        // --- 2. ジャンプ入力の受付 ---
        // スペースキーが押された瞬間、かつ、地面に接地している場合
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // --- 3. 物理演算による移動 ---
        // 現在のY軸方向の速度を保持しつつ、移動を適用
        Vector3 newVelocity = moveInput * moveSpeed;

        // 修正点: .velocity -> .linearVelocity に変更
        newVelocity.y = rb.linearVelocity.y;

        // 修正点: .velocity -> .linearVelocity に変更
        rb.linearVelocity = newVelocity;
    }

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    private void Jump()
    {
        // ジャンプした瞬間に接地フラグをfalseにする（連続ジャンプを防ぐため）
        isGrounded = false;

        // Y軸方向の速度を一度リセットしてから、上向きに力を加える
        // これにより、落下中でも安定した高さでジャンプできる
        // 修正点: .velocity -> .linearVelocity に変更
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // --- 4. 接地判定 ---
    // オブジェクトが何かに接触した瞬間に呼び出される
    private void OnCollisionEnter(Collision collision)
    {
        // 接触したオブジェクトのタグが設定したgroundTagと同じ場合
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
        }
    }

    // オブジェクトが何かから離れた瞬間に呼び出される
    private void OnCollisionExit(Collision collision)
    {
        // 離れたオブジェクトのタグが設定したgroundTagと同じ場合
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = false;
        }
    }
}