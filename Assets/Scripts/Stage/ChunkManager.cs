using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理エンジン不使用の無限チャンク・ストリーミング。
/// プレイヤー周囲の 3x3 チャンクだけを生成し、離れたチャンクはプールへ返す。
/// 地面は Background.cs（無限タイリング）が担当し、ここでは障害物・プロップを配置する。
/// 各チャンクの内容はチャンク座標シードで決定論的に生成され、再訪時も同じ見た目になる。
/// 当たり判定はプレイヤー専用（Arena 経由）。敵は制限しない。
///
/// アートは Resources/Props/&lt;biome&gt;/ にPNGがあればそれを使い、無ければコード生成図形にフォールバック。
/// </summary>
public class ChunkManager : MonoBehaviour
{
    class Chunk
    {
        public long Key;
        public int Cx, Cy;
        public readonly List<GameObject> Props = new List<GameObject>();
        public readonly List<GameObject> Ground = new List<GameObject>();
        public readonly List<Arena.Obstacle> Obstacles = new List<Arena.Obstacle>();
    }

    const float ChunkSize   = 20f;   // 1チャンクのワールドサイズ
    const int   ViewRadius  = 1;     // 周囲±1 = 3x3
    const float EdgeInset   = 3f;    // チャンク境界から障害物を離す → グリッド線沿いに必ず通路ができる
    const float MinGap      = 1.6f;  // 障害物どうしの最小すき間（プレイヤーが抜けられる）
    const float CenterClear = 4.5f;  // ワールド原点（開始地点）周辺は障害物なし
    const int   MinPerChunk = 4;
    const int   MaxPerChunk = 8;
    const float GroundTileSize = 4f; // 地面タイル1枚のワールドサイズ（ChunkSize を割り切ること）

    Transform player;
    Transform parent;
    int stageSeed;
    (Color color, bool round, float min, float max) theme;
    Texture2D[] artTextures;
    Sprite[] groundSprites; // Ground/<biome>/ のタイル（無ければ null）
    int groundBaseIndex;    // 地面に使う基本タイルのインデックス

    readonly Dictionary<long, Chunk> active = new Dictionary<long, Chunk>();
    readonly Stack<GameObject> pool = new Stack<GameObject>();
    readonly Stack<GameObject> groundPool = new Stack<GameObject>();
    readonly Dictionary<Texture2D, Sprite> spriteCache = new Dictionary<Texture2D, Sprite>();
    readonly List<long> tmpRemove = new List<long>();

    bool hasCell;
    int curCx, curCy;

    public void Init(StageData stage)
    {
        Arena.Reset();
        Arena.Enable();
        parent = new GameObject("Chunks").transform;
        stageSeed = (int)stage.Type * 911 + 12345;
        theme = ObstacleTheme(stage.Type);
        artTextures = Resources.LoadAll<Texture2D>("Props/" + BiomeFolder(stage.Type));
        var groundTexs = Resources.LoadAll<Texture2D>("Ground/" + BiomeFolder(stage.Type));
        groundSprites = BuildGroundSprites(groundTexs);
        groundBaseIndex = PickBaseIndex(groundTexs, BiomeFolder(stage.Type));
    }

    void Update()
    {
        if (player == null)
        {
            var p = FindFirstObjectByType<Player>();
            if (p == null) return;
            player = p.transform;
        }

        int cx = Mathf.FloorToInt(player.position.x / ChunkSize);
        int cy = Mathf.FloorToInt(player.position.y / ChunkSize);
        if (hasCell && cx == curCx && cy == curCy) return; // セルを跨いだ時だけ更新

        curCx = cx; curCy = cy; hasCell = true;
        Restream(cx, cy);
    }

    void Restream(int cx, int cy)
    {
        // 必要な近傍チャンクを生成
        for (int dx = -ViewRadius; dx <= ViewRadius; dx++)
        for (int dy = -ViewRadius; dy <= ViewRadius; dy++)
        {
            int x = cx + dx, y = cy + dy;
            long key = Key(x, y);
            if (!active.ContainsKey(key))
                active[key] = BuildChunk(x, y, key);
        }

        // 範囲外のチャンクを解放
        tmpRemove.Clear();
        foreach (var kv in active)
        {
            var c = kv.Value;
            if (Mathf.Abs(c.Cx - cx) > ViewRadius || Mathf.Abs(c.Cy - cy) > ViewRadius)
                tmpRemove.Add(kv.Key);
        }
        for (int i = 0; i < tmpRemove.Count; i++)
        {
            ReleaseChunk(active[tmpRemove[i]]);
            active.Remove(tmpRemove[i]);
        }
    }

    Chunk BuildChunk(int cx, int cy, long key)
    {
        var chunk = new Chunk { Key = key, Cx = cx, Cy = cy };
        // チャンク座標による決定論シード（再訪で同じ内容）
        var rng = new System.Random(cx * 73856093 ^ cy * 19349663 ^ stageSeed);

        float baseX = cx * ChunkSize, baseY = cy * ChunkSize;
        float span  = ChunkSize - EdgeInset * 2f;
        int count = MinPerChunk + rng.Next(MaxPerChunk - MinPerChunk + 1);

        int attempts = 0, maxAttempts = count * 20;
        while (chunk.Obstacles.Count < count && attempts < maxAttempts)
        {
            attempts++;
            var pos = new Vector2(
                baseX + EdgeInset + (float)rng.NextDouble() * span,
                baseY + EdgeInset + (float)rng.NextDouble() * span);

            if (pos.sqrMagnitude < CenterClear * CenterClear) continue; // 開始地点は空ける

            float scale  = theme.min + (float)rng.NextDouble() * (theme.max - theme.min);
            float radius = scale * (theme.round ? 0.46f : 0.42f);

            bool ok = true;
            for (int i = 0; i < chunk.Obstacles.Count; i++)
            {
                float need = chunk.Obstacles[i].Radius + radius + MinGap;
                if ((chunk.Obstacles[i].Center - pos).sqrMagnitude < need * need) { ok = false; break; }
            }
            if (!ok) continue;

            chunk.Obstacles.Add(new Arena.Obstacle { Center = pos, Radius = radius });
            SpawnProp(chunk, pos, scale, rng);
        }

        LayGround(chunk, cx, cy);
        Arena.SetChunk(key, chunk.Obstacles);
        return chunk;
    }

    void SpawnProp(Chunk chunk, Vector2 pos, float scale, System.Random rng)
    {
        var go = GetPooled();
        go.transform.position   = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = Vector3.one * scale;

        var sr = go.GetComponent<SpriteRenderer>();
        if (artTextures != null && artTextures.Length > 0)
        {
            sr.sprite = SpriteFor(artTextures[rng.Next(artTextures.Length)]);
            sr.color  = Color.white;
        }
        else
        {
            sr.sprite = theme.round ? VampireSurvivorsMini.CircleSprite : VampireSurvivorsMini.SquareSprite;
            float shade = 0.85f + (float)rng.NextDouble() * 0.27f;
            sr.color = new Color(theme.color.r * shade, theme.color.g * shade, theme.color.b * shade, 1f);
        }
        sr.sortingOrder = 0; // 地面より上・キャラより下

        go.SetActive(true);
        chunk.Props.Add(go);
    }

    void ReleaseChunk(Chunk chunk)
    {
        Arena.RemoveChunk(chunk.Key);
        for (int i = 0; i < chunk.Props.Count; i++)
        {
            chunk.Props[i].SetActive(false);
            pool.Push(chunk.Props[i]);
        }
        chunk.Props.Clear();
        for (int i = 0; i < chunk.Ground.Count; i++)
        {
            chunk.Ground[i].SetActive(false);
            groundPool.Push(chunk.Ground[i]);
        }
        chunk.Ground.Clear();
    }

    GameObject GetPooled()
    {
        if (pool.Count > 0)
        {
            var g = pool.Pop();
            return g;
        }
        var go = new GameObject("Prop");
        go.transform.SetParent(parent);
        go.AddComponent<SpriteRenderer>();
        // 足元の接地影（子として付くのでプロップのスケールに追従。再利用時も維持される）
        GroundShadow.Attach(go.transform, 0.8f, 0.28f, -0.42f, 0.32f);
        return go;
    }

    /// <summary>テクスチャを 1 ワールド単位幅のスプライトにして使い回す（スケールは transform 側で調整）。</summary>
    Sprite SpriteFor(Texture2D tex)
    {
        if (spriteCache.TryGetValue(tex, out var s)) return s;
        tex.filterMode = FilterMode.Point;
        s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), tex.width); // ppu = width → 横幅1ワールド単位
        spriteCache[tex] = s;
        return s;
    }

    /// <summary>チャンクに地面タイルのグリッドを敷く（Ground/&lt;biome&gt;/ の変種を決定論で選択）。</summary>
    void LayGround(Chunk chunk, int cx, int cy)
    {
        if (groundSprites == null || groundSprites.Length == 0) return;

        int n = Mathf.RoundToInt(ChunkSize / GroundTileSize);
        float baseX = cx * ChunkSize, baseY = cy * ChunkSize;

        // 地面は単一の基本タイルで統一する。明るさの違う変種を混ぜるとグリッド（市松）が
        // 目立つため。変化は Decor レイヤー（散布する透過小物）で付ける方針。
        var sprite = groundSprites[groundBaseIndex];
        for (int gx = 0; gx < n; gx++)
        for (int gy = 0; gy < n; gy++)
        {
            var go = GetGroundTile();
            go.transform.position = new Vector3(
                baseX + (gx + 0.5f) * GroundTileSize,
                baseY + (gy + 0.5f) * GroundTileSize, 0f);
            go.GetComponent<SpriteRenderer>().sprite = sprite;
            go.SetActive(true);
            chunk.Ground.Add(go);
        }
    }

    GameObject GetGroundTile()
    {
        if (groundPool.Count > 0) return groundPool.Pop();
        var go = new GameObject("GroundTile");
        go.transform.SetParent(parent);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -50; // Background(-100)より上、影(-5)・障害物(0)より下
        return go;
    }

    /// <summary>地面テクスチャを「1枚=GroundTileSize ワールド単位」のスプライトに変換。</summary>
    static Sprite[] BuildGroundSprites(Texture2D[] texs)
    {
        if (texs == null || texs.Length == 0) return null;
        var arr = new Sprite[texs.Length];
        for (int i = 0; i < texs.Length; i++)
        {
            texs[i].filterMode = FilterMode.Point; // 同変種の隣接を継ぎ目なく
            arr[i] = Sprite.Create(texs[i], new Rect(0, 0, texs[i].width, texs[i].height),
                new Vector2(0.5f, 0.5f), texs[i].width / GroundTileSize);
        }
        return arr;
    }

    /// <summary>基本タイルのインデックス。biome名と一致 → 無印(_なし) → 先頭、の優先で選ぶ。</summary>
    static int PickBaseIndex(Texture2D[] texs, string folder)
    {
        if (texs == null || texs.Length == 0) return 0;
        for (int i = 0; i < texs.Length; i++) if (texs[i].name == folder) return i;
        for (int i = 0; i < texs.Length; i++) if (!texs[i].name.Contains("_")) return i;
        return 0;
    }

    static long Key(int x, int y) => ((long)(uint)x << 32) | (uint)y;

    static string BiomeFolder(StageType type)
    {
        switch (type)
        {
            case StageType.Forest:    return "forest";
            case StageType.Graveyard: return "graveyard";
            case StageType.Castle:    return "castle";
            default:                  return "grassland";
        }
    }

    /// <summary>ステージごとのコード生成障害物の見た目（色・形・サイズ範囲）。アートが無い時のフォールバック。</summary>
    static (Color color, bool round, float min, float max) ObstacleTheme(StageType type)
    {
        switch (type)
        {
            case StageType.Grassland: return (new Color(0.45f, 0.46f, 0.50f), true,  0.9f, 1.6f); // 岩
            case StageType.Forest:    return (new Color(0.13f, 0.30f, 0.15f), true,  1.2f, 2.2f); // 木立
            case StageType.Graveyard: return (new Color(0.55f, 0.55f, 0.60f), false, 0.9f, 1.5f); // 墓石
            case StageType.Castle:    return (new Color(0.30f, 0.27f, 0.34f), false, 1.0f, 1.7f); // 石柱
            default:                  return (new Color(0.45f, 0.46f, 0.50f), true,  0.9f, 1.6f);
        }
    }
}
