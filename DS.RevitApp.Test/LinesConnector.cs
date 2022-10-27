using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitApp.Test
{
    internal class LinesConnector
    {
        private readonly List<Line> _lines;
        public LinesConnector(List<Line> lines)
        {
            _lines = lines;
        }

        public List<Line> Connect()
        {
            var points = GetIntersectionPoints();
            return LineUtils.GetLines(points);
        }

        private List<XYZ> GetIntersectionPoints()
        {
            var points = new List<XYZ>();

            XYZ startPoint = GetIntersection(_lines.First(), _lines.Last());
            points.Add(startPoint);
            for (int i = 0; i < _lines.Count - 1; i++)
            {
                XYZ point = GetIntersection(_lines[i], _lines[i + 1]);
                points.Add(point);
            }
            points.Add(startPoint);

            return points.Where(obj => obj is not null).ToList();
        }

        private XYZ GetIntersection(Line line1, Line line2)
        {
            Line uLine1 = Line.CreateUnbound(line1.Origin, line1.Direction);
            Line uLine2 = Line.CreateUnbound(line2.Origin, line2.Direction);

            var intersect = uLine1.Intersect(uLine2, out IntersectionResultArray resultArray);

            return resultArray is not null && resultArray.Size > 0 ?
                resultArray.get_Item(0).XYZPoint :
                null;
        }
    }
}
