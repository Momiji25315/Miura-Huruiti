using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("�̗͐ݒ�")]
    [Tooltip("�ő�HP")]
    public float maxHealth = 100f;
    [Tooltip("���݂�HP")]
    [SerializeField]
    private float currentHealth;

    [Header("�ړ��ݒ�")]
    [Tooltip("�L�����N�^�[�̒ʏ�ړ����x")]
    public float moveSpeed = 5.0f;
    [Tooltip("Shift�L�[�������Ă���Ԃ̈ړ����x")]
    public float sprintSpeed = 10.0f;
    [Tooltip("�L�����N�^�[�̉�]���x")]
    public float rotationSpeed = 10.0f;

    [Header("�W�����v�ݒ�")]
    [Tooltip("�W�����v�̋���")]
    public float jumpForce = 7.0f;
    [Tooltip("�n�ʂ𔻒肷�邽�߂̃^�O��")]
    public string groundTag = "Ground";

    [Header("����A�N�V����")]
    [Tooltip("Shift�L�[�ŕ\��/��\����؂�ւ���q�I�u�W�F�N�g")]
    public GameObject shiftObject;
    [Tooltip("Shift�L�[�������Ă���I�u�W�F�N�g���\�������܂ł̎���(�b)")]
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
        // �ړ�����
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveDirection = cameraForward * moveZ + Camera.main.transform.right * moveX;
        moveInput = moveDirection.normalized;

        // �W�����v����
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // ��]����
        RotateCharacter();

        // Shift�L�[����
        HandleShiftObjectWithDelay();

        // �f�o�b�O�p�_���[�W����
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
        Debug.Log(gameObject.name + " �� " + damageAmount + " �̃_���[�W���󂯂��I �c��HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " �͗͐s����...");
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

    // ������ �G���[�̌����ӏ� ������
    // �������R�[�h�͂�����ł�
    private IEnumerator ShowObjectAfterDelay()
    {
        yield return new WaitForSeconds(shiftDelay);
        shiftObject.SetActive(true);
        showObjectCoroutine = null;
    }
    // ������������������������������

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