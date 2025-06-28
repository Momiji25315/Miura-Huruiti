using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("カメラ設定")]
    [Tooltip("カメラが追いかけるターゲット（プレイヤー）")]
    public Transform target;

    [Tooltip("ターゲットからの相対的な位置（距離と高さ）")]
    public Vector3 offset = new Vector3(0f, 5f, -7f);

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("カメラのターゲットが設定されていません！", this);
            return;
        }

        // カメラの目標位置を、ターゲットの位置 + 固定オフセット に設定
        Vector3 desiredPosition = target.position + offset;
        transform.position = desiredPosition;

        // カメラを常にターゲットの方向に向ける
        transform.LookAt(target);
    }
}