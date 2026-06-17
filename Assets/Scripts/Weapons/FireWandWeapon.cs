using UnityEngine;

/// <summary>火の杖：爆発する火球を発射。Lvで爆発範囲と威力UP。</summary>
public class FireWandWeapon : Weapon
{
    public override string Name => "火の杖";
    public override string Description => "爆発する火球を発射（爆発範囲・威力UP）";

    float fireTimer;

    float Interval => 1.4f * Mathf.Pow(0.9f, level - 1);
    int Damage => 20 + level * 8;
    float ExplosionRadius => 0.8f + level * 0.2f;

    public override void Tick(Player player, float dt)
    {
        fireTimer -= dt;
        if (fireTimer > 0f) return;

        var target = GameState.FindNearest(player.transform.position);
        if (target == null) return;

        fireTimer = Interval;
        Vector3 dir = (target.transform.position - player.transform.position).normalized;

        FireballProjectile.Spawn(player.transform.position, dir,
            player.RollDamage(Damage), ExplosionRadius);
    }
}

/// <summary>火球：着弾時に爆発</summary>
public class FireballProjectile : MonoBehaviour, IPoolable
{
    int damage;
    float explosionRadius;
    Vector3 direction;
    float life;

    const float Speed = 7f;
    const float Lifetime = 2f;

    SpriteRenderer sr;

    public static void Spawn(Vector3 pos, Vector3 dir, int damage, float radius)
    {
        FireballProjectile fb;

        if (ObjectPool.Instance != null)
        {
            fb = ObjectPool.Instance.Get<FireballProjectile>(go => SetupFireball(go));
        }
        else
        {
            var go = new GameObject("Fireball");
            SetupFireball(go);
            fb = go.AddComponent<FireballProjectile>();
        }

        fb.Initialize(pos, dir, damage, radius);
    }

    static void SetupFireball(GameObject go)
    {
        go.transform.localScale = Vector3.one * 0.35f;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.CircleSprite;
            sr.color = new Color(1f, 0.5f, 0.1f);
            sr.sortingOrder = 8;
        }
    }

    void Initialize(Vector3 pos, Vector3 dir, int dmg, float radius)
    {
        transform.position = pos;
        direction = dir.normalized;
        damage = dmg;
        explosionRadius = radius;
        life = 0f;

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
    }

    public void OnSpawn() { life = 0f; }
    public void OnDespawn() { }

    void Update()
    {
        transform.position += direction * Speed * Time.deltaTime;
        life += Time.deltaTime;

        // 明滅エフェクト
        float flicker = 0.8f + Mathf.Sin(life * 20f) * 0.2f;
        transform.localScale = Vector3.one * 0.35f * flicker;

        if (life > Lifetime) { ReturnToPool(); return; }

        // 敵との衝突で爆発
        foreach (var e in GameState.Enemies)
        {
            if (e == null) continue;
            if ((e.transform.position - transform.position).sqrMagnitude < 0.4f * 0.4f)
            {
                Explode();
                return;
            }
        }
    }

    void Explode()
    {
        // 爆発ダメージ
        GameState.DamageEnemiesWithin(transform.position, explosionRadius, damage, 0.3f);

        // 爆発エフェクト
        ExplosionEffect.Spawn(transform.position, explosionRadius);

        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(this);
        else
            Destroy(gameObject);
    }
}

/// <summary>爆発エフェクト</summary>
public class ExplosionEffect : MonoBehaviour
{
    float life;
    const float Lifetime = 0.25f;
    SpriteRenderer sr;
    Vector3 baseScale;

    public static void Spawn(Vector3 pos, float radius)
    {
        var go = new GameObject("Explosion");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * radius * 0.5f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.CircleSprite;
        sr.color = new Color(1f, 0.6f, 0.1f, 0.9f);
        sr.sortingOrder = 10;

        var effect = go.AddComponent<ExplosionEffect>();
        effect.sr = sr;
        effect.baseScale = Vector3.one * radius * 2f;
    }

    void Update()
    {
        life += Time.deltaTime;
        if (life >= Lifetime) { Destroy(gameObject); return; }

        float t = life / Lifetime;
        transform.localScale = baseScale * (0.5f + t * 0.5f);

        var c = sr.color;
        c.a = (1f - t) * 0.9f;
        c.g = 0.6f - t * 0.4f;
        sr.color = c;
    }
}
