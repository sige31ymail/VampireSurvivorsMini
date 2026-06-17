using UnityEngine;

/// <summary>雷：ランダムな敵に落雷。Lvで同時落雷数と威力UP。</summary>
public class LightningWeapon : Weapon
{
    public override string Name => "サンダー";
    public override string Description => "ランダムな敵に落雷（同時数・威力UP）";

    float fireTimer;

    float Interval => 1.5f * Mathf.Pow(0.88f, level - 1);
    int Count => 1 + (level - 1) / 2;
    int Damage => 30 + level * 10;
    float Radius => 0.8f + level * 0.1f;

    public override void Tick(Player player, float dt)
    {
        fireTimer -= dt;
        if (fireTimer > 0f) return;

        if (GameState.Enemies.Count == 0) return;

        fireTimer = Interval;

        // ランダムな敵を選択して落雷
        for (int i = 0; i < Count; i++)
        {
            if (GameState.Enemies.Count == 0) break;

            int idx = Random.Range(0, GameState.Enemies.Count);
            var target = GameState.Enemies[idx];
            if (target == null) continue;

            // 範囲ダメージ
            GameState.DamageEnemiesWithin(target.transform.position, Radius,
                player.RollDamage(Damage), 0.1f);

            // 視覚エフェクト
            LightningEffect.Spawn(target.transform.position, Radius);
        }
    }
}

/// <summary>雷の視覚エフェクト</summary>
public class LightningEffect : MonoBehaviour
{
    float life;
    const float Lifetime = 0.2f;
    SpriteRenderer sr;
    Vector3 baseScale;

    public static void Spawn(Vector3 pos, float radius)
    {
        var go = new GameObject("LightningEffect");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * radius * 2f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.CircleSprite;
        sr.color = new Color(0.8f, 0.9f, 1f, 0.9f);
        sr.sortingOrder = 10;

        var effect = go.AddComponent<LightningEffect>();
        effect.sr = sr;
        effect.baseScale = go.transform.localScale;

        // 追加の稲妻ライン
        CreateBolt(pos, radius);
    }

    static void CreateBolt(Vector3 pos, float radius)
    {
        var bolt = new GameObject("Bolt");
        bolt.transform.position = pos + Vector3.up * 3f;
        bolt.transform.localScale = new Vector3(0.15f, 6f, 1f);

        var sr = bolt.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.SquareSprite;
        sr.color = new Color(1f, 1f, 0.7f, 1f);
        sr.sortingOrder = 11;

        Destroy(bolt, 0.1f);
    }

    void Update()
    {
        life += Time.deltaTime;
        if (life >= Lifetime) { Destroy(gameObject); return; }

        float t = life / Lifetime;
        transform.localScale = baseScale * (1f + t * 0.5f);

        var c = sr.color;
        c.a = (1f - t) * 0.9f;
        sr.color = c;
    }
}
