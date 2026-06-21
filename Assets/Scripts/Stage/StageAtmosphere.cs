using UnityEngine;

/// <summary>
/// バイオーム別の「空気感」を作る（物理ライト非依存・スプライトベース）。
/// ・暗いステージはプレイヤー追従の放射状ダークオーバーレイで「光だまり」を演出（Halls of Torment風）
/// ・ScreenVignette の強度をステージ別に調整
/// ・カメラ背景をステージ色から少し沈める
/// URP の Light2D API に依存しないので、確実にコンパイル・描画できる。
/// </summary>
public class StageAtmosphere : MonoBehaviour
{
    static Sprite holeSprite; // 中心透明 → 周辺不透明の放射状グラデ

    Transform target;       // プレイヤー
    SpriteRenderer darkSr;  // プレイヤー追従のダークオーバーレイ
    float lastAspect = -1f;

    public void Init(StageData stage)
    {
        var cfg = Config(stage.Type);

        // ScreenVignette 強度
        var vig = FindFirstObjectByType<ScreenVignette>();
        if (vig != null) vig.intensity = cfg.vignette;

        // カメラ背景をステージ色から少し沈める
        if (Camera.main != null)
            Camera.main.backgroundColor = Color.Lerp(stage.BackgroundColor, Color.black, cfg.bgDarken);

        // 暗所のみダークオーバーレイを生成
        if (cfg.darkness > 0.001f)
        {
            var go = new GameObject("PlayerSpotlight");
            darkSr = go.AddComponent<SpriteRenderer>();
            darkSr.sprite = GetHoleSprite();
            darkSr.color = new Color(cfg.tint.r, cfg.tint.g, cfg.tint.b, cfg.darkness);
            darkSr.sortingOrder = 900; // ワールドより上、ScreenVignette(1000)より下
        }
    }

    void LateUpdate()
    {
        if (darkSr == null) return;

        if (target == null)
        {
            var p = FindFirstObjectByType<Player>();
            if (p == null) return;
            target = p.transform;
        }

        var cam = Camera.main;
        if (cam == null) return;

        // プレイヤーに追従（光だまりを足元に保つ）
        var pos = target.position; pos.z = 0f;
        darkSr.transform.position = pos;

        // 画面を覆うサイズ（追従ズレ・斜めも吸収して大きめに）
        if (!Mathf.Approximately(cam.aspect, lastAspect))
        {
            lastAspect = cam.aspect;
            float hh = cam.orthographicSize * 2f;
            float ww = hh * cam.aspect;
            float cover = Mathf.Max(ww, hh) * 1.7f;
            darkSr.transform.localScale = new Vector3(cover, cover, 1f);
        }
    }

    // ステージ別パラメータ（暗さ・色味・ヴィネット・背景の沈め具合）
    static (float darkness, Color tint, float vignette, float bgDarken) Config(StageType type)
    {
        switch (type)
        {
            // 草原：明るい昼。暗所演出なし
            case StageType.Grassland: return (0.00f, Color.white,                       0.45f, 0.00f);
            // 森：木陰でやや暗い緑
            case StageType.Forest:    return (0.30f, new Color(0.10f, 0.16f, 0.12f),     0.58f, 0.15f);
            // 墓地：冷たく暗い青
            case StageType.Graveyard: return (0.45f, new Color(0.08f, 0.10f, 0.16f),     0.66f, 0.25f);
            // 城：最も暗い紫闇
            case StageType.Castle:    return (0.55f, new Color(0.10f, 0.07f, 0.14f),     0.72f, 0.30f);
            default:                  return (0.00f, Color.white,                       0.45f, 0.00f);
        }
    }

    /// <summary>中心透明 → 周辺不透明の放射状スプライト（色は SpriteRenderer.color で着色）。</summary>
    static Sprite GetHoleSprite()
    {
        if (holeSprite != null) return holeSprite;

        const int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size / 2f, size / 2f);
        float maxd = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), center) / maxd; // 0=中心,1=端中央
            // 中心側(～0.34)は透明な「光だまり」、0.72で完全に暗く
            float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.34f, 0.72f, d));
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        holeSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return holeSprite;
    }
}
