﻿using System.Windows.Media;

namespace BCad.Extensions
{
    public static class ColorExtensions
    {
        public static System.Drawing.Color ToDrawingColor(this RealColor color)
        {
            return System.Drawing.Color.FromArgb((int)(0xFF000000 | (uint)color.ToInt()));
        }

        public static Color ToMediaColor(this RealColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
