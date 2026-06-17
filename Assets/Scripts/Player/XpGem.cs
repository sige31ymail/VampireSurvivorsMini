using UnityEngine;

/// <summary>経験値ジェム：近づくと吸い寄せられる（オブジェクトプール対応）</summary>
public class XpGem : MonoBehaviour, IPoolable
{
    int value;
    Player player;
    const float AttractSpeed = 8f;

    SpriteRenderer sr;

    public static void Spawn(Vector3 pos, int value)
    {
        XpGem gem;

        // オブジェクトプールから取得（利用可能な場合）
        if (ObjectPool.Instance != null)
        {
            gem = ObjectPool.Instance.Get<XpGem>(go => SetupGem(go));
        }
        else
        {
            // フォールバック：従来の直接生成
            var go = new GameObject("XpGem");
            SetupGem(go);
            gem = go.AddComponent<XpGem>();
        }

        gem.Initialize(pos, value);
    }

    static void SetupGem(GameObject go)
    {
        go.transform.localScale = Vector3.one * 0.25f;

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.CircleSprite;
            sr.color = new Color(0.4f, 1f, 0.5f);
            sr.sortingOrder = 3;
        }
    }

    void Initialize(Vector3 pos, int val)
    {
        transform.position = pos;
        value = val;
        player = Object.FindObjectOfType<Player>();

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
    }

    public void OnSpawn()
    {
        value = 0;
        player = null;
    }

    public void OnDespawn()
    {
        value = 0;
        player = null;
    }

    void Update()
    {
        if (player == null || GameState.GameOver) return;

        Vector3 toPlayer = player.transform.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist < player.magnetRange)
            transform.position += toPlayer.normalized * AttractSpeed * Time.deltaTime;

        if (dist < 0.5f)
        {
            AudioManager.PlayXp();
            player.GainXp(value);
            ReturnToPool();
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
