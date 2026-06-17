using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>タイトル画面UI（TitleSceneに配置）</summary>
public class TitleScreen : MonoBehaviour
{
    GUIStyle titleStyle, subtitleStyle, buttonStyle, smallButtonStyle, infoStyle, statsStyle;
    GUIStyle selectedStyle, lockedStyle, charNameStyle;
    SettingsMenu settingsMenu;
    UpgradeShopUI upgradeShopUI;

    enum MenuState { Main, CharacterSelect, StageSelect, Stats }
    MenuState menuState = MenuState.Main;

    List<CharacterData> characters;
    List<StageData> stages;
    int selectedCharIndex = 0;
    int selectedStageIndex = 0;

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

        // キャラクターとステージのデータを取得
        characters = CharacterData.GetAllCharacters();
        stages = StageData.GetAllStages();

        // 現在の選択を反映
        selectedCharIndex = characters.FindIndex(c => c.Type == CharacterSelection.SelectedCharacter);
        selectedStageIndex = stages.FindIndex(s => s.Type == StageSelection.SelectedStage);
        if (selectedCharIndex < 0) selectedCharIndex = 0;
        if (selectedStageIndex < 0) selectedStageIndex = 0;
    }

    void OnGUI()
    {
        if (titleStyle == null) InitStyles();

        // 設定メニューまたはアップグレードショップが表示中は他のUIを描画しない
        if (settingsMenu != null && settingsMenu.IsVisible) return;
        if (upgradeShopUI != null && upgradeShopUI.IsVisible) return;

        switch (menuState)
        {
            case MenuState.Main:
                DrawMainMenu();
                break;
            case MenuState.CharacterSelect:
                DrawCharacterSelect();
                break;
            case MenuState.StageSelect:
                DrawStageSelect();
                break;
            case MenuState.Stats:
                DrawStatsScreen();
                break;
        }
    }

    void DrawMainMenu()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        GUI.Label(new Rect(0, cy - 180, Screen.width, 90), "Vampire Survivors Mini", titleStyle);
        GUI.Label(new Rect(0, cy - 90, Screen.width, 40), "生き残れ、強くなれ", subtitleStyle);

        // 選択中のキャラクターとステージを表示
        var charData = characters[selectedCharIndex];
        var stageData = stages[selectedStageIndex];
        string selectionInfo = $"{charData.Name} / {stageData.Name}";
        GUI.Label(new Rect(0, cy - 50, Screen.width, 30), selectionInfo, infoStyle);

        float btnW = 220f, btnH = 55f;
        float smallBtnW = 100f, smallBtnH = 42f;
        float btnGap = 10f;
        float y = cy;

        // Play ボタン
        if (GUI.Button(new Rect(cx - btnW / 2f, y, btnW, btnH), "PLAY", buttonStyle))
        {
            CharacterSelection.SelectedCharacter = charData.Type;
            StageSelection.SelectedStage = stageData.Type;
            SceneManager.LoadScene("SampleScene");
        }
        y += btnH + 10;

        // Character / Stage 選択ボタン
        float selectBtnW = 140f, selectBtnH = 45f;
        float totalSelectW = selectBtnW * 2 + btnGap;
        float selectX = cx - totalSelectW / 2f;

        if (GUI.Button(new Rect(selectX, y, selectBtnW, selectBtnH), "Character", smallButtonStyle))
        {
            menuState = MenuState.CharacterSelect;
        }
        if (GUI.Button(new Rect(selectX + selectBtnW + btnGap, y, selectBtnW, selectBtnH), "Stage", smallButtonStyle))
        {
            menuState = MenuState.StageSelect;
        }
        y += selectBtnH + 10;

        // Upgrades ボタン
        float shopBtnW = 180f, shopBtnH = 45f;
        if (GUI.Button(new Rect(cx - shopBtnW / 2f, y, shopBtnW, shopBtnH), "Upgrades", smallButtonStyle))
        {
            upgradeShopUI?.Show();
        }
        y += shopBtnH + 5;

        // ゴールド表示
        int gold = MetaProgressionManager.Instance?.Gold ?? 0;
        var goldStyle = new GUIStyle(infoStyle);
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        goldStyle.fontSize = 16;
        GUI.Label(new Rect(0, y, Screen.width, 25), $"Gold: {gold}", goldStyle);
        y += 30;

        // Settings, Stats ボタン
        float totalSmallW = smallBtnW * 2 + btnGap;
        float smallBtnX = cx - totalSmallW / 2f;

        if (GUI.Button(new Rect(smallBtnX, y, smallBtnW, smallBtnH), "Settings", smallButtonStyle))
        {
            settingsMenu?.Show();
        }

        if (GUI.Button(new Rect(smallBtnX + smallBtnW + btnGap, y, smallBtnW, smallBtnH), "Stats", smallButtonStyle))
        {
            menuState = MenuState.Stats;
        }
        y += smallBtnH + 15;

        // 操作説明
        GUI.Label(new Rect(0, y, Screen.width, 24), "WASD / 矢印キー で移動　攻撃は自動", infoStyle);
        GUI.Label(new Rect(0, y + 24, Screen.width, 24), "ESC でポーズ　レベルアップ時: 1/2/3 キーで選択", infoStyle);

        // バージョン表示
        GUI.Label(new Rect(10, Screen.height - 30, 200, 25), "v0.7.0 (Phase 3)", infoStyle);
    }

    void DrawCharacterSelect()
    {
        // 背景オーバーレイ
        GUI.color = new Color(0, 0, 0, 0.92f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float startY = Screen.height * 0.08f;

        // タイトル
        var headerStyle = new GUIStyle(titleStyle);
        headerStyle.fontSize = 36;
        GUI.Label(new Rect(0, startY, Screen.width, 50), "CHARACTER SELECT", headerStyle);

        float y = startY + 70;
        float cardW = 160f, cardH = 180f;
        float cardGap = 15f;
        int cols = Mathf.Min(characters.Count, 4);
        float totalW = cols * cardW + (cols - 1) * cardGap;
        float startX = cx - totalW / 2f;

        for (int i = 0; i < characters.Count; i++)
        {
            int row = i / 4;
            int col = i % 4;
            float x = startX + col * (cardW + cardGap);
            float cardY = y + row * (cardH + cardGap);

            DrawCharacterCard(x, cardY, cardW, cardH, characters[i], i);
        }

        // 選択中キャラの説明
        float descY = y + ((characters.Count - 1) / 4 + 1) * (cardH + cardGap) + 10;
        var selected = characters[selectedCharIndex];
        GUI.Label(new Rect(0, descY, Screen.width, 30), selected.Description, infoStyle);

        // キャラ固有ステータス
        float statY = descY + 35;
        string statText = $"HP: {selected.BaseMaxHp}  速度: {selected.BaseMoveSpeed:F1}  攻撃: {selected.BaseAttackMult:P0}  クリ率: {selected.BaseCritChance:P0}";
        GUI.Label(new Rect(0, statY, Screen.width, 25), statText, infoStyle);

        // 戻るボタン
        float btnW = 120f, btnH = 45f;
        if (GUI.Button(new Rect(cx - btnW / 2f, Screen.height - 80, btnW, btnH), "Back", smallButtonStyle))
        {
            CharacterSelection.SelectedCharacter = characters[selectedCharIndex].Type;
            menuState = MenuState.Main;
        }
    }

    void DrawCharacterCard(float x, float y, float w, float h, CharacterData data, int index)
    {
        bool isSelected = index == selectedCharIndex;
        bool isUnlocked = data.IsUnlockedByDefault || IsCharacterUnlocked(data.Type);

        // カード背景
        Color bgColor = isSelected ? new Color(0.3f, 0.5f, 0.7f, 0.8f) :
                        isUnlocked ? new Color(0.2f, 0.2f, 0.25f, 0.8f) :
                                     new Color(0.15f, 0.15f, 0.15f, 0.6f);
        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // キャラクターアイコン（色付き円）
        float iconSize = 60f;
        float iconX = x + (w - iconSize) / 2f;
        float iconY = y + 20f;

        if (isUnlocked)
        {
            GUI.color = data.SpriteColor;
        }
        else
        {
            GUI.color = new Color(0.3f, 0.3f, 0.3f);
        }
        GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // 名前
        var nameStyle = isUnlocked ? charNameStyle : lockedStyle;
        GUI.Label(new Rect(x, iconY + iconSize + 10, w, 25), data.Name, nameStyle);

        // ロック表示
        if (!isUnlocked)
        {
            var lockStyle = new GUIStyle(infoStyle);
            lockStyle.fontSize = 11;
            lockStyle.normal.textColor = new Color(0.6f, 0.4f, 0.4f);
            GUI.Label(new Rect(x, iconY + iconSize + 35, w, 40), data.UnlockCondition, lockStyle);
        }

        // クリック判定
        if (isUnlocked && GUI.Button(new Rect(x, y, w, h), "", GUIStyle.none))
        {
            selectedCharIndex = index;
            AudioManager.PlayLevelUp();
        }
    }

    void DrawStageSelect()
    {
        // 背景オーバーレイ
        GUI.color = new Color(0, 0, 0, 0.92f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float startY = Screen.height * 0.08f;

        // タイトル
        var headerStyle = new GUIStyle(titleStyle);
        headerStyle.fontSize = 36;
        GUI.Label(new Rect(0, startY, Screen.width, 50), "STAGE SELECT", headerStyle);

        float y = startY + 70;
        float cardW = 180f, cardH = 140f;
        float cardGap = 20f;
        int cols = Mathf.Min(stages.Count, 4);
        float totalW = cols * cardW + (cols - 1) * cardGap;
        float startX = cx - totalW / 2f;

        for (int i = 0; i < stages.Count; i++)
        {
            float x = startX + i * (cardW + cardGap);
            DrawStageCard(x, y, cardW, cardH, stages[i], i);
        }

        // 選択中ステージの説明
        float descY = y + cardH + 20;
        var selected = stages[selectedStageIndex];
        GUI.Label(new Rect(0, descY, Screen.width, 30), selected.Description, infoStyle);

        // ステージ難易度
        float diffY = descY + 35;
        string diffText = $"難易度: x{selected.DifficultyMultiplier:F1}  出現率: x{selected.SpawnRateMultiplier:F1}";
        GUI.Label(new Rect(0, diffY, Screen.width, 25), diffText, infoStyle);

        // 戻るボタン
        float btnW = 120f, btnH = 45f;
        if (GUI.Button(new Rect(cx - btnW / 2f, Screen.height - 80, btnW, btnH), "Back", smallButtonStyle))
        {
            StageSelection.SelectedStage = stages[selectedStageIndex].Type;
            menuState = MenuState.Main;
        }
    }

    void DrawStageCard(float x, float y, float w, float h, StageData data, int index)
    {
        bool isSelected = index == selectedStageIndex;
        bool isUnlocked = data.IsUnlockedByDefault || IsStageUnlocked(data.Type);

        // カード背景（ステージの色を使用）
        Color bgColor;
        if (!isUnlocked)
        {
            bgColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
        }
        else if (isSelected)
        {
            bgColor = new Color(data.GroundColor.r + 0.2f, data.GroundColor.g + 0.2f, data.GroundColor.b + 0.2f, 0.9f);
        }
        else
        {
            bgColor = new Color(data.GroundColor.r, data.GroundColor.g, data.GroundColor.b, 0.7f);
        }

        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // ステージ名
        var nameStyle = isUnlocked ? charNameStyle : lockedStyle;
        GUI.Label(new Rect(x, y + 40, w, 30), data.Name, nameStyle);

        // クリア済みマーク
        if (SaveSystem.IsStageCleared(data.Type))
        {
            var clearStyle = new GUIStyle(infoStyle);
            clearStyle.normal.textColor = new Color(0.3f, 0.9f, 0.3f);
            clearStyle.fontSize = 12;
            GUI.Label(new Rect(x, y + 70, w, 20), "CLEARED", clearStyle);
        }

        // ロック表示
        if (!isUnlocked)
        {
            var lockStyle = new GUIStyle(infoStyle);
            lockStyle.fontSize = 11;
            lockStyle.normal.textColor = new Color(0.6f, 0.4f, 0.4f);
            GUI.Label(new Rect(x, y + 75, w, 40), data.UnlockCondition, lockStyle);
        }

        // クリック判定
        if (isUnlocked && GUI.Button(new Rect(x, y, w, h), "", GUIStyle.none))
        {
            selectedStageIndex = index;
            AudioManager.PlayLevelUp();
        }
    }

    bool IsCharacterUnlocked(CharacterType type)
    {
        string id = $"char_{type.ToString().ToLower()}";
        return UnlockManager.Instance?.IsUnlocked(id) ?? false;
    }

    bool IsStageUnlocked(StageType type)
    {
        string id = $"stage_{type.ToString().ToLower()}";
        return UnlockManager.Instance?.IsUnlocked(id) ?? false;
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
            DrawStatRow(panelX, y, panelW, "Total Gold", $"{stats.TotalGoldEarned}"); y += rowH;
            DrawStatRow(panelX, y, panelW, "Stages Cleared", $"{stats.TotalStagesCleared}"); y += rowH + 15;

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
            menuState = MenuState.Main;
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

        charNameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        charNameStyle.normal.textColor = Color.white;

        lockedStyle = new GUIStyle(charNameStyle);
        lockedStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

        selectedStyle = new GUIStyle(charNameStyle);
        selectedStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);
    }
}
