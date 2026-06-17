using System;
using UnityEngine;

/// <summary>プロシージャル生成によるBGM・SE管理（DontDestroyOnLoad）</summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    AudioSource bgmSource;
    AudioSource seSource;

    AudioClip hitClip, levelUpClip, dieClip, xpClip, gameOverClip;
    AudioClip normalBgm, bossBgm, lowHpBgm;

    bool isBossBgm;
    bool isLowHpBgm;

    const int Rate = 44100;

    // ノート周波数
    const float C3 = 130.81f, G3 = 196.00f, A3 = 220.00f;
    const float C4 = 261.63f, E4 = 329.63f, G4 = 392.00f, A4 = 440.00f;
    const float C5 = 523.25f, D5 = 587.33f, E5 = 659.26f, G5 = 784.00f, A5 = 880.00f;
    const float B4 = 493.88f, D4 = 293.66f, F4 = 349.23f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop   = true;
        bgmSource.volume = 0.35f;

        seSource = gameObject.AddComponent<AudioSource>();
        seSource.volume  = 0.55f;

        // SEを事前生成してキャッシュ
        hitClip      = BuildHit();
        levelUpClip  = BuildLevelUp();
        dieClip      = BuildDie();
        xpClip       = BuildXp();
        gameOverClip = BuildGameOver();

        // BGMを生成
        normalBgm = BuildBgm();
        bossBgm = BuildBossBgm();
        lowHpBgm = BuildLowHpBgm();

        bgmSource.clip = normalBgm;
        bgmSource.Play();
    }

    void Update()
    {
        if (GameState.GameOver) return;

        // ボス戦BGM切り替え
        bool bossPresent = false;
        foreach (var enemy in GameState.Enemies)
        {
            if (enemy != null && IsBossType(enemy.type))
            {
                bossPresent = true;
                break;
            }
        }

        // ピンチBGM（HP低下時）
        var player = FindObjectOfType<Player>();
        bool lowHp = player != null && player.hp < player.maxHp * 0.25f;

        // 優先度: ボス > ピンチ > 通常
        if (bossPresent && !isBossBgm)
        {
            CrossfadeTo(bossBgm);
            isBossBgm = true;
            isLowHpBgm = false;
        }
        else if (!bossPresent && isBossBgm)
        {
            CrossfadeTo(lowHp ? lowHpBgm : normalBgm);
            isBossBgm = false;
            isLowHpBgm = lowHp;
        }
        else if (!isBossBgm && lowHp && !isLowHpBgm)
        {
            CrossfadeTo(lowHpBgm);
            isLowHpBgm = true;
        }
        else if (!isBossBgm && !lowHp && isLowHpBgm)
        {
            CrossfadeTo(normalBgm);
            isLowHpBgm = false;
        }
    }

    bool IsBossType(EnemyType type)
    {
        return type == EnemyType.Boss || type == EnemyType.ForestBoss ||
               type == EnemyType.GraveyardBoss || type == EnemyType.CastleBoss;
    }

    void CrossfadeTo(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // ── 公開 API ─────────────────────────────
    public static void PlayHit()      => Instance?.seSource.PlayOneShot(Instance.hitClip);
    public static void PlayLevelUp()  => Instance?.seSource.PlayOneShot(Instance.levelUpClip);
    public static void PlayDie()      => Instance?.seSource.PlayOneShot(Instance.dieClip);
    public static void PlayXp()       => Instance?.seSource.PlayOneShot(Instance.xpClip);

    public static void PlayGameOver()
    {
        if (Instance == null) return;
        Instance.bgmSource.Stop();
        Instance.seSource.PlayOneShot(Instance.gameOverClip);
    }

    public void RestartBgm()
    {
        isBossBgm = false;
        isLowHpBgm = false;
        bgmSource.clip = normalBgm;
        bgmSource.Play();
    }

    // ── 通常BGM ───────────────────────────────────
    AudioClip BuildBgm()
    {
        float e = 60f / 145f / 2f; // 8分音符（BPM145）
        float q = 60f / 145f;      // 4分音符

        float[] mel = {
            C5, E5, G5, A5,  G5, E5, C5, 0,
            C5, E5, G5, A5,  G5, D5, E5, 0,
            G5, A5, G5, E5,  G4, A4, C5, 0,
            A4, C5, E5, G5,  D5, 0,  C5, 0,
        };
        float[] bas = {
            C3, C3, G3, G3,  A3, A3, G3, G3,
            C3, C3, G3, G3,  A3, A3, G3, G3,
        };

        return Mix("BGM", Sequence(mel, e, 0.28f, SquareWave),
            Sequence(bas, q, 0.18f, TriangleWave));
    }

    // ── ボス戦BGM（緊迫感のある速いテンポ） ───────────────
    AudioClip BuildBossBgm()
    {
        float e = 60f / 180f / 2f; // BPM180
        float q = 60f / 180f;

        float[] mel = {
            A4, 0, A4, 0, A4, C5, D5, E5,
            E5, 0, D5, 0, C5, A4, 0, 0,
            A4, 0, A4, 0, A4, C5, E5, G5,
            G5, 0, E5, 0, D5, C5, A4, 0,
        };
        float[] bas = {
            A3, A3, A3, A3, A3, A3, A3, A3,
            C3, C3, G3, G3, A3, A3, E4, E4,
        };

        return Mix("BossBGM", Sequence(mel, e, 0.32f, SquareWave),
            Sequence(bas, q, 0.22f, TriangleWave));
    }

    // ── ピンチBGM（警告音的なテンション） ───────────────────
    AudioClip BuildLowHpBgm()
    {
        float e = 60f / 160f / 2f; // BPM160
        float q = 60f / 160f;

        float[] mel = {
            C5, 0, E5, 0, C5, 0, E5, 0,
            D5, 0, F4, 0, D5, 0, F4, 0,
            C5, E5, G5, 0, E5, C5, 0, 0,
            D5, F4, A4, 0, F4, D4, 0, 0,
        };
        float[] bas = {
            C3, C3, C3, C3, D4, D4, D4, D4,
            C3, C3, G3, G3, D4, D4, A3, A3,
        };

        return Mix("LowHpBGM", Sequence(mel, e, 0.25f, SquareWave),
            Sequence(bas, q, 0.20f, TriangleWave));
    }

    // ── SE builders ───────────────────────────
    AudioClip BuildHit()
    {
        int n = Samples(0.10f);
        var d = new float[n];
        var rng = new System.Random(1);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            d[i] = (float)(rng.NextDouble() * 2 - 1) * Mathf.Pow(1 - t, 3f) * 0.5f;
        }
        return Make("Hit", d);
    }

    AudioClip BuildLevelUp()
    {
        float[] freqs = { C4, E4, G4, C5 };
        int sn = Samples(0.09f);
        var d = new float[freqs.Length * sn];
        for (int ni = 0; ni < freqs.Length; ni++)
        {
            float phase = 0;
            for (int i = 0; i < sn; i++)
            {
                float t = (float)i / sn;
                d[ni * sn + i] = SquareWave(phase) * (1f - t * 0.4f) * 0.4f;
                phase += 2 * Mathf.PI * freqs[ni] / Rate;
            }
        }
        return Make("LevelUp", d);
    }

    AudioClip BuildDie()
    {
        int n = Samples(0.18f);
        var d = new float[n];
        float phase = 0;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float freq = Mathf.Lerp(600f, 80f, t * t);
            d[i] = SquareWave(phase) * Mathf.Pow(1 - t, 1.5f) * 0.4f;
            phase += 2 * Mathf.PI * freq / Rate;
        }
        return Make("Die", d);
    }

    AudioClip BuildXp()
    {
        int n = Samples(0.07f);
        var d = new float[n];
        float phase = 0;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float freq = Mathf.Lerp(800f, 1200f, t);
            d[i] = Mathf.Sin(phase) * (1 - t) * 0.35f;
            phase += 2 * Mathf.PI * freq / Rate;
        }
        return Make("Xp", d);
    }

    AudioClip BuildGameOver()
    {
        float[] freqs = { C5, A4, G4, E4, C4 };
        int sn = Samples(0.22f);
        var d = new float[freqs.Length * sn];
        for (int ni = 0; ni < freqs.Length; ni++)
        {
            bool last = ni == freqs.Length - 1;
            float phase = 0;
            for (int i = 0; i < sn; i++)
            {
                float t = (float)i / sn;
                float env = last ? 1f - t : 1f - t * 0.3f;
                d[ni * sn + i] = TriangleWave(phase) * env * 0.35f;
                phase += 2 * Mathf.PI * freqs[ni] / Rate;
            }
        }
        return Make("GameOver", d);
    }

    // ── ヘルパー ──────────────────────────────
    int Samples(float sec) => (int)(Rate * sec);

    AudioClip Make(string name, float[] data)
    {
        var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    float[] Sequence(float[] freqs, float noteSec, float vol, Func<float, float> wave)
    {
        int sn = Samples(noteSec);
        var d = new float[freqs.Length * sn];
        for (int ni = 0; ni < freqs.Length; ni++)
        {
            if (freqs[ni] <= 0) continue;
            float phase = 0;
            for (int i = 0; i < sn; i++)
            {
                float t = (float)i / sn;
                d[ni * sn + i] = wave(phase) * Env(t) * vol;
                phase += 2 * Mathf.PI * freqs[ni] / Rate;
            }
        }
        return d;
    }

    AudioClip Mix(string name, float[] a, float[] b)
    {
        var d = new float[a.Length];
        for (int i = 0; i < a.Length; i++)
            d[i] = Mathf.Clamp(a[i] + b[i % b.Length], -1f, 1f);
        return Make(name, d);
    }

    float Env(float t)
    {
        if (t < 0.02f) return t / 0.02f;
        if (t > 0.85f) return (1f - t) / 0.15f;
        return 1f;
    }

    float SquareWave(float phase)
        => phase % (Mathf.PI * 2) < Mathf.PI ? 1f : -1f;

    float TriangleWave(float phase)
    {
        float p = (phase % (Mathf.PI * 2)) / (Mathf.PI * 2);
        return p < 0.5f ? 4 * p - 1 : 3 - 4 * p;
    }
}
