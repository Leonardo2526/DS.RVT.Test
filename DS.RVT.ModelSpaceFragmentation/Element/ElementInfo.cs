using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

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

        public static XYZ StartElemPoint { get; set; }
        public static XYZ EndElemPoint { get; set; }

        double OffsetFromOriginByXInFeets;
        double OffsetFromOriginByYInFeets;
        double OffsetFromOriginByZInFeets;
        private readonly Vector3D _stepVector;
        private readonly XYZ _startPoint;
        private readonly XYZ _endPoint;

        public ElementInfo(Vector3D stepVector, XYZ startPoint, XYZ endPoint)
        {
            _stepVector = stepVector;
            _startPoint = startPoint;
            _endPoint = endPoint;
        }

        public List<XYZ> GetPoints(Element element)
        {
            ConvertToFeets();

            List<XYZ> elementPoints = new List<XYZ>();
            elementPoints.Add(_startPoint);
            elementPoints.Add(_endPoint);
            StartElemPoint = _startPoint;
            EndElemPoint = _endPoint;

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

            var ofx = OffsetFromOriginByXInFeets / _stepVector.X;
            int ofxr = (int)Math.Round(ofx);
            OffsetFromOriginByXInFeets = ofxr * _stepVector.X;


            var ofy= OffsetFromOriginByYInFeets / _stepVector.Y;
            int ofyr = (int)Math.Round(ofy);
            OffsetFromOriginByYInFeets = ofyr * _stepVector.Y;

            var ofz = OffsetFromOriginByZInFeets / _stepVector.Z;
            int ofzr = (int)Math.Round(ofz);
            OffsetFromOriginByZInFeets = ofzr * _stepVector.Z;
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
