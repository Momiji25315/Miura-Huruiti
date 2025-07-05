using UnityEngine;

public class GoalController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // �g���K�[�ɓ����Ă����I�u�W�F�N�g�� "Player" �^�O�������Ă��邩�`�F�b�N
        if (other.CompareTag("Player"))
        {
            // GameManager�̃N���A�������Ăяo��
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowClearScreen();
            }
            else
            {
                Debug.LogError("GameManager��������܂���I");
            }
        }
    }
}