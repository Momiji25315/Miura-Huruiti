using UnityEngine;

// 敵キャラクターにはRigidbodyコンポーネントが必須であることを示す
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("体力設定")]
    [Tooltip("敵の最大HP")]
    public float maxHealth = 5f;

    // [SerializeField]を付けると、privateな変数でもインスペクターに表示される（デバッグに便利）
    [Tooltip("現在のHP（実行中に確認用）")]
    [SerializeField]
    private float currentHealth;

    [Header("AI設定")]
    [Tooltip("プレイヤーを検知する範囲")]
    public float detectionRange = 10f;

    [Tooltip("敵の移動速度")]
    public float moveSpeed = 5f;

    [Tooltip("敵の回転の滑らかさ")]
    public float rotationSpeed = 5f;

    [Tooltip("プレイヤーに与える接触ダメージ量")]
    public float attackDamage = 5f;

    // 内部で使用する変数
    private Transform playerTransform;
    private Rigidbody rb;

    // ゲームオブジェクトが生成された直後に一度だけ呼ばれる
    void Awake()
    {
        // 物理演算コンポーネントを取得して変数に保持
        rb = GetComponent<Rigidbody>();
    }

    // Awakeの後、最初のフレーム更新の前に一度だけ呼ばれる
    void Start()
    {
        // ゲーム開始時にHPを最大値に設定
        currentHealth = maxHealth;

        // "Player"タグを使ってシーン内のプレイヤーオブジェクトを検索
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            // プレイヤーが見つかったら、そのTransform情報を保存しておく
            playerTransform = playerObject.transform;
        }
        else
        {
            // プレイヤーが見つからなかった場合にコンソールにエラーを表示
            Debug.LogError("プレイヤーが見つかりません！ Playerオブジェクトに'Player'タグが付いているか確認してください。");
            // このAIスクリプトを無効にして、以降の処理を停止する
            this.enabled = false;
        }
    }

    // 固定フレームレートで呼び出される（物理演算の処理はこちらに書くのが推奨）
    void FixedUpdate()
    {
        // プレイヤーが見つかっていない場合は何もしない
        if (playerTransform == null) return;

        // 敵とプレイヤーの距離を計算
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 距離が検知範囲内に入っている場合
        if (distanceToPlayer <= detectionRange)
        {
            // --- プレイヤーを追跡する処理 ---
            HandleChase();
        }
        else
        {
            // --- 追跡をやめる処理 ---
            StopChasing();
        }
    }

    /// <summary>
    /// プレイヤーを追いかける処理
    /// </summary>
    private void HandleChase()
    {
        // 1. プレイヤーの方向を向く
        // プレイヤーへの方向ベクトルを計算 (目標地点 - 現在地)
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        // 敵が上下に傾かないように、Y軸の回転を無視する
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            // その方向を向くための回転情報を計算
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            // 現在の角度から目標の角度へ滑らかに回転させる
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 2. プレイヤーの方向へ前進する
        // 自身の前方向へ、速度をかけて移動ベクトルを計算
        Vector3 targetVelocity = transform.forward * moveSpeed;
        // Y軸の速度は現在の重力などによる速度を維持する（ジャンプや落下に影響を与えないため）
        targetVelocity.y = rb.linearVelocity.y;
        // 計算した速度をRigidbodyに適用
        rb.linearVelocity = targetVelocity;
    }

    /// <summary>
    /// 追跡を停止する処理
    /// </summary>
    private void StopChasing()
    {
        // プレイヤーが範囲外にいる場合は、水平方向の動きを止める
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    /// <summary>
    /// 他のオブジェクトと物理的に衝突した時に呼び出される
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // 衝突した相手のタグが "Player" かどうかをチェック
        if (collision.gameObject.CompareTag("Player"))
        {
            // 相手のオブジェクトから PlayerMovement スクリプトを取得しようと試みる
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();

            // スクリプトが取得できた場合（相手が間違いなくプレイヤーの場合）
            if (player != null)
            {
                // プレイヤーのTakeDamageメソッドを呼び出してダメージを与える
                player.TakeDamage(attackDamage);
            }
        }
    }

    /// <summary>
    /// 他のオブジェクトのトリガー（Is TriggerがオンのCollider）に侵入した時に呼び出される
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 侵入したオブジェクトのタグが "Suikomi" かどうかをチェック
        if (other.CompareTag("Suikomi"))
        {
            Debug.Log(gameObject.name + " が 'Suikomi' に吸い込まれた！");

            // この敵オブジェクト自身をシーンから削除する
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ダメージを受ける処理。他のスクリプト（例: プレイヤーの弾丸）から呼び出すためにpublicにする。
    /// </summary>
    /// <param name="damageAmount">受けるダメージ量</param>
    public void TakeDamage(float damageAmount)
    {
        // HPを減らす
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " が " + damageAmount + " のダメージを受けた！ 残りHP: " + currentHealth);

        // HPが0以下になったら死亡処理を呼び出す
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡時の処理
    /// </summary>
    private void Die()
    {
        Debug.Log(gameObject.name + " は倒された。");
        // このゲームオブジェクトをシーンから削除する
        Destroy(gameObject);
    }
}