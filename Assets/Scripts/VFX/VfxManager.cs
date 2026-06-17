using UnityEngine;

/// <summary>視覚効果管理（画面フラッシュ、スローモーションなど）</summary>
public class VfxManager : MonoBehaviour
{
    public static VfxManager Instance { get; private set; }

    // 画面フラッシュ
    float flashTimer;
    float flashDuration;
    Color flashColor;

    // スローモーション（ヒットストップ）
    float slowTimer;
    float originalTimeScale;

    // 画面ダメージエフェクト（赤い枠）
    float damageVignetteTimer;
    float damageVignetteIntensity;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // スローモーション
        if (slowTimer > 0)
        {
            slowTimer -= Time.unscaledDeltaTime;
            if (slowTimer <= 0)
            {
                Time.timeScale = originalTimeScale;
            }
        }

        // 画面フラッシュ
        if (flashTimer > 0)
        {
            flashTimer -= Time.unscaledDeltaTime;
        }

        // ダメージビネット
        if (damageVignetteTimer > 0)
        {
            damageVignetteTimer -= Time.unscaledDeltaTime;
        }
    }

    void OnGUI()
    {
        // 画面フラッシュ
        if (flashTimer > 0)
        {
            float alpha = (flashTimer / flashDuration) * flashColor.a;
            GUI.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // ダメージビネット（画面の端が赤くなる）
        if (damageVignetteTimer > 0)
        {
            float alpha = (damageVignetteTimer / 0.5f) * damageVignetteIntensity;
            DrawVignette(new Color(0.8f, 0.1f, 0.1f, alpha * 0.6f));
        }
    }

    void DrawVignette(Color color)
    {
        GUI.color = color;
        int border = 60;

        // 上
        GUI.DrawTexture(new Rect(0, 0, Screen.width, border), Texture2D.whiteTexture);
        // 下
        GUI.DrawTexture(new Rect(0, Screen.height - border, Screen.width, border), Texture2D.whiteTexture);
        // 左
        GUI.DrawTexture(new Rect(0, border, border, Screen.height - border * 2), Texture2D.whiteTexture);
        // 右
        GUI.DrawTexture(new Rect(Screen.width - border, border, border, Screen.height - border * 2), Texture2D.whiteTexture);

        GUI.color = Color.white;
    }

    /// <summary>画面フラッシュ</summary>
    public static void Flash(Color color, float duration)
    {
        if (Instance == null) return;
        Instance.flashColor = color;
        Instance.flashDuration = duration;
        Instance.flashTimer = duration;
    }

    /// <summary>白フラッシュ（レベルアップ、ボス撃破など）</summary>
    public static void FlashWhite() => Flash(new Color(1f, 1f, 1f, 0.5f), 0.15f);

    /// <summary>赤フラッシュ（大ダメージ）</summary>
    public static void FlashRed() => Flash(new Color(1f, 0.2f, 0.2f, 0.4f), 0.2f);

    /// <summary>金フラッシュ（実績、レアドロップなど）</summary>
    public static void FlashGold() => Flash(new Color(1f, 0.85f, 0.2f, 0.4f), 0.2f);

    /// <summary>スローモーション（ヒットストップ）</summary>
    public static void SlowMotion(float duration, float timeScale = 0.1f)
    {
        if (Instance == null) return;
        Instance.originalTimeScale = Time.timeScale > 0.5f ? Time.timeScale : 1f;
        Instance.slowTimer = duration;
        Time.timeScale = timeScale;
    }

    /// <summary>ヒットストップ（一瞬だけ止まる）</summary>
    public static void HitStop() => SlowMotion(0.05f, 0.02f);

    /// <summary>ダメージビネット（画面端が赤くなる）</summary>
    public static void DamageVignette(float intensity = 0.5f)
    {
        if (Instance == null) return;
        Instance.damageVignetteTimer = 0.5f;
        Instance.damageVignetteIntensity = Mathf.Clamp01(intensity);
    }
}
