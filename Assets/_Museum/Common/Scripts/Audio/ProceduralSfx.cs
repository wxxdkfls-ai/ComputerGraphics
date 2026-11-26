using UnityEngine;

namespace MuseumOfLies.Common
{
    /// <summary>
    /// Placeholder sound effects generated in code (no audio assets needed).
    /// Replace with recorded sounds later if desired — keep the same timing.
    /// </summary>
    public static class ProceduralSfx
    {
        const int Rate = 44100;

        static AudioClip Make(string name, float seconds, System.Func<float, float> f)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)Rate), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Sin(float hz, float t) => Mathf.Sin(2f * Mathf.PI * hz * t);

        /// <summary>Metallic coin clink.</summary>
        public static AudioClip Clink() => Make("sfx_clink", 0.6f, t =>
            Mathf.Exp(-t * 9f) * (0.45f * Sin(2630f, t) + 0.3f * Sin(4110f, t) + 0.2f * Sin(5320f, t)));

        /// <summary>Knock-knock-knock, twice.</summary>
        public static AudioClip Knock()
        {
            var rng = new System.Random(7);
            float[] hits = { 0f, 0.22f, 0.44f, 1.3f, 1.52f, 1.74f };
            return Make("sfx_knock", 2.1f, t =>
            {
                float s = 0f;
                foreach (var h in hits)
                {
                    float dt = t - h;
                    if (dt < 0f || dt > 0.12f) continue;
                    float env = Mathf.Exp(-dt * 45f);
                    s += env * (0.7f * Sin(95f, dt) + 0.35f * Sin(180f, dt) + 0.25f * ((float)rng.NextDouble() * 2f - 1f));
                }
                return s;
            });
        }

        /// <summary>Soft mechanical rise hum.</summary>
        public static AudioClip Rise(float seconds = 1.6f) => Make("sfx_rise", seconds, t =>
        {
            float env = Mathf.Clamp01(t / 0.3f) * Mathf.Clamp01((seconds - t) / 0.3f);
            return 0.18f * env * (Sin(110f + 30f * t, t) + 0.5f * Sin(220f + 60f * t, t));
        });

        /// <summary>Gentle descending tone for a timer reset (not alarming).</summary>
        public static AudioClip ResetTone() => Make("sfx_reset", 0.45f, t =>
        {
            float hz = t < 0.18f ? 330f : 247f;
            return 0.25f * Mathf.Exp(-t * 4f) * Sin(hz, t);
        });

        /// <summary>Two-note ding (used for the lying sign).</summary>
        public static AudioClip Ding() => Make("sfx_ding", 0.9f, t =>
        {
            float a = 0.3f * Mathf.Exp(-t * 5f) * Sin(880f, t);
            float b = t > 0.2f ? 0.3f * Mathf.Exp(-(t - 0.2f) * 5f) * Sin(1175f, t) : 0f;
            return a + b;
        });

        /// <summary>Major arpeggio for success.</summary>
        public static AudioClip Success() => Make("sfx_success", 1.2f, t =>
        {
            float s = 0f;
            float[] f = { 523.25f, 659.25f, 783.99f };
            for (int i = 0; i < 3; i++)
            {
                float dt = t - i * 0.14f;
                if (dt > 0f) s += 0.22f * Mathf.Exp(-dt * 3f) * Sin(f[i], dt);
            }
            return s;
        });

        /// <summary>Soft rolling rumble for a ball.</summary>
        public static AudioClip Roll(float seconds = 2.5f)
        {
            var rng = new System.Random(3);
            float lp = 0f;
            return Make("sfx_roll", seconds, t =>
            {
                float noise = (float)rng.NextDouble() * 2f - 1f;
                lp += 0.02f * (noise - lp);
                float env = Mathf.Clamp01(t / 0.2f) * Mathf.Clamp01((seconds - t) / 0.8f);
                return 2.2f * env * lp;
            });
        }
    
        /// <summary>Insect legs skittering on a hard floor (loopable).</summary>
        public static AudioClip Skitter(float seconds = 1.5f)
        {
            var rng = new System.Random(19);
            int n = Mathf.CeilToInt(seconds * Rate);
            var clicks = new System.Collections.Generic.List<float>();
            float tt = 0f;
            while (tt < seconds) { clicks.Add(tt); tt += 0.012f + (float)rng.NextDouble() * 0.035f; }
            var noise = new System.Random(5);
            return Make("sfx_skitter", seconds, t =>
            {
                float s = 0f;
                int i = clicks.BinarySearch(t);
                if (i < 0) i = ~i - 1;
                if (i >= 0)
                {
                    float dt = t - clicks[i];
                    if (dt < 0.004f) s = (float)(noise.NextDouble() * 2 - 1) * Mathf.Exp(-dt * 1400f) * 0.55f;
                }
                return s;
            });
        }

        /// <summary>Slow footsteps on concrete, approaching (volume handled by 3D source movement).</summary>
        public static AudioClip Footsteps(float seconds = 5f, float interval = 0.62f)
        {
            var rng = new System.Random(31);
            return Make("sfx_footsteps", seconds, t =>
            {
                float local = t % interval;
                if (local > 0.18f) return 0f;
                float env = Mathf.Exp(-local * 28f);
                float heel = Sin(70f, local) * 0.7f + Sin(140f, local) * 0.25f;
                float scuff = ((float)rng.NextDouble() * 2f - 1f) * 0.25f * Mathf.Exp(-local * 60f);
                return env * (heel + scuff) * 0.9f;
            });
        }

        /// <summary>Slow human breathing near the ear (loopable, ~4 s cycle).</summary>
        public static AudioClip Breath(float seconds = 4.2f)
        {
            var rng = new System.Random(77);
            float lp = 0f, lp2 = 0f;
            return Make("sfx_breath", seconds, t =>
            {
                float phase = t / seconds;
                // inhale (0–0.38), pause, exhale (0.48–0.9)
                float env = phase < 0.38f ? Mathf.Sin(phase / 0.38f * Mathf.PI) * 0.6f
                          : phase > 0.48f && phase < 0.9f ? Mathf.Sin((phase - 0.48f) / 0.42f * Mathf.PI) : 0f;
                float noise = (float)rng.NextDouble() * 2f - 1f;
                lp += 0.25f * (noise - lp);
                lp2 += 0.08f * (lp - lp2);       // band-limited breath noise
                return (lp - lp2) * env * 1.6f;
            });
        }

        /// <summary>Deep structural rumble for the floor collapse.</summary>
        public static AudioClip Rumble(float seconds = 3.5f)
        {
            var rng = new System.Random(41);
            float lp = 0f;
            return Make("sfx_rumble", seconds, t =>
            {
                float noise = (float)rng.NextDouble() * 2f - 1f;
                lp += 0.01f * (noise - lp);
                float env = Mathf.Clamp01(t / 0.15f) * Mathf.Clamp01((seconds - t) / 1.2f);
                return env * (lp * 5f + 0.35f * Sin(38f, t) + 0.2f * Sin(55f, t));
            });
        }
}
}
