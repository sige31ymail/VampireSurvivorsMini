using UnityEngine;
using System.Collections.Generic;

/// <summary>敵タイプごとの見た目を子オブジェクトで構成する</summary>
public class EnemyVisuals : MonoBehaviour
{
    List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    List<Color> originalColors = new List<Color>();
    float flashTimer;
    const float FlashDuration = 0.08f;

    public void Build(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Chaser:  BuildChaser();  break;
            case EnemyType.Runner:  BuildRunner();  break;
            case EnemyType.Tank:    BuildTank();    break;
            case EnemyType.Dasher:  BuildDasher();  break;
            case EnemyType.Boss:    BuildBoss();    break;
        }
    }

    // ── Chaser：コウモリ ──────────────────────
    void BuildChaser()
    {
        // 翼（左右の回転した暗赤の四角）
        PartR("WingL", VampireSurvivorsMini.SquareSprite, new Color(0.65f, 0.12f, 0.12f),
              new Vector3(-0.55f, 0.08f, 0), Quaternion.Euler(0, 0,  28f), new Vector3(0.55f, 0.28f, 1), 4);
        PartR("WingR", VampireSurvivorsMini.SquareSprite, new Color(0.65f, 0.12f, 0.12f),
              new Vector3( 0.55f, 0.08f, 0), Quaternion.Euler(0, 0, -28f), new Vector3(0.55f, 0.28f, 1), 4);
        // 黄色い目
        Part("EyeL", VampireSurvivorsMini.CircleSprite, new Color(1f, 0.88f, 0.15f),
             new Vector3(-0.20f, 0.18f, 0), new Vector3(0.22f, 0.22f, 1), 6);
        Part("EyeR", VampireSurvivorsMini.CircleSprite, new Color(1f, 0.88f, 0.15f),
             new Vector3( 0.20f, 0.18f, 0), new Vector3(0.22f, 0.22f, 1), 6);
    }

    // ── Runner：スライム ─────────────────────
    void BuildRunner()
    {
        // 大きな一つ目（白目 + 黒瞳）
        Part("Eye",   VampireSurvivorsMini.CircleSprite, Color.white,
             new Vector3(0f, 0.18f, 0), new Vector3(0.55f, 0.55f, 1), 6);
        Part("Pupil", VampireSurvivorsMini.CircleSprite, new Color(0.08f, 0.04f, 0.18f),
             new Vector3(0.08f, 0.22f, 0), new Vector3(0.28f, 0.28f, 1), 7);
        // 3本の足
        Part("Leg0", VampireSurvivorsMini.SquareSprite, new Color(0.88f, 0.62f, 0.10f),
             new Vector3(-0.26f, -0.55f, 0), new Vector3(0.18f, 0.30f, 1), 6);
        Part("Leg1", VampireSurvivorsMini.SquareSprite, new Color(0.88f, 0.62f, 0.10f),
             new Vector3(  0f,  -0.60f, 0), new Vector3(0.18f, 0.30f, 1), 6);
        Part("Leg2", VampireSurvivorsMini.SquareSprite, new Color(0.88f, 0.62f, 0.10f),
             new Vector3( 0.26f, -0.55f, 0), new Vector3(0.18f, 0.30f, 1), 6);
    }

    // ── Tank：ゴーレム ────────────────────────
    void BuildTank()
    {
        // 胸のアーマープレート
        Part("Chest", VampireSurvivorsMini.SquareSprite, new Color(0.62f, 0.22f, 0.22f),
             new Vector3(0f, 0.05f, 0), new Vector3(0.55f, 0.50f, 1), 6);
        // 怒りの眉目（細い黄色の横線）
        Part("EyeL", VampireSurvivorsMini.SquareSprite, new Color(1f, 0.70f, 0.10f),
             new Vector3(-0.20f, 0.28f, 0), new Vector3(0.22f, 0.08f, 1), 7);
        Part("EyeR", VampireSurvivorsMini.SquareSprite, new Color(1f, 0.70f, 0.10f),
             new Vector3( 0.20f, 0.28f, 0), new Vector3(0.22f, 0.08f, 1), 7);
        // 肩のボルト
        Part("BoltL", VampireSurvivorsMini.SquareSprite, new Color(0.40f, 0.40f, 0.45f),
             new Vector3(-0.52f, 0.18f, 0), new Vector3(0.18f, 0.22f, 1), 4);
        Part("BoltR", VampireSurvivorsMini.SquareSprite, new Color(0.40f, 0.40f, 0.45f),
             new Vector3( 0.52f, 0.18f, 0), new Vector3(0.18f, 0.22f, 1), 4);
    }

    // ── Dasher：ゴースト ─────────────────────
    void BuildDasher()
    {
        // 紫の瞳（円はひし形の親回転を受けても円のまま）
        Part("EyeL", VampireSurvivorsMini.CircleSprite, new Color(0.7f, 0.1f, 1f),
             new Vector3(-0.16f, 0.16f, 0), new Vector3(0.26f, 0.26f, 1), 6);
        Part("EyeR", VampireSurvivorsMini.CircleSprite, new Color(0.7f, 0.1f, 1f),
             new Vector3( 0.16f, 0.16f, 0), new Vector3(0.26f, 0.26f, 1), 6);
    }

    // ── Boss：デーモン ───────────────────────
    void BuildBoss()
    {
        // マント（後ろ側、sortingOrderを低く）
        Part("Cape", VampireSurvivorsMini.SquareSprite, new Color(0.18f, 0.04f, 0.32f),
             new Vector3(0f, -0.05f, 0), new Vector3(1.15f, 0.90f, 1), 3);
        // 角（左右、少し傾ける）
        PartR("HornL", VampireSurvivorsMini.SquareSprite, new Color(0.52f, 0.10f, 0.78f),
              new Vector3(-0.38f, 0.55f, 0), Quaternion.Euler(0, 0,  18f), new Vector3(0.17f, 0.45f, 1), 6);
        PartR("HornR", VampireSurvivorsMini.SquareSprite, new Color(0.52f, 0.10f, 0.78f),
              new Vector3( 0.38f, 0.55f, 0), Quaternion.Euler(0, 0, -18f), new Vector3(0.17f, 0.45f, 1), 6);
        // 赤い目
        Part("EyeL", VampireSurvivorsMini.CircleSprite, new Color(1f, 0.12f, 0.12f),
             new Vector3(-0.22f, 0.10f, 0), new Vector3(0.22f, 0.22f, 1), 7);
        Part("EyeR", VampireSurvivorsMini.CircleSprite, new Color(1f, 0.12f, 0.12f),
             new Vector3( 0.22f, 0.10f, 0), new Vector3(0.22f, 0.22f, 1), 7);
        // 金の王冠（3本のとがり）
        Part("CrownL", VampireSurvivorsMini.SquareSprite, new Color(1f, 0.80f, 0.10f),
             new Vector3(-0.30f, 0.62f, 0), new Vector3(0.11f, 0.24f, 1), 6);
        Part("CrownC", VampireSurvivorsMini.SquareSprite, new Color(1f, 0.80f, 0.10f),
             new Vector3(  0f,   0.68f, 0), new Vector3(0.11f, 0.34f, 1), 6);
        Part("CrownR", VampireSurvivorsMini.SquareSprite, new Color(1f, 0.80f, 0.10f),
             new Vector3( 0.30f, 0.62f, 0), new Vector3(0.11f, 0.24f, 1), 6);
    }

    // ── ヘルパー ──────────────────────────────
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

    void PartR(string name, Sprite sprite, Color color,
               Vector3 localPos, Quaternion localRot, Vector3 localScale, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale    = localScale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.color        = color;
        sr.sortingOrder = order;
    }

    /// <summary>ビルド後にスプライトレンダラーをキャッシュ</summary>
    public void CacheRenderers()
    {
        spriteRenderers.Clear();
        originalColors.Clear();

        var parentSr = GetComponent<SpriteRenderer>();
        if (parentSr != null)
        {
            spriteRenderers.Add(parentSr);
            originalColors.Add(parentSr.color);
        }

        foreach (Transform child in transform)
        {
            var sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                spriteRenderers.Add(sr);
                originalColors.Add(sr.color);
            }
        }
    }

    /// <summary>ヒットフラッシュを開始</summary>
    public void Flash()
    {
        if (spriteRenderers.Count == 0) CacheRenderers();
        flashTimer = FlashDuration;

        foreach (var sr in spriteRenderers)
        {
            if (sr != null) sr.color = Color.white;
        }
    }

    void Update()
    {
        if (flashTimer <= 0) return;

        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0)
        {
            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                if (spriteRenderers[i] != null)
                    spriteRenderers[i].color = originalColors[i];
            }
        }
    }

    /// <summary>リセット時にフラッシュ状態もリセット</summary>
    public void ResetFlash()
    {
        flashTimer = 0;
        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = originalColors[i];
        }
    }
}
