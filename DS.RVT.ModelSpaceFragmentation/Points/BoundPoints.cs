using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    class BoundPoints
    {

        public static XYZ Point1 { get; set; }
        public static XYZ Point2 { get; set; }

        public List<XYZ> GetPoints(Element element)
        {
            List<XYZ> elementPoints = new List<XYZ>();

            ElementUtils elementUtils = new ElementUtils();

            //Get bound points
            elementUtils.GetPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);
            elementPoints.Add(startPoint);
            elementPoints.Add(endPoint);

            PointUtils pointUtils = new PointUtils();
            pointUtils.FindMinMaxPointByPoints(elementPoints, out XYZ minPoint, out XYZ maxPoint);

            List<XYZ> boundPoints = new List<XYZ>();
            double offset = 3;

            minPoint = new XYZ(minPoint.X - offset, minPoint.Y - offset, minPoint.Z - offset);
            maxPoint = new XYZ(maxPoint.X + offset, maxPoint.Y + offset, maxPoint.Z + offset);

            boundPoints.Add(minPoint);
            boundPoints.Add(maxPoint);

            Point1 = minPoint;
            Point2 = maxPoint;

            return boundPoints;
        }
    }
}
