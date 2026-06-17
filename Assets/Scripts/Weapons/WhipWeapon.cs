using UnityEngine;
using System.Collections.Generic;

/// <summary>ムチ：横方向に広範囲攻撃。Lvで範囲と威力UP。</summary>
public class WhipWeapon : Weapon
{
    public override string Name => "ムチ";
    public override string Description => "横方向に広範囲攻撃（範囲・威力UP）";

    float fireTimer;
    bool attackRight = true;

    float Interval => 1.1f * Mathf.Pow(0.9f, level - 1);
    float Range => 2.5f + level * 0.5f;
    float Width => 1.0f + level * 0.2f;
    int Damage => 15 + level * 5;

    public override void Tick(Player player, float dt)
    {
        fireTimer -= dt;
        if (fireTimer > 0f) return;

        fireTimer = Interval;

        // 左右交互に攻撃
        float dir = attackRight ? 1f : -1f;
        attackRight = !attackRight;

        Vector3 center = player.transform.position + Vector3.right * dir * (Range / 2f);

        // 範囲内の敵にダメージ
        var hitEnemies = new List<Enemy>();
        foreach (var e in GameState.Enemies)
        {
            if (e == null) continue;
            Vector3 diff = e.transform.position - player.transform.position;

            // 横方向の範囲チェック
            bool inRangeX = (dir > 0) ? (diff.x > 0 && diff.x < Range) : (diff.x < 0 && diff.x > -Range);
            bool inRangeY = Mathf.Abs(diff.y) < Width;

            if (inRangeX && inRangeY)
                hitEnemies.Add(e);
        }

        foreach (var e in hitEnemies)
        {
            e.TakeDamage(player.RollDamage(Damage), Vector3.right * dir * 0.3f);
        }

        // 視覚エフェクト
        WhipEffect.Spawn(player.transform.position, dir, Range, Width);
    }
}

/// <summary>ムチの視覚エフェクト</summary>
public class WhipEffect : MonoBehaviour
{
    float life;
    const float Lifetime = 0.15f;
    SpriteRenderer sr;

    public static void Spawn(Vector3 playerPos, float dir, float range, float width)
    {
        var go = new GameObject("WhipEffect");
        go.transform.position = playerPos + Vector3.right * dir * (range / 2f);
        go.transform.localScale = new Vector3(range, width * 0.3f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.SquareSprite;
        sr.color = new Color(0.9f, 0.7f, 0.3f, 0.8f);
        sr.sortingOrder = 9;

        go.AddComponent<WhipEffect>().sr = sr;
    }

    void Update()
    {
        life += Time.deltaTime;
        if (life >= Lifetime) { Destroy(gameObject); return; }

        float alpha = 1f - (life / Lifetime);
        var c = sr.color;
        c.a = alpha * 0.8f;
        sr.color = c;
    }
}
