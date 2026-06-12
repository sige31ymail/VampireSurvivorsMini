using UnityEngine;

/// <summary>クロスボルト：敵を狙わず四方（Lv3〜八方）に弾を一斉発射。群衆処理向け。</summary>
public class CrossBoltWeapon : Weapon
{
    public override string Name => "クロスボルト";
    public override string Description => "四方向に弾を撒く。Lv3で八方向に拡大";

    float fireTimer;

    float Interval => 1.0f * Mathf.Pow(0.90f, level - 1); // Lvごとに10%短縮
    int   Dirs     => level >= 3 ? 8 : 4;                  // Lv1-2:4方向, Lv3+:8方向
    int   Damage   => 7 + level * 3;                        // Lv1:10, Lv5:22

    static readonly Color BoltColor = new Color(0.30f, 0.90f, 1.00f); // 水色

    public override void Tick(Player player, float dt)
    {
        fireTimer -= dt;
        if (fireTimer > 0f) return;
        fireTimer = Interval;

        int dirs = Dirs;
        for (int i = 0; i < dirs; i++)
        {
            float rad = i * (2f * Mathf.PI / dirs);
            var dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            Projectile.Spawn(player.transform.position, dir,
                player.RollDamage(Damage), BoltColor, 0.17f);
        }
    }
}
