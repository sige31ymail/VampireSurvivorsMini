using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

/// <summary>
/// ポーズメニュー
/// ESCキーでゲームを一時停止し、メニューを表示
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    public Player player;

    bool showSettings;
    bool isTransitioning;

    GUIStyle titleStyle;
    GUIStyle buttonStyle;
    GUIStyle labelStyle;
    GUIStyle sliderLabelStyle;

    // 設定の一時値（適用前）
    float tempBgmVolume;
    float tempSeVolume;
    bool tempScreenShake;
    bool tempDamageNumbers;

    void Start()
    {
        IsPaused = false;
    }

    void Update()
    {
        if (isTransitioning) return;

        // ゲームオーバー時はポーズ不可
        if (GameState.GameOver) return;

        // レベルアップ選択中はポーズ不可
        if (player != null && player.currentOptions != null) return;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool escPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        bool escPressed = Input.GetKeyDown(KeyCode.Escape);
#endif

        if (escPressed)
        {
            if (showSettings)
            {
                // 設定画面を閉じてポーズメニューに戻る
                showSettings = false;
            }
            else
            {
                TogglePause();
            }
        }
    }

    void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;

        if (IsPaused)
        {
            // 設定値を一時変数にコピー
            LoadTempSettings();
        }
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        showSettings = false;
    }

    void OnGUI()
    {
        if (!IsPaused || isTransitioning) return;

        InitStyles();

        if (showSettings)
            DrawSettingsPanel();
        else
            DrawPausePanel();
    }

    void InitStyles()
    {
        if (titleStyle != null) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 42,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft
        };
        labelStyle.normal.textColor = Color.white;

        sliderLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleRight
        };
        sliderLabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
    }

    void DrawPausePanel()
    {
        // 背景オーバーレイ
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        // タイトル
        GUI.Label(new Rect(0, cy - 180, Screen.width, 60), "PAUSED", titleStyle);

        // ボタン
        float btnW = 200f;
        float btnH = 50f;
        float btnGap = 15f;
        float startY = cy - 80;
        float btnX = cx - btnW / 2f;

        if (GUI.Button(new Rect(btnX, startY, btnW, btnH), "Resume", buttonStyle))
        {
            Resume();
        }

        if (GUI.Button(new Rect(btnX, startY + btnH + btnGap, btnW, btnH), "Settings", buttonStyle))
        {
            showSettings = true;
        }

        if (GUI.Button(new Rect(btnX, startY + (btnH + btnGap) * 2, btnW, btnH), "Restart", buttonStyle))
        {
            Restart();
        }

        if (GUI.Button(new Rect(btnX, startY + (btnH + btnGap) * 3, btnW, btnH), "Title", buttonStyle))
        {
            GoToTitle();
        }

        // ショートカットヒント
        var hintStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };
        hintStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        GUI.Label(new Rect(0, Screen.height - 40, Screen.width, 30), "Press ESC to resume", hintStyle);
    }

    void DrawSettingsPanel()
    {
        // 背景オーバーレイ
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float panelW = Mathf.Min(400f, Screen.width - 40f);
        float panelX = cx - panelW / 2f;
        float startY = Screen.height * 0.15f;

        // タイトル
        GUI.Label(new Rect(0, startY, Screen.width, 50), "SETTINGS", titleStyle);

        float y = startY + 80;
        float rowH = 45f;
        float labelW = 150f;
        float sliderW = panelW - labelW - 60f;
        float valueW = 50f;

        // BGM Volume
        GUI.Label(new Rect(panelX, y, labelW, rowH), "BGM Volume", labelStyle);
        tempBgmVolume = GUI.HorizontalSlider(
            new Rect(panelX + labelW, y + 12, sliderW, 20),
            tempBgmVolume, 0f, 1f);
        GUI.Label(new Rect(panelX + labelW + sliderW + 5, y, valueW, rowH),
            $"{Mathf.RoundToInt(tempBgmVolume * 100)}%", sliderLabelStyle);
        y += rowH;

        // SE Volume
        GUI.Label(new Rect(panelX, y, labelW, rowH), "SE Volume", labelStyle);
        tempSeVolume = GUI.HorizontalSlider(
            new Rect(panelX + labelW, y + 12, sliderW, 20),
            tempSeVolume, 0f, 1f);
        GUI.Label(new Rect(panelX + labelW + sliderW + 5, y, valueW, rowH),
            $"{Mathf.RoundToInt(tempSeVolume * 100)}%", sliderLabelStyle);
        y += rowH;

        // Screen Shake
        GUI.Label(new Rect(panelX, y, labelW, rowH), "Screen Shake", labelStyle);
        tempScreenShake = GUI.Toggle(
            new Rect(panelX + labelW, y + 10, 30, 30),
            tempScreenShake, "");
        y += rowH;

        // Damage Numbers
        GUI.Label(new Rect(panelX, y, labelW, rowH), "Damage Numbers", labelStyle);
        tempDamageNumbers = GUI.Toggle(
            new Rect(panelX + labelW, y + 10, 30, 30),
            tempDamageNumbers, "");
        y += rowH + 20;

        // ボタン
        float btnW = 120f;
        float btnH = 45f;
        float btnGap = 20f;
        float btnX = cx - (btnW * 2 + btnGap) / 2f;

        if (GUI.Button(new Rect(btnX, y, btnW, btnH), "Apply", buttonStyle))
        {
            ApplySettings();
        }

        if (GUI.Button(new Rect(btnX + btnW + btnGap, y, btnW, btnH), "Back", buttonStyle))
        {
            showSettings = false;
        }

        y += btnH + 30;

        // リセットボタン
        var resetStyle = new GUIStyle(buttonStyle);
        resetStyle.fontSize = 14;
        if (GUI.Button(new Rect(cx - 80, y, 160, 35), "Reset to Defaults", resetStyle))
        {
            ResetToDefaults();
        }
    }

    void LoadTempSettings()
    {
        if (SaveSystem.Instance != null)
        {
            var settings = SaveSystem.Instance.Settings;
            tempBgmVolume = settings.BgmVolume;
            tempSeVolume = settings.SeVolume;
            tempScreenShake = settings.ScreenShake;
            tempDamageNumbers = settings.DamageNumbers;
        }
        else
        {
            tempBgmVolume = 0.35f;
            tempSeVolume = 0.55f;
            tempScreenShake = true;
            tempDamageNumbers = true;
        }
    }

    void ApplySettings()
    {
        if (SaveSystem.Instance != null)
        {
            var settings = SaveSystem.Instance.Settings;
            settings.BgmVolume = tempBgmVolume;
            settings.SeVolume = tempSeVolume;
            settings.ScreenShake = tempScreenShake;
            settings.DamageNumbers = tempDamageNumbers;
            SaveSystem.Instance.SaveSettings();
        }

        // AudioManagerに音量を適用
        ApplyAudioSettings();

        showSettings = false;
    }

    void ApplyAudioSettings()
    {
        if (AudioManager.Instance != null)
        {
            var bgmSource = AudioManager.Instance.GetComponent<AudioSource>();
            if (bgmSource != null) bgmSource.volume = tempBgmVolume;

            // SE用のAudioSourceは2番目
            var sources = AudioManager.Instance.GetComponents<AudioSource>();
            if (sources.Length > 1) sources[1].volume = tempSeVolume;
        }
    }

    void ResetToDefaults()
    {
        tempBgmVolume = 0.35f;
        tempSeVolume = 0.55f;
        tempScreenShake = true;
        tempDamageNumbers = true;
    }

    void Restart()
    {
        isTransitioning = true;
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GoToTitle()
    {
        isTransitioning = true;
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    void OnDestroy()
    {
        // シーン破棄時にポーズ状態をリセット
        IsPaused = false;
    }
}
