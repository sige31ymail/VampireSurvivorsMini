using UnityEngine;

/// <summary>
/// キャラの足元に半透明の楕円シャドウ（接地影）を付けるヘルパー。
/// 手描きスプライト／従来のコード生成のどちらでも有効で、キャラを地面に馴染ませ立体感を出す。
///
/// 影は CircleSprite を平たく潰した楕円。sortingOrder はキャラより低く地面より高い値にする。
/// 親の子として付くので、敵の死亡時などは親ごと破棄される。
/// 注意: 敵の被弾フラッシュ(allRenderers)に含めたくない場合は、レンダラーをキャッシュした
///       「後」に Attach すること。
/// </summary>
public static class GroundShadow
{
    /// <param name="parent">影を付ける対象（キャラのtransform）</param>
    /// <param name="width">影の横幅（親ローカル単位）</param>
    /// <param name="height">影の高さ（親ローカル単位、横幅より小さく＝楕円）</param>
    /// <param name="yOffset">足元へのオフセット（親ローカル単位、通常マイナス）</param>
    /// <param name="alpha">影の濃さ</param>
    public static void Attach(Transform parent, float width, float height,
                              float yOffset, float alpha = 0.28f)
    {
        var go = new GameObject("Shadow");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, yOffset, 0f);
        go.transform.localScale = new Vector3(width, height, 1f);
        go.transform.rotation = Quaternion.identity; // 親が回転(Dasher等)していても影は水平に保つ

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.CircleSprite;
        sr.color = new Color(0f, 0f, 0f, alpha);
        sr.sortingOrder = -5; // 地面(-10/-100)より上、キャラ(>=3)より下
    }
}
