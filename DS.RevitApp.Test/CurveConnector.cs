using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{

    internal static class CurveConnector
    {
        public enum ConnectionOption
        { Trim, Extend }

        public static IEnumerable<Curve> Connect(
            Curve sourceCurve,
            Curve targetCurve,
            ConnectionOption connectionOption,
             bool isVirtualEnable = false,
            int staticIndex = 0)
        {
            if (staticIndex != 0 && staticIndex != 1)
            { throw new ArgumentException("index can be only 0 or 1."); }

            var resultCurves = new List<Curve>();

            var moveIndex = Math.Abs(1 - staticIndex);
            var staticPoint = sourceCurve.GetEndPoint(staticIndex);
            var staticParamter = sourceCurve.GetEndParameter(staticIndex);
            //staticPoint.Show(_activeDoc, 200.MMToFeet(), TransactionFactory);
            var movePoint = sourceCurve.GetEndPoint(moveIndex);

            //Debug.WriteLine($"Trying to trim point {movePoint} of " +
            //$"{sourceCurve.GetType().Name} with {targetCurve.GetType().Name}");


            Curve sourceOperationCurve;
            if (connectionOption == ConnectionOption.Extend)
            {
                sourceOperationCurve = sourceCurve.Clone();
                sourceOperationCurve.MakeUnbound();
                staticPoint = sourceOperationCurve.Project(staticPoint).XYZPoint;
            }
            else
            {
                sourceOperationCurve = sourceCurve;
            }


            Curve targetOperationCurve;
            if (isVirtualEnable)
            {
                targetOperationCurve = targetCurve.Clone();
                targetOperationCurve.MakeUnbound();
            }
            else { targetOperationCurve = targetCurve; }

            var intersection = sourceOperationCurve
                .Intersect(targetOperationCurve, out var resultArray);
            var intersectionResults = resultArray?.AsEnumerable<IntersectionResult>();

            IEnumerable<XYZ> connectionPoints = null;
            switch (intersection)
            {
                case SetComparisonResult.Overlap:
                    connectionPoints = intersectionResults.Select(r => r.XYZPoint);
                    break;
                case SetComparisonResult.Subset:
                case SetComparisonResult.Equal:
                    connectionPoints = new List<XYZ>()
                    {targetCurve.GetEndPoint(0), targetCurve.GetEndPoint(1) };
                    break;
                default:
                    break;
            }
            if(connectionPoints == null) { return null; }

            var tolerance = 0.001;
            if(connectionPoints.Any( p => movePoint.DistanceTo(p) < tolerance)) 
            { return new List<Curve> { sourceCurve }; }
            connectionPoints = connectionPoints.
                Where(p => staticPoint.DistanceTo(p) > tolerance);
            connectionPoints = connectionOption == ConnectionOption.Trim ?
                connectionPoints.Where(p => sourceCurve.Contains(p, true, tolerance)) :
                connectionPoints.Where(p => !sourceCurve.Contains(p, true , tolerance));

            //Debug.WriteLine($"{resultCurves.Count} intersection curves were found.");
            var curves = connectionPoints
                .SelectMany(getCurves)
                .OrderBy(c => _distinctLength(sourceCurve, c));
            return curves;
            return connectionPoints?.SelectMany(getCurves);

            IEnumerable<Curve> getCurves(XYZ point)
            => connectionOption == ConnectionOption.Trim ?
                GetTrimCurves(sourceCurve, sourceOperationCurve, staticPoint, movePoint, point) :
                GetExtendCurves(sourceCurve, sourceOperationCurve, staticPoint, movePoint, point);
        }

        private static IEnumerable<Curve> GetTrimCurves(
            Curve sourceCurve,
            Curve operationCurve,
            XYZ staticPoint,
             XYZ movePoint,
            XYZ connectionPoint) =>
              operationCurve.MakeBound(staticPoint, connectionPoint)
              .Where(c => c != null)
              .Where(c => c.ApproximateLength < sourceCurve.ApproximateLength)
            .Where(c => !c.Contains(movePoint) || c.IsCircle());

        private static IEnumerable<Curve> GetExtendCurves(
           Curve sourceCurve,
           Curve operationCurve,
           XYZ staticPoint,
           XYZ movePoint,
           XYZ connectionPoint) =>
             operationCurve.MakeBound(staticPoint, connectionPoint)
             .Where(c => c != null)
             .Where(c => c.ApproximateLength > sourceCurve.ApproximateLength)
            .Where(c => c.Contains(movePoint) || c.IsCircle());

        private static readonly Func<Curve, Curve, double> _distinctLength = (sourceCurve, c) =>
        Math.Abs(sourceCurve.ApproximateLength - c.ApproximateLength);


    }
}
