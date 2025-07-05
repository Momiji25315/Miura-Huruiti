using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを扱うために必要
using TMPro; // TextMeshProを扱うために必要（任意）

public class UIManager : MonoBehaviour
{
    [Header("HP Bar")]
    [Tooltip("HP表示用のスライダー")]
    public Slider hpSlider;
    [Tooltip("HPをテキストで表示する場合に設定（任意）")]
    public TextMeshProUGUI hpText;

    /// <summary>
    /// HPバーの最大値を設定する
    /// </summary>
    /// <param name="maxHealth">プレイヤーの最大HP</param>
    public void SetMaxHealth(float maxHealth)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = maxHealth; // 最初は満タンにする
        }
    }

    /// <summary>
    /// HPバーの現在の値を更新する
    /// </summary>
    /// <param name="currentHealth">プレイヤーの現在のHP</param>
    public void UpdateHealth(float currentHealth)
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        // HPテキストも更新する（任意）
        if (hpText != null)
        {
            hpText.text = "HP: " + (int)currentHealth;
        }
    }
}