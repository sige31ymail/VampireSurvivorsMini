using UnityEngine;

/// <summary>ダメージ数字のポップアップ（上昇しながらフェードアウト）</summary>
public class DamagePopup : MonoBehaviour
{
    static Font font;
    TextMesh tm;
    Vector3 velocity;
    float life;
    const float Lifetime = 0.6f;

    public static void Spawn(Vector3 pos, int damage)
    {
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var go = new GameObject("DamagePopup");
        go.transform.position = pos + new Vector3(Random.Range(-0.2f, 0.2f), 0.3f, 0);

        var tm = go.AddComponent<TextMesh>();
        tm.text = damage.ToString();
        tm.font = font;
        tm.fontSize = 48;
        tm.characterSize = 0.06f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.fontStyle = FontStyle.Bold;
        // 大ダメージは黄色で強調
        tm.color = damage >= 30 ? new Color(1f, 0.85f, 0.2f) : Color.white;

        var mr = go.GetComponent<MeshRenderer>();
        mr.material = font.material;
        mr.sortingOrder = 20; // 最前面

        var p = go.AddComponent<DamagePopup>();
        p.tm = tm;
        p.velocity = new Vector3(Random.Range(-0.5f, 0.5f), 2.2f, 0);
    }

    void Update()
    {
        life += Time.deltaTime;
        if (life >= Lifetime) { Destroy(gameObject); return; }

        velocity.y -= 4f * Time.deltaTime; // 重力っぽく減速しながら上昇
        transform.position += velocity * Time.deltaTime;

        var c = tm.color;
        c.a = 1f - Mathf.Clamp01(life / Lifetime);
        tm.color = c;
    }
}
