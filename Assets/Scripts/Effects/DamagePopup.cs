using UnityEngine;

/// <summary>ダメージ数字のポップアップ（上昇しながらフェードアウト）（オブジェクトプール対応）</summary>
public class DamagePopup : MonoBehaviour, IPoolable
{
    static Font font;
    TextMesh tm;
    MeshRenderer mr;
    Vector3 velocity;
    float life;
    const float Lifetime = 0.6f;

    public static void Spawn(Vector3 pos, int damage)
    {
        // ダメージ数字の設定がOFFの場合はスポーンしない
        if (SaveSystem.Instance != null && !SaveSystem.Instance.Settings.DamageNumbers)
            return;

        if (font == null)
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        DamagePopup popup;

        // オブジェクトプールから取得（利用可能な場合）
        if (ObjectPool.Instance != null)
        {
            popup = ObjectPool.Instance.Get<DamagePopup>(go => SetupPopup(go));
        }
        else
        {
            // フォールバック：従来の直接生成
            var go = new GameObject("DamagePopup");
            SetupPopup(go);
            popup = go.AddComponent<DamagePopup>();
        }

        popup.Initialize(pos, damage);
    }

    static void SetupPopup(GameObject go)
    {
        var tm = go.GetComponent<TextMesh>();
        if (tm == null)
        {
            tm = go.AddComponent<TextMesh>();
            tm.font = font;
            tm.fontSize = 48;
            tm.characterSize = 0.06f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.fontStyle = FontStyle.Bold;

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = font.material;
                mr.sortingOrder = 20; // 最前面
            }
        }
    }

    void Initialize(Vector3 pos, int damage)
    {
        transform.position = pos + new Vector3(Random.Range(-0.2f, 0.2f), 0.3f, 0);
        life = 0f;
        velocity = new Vector3(Random.Range(-0.5f, 0.5f), 2.2f, 0);

        if (tm == null) tm = GetComponent<TextMesh>();
        if (mr == null) mr = GetComponent<MeshRenderer>();

        if (tm != null)
        {
            tm.text = damage.ToString();
            // 大ダメージは黄色で強調
            tm.color = damage >= 30 ? new Color(1f, 0.85f, 0.2f) : Color.white;
        }

        if (mr != null)
        {
            mr.enabled = true;
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

        velocity.y -= 4f * Time.deltaTime; // 重力っぽく減速しながら上昇
        transform.position += velocity * Time.deltaTime;

        if (tm != null)
        {
            var c = tm.color;
            c.a = 1f - Mathf.Clamp01(life / Lifetime);
            tm.color = c;
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
