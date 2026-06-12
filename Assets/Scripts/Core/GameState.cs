using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>ゲーム全体の状態（敵リスト・経過時間など）</summary>
public static class GameState
{
    public static readonly List<Enemy> Enemies = new List<Enemy>();
    public static bool GameOver;
    public static float ElapsedTime;
    public static int KillCount;

    public static void Reset()
    {
        Enemies.Clear();
        GameOver = false;
        ElapsedTime = 0f;
        KillCount = 0;
    }

    public static Enemy FindNearest(Vector3 from)
    {
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

    /// <summary>指定位置の半径内にいる敵全員にダメージ（範囲攻撃用）。knockback>0で中心から押し出す</summary>
    public static void DamageEnemiesWithin(Vector3 pos, float radius, int damage, float knockback = 0f)
    {
        float sqr = radius * radius;
        // TakeDamage内でリストが変化するためスナップショットを取って回す
        var snapshot = Enemies.ToArray();
        foreach (var e in snapshot)
        {
            if (e == null) continue;
            Vector3 diff = e.transform.position - pos;
            if (diff.sqrMagnitude <= sqr)
                e.TakeDamage(damage, diff.normalized * knockback);
        }
    }
}
