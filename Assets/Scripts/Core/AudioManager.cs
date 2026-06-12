using System;
using UnityEngine;

/// <summary>プロシージャル生成によるBGM・SE管理（DontDestroyOnLoad）</summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    AudioSource bgmSource;
    AudioSource seSource;

    AudioClip hitClip, levelUpClip, dieClip, xpClip, gameOverClip;

    const int Rate = 44100;

    // ノート周波数
    const float C3 = 130.81f, G3 = 196.00f, A3 = 220.00f;
    const float C4 = 261.63f, E4 = 329.63f, G4 = 392.00f, A4 = 440.00f;
    const float C5 = 523.25f, D5 = 587.33f, E5 = 659.26f, G5 = 784.00f, A5 = 880.00f;

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

        bgmSource.clip = BuildBgm();
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
        Instance.bgmSource.Stop(); // BGMを止めてゲームオーバーSEを鳴らす
        Instance.seSource.PlayOneShot(Instance.gameOverClip);
    }

    public void RestartBgm()
    {
        bgmSource.Stop();
        bgmSource.Play();
    }

    // ── BGM ───────────────────────────────────
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

        return Mix("BGM",
            Sequence(mel, e, 0.28f, SquareWave),
            Sequence(bas, q, 0.18f, TriangleWave));
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
