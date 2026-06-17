using UnityEngine;
using System.Collections.Generic;

/// <summary>ブーメラン：投げると戻ってくる貫通武器。Lvで本数と速度UP。</summary>
public class BoomerangWeapon : Weapon
{
    public override string Name => "ブーメラン";
    public override string Description => "投げると戻る貫通武器（本数・速度UP）";

    float fireTimer;

    float Interval => 1.8f * Mathf.Pow(0.88f, level - 1);
    int Count => 1 + (level - 1) / 2;
    int Damage => 15 + level * 4;
    float Speed => 8f + level * 1f;

    public override void Tick(Player player, float dt)
    {
        fireTimer -= dt;
        if (fireTimer > 0f) return;

        var target = GameState.FindNearest(player.transform.position);
        if (target == null) return;

        fireTimer = Interval;
        Vector3 baseDir = (target.transform.position - player.transform.position).normalized;

        for (int i = 0; i < Count; i++)
        {
            float offset = (i - (Count - 1) / 2f) * 30f;
            var dir = Quaternion.Euler(0, 0, offset) * baseDir;
            BoomerangProjectile.Spawn(player.transform.position, dir, player,
                player.RollDamage(Damage), Speed);
        }
    }
}

/// <summary>ブーメランの弾：プレイヤーに戻る</summary>
public class BoomerangProjectile : MonoBehaviour, IPoolable
{
    int damage;
    float speed;
    Vector3 direction;
    Player owner;
    float life;
    bool returning;

    const float OutwardTime = 0.6f;
    const float Lifetime = 3f;

    SpriteRenderer sr;
    readonly Dictionary<Enemy, float> hitCooldowns = new Dictionary<Enemy, float>();

    public static void Spawn(Vector3 pos, Vector3 dir, Player owner, int damage, float speed)
    {
        BoomerangProjectile boom;

        if (ObjectPool.Instance != null)
        {
            boom = ObjectPool.Instance.Get<BoomerangProjectile>(go => SetupBoomerang(go));
        }
        else
        {
            var go = new GameObject("Boomerang");
            SetupBoomerang(go);
            boom = go.AddComponent<BoomerangProjectile>();
        }

        boom.Initialize(pos, dir, owner, damage, speed);
    }

    static void SetupBoomerang(GameObject go)
    {
        go.transform.localScale = Vector3.one * 0.28f;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.SquareSprite;
            sr.color = new Color(0.6f, 0.5f, 0.3f);
            sr.sortingOrder = 8;
        }
    }

    void Initialize(Vector3 pos, Vector3 dir, Player p, int dmg, float spd)
    {
        transform.position = pos;
        direction = dir.normalized;
        owner = p;
        damage = dmg;
        speed = spd;
        life = 0f;
        returning = false;
        hitCooldowns.Clear();

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
    }

    public void OnSpawn()
    {
        life = 0f;
        returning = false;
        hitCooldowns.Clear();
    }
    public void OnDespawn() { hitCooldowns.Clear(); }

    void Update()
    {
        life += Time.deltaTime;

        // 回転
        transform.Rotate(0, 0, 1200f * Time.deltaTime);

        // 移動
        if (!returning && life > OutwardTime)
        {
            returning = true;
        }

        if (returning && owner != null)
        {
            direction = (owner.transform.position - transform.position).normalized;
            transform.position += direction * speed * 1.2f * Time.deltaTime;

            // プレイヤーに戻ったら消える
            if ((owner.transform.position - transform.position).sqrMagnitude < 0.5f * 0.5f)
            {
                ReturnToPool();
                return;
            }
        }
        else
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        if (life > Lifetime) { ReturnToPool(); return; }

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
            hitCooldowns[key] -= Time.deltaTime;
            if (hitCooldowns[key] <= 0f) hitCooldowns.Remove(key);
        }

        // 当たり判定（貫通・再ヒット可能）
        foreach (var e in GameState.Enemies)
        {
            if (e == null) continue;
            if (hitCooldowns.ContainsKey(e)) continue;

            if ((e.transform.position - transform.position).sqrMagnitude < 0.45f * 0.45f)
            {
                e.TakeDamage(damage);
                hitCooldowns[e] = 0.3f;
            }
        }
    }

    void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(this);
        else
            Destroy(gameObject);
    }
}
