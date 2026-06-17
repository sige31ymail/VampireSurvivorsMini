using System;
using UnityEngine;

/// <summary>
/// メタプログレッション管理
/// 永続アップグレードの購入・効果適用を管理
/// </summary>
public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance { get; private set; }

    // アップグレード定義
    public static readonly UpgradeDefinition[] Upgrades = new UpgradeDefinition[]
    {
        new UpgradeDefinition(
            UpgradeType.MaxHp, "最大HP", "最大HPが10%増加",
            maxLevel: 10, baseCost: 50, costMultiplier: 1.5f,
            effectPerLevel: 0.10f, effectFormat: "+{0}%"
        ),
        new UpgradeDefinition(
            UpgradeType.Attack, "攻撃力", "全ての攻撃ダメージが5%増加",
            maxLevel: 10, baseCost: 75, costMultiplier: 1.6f,
            effectPerLevel: 0.05f, effectFormat: "+{0}%"
        ),
        new UpgradeDefinition(
            UpgradeType.MoveSpeed, "移動速度", "移動速度が3%増加",
            maxLevel: 10, baseCost: 40, costMultiplier: 1.4f,
            effectPerLevel: 0.03f, effectFormat: "+{0}%"
        ),
        new UpgradeDefinition(
            UpgradeType.XpGain, "経験値獲得", "獲得経験値が5%増加",
            maxLevel: 10, baseCost: 60, costMultiplier: 1.5f,
            effectPerLevel: 0.05f, effectFormat: "+{0}%"
        ),
        new UpgradeDefinition(
            UpgradeType.GoldGain, "ゴールド獲得", "獲得ゴールドが10%増加",
            maxLevel: 10, baseCost: 100, costMultiplier: 1.8f,
            effectPerLevel: 0.10f, effectFormat: "+{0}%"
        ),
        new UpgradeDefinition(
            UpgradeType.Armor, "アーマー", "被ダメージを2軽減",
            maxLevel: 5, baseCost: 80, costMultiplier: 2.0f,
            effectPerLevel: 2f, effectFormat: "+{0}"
        ),
        new UpgradeDefinition(
            UpgradeType.Regen, "リジェネ", "毎秒0.5HP回復",
            maxLevel: 5, baseCost: 120, costMultiplier: 2.0f,
            effectPerLevel: 0.5f, effectFormat: "+{0}/秒"
        ),
        new UpgradeDefinition(
            UpgradeType.Magnet, "磁石範囲", "XP吸引範囲が10%増加",
            maxLevel: 5, baseCost: 45, costMultiplier: 1.5f,
            effectPerLevel: 0.10f, effectFormat: "+{0}%"
        ),
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>現在のゴールド残高</summary>
    public int Gold => SaveSystem.Instance?.MetaProgress?.Gold ?? 0;

    /// <summary>アップグレードの現在レベルを取得</summary>
    public int GetUpgradeLevel(UpgradeType type)
    {
        if (SaveSystem.Instance == null) return 0;
        var meta = SaveSystem.Instance.MetaProgress;

        return type switch
        {
            UpgradeType.MaxHp => meta.MaxHpBonus,
            UpgradeType.Attack => meta.AttackBonus,
            UpgradeType.MoveSpeed => meta.MoveSpeedBonus,
            UpgradeType.XpGain => meta.XpGainBonus,
            UpgradeType.GoldGain => meta.GoldGainBonus,
            UpgradeType.Armor => meta.ArmorBonus,
            UpgradeType.Regen => meta.RegenBonus,
            UpgradeType.Magnet => meta.MagnetBonus,
            _ => 0
        };
    }

    /// <summary>アップグレードの購入コストを計算</summary>
    public int GetUpgradeCost(UpgradeType type)
    {
        var def = GetDefinition(type);
        if (def == null) return int.MaxValue;

        int currentLevel = GetUpgradeLevel(type);
        if (currentLevel >= def.MaxLevel) return int.MaxValue;

        return Mathf.RoundToInt(def.BaseCost * Mathf.Pow(def.CostMultiplier, currentLevel));
    }

    /// <summary>アップグレードが購入可能か</summary>
    public bool CanPurchase(UpgradeType type)
    {
        int cost = GetUpgradeCost(type);
        return Gold >= cost && GetUpgradeLevel(type) < GetDefinition(type).MaxLevel;
    }

    /// <summary>アップグレードを購入</summary>
    public bool PurchaseUpgrade(UpgradeType type)
    {
        if (!CanPurchase(type)) return false;

        int cost = GetUpgradeCost(type);
        var meta = SaveSystem.Instance.MetaProgress;

        // ゴールドを消費
        meta.Gold -= cost;

        // レベルを上げる
        switch (type)
        {
            case UpgradeType.MaxHp: meta.MaxHpBonus++; break;
            case UpgradeType.Attack: meta.AttackBonus++; break;
            case UpgradeType.MoveSpeed: meta.MoveSpeedBonus++; break;
            case UpgradeType.XpGain: meta.XpGainBonus++; break;
            case UpgradeType.GoldGain: meta.GoldGainBonus++; break;
            case UpgradeType.Armor: meta.ArmorBonus++; break;
            case UpgradeType.Regen: meta.RegenBonus++; break;
            case UpgradeType.Magnet: meta.MagnetBonus++; break;
        }

        SaveSystem.Instance.SaveMetaProgress();
        return true;
    }

    /// <summary>ゴールドを追加</summary>
    public void AddGold(int amount)
    {
        if (SaveSystem.Instance == null) return;

        // ゴールド獲得ボーナスを適用
        float bonus = 1f + GetUpgradeLevel(UpgradeType.GoldGain) * 0.10f;
        int finalAmount = Mathf.RoundToInt(amount * bonus);

        SaveSystem.Instance.MetaProgress.Gold += finalAmount;
        SaveSystem.Instance.Statistics.TotalGoldEarned += finalAmount;
        SaveSystem.Instance.SaveMetaProgress();
    }

    /// <summary>アップグレード定義を取得</summary>
    public UpgradeDefinition GetDefinition(UpgradeType type)
    {
        foreach (var def in Upgrades)
        {
            if (def.Type == type) return def;
        }
        return null;
    }

    #region Effect Calculations

    /// <summary>最大HPボーナス（倍率）</summary>
    public float GetMaxHpMultiplier()
    {
        return 1f + GetUpgradeLevel(UpgradeType.MaxHp) * 0.10f;
    }

    /// <summary>攻撃力ボーナス（倍率）</summary>
    public float GetAttackMultiplier()
    {
        return 1f + GetUpgradeLevel(UpgradeType.Attack) * 0.05f;
    }

    /// <summary>移動速度ボーナス（倍率）</summary>
    public float GetMoveSpeedMultiplier()
    {
        return 1f + GetUpgradeLevel(UpgradeType.MoveSpeed) * 0.03f;
    }

    /// <summary>経験値獲得ボーナス（倍率）</summary>
    public float GetXpGainMultiplier()
    {
        return 1f + GetUpgradeLevel(UpgradeType.XpGain) * 0.05f;
    }

    /// <summary>アーマーボーナス（固定値）</summary>
    public int GetArmorBonus()
    {
        return GetUpgradeLevel(UpgradeType.Armor) * 2;
    }

    /// <summary>リジェネボーナス（毎秒回復量）</summary>
    public float GetRegenBonus()
    {
        return GetUpgradeLevel(UpgradeType.Regen) * 0.5f;
    }

    /// <summary>磁石範囲ボーナス（倍率）</summary>
    public float GetMagnetMultiplier()
    {
        return 1f + GetUpgradeLevel(UpgradeType.Magnet) * 0.10f;
    }

    #endregion
}

/// <summary>アップグレードタイプ</summary>
public enum UpgradeType
{
    MaxHp,
    Attack,
    MoveSpeed,
    XpGain,
    GoldGain,
    Armor,
    Regen,
    Magnet
}

/// <summary>アップグレード定義</summary>
[Serializable]
public class UpgradeDefinition
{
    public UpgradeType Type;
    public string Name;
    public string Description;
    public int MaxLevel;
    public int BaseCost;
    public float CostMultiplier;
    public float EffectPerLevel;
    public string EffectFormat;

    public UpgradeDefinition(UpgradeType type, string name, string desc,
        int maxLevel, int baseCost, float costMultiplier,
        float effectPerLevel, string effectFormat)
    {
        Type = type;
        Name = name;
        Description = desc;
        MaxLevel = maxLevel;
        BaseCost = baseCost;
        CostMultiplier = costMultiplier;
        EffectPerLevel = effectPerLevel;
        EffectFormat = effectFormat;
    }

    /// <summary>現在の効果を文字列で取得</summary>
    public string GetEffectText(int level)
    {
        float effect = EffectPerLevel * level;
        if (EffectFormat.Contains("%"))
            return string.Format(EffectFormat, Mathf.RoundToInt(effect * 100));
        return string.Format(EffectFormat, effect);
    }
}
