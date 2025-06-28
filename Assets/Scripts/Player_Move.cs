using System.Collections;
using UnityEngine;

// Rigidbody�R���|�[�l���g���K�{�ł��邱�Ƃ�����
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("�̗͐ݒ�")]
    [Tooltip("�ő�HP")]
    public float maxHealth = 100f;
    [Tooltip("���݂�HP�i���s���Ɋm�F�p�j")]
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

    [Header("�ڒn����")]
    [Tooltip("�n�ʂƔF�����郌�C���[")]
    public LayerMask groundLayer;
    [Tooltip("�ڒn����̒��S�_�i�v���C���[�̑����ɔz�u�j")]
    public Transform groundCheck;
    [Tooltip("�ڒn����̋��̂̔��a")]
    public float groundCheckRadius = 0.2f;

    [Header("����A�N�V�����iShift�L�[�j")]
    [Tooltip("Shift�L�[�ŕ\��/��\����؂�ւ���q�I�u�W�F�N�g")]
    public GameObject shiftObject;
    [Tooltip("Shift�L�[�������Ă���I�u�W�F�N�g���\�������܂ł̎���(�b)")]
    public float shiftDelay = 0.3f;

    [Header("Noddy�\�͐ݒ�")]
    [Tooltip("�\�͔������̉񕜗�")]
    public float abilityHealAmount = 8f;
    [Tooltip("�\�͔������̑���s�\����")]
    public float abilityDisableDuration = 2.5f;
    [Tooltip("�\�͊l�����̐F")]
    public Color abilityColor = Color.magenta;

    // --- �����Ŏg�p����ϐ� ---
    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;
    private Coroutine showObjectCoroutine;

    // Noddy�\�͊֘A�̓����ϐ�
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
        // Noddy�\�͂̓���
        if (hasNoddyAbility && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ActivateNoddyAbility());
            return;
        }
        if (hasNoddyAbility && Input.GetMouseButtonDown(1))
        {
            DiscardNoddyAbility();
        }

        // �ړ����́i���[���h���W��j
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(moveX, 0f, moveZ).normalized;

        // �W�����v����
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // Shift�L�[����
        HandleShiftObjectWithDelay();

        // �f�o�b�O�p�_���[�W�L�[
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10);
        }
    }

    private void MoveCharacter()
    {
        // �ړ�
        float currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        Vector3 newVelocity = moveInput * currentMoveSpeed;
        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;

        // ��]
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

    // --- Noddy�\�͊֘A�̃��\�b�h ---
    public void GainNoddyAbility()
    {
        if (hasNoddyAbility) return;
        hasNoddyAbility = true;
        if (playerRenderer != null) playerRenderer.material.color = abilityColor;
        Debug.Log("Noddy�̔\�͂��l�������I ���N���b�N�Ŕ����A�E�N���b�N�Ŕj���ł��܂��B");
    }

    private IEnumerator ActivateNoddyAbility()
    {
        hasNoddyAbility = false;
        controlsDisabled = true;
        rb.linearVelocity = Vector3.zero;
        Debug.Log("�\�͔����I " + abilityDisableDuration + "�b�ԁA����s�\�ɂȂ�܂�...");
        yield return new WaitForSeconds(abilityDisableDuration);
        Heal(abilityHealAmount);
        if (playerRenderer != null) playerRenderer.material.color = originalColor;
        controlsDisabled = false;
        Debug.Log("HP���񕜂��A����\�ɂȂ�܂����B�\�͎͂����܂����B");
    }

    private void DiscardNoddyAbility()
    {
        hasNoddyAbility = false;
        if (playerRenderer != null) playerRenderer.material.color = originalColor;
        Debug.Log("Noddy�̔\�͂��̂āA���̏�Ԃɖ߂�܂����B");
    }

    // --- HP�E�_���[�W�֘A�̃��\�b�h ---
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        Debug.Log(amount + " �񕜂����I ���݂�HP: " + currentHealth);
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        Debug.Log(gameObject.name + " �� " + damageAmount + " �̃_���[�W���󂯂��I �c��HP: " + currentHealth);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " �͗͐s����...");
        gameObject.SetActive(false);
    }

    // --- Shift�L�[�֘A�̃��\�b�h ---
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