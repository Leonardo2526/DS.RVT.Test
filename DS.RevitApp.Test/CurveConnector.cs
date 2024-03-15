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

            var intersection = sourceCurve
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
                    var eqPoints = new List<XYZ>();
                    var p1 = targetCurve.GetEndPoint(0);
                    var p2 = targetCurve.GetEndPoint(1);
                    if (connectionOption == ConnectionOption.Trim)
                    {
                        if(sourceCurve.Contains(p1))
                        { eqPoints.Add(p1);}
                        if (sourceCurve.Contains(p2))
                        { eqPoints.Add(p2); }
                    }
                    else
                    {
                        if (!sourceCurve.Contains(p1))
                        { eqPoints.Add(p1); }
                        if (!sourceCurve.Contains(p2))
                        { eqPoints.Add(p2); }
                    }
                    connectionPoints = eqPoints;
                    break;
                default:
                    break;
            }

            IEnumerable<Curve> getCurves(XYZ point)
            => connectionOption == ConnectionOption.Trim ?
                GetTrimCurves(sourceCurve, sourceOperationCurve, staticPoint, point) :
                GetExtendCurves(sourceCurve, sourceOperationCurve, staticPoint, point);

            //Debug.WriteLine($"{resultCurves.Count} intersection curves were found.");
            return connectionPoints?.SelectMany(getCurves);
        }

        private static IEnumerable<Curve> GetTrimCurves(
            Curve sourceCurve,
            Curve operationCurve,
            XYZ staticPoint,
            XYZ connectionPoint) =>
              operationCurve.MakeBound(staticPoint, connectionPoint)
              .Where(c => c != null)
              .Where(c => c.ApproximateLength < sourceCurve.ApproximateLength)
            .OrderByDescending(c => c.ApproximateLength);

        private static IEnumerable<Curve> GetExtendCurves(
           Curve sourceCurve,
           Curve operationCurve,
           XYZ staticPoint,
           XYZ connectionPoint) =>
             operationCurve.MakeBound(staticPoint, connectionPoint)
             .Where(c => c != null)
             .Where(c => c.ApproximateLength > sourceCurve.ApproximateLength)
            .OrderBy(c => c.ApproximateLength);

    }
}
