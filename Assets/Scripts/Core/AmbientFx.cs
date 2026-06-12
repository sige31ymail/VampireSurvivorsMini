using UnityEngine;

/// <summary>床装飾・木・道・街をフィールドに配置する</summary>
public class AmbientFx : MonoBehaviour
{
    const float FieldRadius   = 65f;
    const float TileWorldSize = 2f;

    Texture2D soilTex;

    void Start()
    {
        soilTex = Resources.Load<Texture2D>("Tiles/soil");
        if (soilTex != null) soilTex.filterMode = FilterMode.Point;

        SpawnRoads();
        SpawnFloorDecor();
        SpawnForests();
        SpawnTown();
    }

    // ── 道 ──────────────────────────────────
    void SpawnRoads()
    {
        SpawnRoad(new Vector3(0f, 2f, 0),  0f, 130f, 2.6f);
        SpawnRoad(new Vector3(6f, 0f, 0), 90f, 130f, 2.2f);
    }

    void SpawnRoad(Vector3 center, float angleDeg, float length, float width)
    {
        var go = new GameObject("Road");
        go.transform.SetParent(transform);
        go.transform.position = center;
        go.transform.rotation = Quaternion.Euler(0, 0, angleDeg);
        // ※ SpriteDrawMode.Tiled 使用時は localScale ではなく sr.size でサイズ指定

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -98;

        if (soilTex != null)
        {
            float ppu = soilTex.width / TileWorldSize;
            sr.sprite   = Sprite.Create(soilTex,
                new Rect(0, 0, soilTex.width, soilTex.height),
                Vector2.one * 0.5f, ppu);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size     = new Vector2(length, width);
        }
        else
        {
            sr.sprite = VampireSurvivorsMini.SquareSprite;
            sr.color  = new Color(0.20f, 0.17f, 0.13f);
            go.transform.localScale = new Vector3(length, width, 1f);
        }

        // 両端の境界線
        float halfW = width * 0.5f;
        var   edge  = new Color(0.28f, 0.22f, 0.15f);
        Sub(go, VampireSurvivorsMini.SquareSprite, edge,
            new Vector3(0,  halfW - 0.07f, 0), new Vector3(length, 0.14f, 1), -97);
        Sub(go, VampireSurvivorsMini.SquareSprite, edge,
            new Vector3(0, -(halfW - 0.07f), 0), new Vector3(length, 0.14f, 1), -97);
    }

    // ── 床装飾 ───────────────────────────────
    void SpawnFloorDecor()
    {
        var rng   = new System.Random(54321);
        var color = new Color(0.52f, 0.50f, 0.40f);

        for (int i = 0; i < 110; i++)
        {
            float x = (float)(rng.NextDouble() * FieldRadius * 2 - FieldRadius);
            float y = (float)(rng.NextDouble() * FieldRadius * 2 - FieldRadius);
            if (x * x + y * y < 3f) continue;

            var root = new GameObject("Decor");
            root.transform.SetParent(transform);
            root.transform.position = new Vector3(x, y, 0);

            switch (rng.Next(3))
            {
                case 0:
                    Sub(root, VampireSurvivorsMini.SquareSprite, color,
                        Vector3.zero, new Vector3(0.08f, 0.40f, 1), -95);
                    Sub(root, VampireSurvivorsMini.SquareSprite, color,
                        Vector3.zero, new Vector3(0.40f, 0.08f, 1), -95);
                    break;
                case 1:
                    Sub(root, VampireSurvivorsMini.CircleSprite, color,
                        Vector3.zero, new Vector3(0.30f, 0.30f, 1), -95);
                    Sub(root, VampireSurvivorsMini.CircleSprite, new Color(0.07f, 0.07f, 0.13f),
                        Vector3.zero, new Vector3(0.17f, 0.17f, 1), -94);
                    break;
                case 2:
                    Sub(root, VampireSurvivorsMini.SquareSprite, color, new Vector3(-0.16f,  0f, 0), new Vector3(0.10f, 0.10f, 1), -95);
                    Sub(root, VampireSurvivorsMini.SquareSprite, color, new Vector3( 0.16f,  0f, 0), new Vector3(0.10f, 0.10f, 1), -95);
                    Sub(root, VampireSurvivorsMini.SquareSprite, color, new Vector3(0,  0.16f, 0), new Vector3(0.10f, 0.10f, 1), -95);
                    Sub(root, VampireSurvivorsMini.SquareSprite, color, new Vector3(0, -0.16f, 0), new Vector3(0.10f, 0.10f, 1), -95);
                    break;
            }
        }
    }

    // ── 森 ───────────────────────────────────
    void SpawnForests()
    {
        var rng = new System.Random(77777);

        for (int c = 0; c < 8; c++)
        {
            float angle = (float)(rng.NextDouble() * Mathf.PI * 2);
            float dist  = (float)(rng.NextDouble() * 35 + 15);
            var center  = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);

            int treeCount = rng.Next(7, 13);
            for (int t = 0; t < treeCount; t++)
            {
                float ox = (float)(rng.NextDouble() * 10 - 5);
                float oy = (float)(rng.NextDouble() * 10 - 5);
                var pos  = new Vector3(center.x + ox, center.y + oy, 0);
                if (pos.sqrMagnitude < 16f) continue;

                float scale = (float)(rng.NextDouble() * 0.45 + 0.65);
                SpawnTree(pos, scale);
            }
        }
    }

    void SpawnTree(Vector3 pos, float scale)
    {
        var root = new GameObject("Tree");
        root.transform.SetParent(transform);
        root.transform.position   = pos;
        root.transform.localScale = Vector3.one * scale;

        Sub(root, VampireSurvivorsMini.SquareSprite, new Color(0.35f, 0.22f, 0.10f),
            new Vector3(0f, -0.12f, 0), new Vector3(0.14f, 0.34f, 1), 1);
        Sub(root, VampireSurvivorsMini.CircleSprite, new Color(0.11f, 0.30f, 0.09f),
            new Vector3(0f,  0.28f, 0), new Vector3(0.65f, 0.65f, 1), 2);
        Sub(root, VampireSurvivorsMini.CircleSprite, new Color(0.17f, 0.42f, 0.13f),
            new Vector3(-0.16f, 0.20f, 0), new Vector3(0.46f, 0.46f, 1), 3);
        Sub(root, VampireSurvivorsMini.CircleSprite, new Color(0.17f, 0.42f, 0.13f),
            new Vector3( 0.16f, 0.16f, 0), new Vector3(0.42f, 0.42f, 1), 3);
    }

    // ── 街 ───────────────────────────────────
    void SpawnTown()
    {
        // 東西道路(y=2)沿いに、x=15付近を中心に配置
        const float cx    = 15f;
        const float roadY = 2f;

        // 道の北側の建物列
        float northY = roadY + 3.4f;
        SpawnBuilding(new Vector3(cx - 6.2f, northY,        0), 2.2f, 2.4f, true);
        SpawnBuilding(new Vector3(cx - 2.6f, northY + 0.3f, 0), 3.4f, 2.9f, false); // 宿屋（大）
        SpawnBuilding(new Vector3(cx + 2.0f, northY,        0), 2.0f, 2.2f, true);
        SpawnBuilding(new Vector3(cx + 5.2f, northY,        0), 1.8f, 2.0f, false);

        // 道の南側の建物列
        float southY = roadY - 3.2f;
        SpawnBuilding(new Vector3(cx - 5.2f, southY, 0), 2.0f, 2.2f, true);
        SpawnBuilding(new Vector3(cx - 1.5f, southY, 0), 2.8f, 2.5f, false);
        SpawnBuilding(new Vector3(cx + 2.6f, southY, 0), 2.0f, 2.1f, true);

        // 井戸（道の北寄り、建物と道の間の広場）
        SpawnWell(new Vector3(cx, roadY + 1.6f, 0));
    }

    void SpawnBuilding(Vector3 pos, float w, float h, bool hasChimney)
    {
        var root = new GameObject("Building");
        root.transform.SetParent(transform);
        root.transform.position = pos;

        var wallColor = new Color(0.43f, 0.40f, 0.34f);
        var roofColor = new Color(0.42f, 0.22f, 0.16f);
        var winColor  = new Color(0.18f, 0.25f, 0.50f);
        var doorColor = new Color(0.22f, 0.14f, 0.09f);

        float wallH = h * 0.68f;
        float roofH = h * 0.34f;
        float wallY = 0f;
        float roofY = wallH * 0.5f + roofH * 0.5f - 0.04f;

        // 壁
        Sub(root, VampireSurvivorsMini.SquareSprite, wallColor,
            new Vector3(0, wallY, 0), new Vector3(w, wallH, 1), 2);
        // 屋根（壁より少し幅広）
        Sub(root, VampireSurvivorsMini.SquareSprite, roofColor,
            new Vector3(0, roofY, 0), new Vector3(w + 0.28f, roofH, 1), 3);
        // ドア
        Sub(root, VampireSurvivorsMini.SquareSprite, doorColor,
            new Vector3(0, wallY - wallH * 0.27f, 0), new Vector3(0.34f, 0.48f, 1), 4);
        // 窓（建物幅が広ければ4つ、そうでなければ2つ）
        float winW = 0.27f, winH = 0.29f;
        float winY = wallY + wallH * 0.20f;
        if (w >= 2.8f)
        {
            for (int i = 0; i < 4; i++)
            {
                float wx = Mathf.Lerp(-w * 0.36f, w * 0.36f, i / 3f);
                Sub(root, VampireSurvivorsMini.SquareSprite, winColor,
                    new Vector3(wx, winY, 0), new Vector3(winW, winH, 1), 4);
            }
        }
        else
        {
            Sub(root, VampireSurvivorsMini.SquareSprite, winColor,
                new Vector3(-w * 0.23f, winY, 0), new Vector3(winW, winH, 1), 4);
            Sub(root, VampireSurvivorsMini.SquareSprite, winColor,
                new Vector3( w * 0.23f, winY, 0), new Vector3(winW, winH, 1), 4);
        }
        // 煙突
        if (hasChimney)
        {
            Sub(root, VampireSurvivorsMini.SquareSprite, new Color(0.32f, 0.29f, 0.26f),
                new Vector3(w * 0.28f, roofY + roofH * 0.5f + 0.18f, 0), new Vector3(0.22f, 0.36f, 1), 4);
        }
    }

    void SpawnWell(Vector3 pos)
    {
        var root = new GameObject("Well");
        root.transform.SetParent(transform);
        root.transform.position = pos;

        // 石枠（外）
        Sub(root, VampireSurvivorsMini.CircleSprite, new Color(0.40f, 0.37f, 0.30f),
            Vector3.zero, new Vector3(0.88f, 0.88f, 1), 3);
        // 水面（内）
        Sub(root, VampireSurvivorsMini.CircleSprite, new Color(0.14f, 0.28f, 0.56f),
            Vector3.zero, new Vector3(0.50f, 0.50f, 1), 4);
    }

    // ── ヘルパー ──────────────────────────────
    void Sub(GameObject parent, Sprite sprite, Color color,
             Vector3 localPos, Vector3 localScale, int order)
    {
        var go = new GameObject();
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = localScale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.color        = color;
        sr.sortingOrder = order;
    }
}
