using UnityEngine;

/// <summary>
/// グリッド背景。SpriteRenderer で描画し、タイルグリッドにスナップして追従する。
/// タイル境界でスナップするため継ぎ目が見えない。
/// </summary>
public class Background : MonoBehaviour
{
    const float TileWorldSize = 2f;  // グリッド1マス = 2ワールド単位
    const int   TilesAcross  = 16;   // スプライト1枚に含むタイル数（縦横）
    const int   TexPerTile   = 32;   // タイル1マスのピクセル数

    void Start()
    {
        int total = TexPerTile * TilesAcross;
        var tex = new Texture2D(total, total, TextureFormat.RGB24, false);

        var bg   = new Color(0.05f, 0.05f, 0.10f);
        var line = new Color(0.22f, 0.22f, 0.34f);

        for (int y = 0; y < total; y++)
        for (int x = 0; x < total; x++)
        {
            // 2ピクセル幅のグリッド線
            bool isLine = x % TexPerTile < 2 || y % TexPerTile < 2;
            tex.SetPixel(x, y, isLine ? line : bg);
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        float ppu = TexPerTile / TileWorldSize; // pixels per unit
        var sprite = Sprite.Create(tex,
            new Rect(0, 0, total, total),
            Vector2.one * 0.5f,
            ppu);

        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = -100;
        sr.color        = Color.white;
    }

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;

        var p = cam.transform.position;
        // タイルグリッド単位でスナップ → グリッド線の位置が変わらず継ぎ目なし
        p.x = Mathf.Round(p.x / TileWorldSize) * TileWorldSize;
        p.y = Mathf.Round(p.y / TileWorldSize) * TileWorldSize;
        p.z = 0f;
        transform.position = p;
    }
}
