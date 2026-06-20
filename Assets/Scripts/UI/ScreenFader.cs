using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 画面遷移用のフェード（シーンをまたいで永続）。最前面の全画面Imageを黒で出し入れする。
/// timeScale=0 でも動くよう unscaledDeltaTime を使用。
/// </summary>
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    Image overlay;
    const float Dur = 0.25f;

    public static ScreenFader Get()
    {
        if (Instance == null)
        {
            var go = new GameObject("ScreenFader");
            Instance = go.AddComponent<ScreenFader>();
        }
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var canvas = UIKit.CreateCanvas("FaderCanvas", 5000); // 全UIより前面
        canvas.transform.SetParent(transform, false);
        overlay = UIKit.FullScreen(canvas.transform, new Color(0f, 0f, 0f, 0f), "Fade");
        overlay.raycastTarget = false;
    }

    /// <summary>フェードアウト→シーン読込→フェードイン。</summary>
    public void FadeToScene(string sceneName) => StartCoroutine(FadeLoad(sceneName, -1));
    public void FadeToScene(int buildIndex)   => StartCoroutine(FadeLoad(null, buildIndex));

    IEnumerator FadeLoad(string sceneName, int buildIndex)
    {
        overlay.raycastTarget = true;
        yield return Fade(0f, 1f);

        Time.timeScale = 1f;
        if (sceneName != null) SceneManager.LoadScene(sceneName);
        else                   SceneManager.LoadScene(buildIndex);

        yield return Fade(1f, 0f);
        overlay.raycastTarget = false;
    }

    IEnumerator Fade(float from, float to)
    {
        float e = 0f;
        var c = overlay.color;
        while (e < Dur)
        {
            e += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(from, to, Mathf.Clamp01(e / Dur));
            overlay.color = c;
            yield return null;
        }
        c.a = to;
        overlay.color = c;
    }
}
