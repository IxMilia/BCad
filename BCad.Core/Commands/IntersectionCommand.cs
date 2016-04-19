﻿using System.Collections.Generic;
using BCad.Entities;
using BCad.Extensions;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Intersection", "INTERSECTION", "intersection", "int")]
    public class IntersectionCommand : CombinePolylinesCommandBase
    {
        protected override IEnumerable<Polyline> Combine(IEnumerable<Polyline> polylines)
        {
            return polylines.Intersect();
        }
    }
}
