using UnityEngine;

namespace MuseumOfLies.Common
{
    /// <summary>
    /// Design tokens from Docs/DESIGN_GUIDELINES.md. Do not hardcode HEX colors elsewhere.
    /// </summary>
    public static class MuseumPalette
    {
        // Base (60% base / 30% secondary / 10% accent)
        public static readonly Color Ivory     = Hex("#F2F1ED"); // background
        public static readonly Color LightGray = Hex("#D9D9D9"); // secondary background
        public static readonly Color Charcoal  = Hex("#262626"); // main text, buttons
        public static readonly Color Black     = Hex("#111111"); // emphasis text

        // Exhibition accents
        public static readonly Color Weight    = Hex("#C98E43"); // 01 감각
        public static readonly Color Body      = Hex("#547EA6"); // 02 신체
        public static readonly Color Ability   = Hex("#648E79"); // 03 능력
        public static readonly Color DoNothing = Hex("#C35D58"); // 04 의지

        // Semantic (always pair with text/icon, never color alone)
        public static readonly Color Success   = Hex("#39845F");
        public static readonly Color Warning   = Hex("#D6A43B");
        public static readonly Color Error     = Hex("#D44B4B");

        // Environment (space) — from interior references: raw concrete gallery + cool gray corridor.
        // UI panels stay on the Base palette (Ivory placards on concrete).
        public static readonly Color ConcreteWall    = Hex("#B0AEA9");
        public static readonly Color ConcreteDark    = Hex("#7E7D7A");
        public static readonly Color FloorPolished   = Hex("#A9ABAE");
        public static readonly Color CeilingPanel    = Hex("#F3F4F5");
        public static readonly Color CorridorWall    = Hex("#C3C8D0");
        public static readonly Color CorridorFloor   = Hex("#A3AAB5");
        public static readonly Color CorridorCeiling = Hex("#5F6672");
        public static readonly Color DoorSteel       = Hex("#A7AAAE");
        public static readonly Color LightCoolWhite  = Hex("#F4F7FF"); // ~6500K

        public enum Exhibition { Weight, Body, Ability, DoNothing }

        public static Color Accent(Exhibition e)
        {
            switch (e)
            {
                case Exhibition.Weight:  return Weight;
                case Exhibition.Body:    return Body;
                case Exhibition.Ability: return Ability;
                default:                 return DoNothing;
            }
        }

        /// <summary>Disabled state: lower saturation, keep readable.</summary>
        public static Color Desaturate(Color c, float amount = 0.7f)
        {
            float g = c.grayscale;
            return Color.Lerp(c, new Color(g, g, g, c.a), amount);
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
