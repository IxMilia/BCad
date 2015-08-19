﻿using System.Windows.Media;

namespace BCad.Extensions
{
    public static class ColorExtensions
    {
        public static System.Drawing.Color ToDrawingColor(this CadColor color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color ToMediaColor(this CadColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}