using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingBomberAI : MonoBehaviour
{
    [Header("AI設定")]
    public float detectionRange = 10f;

    [Header("落下シーケンス設定")]
    public float feintDuration = 0.625f;
    public float feintDistance = 1.0f;
    public float fallSpeed = 9.81f;

    [Header("爆発設定")]
    public float explosionDamage = 25f;
    public float explosionRadius = 1.0f;
    public GameObject explosionEffectPrefab;
    public float explosionDuration = 0.5f;

    // --- 内部変数 ---
    private Transform playerTransform;
    private Rigidbody rb;
    private Vector3 initialPosition;
    private bool isSequenceStarted = false;
    // --- ここから追加 ---
    private int groundHitCount = 0; // 地面に接触した回数をカウントする変数
    // --- 追加ここまで ---

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        initialPosition = transform.position;
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null || isSequenceStarted) return;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= detectionRange)
        {
            isSequenceStarted = true;
            StartCoroutine(FallSequenceCoroutine());
        }
    }

    private IEnumerator FallSequenceCoroutine()
    {
        Debug.Log(gameObject.name + " 落下シーケンス開始！");
        yield return StartCoroutine(MoveToPosition(initialPosition + transform.forward * feintDistance, feintDuration));
        yield return StartCoroutine(MoveToPosition(initialPosition, feintDuration));
        yield return StartCoroutine(MoveToPosition(initialPosition + transform.forward * feintDistance, feintDuration));

        Debug.Log(gameObject.name + " 落下開始！");
        rb.useGravity = true;
        rb.linearVelocity = Vector3.down * fallSpeed;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;
        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }

    // --- ★★★ ここから全面的に修正しました ★★★ ---
    /// <summary>
    /// 他のオブジェクトと物理的に衝突した時に呼び出される
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // 落下シーケンスが開始した後でなければ、何もしない
        if (!isSequenceStarted) return;

        // 【条件1】衝突した相手がプレイヤーの場合
        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーのスクリプトを取得
            if (collision.gameObject.TryGetComponent<PlayerMovement>(out var player))
            {
                // 先に接触ダメージを与える
                Debug.Log(gameObject.name + " がプレイヤーに直接接触！ 接触ダメージを与えます。");
                player.TakeDamage(explosionDamage);
            }
            // その後、即座に爆発する
            Explode();
        }
        // 【条件2】衝突した相手が地面の場合
        else if (collision.gameObject.CompareTag("Ground"))
        {
            // 地面との接触回数をカウントアップ
            groundHitCount++;
            Debug.Log(gameObject.name + " が地面に接触！ (" + groundHitCount + "回目)");

            // 接触回数が2回以上になったら爆発する
            if (groundHitCount >= 2)
            {
                Explode();
            }
        }
    }
    // --- ★★★ 修正ここまで ★★★ ---

    /// <summary>
    /// 爆発処理
    /// </summary>
    private void Explode()
    {
        Debug.Log(gameObject.name + " が爆発！");
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            float diameter = explosionRadius * 2f;
            effect.transform.localScale = new Vector3(diameter, diameter, diameter);
            Destroy(effect, explosionDuration);
        }

        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var col in collidersInRange)
        {
            if (col.CompareTag("Player"))
            {
                if (col.TryGetComponent<PlayerMovement>(out var player))
                {
                    // ここで爆風ダメージを与える
                    player.TakeDamage(explosionDamage);
                }
            }
        }

        Destroy(gameObject);
    }

    public void TakeDamage(float damageAmount)
    {
        Debug.Log(gameObject.name + " がダメージを受けて誘爆！");
        if (!isSequenceStarted)
        {
            isSequenceStarted = true;
        }
        Explode();
    }
}