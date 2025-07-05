using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("�J�����ݒ�")]
    [Tooltip("�ǂ�������^�[�Q�b�g�i�v���C���[�j")]
    public Transform target;

    [Tooltip("�^�[�Q�b�g����̑��ΓI�Ȉʒu�i�����ƍ����j")]
    public Vector3 offset = new Vector3(0f, 5f, -7f);

    [Header("��Q�����ݒ�")]
    [Tooltip("��Q���ƔF�����郌�C���[")]
    public LayerMask obstacleLayers;

    // ���ׂĂ�Update�������I�������ɌĂ΂��
    void LateUpdate()
    {
        // �^�[�Q�b�g���ݒ肳��Ă��Ȃ���Ή������Ȃ�
        if (target == null) return;

        // 1. �J�����̗��z�I�ȖڕW�ʒu���v�Z����
        // �^�[�Q�b�g�̈ʒu�ɁA�ݒ肵���I�t�Z�b�g�i���Έʒu�j��������
        Vector3 desiredPosition = target.position + offset;

        // 2. ��Q���������
        // �^�[�Q�b�g�̒��S����A���z�I�ȃJ�����ʒu�֌������Đ��������i���C�L���X�g�j
        RaycastHit hit;
        // ����������Q�����C���[�ɏՓ˂����ꍇ
        if (Physics.Linecast(target.position, desiredPosition, out hit, obstacleLayers))
        {
            // �J�����̈ʒu���A��Q���ɏՓ˂����n�_�ɐݒ肷��
            // ����ɂ��A�J�������ǂ̗����ɂ߂荞�ނ̂�h��
            transform.position = hit.point;
        }
        else
        {
            // ��Q�����Ȃ��ꍇ�́A���z�I�Ȉʒu�ɃJ�������ړ�����
            transform.position = desiredPosition;
        }

        // 3. ��Ƀ^�[�Q�b�g�̕���������悤�ɃJ�����̌����𒲐�
        transform.LookAt(target);
    }
}