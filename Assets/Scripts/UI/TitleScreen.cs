using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>タイトル画面UI（TitleSceneに配置）</summary>
public class TitleScreen : MonoBehaviour
{
    GUIStyle titleStyle, subtitleStyle, buttonStyle, infoStyle;

    void Start()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    void OnGUI()
    {
        if (titleStyle == null) InitStyles();

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        GUI.Label(new Rect(0, cy - 150, Screen.width, 90), "Vampire Survivors Mini", titleStyle);
        GUI.Label(new Rect(0, cy - 60,  Screen.width, 40), "生き残れ、強くなれ", subtitleStyle);

        float btnW = 220f, btnH = 64f;
        if (GUI.Button(new Rect(cx - btnW / 2f, cy + 10, btnW, btnH), "PLAY", buttonStyle))
            SceneManager.LoadScene("SampleScene");

        GUI.Label(new Rect(0, cy + 120, Screen.width, 28), "WASD / 矢印キー で移動　攻撃は自動", infoStyle);
        GUI.Label(new Rect(0, cy + 148, Screen.width, 28), "レベルアップ時: 1/2/3 キーまたはクリックで選択", infoStyle);
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

        infoStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter
        };
        infoStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
    }
}
