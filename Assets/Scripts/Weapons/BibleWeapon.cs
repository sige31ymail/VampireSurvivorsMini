using UnityEngine;
using System.Collections.Generic;

/// <summary>聖書：プレイヤーの周囲を回転。Lvで冊数と回転速度UP。</summary>
public class BibleWeapon : Weapon
{
    public override string Name => "聖書";
    public override string Description => "周囲を回転して敵を攻撃（冊数・速度UP）";

    readonly List<Transform> bibles = new List<Transform>();
    readonly Dictionary<Enemy, float> hitCooldowns = new Dictionary<Enemy, float>();

    float angle;

    int Count => 1 + level;
    float RotationSpeed => 180f + level * 30f;
    float Radius => 1.8f;
    int Damage => 12 + level * 3;
    const float HitCooldown = 0.5f;

    public override void Tick(Player player, float dt)
    {
        // 聖書の数を調整
        while (bibles.Count < Count)
        {
            var go = new GameObject("Bible");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.SquareSprite;
            sr.color = new Color(1f, 0.95f, 0.7f);
            sr.sortingOrder = 6;
            go.transform.localScale = Vector3.one * 0.3f;
            bibles.Add(go.transform);
        }

        // 回転更新
        angle += RotationSpeed * dt;

        for (int i = 0; i < bibles.Count; i++)
        {
            if (i >= Count)
            {
                bibles[i].gameObject.SetActive(false);
                continue;
            }

            bibles[i].gameObject.SetActive(true);
            float a = angle + (360f / Count) * i;
            float rad = a * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * Radius;
            bibles[i].position = player.transform.position + offset;
            bibles[i].rotation = Quaternion.Euler(0, 0, a * 2f);
        }

        // クールダウン更新
        var toRemove = new List<Enemy>();
        foreach (var kvp in hitCooldowns)
        {
            if (kvp.Key == null) toRemove.Add(kvp.Key);
        }
        foreach (var e in toRemove) hitCooldowns.Remove(e);

        var keys = new List<Enemy>(hitCooldowns.Keys);
        foreach (var key in keys)
        {
            hitCooldowns[key] -= dt;
            if (hitCooldowns[key] <= 0f) hitCooldowns.Remove(key);
        }

        // 当たり判定
        foreach (var bible in bibles)
        {
            if (!bible.gameObject.activeSelf) continue;

            foreach (var e in GameState.Enemies)
            {
                if (e == null) continue;
                if (hitCooldowns.ContainsKey(e)) continue;

                if ((e.transform.position - bible.position).sqrMagnitude < 0.5f * 0.5f)
                {
                    e.TakeDamage(player.RollDamage(Damage));
                    hitCooldowns[e] = HitCooldown;
                }
            }
        }
    }
}
