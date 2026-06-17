using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

/// <summary>初回プレイ時のチュートリアル表示</summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    const string TutorialCompletedKey = "TutorialCompleted";

    enum TutorialStep
    {
        None,
        Movement,
        AutoAttack,
        CollectXp,
        LevelUp,
        Completed
    }

    TutorialStep currentStep = TutorialStep.None;
    float stepTimer;
    float displayAlpha = 1f;
    bool showTutorial;

    GUIStyle messageStyle, skipStyle;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 既にチュートリアル完了済みならスキップ
        if (PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1)
        {
            currentStep = TutorialStep.Completed;
            return;
        }

        showTutorial = true;
        currentStep = TutorialStep.Movement;
        stepTimer = 0f;
    }

    void Update()
    {
        if (!showTutorial || currentStep == TutorialStep.Completed) return;
        if (GameState.GameOver) return;

        stepTimer += Time.deltaTime;

        // チュートリアルスキップ（Tabキー）
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.Tab))
#endif
        {
            CompleteTutorial();
            return;
        }

        // ステップ進行
        switch (currentStep)
        {
            case TutorialStep.Movement:
                if (stepTimer > 5f) AdvanceStep();
                break;

            case TutorialStep.AutoAttack:
                if (GameState.KillCount >= 3) AdvanceStep();
                else if (stepTimer > 8f) AdvanceStep();
                break;

            case TutorialStep.CollectXp:
                var player = FindObjectOfType<Player>();
                if (player != null && player.xp > 0) AdvanceStep();
                else if (stepTimer > 10f) AdvanceStep();
                break;

            case TutorialStep.LevelUp:
                var p = FindObjectOfType<Player>();
                if (p != null && p.level >= 2) AdvanceStep();
                else if (stepTimer > 15f) AdvanceStep();
                break;
        }

        // フェード処理
        if (stepTimer < 0.5f)
            displayAlpha = stepTimer / 0.5f;
        else
            displayAlpha = 1f;
    }

    void AdvanceStep()
    {
        currentStep++;
        stepTimer = 0f;

        if (currentStep == TutorialStep.Completed)
        {
            CompleteTutorial();
        }
    }

    void CompleteTutorial()
    {
        currentStep = TutorialStep.Completed;
        showTutorial = false;
        PlayerPrefs.SetInt(TutorialCompletedKey, 1);
        PlayerPrefs.Save();
    }

    void OnGUI()
    {
        if (!showTutorial || currentStep == TutorialStep.Completed || currentStep == TutorialStep.None) return;
        if (GameState.GameOver) return;

        // レベルアップ選択中は非表示
        var player = FindObjectOfType<Player>();
        if (player != null && player.currentOptions != null) return;

        if (messageStyle == null)
        {
            messageStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            messageStyle.normal.textColor = Color.white;

            skipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            skipStyle.normal.textColor = new Color(1f, 1f, 1f, 0.6f);
        }

        string message = currentStep switch
        {
            TutorialStep.Movement => "WASD または 矢印キーで移動",
            TutorialStep.AutoAttack => "攻撃は自動！敵に近づくだけ",
            TutorialStep.CollectXp => "緑の光（経験値）を集めよう",
            TutorialStep.LevelUp => "レベルアップで強化を選択！",
            _ => ""
        };

        if (string.IsNullOrEmpty(message)) return;

        // 背景
        float panelW = 420f;
        float panelH = 70f;
        float panelX = (Screen.width - panelW) / 2f;
        float panelY = Screen.height * 0.75f;

        GUI.color = new Color(0f, 0f, 0f, 0.7f * displayAlpha);
        GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);

        // メッセージ
        GUI.color = new Color(1f, 1f, 1f, displayAlpha);
        GUI.Label(new Rect(panelX, panelY + 10f, panelW, 36f), message, messageStyle);

        // スキップヒント
        GUI.color = new Color(1f, 1f, 1f, 0.5f * displayAlpha);
        GUI.Label(new Rect(panelX, panelY + 46f, panelW, 20f), "[Tab] スキップ", skipStyle);

        GUI.color = Color.white;
    }

    /// <summary>チュートリアルをリセット（デバッグ用）</summary>
    public static void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TutorialCompletedKey);
        PlayerPrefs.Save();
    }
}
