using UnityEngine;

/// <summary>
/// IMGUI(OnGUI)のままUIの質感を上げる共有スキン。
/// 角丸9スライスのパネル/ボタン、文字の影、ホバー/押し込み、統一配色を提供する。
/// ※ フォントは GUI.skin 既定のまま（日本語表示を維持するため font は設定しない）。
/// </summary>
public static class UISkin
{
    // ── 配色（ダークファンタジー基調） ──
    public static readonly Color Panel       = new Color(0.10f, 0.10f, 0.15f, 0.92f);
    public static readonly Color PanelDeep   = new Color(0.05f, 0.05f, 0.08f, 0.92f);
    public static readonly Color Accent      = new Color(0.62f, 0.34f, 0.92f);
    public static readonly Color Gold        = new Color(1.00f, 0.84f, 0.28f);
    public static readonly Color TextMain    = new Color(0.96f, 0.96f, 0.98f);
    public static readonly Color TextDim     = new Color(0.72f, 0.72f, 0.80f);
    public static readonly Color Hp          = new Color(0.88f, 0.26f, 0.30f);
    public static readonly Color Xp          = new Color(0.42f, 0.88f, 0.55f);
    public static readonly Color Shadow      = new Color(0f, 0f, 0f, 0.7f);

    static readonly Color BtnNormal  = new Color(0.16f, 0.16f, 0.24f, 0.96f);
    static readonly Color BtnHover   = new Color(0.32f, 0.24f, 0.46f, 1f);
    static readonly Color BtnActive  = new Color(0.09f, 0.08f, 0.14f, 1f);

    static bool ready;
    static Texture2D texWhite;       // 白い角丸（色はGUI.colorで乗算）
    static Texture2D texBtn, texBtnHover, texBtnActive;

    public static GUIStyle Header, Big, Label, LabelDim, GoldLabel, Button, Card, BarText;
    static GUIStyle whiteBox;

    public static void Init()
    {
        if (ready) return;
        ready = true;

        const int s = 32, r = 10;
        texWhite     = BuildRounded(s, r, Color.white);
        texBtn       = BuildRounded(s, r, BtnNormal);
        texBtnHover  = BuildRounded(s, r, BtnHover);
        texBtnActive = BuildRounded(s, r, BtnActive);
        var border = new RectOffset(r, r, r, r);

        whiteBox = new GUIStyle { border = border };
        whiteBox.normal.background = texWhite;

        Header = new GUIStyle(GUI.skin.label)
        { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        Header.normal.textColor = Gold;

        Big = new GUIStyle(GUI.skin.label)
        { fontSize = 54, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        Big.normal.textColor = TextMain;

        Label = new GUIStyle(GUI.skin.label) { fontSize = 18 };
        Label.normal.textColor = TextMain;

        LabelDim = new GUIStyle(GUI.skin.label) { fontSize = 16 };
        LabelDim.normal.textColor = TextDim;

        GoldLabel = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
        GoldLabel.normal.textColor = Gold;

        BarText = new GUIStyle(GUI.skin.label)
        { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        BarText.normal.textColor = TextMain;

        Button = new GUIStyle
        {
            border = border,
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            padding = new RectOffset(12, 12, 8, 8)
        };
        Button.normal.background = texBtn;    Button.normal.textColor = TextMain;
        Button.hover.background  = texBtnHover; Button.hover.textColor = Color.white;
        Button.active.background = texBtnActive; Button.active.textColor = TextDim;

        Card = new GUIStyle(Button)
        { alignment = TextAnchor.MiddleLeft, fontSize = 19, padding = new RectOffset(20, 16, 10, 10), richText = true };
    }

    // ── 描画ヘルパー ──────────────────────
    /// <summary>角丸の塗り（色はGUI.colorで乗算）。</summary>
    public static void Box(Rect r, Color color)
    {
        var prev = GUI.color;
        GUI.color = color;
        GUI.Box(r, GUIContent.none, whiteBox);
        GUI.color = prev;
    }

    /// <summary>全画面の暗転オーバーレイ。</summary>
    public static void DimScreen(float alpha)
    {
        var prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = prev;
    }

    /// <summary>影付きラベル（同じスタイルで2回描く）。</summary>
    public static void ShadowLabel(Rect r, string text, GUIStyle style)
    {
        var keep = style.normal.textColor;
        style.normal.textColor = Shadow;
        GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), text, style);
        style.normal.textColor = keep;
        GUI.Label(r, text, style);
    }

    /// <summary>角丸バー（背景＋フィル＋影付き中央テキスト）。</summary>
    public static void Bar(Rect r, float ratio, Color fill, string text)
    {
        Box(r, new Color(0f, 0f, 0f, 0.55f));
        var inner = new Rect(r.x + 3f, r.y + 3f, (r.width - 6f) * Mathf.Clamp01(ratio), r.height - 6f);
        if (inner.width > 1f) Box(inner, fill);
        if (!string.IsNullOrEmpty(text)) ShadowLabel(r, text, BarText);
    }

    /// <summary>角丸ボタン（ホバー/押し込みつき）。</summary>
    public static bool Button2(Rect r, string text) => GUI.Button(r, text, Button);

    /// <summary>角丸パネル。</summary>
    public static void PanelBox(Rect r, Color color) => Box(r, color);

    static Texture2D BuildRounded(int size, float radius, Color color)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        float half = size / 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float px = x + 0.5f - half;
            float py = y + 0.5f - half;
            float ax = Mathf.Abs(px) - (half - radius);
            float ay = Mathf.Abs(py) - (half - radius);
            float qx = Mathf.Max(ax, 0f);
            float qy = Mathf.Max(ay, 0f);
            float outside = Mathf.Sqrt(qx * qx + qy * qy);
            float inside = Mathf.Min(Mathf.Max(ax, ay), 0f);
            float d = outside + inside - radius;
            float a = Mathf.Clamp01(0.5f - d) * color.a;
            tex.SetPixel(x, y, new Color(color.r, color.g, color.b, a));
        }
        tex.Apply();
        return tex;
    }
}
