using Autodesk.Revit.DB;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using Rhino;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitApp.Test
{
    public static class CurveExtensionsTest
    {
        public static ITransactionFactory TransactionFactory { get; set; }
        private static Document _activeDoc => TransactionFactory.Doc;

        public static IEnumerable<Curve> Trim(
            this Curve sourceCurve,
            Curve targetCurve,
            bool isVirtualEnable = false)
        {
            var resultCurves = new List<Curve>();

            var staticIndex = 0;
            var staticPoint = sourceCurve.GetEndPoint(staticIndex);
            var staticParamter = sourceCurve.GetEndParameter(staticIndex);
            staticPoint.Show(_activeDoc, 200.MMToFeet(), TransactionFactory);
            var movePoint = sourceCurve.GetEndPoint(1);

            Debug.WriteLine($"Static points is {staticPoint}");
            Debug.WriteLine($"pointToConnect is {movePoint}");
            Debug.WriteLine($"trying to connect point {movePoint} " +
                  $"of {sourceCurve.GetType().Name} to {targetCurve.GetType().Name}.");

            Curve targetOperationCurve;
            if (isVirtualEnable)
            {
                targetOperationCurve = targetCurve.Clone();
                targetOperationCurve.MakeUnbound();
            }
            else { targetOperationCurve = targetCurve; }
            var intersection = sourceCurve
                .Intersect(targetOperationCurve, out var resultArray);

            switch (intersection)
            {
                case SetComparisonResult.Overlap:
                    var intersecionCurves =
                        GetIntersectionCurves(sourceCurve, staticParamter, resultArray);
                    if (intersecionCurves != null)
                    { resultCurves.AddRange(intersecionCurves); }
                    break;
                case SetComparisonResult.Equal:
                    { resultCurves.Add(sourceCurve); }
                    break;
                default:
                    break;
            }

            Debug.WriteLine($"{resultCurves.Count} intersection curves were found.");
            return resultCurves;

            static IEnumerable<Curve> GetIntersectionCurves(
                Curve sourceCurve,
                double sourceStaticParameter,
                IntersectionResultArray resultArray)
            {
                var intersectionCurves = new List<Curve>();

                foreach (IntersectionResult intersectionResult in resultArray)
                {
                    var intersectionCurve = sourceCurve.Clone();
                    var projection = intersectionCurve
                                   .Project(intersectionResult.XYZPoint);
                    var intersectionParameter = projection.Parameter;
                    intersectionCurve.MakeBound(sourceStaticParameter, intersectionParameter);
                    intersectionCurves.Add(intersectionCurve);
                }

                intersectionCurves =
                    intersectionCurves.OrderBy(c => c.ApproximateLength).ToList();

                return intersectionCurves;
            }
        }

        public static IEnumerable<Curve> Extend(
            this Curve sourceCurve,
            Curve targetCurve,
            bool isVirtualEnable = false)
        {
            var resultCurves = new List<Curve>();

            var staticIndex = 0;
            var staticPoint = sourceCurve.GetEndPoint(staticIndex);
            var staticParamter = sourceCurve.GetEndParameter(staticIndex);
            staticPoint.Show(_activeDoc, 200.MMToFeet(), TransactionFactory);
            var movePoint = sourceCurve.GetEndPoint(1);

            Debug.WriteLine($"Static points is {staticPoint}");
            Debug.WriteLine($"pointToConnect is {movePoint}");
            Debug.WriteLine($"trying to connect point {movePoint} " +
                  $"of {sourceCurve.GetType().Name} to {targetCurve.GetType().Name}.");

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

            switch (intersection)
            {
                case SetComparisonResult.Overlap:
                    var intersecionCurves =
                        GetIntersectionCurves(sourceCurve, 
                        sourceOperationCurve,
                        staticOperationParamter, movePoint, resultArray);
                    if (intersecionCurves != null)
                    { resultCurves.AddRange(intersecionCurves); }
                    break;
                case SetComparisonResult.Equal:
                    { resultCurves.Add(sourceCurve); }
                    break;
                default:
                    break;
            }

            Debug.WriteLine($"{resultCurves.Count} intersection curves were found.");
            return resultCurves;

            IEnumerable<Curve> GetIntersectionCurves(
                Curve sourceCurve,
               Curve sourceOperationCurve,
               double sourceStaticParameter,
               XYZ movePoint,
               IntersectionResultArray resultArray)
            {
                var intersectionCurves = new List<Curve>();

                foreach (IntersectionResult intersectionResult in resultArray)
                {
                    var currentResultCurves = GetIntersectionCurves(intersectionResult, sourceOperationCurve, sourceStaticParameter);
                    var intersectionCurve = currentResultCurves
                        .FirstOrDefault(c => c.Project(movePoint).Distance < 0.001 
                        && sourceCurve.Project(intersectionResult.XYZPoint).Distance >0.001);

                    foreach (var item in currentResultCurves)
                    {
                        var r= item.Project(movePoint);
                    }

                    if (intersectionCurve != null)
                    { intersectionCurves.Add(intersectionCurve); }
                }

                intersectionCurves =
                    intersectionCurves.OrderBy(c => c.ApproximateLength).ToList();

                return intersectionCurves;

                static IEnumerable<Curve> GetIntersectionCurves(IntersectionResult intersectionResult, Curve baseCurve, double sourceParameter)
                {
                    var intersectionCurves = new List<Curve>();

                    var projection = baseCurve.Project(intersectionResult.XYZPoint);
                    var targetParameter = projection.Parameter;

                    var p11 = Math.Min(sourceParameter, targetParameter);
                    var p12 = Math.Max(sourceParameter, targetParameter);
                    var curve1 = baseCurve.Clone();
                    curve1.MakeBound(p11, p12);
                    intersectionCurves.Add(curve1);

                    if (baseCurve.IsCyclic)
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


    }


}

