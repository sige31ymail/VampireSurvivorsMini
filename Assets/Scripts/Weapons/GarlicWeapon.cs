using UnityEngine;

/// <summary>ニンニク：周囲の敵を押し返しつつダメージ。Lvで範囲とノックバックUP。</summary>
public class GarlicWeapon : Weapon
{
    public override string Name => "ニンニク";
    public override string Description => "敵を押し返す防御オーラ（範囲・押し返しUP）";

    Transform visual;
    float pulseTimer;
    float tickTimer;

    float Radius => 1.5f + level * 0.3f;
    int Damage => 5 + level * 2;
    float Knockback => 0.4f + level * 0.1f;
    const float TickInterval = 0.3f;
    const float PulseInterval = 0.8f;

    public override void Tick(Player player, float dt)
    {
        // 範囲を示す半透明の円
        if (visual == null)
        {
            var go = new GameObject("GarlicAura");
            go.transform.SetParent(player.transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.CircleSprite;
            sr.color = new Color(0.9f, 0.95f, 0.8f, 0.12f);
            sr.sortingOrder = 1;
            visual = go.transform;
        }

        // パルスエフェクト
        pulseTimer += dt;
        float pulse = 1f + Mathf.Sin(pulseTimer * 8f) * 0.05f;
        float diameter = Radius * 2f / player.transform.localScale.x * pulse;
        visual.localScale = Vector3.one * diameter;

        // ダメージ＆ノックバック
        tickTimer -= dt;
        if (tickTimer <= 0f)
        {
            tickTimer = TickInterval;
            GameState.DamageEnemiesWithin(player.transform.position, Radius,
                player.RollDamage(Damage), Knockback);
        }
    }
}
