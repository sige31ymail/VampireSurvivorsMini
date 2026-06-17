using UnityEngine;

/// <summary>ナイフ：移動方向へ高速連射。Lvで弾数と連射UP。</summary>
public class KnifeWeapon : Weapon
{
    public override string Name => "ナイフ";
    public override string Description => "移動方向へ高速ナイフ投擲（連射・弾数UP）";

    float fireTimer;
    Vector3 lastDir = Vector3.right;

    float Interval => 0.35f * Mathf.Pow(0.9f, level - 1);
    int Count => 1 + level / 2;
    int Damage => 8 + level * 2;

    public override void Tick(Player player, float dt)
    {
        // 移動方向を記録
        Vector3 velocity = player.transform.position - lastPos;
        if (velocity.sqrMagnitude > 0.001f)
            lastDir = velocity.normalized;
        lastPos = player.transform.position;

        fireTimer -= dt;
        if (fireTimer > 0f) return;

        fireTimer = Interval;

        for (int i = 0; i < Count; i++)
        {
            float delay = i * 0.05f;
            Vector3 pos = player.transform.position + lastDir * (i * 0.15f);
            Projectile.Spawn(pos, lastDir, player.RollDamage(Damage),
                new Color(0.8f, 0.8f, 0.9f), 0.15f);
        }
    }

    Vector3 lastPos;
}
