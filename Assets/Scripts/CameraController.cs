using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("カメラ設定")]
    [Tooltip("追いかけるターゲット（プレイヤー）")]
    public Transform target;

    [Tooltip("ターゲットからの相対的な位置（距離と高さ）")]
    public Vector3 offset = new Vector3(0f, 5f, -7f);

    [Header("障害物回避設定")]
    [Tooltip("障害物と認識するレイヤー")]
    public LayerMask obstacleLayers;

    // すべてのUpdate処理が終わった後に呼ばれる
    void LateUpdate()
    {
        // ターゲットが設定されていなければ何もしない
        if (target == null) return;

        // 1. カメラの理想的な目標位置を計算する
        // ターゲットの位置に、設定したオフセット（相対位置）を加える
        Vector3 desiredPosition = target.position + offset;

        // 2. 障害物回避処理
        // ターゲットの中心から、理想的なカメラ位置へ向かって線を引く（レイキャスト）
        RaycastHit hit;
        // もし線が障害物レイヤーに衝突した場合
        if (Physics.Linecast(target.position, desiredPosition, out hit, obstacleLayers))
        {
            // カメラの位置を、障害物に衝突した地点に設定する
            // これにより、カメラが壁の裏側にめり込むのを防ぐ
            transform.position = hit.point;
        }
        else
        {
            // 障害物がない場合は、理想的な位置にカメラを移動する
            transform.position = desiredPosition;
        }

        // 3. 常にターゲットの方向を見るようにカメラの向きを調整
        transform.LookAt(target);
    }
}