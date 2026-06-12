using UnityEngine;

/// <summary>敵スポナー：難易度に応じた重み付き抽選＋60秒ごとのボス出現</summary>
public class EnemySpawner : MonoBehaviour
{
    public Player player;
    float spawnTimer;
    float bossTimer = 60f;
    const float SpawnRadius = 10f; // カメラ外の距離
    const float BossInterval = 60f;

    void Update()
    {
        if (GameState.GameOver || player == null) return;

        float difficulty = GameState.ElapsedTime / 30f;

        // ボス：60秒ごとに出現（Time.timeScale=0中はタイマーも止まる）
        bossTimer -= Time.deltaTime;
        if (bossTimer <= 0f)
        {
            bossTimer = BossInterval;
            Enemy.Spawn(EnemyType.Boss, RandomEdgePos(), player, difficulty);
            // 画面上部のトーストで警告
            player.lastUpgradeText = "⚠ ボス出現！";
            player.lastUpgradeTime = Time.time;
        }

        // 通常スポーン：間隔は1.5秒から徐々に短く（最短0.3秒）
        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;
        spawnTimer = Mathf.Max(0.3f, 1.5f - difficulty * 0.15f);

        Enemy.Spawn(PickType(difficulty), RandomEdgePos(), player, difficulty);
    }

    /// <summary>難易度に応じた出現タイプの重み付き抽選</summary>
    EnemyType PickType(float difficulty)
    {
        // 重み：時間経過で強い敵の比率が上がる
        float chaser = 10f;
        float runner = Mathf.Min(6f, difficulty * 3f);                 // 〜30秒で解禁
        float tank   = Mathf.Clamp((difficulty - 1f) * 2.5f, 0f, 5f); // 〜60秒で解禁
        float dasher = Mathf.Clamp((difficulty - 2f) * 2.5f, 0f, 5f); // 〜90秒で解禁

        float total = chaser + runner + tank + dasher;
        float r = Random.value * total;
        if ((r -= chaser) < 0f) return EnemyType.Chaser;
        if ((r -= runner) < 0f) return EnemyType.Runner;
        if ((r -= tank) < 0f)   return EnemyType.Tank;
        return EnemyType.Dasher;
    }

    Vector3 RandomEdgePos()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        var offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * SpawnRadius;
        return player.transform.position + offset;
    }
}
