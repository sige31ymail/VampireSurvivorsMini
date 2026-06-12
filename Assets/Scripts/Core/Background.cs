using UnityEngine;

/// <summary>手続き生成グリッド背景。カメラに追従しつつワールド座標でタイルが固定される。</summary>
public class Background : MonoBehaviour
{
    Material mat;

    const float TileWorldSize = 2f;   // グリッド1マス = 2ワールド単位
    const float QuadHalfSize  = 120f; // クワッドの半辺（カメラ外を十分覆うサイズ）

    void Start()
    {
        var shader = Shader.Find("Sprites/Default")
                  ?? Shader.Find("Unlit/Texture");

        mat = new Material(shader);
        mat.mainTexture      = CreateTileTexture();
        mat.mainTextureScale = Vector2.one * (QuadHalfSize * 2f / TileWorldSize);

        var go = new GameObject("BackgroundMesh");
        go.transform.SetParent(transform);
        go.AddComponent<MeshFilter>().mesh = CreateQuadMesh();
        var mr = go.AddComponent<MeshRenderer>();
        mr.material     = mat;
        mr.sortingOrder = -100;
    }

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;

        // クワッドをカメラ中心に追従
        var p = cam.transform.position;
        p.z = 0f;
        transform.position = p;

        // オフセットを調整してグリッドをワールド座標に固定
        mat.mainTextureOffset = new Vector2(p.x / TileWorldSize, p.y / TileWorldSize);
    }

    Texture2D CreateTileTexture()
    {
        const int size = 64;
        var tex  = new Texture2D(size, size, TextureFormat.RGB24, false);
        var bg   = new Color(0.08f, 0.08f, 0.12f);
        var line = new Color(0.14f, 0.14f, 0.20f);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            tex.SetPixel(x, y, x == 0 || y == 0 ? line : bg);

        tex.Apply();
        tex.wrapMode   = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    Mesh CreateQuadMesh()
    {
        float s = QuadHalfSize;
        var mesh = new Mesh();
        mesh.vertices  = new Vector3[] {
            new Vector3(-s, -s, 0), new Vector3( s, -s, 0),
            new Vector3( s,  s, 0), new Vector3(-s,  s, 0)
        };
        mesh.uv = new Vector2[] {
            Vector2.zero, Vector2.right, Vector2.one, Vector2.up
        };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();
        return mesh;
    }
}
