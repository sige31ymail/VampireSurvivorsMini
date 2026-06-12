using UnityEngine;

/// <summary>
/// グラス画像をタイリングして無限スクロール背景を作る。
/// Resources/Tiles/grass.png が見つからない場合は旧来のグリッドにフォールバック。
/// </summary>
public class Background : MonoBehaviour
{
    const float TileWorldSize = 2f;

    void Start()
    {
        var tex = Resources.Load<Texture2D>("Tiles/grass");
        if (tex != null)
            BuildTiled(tex);
        else
            BuildFallbackGrid();
    }

    void BuildTiled(Texture2D tex)
    {
        tex.filterMode = FilterMode.Point;

        // テクスチャの横幅が TileWorldSize ワールド単位になるよう PPU を計算
        float ppu    = tex.width / TileWorldSize;
        var   sprite = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            Vector2.one * 0.5f, ppu);

        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.drawMode     = SpriteDrawMode.Tiled;
        sr.size         = new Vector2(52f, 52f); // 画面外までカバー
        sr.sortingOrder = -100;
    }

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;

        // タイルグリッド単位でスナップ → 継ぎ目なく追従
        var p = cam.transform.position;
        p.x = Mathf.Round(p.x / TileWorldSize) * TileWorldSize;
        p.y = Mathf.Round(p.y / TileWorldSize) * TileWorldSize;
        p.z = 0f;
        transform.position = p;
    }

    // ── フォールバック：コード生成グリッド ──────
    void BuildFallbackGrid()
    {
        const int TexPerTile = 32;
        const int TilesAcross = 16;
        int total = TexPerTile * TilesAcross;

        var tex  = new Texture2D(total, total, TextureFormat.RGB24, false);
        var bg   = new Color(0.05f, 0.05f, 0.10f);
        var line = new Color(0.22f, 0.22f, 0.34f);

        for (int y = 0; y < total; y++)
        for (int x = 0; x < total; x++)
        {
            bool isLine = x % TexPerTile < 2 || y % TexPerTile < 2;
            tex.SetPixel(x, y, isLine ? line : bg);
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        float ppu    = TexPerTile / TileWorldSize;
        var   sprite = Sprite.Create(tex,
            new Rect(0, 0, total, total), Vector2.one * 0.5f, ppu);

        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = -100;
    }
}
