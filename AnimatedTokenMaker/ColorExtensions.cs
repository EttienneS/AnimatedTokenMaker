using System;
using System.Drawing;

namespace AnimatedTokenMaker
{
    public static class ColorExtensions
    {
        private const int MIN = 0;
        private const int MAX = 255;

        private const float MAXf = 255f;

        public static Color Multiply(this Color color, Color other)
        {
            return Color.FromArgb(
                (int)((color.A * other.A) / MAXf),
                (int)((color.R * other.R) / MAXf),
                (int)((color.G * other.G) / MAXf),
                (int)((color.B * other.B) / MAXf));
        }

        public static Color SetAlpha(this Color color, int alpha)
        {
            return Color.FromArgb(alpha, color);
        }

        public static Color Subtract(this Color color, Color other)
        {
            return Color.FromArgb(Math.Max(MIN, color.A - other.A),
                                  Math.Max(MIN, color.R - other.R),
                                  Math.Max(MIN, color.G - other.G),
                                  Math.Max(MIN, color.B - other.B));
        }

        public static Color Add(this Color color, Color other)
        {
            return Color.FromArgb(Math.Min(MAX, color.A + other.A),
                                  Math.Min(MAX, color.R + other.R),
                                  Math.Min(MAX, color.G + other.G),
                                  Math.Min(MAX, color.B + other.B));
        }
    }
}