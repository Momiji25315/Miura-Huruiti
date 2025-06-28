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
            Debug.LogError("�e�I�u�W�F�N�g��'PlayerMovement'�X�N���v�g��������܂���I", this);
        }
    }

    // ���̃I�u�W�F�N�g�̃g���K�[�iIs Trigger���I����Collider�j�ɐN���������ɌĂяo�����
    private void OnTriggerEnter(Collider other)
    {
        // �v���C���[�̃X�N���v�g���������Ă��Ȃ��ꍇ�͉������Ȃ�
        if (playerMovement == null) return;

        // "Noddy"�^�O���z�����񂾂�A�񕜔\�͂𓾂�
        if (other.CompareTag("Noddy"))
        {
            playerMovement.GainNoddyAbility();
            Destroy(other.gameObject);
        }
        // "Bomber"�^�O���z�����񂾂�A�����\�͂𓾂�
        else if (other.CompareTag("Bomber"))
        {
            playerMovement.GainBomberAbility();
            Destroy(other.gameObject);
        }
    }
}