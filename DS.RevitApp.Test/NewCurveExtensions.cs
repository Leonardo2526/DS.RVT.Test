using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace DS.RevitApp.Test
{
    public static class NewCurveExtensions
    {
        public static ITransactionFactory TransactionFactory { get; set; }
        private static Document _activeDoc => TransactionFactory.Doc;

        public static bool IsCircle(this Curve curve)
        {
            if (!curve.IsCyclic || !curve.IsBound) return false;

            var p1 = curve.GetEndParameter(0);
            var p2 = curve.GetEndParameter(1);
            var deltaP = p2 - p1;

            return Math.Abs(deltaP - Math.PI * 2) < Rhino.RhinoMath.ZeroTolerance;
        }


        public static ViewportRotation TryGetRotation(this Curve curve, XYZ normal)
        {
            if (!curve.IsCyclic) { return ViewportRotation.None; }

            var rNormal = normal.ToVector3d();
            XYZ curveNormal = null;

            var at = 1.DegToRad();
            switch (curve)
            {
                case Ellipse ellipse:
                    curveNormal = ellipse.Normal;
                    break;
                case Arc arc:
                    curveNormal = arc.Normal;
                    break;
                default:
                    break;
            }

            return curveNormal != null && rNormal.IsParallelTo(curveNormal.ToVector3d(), at) == 1 ?
                ViewportRotation.Clockwise :
                ViewportRotation.Counterclockwise;
        }


        public static bool Contains(this Curve curve, XYZ point,
            double tolerance = Rhino.RhinoMath.ZeroTolerance)
            => curve.Distance(point) < tolerance;

        public static Curve BestTrim(
            this Curve sourceCurve,
            Curve targetCurve,
            bool isVirtualEnable = false,
            int staticIndex = 0)
            => CurveConnector.Connect(
                sourceCurve,
                targetCurve,
                CurveConnector.ConnectionOption.Trim,
                isVirtualEnable,
                staticIndex)?.FirstOrDefault();

        public static Curve BestExtend(
           this Curve sourceCurve,
           Curve targetCurve,
           bool isVirtualEnable = false,
           int staticIndex = 0)
           => CurveConnector.Connect(
               sourceCurve,
               targetCurve,
               CurveConnector.ConnectionOption.Extend,
               isVirtualEnable,
               staticIndex)?.FirstOrDefault();


        public static IEnumerable<Curve> Trim(
            this Curve sourceCurve,
            Curve targetCurve,
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

            Curve targetOperationCurve;
            if (isVirtualEnable)
            {
                targetOperationCurve = targetCurve.Clone();
                targetOperationCurve.MakeUnbound();
            }
            else { targetOperationCurve = targetCurve; }
            var intersection = sourceCurve
                .Intersect(targetOperationCurve, out var resultArray);

            Curve getIntersectionCurve(XYZ point) =>
            GetResultIntersectionCurves(point, sourceCurve, staticParamter)
            .FirstOrDefault(c =>
                    (c.Project(movePoint).Distance > 0.001 &&
                        sourceCurve.Project(point).Distance < 0.001)
                    || IsCircle(c)
                );

            switch (intersection)
            {
                case SetComparisonResult.Overlap:
                    var results = resultArray.AsEnumerable<IntersectionResult>();
                    var resultPoints = results.Select(r => r.XYZPoint);
                    var intersecionCurves = resultPoints
                        .Select(getIntersectionCurve)
                        .Where(c => c != null)
                        .OrderByDescending(c => c.ApproximateLength);
                    var count = intersecionCurves.Count();
                    //Debug.WriteLine($"{count} " +
                    //    $"curves were trimmed.");
                    resultCurves.AddRange(intersecionCurves);
                    break;
                case SetComparisonResult.Subset:
                    {
                        var eqResultPoints = new List<XYZ>()
                        { targetCurve.GetEndPoint(0), targetCurve.GetEndPoint(1) };
                        var eqIntersecionCurves = eqResultPoints
                           .Select(getIntersectionCurve)
                           .Where(c => c != null)
                           .OrderByDescending(c => c.ApproximateLength);
                        resultCurves.AddRange(eqIntersecionCurves);
                    }
                    break;
                case SetComparisonResult.Equal:
                    { resultCurves.Add(sourceCurve); }
                    break;
                default:
                    break;
            }

            //Debug.WriteLine($"{resultCurves.Count} intersection curves were found.");
            return resultCurves;
        }

        public static IEnumerable<Curve> Extend(
            this Curve sourceCurve,
            Curve targetCurve,
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

            //Debug.WriteLine($"Trying to extend point {movePoint} of " +
            //$"{sourceCurve.GetType().Name} to {targetCurve.GetType().Name}");

            var sourceOperationCurve = sourceCurve.Clone();
            sourceOperationCurve.MakeUnbound();
            var staticOperationParamter = sourceOperationCurve.Project(staticPoint).Parameter;

            Curve targetOperationCurve;
            if (isVirtualEnable)
            {
                targetOperationCurve = targetCurve.Clone();
                targetOperationCurve.MakeUnbound();
            }
            else { targetOperationCurve = targetCurve; }
            var intersection = sourceOperationCurve
                .Intersect(targetOperationCurve, out var resultArray);

            Curve getIntersectionCurve(XYZ point) =>
                GetResultIntersectionCurves(point, sourceOperationCurve, staticOperationParamter)
                .FirstOrDefault(c =>
                    (c.Project(movePoint).Distance < 0.001 &&
                        sourceCurve.Project(point).Distance > 0.001)
                    || IsCircle(c)
                );

            switch (intersection)
            {
                case SetComparisonResult.Overlap:
                    var results = resultArray.AsEnumerable<IntersectionResult>();
                    var resultPoints = results.Select(r => r.XYZPoint);
                    var intersecionCurves = resultPoints
                       .Select(getIntersectionCurve)
                       .Where(c => c != null)
                       .OrderBy(c => c.ApproximateLength);
                    var count = intersecionCurves.Count();
                    //Debug.WriteLine($"{count} " +
                    //    $"curves were extent.");
                    resultCurves.AddRange(intersecionCurves);
                    break;
                case SetComparisonResult.Equal:
                    {
                        var eqResultPoints = new List<XYZ>()
                        { targetCurve.GetEndPoint(0), targetCurve.GetEndPoint(1) };
                        var eqIntersecionCurves = eqResultPoints
                           .Select(getIntersectionCurve)
                           .Where(c => c != null)
                           .OrderBy(c => c.ApproximateLength);
                        resultCurves.AddRange(eqIntersecionCurves);
                    }
                    break;
                default:
                    break;
            }

            return resultCurves;
        }


        public static IEnumerable<Curve> TrimOrExtend(
          this Curve sourceCurve,
          Curve targetCurve,
          bool isVirtualTrimEnable = false,
          bool isVirtualExtendEnable = false,
           int staticIndex = 0)
        {
            var resultCurves = new List<Curve>();
            var trimResult = sourceCurve.Trim(targetCurve, isVirtualTrimEnable, staticIndex);
            resultCurves.AddRange(trimResult);
            var extendResults = sourceCurve.Extend(targetCurve, isVirtualExtendEnable, staticIndex);
            resultCurves.AddRange(extendResults);

            resultCurves = resultCurves
                .OrderBy(c => Math.Abs(sourceCurve.ApproximateLength - c.ApproximateLength)).ToList();

            return resultCurves;
        }

        public static Curve TrimOrExtend(
          this Curve sourceCurve,
          Curve previous, Curve next,
           bool isVirtualTrimEnable = false,
           bool isVirtualExtendEnable = false)
        {
            var result = sourceCurve
                .TrimOrExtend(previous, isVirtualTrimEnable, isVirtualExtendEnable, 1)
                .FirstOrDefault();
            result ??= sourceCurve;
            return result?.TrimOrExtend(next, isVirtualTrimEnable, isVirtualExtendEnable, 0)
                .FirstOrDefault() ?? result;
        }

        public static IEnumerable<Curve> TrimOrExtendAtClosestPoints(
         this Curve sourceCurve,
         Curve targetCurve,
          bool isVirtualTrimEnable = false,
          bool isVirtualExtendEnable = false)
        {
            var sp1 = sourceCurve.GetEndPoint(0);
            var sp2 = sourceCurve.GetEndPoint(1);

            var tp1 = targetCurve.GetEndPoint(0);
            var tp2 = targetCurve.GetEndPoint(1);

            var d1 = Math.Min(sp1.DistanceTo(tp1), sp1.DistanceTo(tp2));
            var d2 = Math.Min(sp2.DistanceTo(tp1), sp2.DistanceTo(tp2));
            int staticIndex = d1 < d2 ? 1 : 0;

            return TrimOrExtend(sourceCurve, targetCurve,
                isVirtualTrimEnable, isVirtualExtendEnable, staticIndex);
        }

        public static IEnumerable<Curve> MakeBound(
           this Curve curve,
            XYZ point1, XYZ point2)
        {
            var results = new List<Curve>();
            var tolerance = 1;
            var proj1 = curve.Project(point1);
            if (proj1.Distance > tolerance)
            { throw new ArgumentException($"The curve must contains point {point1}."); }

            var proj2 = curve.Project(point2);
            if (proj2.Distance > tolerance)
            { throw new ArgumentException($"The curve must contains point {point2}."); }

            var isReversed = proj1.Parameter > proj2.Parameter;

            var validCurve = curve.Clone();
            if (isReversed)
            {
                validCurve = validCurve.CreateReversed();
                proj1 = validCurve.Project(point1);
                proj2 = validCurve.Project(point2);
            }

            double param2 = equalParamteters(proj1, proj2)
                && (IsCircle(curve) || !curve.IsBound) ?
                proj2.Parameter + curve.Period : proj2.Parameter;
            var curve1 = validCurve.Clone();
            curve1.MakeBound(proj1.Parameter, param2);
            results.Add(curve1);

            if (curve.IsCyclic && !curve.IsBound)
            {
                validCurve.MakeBound(proj2.Parameter, proj1.Parameter + curve.Period);
                results.Add(validCurve.CreateReversed());
            }
            return results;

            static bool equalParamteters(IntersectionResult proj1, IntersectionResult proj2)
            {
                return Math.Abs(proj2.Parameter - proj1.Parameter) < Rhino.RhinoMath.ZeroTolerance;
            }
        }


        public static IEnumerable<Curve> GetResultIntersectionCurves(
            XYZ intersectionPoint,
            Curve baseCurve,
            double baseStaticParameter)
        {
            var intersectionCurves = new List<Curve>();

            var projection = baseCurve.Project(intersectionPoint);
            var targetParameter = projection.Parameter;
            if (Math.Abs(baseStaticParameter - targetParameter) < Rhino.RhinoMath.ZeroTolerance)
            { return intersectionCurves; }

            var p11 = Math.Min(baseStaticParameter, targetParameter);
            var p12 = Math.Max(baseStaticParameter, targetParameter);
            var p121 = Math.Abs(p12 - p11) < Rhino.RhinoMath.ZeroTolerance ? p12 + baseCurve.Period : p12;
            var curve1 = baseCurve.Clone();
            curve1.MakeBound(p11, p121);
            intersectionCurves.Add(curve1);

            if (baseCurve.IsCyclic && !IsCircle(curve1))
            {
                var p21 = p12;
                var p22 = baseCurve.Period + p11;
                var curve2 = baseCurve.Clone();
                curve2.MakeBound(p21, p22);
                intersectionCurves.Add(curve2);
            }

            return intersectionCurves;
        }

    }
}

