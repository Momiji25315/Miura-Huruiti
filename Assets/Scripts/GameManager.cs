using UnityEngine;
// using UnityEngine.SceneManagement; // ���g���C�@�\���s�v�Ȃ��߁A���̍s�̓R�����g�A�E�g�܂��͍폜���܂�

/// <summary>
/// �Q�[���S�̂̐i�s��UI�\�����Ǘ�����V���O���g���N���X
/// </summary>
public class GameManager : MonoBehaviour
{
    // ���̃X�N���v�g����ȒP�ɃA�N�Z�X�ł���悤�ɂ��邽�߂̐ÓI�C���X�^���X�i�V���O���g���p�^�[���j
    public static GameManager Instance { get; private set; }

    [Header("UI��ʐݒ�")]
    [Tooltip("�N���A���ɕ\������UI�I�u�W�F�N�g�iPanel�Ȃǁj")]
    public GameObject clearScreenUI;

    [Tooltip("�Q�[���I�[�o�[���ɕ\������UI�I�u�W�F�N�g�iPanel�Ȃǁj")]
    public GameObject gameOverScreenUI;

    // �Q�[�����J�n�����O�Ɉ�x�����Ăяo�����
    private void Awake()
    {
        // �V���O���g���̐ݒ�
        if (Instance == null)
        {
            // ���̃C���X�^���X��B��̂��̂Ƃ��Đݒ�
            Instance = this;
            // �V�[�����܂����ł��j������Ȃ��悤�ɂ���ꍇ�͈ȉ��̃R�����g���O��
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            // ���ɃC���X�^���X�����݂���ꍇ�́A���̏d�������I�u�W�F�N�g��j������
            Destroy(gameObject);
        }
    }

    // �ŏ��̃t���[���X�V�̑O�Ɉ�x�����Ăяo�����
    private void Start()
    {
        // �Q�[���J�n���ɁA�O�̂��ߗ�����UI���\���ɐݒ�
        if (clearScreenUI != null)
        {
            clearScreenUI.SetActive(false);
        }
        if (gameOverScreenUI != null)
        {
            gameOverScreenUI.SetActive(false);
        }

        // �Q�[���̎��Ԃ�ʏ푬�x�ɂ���i���X�^�[�g���Ȃǂ��l���j
        Time.timeScale = 1f;
    }

    /// <summary>
    /// �N���A��ʂ�\�����A�Q�[�����|�[�Y������
    /// </summary>
    public void ShowClearScreen()
    {
        // UI�I�u�W�F�N�g���ݒ肳��Ă���ꍇ�̂ݎ��s
        if (clearScreenUI != null)
        {
            clearScreenUI.SetActive(true);
        }
        // �Q�[���̎��Ԃ��~�߂�
        Time.timeScale = 0f;
        Debug.Log("�Q�[���N���A�I");
    }

    /// <summary>
    /// �Q�[���I�[�o�[��ʂ�\�����A�Q�[�����|�[�Y������
    /// </summary>
    public void ShowGameOverScreen()
    {
        // UI�I�u�W�F�N�g���ݒ肳��Ă���ꍇ�̂ݎ��s
        if (gameOverScreenUI != null)
        {
            gameOverScreenUI.SetActive(true);
        }
        // �Q�[���̎��Ԃ��~�߂�
        Time.timeScale = 0f;
        Debug.Log("�Q�[���I�[�o�[...");
    }
}