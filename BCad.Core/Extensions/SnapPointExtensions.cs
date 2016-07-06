﻿using System;
using BCad.SnapPoints;

namespace BCad.Extensions
{
    public static class SnapPointExtensions
    {
        public static SnapPoint Move(this SnapPoint snap, Vector offset)
        {
            var newPoint = snap.Point + offset;
            switch (snap.Kind)
            {
                case SnapPointKind.Center:
                    return new CenterPoint(newPoint);
                case SnapPointKind.EndPoint:
                    return new EndPoint(newPoint);
                case SnapPointKind.Focus:
                    return new FocusPoint(newPoint);
                case SnapPointKind.MidPoint:
                    return new MidPoint(newPoint);
                case SnapPointKind.Quadrant:
                    return new QuadrantPoint(newPoint);
                case SnapPointKind.None:
                default:
                    throw new ArgumentException("Invalid snap point type", "snap.Kind");
            }
        }
    }
}
