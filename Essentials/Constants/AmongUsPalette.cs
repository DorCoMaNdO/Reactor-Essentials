using UnityEngine;

namespace Essentials.Constants
{
    public static class AmongUsPalette
    {
        public static readonly Color DisabledGrey = new Color(0.3f, 0.3f, 0.3f, 1f);
        public static readonly Color DisabledColor = new Color(1f, 1f, 1f, 0.3f);
        public static readonly Color EnabledColor = new Color(1f, 1f, 1f, 1f);
        public static readonly Color Black = new Color(0f, 0f, 0f, 1f);
        public static readonly Color ClearWhite = new Color(1f, 1f, 1f, 0f);
        public static readonly Color HalfWhite = new Color(1f, 1f, 1f, 0.5f);
        public static readonly Color White = new Color(1f, 1f, 1f, 1f);
        public static readonly Color LightBlue = new Color(0.5f, 0.5f, 1f);
        public static readonly Color Blue = new Color(0.2f, 0.2f, 1f);
        public static readonly Color Orange = new Color(1f, 0.6f, 0.005f);
        public static readonly Color Purple = new Color(0.6f, 0.1f, 0.6f);
        public static readonly Color Brown = new Color(0.72f, 0.43f, 0.11f);
        public static readonly Color CrewmateBlue = new Color32(140, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        public static readonly Color ImpostorRed = new Color32(byte.MaxValue, 25, 25, byte.MaxValue);
        public static readonly Color32 VisorColor = new Color32(149, 202, 220, byte.MaxValue);

        public static string[] ShortColorNames { get { return Palette.ShortColorNames; } set { Palette.ShortColorNames = value; } }
        public static Color32[] PlayerColors { get { return Palette.PlayerColors; } set { Palette.PlayerColors = value; } }
        public static Color32[] ShadowColors { get { return Palette.ShadowColors; } set { Palette.ShadowColors = value; } }
    }
}
