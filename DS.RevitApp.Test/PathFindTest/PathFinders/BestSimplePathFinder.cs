using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Lines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFindTest.PathFinders
{
    internal class BestSimplePathFinder : IPathFinder
    {

        private XYZ _point1;
        private XYZ _point2;
        private Line _baseLine;
        private readonly double _minPointDist;
        private readonly double _minZDist;

        public BestSimplePathFinder(Line baseLine, double minPointDist = 0, double minZDist = 0)
        {
            if (minZDist < minPointDist)
            {
                throw new ArgumentException("minZDist < minPointDist");
            }
            _baseLine = baseLine;
            _minPointDist = minPointDist;
            _minZDist = minZDist;
        }

        private XYZ GetDeltaPoint()
        {
            XYZ vector = _point2 - _point1;

            if (Math.Round(vector.X, 3) != 0 && Math.Abs(vector.X) < _minPointDist)
            {
                vector = new XYZ(_minPointDist, vector.Y, vector.Z);
            }
            if (Math.Round(vector.Y, 3) != 0 && Math.Abs(vector.Y) < _minPointDist)
            {
                vector = new XYZ(vector.X, _minPointDist, vector.Z);
            }
            if (Math.Abs(vector.Z) < _minPointDist)
            {
                vector = new XYZ(vector.X, vector.Y, _minPointDist);
            }
            if (Math.Abs(vector.Z) < _minZDist)
            {
                vector = new XYZ(vector.X, vector.Y, _minZDist);
            }
            return vector;
        }

        public List<XYZ> FindPath(XYZ point1, XYZ point2)
        {
            _point1 = point1;
            _point2 = point2;

            _baseLine ??= Line.CreateBound(point1, point2);

            XYZ dp = GetDeltaPoint();

            var result = new List<XYZ>() { _point1 };

            XYZ gp1 = new XYZ(_point1.X, _point1.Y, _point1.Z + dp.Z);
            Line line = Line.CreateUnbound(gp1, _baseLine.Direction);
            XYZ gp2 = line.Project(_point2).XYZPoint;
            line = Line.CreateUnbound(gp2, line.Direction.CrossProduct(gp1 - _point1));
            XYZ gp3 = line.Project(point2).XYZPoint;

            if (Math.Round(gp1.DistanceTo(_point1), 3) != 0)
            {
                result.Add(gp1);
            }
            if (Math.Round(gp2.DistanceTo(_point2), 3) != 0 && Math.Round(gp2.DistanceTo(gp1), 3) != 0)
            {
                result.Add(gp2);

            }
            if (Math.Round(gp3.DistanceTo(_point2), 3) != 0 && Math.Round(gp3.DistanceTo(gp2), 3) != 0)
            {
                result.Add(gp3);
            }

            result.Add(_point2);

            return result;
        }
    }
}
