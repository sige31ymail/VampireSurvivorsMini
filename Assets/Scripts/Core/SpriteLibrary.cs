using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resources/ 以下の手描きスプライトPNGを読み込むヘルパー。
///
/// 設計方針:
///   - 画像が存在すれば Sprite を返し、無ければ null を返す。
///     呼び出し側は null のとき従来のコード生成（円/四角の組み合わせ）にフォールバックする。
///   - 手描き・イラスト調を想定し filterMode = Bilinear（なめらかな拡大）。
///   - pivot は中央、PPU = テクスチャの高さ。
///     これにより「スプライトの高さ = 1 ワールド単位」に正規化されるため、
///     画像の解像度（256px でも 512px でも）に依存せず同じ見た目サイズになる。
///     最終的な大きさは Attach の localScale や親の transform.localScale で調整する。
///
/// 画像の置き場所（拡張子なしのパスで指定）:
///   Assets/Resources/Characters/player.png        → "Characters/player"
///   Assets/Resources/Characters/Enemies/chaser.png → "Characters/Enemies/chaser"
/// </summary>
public static class SpriteLibrary
{
    // 結果（null を含む）をキャッシュして、毎回の Resources.Load を避ける。
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    /// <summary>
    /// Resources 以下のパス（拡張子なし）から Sprite を取得する。
    /// 見つからなければ null。
    /// </summary>
    public static Sprite Get(string resourcePath)
    {
        if (cache.TryGetValue(resourcePath, out var cached))
            return cached;

        Sprite sprite = null;
        var tex = Resources.Load<Texture2D>(resourcePath);
        if (tex != null)
        {
            tex.filterMode = FilterMode.Bilinear;
            sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), // 中央ピボット
                tex.height);             // PPU=高さ → スプライト高さが 1 ワールド単位
        }

        cache[resourcePath] = sprite; // null もキャッシュ（毎フレームの再読込防止）
        return sprite;
    }

    /// <summary>指定パスに画像が存在するか。</summary>
    public static bool Has(string resourcePath) => Get(resourcePath) != null;

    /// <summary>
    /// 親に手描きスプライトの子オブジェクトを付ける。
    /// localScale は親スケールに乗る最終倍率（スプライト高さ1単位 × localScale × 親スケール = 見かけ高さ）。
    /// 返り値の SpriteRenderer は被弾フラッシュ等に使える。
    /// </summary>
    public static SpriteRenderer Attach(Transform parent, Sprite sprite,
                                        float localScale, int sortingOrder, string name = "Art")
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localScale = Vector3.one * localScale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        return sr;
    }
}
