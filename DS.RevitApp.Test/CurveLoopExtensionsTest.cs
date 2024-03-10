using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    public static class CurveLoopExtensionsTest
    {
        public static CurveLoop TryMakeClosed(this CurveLoop curveLoop)
        {
            if (!curveLoop.IsOpen()) return curveLoop;

            var resultLoop = new CurveLoop();

            var linkedCurves = new LinkedList<Curve>(curveLoop);
            var currentNode = linkedCurves.First;
            var previous = linkedCurves.Last;
            while (resultLoop.Count() <= curveLoop.Count())
            {
                var connectedCurve = ConnectCurve(currentNode.Value, previous.Value, currentNode.Next.Value);
                if (connectedCurve != null)
                { resultLoop.Append(connectedCurve); }
                else { break; }
                previous = currentNode;

                currentNode = currentNode.Next ?? linkedCurves.First;
            }

            if (resultLoop.IsOpen())
            { Debug.WriteLine("Failed to create closed loop."); return null; }


            return resultLoop;

            static Curve ConnectCurve(Curve curve, Curve previous, Curve next)
            {
                var result = curve.CreateReversed();
                result = (result.Extend(previous, true) ?? result.Trim(previous)).FirstOrDefault();
                result = result.CreateReversed();
                result = (result.Extend(next, true) ?? result.Trim(next)).FirstOrDefault();
                return result;
            }
        }
    }
}
