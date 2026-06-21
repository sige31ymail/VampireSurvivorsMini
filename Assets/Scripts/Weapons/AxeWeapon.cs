using UnityEngine;

/// <summary>斧：上方に放物線を描いて飛ぶ高火力武器。Lvで本数と威力UP。</summary>
public class AxeWeapon : Weapon
{
    public override string Name => "斧";
    public override string Description => "放物線を描く高火力攻撃（本数・威力UP）";

    float fireTimer;

    float Interval => 1.2f * Mathf.Pow(0.88f, level - 1);
    int Count => 1 + (level - 1) / 2;
    int Damage => 25 + level * 8;

    public override void Tick(Player player, float dt)
    {
        fireTimer -= dt;
        if (fireTimer > 0f) return;

        fireTimer = Interval;

        for (int i = 0; i < Count; i++)
        {
            float offsetX = (i - (Count - 1) / 2f) * 0.8f;
            AxeProjectile.Spawn(player.transform.position + Vector3.right * offsetX,
                player.RollDamage(Damage));
        }
    }
}

/// <summary>斧の弾：放物線を描いて飛ぶ</summary>
public class AxeProjectile : MonoBehaviour, IPoolable
{
    int damage;
    Vector3 velocity;
    float rotation;
    float life;
    const float Lifetime = 2.5f;
    const float Gravity = -15f;

    SpriteRenderer sr;

    public static void Spawn(Vector3 pos, int damage)
    {
        AxeProjectile axe;

        if (ObjectPool.Instance != null)
        {
            axe = ObjectPool.Instance.Get<AxeProjectile>(go => SetupAxe(go));
        }
        else
        {
            var go = new GameObject("AxeProjectile");
            SetupAxe(go);
            axe = go.AddComponent<AxeProjectile>();
        }

        axe.Initialize(pos, damage);
    }

    static void SetupAxe(GameObject go)
    {
        go.transform.localScale = Vector3.one * 0.35f;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.SquareSprite;
            sr.color = new Color(0.6f, 0.4f, 0.2f);
            sr.sortingOrder = 8;
        }
    }

    void Initialize(Vector3 pos, int dmg)
    {
        transform.position = pos;
        damage = dmg;
        velocity = new Vector3(Random.Range(-2f, 2f), 12f, 0);
        rotation = 0f;
        life = 0f;

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
    }

    public void OnSpawn() { life = 0f; }
    public void OnDespawn() { }

    void Update()
    {
        life += Time.deltaTime;
        if (life > Lifetime) { ReturnToPool(); return; }

        velocity.y += Gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        rotation += 720f * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, rotation);

        // TakeDamageで敵が死ぬとGameState.Enemiesから除去されるため、スナップショットを回す
        foreach (var e in GameState.Enemies.ToArray())
        {
            if (e == null) continue;
            if ((e.transform.position - transform.position).sqrMagnitude < 0.5f * 0.5f)
            {
                e.TakeDamage(damage);
                // 斧は貫通する（破壊しない）
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
