using UnityEngine;

/// <summary>
/// パフォーマンス監視ツール（デバッグ用）
/// F3キーで表示/非表示を切り替え
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    public static PerformanceMonitor Instance { get; private set; }

    bool showStats = false;
    float deltaTime;
    float updateInterval = 0.5f;
    float accum;
    int frames;
    float fps;
    float msec;

    // メモリ
    long usedMemory;

    GUIStyle style;
    GUIStyle bgStyle;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.f3Key.wasPressedThisFrame)
            showStats = !showStats;
#else
        if (Input.GetKeyDown(KeyCode.F3))
            showStats = !showStats;
#endif

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        accum += Time.unscaledDeltaTime;
        frames++;

        if (accum >= updateInterval)
        {
            fps = frames / accum;
            msec = 1000f / fps;
            frames = 0;
            accum = 0;

            usedMemory = System.GC.GetTotalMemory(false) / 1024 / 1024;
        }
    }

    void OnGUI()
    {
        if (!showStats) return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.UpperLeft
            };
            style.normal.textColor = Color.white;

            bgStyle = new GUIStyle(GUI.skin.box);
        }

        float w = 220f;
        float h = 180f;
        float x = Screen.width - w - 10f;
        float y = 80f;

        // 背景
        GUI.color = new Color(0f, 0f, 0f, 0.8f);
        GUI.Box(new Rect(x - 5, y - 5, w + 10, h + 10), "");
        GUI.color = Color.white;

        float lineH = 20f;
        float cy = y;

        // FPS
        Color fpsColor = fps >= 55 ? Color.green : (fps >= 30 ? Color.yellow : Color.red);
        GUI.color = fpsColor;
        GUI.Label(new Rect(x, cy, w, lineH), $"FPS: {fps:F1} ({msec:F1} ms)", style);
        GUI.color = Color.white;
        cy += lineH;

        // 敵数
        int enemyCount = GameState.Enemies.Count;
        Color enemyColor = enemyCount < 100 ? Color.green : (enemyCount < 200 ? Color.yellow : Color.red);
        GUI.color = enemyColor;
        GUI.Label(new Rect(x, cy, w, lineH), $"Enemies: {enemyCount}", style);
        GUI.color = Color.white;
        cy += lineH;

        // メモリ
        GUI.Label(new Rect(x, cy, w, lineH), $"Memory: {usedMemory} MB", style);
        cy += lineH;

        // 経過時間
        int t = (int)GameState.ElapsedTime;
        GUI.Label(new Rect(x, cy, w, lineH), $"Time: {t / 60:00}:{t % 60:00}", style);
        cy += lineH;

        // キル数
        GUI.Label(new Rect(x, cy, w, lineH), $"Kills: {GameState.KillCount}", style);
        cy += lineH;

        // オブジェクトプール統計
        if (ObjectPool.Instance != null)
        {
            cy += 5f;
            GUI.Label(new Rect(x, cy, w, lineH), "--- Object Pool ---", style);
            cy += lineH;

            // 簡易表示
            GUI.Label(new Rect(x, cy, w, lineH * 3), "[F3] Toggle Stats", style);
        }
    }

    /// <summary>統計表示の切り替え</summary>
    public static void Toggle()
    {
        if (Instance != null)
            Instance.showStats = !Instance.showStats;
    }
}
