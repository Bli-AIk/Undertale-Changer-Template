using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace UCT.Extensions
{
    /// <summary>
    ///     Color 扩展工具
    /// </summary>
    public static class ColorEx
    {
        public static Color WhiteClear => new(1, 1, 1, 0);
        public static Color RedClear => new(1, 0, 0, 0);
        public static Color HalfAlpha => new(1, 1, 1, 0.5f);

        public static Color Purple => new(0.5f, 0, 0.5f, 1);
        public static Color Magenta => new(1, 0, 1, 1);
        public static Color Orange => new(1, 0.5f, 0, 1);
        public static Color Gold => new(1, 0.84f, 0, 1);
        public static Color Teal => new(0, 0.5f, 0.5f, 1);
        public static Color Olive => new(0.5f, 0.5f, 0, 1);
        public static Color Navy => new(0, 0, 0.5f, 1);
        public static Color Maroon => new(0.5f, 0, 0, 1);
        public static Color Pink => new(1, 0.75f, 0.8f, 1);
        public static Color LightBlue => new(0.68f, 0.85f, 0.9f, 1);
    }
}