using UnityEngine;

/// <summary>敵スポナー：ステージに応じた敵を出現させる</summary>
public class EnemySpawner : MonoBehaviour
{
    public Player player;
    float spawnTimer;
    const float SpawnRadius = 10f;

    StageData stageData;
    float difficulty;

    void Start()
    {
        // ステージデータを取得
        stageData = StageSelection.GetSelectedData();
    }

    void Update()
    {
        if (GameState.GameOver || player == null) return;

        // ボスはStageManagerが管理するのでここでは通常敵のみ
        difficulty = GameState.ElapsedTime / 30f;

        // スポーン間隔：ステージの難易度で調整
        float baseInterval = Mathf.Max(0.3f, 1.5f - difficulty * 0.15f);
        float adjustedInterval = baseInterval / (stageData?.SpawnRateMultiplier ?? 1f);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;
        spawnTimer = adjustedInterval;

        // スポーン数：時間経過で増加（最大3体同時）
        int spawnCount = 1 + Mathf.FloorToInt(difficulty / 3f);
        spawnCount = Mathf.Min(spawnCount, 3);

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnRandomEnemy();
        }
    }

    void SpawnRandomEnemy()
    {
        EnemyType type;

        if (stageData != null)
        {
            type = stageData.GetRandomEnemyType(GameState.ElapsedTime);
        }
        else
        {
            type = PickTypeFallback();
        }

        SpawnEnemy(type, RandomEdgePos());
    }

    /// <summary>指定した敵タイプを指定位置にスポーン</summary>
    public void SpawnEnemy(EnemyType type, Vector3 position)
    {
        float adjustedDifficulty = difficulty * (stageData?.DifficultyMultiplier ?? 1f);
        Enemy.Spawn(type, position, player, adjustedDifficulty);
    }

    /// <summary>ステージデータがない場合のフォールバック（従来の動作）</summary>
    EnemyType PickTypeFallback()
    {
        float chaser = 10f;
        float runner = Mathf.Min(6f, difficulty * 3f);
        float tank = Mathf.Clamp((difficulty - 1f) * 2.5f, 0f, 5f);
        float dasher = Mathf.Clamp((difficulty - 2f) * 2.5f, 0f, 5f);

        float total = chaser + runner + tank + dasher;
        float r = Random.value * total;
        if ((r -= chaser) < 0f) return EnemyType.Chaser;
        if ((r -= runner) < 0f) return EnemyType.Runner;
        if ((r -= tank) < 0f) return EnemyType.Tank;
        return EnemyType.Dasher;
    }

    Vector3 RandomEdgePos()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        var offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * SpawnRadius;
        return player.transform.position + offset;
    }
}
