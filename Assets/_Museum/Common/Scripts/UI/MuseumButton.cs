using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MuseumOfLies.Common
{
    /// <summary>
    /// Shared button styles from the design system: Primary / Secondary / Exit / Disabled.
    /// Hover = brightness change, Pressed = darker + slight depth (label offset), Disabled = desaturated + lock.
    /// </summary>
    [RequireComponent(typeof(Button), typeof(Image))]
    public class MuseumButton : MonoBehaviour
    {
        public enum Style { Primary, Secondary, Exit, Disabled }

        public Style style = Style.Primary;
        public TMP_Text label;
        [Tooltip("Optional leading icon glyph, e.g. ▶ ↺ →")]
        public string icon = "";

        Button _button;
        Image _image;
        Outline _outline;
        string _text;

        public Button Button => _button != null ? _button : (_button = GetComponent<Button>());

        void Awake()
        {
            Apply();
            if (Application.isPlaying) Button.onClick.AddListener(MuseumHaptics.UITick); // 햅틱 역할: UI 확인
        }
        void OnValidate() { Apply(); }

        public void SetText(string text, string iconGlyph = null)
        {
            _text = text;
            if (iconGlyph != null) icon = iconGlyph;
            Apply();
        }

        public void SetStyle(Style s) { style = s; Apply(); }

        public void Apply()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _outline = GetComponent<Outline>();
            if (_image == null || _button == null) return;

            Color bg, fg; bool border = false;
            switch (style)
            {
                case Style.Primary:   bg = MuseumPalette.Charcoal;  fg = MuseumPalette.Ivory;    break;
                case Style.Secondary: bg = MuseumPalette.LightGray; fg = MuseumPalette.Charcoal; break;
                case Style.Exit:      bg = MuseumPalette.Ivory;     fg = MuseumPalette.Charcoal; border = true; break;
                default:              bg = MuseumPalette.Desaturate(MuseumPalette.LightGray); fg = MuseumPalette.Desaturate(new Color(0.55f, 0.55f, 0.55f)); break;
            }
            _image.color = bg;

            if (_outline == null && border) _outline = gameObject.AddComponent<Outline>();
            if (_outline != null)
            {
                _outline.enabled = border;
                _outline.effectColor = MuseumPalette.Charcoal;
                _outline.effectDistance = new Vector2(3, -3);
            }

            var cb = _button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            cb.selectedColor = Color.white;
            cb.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            cb.disabledColor = Color.white; // disabled look is handled by Style.Disabled
            cb.fadeDuration = 0.08f;
            _button.colors = cb;
            _button.interactable = style != Style.Disabled;

            if (label != null)
            {
                if (_text == null) _text = StripIcon(label.text);
                string glyph = style == Style.Disabled ? "잠김 ·" : icon; // locked state shown in text, not color alone
                label.text = string.IsNullOrEmpty(glyph) ? _text : glyph + "  " + _text;
                label.color = fg;
            }
        }

        string StripIcon(string t)
        {
            if (string.IsNullOrEmpty(icon) || t == null) return t;
            return t.StartsWith(icon) ? t.Substring(icon.Length).TrimStart() : t;
        }
    }
}
