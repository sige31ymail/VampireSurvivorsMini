using System;
using System.IO;
using UnityEngine;

/// <summary>
/// セーブ/ロードシステム
/// ゲーム設定とメタプログレッションを永続化
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    // 設定データ
    public GameSettings Settings { get; private set; }

    // メタプログレッションデータ（Phase 2で使用）
    public MetaProgressData MetaProgress { get; private set; }

    // 統計データ
    public StatisticsData Statistics { get; private set; }

    const string SettingsKey = "GameSettings";
    const string MetaProgressFile = "metaprogress.json";
    const string StatisticsFile = "statistics.json";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAll();
    }

    void OnApplicationQuit()
    {
        SaveAll();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveAll();
    }

    #region Public API

    /// <summary>全データを保存</summary>
    public void SaveAll()
    {
        SaveSettings();
        SaveMetaProgress();
        SaveStatistics();
    }

    /// <summary>全データを読み込み</summary>
    public void LoadAll()
    {
        LoadSettings();
        LoadMetaProgress();
        LoadStatistics();
    }

    /// <summary>全データを削除（デバッグ用）</summary>
    public void DeleteAll()
    {
        PlayerPrefs.DeleteKey(SettingsKey);
        DeleteFile(MetaProgressFile);
        DeleteFile(StatisticsFile);

        Settings = new GameSettings();
        MetaProgress = new MetaProgressData();
        Statistics = new StatisticsData();
    }

    #endregion

    #region Settings (PlayerPrefs)

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(Settings);
        PlayerPrefs.SetString(SettingsKey, json);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        if (PlayerPrefs.HasKey(SettingsKey))
        {
            string json = PlayerPrefs.GetString(SettingsKey);
            Settings = JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            Settings = new GameSettings();
        }
    }

    #endregion

    #region MetaProgress (JSON File)

    public void SaveMetaProgress()
    {
        SaveToFile(MetaProgressFile, MetaProgress);
    }

    void LoadMetaProgress()
    {
        MetaProgress = LoadFromFile<MetaProgressData>(MetaProgressFile) ?? new MetaProgressData();
    }

    #endregion

    #region Statistics (JSON File)

    public void SaveStatistics()
    {
        SaveToFile(StatisticsFile, Statistics);
    }

    void LoadStatistics()
    {
        Statistics = LoadFromFile<StatisticsData>(StatisticsFile) ?? new StatisticsData();
    }

    /// <summary>ゲーム終了時に統計を更新</summary>
    public void RecordGameResult(float survivalTime, int kills, int level, int goldEarned)
    {
        Statistics.TotalPlayTime += survivalTime;
        Statistics.TotalKills += kills;
        Statistics.TotalGamesPlayed++;
        Statistics.TotalGoldEarned += goldEarned;

        if (survivalTime > Statistics.BestSurvivalTime)
            Statistics.BestSurvivalTime = survivalTime;
        if (kills > Statistics.BestKills)
            Statistics.BestKills = kills;
        if (level > Statistics.BestLevel)
            Statistics.BestLevel = level;

        SaveStatistics();
    }

    #endregion

    #region File I/O Helpers

    void SaveToFile<T>(string filename, T data)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem: Failed to save {filename}: {e.Message}");
        }
    }

    T LoadFromFile<T>(string filename) where T : class
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem: Failed to load {filename}: {e.Message}");
        }
        return null;
    }

    void DeleteFile(string filename)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem: Failed to delete {filename}: {e.Message}");
        }
    }

    #endregion
}

/// <summary>ゲーム設定データ</summary>
[Serializable]
public class GameSettings
{
    public float BgmVolume = 0.35f;
    public float SeVolume = 0.55f;
    public bool Fullscreen = true;
    public int ResolutionIndex = -1; // -1 = デフォルト
    public bool ScreenShake = true;
    public bool DamageNumbers = true;

    // アクセシビリティ
    public float UiScale = 1f;
    public bool HighContrastMode = false;
}

/// <summary>メタプログレッションデータ（Phase 2で使用）</summary>
[Serializable]
public class MetaProgressData
{
    // ゴールド
    public int Gold = 0;

    // 永続アップグレードレベル
    public int MaxHpBonus = 0;        // +10% per level, max 10
    public int AttackBonus = 0;       // +5% per level, max 10
    public int MoveSpeedBonus = 0;    // +3% per level, max 10
    public int XpGainBonus = 0;       // +5% per level, max 10
    public int GoldGainBonus = 0;     // +10% per level, max 10
    public int ArmorBonus = 0;        // +2 per level, max 5
    public int RegenBonus = 0;        // +0.5/sec per level, max 5
    public int MagnetBonus = 0;       // +10% per level, max 5

    // アンロック状態（Phase 3で使用）
    public bool[] UnlockedWeapons = new bool[16];
    public bool[] UnlockedCharacters = new bool[8];
    public bool[] UnlockedStages = new bool[8];

    public MetaProgressData()
    {
        // 初期武器・キャラ・ステージは解放済み
        UnlockedWeapons[0] = true;
        UnlockedCharacters[0] = true;
        UnlockedStages[0] = true;
    }
}

/// <summary>統計データ</summary>
[Serializable]
public class StatisticsData
{
    // 累計
    public int TotalGamesPlayed = 0;
    public float TotalPlayTime = 0f;
    public int TotalKills = 0;
    public int TotalGoldEarned = 0;

    // ベスト記録
    public float BestSurvivalTime = 0f;
    public int BestKills = 0;
    public int BestLevel = 0;

    // 敵撃破数（タイプ別、Phase 3で使用）
    public int[] KillsByEnemyType = new int[16];

    // 武器使用回数（Phase 3で使用）
    public int[] WeaponUsageCount = new int[16];
}
