using System;
using UnityEngine;

/// <summary>
/// 設定メニュー（タイトル画面用）
/// PauseMenuの設定パネルと共通の設定を管理
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    public static event Action OnSettingsChanged;

    public bool IsVisible { get; private set; }

    GUIStyle titleStyle;
    GUIStyle buttonStyle;
    GUIStyle labelStyle;
    GUIStyle sliderLabelStyle;
    GUIStyle sectionStyle;

    // 設定の一時値
    float tempBgmVolume;
    float tempSeVolume;
    bool tempFullscreen;
    bool tempScreenShake;
    bool tempDamageNumbers;
    float tempUiScale;

    Resolution[] resolutions;
    int tempResolutionIndex;
    string[] resolutionOptions;

    void Awake()
    {
        resolutions = Screen.resolutions;
        resolutionOptions = new string[resolutions.Length + 1];
        resolutionOptions[0] = "Default";
        for (int i = 0; i < resolutions.Length; i++)
        {
            var r = resolutions[i];
            resolutionOptions[i + 1] = $"{r.width}x{r.height} @{r.refreshRateRatio.value:F0}Hz";
        }
    }

    public void Show()
    {
        IsVisible = true;
        LoadCurrentSettings();
    }

    public void Hide()
    {
        IsVisible = false;
    }

    void LoadCurrentSettings()
    {
        if (SaveSystem.Instance != null)
        {
            var settings = SaveSystem.Instance.Settings;
            tempBgmVolume = settings.BgmVolume;
            tempSeVolume = settings.SeVolume;
            tempFullscreen = settings.Fullscreen;
            tempScreenShake = settings.ScreenShake;
            tempDamageNumbers = settings.DamageNumbers;
            tempUiScale = settings.UiScale;
            tempResolutionIndex = settings.ResolutionIndex + 1; // -1 offset for "Default"
        }
        else
        {
            tempBgmVolume = 0.35f;
            tempSeVolume = 0.55f;
            tempFullscreen = true;
            tempScreenShake = true;
            tempDamageNumbers = true;
            tempUiScale = 1f;
            tempResolutionIndex = 0;
        }
    }

    void OnGUI()
    {
        if (!IsVisible) return;

        InitStyles();

        // 背景オーバーレイ
        GUI.color = new Color(0, 0, 0, 0.9f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float panelW = Mathf.Min(450f, Screen.width - 40f);
        float panelX = cx - panelW / 2f;
        float startY = Screen.height * 0.08f;

        // タイトル
        GUI.Label(new Rect(0, startY, Screen.width, 50), "SETTINGS", titleStyle);

        float y = startY + 70;
        float rowH = 40f;
        float labelW = 160f;
        float controlX = panelX + labelW;
        float controlW = panelW - labelW - 60f;
        float valueW = 50f;

        // === Audio Section ===
        GUI.Label(new Rect(panelX, y, panelW, 30), "Audio", sectionStyle);
        y += 35;

        // BGM Volume
        GUI.Label(new Rect(panelX, y, labelW, rowH), "BGM Volume", labelStyle);
        tempBgmVolume = GUI.HorizontalSlider(
            new Rect(controlX, y + 10, controlW, 20),
            tempBgmVolume, 0f, 1f);
        GUI.Label(new Rect(controlX + controlW + 5, y, valueW, rowH),
            $"{Mathf.RoundToInt(tempBgmVolume * 100)}%", sliderLabelStyle);
        y += rowH;

        // SE Volume
        GUI.Label(new Rect(panelX, y, labelW, rowH), "SE Volume", labelStyle);
        tempSeVolume = GUI.HorizontalSlider(
            new Rect(controlX, y + 10, controlW, 20),
            tempSeVolume, 0f, 1f);
        GUI.Label(new Rect(controlX + controlW + 5, y, valueW, rowH),
            $"{Mathf.RoundToInt(tempSeVolume * 100)}%", sliderLabelStyle);
        y += rowH + 15;

        // === Display Section ===
        GUI.Label(new Rect(panelX, y, panelW, 30), "Display", sectionStyle);
        y += 35;

        // Fullscreen
        GUI.Label(new Rect(panelX, y, labelW, rowH), "Fullscreen", labelStyle);
        tempFullscreen = GUI.Toggle(new Rect(controlX, y + 8, 30, 30), tempFullscreen, "");
        y += rowH;

        // Resolution
        GUI.Label(new Rect(panelX, y, labelW, rowH), "Resolution", labelStyle);
        if (GUI.Button(new Rect(controlX, y + 5, controlW, 28), resolutionOptions[tempResolutionIndex]))
        {
            tempResolutionIndex = (tempResolutionIndex + 1) % resolutionOptions.Length;
        }
        y += rowH + 15;

        // === Gameplay Section ===
        GUI.Label(new Rect(panelX, y, panelW, 30), "Gameplay", sectionStyle);
        y += 35;

        // Screen Shake
        GUI.Label(new Rect(panelX, y, labelW, rowH), "Screen Shake", labelStyle);
        tempScreenShake = GUI.Toggle(new Rect(controlX, y + 8, 30, 30), tempScreenShake, "");
        y += rowH;

        // Damage Numbers
        GUI.Label(new Rect(panelX, y, labelW, rowH), "Damage Numbers", labelStyle);
        tempDamageNumbers = GUI.Toggle(new Rect(controlX, y + 8, 30, 30), tempDamageNumbers, "");
        y += rowH;

        // UI Scale
        GUI.Label(new Rect(panelX, y, labelW, rowH), "UI Scale", labelStyle);
        tempUiScale = GUI.HorizontalSlider(
            new Rect(controlX, y + 10, controlW, 20),
            tempUiScale, 0.75f, 1.5f);
        GUI.Label(new Rect(controlX + controlW + 5, y, valueW, rowH),
            $"{Mathf.RoundToInt(tempUiScale * 100)}%", sliderLabelStyle);
        y += rowH + 25;

        // ボタン
        float btnW = 110f;
        float btnH = 45f;
        float btnGap = 15f;
        float totalBtnW = btnW * 3 + btnGap * 2;
        float btnX = cx - totalBtnW / 2f;

        if (GUI.Button(new Rect(btnX, y, btnW, btnH), "Apply", buttonStyle))
        {
            ApplySettings();
        }

        if (GUI.Button(new Rect(btnX + btnW + btnGap, y, btnW, btnH), "Reset", buttonStyle))
        {
            ResetToDefaults();
        }

        if (GUI.Button(new Rect(btnX + (btnW + btnGap) * 2, y, btnW, btnH), "Back", buttonStyle))
        {
            Hide();
        }
    }

    void InitStyles()
    {
        if (titleStyle != null) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 38,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = Color.white;

        sectionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold
        };
        sectionStyle.normal.textColor = new Color(0.9f, 0.8f, 0.4f);

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleLeft
        };
        labelStyle.normal.textColor = Color.white;

        sliderLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleRight
        };
        sliderLabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
    }

    void ApplySettings()
    {
        if (SaveSystem.Instance != null)
        {
            var settings = SaveSystem.Instance.Settings;
            settings.BgmVolume = tempBgmVolume;
            settings.SeVolume = tempSeVolume;
            settings.Fullscreen = tempFullscreen;
            settings.ScreenShake = tempScreenShake;
            settings.DamageNumbers = tempDamageNumbers;
            settings.UiScale = tempUiScale;
            settings.ResolutionIndex = tempResolutionIndex - 1;
            SaveSystem.Instance.SaveSettings();
        }

        // 設定を実際に適用
        ApplyDisplaySettings();
        ApplyAudioSettings();

        OnSettingsChanged?.Invoke();
    }

    void ApplyDisplaySettings()
    {
        // フルスクリーン
        Screen.fullScreen = tempFullscreen;

        // 解像度
        if (tempResolutionIndex > 0 && tempResolutionIndex <= resolutions.Length)
        {
            var res = resolutions[tempResolutionIndex - 1];
            Screen.SetResolution(res.width, res.height, tempFullscreen);
        }
    }

    void ApplyAudioSettings()
    {
        if (AudioManager.Instance != null)
        {
            var sources = AudioManager.Instance.GetComponents<AudioSource>();
            if (sources.Length > 0) sources[0].volume = tempBgmVolume;
            if (sources.Length > 1) sources[1].volume = tempSeVolume;
        }
    }

    void ResetToDefaults()
    {
        tempBgmVolume = 0.35f;
        tempSeVolume = 0.55f;
        tempFullscreen = true;
        tempScreenShake = true;
        tempDamageNumbers = true;
        tempUiScale = 1f;
        tempResolutionIndex = 0;
    }
}
