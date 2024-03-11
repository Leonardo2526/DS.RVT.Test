using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using Rhino;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace DS.RevitApp.Test
{
    public static class NewCurveExtensions
    {
        public static ITransactionFactory TransactionFactory { get; set; }
        private static Document _activeDoc => TransactionFactory.Doc;

        public static bool IsCircle(this Curve curve)
        {
            var p1 = curve.GetEndParameter(0);
            var p2 = curve.GetEndParameter(1);
            var deltaP = p2 - p1;

            return Math.Abs(deltaP - Math.PI * 2) < RhinoMath.ZeroTolerance;
        }


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

            Curve targetOperationCurve;
            if (isVirtualEnable)
            {
                targetOperationCurve = targetCurve.Clone();
                targetOperationCurve.MakeUnbound();
            }
            else { targetOperationCurve = targetCurve; }
            var intersection = sourceCurve
                .Intersect(targetOperationCurve, out var resultArray);

            Curve getIntersectionCurve(IntersectionResult result) =>
            GetResultIntersectionCurves(result, sourceCurve, staticParamter).FirstOrDefault();

            switch (intersection)
            {
                case SetComparisonResult.Overlap:
                    var results = resultArray.AsEnumerable<IntersectionResult>();
                    var intersecionCurves = results
                        .Select(getIntersectionCurve)
                        .Where(c => c != null)
                        .OrderBy(c => c.ApproximateLength);
                    resultCurves.AddRange(intersecionCurves);
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

            Curve getIntersectionCurve(IntersectionResult result) =>
                GetResultIntersectionCurves(result, sourceOperationCurve, staticOperationParamter)
                .FirstOrDefault(c =>
                    (c.Project(movePoint).Distance < 0.001 &&
                        sourceCurve.Project(result.XYZPoint).Distance > 0.001)
                    || IsCircle(c)
                );

            switch (intersection)
            {
                case SetComparisonResult.Overlap:
                    var results = resultArray.AsEnumerable<IntersectionResult>();
                    var intersecionCurves = results
                       .Select(getIntersectionCurve)
                       .Where(c => c != null)
                       .OrderBy(c => c.ApproximateLength);
                    resultCurves.AddRange(intersecionCurves);
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

        public static IEnumerable<Curve> TrimOrExtend(
           this Curve sourceCurve,
           Curve targetCurve,
           bool isVirtualTrimEnable = false,
           bool isVirtualExtendEnable = false,
            int staticIndex = 0)
        {
            var result = sourceCurve.Trim(targetCurve, isVirtualTrimEnable, staticIndex);
            result = result == null || result.Count() == 0 ? 
                sourceCurve.Extend(targetCurve, isVirtualExtendEnable, staticIndex) : 
                result;

            return result;  
        }

        public static IEnumerable<Curve> TrimOrExtendAnyPoint(
          this Curve sourceCurve,
          Curve targetCurve,
           bool isVirtualTrimEnable = false,
           bool isVirtualExtendEnable = false)
        {
            var result = TrimOrExtend(sourceCurve, targetCurve, 
                isVirtualTrimEnable, isVirtualExtendEnable, 0);
            result = result == null || result.Count() == 0 ? 
                TrimOrExtend(sourceCurve, targetCurve, 
                isVirtualTrimEnable, isVirtualExtendEnable, 1) :
                result;
            return result;
        }

        public static IEnumerable<Curve> GetResultIntersectionCurves(
            IntersectionResult intersectionResult,
            Curve baseCurve,
            double baseStaticParameter)
        {
            var intersectionCurves = new List<Curve>();

            var projection = baseCurve.Project(intersectionResult.XYZPoint);
            var targetParameter = projection.Parameter;

            var p11 = Math.Min(baseStaticParameter, targetParameter);
            var p12 = Math.Max(baseStaticParameter, targetParameter);
            var p121 = Math.Abs(p12 - p11) < RhinoMath.ZeroTolerance ? p12 + baseCurve.Period : p12;
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

