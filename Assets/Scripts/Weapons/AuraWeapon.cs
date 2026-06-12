using UnityEngine;

/// <summary>ダメージオーラ：周囲の敵に継続ダメージ。Lvで範囲と威力UP。</summary>
public class AuraWeapon : Weapon
{
    public override string Name => "ダメージオーラ";
    public override string Description => "周囲の敵に継続ダメージ（範囲・威力UP）";

    Transform visual;
    float tickTimer;

    float Radius => 1.3f + level * 0.35f;
    int Damage => 8 + level * 3;
    const float TickInterval = 0.4f;

    public override void Tick(Player player, float dt)
    {
        // 範囲を示す半透明の円（初回に生成、プレイヤーの子にする）
        if (visual == null)
        {
            var go = new GameObject("Aura");
            go.transform.SetParent(player.transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.CircleSprite;
            sr.color = new Color(0.4f, 1f, 0.5f, 0.15f);
            sr.sortingOrder = 1;
            visual = go.transform;
        }
        // 親(Player)のスケール0.6を打ち消しつつ直径に合わせる
        float diameter = Radius * 2f / player.transform.localScale.x;
        visual.localScale = Vector3.one * diameter;

        tickTimer -= dt;
        if (tickTimer <= 0f)
        {
            tickTimer = TickInterval;
            // 軽いノックバック付きで範囲ダメージ（ニンニク風の押し返し）
            GameState.DamageEnemiesWithin(player.transform.position, Radius, Damage, 0.15f);
        }
    }
}
