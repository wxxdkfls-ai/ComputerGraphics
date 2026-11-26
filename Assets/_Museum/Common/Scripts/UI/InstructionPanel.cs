using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MuseumOfLies.Common
{
    /// <summary>Title → key info → description → action button.</summary>
    public class InstructionPanel : MonoBehaviour
    {
        public Image accentBar;
        public TMP_Text eyebrow;
        public TMP_Text title;
        public TMP_Text body;
        public MuseumButton primaryButton;

        public void SetContent(string eyebrowText, string titleText, string bodyText, Color accent, string buttonText = null)
        {
            if (eyebrow) { eyebrow.text = eyebrowText; eyebrow.color = accent; }
            if (title) title.text = titleText;
            if (body) body.text = bodyText;
            if (accentBar) accentBar.color = accent;
            if (primaryButton && buttonText != null) primaryButton.SetText(buttonText, "▶");
        }

        public void Show(bool on) { gameObject.SetActive(on); }
    }
}
