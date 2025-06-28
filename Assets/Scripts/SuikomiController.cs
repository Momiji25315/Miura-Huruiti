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
            Debug.LogError("親オブジェクトに'PlayerMovement'スクリプトが見つかりません！", this);
        }
    }

    // 他のオブジェクトのトリガー（Is TriggerがオンのCollider）に侵入した時に呼び出される
    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーのスクリプトが見つかっていない場合は何もしない
        if (playerMovement == null) return;

        // "Noddy"タグを吸い込んだら、回復能力を得る
        if (other.CompareTag("Noddy"))
        {
            playerMovement.GainNoddyAbility();
            Destroy(other.gameObject);
        }
        // "Bomber"タグを吸い込んだら、爆発能力を得る
        else if (other.CompareTag("Bomber"))
        {
            playerMovement.GainBomberAbility();
            Destroy(other.gameObject);
        }
    }
}