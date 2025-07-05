using UnityEngine;

public class GoalController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // トリガーに入ってきたオブジェクトが "Player" タグを持っているかチェック
        if (other.CompareTag("Player"))
        {
            // GameManagerのクリア処理を呼び出す
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowClearScreen();
            }
            else
            {
                Debug.LogError("GameManagerが見つかりません！");
            }
        }
    }
}