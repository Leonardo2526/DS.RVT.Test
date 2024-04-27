using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Basis;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using DS.RhinoInside;
using DS.GraphUtils.Entities;
using OLMP.RevitAPI.Tools.Graphs;
using Rhino;
using Autodesk.Revit.DB.DirectContext3D;
using OLMP.RevitAPI.Tools.Untested;

namespace DS.RevitApp.Test
{
    public static class NewCurveExtensions
    {

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

        public static bool HasEqualDirection(this Curve curve1, Curve curve2, double parameter = 0)
        {
            var basis1 = curve1.GetBasis(parameter);
            var basis2 = curve2.GetBasis(parameter);
            var at = 1.DegToRad();

            if (curve1 is Line && curve2 is Line)
            { return basis1.X.IsParallelTo(basis2.X, at) == 1; }

            return basis1.Z.IsParallelTo(basis2.Z, at) == 1;
        }

        public static Curve GetClosestIntersection(
            this Curve sourceCurve,
            Curve targetCurve,
            bool isVirtualTrimEnable,
            bool isVirtualExtendEnable, out XYZ intersectionPoint)
        {
            intersectionPoint = null;
            var curve1 = CurveUtils_Untested.IsBaseEndFitted(sourceCurve, targetCurve) ?
               sourceCurve :
               sourceCurve.CreateReversed();
            var resultCurve = curve1
                .TrimOrExtend(targetCurve, isVirtualTrimEnable, isVirtualExtendEnable)
                .FirstOrDefault();
            if (resultCurve == null) { return null; }

            //var sp1 = sourceCurve.GetEndPoint(0);
            //var sp2 = sourceCurve.GetEndPoint(1);
            var rp1 = resultCurve.GetEndPoint(0);
            var rp2 = resultCurve.GetEndPoint(1);
            intersectionPoint = targetCurve.GetDistance(rp1) < targetCurve.GetDistance(rp2) ?
                rp1 : rp2;
            return resultCurve;
        }

        public static double GetDistance(this Curve curve, XYZ point)
        {
            try
            {
                return curve.Distance(point);
            }
            catch (Exception)
            {
                return point.DistanceTo(curve.GetEndPoint(0));
            }
        }


    }
}

