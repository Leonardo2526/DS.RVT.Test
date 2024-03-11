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
                previous = currentNode;
                currentNode = currentNode.Next;
            }

            return resultCurves;
        }
    }
}
