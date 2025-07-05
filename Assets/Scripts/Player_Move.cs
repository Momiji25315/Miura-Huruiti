using System.Collections;
using UnityEngine;

// ���̃X�N���v�g���A�^�b�`���ꂽ�I�u�W�F�N�g�ɂ�Rigidbody���K�{�ł��邱�Ƃ�����
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

    [Header("�񕜔\�͐ݒ�iNoddy�j")]
    [Tooltip("�\�͔������̉񕜗�")]
    public float abilityHealAmount = 8f;
    [Tooltip("����s�\����")]
    public float noddyAbilityDisableDuration = 2.5f;
    [Tooltip("�\�͊l�����̐F")]
    public Color noddyAbilityColor = Color.magenta;

    [Header("�����\�͐ݒ�iBomber�j")]
    [Tooltip("�����_���[�W")]
    public float bomberAbilityDamage = 15f;
    [Tooltip("�����͈�")]
    public float bomberAbilityRadius = 2f;
    [Tooltip("����s�\����")]
    public float bomberAbilityDisableDuration = 2.0f;
    [Tooltip("�\�͊l�����̐F")]
    public Color bomberAbilityColor = Color.black;
    [Tooltip("�����\�͎g�p���̃G�t�F�N�g�v���n�u")]
    public GameObject bomberAbilityEffectPrefab;
    [Tooltip("�����G�t�F�N�g�̕\������")]
    public float bomberAbilityEffectDuration = 1.0f;

    // --- �����ϐ� ---
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
        // �V�[������UIManager��T���Ď擾�i�V����FindAnyObjectByType���g�p�j
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
        // �\�͔����i���N���b�N�j
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

        // �\�͔j���i�E�N���b�N�j
        if ((hasNoddyAbility || hasBomberAbility) && Input.GetMouseButtonDown(1))
        {
            DiscardAnyAbility();
        }

        // �ړ����́i�J������j
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
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

        // GameManager�ɃQ�[���I�[�o�[��ʒm����
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOverScreen();
        }

        // �v���C���[�I�u�W�F�N�g���\���ɂ���
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