using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("カメラ設定")]
    [Tooltip("カメラが追いかけるターゲット（プレイヤー）")]
    public Transform target;

    [Tooltip("ターゲットからの距離")]
    public float distance = 7.0f;

    [Tooltip("ターゲットからの高さ")]
    public float height = 5.0f;

    [Tooltip("カメラが追従する際の滑らかさ。値が大きいほど素早く追従します。")]
    public float smoothSpeed = 5.0f;

    // LateUpdateは、すべてのUpdate関数の呼び出しが終わった後に呼び出される
    // プレイヤーの移動が完了した後にカメラを動かすことで、カクつきやガタつきを防ぎます。
    void LateUpdate()
    {
        // ターゲット（プレイヤー）が設定されていなければ、何もしない（エラー防止）
        if (target == null)
        {
            // ゲーム実行中に一度だけ警告メッセージを表示
            Debug.LogWarning("CameraControllerのターゲットが設定されていません！", this);
            return;
        }

        // 1. カメラの目標位置を計算する
        // ターゲットの位置から、ターゲットの「後ろ」方向( -target.forward )にdistance分離れ、
        // さらに「上」方向( Vector3.up )にheight分上がった位置を目標とする。
        Vector3 desiredPosition = target.position - (target.forward * distance) + (Vector3.up * height);

        // 2. カメラの位置を、現在の位置から目標位置へスムーズに移動させる
        // Vector3.Lerp(現在位置, 目標位置, 速度)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. カメラを常にターゲットの方向に向ける
        transform.LookAt(target);
    }
}