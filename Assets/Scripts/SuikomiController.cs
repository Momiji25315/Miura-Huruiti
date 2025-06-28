using UnityEngine;

public class SuikomiController : MonoBehaviour
{
    // �e�I�u�W�F�N�g��PlayerMovement�X�N���v�g��ێ�����ϐ�
    private PlayerMovement playerMovement;

    void Awake()
    {
        // ���g�̐e�I�u�W�F�N�g����PlayerMovement�R���|�[�l���g��T���Ď擾����
        playerMovement = GetComponentInParent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("�e�I�u�W�F�N�g��'PlayerMovement'�X�N���v�g��������܂���I");
        }
    }

    // ���̃I�u�W�F�N�g�̃g���K�[�ɐN���������ɌĂяo�����
    private void OnTriggerEnter(Collider other)
    {
        // �v���C���[���L���ŁA�N����������̃^�O�� "Noddy" �̏ꍇ
        if (playerMovement != null && other.CompareTag("Noddy"))
        {
            // PlayerMovement�X�N���v�g�̔\�͊l�����\�b�h���Ăяo��
            playerMovement.GainNoddyAbility();

            // �z������ "Noddy" �I�u�W�F�N�g��j�󂷂�
            Destroy(other.gameObject);
        }
    }
}