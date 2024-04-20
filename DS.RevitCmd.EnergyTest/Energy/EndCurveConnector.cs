using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using OLMP.RevitAPI.Tools.Connections.PointModels;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.Energy
{
    /// <summary>
    /// The object to connect <see cref="Curve"/>s.
    /// </summary>
    internal class EndCurveConnector : ISerilogged, ICurveConnector
    {
        /// <summary>
        /// Is virtual intersection enable.
        /// </summary>
        public bool IsVirtualEnable { get; set; }

        public ILogger Logger { get; set; }

        /// <inheritdoc/>
        public XYZ ConnectionPoint { get; private set; }

        /// <summary>
        /// Try to connect the end point (at index 1) of <paramref name="sourceCurve"/> to <paramref name="targedCurve"/>.
        /// </summary>
        /// <param name="sourceCurve"></param>
        /// <param name="targedCurve"></param>
        /// <returns>
        /// Connected <see cref="Curve"/> or <see langword="null"/>.
        /// </returns>
        public Curve TryConnect(Curve sourceCurve, Curve targedCurve)
        {
            var staticPoint = sourceCurve.GetEndPoint(0);
            var pointToConnect = sourceCurve.GetEndPoint(1);

            Logger?.Verbose($"Static points is {staticPoint}");
            Logger?.Verbose($"pointToConnect is {pointToConnect}");

            //test stuff
            //var sp1 = sourceCurve.GetEndPoint(staticIndex);
            //var sParameter1 = sourceCurve.GetEndParameter(staticIndex);
            //var sp2 = sourceCurve.GetEndPoint(indexOfPointToConnect);
            //var sParameter2 = sourceCurve.GetEndParameter(indexOfPointToConnect);
            Logger?.Verbose($"trying to connect point {pointToConnect} " +
                $"of {sourceCurve.GetType().Name} to {targedCurve.GetType().Name}.");

            Curve sourceConnected =
                TryConnectWithRealIntersection(sourceCurve, targedCurve, pointToConnect, staticPoint);
            sourceConnected = sourceConnected == null && IsVirtualEnable ?
                TryConnectWithVirtualIntersection(sourceCurve, targedCurve, pointToConnect, staticPoint) :
                sourceConnected;

            if (sourceConnected == null)
            { Logger?.Information("Failed to connect curves."); }

            return sourceConnected;

        }


        private Curve TryConnectWithRealIntersection(
            Curve sourceCurve,
            Curve targedCurve,
            XYZ pointToConnect,
            XYZ staticPoint)
        {
            Logger?.Verbose("trying connect with real intersection");
            var sourceStaticProj = sourceCurve.Project(staticPoint);

            var sourceConnected = GetConnectedCurve(
                sourceCurve,
                targedCurve,
                pointToConnect,
                sourceStaticProj.Parameter, out var realConnectionPoint);

            if (sourceConnected != null)
            {
                ConnectionPoint = realConnectionPoint;
                Logger?.Verbose("Connected with real intersection successfully!");
            }
            return sourceConnected;
        }

        private Curve TryConnectWithVirtualIntersection(
            Curve sourceCurve,
            Curve targedCurve,
            XYZ pointToConnect,
            XYZ staticPoint)
        {
            Logger?.Verbose("trying connect with virtual intersection");
            var sourceCloned = sourceCurve.Clone();
            sourceCloned.MakeUnbound();
            var sourceClonedProj1 = sourceCloned.Project(staticPoint);

            var targetCloned = targedCurve.Clone();
            targetCloned.MakeUnbound();

            var sourceConnected = GetConnectedCurve(
                sourceCloned,
                targetCloned,
                pointToConnect,
                sourceClonedProj1.Parameter, out var virtualConnectionPoint);

            if (sourceConnected != null)
            {
                ConnectionPoint = virtualConnectionPoint;
                Logger?.Verbose("Connected with virtual intersection successfully!");
            }

            return sourceConnected;
        }

        private Curve GetConnectedCurve(
            Curve curve1,
            Curve curve2,
            XYZ pointToConnect,
            double staticParameter,
            out XYZ connectionPoint)
        {
            connectionPoint = null;

            var intersection = curve1.Intersect(curve2, out var resultArray);
            if (resultArray is null || resultArray.Size == 0) { return null; }

            var connectedCurves = new List<Curve>();
            switch (intersection)
            {
                case SetComparisonResult.Overlap:
                    {
                        foreach (IntersectionResult intersectionResult in resultArray)
                        {
                            var checkCurve = curve1.Clone();
                            var checkCurveIntersection = checkCurve
                                .Project(intersectionResult.XYZPoint);
                            var intersectionParameter = checkCurveIntersection.Parameter;

                            //var sourceClonedProj1 = checkCurve.Project(_staticPoint);
                            //var sourceClonedProj2 = checkCurve.Project(pointToConnect);

                            if (intersectionParameter < staticParameter)
                            {
                                if (curve1.IsCyclic)
                                { intersectionParameter = curve1.Period + intersectionParameter; }
                                else { continue; }
                            }
                            checkCurve.MakeBound(staticParameter, intersectionParameter);
                            connectedCurves.Add(checkCurve);
                        }
                        break;
                    }
                case SetComparisonResult.Equal:
                    {
                        return curve1;
                    }

                default:
                    break;
            }

            connectedCurves = connectedCurves
                .OrderByDescending(c => c.ApproximateLength).ToList();

            var connectedCurve = connectedCurves.LastOrDefault();
            connectionPoint = connectedCurve?.GetEndPoint(1);

            return connectedCurve;
        }
    }

}
