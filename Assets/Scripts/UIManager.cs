using UnityEngine;
using UnityEngine.UI; // UI�R���|�[�l���g���������߂ɕK�v
using TMPro; // TextMeshPro���������߂ɕK�v�i�C�Ӂj

public class UIManager : MonoBehaviour
{
    [Header("HP Bar")]
    [Tooltip("HP�\���p�̃X���C�_�[")]
    public Slider hpSlider;
    [Tooltip("HP���e�L�X�g�ŕ\������ꍇ�ɐݒ�i�C�Ӂj")]
    public TextMeshProUGUI hpText;

    /// <summary>
    /// HP�o�[�̍ő�l��ݒ肷��
    /// </summary>
    /// <param name="maxHealth">�v���C���[�̍ő�HP</param>
    public void SetMaxHealth(float maxHealth)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = maxHealth; // �ŏ��͖��^���ɂ���
        }
    }

    /// <summary>
    /// HP�o�[�̌��݂̒l���X�V����
    /// </summary>
    /// <param name="currentHealth">�v���C���[�̌��݂�HP</param>
    public void UpdateHealth(float currentHealth)
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        // HP�e�L�X�g���X�V����i�C�Ӂj
        if (hpText != null)
        {
            hpText.text = "HP: " + (int)currentHealth;
        }
    }
}