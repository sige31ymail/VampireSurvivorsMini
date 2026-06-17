using UnityEngine;
using System.Collections.Generic;

/// <summary>ステージの種類</summary>
public enum StageType
{
    Grassland,  // 草原（基本）
    Forest,     // 森
    Graveyard,  // 墓地
    Castle      // 城
}

/// <summary>ステージデータの定義</summary>
[System.Serializable]
public class StageData
{
    public StageType Type;
    public string Name;
    public string Description;
    public Color BackgroundColor;
    public Color GroundColor;

    // ステージ難易度
    public float DifficultyMultiplier;
    public float SpawnRateMultiplier;

    // 出現する敵タイプ
    public EnemyType[] BasicEnemies;
    public EnemyType[] MidGameEnemies;
    public EnemyType[] LateGameEnemies;
    public EnemyType BossType;

    // ボス出現時間（秒）
    public float BossSpawnTime;

    // アンロック条件
    public bool IsUnlockedByDefault;
    public string UnlockCondition;

    /// <summary>全ステージデータを取得</summary>
    public static List<StageData> GetAllStages()
    {
        return new List<StageData>
        {
            // 草原ステージ（基本）
            new StageData
            {
                Type = StageType.Grassland,
                Name = "草原",
                Description = "始まりの地。基本的な敵が出現",
                BackgroundColor = new Color(0.2f, 0.35f, 0.15f),
                GroundColor = new Color(0.3f, 0.5f, 0.2f),
                DifficultyMultiplier = 1.0f,
                SpawnRateMultiplier = 1.0f,
                BasicEnemies = new[] { EnemyType.Chaser, EnemyType.Runner },
                MidGameEnemies = new[] { EnemyType.Chaser, EnemyType.Runner, EnemyType.Tank },
                LateGameEnemies = new[] { EnemyType.Chaser, EnemyType.Runner, EnemyType.Tank, EnemyType.Dasher },
                BossType = EnemyType.Boss,
                BossSpawnTime = 300f, // 5分
                IsUnlockedByDefault = true,
                UnlockCondition = ""
            },

            // 森ステージ
            new StageData
            {
                Type = StageType.Forest,
                Name = "暗黒の森",
                Description = "素早い敵と分裂するスライムが出現",
                BackgroundColor = new Color(0.1f, 0.2f, 0.1f),
                GroundColor = new Color(0.15f, 0.25f, 0.1f),
                DifficultyMultiplier = 1.3f,
                SpawnRateMultiplier = 1.2f,
                BasicEnemies = new[] { EnemyType.Chaser, EnemyType.Bat },
                MidGameEnemies = new[] { EnemyType.Bat, EnemyType.Slime, EnemyType.Runner },
                LateGameEnemies = new[] { EnemyType.Bat, EnemyType.Slime, EnemyType.Archer, EnemyType.Dasher },
                BossType = EnemyType.ForestBoss,
                BossSpawnTime = 300f,
                IsUnlockedByDefault = false,
                UnlockCondition = "草原ステージをクリア"
            },

            // 墓地ステージ
            new StageData
            {
                Type = StageType.Graveyard,
                Name = "忘れられた墓地",
                Description = "復活するスケルトンと幽霊が出現",
                BackgroundColor = new Color(0.15f, 0.12f, 0.18f),
                GroundColor = new Color(0.25f, 0.2f, 0.28f),
                DifficultyMultiplier = 1.6f,
                SpawnRateMultiplier = 1.0f,
                BasicEnemies = new[] { EnemyType.Chaser, EnemyType.Ghost },
                MidGameEnemies = new[] { EnemyType.Ghost, EnemyType.Skeleton, EnemyType.Bat },
                LateGameEnemies = new[] { EnemyType.Ghost, EnemyType.Skeleton, EnemyType.Mage, EnemyType.Archer },
                BossType = EnemyType.GraveyardBoss,
                BossSpawnTime = 300f,
                IsUnlockedByDefault = false,
                UnlockCondition = "森ステージをクリア"
            },

            // 城ステージ
            new StageData
            {
                Type = StageType.Castle,
                Name = "闇の城",
                Description = "最も困難なステージ。強力な敵が出現",
                BackgroundColor = new Color(0.1f, 0.08f, 0.12f),
                GroundColor = new Color(0.2f, 0.15f, 0.22f),
                DifficultyMultiplier = 2.0f,
                SpawnRateMultiplier = 0.9f, // 数は少ないが強い
                BasicEnemies = new[] { EnemyType.Tank, EnemyType.Ghost },
                MidGameEnemies = new[] { EnemyType.Golem, EnemyType.Skeleton, EnemyType.Mage },
                LateGameEnemies = new[] { EnemyType.Golem, EnemyType.Vampire, EnemyType.Mage, EnemyType.Archer },
                BossType = EnemyType.CastleBoss,
                BossSpawnTime = 300f,
                IsUnlockedByDefault = false,
                UnlockCondition = "墓地ステージをクリア"
            }
        };
    }

    /// <summary>特定のステージデータを取得</summary>
    public static StageData GetStage(StageType type)
    {
        var all = GetAllStages();
        return all.Find(s => s.Type == type) ?? all[0];
    }

    /// <summary>経過時間に応じた敵タイプを取得</summary>
    public EnemyType GetRandomEnemyType(float elapsedTime)
    {
        EnemyType[] pool;

        if (elapsedTime < 60f)
        {
            pool = BasicEnemies;
        }
        else if (elapsedTime < 180f)
        {
            pool = MidGameEnemies;
        }
        else
        {
            pool = LateGameEnemies;
        }

        return pool[Random.Range(0, pool.Length)];
    }
}

/// <summary>選択中のステージを管理</summary>
public static class StageSelection
{
    public static StageType SelectedStage { get; set; } = StageType.Grassland;

    public static StageData GetSelectedData()
    {
        return StageData.GetStage(SelectedStage);
    }
}

/// <summary>ステージ管理（ゲーム中のステージ処理）</summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    public StageData CurrentStage { get; private set; }
    public bool BossSpawned { get; private set; }
    public bool StageCleared { get; private set; }

    SpriteRenderer background;
    Transform groundParent;

    void Awake()
    {
        Instance = this;
        CurrentStage = StageSelection.GetSelectedData();
    }

    void Start()
    {
        SetupStageVisuals();
    }

    void SetupStageVisuals()
    {
        // カメラ背景色を設定
        Camera.main.backgroundColor = CurrentStage.BackgroundColor;

        // 地面タイルを生成（シンプルなグリッド）
        CreateGroundTiles();
    }

    void CreateGroundTiles()
    {
        groundParent = new GameObject("Ground").transform;

        // プレイヤー周辺の地面を生成
        int size = 20;
        float tileSize = 2f;

        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                var tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.SetParent(groundParent);
                tile.transform.position = new Vector3(x * tileSize, y * tileSize, 0);
                tile.transform.localScale = Vector3.one * tileSize;

                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = VampireSurvivorsMini.SquareSprite;

                // 少し色にバリエーションを持たせる
                float variation = Random.Range(-0.03f, 0.03f);
                sr.color = new Color(
                    CurrentStage.GroundColor.r + variation,
                    CurrentStage.GroundColor.g + variation,
                    CurrentStage.GroundColor.b + variation
                );
                sr.sortingOrder = -10;
            }
        }
    }

    void Update()
    {
        if (GameState.GameOver || StageCleared) return;

        // ボス出現チェック
        if (!BossSpawned && GameState.ElapsedTime >= CurrentStage.BossSpawnTime)
        {
            SpawnBoss();
        }
    }

    void SpawnBoss()
    {
        BossSpawned = true;

        // ボス出現警告
        AudioManager.PlayLevelUp(); // 仮のサウンド

        // プレイヤーから離れた位置にボスをスポーン
        var player = FindFirstObjectByType<Player>();
        if (player == null) return;

        Vector2 offset = Random.insideUnitCircle.normalized * 8f;
        Vector3 spawnPos = player.transform.position + new Vector3(offset.x, offset.y, 0);

        // ボスを生成（EnemySpawnerを通じて）
        var spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.SpawnEnemy(CurrentStage.BossType, spawnPos);
        }
    }

    /// <summary>ボスを倒した時に呼ばれる</summary>
    public void OnBossDefeated()
    {
        StageCleared = true;

        // ステージクリア処理
        UnlockNextStage();

        // 統計を保存
        SaveSystem.MarkStageCleared(CurrentStage.Type);
    }

    void UnlockNextStage()
    {
        // 次のステージをアンロック
        switch (CurrentStage.Type)
        {
            case StageType.Grassland:
                UnlockManager.Instance?.Unlock("stage_forest");
                break;
            case StageType.Forest:
                UnlockManager.Instance?.Unlock("stage_graveyard");
                break;
            case StageType.Graveyard:
                UnlockManager.Instance?.Unlock("stage_castle");
                UnlockManager.Instance?.Unlock("char_vampire"); // ヴァンパイアキャラもアンロック
                break;
            case StageType.Castle:
                // ゲームクリア
                break;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
