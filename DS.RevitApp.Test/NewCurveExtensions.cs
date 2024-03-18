using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Basis;
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

        public static Basis3d GetBasis(this Curve curve, double parameter = 0)
        {
            var transforms = curve.ComputeDerivatives(parameter, true);

            var origin = transforms.Origin.Normalize().ToPoint3d();
            var x = transforms.BasisX.ToVector3d();
            x.Unitize();
            var z = transforms.BasisZ.ToVector3d();
            z = z.IsZero ? Rhino.Geometry.Vector3d.ZAxis : z;
            z.Unitize();
            var y = Rhino.Geometry.Vector3d.CrossProduct(z, x);
            y.Unitize();
            return new Basis3d(origin, x, y, z);
        }

        public static Rhino.Geometry.Vector3d GetNormal(this Curve curve, double parameter = 0)
        => curve.GetBasis(parameter).Z;

        public static bool HasEqualDirection(this Curve curve1, Curve curve2, double parameter = 0)
        {
            var basis1 = curve1.GetBasis(parameter);
            var basis2 = curve2.GetBasis(parameter);
            var at = 1.DegToRad();

            if (curve1 is Line && curve2 is Line)
            { return basis1.X.IsParallelTo(basis2.X, at) == 1; }

            return basis1.Z.IsParallelTo(basis2.Z, at) == 1;
        }

        public static bool Contains(this Curve curve, XYZ point,
             bool containsStrict = true,
             double tolerance = Rhino.RhinoMath.ZeroTolerance)
        {
            if (curve.IsBound && !containsStrict)
            {
                if (curve.GetEndPoint(0).DistanceTo(point) < tolerance ||
                    curve.GetEndPoint(1).DistanceTo(point) < tolerance)
                { return false; }
            }

            return curve.Distance(point) < tolerance;
        }

        public static Curve Trim(
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

        public static Curve Extend(
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

        public static IEnumerable<Curve> TrimOrExtend(
         this Curve sourceCurve,
         Curve targetCurve,
         bool isVirtualTrimEnable = false,
         bool isVirtualExtendEnable = false,
          int staticIndex = 0)
        {
            var resultCurves = new List<Curve>();
            var trimResult = sourceCurve.Trim(targetCurve, isVirtualTrimEnable, staticIndex);
            if (trimResult != null)
            { resultCurves.Add(trimResult); }
            var extendResult = sourceCurve.Extend(targetCurve, isVirtualExtendEnable, staticIndex);
            if (extendResult != null)
            { resultCurves.Add(extendResult); }

            Func<Curve, Curve, double> _distinctLength = (sourceCurve, c) =>
            Math.Abs(sourceCurve.ApproximateLength - c.ApproximateLength);
            resultCurves = resultCurves
                .OrderBy(c => _distinctLength(sourceCurve, c)).ToList();

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
            var sp1 = sourceCurve.GetEndPoint(0);
            var rp1 = result?.GetEndPoint(0);
            //var sp2 = sourceCurve.GetEndPoint(1);
            //var rp2 = result?.GetEndPoint(1);

            if (result != null && sp1.DistanceTo(rp1) > 0.001)
            { result = result.CreateReversed(); }
            //return result;
            result ??= sourceCurve;
            return result?.TrimOrExtend(next, isVirtualTrimEnable, isVirtualExtendEnable, 0)
                .FirstOrDefault() ?? result;
        }

        public static Curve TrimOrExtendAny(
        this Curve sourceCurve,
        Curve target,
         bool isVirtualTrimEnable = false,
         bool isVirtualExtendEnable = false)
        {
            var result = sourceCurve
                .TrimOrExtend(target, isVirtualTrimEnable, isVirtualExtendEnable, 1)
                .FirstOrDefault();
            //return result;
            if (result != null && result.GetEndParameter(0) != sourceCurve.GetEndParameter(0))
            { result = result.CreateReversed(); }
            result ??= sourceCurve;
            return result?.TrimOrExtend(target, isVirtualTrimEnable, isVirtualExtendEnable, 0)
                .FirstOrDefault() ?? result;
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
            //curve1 = isReversed ? curve1.CreateReversed() : curve1;
            results.Add(curve1);

            if (curve.IsCyclic && !curve.IsBound)
            {
                validCurve.MakeBound(proj2.Parameter, proj1.Parameter + curve.Period);
                //validCurve = isReversed ? validCurve : validCurve.CreateReversed();
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

