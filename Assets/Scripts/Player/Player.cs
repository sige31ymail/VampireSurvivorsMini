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

    void Awake()
    {
        hp = maxHp;
        weapons.Add(new BoltWeapon()); // 初期武器
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
        hp -= damage;
        invincibleTimer = 0.5f;
        if (hp <= 0)
        {
            hp = 0;
            GameState.GameOver = true;
        }
    }

    public void GainXp(int amount)
    {
        xp += amount;
        while (xp >= xpToNext)
        {
            xp -= xpToNext;
            level++;
            xpToNext = 5 + level * 3;
            pendingChoices++; // 3択を表示（複数レベルアップ時は連続で表示）
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
        if (!ownedTypes.Contains(typeof(OrbitWeapon)))
        {
            var w = new OrbitWeapon();
            pool.Add(new UpgradeOption("新武器: " + w.Name, w.Description,
                p => p.weapons.Add(w)));
        }
        if (!ownedTypes.Contains(typeof(AuraWeapon)))
        {
            var w = new AuraWeapon();
            pool.Add(new UpgradeOption("新武器: " + w.Name, w.Description,
                p => p.weapons.Add(w)));
        }

        // 汎用強化（武器が全部MAXでも選択肢が3つ揃うようにする）
        pool.Add(new UpgradeOption("HP回復", "HPを30回復する",
            p => p.hp = Mathf.Min(p.maxHp, p.hp + 30)));
        pool.Add(new UpgradeOption("最大HP +20", "最大HPが20増える（その分回復）",
            p => { p.maxHp += 20; p.hp += 20; }));
        pool.Add(new UpgradeOption("移動速度 UP", "移動速度が8%上がる",
            p => p.moveSpeed *= 1.08f));

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
}
