using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

/// <summary>
/// コードで uGUI を組むための共通ヘルパー。全UIクラスから再利用する。
/// このプロジェクトはシーン/プレハブを使わず全GameObjectをコード生成するため、UIも同方式に統一。
/// </summary>
public static class UIKit
{
    /// <summary>新Input System用のEventSystemを保証（無ければ生成）。</summary>
    public static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null) return;

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>(); // 旧StandaloneInputModuleでは新Inputで動かない
    }

    /// <summary>ScreenSpaceOverlay の Canvas を生成（解像度追従つき）。</summary>
    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        EnsureEventSystem();

        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    // ── 基本ノード生成 ───────────────────────
    public static GameObject Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    /// <summary>anchor/pivot/位置/サイズをまとめて設定。</summary>
    public static RectTransform SetRect(Component c, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 anchoredPos, Vector2 size)
    {
        var rt = (RectTransform)c.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        return rt;
    }

    /// <summary>親いっぱいに広げる（オプションで余白）。</summary>
    public static RectTransform Stretch(Component c, float padding = 0f)
    {
        var rt = (RectTransform)c.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
        return rt;
    }

    // ── パーツ ───────────────────────────────
    public static Image Panel(Transform parent, Color color, string name = "Panel")
    {
        var go = Node(name, parent);
        var img = go.AddComponent<Image>();
        img.sprite = UITheme.RoundedSprite;
        img.type = Image.Type.Sliced;
        img.color = color;
        return img;
    }

    /// <summary>単色の四角（バー背景・オーバーレイ等）。角丸なし。</summary>
    public static Image Solid(Transform parent, Color color, string name = "Solid")
    {
        var go = Node(name, parent);
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    public static TextMeshProUGUI Label(Transform parent, string text, float fontSize,
        Color color, TextAlignmentOptions align = TextAlignmentOptions.Left, string name = "Label")
    {
        var go = Node(name, parent);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (UITheme.Font != null) t.font = UITheme.Font;
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    /// <summary>角丸ボタン（色遷移＋ホバー/押し込みのスケール）。ラベルTMPを子に持つ。</summary>
    public static Button Button(Transform parent, string label, Action onClick,
        float fontSize = 28f, string name = "Button")
    {
        var go = Node(name, parent);
        var img = go.AddComponent<Image>();
        img.sprite = UITheme.RoundedSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white; // 実際の色は Button.colors で乗算

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor = UITheme.ButtonNormal;
        cb.highlightedColor = UITheme.ButtonHover;
        cb.pressedColor = UITheme.ButtonPressed;
        cb.selectedColor = UITheme.ButtonNormal;
        cb.disabledColor = UITheme.ButtonDisabled;
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        btn.colors = cb;
        if (onClick != null) btn.onClick.AddListener(() => onClick());

        go.AddComponent<UIButtonFx>();

        var lbl = Label(go.transform, label, fontSize, UITheme.TextMain, TextAlignmentOptions.Center, "Text");
        Stretch(lbl, 6f);

        return btn;
    }

    /// <summary>HP/XP等のバー。背景＋フィルを作り、フィルImage(Filled横)を返す。ratioは Image.fillAmount。</summary>
    public static Image Bar(Transform parent, Color fillColor, string name = "Bar")
    {
        var bg = Solid(parent, new Color(0f, 0f, 0f, 0.55f), name);
        bg.sprite = UITheme.RoundedSprite;
        bg.type = Image.Type.Sliced;

        var fillGo = Node("Fill", bg.transform);
        var fill = fillGo.AddComponent<Image>();
        fill.sprite = UITheme.RoundedSprite;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.color = fillColor;
        fill.fillAmount = 1f;
        Stretch(fill, 2f);
        return fill;
    }

    /// <summary>親いっぱいの単色Image（フェード等のオーバーレイ用）。</summary>
    public static Image FullScreen(Transform parent, Color color, string name = "Overlay")
    {
        var img = Solid(parent, color, name);
        Stretch(img);
        return img;
    }
}
