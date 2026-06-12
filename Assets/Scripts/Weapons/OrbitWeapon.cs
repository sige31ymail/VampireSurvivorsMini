using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>オービットオーブ：周囲を回転する球。Lvで個数と威力UP。</summary>
public class OrbitWeapon : Weapon
{
    public override string Name => "オービットオーブ";
    public override string Description => "周囲を回転する球（個数・威力UP）";

    readonly List<Transform> orbs = new List<Transform>();
    readonly Dictionary<Enemy, float> lastHitTime = new Dictionary<Enemy, float>();
    float angle;

    int OrbCount => 1 + level;        // Lv1:2個 〜 Lv5:6個
    float Radius => 1.6f;
    float RotateSpeed => 140f + level * 15f; // 度/秒
    int Damage => 8 + level * 3;
    const float HitRadius = 0.55f;    // オーブの当たり判定半径
    const float HitCooldown = 0.35f;  // 同じ敵への再ヒット間隔
    const float Knockback = 0.3f;     // ヒット時の押し出し距離

    public override void Tick(Player player, float dt)
    {
        // レベルに応じてオーブ数を同期
        while (orbs.Count < OrbCount) orbs.Add(CreateOrb());

        // 回転して配置
        angle += RotateSpeed * dt;
        for (int i = 0; i < orbs.Count; i++)
        {
            float a = (angle + i * 360f / orbs.Count) * Mathf.Deg2Rad;
            var offset = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * Radius;
            orbs[i].position = player.transform.position + offset;
        }

        // 連続接触判定：オーブに触れている敵へ、敵ごとのクールダウン付きでヒット
        float sqrHit = HitRadius * HitRadius;
        var snapshot = GameState.Enemies.ToArray();
        foreach (var e in snapshot)
        {
            if (e == null) continue;
            if (lastHitTime.TryGetValue(e, out float t) && Time.time - t < HitCooldown)
                continue;

            foreach (var orb in orbs)
            {
                Vector3 diff = e.transform.position - orb.position;
                if (diff.sqrMagnitude <= sqrHit)
                {
                    lastHitTime[e] = Time.time;
                    // プレイヤーから外向きに押し出す
                    Vector3 outward = (e.transform.position - player.transform.position).normalized;
                    e.TakeDamage(Damage, outward * Knockback);
                    break;
                }
            }
        }

        // 破棄済みの敵をクールダウン辞書から定期的に掃除
        if (lastHitTime.Count > 64)
        {
            var deadKeys = lastHitTime.Keys.Where(k => k == null).ToList();
            foreach (var k in deadKeys) lastHitTime.Remove(k);
        }
    }

    Transform CreateOrb()
    {
        var go = new GameObject("Orb");
        go.transform.localScale = Vector3.one * 0.35f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.CircleSprite;
        sr.color = new Color(0.8f, 0.5f, 1f);
        sr.sortingOrder = 9;
        return go.transform;
    }
}
