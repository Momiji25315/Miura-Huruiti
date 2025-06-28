using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("�J�����ݒ�")]
    [Tooltip("�J�������ǂ�������^�[�Q�b�g�i�v���C���[�j")]
    public Transform target;

    [Tooltip("�^�[�Q�b�g����̑��ΓI�Ȉʒu�i�����ƍ����j")]
    public Vector3 offset = new Vector3(0f, 5f, -7f);

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("�J�����̃^�[�Q�b�g���ݒ肳��Ă��܂���I", this);
            return;
        }

        // �J�����̖ڕW�ʒu���A�^�[�Q�b�g�̈ʒu + �Œ�I�t�Z�b�g �ɐݒ�
        Vector3 desiredPosition = target.position + offset;
        transform.position = desiredPosition;

        // �J��������Ƀ^�[�Q�b�g�̕����Ɍ�����
        transform.LookAt(target);
    }
}