﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.SnapPoints;

namespace BCad.Objects
{
    public interface IObject
    {
        Layer Layer { get; }
        IEnumerable<IPrimitive> GetPrimitives();
        IEnumerable<SnapPoint> GetSnapPoints();
    }
}
