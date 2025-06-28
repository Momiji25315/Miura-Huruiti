using UnityEngine;

public class SuikomiController : MonoBehaviour
{
    // 親オブジェクトのPlayerMovementスクリプトを保持する変数
    private PlayerMovement playerMovement;

    void Awake()
    {
        // 自身の親オブジェクトからPlayerMovementコンポーネントを探して取得する
        playerMovement = GetComponentInParent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("親オブジェクトに'PlayerMovement'スクリプトが見つかりません！");
        }
    }

    // 他のオブジェクトのトリガーに侵入した時に呼び出される
    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーが有効で、侵入した相手のタグが "Noddy" の場合
        if (playerMovement != null && other.CompareTag("Noddy"))
        {
            // PlayerMovementスクリプトの能力獲得メソッドを呼び出す
            playerMovement.GainNoddyAbility();

            // 吸い込んだ "Noddy" オブジェクトを破壊する
            Destroy(other.gameObject);
        }
    }
}