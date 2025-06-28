using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("�J�����ݒ�")]
    [Tooltip("�J�������ǂ�������^�[�Q�b�g�i�v���C���[�j")]
    public Transform target;

    [Tooltip("�^�[�Q�b�g����̋���")]
    public float distance = 7.0f;

    [Tooltip("�^�[�Q�b�g����̍���")]
    public float height = 5.0f;

    [Tooltip("�J�������Ǐ]����ۂ̊��炩���B�l���傫���قǑf�����Ǐ]���܂��B")]
    public float smoothSpeed = 5.0f;

    // LateUpdate�́A���ׂĂ�Update�֐��̌Ăяo�����I�������ɌĂяo�����
    // �v���C���[�̈ړ�������������ɃJ�����𓮂������ƂŁA�J�N����K�^����h���܂��B
    void LateUpdate()
    {
        // �^�[�Q�b�g�i�v���C���[�j���ݒ肳��Ă��Ȃ���΁A�������Ȃ��i�G���[�h�~�j
        if (target == null)
        {
            // �Q�[�����s���Ɉ�x�����x�����b�Z�[�W��\��
            Debug.LogWarning("CameraController�̃^�[�Q�b�g���ݒ肳��Ă��܂���I", this);
            return;
        }

        // 1. �J�����̖ڕW�ʒu���v�Z����
        // �^�[�Q�b�g�̈ʒu����A�^�[�Q�b�g�́u���v����( -target.forward )��distance������A
        // ����Ɂu��v����( Vector3.up )��height���オ�����ʒu��ڕW�Ƃ���B
        Vector3 desiredPosition = target.position - (target.forward * distance) + (Vector3.up * height);

        // 2. �J�����̈ʒu���A���݂̈ʒu����ڕW�ʒu�փX���[�Y�Ɉړ�������
        // Vector3.Lerp(���݈ʒu, �ڕW�ʒu, ���x)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. �J��������Ƀ^�[�Q�b�g�̕����Ɍ�����
        transform.LookAt(target);
    }
}