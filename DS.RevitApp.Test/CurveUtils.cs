using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    public static class CurveUtils
    {
        public static CurveLoop TryCreateLoop(
            IEnumerable<Curve> curves, 
            Func<Curve, Curve, Curve, Curve> getConnectedCurve)
        {
            var resultLoop = new CurveLoop();

            var connectedCurves = TryConnect(curves, getConnectedCurve);
            foreach (var curve in connectedCurves)
            {
                try
                { resultLoop.Append(curve); }
                catch (Exception)
                { Debug.WriteLine("Failed to create closed loop."); return null; }
            }

            if (resultLoop.Count() == 0 || resultLoop.IsOpen())
            { Debug.WriteLine("Failed to create closed loop."); return null; }
            else
            {
                Debug.WriteLine($"CurveLoop create successfully with " +
                $"{resultLoop.Count()} curves!");
            }

            return resultLoop;
        }

        public static IEnumerable<Curve> TryConnect(
            IEnumerable<Curve> curves, 
            Func<Curve, Curve, Curve, Curve> getConnectedCurve)
        {
            var resultCurves = new List<Curve>();

            var linkedCurves = new LinkedList<Curve>(curves);
            var currentNode = linkedCurves.First;
            var previous = linkedCurves.Last;
            while (currentNode != null)
            {
                var connectedCurve = getConnectedCurve(
                    currentNode.Value,
                    previous.Value,
                    (currentNode.Next ?? linkedCurves.First).Value);
                if (connectedCurve != null)
                { resultCurves.Add(connectedCurve); }
                else { break; }
                //break;
                previous = currentNode;
                currentNode = currentNode.Next;
            }

            return resultCurves;
        }

        public static bool IsBaseEndFitted(
            Curve baseCurve,
            Curve curveToFit)
        {
            var sp1 = baseCurve.GetEndPoint(0);
            var sp2 = baseCurve.GetEndPoint(1);

            var tp1 = curveToFit.GetEndPoint(0);
            var tp2 = curveToFit.GetEndPoint(1);

            var d1 = Math.Min(sp1.DistanceTo(tp1), sp1.DistanceTo(tp2));
            var d2 = Math.Min(sp2.DistanceTo(tp1), sp2.DistanceTo(tp2));
            return d2 < d1;
        }


        public static Curve FitStartToBaseEnd(
        Curve baseCurve,
        Curve curveToFit)
        {
            var sourceEnd = baseCurve.GetEndPoint(1);

            var tp1 = curveToFit.GetEndPoint(0);
            var tp2 = curveToFit.GetEndPoint(1);

            var d1 = sourceEnd.DistanceTo(tp1);
            var d2 = sourceEnd.DistanceTo(tp2);

            return d1 < d2 ? curveToFit : curveToFit.CreateReversed();
        }

        public static IEnumerable<Curve> FitEndToStart(IEnumerable<Curve> curves)
        {
            var firstFitted = curves.ElementAt(0);
            firstFitted = IsBaseEndFitted(firstFitted, curves.ElementAt(1)) ? 
                firstFitted : 
                firstFitted.CreateReversed();
            var fittedCurves = new List<Curve>()
            {
                firstFitted
            };

            for (var i = 1; i < curves.Count(); i++)
            {
                var current = curves.ElementAt(i);
                var fitted = FitStartToBaseEnd(fittedCurves.Last(), current);
                fittedCurves.Add(fitted);
            }

            return fittedCurves;
        }
    }
}
