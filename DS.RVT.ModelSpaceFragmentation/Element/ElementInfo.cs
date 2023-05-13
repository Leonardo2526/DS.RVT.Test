using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DS.RVT.ModelSpaceFragmentation
{
    class ElementInfo
    {
        /// <summary>
        /// Minimum point of zone for fragmentation
        /// </summary>
        public static XYZ MinBoundPoint { get; set; }
        /// <summary>
        /// Maximium point of zone for fragmentation
        /// </summary>
        public static XYZ MaxBoundPoint { get; set; }
        public static double OffsetFromOriginByX { get; } = 1000;
        public static double OffsetFromOriginByY { get; } = 1000;
        public static double OffsetFromOriginByZ { get; } = 2000;

        public static XYZ CenterElemPoint { get; set; }
        public static XYZ StartElemPoint { get; set; }
        public static XYZ EndElemPoint { get; set; }

        double OffsetFromOriginByXInFeets;
        double OffsetFromOriginByYInFeets;
        double OffsetFromOriginByZInFeets;

        public List<XYZ> GetPoints(Element element)
        {
            ConvertToFeets();

            List<XYZ> elementPoints = new List<XYZ>();

            ElementUtils elementUtils = new ElementUtils();

            //Get bound points
            elementUtils.GetPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);
            elementPoints.Add(startPoint);
            elementPoints.Add(endPoint);
            CenterElemPoint = centerPoint;
            StartElemPoint = startPoint;
            EndElemPoint = endPoint;

            //GetOffset();

            PointUtils pointUtils = new PointUtils();
            pointUtils.FindMinMaxPointByPoints(elementPoints, out XYZ minPoint, out XYZ maxPoint);

            List<XYZ> boundPoints = new List<XYZ>();

            minPoint = new XYZ(minPoint.X - OffsetFromOriginByXInFeets, minPoint.Y - OffsetFromOriginByYInFeets, minPoint.Z - OffsetFromOriginByZInFeets);
            maxPoint = new XYZ(maxPoint.X + OffsetFromOriginByXInFeets, maxPoint.Y + OffsetFromOriginByYInFeets, maxPoint.Z + OffsetFromOriginByZInFeets);

            boundPoints.Add(minPoint);
            boundPoints.Add(maxPoint);

            MinBoundPoint = minPoint;
            MaxBoundPoint = maxPoint;

            return boundPoints;
        }

        void ConvertToFeets()
        {
            OffsetFromOriginByXInFeets = UnitUtils.Convert((double)OffsetFromOriginByX / 1000,
                                          DisplayUnitType.DUT_METERS,
                                          DisplayUnitType.DUT_DECIMAL_FEET);
            OffsetFromOriginByYInFeets = UnitUtils.Convert((double)OffsetFromOriginByY / 1000,
                                          DisplayUnitType.DUT_METERS,
                                          DisplayUnitType.DUT_DECIMAL_FEET); 
            OffsetFromOriginByZInFeets = UnitUtils.Convert((double)OffsetFromOriginByZ / 1000,
                                           DisplayUnitType.DUT_METERS,
                                           DisplayUnitType.DUT_DECIMAL_FEET);
        }

        //void GetOffset()
        //{
        //    CLZInfo cLZInfo = new CLZInfo();

        //    if (Math.Abs(StartElemPoint.X - EndElemPoint.X)<0.01)
        //        OffsetFromOriginByYInFeets = CLZInfo.WidthClearanceF;
        //    else if (Math.Abs(StartElemPoint.Y - EndElemPoint.Y) < 0.01)
        //        OffsetFromOriginByXInFeets = CLZInfo.WidthClearanceF;
        //    else if (Math.Abs(StartElemPoint.X - EndElemPoint.X) < 0.01 && 
        //        Math.Abs(StartElemPoint.Y - EndElemPoint.Y) < 0.01)
        //        OffsetFromOriginByZInFeets = CLZInfo.HeightClearanceF;
        //}
    }
}
