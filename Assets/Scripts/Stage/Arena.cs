using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤー専用の距離ベース衝突判定（物理エンジン不使用）。
/// 無限マップに対応するため、障害物は「チャンク単位」で動的に登録/解除される。
/// ChunkManager が近傍チャンクの障害物だけを登録するので、判定対象は常に少数。
/// 障害物は円として扱う（敵は制限しない＝プレイヤーのみ）。
/// </summary>
public static class Arena
{
    public struct Obstacle { public Vector2 Center; public float Radius; }

    // チャンクキー → そのチャンクの障害物群
    static readonly Dictionary<long, List<Obstacle>> chunkObstacles = new Dictionary<long, List<Obstacle>>();

    public static bool Active { get; private set; }

    /// <summary>シーン再生成時に呼ぶ（静的状態のリセット）。</summary>
    public static void Reset()
    {
        chunkObstacles.Clear();
        Active = false;
    }

    public static void Enable() => Active = true;

    /// <summary>チャンクの障害物を登録（既存があれば置き換え）。</summary>
    public static void SetChunk(long key, List<Obstacle> obstacles)
    {
        chunkObstacles[key] = obstacles;
        Active = true;
    }

    /// <summary>チャンクの障害物を解除（チャンクのアンロード時）。</summary>
    public static void RemoveChunk(long key) => chunkObstacles.Remove(key);

    /// <summary>
    /// from から to へ移動しようとした結果の到達位置を返す。
    /// X軸・Y軸を個別に判定することで、障害物に沿った「ずり移動」を可能にする。
    /// </summary>
    public static Vector3 ClampMovement(Vector3 from, Vector3 to, float radius)
    {
        if (!Active) return to;

        Vector3 result = from;

        Vector3 stepX = new Vector3(to.x, from.y, from.z);
        if (!Blocked(stepX, radius)) result.x = to.x;

        Vector3 stepY = new Vector3(result.x, to.y, from.z);
        if (!Blocked(stepY, radius)) result.y = to.y;

        return result;
    }

    /// <summary>半径 radius の円を pos に置いたとき、いずれかの障害物に重なるか。</summary>
    public static bool Blocked(Vector3 pos, float radius)
    {
        if (!Active) return false;

        foreach (var kv in chunkObstacles)
        {
            var list = kv.Value;
            for (int i = 0; i < list.Count; i++)
            {
                float rr = list[i].Radius + radius;
                float dx = pos.x - list[i].Center.x;
                float dy = pos.y - list[i].Center.y;
                if (dx * dx + dy * dy < rr * rr) return true;
            }
        }
        return false;
    }
}
