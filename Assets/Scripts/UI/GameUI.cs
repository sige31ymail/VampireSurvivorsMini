using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // 新Input System使用時
#endif

/// <summary>
/// ゲーム中UI。HUD は uGUI(TextMeshPro)で構築。
/// レベルアップ3択・リザルトは段階的移行中のため当面 OnGUI（IMGUIは常にuGUIの上に描かれる）。
/// </summary>
public class GameUI : MonoBehaviour
{
    public Player player;

    // ── HUD(uGUI) ─────────────────────────
    Canvas hudCanvas;
    Image hpFill, xpFill;
    TextMeshProUGUI hpLabel, xpLabel, infoLabel, goldLabel, toastLabel;
    TextMeshProUGUI[] weaponLabels;
    const int MaxWeaponRows = 12;

    // ── OnGUI(レベルアップ/リザルト用、移行までの暫定) ──
    GUIStyle bigStyle, toastStyleGUI, buttonStyle, resultStyle;
    bool isTransitioning;

    void Start()
    {
        BuildHud();
    }

    // ───────────────────────────────────────
    //  HUD 構築
    // ───────────────────────────────────────
    void BuildHud()
    {
        hudCanvas = UIKit.CreateCanvas("HUDCanvas", 100);
        var root = hudCanvas.transform;

        const float margin = 32f;
        var topLeft = new Vector2(0f, 1f);

        // HPバー
        hpFill = UIKit.Bar(root, UITheme.Hp, "HpBar");
        UIKit.SetRect(hpFill.transform.parent.GetComponent<Image>(), topLeft, topLeft, topLeft,
            new Vector2(margin, -margin), new Vector2(460f, 44f));
        hpLabel = UIKit.Label(hpFill.transform.parent, "HP", 24f, UITheme.TextMain, TextAlignmentOptions.Center, "HpText");
        UIKit.Stretch(hpLabel);

        // XPバー
        xpFill = UIKit.Bar(root, UITheme.Xp, "XpBar");
        UIKit.SetRect(xpFill.transform.parent.GetComponent<Image>(), topLeft, topLeft, topLeft,
            new Vector2(margin, -(margin + 54f)), new Vector2(460f, 30f));
        xpLabel = UIKit.Label(xpFill.transform.parent, "Lv 1", 20f, UITheme.TextMain, TextAlignmentOptions.Center, "XpText");
        UIKit.Stretch(xpLabel);

        // 時間・キル
        infoLabel = UIKit.Label(root, "", 30f, UITheme.TextMain, TextAlignmentOptions.Left, "Info");
        UIKit.SetRect(infoLabel, topLeft, topLeft, topLeft, new Vector2(margin, -(margin + 94f)), new Vector2(640f, 40f));

        // ゴールド
        goldLabel = UIKit.Label(root, "", 28f, UITheme.Gold, TextAlignmentOptions.Left, "Gold");
        UIKit.SetRect(goldLabel, topLeft, topLeft, topLeft, new Vector2(margin, -(margin + 134f)), new Vector2(460f, 36f));

        // 武器リスト
        weaponLabels = new TextMeshProUGUI[MaxWeaponRows];
        for (int i = 0; i < MaxWeaponRows; i++)
        {
            weaponLabels[i] = UIKit.Label(root, "", 24f, UITheme.TextDim, TextAlignmentOptions.Left, "Weapon" + i);
            UIKit.SetRect(weaponLabels[i], topLeft, topLeft, topLeft,
                new Vector2(margin, -(margin + 180f + i * 34f)), new Vector2(560f, 30f));
            weaponLabels[i].gameObject.SetActive(false);
        }

        // レベルアップ獲得トースト（中央上）
        var topCenter = new Vector2(0.5f, 1f);
        toastLabel = UIKit.Label(root, "", 40f, UITheme.Gold, TextAlignmentOptions.Center, "Toast");
        toastLabel.fontStyle = FontStyles.Bold;
        UIKit.SetRect(toastLabel, topCenter, topCenter, topCenter, new Vector2(0f, -170f), new Vector2(1200f, 60f));
        toastLabel.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null || hudCanvas == null) return;

        // ポーズ中はHUDを隠す（ポーズメニューが表示される）
        bool show = !PauseMenu.IsPaused;
        if (hudCanvas.gameObject.activeSelf != show)
            hudCanvas.gameObject.SetActive(show);
        if (!show) return;

        // HP
        hpFill.fillAmount = player.maxHp > 0 ? (float)player.hp / player.maxHp : 0f;
        hpLabel.text = $"HP {player.hp}/{player.maxHp}";

        // XP
        xpFill.fillAmount = player.xpToNext > 0 ? (float)player.xp / player.xpToNext : 0f;
        xpLabel.text = $"Lv {player.level}";

        // 時間・キル
        int t = (int)GameState.ElapsedTime;
        infoLabel.text = $"{t / 60:00}:{t % 60:00}   Kills {GameState.KillCount}";

        // ゴールド
        goldLabel.text = $"Gold {GoldCoin.SessionGold}";

        // 武器
        for (int i = 0; i < MaxWeaponRows; i++)
        {
            if (i < player.weapons.Count)
            {
                var w = player.weapons[i];
                weaponLabels[i].text = $"・{w.Name} Lv{w.level}{(w.IsMaxLevel ? " (MAX)" : "")}";
                if (!weaponLabels[i].gameObject.activeSelf) weaponLabels[i].gameObject.SetActive(true);
            }
            else if (weaponLabels[i].gameObject.activeSelf)
            {
                weaponLabels[i].gameObject.SetActive(false);
            }
        }

        // 獲得トースト（2.5秒）
        bool toast = Time.time - player.lastUpgradeTime < 2.5f && !string.IsNullOrEmpty(player.lastUpgradeText);
        if (toast) toastLabel.text = player.lastUpgradeText;
        if (toastLabel.gameObject.activeSelf != toast) toastLabel.gameObject.SetActive(toast);
    }

    // ───────────────────────────────────────
    //  OnGUI（レベルアップ3択・リザルト：段階3/4で uGUI 化予定）
    // ───────────────────────────────────────
    void OnGUI()
    {
        if (player == null) return;
        if (PauseMenu.IsPaused) return;

        if (bigStyle == null) InitGuiStyles();

        if (player.currentOptions != null && !GameState.GameOver)
            DrawLevelUpPanel();

        if (GameState.GameOver)
            DrawResultScreen();
    }

    void InitGuiStyles()
    {
        bigStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 48, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold
        };
        bigStyle.normal.textColor = Color.white;
        toastStyleGUI = new GUIStyle(GUI.skin.label)
        {
            fontSize = 26, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold
        };
        toastStyleGUI.normal.textColor = new Color(1f, 0.9f, 0.3f);
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        resultStyle = new GUIStyle(GUI.skin.label) { fontSize = 22 };
        resultStyle.normal.textColor = Color.white;
    }

    void DrawResultScreen()
    {
        if (isTransitioning) return;

        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        GUI.Label(new Rect(0, cy - 210, Screen.width, 70), "GAME OVER", bigStyle);

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

        var goldResultStyle = new GUIStyle(resultStyle);
        goldResultStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(lx, ly + 102, panelW - 20, lineH), $"獲得ゴールド +{GoldCoin.SessionGold}", goldResultStyle);

        for (int i = 0; i < player.weapons.Count; i++)
        {
            var w = player.weapons[i];
            string prefix = i == 0 ? "武器         " : "             ";
            GUI.Label(new Rect(lx, ly + 136 + i * lineH, panelW - 20, lineH), prefix + $"{w.Name} Lv{w.level}", resultStyle);
        }

        float btnW = 160f, btnH = 52f, btnGap = 20f;
        float btnY = panelY + panelH + 24f;
        float btnX = cx - (btnW * 2 + btnGap) / 2f;

        if (GUI.Button(new Rect(btnX,                  btnY, btnW, btnH), "もう一度 [R]", buttonStyle))
            Transition(SceneManager.GetActiveScene().buildIndex);
        if (GUI.Button(new Rect(btnX + btnW + btnGap, btnY, btnW, btnH), "タイトルへ", buttonStyle))
            Transition("TitleScene");

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.R))
#endif
            Transition(SceneManager.GetActiveScene().buildIndex);
    }

    void DrawLevelUpPanel()
    {
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(0, Screen.height * 0.15f, Screen.width, 60), "LEVEL UP!", bigStyle);

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
                return;
            }
        }

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

    void Transition(string sceneName)
    {
        isTransitioning = true;
        ScreenFader.Get().FadeToScene(sceneName);
    }

    void Transition(int buildIndex)
    {
        isTransitioning = true;
        ScreenFader.Get().FadeToScene(buildIndex);
    }
}
