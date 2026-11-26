using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MuseumOfLies.Common
{
    /// <summary>Result panel: status badge (icon + text, never color alone), label/value rows, two actions.</summary>
    public class ResultPanel : MonoBehaviour
    {
        public enum Outcome { Success, Ended, Error }

        public Image accentBar;
        public TMP_Text eyebrow;
        public TMP_Text title;
        public Image badge;
        public TMP_Text badgeText;
        public RectTransform rowContainer;
        public GameObject rowTemplate;   // children: "Label" (TMP), "Value" (TMP)
        public TMP_Text footnote;
        public MuseumButton primaryButton;
        public MuseumButton secondaryButton;

        readonly List<GameObject> _rows = new List<GameObject>();

        public void SetHeader(string eyebrowText, string titleText, Color accent)
        {
            if (eyebrow) { eyebrow.text = eyebrowText; eyebrow.color = accent; }
            if (title) title.text = titleText;
            if (accentBar) accentBar.color = accent;
        }

        public void SetOutcome(Outcome o, string text)
        {
            Color c = o == Outcome.Success ? MuseumPalette.Success : o == Outcome.Error ? MuseumPalette.Error : MuseumPalette.Charcoal;
            string icon = o == Outcome.Success ? "✓" : o == Outcome.Error ? "!" : "■";
            if (badge) badge.color = c;
            if (badgeText) { badgeText.text = icon + "  " + text; badgeText.color = MuseumPalette.Ivory; }
        }

        public void ClearRows()
        {
            foreach (var r in _rows) if (r) Destroy(r);
            _rows.Clear();
        }

        /// <param name="numeric">true → Space Mono value (numbers/time). false → same font as the label (Korean text).</param>
        public void AddRow(string label, string value, bool emphasize = false, bool numeric = true)
        {
            if (!rowTemplate || !rowContainer) return;
            var row = Instantiate(rowTemplate, rowContainer);
            row.SetActive(true);
            var l = row.transform.Find("Label")?.GetComponent<TMP_Text>();
            var v = row.transform.Find("Value")?.GetComponent<TMP_Text>();
            if (l) l.text = label;
            if (v)
            {
                v.text = value;
                v.color = emphasize ? MuseumPalette.Black : MuseumPalette.Charcoal;
                if (!numeric && l) v.font = l.font;
            }
            _rows.Add(row);
        }

        public void SetFootnote(string text) { if (footnote) footnote.text = text; }
        public void Show(bool on) { gameObject.SetActive(on); }
    }
}
