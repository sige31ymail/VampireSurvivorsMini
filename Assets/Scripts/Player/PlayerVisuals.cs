using UnityEngine;

/// <summary>プレイヤーの見た目を子オブジェクトで構成するウィザードキャラクター</summary>
public class PlayerVisuals : MonoBehaviour
{
    void Start()
    {
        // 足元の接地シャドウ（手描き／コード生成どちらでも付ける）
        GroundShadow.Attach(transform, 0.85f, 0.26f, -0.55f);

        // 手描きイラストがあれば1枚スプライトに差し替え、パーツ組み立てはスキップ
        var art = SpriteLibrary.Get("Characters/player");
        if (art != null)
        {
            // 親スケール0.6 × localScale2.6 ≒ 高さ1.56ワールド単位（パーツ版とほぼ同サイズ）
            SpriteLibrary.Attach(transform, art, 2.6f, 11, "PlayerArt");
            return;
        }

        // ── ローブ ───────────────────────────
        Part("Body",    VampireSurvivorsMini.SquareSprite,  new Color(0.25f, 0.45f, 0.90f), new Vector3( 0.00f, -0.10f, 0), new Vector3(0.55f, 0.55f, 1), 10);
        Part("Hem",     VampireSurvivorsMini.SquareSprite,  new Color(0.20f, 0.38f, 0.80f), new Vector3( 0.00f, -0.38f, 0), new Vector3(0.65f, 0.18f, 1),  9);
        // ── ブーツ ───────────────────────────
        Part("BootL",   VampireSurvivorsMini.SquareSprite,  new Color(0.18f, 0.12f, 0.28f), new Vector3(-0.13f, -0.56f, 0), new Vector3(0.18f, 0.24f, 1),  8);
        Part("BootR",   VampireSurvivorsMini.SquareSprite,  new Color(0.18f, 0.12f, 0.28f), new Vector3( 0.13f, -0.56f, 0), new Vector3(0.18f, 0.24f, 1),  8);
        // ── 顔 ──────────────────────────────
        Part("Head",    VampireSurvivorsMini.CircleSprite,  new Color(0.98f, 0.82f, 0.64f), new Vector3( 0.00f,  0.22f, 0), new Vector3(0.48f, 0.48f, 1), 11);
        Part("EyeL",    VampireSurvivorsMini.CircleSprite,  new Color(0.12f, 0.08f, 0.22f), new Vector3(-0.12f,  0.26f, 0), new Vector3(0.12f, 0.12f, 1), 12);
        Part("EyeR",    VampireSurvivorsMini.CircleSprite,  new Color(0.12f, 0.08f, 0.22f), new Vector3( 0.12f,  0.26f, 0), new Vector3(0.12f, 0.12f, 1), 12);
        // ── 帽子 ────────────────────────────
        Part("HatBrim", VampireSurvivorsMini.SquareSprite,  new Color(0.38f, 0.10f, 0.60f), new Vector3( 0.00f,  0.40f, 0), new Vector3(0.62f, 0.10f, 1), 12);
        Part("HatBody", VampireSurvivorsMini.SquareSprite,  new Color(0.45f, 0.13f, 0.70f), new Vector3( 0.00f,  0.68f, 0), new Vector3(0.32f, 0.52f, 1), 12);
        Part("HatTip",  VampireSurvivorsMini.SquareSprite,  new Color(0.45f, 0.13f, 0.70f), new Vector3( 0.00f,  0.92f, 0), new Vector3(0.16f, 0.18f, 1), 12);
        // 帽子の金バックル
        Part("HatGem",  VampireSurvivorsMini.CircleSprite,  new Color(1.00f, 0.85f, 0.20f), new Vector3( 0.00f,  0.42f, 0), new Vector3(0.10f, 0.10f, 1), 13);
    }

    void Part(string name, Sprite sprite, Color color,
              Vector3 localPos, Vector3 localScale, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = localScale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.color        = color;
        sr.sortingOrder = order;
    }
}
