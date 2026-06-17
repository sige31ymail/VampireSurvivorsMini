using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>タイトル画面UI（TitleSceneに配置）</summary>
public class TitleScreen : MonoBehaviour
{
    GUIStyle titleStyle, subtitleStyle, buttonStyle, smallButtonStyle, infoStyle, statsStyle;
    SettingsMenu settingsMenu;
    UpgradeShopUI upgradeShopUI;
    bool showStats;

    void Start()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        // SaveSystem の初期化（タイトル画面から開始した場合）
        if (SaveSystem.Instance == null)
            new GameObject("SaveSystem").AddComponent<SaveSystem>();

        // AudioManager の初期化
        if (AudioManager.Instance == null)
            new GameObject("AudioManager").AddComponent<AudioManager>();

        // MetaProgressionManager の初期化
        if (MetaProgressionManager.Instance == null)
            new GameObject("MetaProgressionManager").AddComponent<MetaProgressionManager>();

        // UnlockManager の初期化
        if (UnlockManager.Instance == null)
            new GameObject("UnlockManager").AddComponent<UnlockManager>();

        // UIコンポーネントを追加
        settingsMenu = gameObject.AddComponent<SettingsMenu>();
        upgradeShopUI = gameObject.AddComponent<UpgradeShopUI>();
    }

    void OnGUI()
    {
        if (titleStyle == null) InitStyles();

        // 設定メニューまたはアップグレードショップが表示中は他のUIを描画しない
        if (settingsMenu != null && settingsMenu.IsVisible) return;
        if (upgradeShopUI != null && upgradeShopUI.IsVisible) return;

        // 統計画面
        if (showStats)
        {
            DrawStatsScreen();
            return;
        }

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        GUI.Label(new Rect(0, cy - 150, Screen.width, 90), "Vampire Survivors Mini", titleStyle);
        GUI.Label(new Rect(0, cy - 60,  Screen.width, 40), "生き残れ、強くなれ", subtitleStyle);

        float btnW = 220f, btnH = 64f;
        float smallBtnW = 100f, smallBtnH = 42f;
        float btnGap = 10f;

        // Play ボタン
        if (GUI.Button(new Rect(cx - btnW / 2f, cy + 10, btnW, btnH), "PLAY", buttonStyle))
            SceneManager.LoadScene("SampleScene");

        // Upgrade Shop ボタン（大きめ）
        float shopBtnW = 180f, shopBtnH = 50f;
        float shopBtnY = cy + 10 + btnH + 15;
        if (GUI.Button(new Rect(cx - shopBtnW / 2f, shopBtnY, shopBtnW, shopBtnH), "Upgrades", buttonStyle))
        {
            upgradeShopUI?.Show();
        }

        // ゴールド表示
        int gold = MetaProgressionManager.Instance?.Gold ?? 0;
        var goldStyle = new GUIStyle(infoStyle);
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        goldStyle.fontSize = 16;
        GUI.Label(new Rect(0, shopBtnY + shopBtnH + 5, Screen.width, 25), $"Gold: {gold}", goldStyle);

        // Settings, Stats ボタン
        float smallBtnY = shopBtnY + shopBtnH + 35;
        float totalSmallW = smallBtnW * 2 + btnGap;
        float smallBtnX = cx - totalSmallW / 2f;

        if (GUI.Button(new Rect(smallBtnX, smallBtnY, smallBtnW, smallBtnH), "Settings", smallButtonStyle))
        {
            settingsMenu?.Show();
        }

        if (GUI.Button(new Rect(smallBtnX + smallBtnW + btnGap, smallBtnY, smallBtnW, smallBtnH), "Stats", smallButtonStyle))
        {
            showStats = true;
        }

        // 操作説明
        float infoY = smallBtnY + smallBtnH + 20;
        GUI.Label(new Rect(0, infoY, Screen.width, 24), "WASD / 矢印キー で移動　攻撃は自動", infoStyle);
        GUI.Label(new Rect(0, infoY + 24, Screen.width, 24), "ESC でポーズ　レベルアップ時: 1/2/3 キーで選択", infoStyle);

        // バージョン表示
        GUI.Label(new Rect(10, Screen.height - 30, 200, 25), "v0.6.0 (Phase 2)", infoStyle);
    }

    void DrawStatsScreen()
    {
        // 背景オーバーレイ
        GUI.color = new Color(0, 0, 0, 0.9f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float panelW = Mathf.Min(400f, Screen.width - 40f);
        float panelX = cx - panelW / 2f;
        float startY = Screen.height * 0.12f;

        // タイトル
        var headerStyle = new GUIStyle(titleStyle);
        headerStyle.fontSize = 36;
        GUI.Label(new Rect(0, startY, Screen.width, 50), "STATISTICS", headerStyle);

        float y = startY + 70;
        float rowH = 32f;

        if (SaveSystem.Instance != null)
        {
            var stats = SaveSystem.Instance.Statistics;

            DrawStatRow(panelX, y, panelW, "Total Games", $"{stats.TotalGamesPlayed}"); y += rowH;
            DrawStatRow(panelX, y, panelW, "Total Play Time", FormatTime(stats.TotalPlayTime)); y += rowH;
            DrawStatRow(panelX, y, panelW, "Total Kills", $"{stats.TotalKills}"); y += rowH;
            DrawStatRow(panelX, y, panelW, "Total Gold", $"{stats.TotalGoldEarned}"); y += rowH + 15;

            // ベスト記録
            var sectionStyle = new GUIStyle(statsStyle);
            sectionStyle.fontStyle = FontStyle.Bold;
            sectionStyle.normal.textColor = new Color(0.9f, 0.8f, 0.3f);
            GUI.Label(new Rect(panelX, y, panelW, rowH), "Best Records", sectionStyle);
            y += rowH + 5;

            DrawStatRow(panelX, y, panelW, "Best Survival", FormatTime(stats.BestSurvivalTime)); y += rowH;
            DrawStatRow(panelX, y, panelW, "Best Kills", $"{stats.BestKills}"); y += rowH;
            DrawStatRow(panelX, y, panelW, "Best Level", $"{stats.BestLevel}"); y += rowH;
        }
        else
        {
            GUI.Label(new Rect(panelX, y, panelW, rowH), "No statistics yet", statsStyle);
        }

        // 戻るボタン
        float btnW = 120f, btnH = 45f;
        if (GUI.Button(new Rect(cx - btnW / 2f, Screen.height - 100, btnW, btnH), "Back", smallButtonStyle))
        {
            showStats = false;
        }
    }

    void DrawStatRow(float x, float y, float width, string label, string value)
    {
        GUI.Label(new Rect(x, y, width * 0.6f, 30), label, statsStyle);

        var valueStyle = new GUIStyle(statsStyle);
        valueStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(x + width * 0.6f, y, width * 0.4f, 30), value, valueStyle);
    }

    string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.RoundToInt(seconds);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int secs = totalSeconds % 60;

        if (hours > 0)
            return $"{hours}h {minutes}m {secs}s";
        else if (minutes > 0)
            return $"{minutes}m {secs}s";
        else
            return $"{secs}s";
    }

    void InitStyles()
    {
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 52,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);

        subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter
        };
        subtitleStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold
        };

        smallButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };

        infoStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter
        };
        infoStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);

        statsStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft
        };
        statsStyle.normal.textColor = Color.white;
    }
}
