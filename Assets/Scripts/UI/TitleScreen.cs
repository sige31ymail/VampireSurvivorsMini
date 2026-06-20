using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>タイトル画面UI（TitleSceneに配置）。見た目は UISkin で統一。</summary>
public class TitleScreen : MonoBehaviour
{
    GUIStyle titleStyle, subtitleStyle, infoStyle, statsStyle, charNameStyle, lockedStyle;
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
            cam.backgroundColor = new Color(0.07f, 0.06f, 0.10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        if (SaveSystem.Instance == null)
            new GameObject("SaveSystem").AddComponent<SaveSystem>();
        if (AudioManager.Instance == null)
            new GameObject("AudioManager").AddComponent<AudioManager>();
        if (MetaProgressionManager.Instance == null)
            new GameObject("MetaProgressionManager").AddComponent<MetaProgressionManager>();
        if (UnlockManager.Instance == null)
            new GameObject("UnlockManager").AddComponent<UnlockManager>();

        settingsMenu = gameObject.AddComponent<SettingsMenu>();
        upgradeShopUI = gameObject.AddComponent<UpgradeShopUI>();

        characters = CharacterData.GetAllCharacters();
        stages = StageData.GetAllStages();

        selectedCharIndex = characters.FindIndex(c => c.Type == CharacterSelection.SelectedCharacter);
        selectedStageIndex = stages.FindIndex(s => s.Type == StageSelection.SelectedStage);
        if (selectedCharIndex < 0) selectedCharIndex = 0;
        if (selectedStageIndex < 0) selectedStageIndex = 0;
    }

    void OnGUI()
    {
        UISkin.Init();
        if (titleStyle == null) InitStyles();

        if (settingsMenu != null && settingsMenu.IsVisible) return;
        if (upgradeShopUI != null && upgradeShopUI.IsVisible) return;

        switch (menuState)
        {
            case MenuState.Main:            DrawMainMenu();       break;
            case MenuState.CharacterSelect: DrawCharacterSelect(); break;
            case MenuState.StageSelect:     DrawStageSelect();     break;
            case MenuState.Stats:           DrawStatsScreen();     break;
        }
    }

    // ───────────────────────────────────────
    void DrawMainMenu()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        UISkin.ShadowLabel(new Rect(0, cy - 200, Screen.width, 90), "Vampire Survivors Mini", titleStyle);
        UISkin.ShadowLabel(new Rect(0, cy - 110, Screen.width, 40), "生き残れ、強くなれ", subtitleStyle);

        var charData = characters[selectedCharIndex];
        var stageData = stages[selectedStageIndex];
        UISkin.ShadowLabel(new Rect(0, cy - 64, Screen.width, 30), $"{charData.Name} / {stageData.Name}", infoStyle);

        float y = cy - 16f;
        float btnGap = 10f;

        // PLAY（プライマリ）
        float playW = 240f, playH = 60f;
        if (UISkin.PrimaryButton2(new Rect(cx - playW / 2f, y, playW, playH), "PLAY"))
        {
            CharacterSelection.SelectedCharacter = charData.Type;
            StageSelection.SelectedStage = stageData.Type;
            SceneManager.LoadScene("SampleScene");
        }
        y += playH + 12f;

        // Character / Stage
        float selW = 150f, selH = 46f;
        float selX = cx - (selW * 2 + btnGap) / 2f;
        if (UISkin.Button2(new Rect(selX, y, selW, selH), "キャラクター"))
            menuState = MenuState.CharacterSelect;
        if (UISkin.Button2(new Rect(selX + selW + btnGap, y, selW, selH), "ステージ"))
            menuState = MenuState.StageSelect;
        y += selH + 10f;

        // Upgrades
        float shopW = 200f, shopH = 46f;
        if (UISkin.Button2(new Rect(cx - shopW / 2f, y, shopW, shopH), "アップグレード"))
            upgradeShopUI?.Show();
        y += shopH + 8f;

        // ゴールド
        int gold = MetaProgressionManager.Instance?.Gold ?? 0;
        UISkin.ShadowLabel(new Rect(0, y, Screen.width, 26), $"Gold {gold}", goldInfoStyle);
        y += 32f;

        // Settings / Stats
        float smW = 110f, smH = 44f;
        float smX = cx - (smW * 2 + btnGap) / 2f;
        if (UISkin.Button2(new Rect(smX, y, smW, smH), "設定"))
            settingsMenu?.Show();
        if (UISkin.Button2(new Rect(smX + smW + btnGap, y, smW, smH), "記録"))
            menuState = MenuState.Stats;
        y += smH + 16f;

        UISkin.ShadowLabel(new Rect(0, y, Screen.width, 24), "WASD / 矢印キー で移動　攻撃は自動", infoStyle);
        UISkin.ShadowLabel(new Rect(0, y + 24, Screen.width, 24), "ESC でポーズ　レベルアップ時: 1/2/3 キーで選択", infoStyle);

        UISkin.ShadowLabel(new Rect(12, Screen.height - 30, 200, 25), "v0.7.0 (Phase 3)", infoStyle);
    }

    // ───────────────────────────────────────
    void DrawCharacterSelect()
    {
        UISkin.DimScreen(0.9f);

        float cx = Screen.width / 2f;
        float startY = Screen.height * 0.08f;

        UISkin.ShadowLabel(new Rect(0, startY, Screen.width, 50), "CHARACTER SELECT", UISkin.Title);

        float y = startY + 76;
        float cardW = 160f, cardH = 184f, cardGap = 16f;
        int cols = Mathf.Min(characters.Count, 4);
        float totalW = cols * cardW + (cols - 1) * cardGap;
        float startX = cx - totalW / 2f;

        for (int i = 0; i < characters.Count; i++)
        {
            int row = i / 4, col = i % 4;
            float x = startX + col * (cardW + cardGap);
            float cardY = y + row * (cardH + cardGap);
            DrawCharacterCard(x, cardY, cardW, cardH, characters[i], i);
        }

        float descY = y + ((characters.Count - 1) / 4 + 1) * (cardH + cardGap) + 10;
        var selected = characters[selectedCharIndex];
        UISkin.ShadowLabel(new Rect(0, descY, Screen.width, 30), selected.Description, infoStyle);
        string statText = $"HP {selected.BaseMaxHp}   速度 {selected.BaseMoveSpeed:F1}   攻撃 {selected.BaseAttackMult:P0}   クリ {selected.BaseCritChance:P0}";
        UISkin.ShadowLabel(new Rect(0, descY + 32, Screen.width, 25), statText, infoStyle);

        float btnW = 140f, btnH = 46f;
        if (UISkin.Button2(new Rect(cx - btnW / 2f, Screen.height - 84, btnW, btnH), "戻る"))
        {
            CharacterSelection.SelectedCharacter = characters[selectedCharIndex].Type;
            menuState = MenuState.Main;
        }
    }

    void DrawCharacterCard(float x, float y, float w, float h, CharacterData data, int index)
    {
        bool isSelected = index == selectedCharIndex;
        bool isUnlocked = data.IsUnlockedByDefault || IsCharacterUnlocked(data.Type);

        if (isSelected) // 選択グロー
            UISkin.Box(new Rect(x - 4, y - 4, w + 8, h + 8), UISkin.Accent);

        Color bg = !isUnlocked ? new Color(0.12f, 0.12f, 0.14f, 0.85f)
                 : isSelected  ? new Color(0.22f, 0.18f, 0.32f, 0.95f)
                               : UISkin.Panel;
        UISkin.Box(new Rect(x, y, w, h), bg);

        // アイコン（角丸の色チップ）
        float iconSize = 64f;
        float iconX = x + (w - iconSize) / 2f;
        float iconY = y + 20f;
        UISkin.Box(new Rect(iconX, iconY, iconSize, iconSize), isUnlocked ? data.SpriteColor : new Color(0.3f, 0.3f, 0.3f));

        UISkin.ShadowLabel(new Rect(x, iconY + iconSize + 10, w, 26), data.Name, isUnlocked ? charNameStyle : lockedStyle);

        if (!isUnlocked)
            UISkin.ShadowLabel(new Rect(x + 8, iconY + iconSize + 38, w - 16, 44), data.UnlockCondition, lockHintStyle);

        if (isUnlocked && GUI.Button(new Rect(x, y, w, h), "", GUIStyle.none))
        {
            selectedCharIndex = index;
            AudioManager.PlayLevelUp();
        }
    }

    // ───────────────────────────────────────
    void DrawStageSelect()
    {
        UISkin.DimScreen(0.9f);

        float cx = Screen.width / 2f;
        float startY = Screen.height * 0.08f;

        UISkin.ShadowLabel(new Rect(0, startY, Screen.width, 50), "STAGE SELECT", UISkin.Title);

        float y = startY + 76;
        float cardW = 184f, cardH = 144f, cardGap = 20f;
        int cols = Mathf.Min(stages.Count, 4);
        float totalW = cols * cardW + (cols - 1) * cardGap;
        float startX = cx - totalW / 2f;

        for (int i = 0; i < stages.Count; i++)
        {
            float x = startX + i * (cardW + cardGap);
            DrawStageCard(x, y, cardW, cardH, stages[i], i);
        }

        float descY = y + cardH + 22;
        var selected = stages[selectedStageIndex];
        UISkin.ShadowLabel(new Rect(0, descY, Screen.width, 30), selected.Description, infoStyle);
        string diffText = $"難易度 x{selected.DifficultyMultiplier:F1}   出現率 x{selected.SpawnRateMultiplier:F1}";
        UISkin.ShadowLabel(new Rect(0, descY + 32, Screen.width, 25), diffText, infoStyle);

        float btnW = 140f, btnH = 46f;
        if (UISkin.Button2(new Rect(cx - btnW / 2f, Screen.height - 84, btnW, btnH), "戻る"))
        {
            StageSelection.SelectedStage = stages[selectedStageIndex].Type;
            menuState = MenuState.Main;
        }
    }

    void DrawStageCard(float x, float y, float w, float h, StageData data, int index)
    {
        bool isSelected = index == selectedStageIndex;
        bool isUnlocked = data.IsUnlockedByDefault || IsStageUnlocked(data.Type);

        if (isSelected)
            UISkin.Box(new Rect(x - 4, y - 4, w + 8, h + 8), UISkin.Accent);

        Color bg;
        if (!isUnlocked)
            bg = new Color(0.12f, 0.12f, 0.14f, 0.85f);
        else if (isSelected)
            bg = new Color(data.GroundColor.r + 0.18f, data.GroundColor.g + 0.18f, data.GroundColor.b + 0.18f, 0.95f);
        else
            bg = new Color(data.GroundColor.r, data.GroundColor.g, data.GroundColor.b, 0.85f);
        UISkin.Box(new Rect(x, y, w, h), bg);

        UISkin.ShadowLabel(new Rect(x, y + 40, w, 30), data.Name, isUnlocked ? charNameStyle : lockedStyle);

        if (SaveSystem.IsStageCleared(data.Type))
            UISkin.ShadowLabel(new Rect(x, y + 74, w, 20), "CLEARED", clearedStyle);

        if (!isUnlocked)
            UISkin.ShadowLabel(new Rect(x + 8, y + 78, w - 16, 44), data.UnlockCondition, lockHintStyle);

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

    // ───────────────────────────────────────
    void DrawStatsScreen()
    {
        UISkin.DimScreen(0.9f);

        float cx = Screen.width / 2f;
        float panelW = Mathf.Min(420f, Screen.width - 40f);
        float panelX = cx - panelW / 2f;
        float startY = Screen.height * 0.12f;

        UISkin.ShadowLabel(new Rect(0, startY, Screen.width, 50), "STATISTICS", UISkin.Title);

        float y = startY + 76;
        float rowH = 32f;

        UISkin.PanelBox(new Rect(panelX - 16, y - 14, panelW + 32, rowH * 10 + 40), UISkin.PanelDeep);

        if (SaveSystem.Instance != null)
        {
            var stats = SaveSystem.Instance.Statistics;
            DrawStatRow(panelX, y, panelW, "Total Games", $"{stats.TotalGamesPlayed}"); y += rowH;
            DrawStatRow(panelX, y, panelW, "Total Play Time", FormatTime(stats.TotalPlayTime)); y += rowH;
            DrawStatRow(panelX, y, panelW, "Total Kills", $"{stats.TotalKills}"); y += rowH;
            DrawStatRow(panelX, y, panelW, "Total Gold", $"{stats.TotalGoldEarned}"); y += rowH;
            DrawStatRow(panelX, y, panelW, "Stages Cleared", $"{stats.TotalStagesCleared}"); y += rowH + 14;

            UISkin.ShadowLabel(new Rect(panelX, y, panelW, rowH), "Best Records", sectionStyle);
            y += rowH + 4;
            DrawStatRow(panelX, y, panelW, "Best Survival", FormatTime(stats.BestSurvivalTime)); y += rowH;
            DrawStatRow(panelX, y, panelW, "Best Kills", $"{stats.BestKills}"); y += rowH;
            DrawStatRow(panelX, y, panelW, "Best Level", $"{stats.BestLevel}"); y += rowH;
        }
        else
        {
            UISkin.ShadowLabel(new Rect(panelX, y, panelW, rowH), "No statistics yet", statsStyle);
        }

        float btnW = 140f, btnH = 46f;
        if (UISkin.Button2(new Rect(cx - btnW / 2f, Screen.height - 96, btnW, btnH), "戻る"))
            menuState = MenuState.Main;
    }

    void DrawStatRow(float x, float y, float width, string label, string value)
    {
        UISkin.ShadowLabel(new Rect(x, y, width * 0.6f, 30), label, statsStyle);
        UISkin.ShadowLabel(new Rect(x + width * 0.4f, y, width * 0.6f, 30), value, statsValueStyle);
    }

    string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.RoundToInt(seconds);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int secs = totalSeconds % 60;
        if (hours > 0)   return $"{hours}h {minutes}m {secs}s";
        if (minutes > 0) return $"{minutes}m {secs}s";
        return $"{secs}s";
    }

    // ── ラベル系スタイル（背景なし＝GUI.skin既定フォントで日本語OK） ──
    GUIStyle goldInfoStyle, lockHintStyle, clearedStyle, sectionStyle, statsValueStyle;

    void InitStyles()
    {
        titleStyle = new GUIStyle(GUI.skin.label)
        { fontSize = 52, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = true };
        titleStyle.normal.textColor = UISkin.Gold;

        subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        subtitleStyle.normal.textColor = UISkin.TextDim;

        infoStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        infoStyle.normal.textColor = UISkin.TextDim;

        goldInfoStyle = new GUIStyle(infoStyle) { fontStyle = FontStyle.Bold };
        goldInfoStyle.normal.textColor = UISkin.Gold;

        statsStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleLeft };
        statsStyle.normal.textColor = UISkin.TextMain;

        statsValueStyle = new GUIStyle(statsStyle) { alignment = TextAnchor.MiddleRight };

        sectionStyle = new GUIStyle(statsStyle) { fontStyle = FontStyle.Bold };
        sectionStyle.normal.textColor = UISkin.Gold;

        charNameStyle = new GUIStyle(GUI.skin.label)
        { fontSize = 16, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        charNameStyle.normal.textColor = UISkin.TextMain;

        lockedStyle = new GUIStyle(charNameStyle);
        lockedStyle.normal.textColor = new Color(0.55f, 0.55f, 0.6f);

        lockHintStyle = new GUIStyle(GUI.skin.label)
        { fontSize = 11, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        lockHintStyle.normal.textColor = new Color(0.7f, 0.5f, 0.5f);

        clearedStyle = new GUIStyle(GUI.skin.label)
        { fontSize = 12, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        clearedStyle.normal.textColor = new Color(0.35f, 0.92f, 0.4f);
    }
}
