using UnityEngine;

/// <summary>撃破時に敵の色の破片が飛び散るパーティクル（オブジェクトプール対応）</summary>
public class DeathParticle : MonoBehaviour, IPoolable
{
    SpriteRenderer sr;
    Vector3 velocity;
    float life;
    Color baseColor;
    Vector3 baseScale;
    const float Lifetime = 0.45f;

    /// <summary>scale: 敵の大きさに応じた倍率（標準=1）</summary>
    public static void Burst(Vector3 pos, Color color, float scale)
    {
        int count = Random.Range(6, 9);
        for (int i = 0; i < count; i++)
        {
            DeathParticle particle;

            // オブジェクトプールから取得（利用可能な場合）
            if (ObjectPool.Instance != null)
            {
                particle = ObjectPool.Instance.Get<DeathParticle>(go => SetupParticle(go));
            }
            else
            {
                // フォールバック：従来の直接生成
                var go = new GameObject("Particle");
                SetupParticle(go);
                particle = go.AddComponent<DeathParticle>();
            }

            particle.Initialize(pos, color, scale);
        }
    }

    static void SetupParticle(GameObject go)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.SquareSprite;
            sr.sortingOrder = 7;
        }
    }

    void Initialize(Vector3 pos, Color color, float scale)
    {
        transform.position = pos;
        float particleScale = Random.Range(0.1f, 0.18f) * scale;
        transform.localScale = Vector3.one * particleScale;
        baseScale = transform.localScale;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 90f));

        life = 0f;
        baseColor = color;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float speed = Random.Range(2f, 5f);
        velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * speed;

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
        velocity = Vector3.zero;
    }

    public void OnDespawn()
    {
        velocity = Vector3.zero;
    }

    void Update()
    {
        life += Time.deltaTime;
        if (life >= Lifetime)
        {
            ReturnToPool();
            return;
        }

        velocity *= Mathf.Max(0f, 1f - 5f * Time.deltaTime);             // 減速
        transform.position += velocity * Time.deltaTime;
        transform.localScale = baseScale * Mathf.Max(0f, 1f - life / Lifetime); // 縮小

        if (sr != null)
        {
            var c = baseColor;
            c.a = 1f - life / Lifetime;
            sr.color = c;
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
