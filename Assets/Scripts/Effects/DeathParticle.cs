using UnityEngine;

/// <summary>撃破時に敵の色の破片が飛び散るパーティクル</summary>
public class DeathParticle : MonoBehaviour
{
    SpriteRenderer sr;
    Vector3 velocity;
    float life;
    const float Lifetime = 0.45f;

    /// <summary>scale: 敵の大きさに応じた倍率（標準=1）</summary>
    public static void Burst(Vector3 pos, Color color, float scale)
    {
        int count = Random.Range(6, 9);
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Particle");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * Random.Range(0.1f, 0.18f) * scale;
            go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 90f));

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.SquareSprite;
            sr.color = color;
            sr.sortingOrder = 7;

            var p = go.AddComponent<DeathParticle>();
            p.sr = sr;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(2f, 5f);
            p.velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * speed;
        }
    }

    void Update()
    {
        life += Time.deltaTime;
        if (life >= Lifetime) { Destroy(gameObject); return; }

        velocity *= Mathf.Max(0f, 1f - 5f * Time.deltaTime);             // 減速
        transform.position += velocity * Time.deltaTime;
        transform.localScale *= Mathf.Max(0f, 1f - 2.5f * Time.deltaTime); // 縮小

        var c = sr.color;
        c.a = 1f - life / Lifetime;
        sr.color = c;
    }
}
