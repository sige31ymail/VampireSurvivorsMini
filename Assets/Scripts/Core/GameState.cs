using System.Collections.Generic;
using UnityEngine;

/// <summary>ゲーム全体の状態（敵リスト・経過時間など）</summary>
public static class GameState
{
    public static readonly List<Enemy> Enemies = new List<Enemy>();
    public static bool GameOver;
    public static float ElapsedTime;
    public static int KillCount;

    static SpatialHash spatialHash;

    public static void Reset()
    {
        Enemies.Clear();
        GameOver = false;
        ElapsedTime = 0f;
        KillCount = 0;
        spatialHash = new SpatialHash(2f);
    }

    /// <summary>毎フレーム呼び出して空間ハッシュを更新</summary>
    public static void UpdateSpatialHash()
    {
        if (spatialHash == null)
            spatialHash = new SpatialHash(2f);
        spatialHash.Rebuild();
    }

    public static Enemy FindNearest(Vector3 from)
    {
        // 空間ハッシュを使用（利用可能な場合）
        if (spatialHash != null)
        {
            var result = spatialHash.QueryNearest(from, 20f);
            if (result != null) return result;
        }

        // フォールバック：従来の線形探索
        Enemy nearest = null;
        float best = float.MaxValue;
        foreach (var e in Enemies)
        {
            if (e == null) continue;
            float d = (e.transform.position - from).sqrMagnitude;
            if (d < best) { best = d; nearest = e; }
        }
        return nearest;
    }

    /// <summary>指定位置の半径内にいる敵全員にダメージ（範囲攻撃用）</summary>
    public static void DamageEnemiesWithin(Vector3 pos, float radius, int damage, float knockback = 0f)
    {
        // 空間ハッシュを使用（利用可能な場合）
        if (spatialHash != null)
        {
            var enemies = spatialHash.Query(pos, radius);
            foreach (var e in enemies)
            {
                if (e == null) continue;
                Vector3 diff = e.transform.position - pos;
                e.TakeDamage(damage, diff.normalized * knockback);
            }
            return;
        }

        // フォールバック：従来の線形探索
        float sqr = radius * radius;
        int count = Enemies.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            if (i >= Enemies.Count) continue;
            var e = Enemies[i];
            if (e == null) continue;
            Vector3 diff = e.transform.position - pos;
            if (diff.sqrMagnitude <= sqr)
                e.TakeDamage(damage, diff.normalized * knockback);
        }
    }

    /// <summary>指定位置の半径内にいる敵を取得（ダメージなし）</summary>
    public static List<Enemy> GetEnemiesWithin(Vector3 pos, float radius)
    {
        if (spatialHash != null)
        {
            return spatialHash.Query(pos, radius);
        }

        // フォールバック
        var result = new List<Enemy>();
        float sqr = radius * radius;
        foreach (var e in Enemies)
        {
            if (e == null) continue;
            if ((e.transform.position - pos).sqrMagnitude <= sqr)
                result.Add(e);
        }
        return result;
    }

    /// <summary>指定位置の単一セル内の敵を取得（高速・近距離用）</summary>
    public static List<Enemy> GetEnemiesInCell(Vector3 pos)
    {
        if (spatialHash != null)
            return spatialHash.GetCell(pos);
        return new List<Enemy>();
    }
}
