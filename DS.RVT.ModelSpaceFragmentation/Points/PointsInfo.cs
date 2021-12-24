using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PointsInfo
    {
        /// <summary>
        /// Minimum point of zone for fragmentation
        /// </summary>
        public static XYZ MinBoundPoint { get; set; }
        /// <summary>
        /// Maximium point of zone for fragmentation
        /// </summary>
        public static XYZ MaxBoundPoint { get; set; }
        public static double OffsetFromOriginByZ { get; } = 1000;
        public static double OffsetFromOriginByXY { get; } = 1000;

        double OffsetFromOriginByZInFeets;
        double OffsetFromOriginByXYInFeets;

        public List<XYZ> GetPoints(Element element)
        {
            ConvertToFeets();

            List<XYZ> elementPoints = new List<XYZ>();

            ElementUtils elementUtils = new ElementUtils();

            //Get bound points
            elementUtils.GetPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);
            elementPoints.Add(startPoint);
            elementPoints.Add(endPoint);

            PointUtils pointUtils = new PointUtils();
            pointUtils.FindMinMaxPointByPoints(elementPoints, out XYZ minPoint, out XYZ maxPoint);

            List<XYZ> boundPoints = new List<XYZ>();

            minPoint = new XYZ(minPoint.X - OffsetFromOriginByXYInFeets, minPoint.Y - OffsetFromOriginByXYInFeets, minPoint.Z - OffsetFromOriginByZInFeets);
            maxPoint = new XYZ(maxPoint.X + OffsetFromOriginByXYInFeets, maxPoint.Y + OffsetFromOriginByXYInFeets, maxPoint.Z + OffsetFromOriginByZInFeets);

            boundPoints.Add(minPoint);
            boundPoints.Add(maxPoint);

            MinBoundPoint = minPoint;
            MaxBoundPoint = maxPoint;

            return boundPoints;
        }

        void ConvertToFeets()
        {
            OffsetFromOriginByZInFeets = UnitUtils.Convert((double)OffsetFromOriginByZ / 1000,
                                          DisplayUnitType.DUT_METERS,
                                          DisplayUnitType.DUT_DECIMAL_FEET);
            OffsetFromOriginByXYInFeets = UnitUtils.Convert((double)OffsetFromOriginByXY / 1000,
                               DisplayUnitType.DUT_METERS,
                               DisplayUnitType.DUT_DECIMAL_FEET);
        }
    }
}
