namespace MuseumOfLies.Common
{
    /// <summary>
    /// Type hierarchy (Display → Heading → Subheading → Body → Numeric) in canvas pixels.
    /// World canvases use scale 0.001 (1000 px = 1 m). Initial values — confirm in headset.
    /// </summary>
    public static class MuseumTypeScale
    {
        public const float Display    = 120f;
        public const float Heading    = 64f;
        public const float Subheading = 40f;
        public const float Body       = 34f;
        public const float Caption    = 26f;
        public const float Numeric    = 240f;

        // Spacing scale (relative UI units, canvas px)
        public const float S4 = 4f, S8 = 8f, S16 = 16f, S24 = 24f, S32 = 32f, S48 = 48f;

        public const float WorldCanvasScale = 0.001f;
    }
}
