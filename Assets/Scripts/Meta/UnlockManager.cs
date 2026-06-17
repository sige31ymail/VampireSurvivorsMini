using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アンロック管理
/// 武器・キャラクター・ステージの解放条件を管理
/// </summary>
public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance { get; private set; }

    public static event Action<UnlockableItem> OnItemUnlocked;

    // 武器アンロック定義（ID 0は初期解放）
    public static readonly UnlockableItem[] Weapons = new UnlockableItem[]
    {
        new UnlockableItem(0, "マジックボルト", "初期武器", UnlockCondition.None, 0),
        new UnlockableItem(1, "オービットオーブ", "累計100キルで解放", UnlockCondition.TotalKills, 100),
        new UnlockableItem(2, "ダメージオーラ", "累計300キルで解放", UnlockCondition.TotalKills, 300),
        new UnlockableItem(3, "クロスボルト", "累計500キルで解放", UnlockCondition.TotalKills, 500),
        // Phase 3で追加予定
        new UnlockableItem(4, "ナイフ", "5分生存で解放", UnlockCondition.SurviveTime, 300),
        new UnlockableItem(5, "斧", "累計1000キルで解放", UnlockCondition.TotalKills, 1000),
        new UnlockableItem(6, "ムチ", "レベル20到達で解放", UnlockCondition.ReachLevel, 20),
        new UnlockableItem(7, "聖書", "累計10ゲームプレイで解放", UnlockCondition.TotalGames, 10),
    };

    // キャラクターアンロック定義（ID 0は初期解放）
    public static readonly UnlockableItem[] Characters = new UnlockableItem[]
    {
        new UnlockableItem(0, "ウィザード", "初期キャラ", UnlockCondition.None, 0),
        new UnlockableItem(1, "騎士", "累計500キルで解放", UnlockCondition.TotalKills, 500),
        new UnlockableItem(2, "ローグ", "10分生存で解放", UnlockCondition.SurviveTime, 600),
        new UnlockableItem(3, "クレリック", "累計30ゲームプレイで解放", UnlockCondition.TotalGames, 30),
    };

    // ステージアンロック定義（ID 0は初期解放）
    public static readonly UnlockableItem[] Stages = new UnlockableItem[]
    {
        new UnlockableItem(0, "草原", "初期ステージ", UnlockCondition.None, 0),
        new UnlockableItem(1, "森", "累計1000キルで解放", UnlockCondition.TotalKills, 1000),
        new UnlockableItem(2, "墓地", "15分生存で解放", UnlockCondition.SurviveTime, 900),
        new UnlockableItem(3, "城", "累計50ゲームプレイで解放", UnlockCondition.TotalGames, 50),
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>武器がアンロック済みか</summary>
    public bool IsWeaponUnlocked(int id)
    {
        if (SaveSystem.Instance == null) return id == 0;
        var unlocks = SaveSystem.Instance.MetaProgress.UnlockedWeapons;
        return id >= 0 && id < unlocks.Length && unlocks[id];
    }

    /// <summary>キャラクターがアンロック済みか</summary>
    public bool IsCharacterUnlocked(int id)
    {
        if (SaveSystem.Instance == null) return id == 0;
        var unlocks = SaveSystem.Instance.MetaProgress.UnlockedCharacters;
        return id >= 0 && id < unlocks.Length && unlocks[id];
    }

    /// <summary>ステージがアンロック済みか</summary>
    public bool IsStageUnlocked(int id)
    {
        if (SaveSystem.Instance == null) return id == 0;
        var unlocks = SaveSystem.Instance.MetaProgress.UnlockedStages;
        return id >= 0 && id < unlocks.Length && unlocks[id];
    }

    /// <summary>ゲーム終了時にアンロック条件をチェック</summary>
    public List<UnlockableItem> CheckUnlocks()
    {
        var newUnlocks = new List<UnlockableItem>();
        if (SaveSystem.Instance == null) return newUnlocks;

        var stats = SaveSystem.Instance.Statistics;
        var meta = SaveSystem.Instance.MetaProgress;

        // 武器チェック
        foreach (var weapon in Weapons)
        {
            if (!meta.UnlockedWeapons[weapon.Id] && CheckCondition(weapon, stats))
            {
                meta.UnlockedWeapons[weapon.Id] = true;
                newUnlocks.Add(weapon);
                OnItemUnlocked?.Invoke(weapon);
            }
        }

        // キャラクターチェック
        foreach (var character in Characters)
        {
            if (character.Id < meta.UnlockedCharacters.Length &&
                !meta.UnlockedCharacters[character.Id] && CheckCondition(character, stats))
            {
                meta.UnlockedCharacters[character.Id] = true;
                newUnlocks.Add(character);
                OnItemUnlocked?.Invoke(character);
            }
        }

        // ステージチェック
        foreach (var stage in Stages)
        {
            if (stage.Id < meta.UnlockedStages.Length &&
                !meta.UnlockedStages[stage.Id] && CheckCondition(stage, stats))
            {
                meta.UnlockedStages[stage.Id] = true;
                newUnlocks.Add(stage);
                OnItemUnlocked?.Invoke(stage);
            }
        }

        if (newUnlocks.Count > 0)
        {
            SaveSystem.Instance.SaveMetaProgress();
        }

        return newUnlocks;
    }

    bool CheckCondition(UnlockableItem item, StatisticsData stats)
    {
        return item.Condition switch
        {
            UnlockCondition.None => true,
            UnlockCondition.TotalKills => stats.TotalKills >= item.RequiredValue,
            UnlockCondition.TotalGames => stats.TotalGamesPlayed >= item.RequiredValue,
            UnlockCondition.SurviveTime => stats.BestSurvivalTime >= item.RequiredValue,
            UnlockCondition.ReachLevel => stats.BestLevel >= item.RequiredValue,
            UnlockCondition.TotalGold => stats.TotalGoldEarned >= item.RequiredValue,
            _ => false
        };
    }

    /// <summary>アンロック進捗を取得（0.0〜1.0）</summary>
    public float GetUnlockProgress(UnlockableItem item)
    {
        if (SaveSystem.Instance == null) return 0f;
        var stats = SaveSystem.Instance.Statistics;

        float current = item.Condition switch
        {
            UnlockCondition.None => item.RequiredValue,
            UnlockCondition.TotalKills => stats.TotalKills,
            UnlockCondition.TotalGames => stats.TotalGamesPlayed,
            UnlockCondition.SurviveTime => stats.BestSurvivalTime,
            UnlockCondition.ReachLevel => stats.BestLevel,
            UnlockCondition.TotalGold => stats.TotalGoldEarned,
            _ => 0
        };

        return Mathf.Clamp01(current / item.RequiredValue);
    }

    /// <summary>解放済み武器のリストを取得</summary>
    public List<UnlockableItem> GetUnlockedWeapons()
    {
        var list = new List<UnlockableItem>();
        foreach (var w in Weapons)
        {
            if (IsWeaponUnlocked(w.Id)) list.Add(w);
        }
        return list;
    }

    /// <summary>解放済みキャラクターのリストを取得</summary>
    public List<UnlockableItem> GetUnlockedCharacters()
    {
        var list = new List<UnlockableItem>();
        foreach (var c in Characters)
        {
            if (IsCharacterUnlocked(c.Id)) list.Add(c);
        }
        return list;
    }
}

/// <summary>アンロック条件タイプ</summary>
public enum UnlockCondition
{
    None,           // 初期解放
    TotalKills,     // 累計キル数
    TotalGames,     // 累計ゲーム数
    SurviveTime,    // 最長生存時間（秒）
    ReachLevel,     // 到達レベル
    TotalGold       // 累計ゴールド
}

/// <summary>アンロック可能アイテム定義</summary>
[Serializable]
public class UnlockableItem
{
    public int Id;
    public string Name;
    public string Description;
    public UnlockCondition Condition;
    public float RequiredValue;

    public UnlockableItem(int id, string name, string desc, UnlockCondition condition, float required)
    {
        Id = id;
        Name = name;
        Description = desc;
        Condition = condition;
        RequiredValue = required;
    }
}
