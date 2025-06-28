using UnityEngine;
using UnityEngine.SceneManagement; // シーンの管理に必要

public class GoalDetector : MonoBehaviour
{
    // ゴールしたときにロードする次のシーン名
    [SerializeField]
    //private string nextSceneName = "GameClearScene"; // インスペクターから設定できるように

    // プレイヤーが触れた時に呼ばれるメソッド
    private void OnTriggerEnter(Collider other)
    {
        // 触れたオブジェクトがプレイヤーかどうかを判定する
        // プレイヤーのオブジェクトに "Player" タグを設定しておく
        if (other.CompareTag("Player"))
        {
            Debug.Log("ゴール！"); // コンソールにメッセージを表示

            // ここにゴール後の処理を記述します

            // 例1：ゲームクリアシーンに移動する
            //SceneManager.LoadScene(nextSceneName);

            // 例2：特定のUIを表示する
            // UIManager.Instance.ShowGameClearUI(); 

            // 例3：プレイヤーの動きを止める
            // other.GetComponent<PlayerMovement>().enabled = false; 
        }
    }
}