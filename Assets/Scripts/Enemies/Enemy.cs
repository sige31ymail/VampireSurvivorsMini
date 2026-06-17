using UnityEngine;

/// <summary>敵：タイプごとに性能と挙動が変わる</summary>
public class Enemy : MonoBehaviour
{
    public EnemyType type;
    public float moveSpeed = 2f;
    public int hp = 20;
    public int maxHp = 20;
    public int touchDamage = 10;
    public int xpValue = 1;

    Player player;
    SpriteRenderer sr;
    Color baseColor;
    float flashTimer;
    SpriteRenderer[] allRenderers;
    Color[] allBaseColors;

    // 状態タイマー（複数タイプで使用）
    float stateTimer;
    float behaviorTimer;
    int behaviorPhase;
    Vector3 moveDir;
    bool hasRevived; // スケルトン用

    // ダッシャー用
    enum DashState { Approach, Telegraph, Dash, Cooldown }
    DashState dashState = DashState.Approach;

    // アーチャー用
    float shootTimer;
    const float ShootInterval = 2f;
    const float ShootRange = 6f;

    // ゴースト用
    float dodgeChance = 0.3f;

    public static Enemy Spawn(EnemyType type, Vector3 pos, Player target, float difficulty)
    {
        var go = new GameObject("Enemy_" + type);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.SquareSprite;
        sr.sortingOrder = 5;

        var e = go.AddComponent<Enemy>();
        e.type = type;
        e.player = target;
        e.sr = sr;

        // タイプ別パラメータ
        switch (type)
        {
            case EnemyType.Chaser:
                sr.sprite = VampireSurvivorsMini.CircleSprite;
                sr.color = new Color(1f, 0.35f, 0.35f);
                go.transform.localScale = Vector3.one * 0.5f;
                e.hp = 20 + (int)(difficulty * 10f);
                e.moveSpeed = 2f + difficulty * 0.3f;
                e.touchDamage = 10;
                e.xpValue = 1;
                break;

            case EnemyType.Runner:
                sr.sprite = VampireSurvivorsMini.CircleSprite;
                sr.color = new Color(1f, 0.75f, 0.2f);
                go.transform.localScale = Vector3.one * 0.35f;
                e.hp = 8 + (int)(difficulty * 4f);
                e.moveSpeed = 4.2f + difficulty * 0.4f;
                e.touchDamage = 6;
                e.xpValue = 1;
                break;

            case EnemyType.Tank:
                sr.color = new Color(0.55f, 0.15f, 0.15f);
                go.transform.localScale = Vector3.one * 0.85f;
                e.hp = 80 + (int)(difficulty * 40f);
                e.moveSpeed = 1.1f + difficulty * 0.15f;
                e.touchDamage = 20;
                e.xpValue = 3;
                break;

            case EnemyType.Dasher:
                sr.color = new Color(1f, 0.4f, 0.85f);
                go.transform.localScale = Vector3.one * 0.45f;
                go.transform.rotation = Quaternion.Euler(0, 0, 45f);
                e.hp = 25 + (int)(difficulty * 10f);
                e.moveSpeed = 2.5f + difficulty * 0.3f;
                e.touchDamage = 15;
                e.xpValue = 2;
                break;

            case EnemyType.Bat:
                sr.sprite = VampireSurvivorsMini.CircleSprite;
                sr.color = new Color(0.3f, 0.2f, 0.4f);
                go.transform.localScale = Vector3.one * 0.3f;
                e.hp = 12 + (int)(difficulty * 5f);
                e.moveSpeed = 3.5f + difficulty * 0.4f;
                e.touchDamage = 8;
                e.xpValue = 1;
                break;

            case EnemyType.Slime:
                sr.sprite = VampireSurvivorsMini.CircleSprite;
                sr.color = new Color(0.3f, 0.9f, 0.4f);
                go.transform.localScale = Vector3.one * 0.55f;
                e.hp = 30 + (int)(difficulty * 12f);
                e.moveSpeed = 1.5f + difficulty * 0.2f;
                e.touchDamage = 12;
                e.xpValue = 2;
                break;

            case EnemyType.Archer:
                sr.color = new Color(0.4f, 0.6f, 0.3f);
                go.transform.localScale = Vector3.one * 0.45f;
                e.hp = 15 + (int)(difficulty * 6f);
                e.moveSpeed = 1.8f + difficulty * 0.2f;
                e.touchDamage = 8;
                e.xpValue = 2;
                break;

            case EnemyType.Ghost:
                sr.sprite = VampireSurvivorsMini.CircleSprite;
                sr.color = new Color(0.8f, 0.8f, 1f, 0.6f);
                go.transform.localScale = Vector3.one * 0.5f;
                e.hp = 25 + (int)(difficulty * 8f);
                e.moveSpeed = 2.2f + difficulty * 0.25f;
                e.touchDamage = 12;
                e.xpValue = 2;
                e.dodgeChance = 0.3f + difficulty * 0.02f;
                break;

            case EnemyType.Skeleton:
                sr.color = new Color(0.9f, 0.9f, 0.85f);
                go.transform.localScale = Vector3.one * 0.5f;
                e.hp = 20 + (int)(difficulty * 8f);
                e.moveSpeed = 2f + difficulty * 0.25f;
                e.touchDamage = 12;
                e.xpValue = 2;
                break;

            case EnemyType.Mage:
                sr.color = new Color(0.6f, 0.3f, 0.8f);
                go.transform.localScale = Vector3.one * 0.5f;
                e.hp = 18 + (int)(difficulty * 7f);
                e.moveSpeed = 1.6f + difficulty * 0.2f;
                e.touchDamage = 10;
                e.xpValue = 3;
                break;

            case EnemyType.Golem:
                sr.color = new Color(0.5f, 0.45f, 0.4f);
                go.transform.localScale = Vector3.one * 1.0f;
                e.hp = 150 + (int)(difficulty * 60f);
                e.moveSpeed = 0.8f + difficulty * 0.1f;
                e.touchDamage = 25;
                e.xpValue = 5;
                break;

            case EnemyType.Vampire:
                sr.sprite = VampireSurvivorsMini.CircleSprite;
                sr.color = new Color(0.4f, 0.1f, 0.2f);
                go.transform.localScale = Vector3.one * 0.55f;
                e.hp = 35 + (int)(difficulty * 12f);
                e.moveSpeed = 2.5f + difficulty * 0.3f;
                e.touchDamage = 15;
                e.xpValue = 3;
                break;

            case EnemyType.Boss:
            case EnemyType.ForestBoss:
            case EnemyType.GraveyardBoss:
            case EnemyType.CastleBoss:
                SetupBoss(e, sr, go, type, difficulty);
                break;
        }

        e.maxHp = e.hp;
        e.baseColor = sr.color;

        go.AddComponent<EnemyVisuals>().Build(type);

        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
        e.allRenderers = srs;
        e.allBaseColors = new Color[srs.Length];
        for (int i = 0; i < srs.Length; i++)
            e.allBaseColors[i] = srs[i].color;

        GameState.Enemies.Add(e);
        return e;
    }

    static void SetupBoss(Enemy e, SpriteRenderer sr, GameObject go, EnemyType type, float difficulty)
    {
        sr.sprite = VampireSurvivorsMini.CircleSprite;
        go.transform.localScale = Vector3.one * 1.6f;
        e.touchDamage = 30;
        e.xpValue = 15;

        switch (type)
        {
            case EnemyType.Boss:
                sr.color = new Color(0.7f, 0.2f, 1f);
                e.hp = 400 + (int)(difficulty * 250f);
                e.moveSpeed = 1.6f + difficulty * 0.1f;
                break;

            case EnemyType.ForestBoss:
                sr.color = new Color(0.2f, 0.8f, 0.3f);
                go.transform.localScale = Vector3.one * 1.8f;
                e.hp = 500 + (int)(difficulty * 300f);
                e.moveSpeed = 1.4f + difficulty * 0.1f;
                e.xpValue = 20;
                break;

            case EnemyType.GraveyardBoss:
                sr.color = new Color(0.5f, 0.5f, 0.7f);
                go.transform.localScale = Vector3.one * 1.7f;
                e.hp = 450 + (int)(difficulty * 280f);
                e.moveSpeed = 1.5f + difficulty * 0.1f;
                e.xpValue = 20;
                break;

            case EnemyType.CastleBoss:
                sr.color = new Color(0.8f, 0.2f, 0.2f);
                go.transform.localScale = Vector3.one * 2.0f;
                e.hp = 600 + (int)(difficulty * 350f);
                e.moveSpeed = 1.2f + difficulty * 0.1f;
                e.touchDamage = 40;
                e.xpValue = 25;
                break;
        }
    }

    void Update()
    {
        if (GameState.GameOver || player == null) return;
        if (Time.timeScale == 0f) return;

        // 被弾フラッシュ
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            bool white = flashTimer > 0f;
            for (int i = 0; i < allRenderers.Length; i++)
                allRenderers[i].color = white ? Color.white : allBaseColors[i];
        }

        // タイプ別行動
        switch (type)
        {
            case EnemyType.Dasher:
                UpdateDasher();
                break;
            case EnemyType.Bat:
                UpdateBat();
                break;
            case EnemyType.Archer:
                UpdateArcher();
                break;
            case EnemyType.Ghost:
                UpdateGhost();
                break;
            case EnemyType.Mage:
                UpdateMage();
                break;
            case EnemyType.ForestBoss:
            case EnemyType.GraveyardBoss:
            case EnemyType.CastleBoss:
                UpdateSpecialBoss();
                break;
            default:
                UpdateChase();
                break;
        }
    }

    void UpdateChase()
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
        CheckContact(toPlayer);
    }

    void UpdateDasher()
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        stateTimer -= Time.deltaTime;

        switch (dashState)
        {
            case DashState.Approach:
                transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
                if (toPlayer.magnitude < 4f)
                {
                    dashState = DashState.Telegraph;
                    stateTimer = 0.6f;
                }
                break;

            case DashState.Telegraph:
                sr.color = Color.Lerp(baseColor, Color.white, Mathf.PingPong(Time.time * 8f, 1f));
                moveDir = toPlayer.normalized;
                if (stateTimer <= 0f)
                {
                    dashState = DashState.Dash;
                    stateTimer = 0.45f;
                    sr.color = baseColor;
                }
                break;

            case DashState.Dash:
                transform.position += moveDir * (moveSpeed * 4f) * Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    dashState = DashState.Cooldown;
                    stateTimer = 0.8f;
                }
                break;

            case DashState.Cooldown:
                if (stateTimer <= 0f) dashState = DashState.Approach;
                break;
        }
        CheckContact(player.transform.position - transform.position);
    }

    void UpdateBat()
    {
        behaviorTimer += Time.deltaTime;
        Vector3 toPlayer = player.transform.position - transform.position;

        // 波状移動
        float wave = Mathf.Sin(behaviorTimer * 6f) * 1.5f;
        Vector3 perpendicular = new Vector3(-toPlayer.normalized.y, toPlayer.normalized.x, 0);
        Vector3 movement = toPlayer.normalized * moveSpeed + perpendicular * wave;

        transform.position += movement * Time.deltaTime;
        CheckContact(toPlayer);
    }

    void UpdateArcher()
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        float dist = toPlayer.magnitude;

        // 適切な距離を保つ
        if (dist > ShootRange)
        {
            transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
        }
        else if (dist < ShootRange * 0.5f)
        {
            transform.position -= toPlayer.normalized * moveSpeed * 0.5f * Time.deltaTime;
        }

        // 射撃
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f && dist < ShootRange)
        {
            shootTimer = ShootInterval;
            EnemyProjectile.Spawn(transform.position, toPlayer.normalized, touchDamage / 2);
        }

        CheckContact(toPlayer);
    }

    void UpdateGhost()
    {
        // 半透明で揺らぐ
        behaviorTimer += Time.deltaTime;
        float alpha = 0.5f + Mathf.Sin(behaviorTimer * 3f) * 0.15f;
        sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

        UpdateChase();
    }

    void UpdateMage()
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        float dist = toPlayer.magnitude;

        // 距離を保つ
        if (dist > 5f)
            transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
        else if (dist < 3f)
            transform.position -= toPlayer.normalized * moveSpeed * 0.6f * Time.deltaTime;

        // 魔法陣攻撃
        behaviorTimer -= Time.deltaTime;
        if (behaviorTimer <= 0f)
        {
            behaviorTimer = 3f;
            MagicCircle.Spawn(player.transform.position, touchDamage);
        }

        CheckContact(toPlayer);
    }

    void UpdateSpecialBoss()
    {
        UpdateChase();

        behaviorTimer -= Time.deltaTime;
        if (behaviorTimer <= 0f)
        {
            behaviorTimer = 4f;

            switch (type)
            {
                case EnemyType.ForestBoss:
                    // 小型スライムを召喚
                    for (int i = 0; i < 3; i++)
                    {
                        var offset = (Vector3)(Random.insideUnitCircle * 1.5f);
                        Spawn(EnemyType.Slime, transform.position + offset, player,
                            GameState.ElapsedTime / 30f);
                    }
                    break;

                case EnemyType.GraveyardBoss:
                    // スケルトンを召喚
                    for (int i = 0; i < 2; i++)
                    {
                        var offset = (Vector3)(Random.insideUnitCircle * 2f);
                        Spawn(EnemyType.Skeleton, transform.position + offset, player,
                            GameState.ElapsedTime / 30f);
                    }
                    break;

                case EnemyType.CastleBoss:
                    // 全方位弾
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = i * 45f * Mathf.Deg2Rad;
                        Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                        EnemyProjectile.Spawn(transform.position, dir, touchDamage / 3);
                    }
                    break;
            }
        }
    }

    void CheckContact(Vector3 toPlayer)
    {
        float hitDist = 0.35f + transform.localScale.x * 0.4f;
        if (toPlayer.magnitude < hitDist)
            player.TakeDamage(touchDamage);
    }

    public void TakeDamage(int damage) => TakeDamage(damage, Vector3.zero);

    public void TakeDamage(int damage, Vector3 knockback)
    {
        if (hp <= 0) return;

        // ゴーストは確率で回避
        if (type == EnemyType.Ghost && Random.value < dodgeChance)
        {
            // 回避エフェクト（一瞬消える）
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.1f);
            return;
        }

        hp -= damage;
        flashTimer = 0.08f;
        DamagePopup.Spawn(transform.position, damage);

        // ノックバック（ボス・ゴーレムは重いので軽減）
        if (knockback != Vector3.zero)
        {
            float knockbackMult = IsBoss() ? 0.3f : (type == EnemyType.Golem ? 0.2f : 1f);
            transform.position += knockback * knockbackMult;
        }

        if (hp <= 0) Die();
    }

    bool IsBoss()
    {
        return type == EnemyType.Boss || type == EnemyType.ForestBoss ||
               type == EnemyType.GraveyardBoss || type == EnemyType.CastleBoss;
    }

    bool IsCurrentStageBoss()
    {
        if (StageManager.Instance == null) return false;
        return type == StageManager.Instance.CurrentStage.BossType;
    }

    void Die()
    {
        // スケルトンは1回だけ復活
        if (type == EnemyType.Skeleton && !hasRevived)
        {
            hasRevived = true;
            hp = maxHp / 2;
            flashTimer = 0.3f;
            // 復活エフェクト
            transform.localScale *= 0.8f;
            return;
        }

        GameState.Enemies.Remove(this);
        GameState.KillCount++;
        AudioManager.PlayDie();

        DeathParticle.Burst(transform.position, baseColor, transform.localScale.x / 0.5f);

        // スライムは分裂
        if (type == EnemyType.Slime && transform.localScale.x > 0.35f)
        {
            for (int i = 0; i < 2; i++)
            {
                var offset = (Vector3)(Random.insideUnitCircle * 0.5f);
                var mini = Spawn(EnemyType.Slime, transform.position + offset, player,
                    GameState.ElapsedTime / 30f);
                mini.transform.localScale = transform.localScale * 0.6f;
                mini.hp = hp / 3 + 5;
                mini.maxHp = mini.hp;
                mini.xpValue = 1;
            }
        }

        // ボスドロップ
        if (IsBoss())
        {
            for (int i = 0; i < xpValue; i++)
            {
                var offset = (Vector3)(Random.insideUnitCircle * 1.2f);
                XpGem.Spawn(transform.position + offset, 1);
            }
            int goldAmount = 10 + (int)(GameState.ElapsedTime / 60f) * 5;
            if (type != EnemyType.Boss) goldAmount = (int)(goldAmount * 1.5f);
            for (int i = 0; i < 5; i++)
            {
                var offset = (Vector3)(Random.insideUnitCircle * 1.0f);
                GoldCoin.Spawn(transform.position + offset, goldAmount / 5);
            }

            // ステージボスを倒したらステージクリア
            if (IsCurrentStageBoss())
            {
                StageManager.Instance?.OnBossDefeated();
            }
        }
        else
        {
            XpGem.Spawn(transform.position, xpValue);
            TryDropGold();
        }

        Destroy(gameObject);
    }

    void TryDropGold()
    {
        float dropChance = type switch
        {
            EnemyType.Chaser => 0.15f,
            EnemyType.Runner => 0.10f,
            EnemyType.Tank => 0.35f,
            EnemyType.Dasher => 0.25f,
            EnemyType.Bat => 0.12f,
            EnemyType.Slime => 0.20f,
            EnemyType.Archer => 0.25f,
            EnemyType.Ghost => 0.30f,
            EnemyType.Skeleton => 0.20f,
            EnemyType.Mage => 0.35f,
            EnemyType.Golem => 0.50f,
            EnemyType.Vampire => 0.40f,
            _ => 0.15f
        };

        if (Random.value < dropChance)
        {
            int goldValue = type switch
            {
                EnemyType.Tank => 3,
                EnemyType.Dasher => 2,
                EnemyType.Mage => 3,
                EnemyType.Golem => 5,
                EnemyType.Vampire => 3,
                _ => 1
            };
            GoldCoin.Spawn(transform.position, goldValue);
        }
    }

    void OnDestroy() { GameState.Enemies.Remove(this); }
}

/// <summary>敵の弾（アーチャー、ボス用）</summary>
public class EnemyProjectile : MonoBehaviour
{
    int damage;
    Vector3 direction;
    float life;
    const float Speed = 5f;
    const float Lifetime = 4f;

    public static void Spawn(Vector3 pos, Vector3 dir, int damage)
    {
        var go = new GameObject("EnemyProjectile");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.2f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.CircleSprite;
        sr.color = new Color(1f, 0.3f, 0.3f);
        sr.sortingOrder = 7;

        var proj = go.AddComponent<EnemyProjectile>();
        proj.damage = damage;
        proj.direction = dir.normalized;
    }

    void Update()
    {
        transform.position += direction * Speed * Time.deltaTime;
        life += Time.deltaTime;
        if (life > Lifetime) { Destroy(gameObject); return; }

        var player = Object.FindObjectOfType<Player>();
        if (player != null)
        {
            if ((player.transform.position - transform.position).sqrMagnitude < 0.4f * 0.4f)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}

/// <summary>魔法陣（メイジ用）</summary>
public class MagicCircle : MonoBehaviour
{
    int damage;
    float life;
    const float ChargeTime = 1.2f;
    const float Radius = 1.2f;
    SpriteRenderer sr;

    public static void Spawn(Vector3 pos, int damage)
    {
        var go = new GameObject("MagicCircle");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * Radius * 2f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.CircleSprite;
        sr.color = new Color(0.8f, 0.3f, 1f, 0.3f);
        sr.sortingOrder = 1;

        var mc = go.AddComponent<MagicCircle>();
        mc.damage = damage;
        mc.sr = sr;
    }

    void Update()
    {
        life += Time.deltaTime;

        // 点滅警告
        float flash = Mathf.Sin(life * 10f) * 0.2f + 0.4f;
        sr.color = new Color(0.8f, 0.3f, 1f, flash);

        if (life >= ChargeTime)
        {
            // 爆発ダメージ
            var player = Object.FindObjectOfType<Player>();
            if (player != null)
            {
                if ((player.transform.position - transform.position).sqrMagnitude < Radius * Radius)
                {
                    player.TakeDamage(damage);
                }
            }

            // 爆発エフェクト
            sr.color = new Color(1f, 0.5f, 1f, 0.8f);
            Destroy(gameObject, 0.1f);
        }
    }
}
