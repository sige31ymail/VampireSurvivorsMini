using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ヴァンパイアサバイバー風ミニプロトタイプ v8（Phase 5: 最適化・QA完了）
///
/// 使い方:
///   1. このファイルを Assets/ に置く（旧版があれば上書き）
///   2. 空の GameObject にこのスクリプトをアタッチして Play
///
/// 操作:
///   WASD / 矢印キー : 移動（攻撃は自動）
///   ESC            : ポーズメニュー（設定へアクセス可能）
///   Tab            : チュートリアルスキップ
///   レベルアップ時  : 3択をクリック or 1/2/3キーで選択
///   R              : ゲームオーバー後にリスタート
///   F3             : パフォーマンスモニター（デバッグ）
///
/// 実装済み機能:
///   Phase 1: オブジェクトプール、セーブ、設定、ポーズ
///   Phase 2: メタプログレッション、アンロック、統計
///   Phase 3: 12武器、16敵、6キャラ、4ステージ
///   Phase 4: VFX、チュートリアル、実績、状況BGM
///   Phase 5: 空間ハッシュ最適化、視野外カリング、QA
/// </summary>
public class VampireSurvivorsMini : MonoBehaviour
{
    public static Sprite CircleSprite { get; private set; }
    public static Sprite SquareSprite { get; private set; }

    // ゲーム終了時のアンロック通知用
    static List<UnlockableItem> pendingUnlocks = new List<UnlockableItem>();

    void Start()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1f; // 一時停止中のリスタート対策
        CreateSprites();
        SetupCamera();

        // === Core Systems ===

        // SaveSystem（シーンをまたいで永続）
        if (SaveSystem.Instance == null)
            new GameObject("SaveSystem").AddComponent<SaveSystem>();

        // AudioManager（シーンをまたいで永続）
        if (AudioManager.Instance == null)
            new GameObject("AudioManager").AddComponent<AudioManager>();
        else
            AudioManager.Instance.RestartBgm();

        // MetaProgressionManager（シーンをまたいで永続）
        if (MetaProgressionManager.Instance == null)
            new GameObject("MetaProgressionManager").AddComponent<MetaProgressionManager>();

        // UnlockManager（シーンをまたいで永続）
        if (UnlockManager.Instance == null)
            new GameObject("UnlockManager").AddComponent<UnlockManager>();

        // AchievementManager（シーンをまたいで永続）
        if (AchievementManager.Instance == null)
            new GameObject("AchievementManager").AddComponent<AchievementManager>();

        // 設定を適用
        ApplySavedSettings();

        // ObjectPool（シーン単位）
        var poolGo = new GameObject("ObjectPool");
        poolGo.AddComponent<ObjectPool>();

        // プールの事前生成（パフォーマンス向上）
        PrewarmPools();

        // セッションゴールドをリセット
        GoldCoin.ResetSessionGold();

        // === Stage Manager ===
        var stageGo = new GameObject("StageManager");
        stageGo.AddComponent<StageManager>();

        // === Background（StageManagerが設定するので不要だが互換性のため残す） ===
        // new GameObject("Background").AddComponent<Background>();
        new GameObject("AmbientFx").AddComponent<AmbientFx>();

        // === Screen Vignette（四隅を暗くして空気感を出す） ===
        new GameObject("ScreenVignette").AddComponent<ScreenVignette>();

        // === Player ===
        var playerGo = new GameObject("Player");
        playerGo.transform.localScale = Vector3.one * 0.6f;
        var player = playerGo.AddComponent<Player>();
        playerGo.AddComponent<PlayerVisuals>();

        // === Spawner ===
        var spawnerGo = new GameObject("Spawner");
        var spawner = spawnerGo.AddComponent<EnemySpawner>();
        spawner.player = player;

        // === UI ===
        var uiGo = new GameObject("UI");
        uiGo.AddComponent<GameUI>().player = player;

        // === Pause Menu ===
        var pauseGo = new GameObject("PauseMenu");
        pauseGo.AddComponent<PauseMenu>().player = player;

        // === Tutorial ===
        var tutorialGo = new GameObject("TutorialManager");
        tutorialGo.AddComponent<TutorialManager>();

        // === VFX Manager ===
        var vfxGo = new GameObject("VfxManager");
        vfxGo.AddComponent<VfxManager>();

        // === Performance Monitor (Debug) ===
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        var perfGo = new GameObject("PerformanceMonitor");
        perfGo.AddComponent<PerformanceMonitor>();
        #endif

        // === Reset Game State ===
        GameState.Reset();
        pendingUnlocks.Clear();
    }

    void Update()
    {
        // 空間ハッシュを毎フレーム更新（衝突判定の高速化）
        if (!GameState.GameOver)
        {
            GameState.UpdateSpatialHash();
        }
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.gameObject.AddComponent<CameraFollow>();
    }

    void CreateSprites()
    {
        if (CircleSprite != null) return;

        const int size = 64;

        // 円
        var circleTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), center);
            circleTex.SetPixel(x, y, d <= radius ? Color.white : Color.clear);
        }
        circleTex.Apply();
        CircleSprite = Sprite.Create(circleTex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);

        // 四角
        var squareTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var white = new Color32[size * size];
        for (int i = 0; i < white.Length; i++) white[i] = Color.white;
        squareTex.SetPixels32(white);
        squareTex.Apply();
        SquareSprite = Sprite.Create(squareTex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
    }

    void ApplySavedSettings()
    {
        if (SaveSystem.Instance == null) return;

        var settings = SaveSystem.Instance.Settings;

        // 音量を適用
        if (AudioManager.Instance != null)
        {
            var sources = AudioManager.Instance.GetComponents<AudioSource>();
            if (sources.Length > 0) sources[0].volume = settings.BgmVolume;
            if (sources.Length > 1) sources[1].volume = settings.SeVolume;
        }
    }

    void PrewarmPools()
    {
        if (ObjectPool.Instance == null) return;

        // 弾：同時に画面上に存在しうる最大数
        ObjectPool.Instance.Prewarm<Projectile>(50, go =>
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CircleSprite;
            sr.sortingOrder = 8;
        });

        // 経験値ジェム：ボス討伐時などで大量にスポーン
        ObjectPool.Instance.Prewarm<XpGem>(30, go =>
        {
            go.transform.localScale = Vector3.one * 0.25f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CircleSprite;
            sr.color = new Color(0.4f, 1f, 0.5f);
            sr.sortingOrder = 3;
        });

        // ゴールドコイン
        ObjectPool.Instance.Prewarm<GoldCoin>(20, go =>
        {
            go.transform.localScale = Vector3.one * 0.22f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CircleSprite;
            sr.color = new Color(1f, 0.85f, 0.2f);
            sr.sortingOrder = 4;
        });

        // ダメージポップアップ
        ObjectPool.Instance.Prewarm<DamagePopup>(20);

        // 死亡パーティクル
        ObjectPool.Instance.Prewarm<DeathParticle>(60, go =>
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SquareSprite;
            sr.sortingOrder = 7;
        });
    }

    void OnDestroy()
    {
        // ゲーム終了時の処理
        if (SaveSystem.Instance != null && GameState.ElapsedTime > 0)
        {
            var player = FindObjectOfType<Player>();
            int level = player != null ? player.level : 1;

            // セッション中に獲得したゴールドを永続化
            int sessionGold = GoldCoin.SessionGold;
            if (sessionGold > 0 && MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.AddGold(sessionGold);
            }

            // 統計を記録
            SaveSystem.Instance.RecordGameResult(
                GameState.ElapsedTime,
                GameState.KillCount,
                level,
                sessionGold
            );

            // アンロック条件をチェック
            if (UnlockManager.Instance != null)
            {
                pendingUnlocks = UnlockManager.Instance.CheckUnlocks();
            }

            // 実績をチェック
            AchievementManager.Instance?.CheckAchievements();
        }
    }

    /// <summary>直前のゲームでアンロックされたアイテムを取得（タイトル画面で表示用）</summary>
    public static List<UnlockableItem> GetPendingUnlocks()
    {
        var result = new List<UnlockableItem>(pendingUnlocks);
        pendingUnlocks.Clear();
        return result;
    }
}
