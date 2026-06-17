using UnityEngine;
using System.Collections.Generic;

/// <summary>キャラクターの種類</summary>
public enum CharacterType
{
    Knight,     // 騎士：バランス型、初期武器ナイフ
    Mage,       // 魔法使い：攻撃力高、HP低、初期武器火の杖
    Rogue,      // ローグ：移動速度高、クリティカル率高、初期武器ナイフ
    Cleric,     // クレリック：回復力高、初期武器ニンニク
    Vampire,    // ヴァンパイア：HP吸収、初期武器ムチ（アンロック必要）
    Necromancer // ネクロマンサー：敵を倒すと一時的に味方化（アンロック必要）
}

/// <summary>キャラクターデータの定義</summary>
[System.Serializable]
public class CharacterData
{
    public CharacterType Type;
    public string Name;
    public string Description;
    public Color SpriteColor;

    // 基本ステータス
    public int BaseMaxHp;
    public float BaseMoveSpeed;
    public float BaseAttackMult;
    public float BaseCritChance;
    public float BaseHpRegen;
    public float BaseLifesteal;

    // 初期武器
    public System.Type StartingWeapon;

    // アンロック条件
    public bool IsUnlockedByDefault;
    public string UnlockCondition;

    /// <summary>全キャラクターデータを取得</summary>
    public static List<CharacterData> GetAllCharacters()
    {
        return new List<CharacterData>
        {
            // 騎士 - バランス型
            new CharacterData
            {
                Type = CharacterType.Knight,
                Name = "騎士",
                Description = "バランスの取れた標準的な戦士",
                SpriteColor = new Color(0.4f, 0.5f, 0.8f),
                BaseMaxHp = 100,
                BaseMoveSpeed = 3.5f,
                BaseAttackMult = 1.0f,
                BaseCritChance = 0.05f,
                BaseHpRegen = 0f,
                BaseLifesteal = 0f,
                StartingWeapon = typeof(KnifeWeapon),
                IsUnlockedByDefault = true,
                UnlockCondition = ""
            },

            // 魔法使い - 高火力・低HP
            new CharacterData
            {
                Type = CharacterType.Mage,
                Name = "魔法使い",
                Description = "強力な魔法を扱う。HPが低い",
                SpriteColor = new Color(0.6f, 0.3f, 0.8f),
                BaseMaxHp = 70,
                BaseMoveSpeed = 3.2f,
                BaseAttackMult = 1.5f,
                BaseCritChance = 0.1f,
                BaseHpRegen = 0f,
                BaseLifesteal = 0f,
                StartingWeapon = typeof(FireWandWeapon),
                IsUnlockedByDefault = true,
                UnlockCondition = ""
            },

            // ローグ - 高速・高クリティカル
            new CharacterData
            {
                Type = CharacterType.Rogue,
                Name = "ローグ",
                Description = "素早い動きと高いクリティカル率",
                SpriteColor = new Color(0.3f, 0.6f, 0.3f),
                BaseMaxHp = 80,
                BaseMoveSpeed = 4.5f,
                BaseAttackMult = 0.9f,
                BaseCritChance = 0.25f,
                BaseHpRegen = 0f,
                BaseLifesteal = 0f,
                StartingWeapon = typeof(KnifeWeapon),
                IsUnlockedByDefault = true,
                UnlockCondition = ""
            },

            // クレリック - 回復特化
            new CharacterData
            {
                Type = CharacterType.Cleric,
                Name = "クレリック",
                Description = "回復力が高く生存力に優れる",
                SpriteColor = new Color(0.9f, 0.9f, 0.6f),
                BaseMaxHp = 90,
                BaseMoveSpeed = 3.0f,
                BaseAttackMult = 0.8f,
                BaseCritChance = 0.05f,
                BaseHpRegen = 2f,
                BaseLifesteal = 0f,
                StartingWeapon = typeof(GarlicWeapon),
                IsUnlockedByDefault = true,
                UnlockCondition = ""
            },

            // ヴァンパイア - HP吸収
            new CharacterData
            {
                Type = CharacterType.Vampire,
                Name = "ヴァンパイア",
                Description = "攻撃時にHPを吸収する",
                SpriteColor = new Color(0.5f, 0.1f, 0.2f),
                BaseMaxHp = 85,
                BaseMoveSpeed = 3.8f,
                BaseAttackMult = 1.1f,
                BaseCritChance = 0.1f,
                BaseHpRegen = -0.5f, // 自然回復なし、むしろ減る
                BaseLifesteal = 0.15f,
                StartingWeapon = typeof(WhipWeapon),
                IsUnlockedByDefault = false,
                UnlockCondition = "墓地ステージをクリア"
            },

            // ネクロマンサー - 特殊能力
            new CharacterData
            {
                Type = CharacterType.Necromancer,
                Name = "ネクロマンサー",
                Description = "倒した敵が一時的に味方になる",
                SpriteColor = new Color(0.2f, 0.2f, 0.3f),
                BaseMaxHp = 75,
                BaseMoveSpeed = 3.0f,
                BaseAttackMult = 0.85f,
                BaseCritChance = 0.05f,
                BaseHpRegen = 0f,
                BaseLifesteal = 0f,
                StartingWeapon = typeof(LightningWeapon),
                IsUnlockedByDefault = false,
                UnlockCondition = "累計1000体の敵を倒す"
            }
        };
    }

    /// <summary>特定のキャラクターデータを取得</summary>
    public static CharacterData GetCharacter(CharacterType type)
    {
        var all = GetAllCharacters();
        return all.Find(c => c.Type == type) ?? all[0];
    }
}

/// <summary>選択中のキャラクターを管理</summary>
public static class CharacterSelection
{
    public static CharacterType SelectedCharacter { get; set; } = CharacterType.Knight;

    public static CharacterData GetSelectedData()
    {
        return CharacterData.GetCharacter(SelectedCharacter);
    }
}
