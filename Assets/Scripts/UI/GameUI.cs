using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // 新Input System使用時
#endif

/// <summary>OnGUIによる簡易UI（Canvas設定不要）</summary>
public class GameUI : MonoBehaviour
{
    public Player player;
    GUIStyle labelStyle, bigStyle, toastStyle, buttonStyle;

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
        }

        if (player == null) return;

        // HPバー
        DrawBar(new Rect(20, 20, 240, 22),
            (float)player.hp / player.maxHp,
            new Color(0.9f, 0.25f, 0.25f), $"HP {player.hp}/{player.maxHp}");

        // XPバー
        DrawBar(new Rect(20, 48, 240, 16),
            (float)player.xp / player.xpToNext,
            new Color(0.4f, 0.9f, 0.5f), $"Lv {player.level}");

        // 経過時間とキル数
        int t = (int)GameState.ElapsedTime;
        GUI.Label(new Rect(20, 72, 300, 30),
            $"{t / 60:00}:{t % 60:00}   Kills: {GameState.KillCount}", labelStyle);

        // 所持武器一覧
        float y = 100;
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
        {
            var rect = new Rect(0, Screen.height / 2f - 60, Screen.width, 120);
            GUI.Label(rect, "GAME OVER\n[R] でリスタート", bigStyle);

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            bool restart = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
            bool restart = Input.GetKeyDown(KeyCode.R);
#endif
            if (restart)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
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
