using UnityEngine;

/// <summary>
/// 画面四隅を暗くする常設ビネット。カメラの子として追従し、ビューポート全体を覆う。
/// コード生成テクスチャなのでアセット不要。リッチなキャラと地面の間に「空気感」を作り、
/// 中央のプレイヤーに視線を集める。
///
/// UI は OnGUI(IMGUI) 描画で常にスプライトの上に出るため、このビネットは HP バーや
/// メニューを暗くしない（ワールド空間スプライトのため）。
/// </summary>
public class ScreenVignette : MonoBehaviour
{
    [Range(0f, 1f)] public float intensity = 0.6f; // 四隅の暗さ

    static Sprite vignetteSprite;
    float lastAspect = -1f;

    void Start()
    {
        var cam = Camera.main;
        if (cam == null) { enabled = false; return; }

        transform.SetParent(cam.transform, false);
        transform.localPosition = new Vector3(0f, 0f, 1f); // カメラ前方（クリップ内）

        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetSprite();
        sr.color = new Color(1f, 1f, 1f, intensity);
        sr.sortingOrder = 1000; // 全ワールドスプライトより上（IMGUIのUIより下）

        Resize(cam);
    }

    void LateUpdate()
    {
        // アスペクト比が変わったら覆い直す（ウィンドウリサイズ対策）
        var cam = Camera.main;
        if (cam != null && !Mathf.Approximately(cam.aspect, lastAspect))
            Resize(cam);
    }

    void Resize(Camera cam)
    {
        lastAspect = cam.aspect;
        float hh = cam.orthographicSize * 2f;
        float ww = hh * cam.aspect;
        transform.localScale = new Vector3(ww * 1.05f, hh * 1.05f, 1f); // 少し大きめに覆う
    }

    static Sprite GetSprite()
    {
        if (vignetteSprite != null) return vignetteSprite;

        const int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size / 2f, size / 2f);
        float maxd = size * 0.5f; // 端中央までの距離
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // 0=中央, 1=端中央, ~1.41=隅
            float d = Vector2.Distance(new Vector2(x, y), center) / maxd;
            float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1.15f, d));
            tex.SetPixel(x, y, new Color(0f, 0f, 0f, a));
        }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        vignetteSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
        return vignetteSprite;
    }
}
