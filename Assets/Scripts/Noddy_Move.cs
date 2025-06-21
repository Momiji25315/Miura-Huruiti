using UnityEngine;

// �G�L�����N�^�[�ɂ�Rigidbody�R���|�[�l���g���K�{�ł��邱�Ƃ�����
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("�̗͐ݒ�")]
    [Tooltip("�G�̍ő�HP")]
    public float maxHealth = 5f;

    // [SerializeField]��t����ƁAprivate�ȕϐ��ł��C���X�y�N�^�[�ɕ\�������i�f�o�b�O�ɕ֗��j
    [Tooltip("���݂�HP�i���s���Ɋm�F�p�j")]
    [SerializeField]
    private float currentHealth;

    [Header("AI�ݒ�")]
    [Tooltip("�v���C���[�����m����͈�")]
    public float detectionRange = 10f;

    [Tooltip("�G�̈ړ����x")]
    public float moveSpeed = 5f;

    [Tooltip("�G�̉�]�̊��炩��")]
    public float rotationSpeed = 5f;

    [Tooltip("�v���C���[�ɗ^����ڐG�_���[�W��")]
    public float attackDamage = 5f;

    // �����Ŏg�p����ϐ�
    private Transform playerTransform;
    private Rigidbody rb;

    // �Q�[���I�u�W�F�N�g���������ꂽ����Ɉ�x�����Ă΂��
    void Awake()
    {
        // �������Z�R���|�[�l���g���擾���ĕϐ��ɕێ�
        rb = GetComponent<Rigidbody>();
    }

    // Awake�̌�A�ŏ��̃t���[���X�V�̑O�Ɉ�x�����Ă΂��
    void Start()
    {
        // �Q�[���J�n����HP���ő�l�ɐݒ�
        currentHealth = maxHealth;

        // "Player"�^�O���g���ăV�[�����̃v���C���[�I�u�W�F�N�g������
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            // �v���C���[������������A����Transform����ۑ����Ă���
            playerTransform = playerObject.transform;
        }
        else
        {
            // �v���C���[��������Ȃ������ꍇ�ɃR���\�[���ɃG���[��\��
            Debug.LogError("�v���C���[��������܂���I Player�I�u�W�F�N�g��'Player'�^�O���t���Ă��邩�m�F���Ă��������B");
            // ����AI�X�N���v�g�𖳌��ɂ��āA�ȍ~�̏������~����
            this.enabled = false;
        }
    }

    // �Œ�t���[�����[�g�ŌĂяo�����i�������Z�̏����͂�����ɏ����̂������j
    void FixedUpdate()
    {
        // �v���C���[���������Ă��Ȃ��ꍇ�͉������Ȃ�
        if (playerTransform == null) return;

        // �G�ƃv���C���[�̋������v�Z
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // ���������m�͈͓��ɓ����Ă���ꍇ
        if (distanceToPlayer <= detectionRange)
        {
            // --- �v���C���[��ǐՂ��鏈�� ---
            HandleChase();
        }
        else
        {
            // --- �ǐՂ���߂鏈�� ---
            StopChasing();
        }
    }

    /// <summary>
    /// �v���C���[��ǂ������鏈��
    /// </summary>
    private void HandleChase()
    {
        // 1. �v���C���[�̕���������
        // �v���C���[�ւ̕����x�N�g�����v�Z (�ڕW�n�_ - ���ݒn)
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        // �G���㉺�ɌX���Ȃ��悤�ɁAY���̉�]�𖳎�����
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            // ���̕������������߂̉�]�����v�Z
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            // ���݂̊p�x����ڕW�̊p�x�֊��炩�ɉ�]������
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 2. �v���C���[�̕����֑O�i����
        // ���g�̑O�����ցA���x�������Ĉړ��x�N�g�����v�Z
        Vector3 targetVelocity = transform.forward * moveSpeed;
        // Y���̑��x�͌��݂̏d�͂Ȃǂɂ�鑬�x���ێ�����i�W�����v�◎���ɉe����^���Ȃ����߁j
        targetVelocity.y = rb.linearVelocity.y;
        // �v�Z�������x��Rigidbody�ɓK�p
        rb.linearVelocity = targetVelocity;
    }

    /// <summary>
    /// �ǐՂ��~���鏈��
    /// </summary>
    private void StopChasing()
    {
        // �v���C���[���͈͊O�ɂ���ꍇ�́A���������̓������~�߂�
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    /// <summary>
    /// ���̃I�u�W�F�N�g�ƕ����I�ɏՓ˂������ɌĂяo�����
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // �Փ˂�������̃^�O�� "Player" ���ǂ������`�F�b�N
        if (collision.gameObject.CompareTag("Player"))
        {
            // ����̃I�u�W�F�N�g���� PlayerMovement �X�N���v�g���擾���悤�Ǝ��݂�
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();

            // �X�N���v�g���擾�ł����ꍇ�i���肪�ԈႢ�Ȃ��v���C���[�̏ꍇ�j
            if (player != null)
            {
                // �v���C���[��TakeDamage���\�b�h���Ăяo���ă_���[�W��^����
                player.TakeDamage(attackDamage);
            }
        }
    }

    /// <summary>
    /// ���̃I�u�W�F�N�g�̃g���K�[�iIs Trigger���I����Collider�j�ɐN���������ɌĂяo�����
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // �N�������I�u�W�F�N�g�̃^�O�� "Suikomi" ���ǂ������`�F�b�N
        if (other.CompareTag("Suikomi"))
        {
            Debug.Log(gameObject.name + " �� 'Suikomi' �ɋz�����܂ꂽ�I");

            // ���̓G�I�u�W�F�N�g���g���V�[������폜����
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �_���[�W���󂯂鏈���B���̃X�N���v�g�i��: �v���C���[�̒e�ہj����Ăяo�����߂�public�ɂ���B
    /// </summary>
    /// <param name="damageAmount">�󂯂�_���[�W��</param>
    public void TakeDamage(float damageAmount)
    {
        // HP�����炷
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " �� " + damageAmount + " �̃_���[�W���󂯂��I �c��HP: " + currentHealth);

        // HP��0�ȉ��ɂȂ����玀�S�������Ăяo��
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// ���S���̏���
    /// </summary>
    private void Die()
    {
        Debug.Log(gameObject.name + " �͓|���ꂽ�B");
        // ���̃Q�[���I�u�W�F�N�g���V�[������폜����
        Destroy(gameObject);
    }
}