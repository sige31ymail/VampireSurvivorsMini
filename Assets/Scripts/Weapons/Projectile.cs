using UnityEngine;

/// <summary>弾：直進して敵に当たるとダメージ（オブジェクトプール対応）</summary>
public class Projectile : MonoBehaviour, IPoolable
{
    Vector3 direction;
    int damage;
    const float Speed = 10f;
    const float Lifetime = 2f;
    float life;

    SpriteRenderer sr;
    bool isInitialized;

    public static void Spawn(Vector3 pos, Vector3 dir, int damage)
        => Spawn(pos, dir, damage, new Color(1f, 0.95f, 0.4f), 0.20f);

    public static void Spawn(Vector3 pos, Vector3 dir, int damage, Color color, float scale)
    {
        Projectile p;

        // オブジェクトプールから取得（利用可能な場合）
        if (ObjectPool.Instance != null)
        {
            p = ObjectPool.Instance.Get<Projectile>(go => SetupProjectile(go));
        }
        else
        {
            // フォールバック：従来の直接生成
            var go = new GameObject("Projectile");
            SetupProjectile(go);
            p = go.AddComponent<Projectile>();
        }

        p.Initialize(pos, dir, damage, color, scale);
    }

    static void SetupProjectile(GameObject go)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.CircleSprite;
            sr.sortingOrder = 8;
        }
    }

    void Initialize(Vector3 pos, Vector3 dir, int dmg, Color color, float scale)
    {
        transform.position = pos;
        transform.localScale = Vector3.one * scale;
        direction = dir.normalized;
        damage = dmg;
        life = 0f;

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
            sr.enabled = true;
        }
    }

    public void OnSpawn()
    {
        life = 0f;
    }

    public void OnDespawn()
    {
        direction = Vector3.zero;
        damage = 0;
    }

    void Update()
    {
        transform.position += direction * Speed * Time.deltaTime;

        life += Time.deltaTime;
        if (life > Lifetime)
        {
            ReturnToPool();
            return;
        }

        // 命中判定（最初に当たった敵1体にダメージ）
        foreach (var e in GameState.Enemies)
        {
            if (e == null) continue;
            if ((e.transform.position - transform.position).sqrMagnitude < 0.35f * 0.35f)
            {
                e.TakeDamage(damage);
                ReturnToPool();
                return;
            }
        }
    }

    void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.Return(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
