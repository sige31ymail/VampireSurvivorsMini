using UnityEngine;

/// <summary>
/// ゴールドコイン
/// 敵を倒すとドロップし、プレイヤーが回収するとゴールドを獲得
/// </summary>
public class GoldCoin : MonoBehaviour, IPoolable
{
    int value;
    Player player;
    const float AttractSpeed = 10f;
    const float MagnetRange = 3.5f; // XpGemより広い吸引範囲

    SpriteRenderer sr;
    float spawnTime;
    bool collected;

    // 現在のランで獲得したゴールド（ゲーム終了時にまとめて付与）
    public static int SessionGold { get; private set; }

    public static void ResetSessionGold() => SessionGold = 0;

    public static void Spawn(Vector3 pos, int value)
    {
        GoldCoin coin;

        if (ObjectPool.Instance != null)
        {
            coin = ObjectPool.Instance.Get<GoldCoin>(go => SetupCoin(go));
        }
        else
        {
            var go = new GameObject("GoldCoin");
            SetupCoin(go);
            coin = go.AddComponent<GoldCoin>();
        }

        coin.Initialize(pos, value);
    }

    static void SetupCoin(GameObject go)
    {
        go.transform.localScale = Vector3.one * 0.22f;

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VampireSurvivorsMini.CircleSprite;
            sr.color = new Color(1f, 0.85f, 0.2f); // 金色
            sr.sortingOrder = 4; // XpGemより上
        }
    }

    void Initialize(Vector3 pos, int val)
    {
        transform.position = pos;
        value = val;
        player = Object.FindObjectOfType<Player>();
        spawnTime = Time.time;
        collected = false;

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
    }

    public void OnSpawn()
    {
        value = 0;
        player = null;
        collected = false;
    }

    public void OnDespawn()
    {
        value = 0;
        player = null;
    }

    void Update()
    {
        if (player == null || GameState.GameOver || collected) return;

        // スポーン直後は少し待機（演出）
        float age = Time.time - spawnTime;
        if (age < 0.15f) return;

        Vector3 toPlayer = player.transform.position - transform.position;
        float dist = toPlayer.magnitude;

        // プレイヤーの磁石範囲 + コインの固定範囲
        float effectiveMagnet = MagnetRange + (player.magnetRange - 2.5f) * 0.5f;

        if (dist < effectiveMagnet)
        {
            // 近づくほど速く吸引
            float speedMult = Mathf.Lerp(1f, 2f, 1f - (dist / effectiveMagnet));
            transform.position += toPlayer.normalized * AttractSpeed * speedMult * Time.deltaTime;
        }

        if (dist < 0.5f)
        {
            Collect();
        }
    }

    void Collect()
    {
        if (collected) return;
        collected = true;

        // セッション内ゴールドに加算
        SessionGold += value;

        // 回収SE（XpとSEを共用、または専用SE追加可能）
        AudioManager.PlayXp();

        ReturnToPool();
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
