using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ボタンにホバー拡大・押し込み縮小の手触りを足す小コンポーネント。
/// uGUI Button の色遷移に加えて「動き」を与える。
/// </summary>
public class UIButtonFx : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public float hoverScale = 1.06f;
    public float pressScale = 0.96f;

    Vector3 baseScale = Vector3.one;
    Vector3 target = Vector3.one;
    bool hovering;

    void OnEnable()
    {
        target = baseScale;
        transform.localScale = baseScale;
    }

    void Update()
    {
        // unscaledDeltaTime：ポーズ/レベルアップ中(timeScale=0)でも反応する
        transform.localScale = Vector3.Lerp(transform.localScale, target,
            1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));
    }

    public void OnPointerEnter(PointerEventData e) { hovering = true;  target = baseScale * hoverScale; }
    public void OnPointerExit(PointerEventData e)  { hovering = false; target = baseScale; }
    public void OnPointerDown(PointerEventData e)  { target = baseScale * pressScale; }
    public void OnPointerUp(PointerEventData e)    { target = hovering ? baseScale * hoverScale : baseScale; }
}
