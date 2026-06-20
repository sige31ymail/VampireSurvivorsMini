using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // 新Input System使用時
#endif

/// <summary>OnGUIによるUI。見た目は UISkin（角丸/影/ホバー/配色）で統一。</summary>
public class GameUI : MonoBehaviour
{
    public Player player;
    bool isTransitioning;

    void OnGUI()
    {
        UISkin.Init();
        if (player == null) return;

        // ポーズ中はHUDを描画しない（ポーズメニューが表示される）
        if (PauseMenu.IsPaused) return;

        DrawHud();

        if (player.currentOptions != null && !GameState.GameOver)
            DrawLevelUpPanel();

        if (GameState.GameOver)
            DrawResultScreen();
    }

    // ───────────────────────────────────────
    //  HUD
    // ───────────────────────────────────────
    void DrawHud()
    {
        // 背景パネル（視認性）
        float panelH = 122f + player.weapons.Count * 24f + 8f;
        UISkin.PanelBox(new Rect(10f, 10f, 280f, panelH), UISkin.Panel);

        // HP / XP バー
        UISkin.Bar(new Rect(22f, 20f, 256f, 24f),
            player.maxHp > 0 ? (float)player.hp / player.maxHp : 0f,
            UISkin.Hp, $"HP {player.hp}/{player.maxHp}");
        UISkin.Bar(new Rect(22f, 50f, 256f, 18f),
            player.xpToNext > 0 ? (float)player.xp / player.xpToNext : 0f,
            UISkin.Xp, $"Lv {player.level}");

        // 時間・キル
        int t = (int)GameState.ElapsedTime;
        UISkin.ShadowLabel(new Rect(24f, 74f, 300f, 28f),
            $"{t / 60:00}:{t % 60:00}   Kills {GameState.KillCount}", UISkin.Label);

        // ゴールド
        UISkin.ShadowLabel(new Rect(24f, 98f, 260f, 26f),
            $"Gold {GoldCoin.SessionGold}", UISkin.GoldLabel);

        // 所持武器一覧
        float y = 124f;
        foreach (var w in player.weapons)
        {
            string max = w.IsMaxLevel ? " (MAX)" : "";
            UISkin.ShadowLabel(new Rect(24f, y, 260f, 24f),
                $"・{w.Name} Lv{w.level}{max}", UISkin.LabelDim);
            y += 24f;
        }

        // レベルアップ獲得トースト（2.5秒）
        if (Time.time - player.lastUpgradeTime < 2.5f && !string.IsNullOrEmpty(player.lastUpgradeText))
        {
            UISkin.ShadowLabel(new Rect(0f, Screen.height * 0.24f, Screen.width, 44f),
                player.lastUpgradeText, UISkin.Header);
        }
    }

    // ───────────────────────────────────────
    //  レベルアップ3択
    // ───────────────────────────────────────
    void DrawLevelUpPanel()
    {
        UISkin.DimScreen(0.72f);
        UISkin.ShadowLabel(new Rect(0f, Screen.height * 0.13f, Screen.width, 64f), "LEVEL UP!", UISkin.Big);

        float panelW = Mathf.Min(480f, Screen.width - 40f);
        float cardH = 88f;
        float gap = 14f;
        float x = (Screen.width - panelW) / 2f;
        float startY = Screen.height * 0.28f;

        for (int i = 0; i < player.currentOptions.Count; i++)
        {
            var opt = player.currentOptions[i];
            var rect = new Rect(x, startY + i * (cardH + gap), panelW, cardH);
            string text = $"<b>[{i + 1}]  {opt.title}</b>\n<color=#B9B9C8>{opt.desc}</color>";
            if (GUI.Button(rect, text, UISkin.Card))
            {
                player.ChooseOption(i);
                return;
            }
        }

        int key = GetNumberKeyPressed();
        if (key >= 0 && key < player.currentOptions.Count) player.ChooseOption(key);
    }

    // ───────────────────────────────────────
    //  リザルト
    // ───────────────────────────────────────
    void DrawResultScreen()
    {
        if (isTransitioning) return;

        UISkin.DimScreen(0.85f);

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        UISkin.ShadowLabel(new Rect(0f, cy - 220f, Screen.width, 70f), "GAME OVER", UISkin.Big);

        float panelW = Mathf.Min(380f, Screen.width - 40f);
        float lineH = 34f;
        float panelH = 150f + player.weapons.Count * lineH + 24f;
        float panelX = cx - panelW / 2f;
        float panelY = cy - 150f;

        UISkin.PanelBox(new Rect(panelX, panelY, panelW, panelH), UISkin.PanelDeep);

        int t = (int)GameState.ElapsedTime;
        float lx = panelX + 22f;
        float lw = panelW - 44f;
        float ly = panelY + 16f;
        UISkin.ShadowLabel(new Rect(lx, ly,        lw, lineH), $"生存時間     {t / 60:00}:{t % 60:00}", UISkin.Label);
        UISkin.ShadowLabel(new Rect(lx, ly + 34f,  lw, lineH), $"撃破数       {GameState.KillCount}", UISkin.Label);
        UISkin.ShadowLabel(new Rect(lx, ly + 68f,  lw, lineH), $"到達レベル   {player.level}", UISkin.Label);
        UISkin.ShadowLabel(new Rect(lx, ly + 102f, lw, lineH), $"獲得ゴールド +{GoldCoin.SessionGold}", UISkin.GoldLabel);

        for (int i = 0; i < player.weapons.Count; i++)
        {
            var w = player.weapons[i];
            string prefix = i == 0 ? "武器         " : "             ";
            UISkin.ShadowLabel(new Rect(lx, ly + 146f + i * lineH, lw, lineH),
                prefix + $"{w.Name} Lv{w.level}", UISkin.LabelDim);
        }

        float btnW = 168f, btnH = 52f, btnGap = 18f;
        float btnY = panelY + panelH + 22f;
        float btnX = cx - (btnW * 2f + btnGap) / 2f;

        if (UISkin.Button2(new Rect(btnX, btnY, btnW, btnH), "もう一度 [R]"))
            Transition(SceneManager.GetActiveScene().buildIndex);
        if (UISkin.Button2(new Rect(btnX + btnW + btnGap, btnY, btnW, btnH), "タイトルへ"))
            Transition("TitleScene");

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.R))
#endif
            Transition(SceneManager.GetActiveScene().buildIndex);
    }

    // ───────────────────────────────────────
    int GetNumberKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var kb = Keyboard.current;
        if (kb == null) return -1;
        if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) return 0;
        if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) return 1;
        if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame) return 2;
#else
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 2;
#endif
        return -1;
    }

    void Transition(string sceneName)
    {
        isTransitioning = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    void Transition(int buildIndex)
    {
        isTransitioning = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex);
    }
}
