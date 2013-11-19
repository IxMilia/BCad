﻿using System;

namespace BCad.Helpers
{
    public static class MathHelper
    {
        public const int Precision = 12;
        public const double PI = 3.1415926535898;
        public const double RadiansToDegrees = 180.0 / MathHelper.PI;
        public const double DegreesToRadians = MathHelper.PI / 180.0;
        public const double TwoPI = MathHelper.PI * 2.0;
        public const double Epsilon = 0.000000000001;

        public static bool Between(double a, double b, double value)
        {
            var min = Math.Min(a, b) - Epsilon;
            var max = Math.Max(a, b) + Epsilon;
            return value >= min && value <= max;
        }

        public static bool BetweenNarrow(double a, double b, double value)
        {
            var min = Math.Min(a, b) + Epsilon;
            var max = Math.Max(a, b) - Epsilon;
            return value >= min && value <= max;
        }

        public static double CorrectAngleDegrees(this double angle)
        {
            while (angle < 0.0)
                angle += 360.0;
            while (angle >= 360.0)
                angle -= 360.0;
            return angle;
        }

        public static bool CloseTo(double expected, double actual)
        {
            return Between(expected - Epsilon, expected + Epsilon, actual);
        }

        public static bool CloseTo(Matrix4 expected, Matrix4 actual)
        {
            return CloseTo(expected.M11, actual.M11)
                && CloseTo(expected.M12, actual.M12)
                && CloseTo(expected.M13, actual.M13)
                && CloseTo(expected.M14, actual.M14)
                && CloseTo(expected.M21, actual.M21)
                && CloseTo(expected.M22, actual.M22)
                && CloseTo(expected.M23, actual.M23)
                && CloseTo(expected.M24, actual.M24)
                && CloseTo(expected.M31, actual.M31)
                && CloseTo(expected.M32, actual.M32)
                && CloseTo(expected.M33, actual.M33)
                && CloseTo(expected.M34, actual.M34)
                && CloseTo(expected.M41, actual.M41)
                && CloseTo(expected.M42, actual.M42)
                && CloseTo(expected.M43, actual.M43)
                && CloseTo(expected.M44, actual.M44);
        }
    }
}
