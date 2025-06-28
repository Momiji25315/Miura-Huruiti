using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("カメラ設定")]
    [Tooltip("カメラが追いかけるターゲット（プレイヤーなど）")]
    public Transform target;

    [Tooltip("ターゲットからの相対的な位置（距離と高さ）")]
    public Vector3 offset = new Vector3(0f, 5f, -7f);

    // LateUpdateは、すべてのUpdate関数の呼び出しが終わった後に呼び出される
    // プレイヤーの移動が完了した後にカメラを動かすことで、カクつきやガタつきを防ぐことができます。
    void LateUpdate()
    {
        // ターゲット（プレイヤー）が設定されていなければ、何もしない（エラー防止）
        if (target == null)
        {
            Debug.LogWarning("カメラのターゲットが設定されていません！");
            return;
        }

        // 1. カメラの目標位置を計算する
        // ターゲット（プレイヤー）の位置に、設定したオフセット（距離）を加える
        Vector3 desiredPosition = target.position + offset;

        // 2. カメラの位置を、計算した目標位置にスムーズに移動させる（任意）
        // transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // 今回は即座に追従させるため、直接代入します。
        transform.position = desiredPosition;

        // 3. カメラを常にターゲットの方向に向ける
        // これにより、プレイヤーが常に画面の中心に映ります。
        transform.LookAt(target);
    }
}