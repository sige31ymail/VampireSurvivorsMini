using UnityEngine;

/// <summary>カメラ追従 + 画面シェイク</summary>
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    Transform target;

    // 画面シェイク
    float shakeIntensity;
    float shakeDuration;
    float shakeTimer;
    Vector3 shakeOffset;

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            var p = Object.FindObjectOfType<Player>();
            if (p == null) return;
            target = p.transform;
        }

        var pos = target.position;
        pos.z = -10f;

        // シェイク処理
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            float t = shakeTimer / shakeDuration;
            float currentIntensity = shakeIntensity * t;
            shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * currentIntensity,
                Random.Range(-1f, 1f) * currentIntensity,
                0
            );
            pos += shakeOffset;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }

        transform.position = pos;
    }

    /// <summary>画面シェイクを開始</summary>
    public static void Shake(float intensity, float duration)
    {
        if (Instance == null) return;

        // 設定で無効化されている場合はスキップ
        if (SaveSystem.Instance != null && !SaveSystem.Instance.Settings.ScreenShake)
            return;

        if (intensity > Instance.shakeIntensity || Instance.shakeTimer <= 0)
        {
            Instance.shakeIntensity = intensity;
            Instance.shakeDuration = duration;
            Instance.shakeTimer = duration;
        }
    }

    /// <summary>小さいシェイク（ダメージ受けた時）</summary>
    public static void ShakeSmall() => Shake(0.15f, 0.1f);

    /// <summary>中程度のシェイク（敵撃破時など）</summary>
    public static void ShakeMedium() => Shake(0.25f, 0.15f);

    /// <summary>大きいシェイク（ボス出現、クリティカルなど）</summary>
    public static void ShakeLarge() => Shake(0.5f, 0.25f);

    /// <summary>超巨大シェイク（ボス撃破時）</summary>
    public static void ShakeHuge() => Shake(0.8f, 0.4f);
}
