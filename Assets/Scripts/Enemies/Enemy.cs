using UnityEngine;

/// <summary>敵：タイプごとに性能と挙動が変わる</summary>
public class Enemy : MonoBehaviour
{
    public EnemyType type;
    public float moveSpeed = 2f;
    public int hp = 20;
    public int touchDamage = 10;
    public int xpValue = 1;

    Player player;
    SpriteRenderer sr;
    Color baseColor;
    float flashTimer; // 被弾時の白フラッシュ残り時間

    // ダッシャー用ステートマシン
    enum DashState { Approach, Telegraph, Dash, Cooldown }
    DashState dashState = DashState.Approach;
    float stateTimer;
    Vector3 dashDir;

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

        // タイプ別パラメータ（difficulty = 経過30秒ごとに+1で補正）
        switch (type)
        {
            case EnemyType.Chaser:
                sr.color = new Color(1f, 0.35f, 0.35f);
                go.transform.localScale = Vector3.one * 0.5f;
                e.hp = 20 + (int)(difficulty * 10f);
                e.moveSpeed = 2f + difficulty * 0.3f;
                e.touchDamage = 10;
                e.xpValue = 1;
                break;

            case EnemyType.Runner:
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

            case EnemyType.Dasher: // ひし形（45度回転）
                sr.color = new Color(1f, 0.4f, 0.85f);
                go.transform.localScale = Vector3.one * 0.45f;
                go.transform.rotation = Quaternion.Euler(0, 0, 45f);
                e.hp = 25 + (int)(difficulty * 10f);
                e.moveSpeed = 2.5f + difficulty * 0.3f;
                e.touchDamage = 15;
                e.xpValue = 2;
                break;

            case EnemyType.Boss:
                sr.color = new Color(0.7f, 0.2f, 1f);
                go.transform.localScale = Vector3.one * 1.6f;
                e.hp = 400 + (int)(difficulty * 250f);
                e.moveSpeed = 1.6f + difficulty * 0.1f;
                e.touchDamage = 30;
                e.xpValue = 15; // 死亡時にジェムをばら撒く
                break;
        }
        e.baseColor = sr.color;

        GameState.Enemies.Add(e);
        return e;
    }

    void Update()
    {
        if (GameState.GameOver || player == null) return;
        if (Time.timeScale == 0f) return; // レベルアップ選択中は停止

        // 被弾フラッシュ：一定時間白くして元の色に戻す
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            sr.color = flashTimer > 0f ? Color.white : baseColor;
        }

        if (type == EnemyType.Dasher) UpdateDasher();
        else UpdateChase();
    }

    /// <summary>標準挙動：プレイヤーへ直進</summary>
    void UpdateChase()
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
        CheckContact(toPlayer);
    }

    /// <summary>突進挙動：接近 → 狙い(点滅) → 突進 → クールダウン</summary>
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

            case DashState.Telegraph: // 停止して白く点滅しながら狙いを定める
                sr.color = Color.Lerp(baseColor, Color.white,
                    Mathf.PingPong(Time.time * 8f, 1f));
                dashDir = toPlayer.normalized; // 突進直前まで追尾
                if (stateTimer <= 0f)
                {
                    dashState = DashState.Dash;
                    stateTimer = 0.45f;
                    sr.color = baseColor;
                }
                break;

            case DashState.Dash: // 定めた方向へ高速直進
                transform.position += dashDir * (moveSpeed * 4f) * Time.deltaTime;
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

    /// <summary>接触ダメージ判定（敵サイズに応じて距離を調整）</summary>
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
        hp -= damage;
        flashTimer = 0.08f; // 白フラッシュで被弾を可視化
        DamagePopup.Spawn(transform.position, damage);

        // ノックバック（ボスは重いので半減）
        if (knockback != Vector3.zero)
            transform.position += type == EnemyType.Boss ? knockback * 0.5f : knockback;

        if (hp <= 0) Die();
    }

    void Die()
    {
        GameState.Enemies.Remove(this);
        GameState.KillCount++;
        AudioManager.PlayDie();

        // 敵の色とサイズに応じた破片を飛ばす（標準サイズ0.5を基準に倍率計算）
        DeathParticle.Burst(transform.position, baseColor, transform.localScale.x / 0.5f);

        if (type == EnemyType.Boss)
        {
            // ジェムを周囲にばら撒く
            for (int i = 0; i < xpValue; i++)
            {
                var offset = (Vector3)(Random.insideUnitCircle * 1.2f);
                XpGem.Spawn(transform.position + offset, 1);
            }
        }
        else
        {
            XpGem.Spawn(transform.position, xpValue);
        }
        Destroy(gameObject);
    }

    void OnDestroy() { GameState.Enemies.Remove(this); }
}
