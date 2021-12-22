using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace DS.RVT.ModelSpaceFragmentation.Lines
{
    class Ray : ILine
    {
        readonly XYZ StartPoint;

        public Ray(XYZ startPoint)
        {
            StartPoint = startPoint;
        }

        public Line Create()
        {
            XYZ endPoint = new XYZ(StartPoint.X + 10, StartPoint.Y + 5, StartPoint.Z + 3);
            Line line = Line.CreateBound(StartPoint, endPoint);

            return line;
        }

    }
}
