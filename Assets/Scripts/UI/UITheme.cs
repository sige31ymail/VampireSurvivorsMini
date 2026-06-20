using UnityEngine;
using TMPro;

/// <summary>
/// uGUI 全体で共有する配色・フォント・パネル用スプライト。
/// ダークファンタジー基調（プレイヤーの紫帽子・金装飾に合わせる）。
/// </summary>
public static class UITheme
{
    // ── 配色 ──────────────────────────────
    public static readonly Color Bg          = new Color(0.05f, 0.05f, 0.08f, 0.96f); // 全画面暗転
    public static readonly Color Panel       = new Color(0.12f, 0.12f, 0.17f, 0.94f);
    public static readonly Color PanelLight  = new Color(0.18f, 0.18f, 0.26f, 0.96f);
    public static readonly Color Accent      = new Color(0.62f, 0.34f, 0.92f);        // 紫
    public static readonly Color AccentSoft  = new Color(0.42f, 0.26f, 0.60f, 1f);
    public static readonly Color Gold        = new Color(1.00f, 0.82f, 0.25f);
    public static readonly Color TextMain    = new Color(0.96f, 0.96f, 0.98f);
    public static readonly Color TextDim     = new Color(0.70f, 0.70f, 0.78f);
    public static readonly Color Hp          = new Color(0.90f, 0.25f, 0.30f);
    public static readonly Color Xp          = new Color(0.40f, 0.85f, 0.95f);

    public static readonly Color ButtonNormal  = new Color(0.17f, 0.17f, 0.25f, 0.96f);
    public static readonly Color ButtonHover   = new Color(0.32f, 0.24f, 0.46f, 1f);
    public static readonly Color ButtonPressed = new Color(0.10f, 0.09f, 0.15f, 1f);
    public static readonly Color ButtonDisabled= new Color(0.14f, 0.14f, 0.18f, 0.7f);

    // ── フォント ──────────────────────────
    static TMP_FontAsset font;
    static bool fontResolved;

    /// <summary>Resources/Fonts/ にある日本語TMPフォントを使う。無ければ既定にフォールバック。</summary>
    public static TMP_FontAsset Font
    {
        get
        {
            if (!fontResolved)
            {
                fontResolved = true;
                var found = Resources.LoadAll<TMP_FontAsset>("Fonts");
                if (found != null && found.Length > 0)
                    font = found[0];
                if (font == null)
                {
                    font = TMP_Settings.defaultFontAsset;
                    Debug.LogWarning("[UITheme] Resources/Fonts に日本語TMPフォントが見つかりません。" +
                                     "docs/UI_SETUP.md の手順でフォントアセットを生成・配置してください。" +
                                     "（現状は既定フォントで描画＝日本語が表示されない可能性があります）");
                }
            }
            return font;
        }
    }

    // ── パネル/ボタン用の角丸スプライト（9スライス） ──
    static Sprite rounded;
    public static Sprite RoundedSprite => rounded != null ? rounded : (rounded = BuildRounded());

    static Sprite BuildRounded()
    {
        const int s = 64;
        const float r = 16f;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        float half = s / 2f;
        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            // 角丸長方形のSDF
            float px = x + 0.5f - half;
            float py = y + 0.5f - half;
            float ax = Mathf.Abs(px) - (half - r);
            float ay = Mathf.Abs(py) - (half - r);
            float qx = Mathf.Max(ax, 0f);
            float qy = Mathf.Max(ay, 0f);
            float outside = Mathf.Sqrt(qx * qx + qy * qy);
            float inside = Mathf.Min(Mathf.Max(ax, ay), 0f);
            float d = outside + inside - r;
            float a = Mathf.Clamp01(0.5f - d);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        // border=r で9スライス化（角を保ったまま伸縮）
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f),
            s, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
    }
}
