using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>実績管理システム</summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    public static event Action<Achievement> OnAchievementUnlocked;

    // 実績定義
    public static readonly Achievement[] Achievements = new Achievement[]
    {
        // 撃破系
        new Achievement("first_blood", "ファーストブラッド", "最初の敵を倒す", AchievementType.Kills, 1),
        new Achievement("slayer_100", "スレイヤー", "累計100体撃破", AchievementType.Kills, 100),
        new Achievement("slayer_500", "マスタースレイヤー", "累計500体撃破", AchievementType.Kills, 500),
        new Achievement("slayer_1000", "レジェンドスレイヤー", "累計1000体撃破", AchievementType.Kills, 1000),
        new Achievement("slayer_5000", "デスブリンガー", "累計5000体撃破", AchievementType.Kills, 5000),

        // 生存系
        new Achievement("survivor_5", "サバイバー", "5分間生存", AchievementType.SurviveTime, 300),
        new Achievement("survivor_10", "ベテランサバイバー", "10分間生存", AchievementType.SurviveTime, 600),
        new Achievement("survivor_15", "エリートサバイバー", "15分間生存", AchievementType.SurviveTime, 900),
        new Achievement("survivor_20", "アルティメットサバイバー", "20分間生存", AchievementType.SurviveTime, 1200),

        // レベル系
        new Achievement("level_10", "成長中", "レベル10到達", AchievementType.Level, 10),
        new Achievement("level_20", "熟練", "レベル20到達", AchievementType.Level, 20),
        new Achievement("level_30", "マスター", "レベル30到達", AchievementType.Level, 30),
        new Achievement("level_50", "レジェンド", "レベル50到達", AchievementType.Level, 50),

        // ゴールド系
        new Achievement("gold_100", "コインコレクター", "累計100ゴールド獲得", AchievementType.Gold, 100),
        new Achievement("gold_500", "トレジャーハンター", "累計500ゴールド獲得", AchievementType.Gold, 500),
        new Achievement("gold_1000", "ゴールドラッシュ", "累計1000ゴールド獲得", AchievementType.Gold, 1000),
        new Achievement("gold_5000", "大富豪", "累計5000ゴールド獲得", AchievementType.Gold, 5000),

        // ゲーム回数系
        new Achievement("games_10", "常連", "10回プレイ", AchievementType.Games, 10),
        new Achievement("games_50", "ヘビープレイヤー", "50回プレイ", AchievementType.Games, 50),
        new Achievement("games_100", "熱狂的ファン", "100回プレイ", AchievementType.Games, 100),

        // ボス系
        new Achievement("boss_first", "ボスハンター", "初めてボスを倒す", AchievementType.BossKills, 1),
        new Achievement("boss_10", "ボススレイヤー", "ボスを10体倒す", AchievementType.BossKills, 10),

        // 武器系
        new Achievement("weapons_5", "武器収集家", "5種類の武器を使用", AchievementType.WeaponsUsed, 5),
        new Achievement("weapons_10", "武器マスター", "10種類の武器を使用", AchievementType.WeaponsUsed, 10),

        // ステージ系
        new Achievement("stage_clear", "ステージクリア", "初めてステージをクリア", AchievementType.StagesCleared, 1),
        new Achievement("all_stages", "コンプリート", "全ステージクリア", AchievementType.StagesCleared, 4),
    };

    List<string> unlockedAchievements = new List<string>();
    Queue<Achievement> pendingNotifications = new Queue<Achievement>();
    float notificationTimer;
    Achievement currentNotification;

    GUIStyle notificationStyle, titleStyle;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadUnlockedAchievements();
    }

    void LoadUnlockedAchievements()
    {
        unlockedAchievements.Clear();
        string saved = PlayerPrefs.GetString("UnlockedAchievements", "");
        if (!string.IsNullOrEmpty(saved))
        {
            unlockedAchievements.AddRange(saved.Split(','));
        }
    }

    void SaveUnlockedAchievements()
    {
        PlayerPrefs.SetString("UnlockedAchievements", string.Join(",", unlockedAchievements));
        PlayerPrefs.Save();
    }

    public bool IsUnlocked(string id)
    {
        return unlockedAchievements.Contains(id);
    }

    /// <summary>実績の解除をチェック（ゲーム終了時に呼ばれる）</summary>
    public List<Achievement> CheckAchievements()
    {
        var newUnlocks = new List<Achievement>();
        if (SaveSystem.Instance == null) return newUnlocks;

        var stats = SaveSystem.Instance.Statistics;

        foreach (var achievement in Achievements)
        {
            if (IsUnlocked(achievement.Id)) continue;

            bool unlocked = achievement.Type switch
            {
                AchievementType.Kills => stats.TotalKills >= achievement.RequiredValue,
                AchievementType.SurviveTime => stats.BestSurvivalTime >= achievement.RequiredValue,
                AchievementType.Level => stats.BestLevel >= achievement.RequiredValue,
                AchievementType.Gold => stats.TotalGoldEarned >= achievement.RequiredValue,
                AchievementType.Games => stats.TotalGamesPlayed >= achievement.RequiredValue,
                AchievementType.BossKills => stats.TotalBossKills >= achievement.RequiredValue,
                AchievementType.WeaponsUsed => stats.UniqueWeaponsUsed >= achievement.RequiredValue,
                AchievementType.StagesCleared => stats.TotalStagesCleared >= achievement.RequiredValue,
                _ => false
            };

            if (unlocked)
            {
                unlockedAchievements.Add(achievement.Id);
                newUnlocks.Add(achievement);
                pendingNotifications.Enqueue(achievement);
                OnAchievementUnlocked?.Invoke(achievement);
            }
        }

        if (newUnlocks.Count > 0)
        {
            SaveUnlockedAchievements();
        }

        return newUnlocks;
    }

    void Update()
    {
        // 通知表示
        if (currentNotification != null)
        {
            notificationTimer -= Time.unscaledDeltaTime;
            if (notificationTimer <= 0)
            {
                currentNotification = null;
            }
        }
        else if (pendingNotifications.Count > 0)
        {
            currentNotification = pendingNotifications.Dequeue();
            notificationTimer = 3f;
        }
    }

    void OnGUI()
    {
        if (currentNotification == null) return;

        if (notificationStyle == null)
        {
            notificationStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            notificationStyle.normal.textColor = Color.white;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        }

        // フェードイン/アウト
        float alpha = 1f;
        if (notificationTimer > 2.5f)
            alpha = 1f - (notificationTimer - 2.5f) / 0.5f;
        else if (notificationTimer < 0.5f)
            alpha = notificationTimer / 0.5f;

        float panelW = 300f;
        float panelH = 70f;
        float panelX = Screen.width - panelW - 20f;
        float panelY = 20f;

        // 背景
        GUI.color = new Color(0.1f, 0.1f, 0.15f, 0.9f * alpha);
        GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);

        // 枠線（金色）
        GUI.color = new Color(1f, 0.85f, 0.2f, alpha);
        GUI.DrawTexture(new Rect(panelX, panelY, panelW, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelX, panelY + panelH - 2, panelW, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelX, panelY, 2, panelH), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelX + panelW - 2, panelY, 2, panelH), Texture2D.whiteTexture);

        // テキスト
        GUI.color = new Color(1f, 0.85f, 0.2f, alpha);
        GUI.Label(new Rect(panelX, panelY + 12f, panelW, 24f), "実績解除！", titleStyle);

        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.Label(new Rect(panelX, panelY + 38f, panelW, 22f), currentNotification.Name, notificationStyle);

        GUI.color = Color.white;
    }

    /// <summary>解除済み実績数を取得</summary>
    public int GetUnlockedCount()
    {
        return unlockedAchievements.Count;
    }

    /// <summary>全実績数を取得</summary>
    public int GetTotalCount()
    {
        return Achievements.Length;
    }

    /// <summary>実績をリセット（デバッグ用）</summary>
    public void ResetAllAchievements()
    {
        unlockedAchievements.Clear();
        SaveUnlockedAchievements();
    }
}

public enum AchievementType
{
    Kills,
    SurviveTime,
    Level,
    Gold,
    Games,
    BossKills,
    WeaponsUsed,
    StagesCleared
}

[Serializable]
public class Achievement
{
    public string Id;
    public string Name;
    public string Description;
    public AchievementType Type;
    public float RequiredValue;

    public Achievement(string id, string name, string desc, AchievementType type, float required)
    {
        Id = id;
        Name = name;
        Description = desc;
        Type = type;
        RequiredValue = required;
    }
}
