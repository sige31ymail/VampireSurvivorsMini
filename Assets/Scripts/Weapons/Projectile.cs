using UnityEngine;

/// <summary>弾：直進して敵に当たるとダメージ</summary>
public class Projectile : MonoBehaviour
{
    Vector3 direction;
    int damage;
    const float Speed = 10f;
    const float Lifetime = 2f;
    float life;

    public static void Spawn(Vector3 pos, Vector3 dir, int damage)
        => Spawn(pos, dir, damage, new Color(1f, 0.95f, 0.4f), 0.20f);

    public static void Spawn(Vector3 pos, Vector3 dir, int damage, Color color, float scale)
    {
        var go = new GameObject("Projectile");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.CircleSprite;
        sr.color = color;
        sr.sortingOrder = 8;

        var p = go.AddComponent<Projectile>();
        p.direction = dir.normalized;
        p.damage = damage;
    }

    void Update()
    {
        transform.position += direction * Speed * Time.deltaTime;

        life += Time.deltaTime;
        if (life > Lifetime) { Destroy(gameObject); return; }

        // 命中判定（最初に当たった敵1体にダメージ）
        foreach (var e in GameState.Enemies)
        {
            if (e == null) continue;
            if ((e.transform.position - transform.position).sqrMagnitude < 0.35f * 0.35f)
            {
                e.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
    }
}
