using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // 新Input System使用時
#endif

/// <summary>OnGUIによる簡易UI（Canvas設定不要）</summary>
public class GameUI : MonoBehaviour
{
    public Player player;
    GUIStyle labelStyle, bigStyle, toastStyle, buttonStyle, resultStyle;
    bool isTransitioning;

    void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            labelStyle.normal.textColor = Color.white;
            bigStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            bigStyle.normal.textColor = Color.white;
            toastStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            toastStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            resultStyle = new GUIStyle(GUI.skin.label) { fontSize = 22 };
            resultStyle.normal.textColor = Color.white;
        }

        if (player == null) return;

        // ポーズ中はHUDを描画しない（ポーズメニューが表示される）
        if (PauseMenu.IsPaused) return;

        // HPバー
        DrawBar(new Rect(20, 20, 240, 22),
            (float)player.hp / player.maxHp,
            new Color(0.9f, 0.25f, 0.25f), $"HP {player.hp}/{player.maxHp}");

        // XPバー
        DrawBar(new Rect(20, 48, 240, 16),
            (float)player.xp / player.xpToNext,
            new Color(0.4f, 0.9f, 0.5f), $"Lv {player.level}");

        // 経過時間、キル数、ゴールド
        int t = (int)GameState.ElapsedTime;
        GUI.Label(new Rect(20, 72, 300, 30),
            $"{t / 60:00}:{t % 60:00}   Kills: {GameState.KillCount}", labelStyle);

        // ゴールド表示
        var goldStyle = new GUIStyle(labelStyle);
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(20, 96, 200, 26), $"Gold: {GoldCoin.SessionGold}", goldStyle);

        // 所持武器一覧
        float y = 124;
        foreach (var w in player.weapons)
        {
            string max = w.IsMaxLevel ? " (MAX)" : "";
            GUI.Label(new Rect(20, y, 320, 26), $"・{w.Name} Lv{w.level}{max}", labelStyle);
            y += 24;
        }

        // レベルアップ獲得トースト（2.5秒間表示）
        if (Time.time - player.lastUpgradeTime < 2.5f)
        {
            var rect = new Rect(0, Screen.height * 0.25f, Screen.width, 40);
            GUI.Label(rect, player.lastUpgradeText, toastStyle);
        }

        // レベルアップ3択パネル
        if (player.currentOptions != null && !GameState.GameOver)
            DrawLevelUpPanel();

        if (GameState.GameOver)
            DrawResultScreen();
    }

    void DrawResultScreen()
    {
        if (isTransitioning) return;

        // 背景オーバーレイ
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        // ヘッダー
        GUI.Label(new Rect(0, cy - 210, Screen.width, 70), "GAME OVER", bigStyle);

        // ステータスパネル
        float panelW = Mathf.Min(360f, Screen.width - 40f);
        float lineH = 34f;
        float panelH = 136f + player.weapons.Count * lineH + 20f;
        float panelX = cx - panelW / 2f;
        float panelY = cy - 140f;

        GUI.color = new Color(1f, 1f, 1f, 0.08f);
        GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        int t = (int)GameState.ElapsedTime;
        float lx = panelX + 20f;
        float ly = panelY + 12f;
        GUI.Label(new Rect(lx, ly,      panelW - 20, lineH), $"生存時間     {t / 60:00}:{t % 60:00}", resultStyle);
        GUI.Label(new Rect(lx, ly + 34, panelW - 20, lineH), $"撃破数       {GameState.KillCount}", resultStyle);
        GUI.Label(new Rect(lx, ly + 68, panelW - 20, lineH), $"到達レベル   {player.level}", resultStyle);

        // 獲得ゴールド
        var goldResultStyle = new GUIStyle(resultStyle);
        goldResultStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(lx, ly + 102, panelW - 20, lineH), $"獲得ゴールド +{GoldCoin.SessionGold}", goldResultStyle);

        for (int i = 0; i < player.weapons.Count; i++)
        {
            var w = player.weapons[i];
            string prefix = i == 0 ? "武器         " : "             ";
            GUI.Label(new Rect(lx, ly + 136 + i * lineH, panelW - 20, lineH),
                prefix + $"{w.Name} Lv{w.level}", resultStyle);
        }

        // ボタン
        float btnW = 160f, btnH = 52f, btnGap = 20f;
        float btnY = panelY + panelH + 24f;
        float btnX = cx - (btnW * 2 + btnGap) / 2f;

        if (GUI.Button(new Rect(btnX,              btnY, btnW, btnH), "もう一度 [R]", buttonStyle))
            Transition(SceneManager.GetActiveScene().buildIndex);

        if (GUI.Button(new Rect(btnX + btnW + btnGap, btnY, btnW, btnH), "タイトルへ", buttonStyle))
            Transition("TitleScene");

        // Rキーでも即リスタート
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.R))
#endif
            Transition(SceneManager.GetActiveScene().buildIndex);
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

    void DrawBar(Rect rect, float ratio, Color color, string text)
    {
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = color;
        var fill = rect;
        fill.width *= Mathf.Clamp01(ratio);
        GUI.DrawTexture(fill, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 6, rect.y, rect.width, rect.height), text, labelStyle);
    }

    /// <summary>レベルアップ時の3択パネル（クリック or 1/2/3キーで選択）</summary>
    void DrawLevelUpPanel()
    {
        // 画面全体を暗くする
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(0, Screen.height * 0.15f, Screen.width, 60),
            "LEVEL UP!", bigStyle);

        float panelW = Mathf.Min(420f, Screen.width - 40f);
        float panelH = 80f;
        float gap = 14f;
        float x = (Screen.width - panelW) / 2f;
        float startY = Screen.height * 0.30f;

        for (int i = 0; i < player.currentOptions.Count; i++)
        {
            var opt = player.currentOptions[i];
            var rect = new Rect(x, startY + i * (panelH + gap), panelW, panelH);
            if (GUI.Button(rect, $"[{i + 1}] {opt.title}\n{opt.desc}", buttonStyle))
            {
                player.ChooseOption(i);
                return; // 選択後はパネルが消えるので即終了
            }
        }

        // キーボード(1/2/3)でも選択可能
        int key = GetNumberKeyPressed();
        if (key >= 0) player.ChooseOption(key);
    }

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
}
