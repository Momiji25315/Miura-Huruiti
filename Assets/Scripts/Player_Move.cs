using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("�̗͐ݒ�")]
    public float maxHealth = 100f;
    [SerializeField]
    private float currentHealth;

    [Header("�ړ��ݒ�")]
    public float moveSpeed = 5.0f;
    public float sprintSpeed = 10.0f;
    public float rotationSpeed = 10.0f;

    [Header("�W�����v�ݒ�")]
    public float jumpForce = 7.0f;
    public string groundTag = "Ground";

    [Header("����A�N�V����")]
    public GameObject shiftObject;
    public float shiftDelay = 0.3f;

    [Header("Noddy�\�͐ݒ�")]
    public float abilityHealAmount = 8f;
    public float abilityDisableDuration = 2.5f;
    public Color abilityColor = Color.magenta;

    private bool isGrounded = false;
    private Rigidbody rb;
    private Vector3 moveInput;
    private Coroutine showObjectCoroutine;

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
        if (shiftObject != null) shiftObject.SetActive(false);
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (controlsDisabled) return;

        if (hasNoddyAbility && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ActivateNoddyAbility());
            return;
        }

        if (hasNoddyAbility && Input.GetMouseButtonDown(1)) // 1�͉E�N���b�N
        {
            DiscardNoddyAbility();
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveDirection = cameraForward * moveZ + Camera.main.transform.right * moveX;
        moveInput = moveDirection.normalized;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) Jump();
        RotateCharacter();
        HandleShiftObjectWithDelay();
        if (Input.GetKeyDown(KeyCode.K)) TakeDamage(10);
    }

    void FixedUpdate()
    {
        if (controlsDisabled) return;
        float currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        Vector3 newVelocity = moveInput * currentMoveSpeed;
        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;
    }

    public void GainNoddyAbility()
    {
        if (hasNoddyAbility) return;
        hasNoddyAbility = true;
        if (playerRenderer != null)
        {
            playerRenderer.material.color = abilityColor;
        }
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
        if (playerRenderer != null)
        {
            playerRenderer.material.color = originalColor;
        }
        controlsDisabled = false;
        Debug.Log("HP���񕜂��A����\�ɂȂ�܂����B�\�͎͂����܂����B");
    }

    private void DiscardNoddyAbility()
    {
        hasNoddyAbility = false;
        if (playerRenderer != null)
        {
            playerRenderer.material.color = originalColor;
        }
        Debug.Log("Noddy�̔\�͂��̂āA���̏�Ԃɖ߂�܂����B");
    }

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(groundTag)) isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(groundTag)) isGrounded = false;
    }
}