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

    [Header("�ڒn����")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("����A�N�V�����iShift�L�[�j")]
    public GameObject shiftObject;
    public float shiftDelay = 0.3f;

    [Header("�񕜔\�͐ݒ�iNoddy�j")]
    public float abilityHealAmount = 8f;
    public float noddyAbilityDisableDuration = 2.5f;
    public Color noddyAbilityColor = Color.magenta;

    [Header("�����\�͐ݒ�iBomber�j")]
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
            Debug.LogError("UIManager��������܂���I");
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

        // �ړ����́i�J�����̌�������ɂ���j
        float moveX = Input.GetAxisRaw("Horizontal"); // A, D�L�[
        float moveZ = Input.GetAxisRaw("Vertical");   // W, S�L�[
        // �J�����̑O���x�N�g���iY���𖳎����Đ����ɂ���j
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        // �J�����̌�������ɁA�ړ��������v�Z
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
        Debug.Log("�񕜔\�͂��l���I");
    }

    public void GainBomberAbility()
    {
        ResetAbilityState();
        hasBomberAbility = true;
        if (playerRenderer != null) playerRenderer.material.color = bomberAbilityColor;
        Debug.Log("�����\�͂��l���I");
    }

    private IEnumerator ActivateNoddyAbility()
    {
        controlsDisabled = true;
        rb.linearVelocity = Vector3.zero;
        Debug.Log("�񕜔\�͔����I " + noddyAbilityDisableDuration + "�b�ԁA����s�\...");
        yield return new WaitForSeconds(noddyAbilityDisableDuration);
        Heal(abilityHealAmount);
        ResetAbilityState();
        controlsDisabled = false;
        Debug.Log("HP���񕜂��A����\�ɂȂ�܂����B");
    }

    private IEnumerator ActivateBomberAbility()
    {
        controlsDisabled = true;
        rb.linearVelocity = Vector3.zero;
        Debug.Log("�����\�͔����I " + bomberAbilityDisableDuration + "�b�ԁA����s�\...");
        ExplodeWithAbility();
        yield return new WaitForSeconds(bomberAbilityDisableDuration);
        ResetAbilityState();
        controlsDisabled = false;
        Debug.Log("����\�ɂȂ�܂����B");
    }

    private void ExplodeWithAbility()
    {
        Debug.Log("BOOM! �͈͓��̓���̓G�Ƀ_���[�W��^���܂��B");

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
        Debug.Log("�\�͂��̂āA���̏�Ԃɖ߂�܂����B");
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

        Debug.Log(amount + " �񕜂����I ���݂�HP: " + currentHealth);
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth);
        }

        Debug.Log(gameObject.name + " �� " + damageAmount + " �̃_���[�W���󂯂��I �c��HP: " + currentHealth);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " �͗͐s����...");
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