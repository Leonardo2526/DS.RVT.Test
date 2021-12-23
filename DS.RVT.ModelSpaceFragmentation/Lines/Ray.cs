﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace DS.RVT.ModelSpaceFragmentation.Lines
{
    class Ray : ILine
    {      
        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }

        public Ray(XYZ point)
        {
            StartPoint = point;
        }

        public Line Create()
        {
            EndPoint = new XYZ(StartPoint.X + 10, StartPoint.Y + 5, StartPoint.Z + 3);
            Line line = Line.CreateBound(StartPoint, EndPoint);

            return line;
        }

    }
}
