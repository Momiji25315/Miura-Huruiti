using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("�J�����ݒ�")]
    [Tooltip("�J�������ǂ�������^�[�Q�b�g�i�v���C���[�Ȃǁj")]
    public Transform target;

    [Tooltip("�^�[�Q�b�g����̑��ΓI�Ȉʒu�i�����ƍ����j")]
    public Vector3 offset = new Vector3(0f, 5f, -7f);

    // LateUpdate�́A���ׂĂ�Update�֐��̌Ăяo�����I�������ɌĂяo�����
    // �v���C���[�̈ړ�������������ɃJ�����𓮂������ƂŁA�J�N����K�^����h�����Ƃ��ł��܂��B
    void LateUpdate()
    {
        // �^�[�Q�b�g�i�v���C���[�j���ݒ肳��Ă��Ȃ���΁A�������Ȃ��i�G���[�h�~�j
        if (target == null)
        {
            Debug.LogWarning("�J�����̃^�[�Q�b�g���ݒ肳��Ă��܂���I");
            return;
        }

        // 1. �J�����̖ڕW�ʒu���v�Z����
        // �^�[�Q�b�g�i�v���C���[�j�̈ʒu�ɁA�ݒ肵���I�t�Z�b�g�i�����j��������
        Vector3 desiredPosition = target.position + offset;

        // 2. �J�����̈ʒu���A�v�Z�����ڕW�ʒu�ɃX���[�Y�Ɉړ�������i�C�Ӂj
        // transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // ����͑����ɒǏ]�����邽�߁A���ڑ�����܂��B
        transform.position = desiredPosition;

        // 3. �J��������Ƀ^�[�Q�b�g�̕����Ɍ�����
        // ����ɂ��A�v���C���[����ɉ�ʂ̒��S�ɉf��܂��B
        transform.LookAt(target);
    }
}