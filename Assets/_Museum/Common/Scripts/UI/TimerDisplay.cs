using TMPro;
using UnityEngine;

namespace MuseumOfLies.Common
{
    /// <summary>Numeric timer (Space Mono) with a status line underneath.</summary>
    public class TimerDisplay : MonoBehaviour
    {
        public enum State { Normal, Waiting, Reset, Success }

        public TMP_Text timeText;
        public TMP_Text statusText;
        public UnityEngine.UI.Image accentLine;

        public static string Format(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int total = Mathf.CeilToInt(seconds - 0.0001f);
            return $"{total / 60:00}:{total % 60:00}";
        }

        /// <summary>Elapsed time (floored) — use for records, not countdowns.</summary>
        public static string FormatElapsed(float seconds)
        {
            int total = Mathf.FloorToInt(Mathf.Max(0f, seconds));
            return $"{total / 60:00}:{total % 60:00}";
        }

        public void SetTime(float seconds) { if (timeText) timeText.text = Format(seconds); }
        public void SetRaw(string text) { if (timeText) timeText.text = text; }
        public void SetStatus(string text) { if (statusText) statusText.text = text; }

        public void SetState(State s)
        {
            if (!timeText) return;
            switch (s)
            {
                case State.Reset:   timeText.color = MuseumPalette.Error; break;
                case State.Success: timeText.color = MuseumPalette.Success; break;
                case State.Waiting: timeText.color = new Color(MuseumPalette.Charcoal.r, MuseumPalette.Charcoal.g, MuseumPalette.Charcoal.b, 0.45f); break;
                default:            timeText.color = MuseumPalette.Charcoal; break;
            }
        }

        public void SetAccent(Color c) { if (accentLine) accentLine.color = c; }
    }
}
