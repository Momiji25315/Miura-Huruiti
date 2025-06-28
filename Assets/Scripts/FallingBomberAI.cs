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

    private Transform playerTransform;
    private Rigidbody rb;
    private Vector3 initialPosition;
    private bool isSequenceStarted = false;

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
        Debug.Log("落下シーケンス開始！");
        yield return StartCoroutine(MoveToPosition(initialPosition + transform.forward * feintDistance, feintDuration));
        yield return StartCoroutine(MoveToPosition(initialPosition, feintDuration));
        yield return StartCoroutine(MoveToPosition(initialPosition + transform.forward * feintDistance, feintDuration));

        Debug.Log("落下開始！");
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

        // 衝突した相手のタグをチェック
        bool hitPlayer = collision.gameObject.CompareTag("Player");
        bool hitGround = collision.gameObject.CompareTag("Ground");

        // プレイヤーに衝突した場合
        if (hitPlayer)
        {
            // 先に接触ダメージを与える
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                Debug.Log("プレイヤーに直接接触！ 接触ダメージを与えます。");
                player.TakeDamage(explosionDamage); // 爆発ダメージと同じ量のダメージを与える
            }
        }

        // プレイヤーまたは地面に衝突した場合、爆発処理を呼び出す
        if (hitPlayer || hitGround)
        {
            Explode();
        }
    }
    // --- ★★★ 修正ここまで ★★★ ---

    /// <summary>
    /// 爆発処理
    /// </summary>
    private void Explode()
    {
        Debug.Log("爆発！");
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
                PlayerMovement player = col.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    // ここで爆風ダメージを与える
                    player.TakeDamage(explosionDamage);
                }
            }
        }

        Destroy(gameObject);
    }
}