using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("�ړ��ݒ�")]
    [Tooltip("�L�����N�^�[�̈ړ����x")]
    public float moveSpeed = 5.0f;

    [Header("�W�����v�ݒ�")]
    [Tooltip("�W�����v�̋���")]
    public float jumpForce = 7.0f;
    [Tooltip("�n�ʂ𔻒肷�邽�߂̃^�O��")]
    public string groundTag = "Ground";

    // �ڒn���Ă��邩�ǂ����𔻒肷��t���O
    private bool isGrounded = false;

    private Rigidbody rb;
    private Vector3 moveInput;

    void Awake()
    {
        // Rigidbody�R���|�[�l���g���擾
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // --- 1. �ړ����͂̎�t ---
        float moveX = Input.GetAxisRaw("Horizontal"); // A, D�L�[
        float moveZ = Input.GetAxisRaw("Vertical");   // W, S�L�[

        moveInput = (transform.forward * moveZ + transform.right * moveX).normalized;

        // --- 2. �W�����v���͂̎�t ---
        // �X�y�[�X�L�[�������ꂽ�u�ԁA���A�n�ʂɐڒn���Ă���ꍇ
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // --- 3. �������Z�ɂ��ړ� ---
        // ���݂�Y�������̑��x��ێ����A�ړ���K�p
        Vector3 newVelocity = moveInput * moveSpeed;

        // �C���_: .velocity -> .linearVelocity �ɕύX
        newVelocity.y = rb.linearVelocity.y;

        // �C���_: .velocity -> .linearVelocity �ɕύX
        rb.linearVelocity = newVelocity;
    }

    /// <summary>
    /// �W�����v����
    /// </summary>
    private void Jump()
    {
        // �W�����v�����u�Ԃɐڒn�t���O��false�ɂ���i�A���W�����v��h�����߁j
        isGrounded = false;

        // Y�������̑��x����x���Z�b�g���Ă���A������ɗ͂�������
        // ����ɂ��A�������ł����肵�������ŃW�����v�ł���
        // �C���_: .velocity -> .linearVelocity �ɕύX
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // --- 4. �ڒn���� ---
    // �I�u�W�F�N�g�������ɐڐG�����u�ԂɌĂяo�����
    private void OnCollisionEnter(Collision collision)
    {
        // �ڐG�����I�u�W�F�N�g�̃^�O���ݒ肵��groundTag�Ɠ����ꍇ
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
        }
    }

    // �I�u�W�F�N�g���������痣�ꂽ�u�ԂɌĂяo�����
    private void OnCollisionExit(Collision collision)
    {
        // ���ꂽ�I�u�W�F�N�g�̃^�O���ݒ肵��groundTag�Ɠ����ꍇ
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = false;
        }
    }
}