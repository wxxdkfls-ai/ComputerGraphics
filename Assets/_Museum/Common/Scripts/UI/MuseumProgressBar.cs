using UnityEngine;
using UnityEngine.UI;

namespace MuseumOfLies.Common
{
    /// <summary>Flat progress bar: background track + fill scaled by anchorMax.x.</summary>
    public class MuseumProgressBar : MonoBehaviour
    {
        public RectTransform fill;
        public Image fillImage;
        [Range(0, 1)] public float value;

        public void SetValue(float v)
        {
            value = Mathf.Clamp01(v);
            if (fill != null) fill.anchorMax = new Vector2(value, 1f);
        }

        public void SetColor(Color c) { if (fillImage != null) fillImage.color = c; }

        void OnValidate() { SetValue(value); }
    }
}
