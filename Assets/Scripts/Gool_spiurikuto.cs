using UnityEngine;
using UnityEngine.SceneManagement; // �V�[���̊Ǘ��ɕK�v

public class GoalDetector : MonoBehaviour
{
    // �S�[�������Ƃ��Ƀ��[�h���鎟�̃V�[����
    [SerializeField]
    //private string nextSceneName = "GameClearScene"; // �C���X�y�N�^�[����ݒ�ł���悤��

    // �v���C���[���G�ꂽ���ɌĂ΂�郁�\�b�h
    private void OnTriggerEnter(Collider other)
    {
        // �G�ꂽ�I�u�W�F�N�g���v���C���[���ǂ����𔻒肷��
        // �v���C���[�̃I�u�W�F�N�g�� "Player" �^�O��ݒ肵�Ă���
        if (other.CompareTag("Player"))
        {
            Debug.Log("�S�[���I"); // �R���\�[���Ƀ��b�Z�[�W��\��

            // �����ɃS�[����̏������L�q���܂�

            // ��1�F�Q�[���N���A�V�[���Ɉړ�����
            //SceneManager.LoadScene(nextSceneName);

            // ��2�F�����UI��\������
            // UIManager.Instance.ShowGameClearUI(); 

            // ��3�F�v���C���[�̓������~�߂�
            // other.GetComponent<PlayerMovement>().enabled = false; 
        }
    }
}