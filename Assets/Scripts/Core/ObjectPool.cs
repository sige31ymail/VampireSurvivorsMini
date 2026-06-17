using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 汎用オブジェクトプールシステム
/// Projectile, XpGem, DamagePopup, DeathParticle などの頻繁に生成/破棄されるオブジェクトを再利用
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    // プール辞書：型ごとにプールを管理
    readonly Dictionary<Type, Queue<Component>> pools = new Dictionary<Type, Queue<Component>>();
    readonly Dictionary<Type, Transform> poolParents = new Dictionary<Type, Transform>();

    // 統計情報
    readonly Dictionary<Type, int> totalCreated = new Dictionary<Type, int>();
    readonly Dictionary<Type, int> totalReused = new Dictionary<Type, int>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>プールから取得または新規作成</summary>
    public T Get<T>(Action<GameObject> setup = null) where T : Component
    {
        var type = typeof(T);
        EnsurePool(type);

        var pool = pools[type];

        if (pool.Count > 0)
        {
            var obj = pool.Dequeue() as T;
            if (obj != null)
            {
                obj.gameObject.SetActive(true);
                totalReused[type]++;
                return obj;
            }
        }

        // 新規作成
        var go = new GameObject(type.Name);
        go.transform.SetParent(poolParents[type]);
        setup?.Invoke(go);
        var component = go.GetComponent<T>() ?? go.AddComponent<T>();
        totalCreated[type]++;
        return component;
    }

    /// <summary>プールへ返却</summary>
    public void Return<T>(T obj) where T : Component
    {
        if (obj == null) return;

        var type = typeof(T);
        EnsurePool(type);

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(poolParents[type]);
        pools[type].Enqueue(obj);
    }

    /// <summary>プールの初期化（事前生成）</summary>
    public void Prewarm<T>(int count, Action<GameObject> setup = null) where T : Component
    {
        var type = typeof(T);
        EnsurePool(type);

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject(type.Name);
            go.transform.SetParent(poolParents[type]);
            setup?.Invoke(go);
            var component = go.GetComponent<T>() ?? go.AddComponent<T>();
            go.SetActive(false);
            pools[type].Enqueue(component);
            totalCreated[type]++;
        }
    }

    void EnsurePool(Type type)
    {
        if (!pools.ContainsKey(type))
        {
            pools[type] = new Queue<Component>();
            var parent = new GameObject($"Pool_{type.Name}");
            parent.transform.SetParent(transform);
            poolParents[type] = parent.transform;
            totalCreated[type] = 0;
            totalReused[type] = 0;
        }
    }

    /// <summary>全プールをクリア（シーン遷移時など）</summary>
    public void ClearAll()
    {
        foreach (var kvp in pools)
        {
            while (kvp.Value.Count > 0)
            {
                var obj = kvp.Value.Dequeue();
                if (obj != null) Destroy(obj.gameObject);
            }
        }
        pools.Clear();
        poolParents.Clear();
        totalCreated.Clear();
        totalReused.Clear();
    }

    /// <summary>デバッグ用：プール統計情報</summary>
    public string GetStats()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Object Pool Stats ===");
        foreach (var kvp in pools)
        {
            var type = kvp.Key;
            int available = kvp.Value.Count;
            int created = totalCreated.ContainsKey(type) ? totalCreated[type] : 0;
            int reused = totalReused.ContainsKey(type) ? totalReused[type] : 0;
            float reuseRate = created > 0 ? (float)reused / (created + reused) * 100 : 0;
            sb.AppendLine($"{type.Name}: Available={available}, Created={created}, Reused={reused} ({reuseRate:F1}%)");
        }
        return sb.ToString();
    }
}

/// <summary>プール対応オブジェクトのインターフェース</summary>
public interface IPoolable
{
    /// <summary>プールから取り出された時の初期化</summary>
    void OnSpawn();
    /// <summary>プールへ返却される時のクリーンアップ</summary>
    void OnDespawn();
}
