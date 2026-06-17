using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 空間ハッシュグリッドによる高速な近傍検索
/// 敵の衝突判定をO(n²)からO(n)に最適化
/// </summary>
public class SpatialHash
{
    public static SpatialHash Instance { get; private set; }

    readonly float cellSize;
    readonly Dictionary<long, List<Enemy>> grid = new Dictionary<long, List<Enemy>>();
    readonly List<Enemy> emptyList = new List<Enemy>();

    // 再利用用のリスト（GC軽減）
    readonly List<Enemy> queryResult = new List<Enemy>();
    readonly HashSet<long> checkedCells = new HashSet<long>();

    public SpatialHash(float cellSize = 2f)
    {
        this.cellSize = cellSize;
        Instance = this;
    }

    /// <summary>グリッドをクリア（毎フレーム呼ぶ）</summary>
    public void Clear()
    {
        foreach (var list in grid.Values)
            list.Clear();
    }

    /// <summary>敵をグリッドに登録</summary>
    public void Insert(Enemy enemy)
    {
        if (enemy == null) return;
        long key = GetKey(enemy.transform.position);

        if (!grid.TryGetValue(key, out var list))
        {
            list = new List<Enemy>(8);
            grid[key] = list;
        }
        list.Add(enemy);
    }

    /// <summary>全敵を再登録（GameState.Enemiesから）</summary>
    public void Rebuild()
    {
        Clear();
        foreach (var enemy in GameState.Enemies)
        {
            Insert(enemy);
        }
    }

    /// <summary>指定位置の半径内にいる敵を取得</summary>
    public List<Enemy> Query(Vector3 pos, float radius)
    {
        queryResult.Clear();
        checkedCells.Clear();

        int cellRadius = Mathf.CeilToInt(radius / cellSize);
        int cx = Mathf.FloorToInt(pos.x / cellSize);
        int cy = Mathf.FloorToInt(pos.y / cellSize);

        float sqrRadius = radius * radius;

        for (int dx = -cellRadius; dx <= cellRadius; dx++)
        {
            for (int dy = -cellRadius; dy <= cellRadius; dy++)
            {
                long key = PackKey(cx + dx, cy + dy);
                if (checkedCells.Contains(key)) continue;
                checkedCells.Add(key);

                if (!grid.TryGetValue(key, out var list)) continue;

                foreach (var enemy in list)
                {
                    if (enemy == null) continue;
                    if ((enemy.transform.position - pos).sqrMagnitude <= sqrRadius)
                    {
                        queryResult.Add(enemy);
                    }
                }
            }
        }

        return queryResult;
    }

    /// <summary>指定位置に最も近い敵を取得</summary>
    public Enemy QueryNearest(Vector3 pos, float maxRadius = 20f)
    {
        Enemy nearest = null;
        float bestSqr = maxRadius * maxRadius;

        int maxCellRadius = Mathf.CeilToInt(maxRadius / cellSize);
        int cx = Mathf.FloorToInt(pos.x / cellSize);
        int cy = Mathf.FloorToInt(pos.y / cellSize);

        // 中心から外側に向かって探索（近い敵が見つかったら早期終了）
        for (int r = 0; r <= maxCellRadius; r++)
        {
            bool foundInRing = false;

            // リング状に探索
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    // リングの外周のみ
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;

                    long key = PackKey(cx + dx, cy + dy);
                    if (!grid.TryGetValue(key, out var list)) continue;

                    foreach (var enemy in list)
                    {
                        if (enemy == null) continue;
                        float sqr = (enemy.transform.position - pos).sqrMagnitude;
                        if (sqr < bestSqr)
                        {
                            bestSqr = sqr;
                            nearest = enemy;
                            foundInRing = true;
                        }
                    }
                }
            }

            // このリングで見つかり、次のリングの最小距離より近いなら終了
            if (foundInRing && bestSqr < (r + 1) * cellSize * (r + 1) * cellSize)
                break;
        }

        return nearest;
    }

    /// <summary>指定位置の単一セルの敵を取得（高速）</summary>
    public List<Enemy> GetCell(Vector3 pos)
    {
        long key = GetKey(pos);
        return grid.TryGetValue(key, out var list) ? list : emptyList;
    }

    long GetKey(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / cellSize);
        int y = Mathf.FloorToInt(pos.y / cellSize);
        return PackKey(x, y);
    }

    long PackKey(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }
}
