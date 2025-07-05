using UnityEngine;
// using UnityEngine.SceneManagement; // リトライ機能が不要なため、この行はコメントアウトまたは削除します

/// <summary>
/// ゲーム全体の進行やUI表示を管理するシングルトンクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    // 他のスクリプトから簡単にアクセスできるようにするための静的インスタンス（シングルトンパターン）
    public static GameManager Instance { get; private set; }

    [Header("UI画面設定")]
    [Tooltip("クリア時に表示するUIオブジェクト（Panelなど）")]
    public GameObject clearScreenUI;

    [Tooltip("ゲームオーバー時に表示するUIオブジェクト（Panelなど）")]
    public GameObject gameOverScreenUI;

    // ゲームが開始される前に一度だけ呼び出される
    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            // このインスタンスを唯一のものとして設定
            Instance = this;
            // シーンをまたいでも破棄されないようにする場合は以下のコメントを外す
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            // 既にインスタンスが存在する場合は、この重複したオブジェクトを破棄する
            Destroy(gameObject);
        }
    }

    // 最初のフレーム更新の前に一度だけ呼び出される
    private void Start()
    {
        // ゲーム開始時に、念のため両方のUIを非表示に設定
        if (clearScreenUI != null)
        {
            clearScreenUI.SetActive(false);
        }
        if (gameOverScreenUI != null)
        {
            gameOverScreenUI.SetActive(false);
        }

        // ゲームの時間を通常速度にする（リスタート時などを考慮）
        Time.timeScale = 1f;
    }

    /// <summary>
    /// クリア画面を表示し、ゲームをポーズさせる
    /// </summary>
    public void ShowClearScreen()
    {
        // UIオブジェクトが設定されている場合のみ実行
        if (clearScreenUI != null)
        {
            clearScreenUI.SetActive(true);
        }
        // ゲームの時間を止める
        Time.timeScale = 0f;
        Debug.Log("ゲームクリア！");
    }

    /// <summary>
    /// ゲームオーバー画面を表示し、ゲームをポーズさせる
    /// </summary>
    public void ShowGameOverScreen()
    {
        // UIオブジェクトが設定されている場合のみ実行
        if (gameOverScreenUI != null)
        {
            gameOverScreenUI.SetActive(true);
        }
        // ゲームの時間を止める
        Time.timeScale = 0f;
        Debug.Log("ゲームオーバー...");
    }
}