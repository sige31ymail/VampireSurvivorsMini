using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // 新Input System使用時
#endif

/// <summary>プレイヤー：移動・武器管理・HP・レベル管理</summary>
public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int maxHp = 100;
    public int hp;

    [Header("成長")]
    public int level = 1;
    public int xp;
    public int xpToNext = 5;

    public readonly List<Weapon> weapons = new List<Weapon>();

    // レベルアップ時の獲得表示用
    public string lastUpgradeText = "";
    public float lastUpgradeTime = -10f;

    // レベルアップ3択
    public int pendingChoices;                  // 未消化のレベルアップ回数
    public List<UpgradeOption> currentOptions;  // 表示中の選択肢（null=非表示）

    float invincibleTimer; // 被弾後の無敵時間

    [Header("パッシブ")]
    public float magnetRange = 2.5f;
    public int armor = 0;
    public float critChance = 0f;
    public float regenPerSec = 0f;
    float regenTimer;

    [Header("キャラクター固有")]
    public CharacterData characterData;
    public float attackMultiplier = 1f;
    public float lifesteal = 0f;

    void Awake()
    {
        // キャラクターデータを適用
        ApplyCharacterData();

        // メタプログレッションボーナスを適用
        ApplyMetaProgressionBonuses();

        hp = maxHp;

        // 初期武器（キャラクターデータから）
        AddStartingWeapon();
    }

    /// <summary>選択されたキャラクターのデータを適用</summary>
    void ApplyCharacterData()
    {
        characterData = CharacterSelection.GetSelectedData();

        maxHp = characterData.BaseMaxHp;
        moveSpeed = characterData.BaseMoveSpeed;
        attackMultiplier = characterData.BaseAttackMult;
        critChance = characterData.BaseCritChance;
        regenPerSec = characterData.BaseHpRegen;
        lifesteal = characterData.BaseLifesteal;

        // キャラクターの色を適用
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = characterData.SpriteColor;
        }
    }

    /// <summary>キャラクター固有の初期武器を追加</summary>
    void AddStartingWeapon()
    {
        if (characterData.StartingWeapon == null)
        {
            weapons.Add(new BoltWeapon());
            return;
        }

        var weapon = System.Activator.CreateInstance(characterData.StartingWeapon) as Weapon;
        if (weapon != null)
            weapons.Add(weapon);
        else
            weapons.Add(new BoltWeapon());
    }

    /// <summary>メタプログレッションの永続ボーナスを適用</summary>
    void ApplyMetaProgressionBonuses()
    {
        var meta = MetaProgressionManager.Instance;
        if (meta == null) return;

        // 最大HP（基本値に倍率適用）
        maxHp = Mathf.RoundToInt(maxHp * meta.GetMaxHpMultiplier());

        // 移動速度
        moveSpeed *= meta.GetMoveSpeedMultiplier();

        // 磁石範囲
        magnetRange *= meta.GetMagnetMultiplier();

        // アーマー
        armor += meta.GetArmorBonus();

        // リジェネ
        regenPerSec += meta.GetRegenBonus();
    }

    void Update()
    {
        if (GameState.GameOver) return;

        // レベルアップの選択待ち中はゲームを一時停止
        if (pendingChoices > 0)
        {
            if (currentOptions == null) GenerateOptions();
            Time.timeScale = 0f;
            return;
        }

        GameState.ElapsedTime += Time.deltaTime;
        Move();

        foreach (var w in weapons)
            w.Tick(this, Time.deltaTime);

        if (invincibleTimer > 0f) invincibleTimer -= Time.deltaTime;

        if (regenPerSec > 0f)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= 1f)
            {
                regenTimer -= 1f;
                hp = Mathf.Min(maxHp, hp + Mathf.RoundToInt(regenPerSec));
            }
        }
    }

    void Move()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        float x = 0f, y = 0f;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    y += 1f;
        }
#else
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
#endif
        var dir = new Vector3(x, y, 0f).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (invincibleTimer > 0f || GameState.GameOver) return;
        hp -= Mathf.Max(1, damage - armor); // 最低1ダメージ
        invincibleTimer = 0.5f;
        AudioManager.PlayHit();
        if (hp <= 0)
        {
            hp = 0;
            GameState.GameOver = true;
            AudioManager.PlayGameOver();
        }
    }

    public int RollDamage(int baseDamage)
    {
        // キャラクター固有の攻撃倍率を適用
        float charMult = attackMultiplier;

        // メタプログレッションの攻撃力ボーナスを適用
        float metaMult = MetaProgressionManager.Instance?.GetAttackMultiplier() ?? 1f;

        int boostedDamage = Mathf.RoundToInt(baseDamage * charMult * metaMult);

        return critChance > 0f && Random.value < critChance ? boostedDamage * 2 : boostedDamage;
    }

    /// <summary>ダメージを与えた時にライフスティールを適用</summary>
    public void OnDealDamage(int damage)
    {
        if (lifesteal > 0f && hp < maxHp)
        {
            int heal = Mathf.Max(1, Mathf.RoundToInt(damage * lifesteal));
            hp = Mathf.Min(maxHp, hp + heal);
        }
    }

    public void GainXp(int amount)
    {
        // メタプログレッションのXPボーナスを適用
        float xpMult = MetaProgressionManager.Instance?.GetXpGainMultiplier() ?? 1f;
        int boostedAmount = Mathf.RoundToInt(amount * xpMult);

        xp += boostedAmount;
        while (xp >= xpToNext)
        {
            xp -= xpToNext;
            level++;
            xpToNext = 5 + level * 3;
            pendingChoices++; // 3択を表示（複数レベルアップ時は連続で表示）
            AudioManager.PlayLevelUp();
        }
    }

    /// <summary>選択肢プールから3つ抽選して表示用リストを作る</summary>
    void GenerateOptions()
    {
        var pool = new List<UpgradeOption>();

        // 所持武器の強化
        foreach (var w in weapons)
        {
            if (w.IsMaxLevel) continue;
            var captured = w; // クロージャ用にキャプチャ
            pool.Add(new UpgradeOption(
                $"{w.Name} Lv{w.level + 1}", w.Description,
                p => captured.LevelUp(p)));
        }

        // 未所持の新武器
        var ownedTypes = weapons.Select(w => w.GetType()).ToList();

        // 基本武器（常に選択可能）
        TryAddWeaponOption<OrbitWeapon>(pool, ownedTypes);
        TryAddWeaponOption<AuraWeapon>(pool, ownedTypes);
        TryAddWeaponOption<CrossBoltWeapon>(pool, ownedTypes);
        TryAddWeaponOption<KnifeWeapon>(pool, ownedTypes);
        TryAddWeaponOption<AxeWeapon>(pool, ownedTypes);
        TryAddWeaponOption<WhipWeapon>(pool, ownedTypes);
        TryAddWeaponOption<LightningWeapon>(pool, ownedTypes);
        TryAddWeaponOption<BibleWeapon>(pool, ownedTypes);
        TryAddWeaponOption<GarlicWeapon>(pool, ownedTypes);
        TryAddWeaponOption<FireWandWeapon>(pool, ownedTypes);
        TryAddWeaponOption<BoomerangWeapon>(pool, ownedTypes);

        // ボルトは初期武器として持っていない場合のみ
        if (!ownedTypes.Contains(typeof(BoltWeapon)))
            TryAddWeaponOption<BoltWeapon>(pool, ownedTypes);

        // 汎用強化（武器が全部MAXでも選択肢が3つ揃うようにする）
        pool.Add(new UpgradeOption("HP回復", "HPを30回復する",
            p => p.hp = Mathf.Min(p.maxHp, p.hp + 30)));
        pool.Add(new UpgradeOption("最大HP +20", "最大HPが20増える（その分回復）",
            p => { p.maxHp += 20; p.hp += 20; }));
        pool.Add(new UpgradeOption("移動速度 UP", "移動速度が8%上がる",
            p => p.moveSpeed *= 1.08f));

        // パッシブアイテム
        if (magnetRange < 8f)
            pool.Add(new UpgradeOption("磁石", "XP吸引範囲が50%広がる（最大3回）",
                p => p.magnetRange = Mathf.Min(8f, p.magnetRange * 1.5f)));
        if (armor < 25)
            pool.Add(new UpgradeOption("アーマー", "被ダメージを5軽減する（最大25）",
                p => p.armor = Mathf.Min(25, p.armor + 5)));
        if (critChance < 0.5f)
            pool.Add(new UpgradeOption("クリティカル", "攻撃の10%が2倍ダメージ（最大50%）",
                p => p.critChance = Mathf.Min(0.5f, p.critChance + 0.1f)));
        if (regenPerSec < 5f)
            pool.Add(new UpgradeOption("リジェネ", "毎秒1HP自動回復する（最大5/秒）",
                p => p.regenPerSec = Mathf.Min(5f, p.regenPerSec + 1f)));

        // シャッフルして3つ選出
        currentOptions = pool.OrderBy(_ => Random.value).Take(3).ToList();
    }

    /// <summary>選択肢を確定する（GameUIから呼ばれる）</summary>
    public void ChooseOption(int index)
    {
        if (currentOptions == null || index < 0 || index >= currentOptions.Count) return;

        var opt = currentOptions[index];
        opt.apply(this);
        ShowUpgrade(opt.title);

        currentOptions = null;
        pendingChoices--;
        if (pendingChoices <= 0)
        {
            pendingChoices = 0;
            Time.timeScale = 1f; // ゲーム再開
        }
        // pendingChoicesが残っていれば次フレームで再度3択が出る
    }

    void ShowUpgrade(string text)
    {
        lastUpgradeText = text;
        lastUpgradeTime = Time.time;
    }

    /// <summary>武器を選択肢プールに追加（未所持の場合のみ）</summary>
    void TryAddWeaponOption<T>(List<UpgradeOption> pool, List<System.Type> ownedTypes) where T : Weapon, new()
    {
        if (ownedTypes.Contains(typeof(T))) return;

        var w = new T();
        pool.Add(new UpgradeOption("新武器: " + w.Name, w.Description,
            p => p.weapons.Add(new T())));
    }
}
