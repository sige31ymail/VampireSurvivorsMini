using UnityEngine;

/// <summary>マジックボルト：最も近い敵へ弾を発射。Lvで弾数と連射速度UP。</summary>
public class BoltWeapon : Weapon
{
    public override string Name => "マジックボルト";
    public override string Description => "最も近い敵へ弾を発射（弾数・連射UP）";

    float fireTimer;

    float Interval => 0.8f * Mathf.Pow(0.88f, level - 1); // Lvごとに12%短縮
    int Count => 1 + (level - 1) / 2;                     // Lv1:1発, Lv3:2発, Lv5:3発
    int Damage => 10 + level * 2;

    public override void Tick(Player player, float dt)
    {
        fireTimer -= dt;
        if (fireTimer > 0f) return;

        var target = GameState.FindNearest(player.transform.position);
        if (target == null) return;

        fireTimer = Interval;
        Vector3 baseDir = (target.transform.position - player.transform.position).normalized;

        const float spread = 15f; // 複数発射時の扇角（度）
        for (int i = 0; i < Count; i++)
        {
            float offset = (i - (Count - 1) / 2f) * spread;
            var dir = Quaternion.Euler(0, 0, offset) * baseDir;
            Projectile.Spawn(player.transform.position, dir, player.RollDamage(Damage));
        }
    }
}
