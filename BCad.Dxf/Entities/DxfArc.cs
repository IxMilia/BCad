﻿using System.Collections.Generic;

namespace BCad.Dxf.Entities
{
    public class DxfArc : DxfEntity
    {
        public override DxfEntityType EntityType
        {
            get { return DxfEntityType.Arc; }
        }

        public DxfPoint Center { get; set; }

        public double Radius { get; set; }

        public DxfVector Normal { get; set; }

        public double StartAngle { get; set; }

        public double EndAngle { get; set; }

        public DxfArc()
            : this(new DxfPoint(0.0, 0.0, 0.0), 0.0, 0.0, 360.0)
        {
        }

        public DxfArc(DxfPoint center, double radius, double startAngle, double endAngle)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
            Normal = new DxfVector(0.0, 0.0, 1.0);
        }

        public static DxfArc FromPairs(IEnumerable<DxfCodePair> pairs)
        {
            var arc = new DxfArc();
            arc.PopulateDefaultAndCommonValues(pairs);
            foreach (var pair in pairs)
            {
                switch (pair.Code)
                {
                    case 10:
                        arc.Center.X = pair.DoubleValue;
                        break;
                    case 20:
                        arc.Center.Y = pair.DoubleValue;
                        break;
                    case 30:
                        arc.Center.Z = pair.DoubleValue;
                        break;
                    case 40:
                        arc.Radius = pair.DoubleValue;
                        break;
                    case 210:
                        arc.Normal.X = pair.DoubleValue;
                        break;
                    case 220:
                        arc.Normal.Y = pair.DoubleValue;
                        break;
                    case 230:
                        arc.Normal.Z = pair.DoubleValue;
                        break;
                    case 50:
                        arc.StartAngle = pair.DoubleValue;
                        break;
                    case 51:
                        arc.EndAngle = pair.DoubleValue;
                        break;
                }
            }
            return arc;
        }

        protected override IEnumerable<DxfCodePair> GetEntitySpecificPairs()
        {
            yield return new DxfCodePair(10, Center.X);
            yield return new DxfCodePair(20, Center.Y);
            yield return new DxfCodePair(30, Center.Z);
            yield return new DxfCodePair(40, Radius);
            yield return new DxfCodePair(50, StartAngle);
            yield return new DxfCodePair(51, EndAngle);
            if (Normal != new DxfVector(0, 0, 1))
            {
                yield return new DxfCodePair(210, Normal.X);
                yield return new DxfCodePair(220, Normal.Y);
                yield return new DxfCodePair(230, Normal.Z);
            }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(":{0}R{1}", Center, Radius);
        }
    }
}
