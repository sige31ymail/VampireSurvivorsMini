using UnityEngine;

/// <summary>
/// ヴァンパイアサバイバー風ミニプロトタイプ v4（敵バリエーション追加）
///
/// 使い方:
///   1. このファイルを Assets/ に置く（旧版があれば上書き）
///   2. 空の GameObject にこのスクリプトをアタッチして Play
///
/// 操作:
///   WASD / 矢印キー : 移動（攻撃は自動）
///   レベルアップ時  : 3択をクリック or 1/2/3キーで選択（選択中はゲーム停止）
///   R : ゲームオーバー後にリスタート
///
/// 敵の種類:
///   チェイサー(赤・中)     : 標準。まっすぐ追ってくる
///   ランナー(橙・小)       : 速いが脆い。30秒頃から出現
///   タンク(暗赤・大)       : 遅くて硬い。倒すとXP3個分。60秒頃から
///   ダッシャー(桃・ひし形) : 白く点滅した後に突進。90秒頃から
///   ボス(紫・特大)         : 60秒ごとに出現。倒すとジェムをばら撒く
///
/// 武器一覧:
///   マジックボルト   : 最も近い敵へ弾を発射（Lvで弾数・連射UP）
///   オービットオーブ : 周囲を回転する球（Lvで個数・威力UP）
///   ダメージオーラ   : 周囲の敵に継続ダメージ（Lvで範囲・威力UP）
/// </summary>
public class VampireSurvivorsMini : MonoBehaviour
{
    public static Sprite CircleSprite { get; private set; }
    public static Sprite SquareSprite { get; private set; }

    void Start()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1f; // 一時停止中のリスタート対策
        CreateSprites();
        SetupCamera();

        // AudioManager（シーンをまたいで永続）
        if (AudioManager.Instance == null)
            new GameObject("AudioManager").AddComponent<AudioManager>();
        else
            AudioManager.Instance.RestartBgm();

        // 背景
        new GameObject("Background").AddComponent<Background>();

        // プレイヤー生成
        var playerGo = new GameObject("Player");
        var sr = playerGo.AddComponent<SpriteRenderer>();
        sr.sprite = CircleSprite;
        sr.color = new Color(0.3f, 0.8f, 1f);
        sr.sortingOrder = 10;
        playerGo.transform.localScale = Vector3.one * 0.6f;
        var player = playerGo.AddComponent<Player>();

        // スポナー生成
        var spawnerGo = new GameObject("Spawner");
        var spawner = spawnerGo.AddComponent<EnemySpawner>();
        spawner.player = player;

        // UI
        var uiGo = new GameObject("UI");
        uiGo.AddComponent<GameUI>().player = player;

        GameState.Reset();
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
}
