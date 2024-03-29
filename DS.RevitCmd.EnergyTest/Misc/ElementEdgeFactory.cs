using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitApp.Test;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Graphs;
using QuickGraph;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using boundaryEdge = QuickGraph.TaggedEdge<OLMP.RevitAPI.Tools.Graphs.XYZVertex,
    DS.RevitCmd.EnergyTest.SpaceBoundary.BoundaryCurve>;

namespace DS.RevitCmd.EnergyTest.SpaceBoundary
{
    internal class ElementEdgeFactory
    {
        private IVertexAndEdgeListGraph<XYZVertex, boundaryEdge> _graph;
        private static double _tolerance = RhinoMath.ZeroTolerance;
        private int _lastVertexId;

        public ElementEdgeFactory()
        {
        }


        public ElementEdgeFactory Initiate(
            IVertexAndEdgeListGraph<XYZVertex, boundaryEdge> graph)
        {
            _graph = graph;
            return this;
        }


        public IEnumerable<boundaryEdge> CreateEdges(
            IEnumerable<ElementXYZIntersection> xYZIntersections, 
            XYZVertex parentVertex)
        {
            var result = new List<boundaryEdge>();

            var xYZIntersectionsList = xYZIntersections.ToList();
            _lastVertexId = _graph.Vertices.Last().Id;

            for (int i = 1; i < xYZIntersectionsList.Count; i++)
            {
                ElementXYZIntersection intersection = xYZIntersectionsList[i];
                var v1 = result.LastOrDefault()?.Target ?? parentVertex;

                var p2 = intersection.Result;
                var v2 = TryGetVertex(p2, _graph.Vertices, ref _lastVertexId);

                var wall1 = intersection.Item1 as Wall;
                var curve = CreateBoundCurve(wall1, v1.Tag, v2.Tag);
                var boundaryCurve = new BoundaryCurve(wall1.Id, curve);
                var edge = new boundaryEdge(v1, v2, boundaryCurve);
                result.Add(edge);
            }
            
            var axEdge1 = TryGetAuxiliaryEdge(xYZIntersections.First(),
                result, _graph, ref _lastVertexId);
            if (axEdge1 != null) { result.Add(axEdge1); }
            var axEdge2 = TryGetAuxiliaryEdge(xYZIntersections.Last(),
                result, _graph, ref _lastVertexId);
            if (axEdge2 != null) { result.Add(axEdge2); }

            return result;

            static Curve CreateBoundCurve(Wall wall, XYZ point1, XYZ point2)
            {
                var wallCurve = wall.GetLocationCurve();
                var uWallCurve = wallCurve.Clone();
                uWallCurve.MakeUnbound();
                var resultCurves = uWallCurve.MakeBound(point1, point2);
                var result = resultCurves.FirstOrDefault(c => wallCurve.Contains(c.GetCenter()));
                if(!result.GetEndPoint(0).IsPointAlmostEqualTo(point1))
                { result = result.CreateReversed(); }
                return result;
            }
        }

        private boundaryEdge TryGetAuxiliaryEdge(
            ElementXYZIntersection xYZIntersection, 
            IEnumerable<boundaryEdge> edges,
            IVertexAndEdgeListGraph<XYZVertex, boundaryEdge> graph, ref int index)
        {
            if(xYZIntersection.Item2 is null || 
                graph.Edges.Select(e => e.Tag.ElementId).Contains(xYZIntersection.Item2.Id)) 
            { return null; }
            var fitLine = FitIntersection(xYZIntersection);
            if (fitLine == null) { return null; }

            var p1 = fitLine.GetEndPoint(0);
            var p2 = fitLine.GetEndPoint(1);

            var vertices = edges.SelectMany(e => e.Vertices());
            var source = NewExtensions.FindVertexByLocation(p1, vertices);
            var target = TryGetVertex(p2, graph.Vertices, ref index);
            var boundaryCurve = new BoundaryCurve(xYZIntersection.Item2.Id, fitLine);
            return new boundaryEdge(source, target, boundaryCurve);
        }


        private Line FitIntersection(ElementXYZIntersection intersection)
        {
            if (intersection.Item2 is not Wall wall) { return null; }

            var sourcePoint = intersection.Result;

            var targetCurve = wall.GetLocationCurve();
            var unboundTargetCurve = targetCurve.Clone();
            unboundTargetCurve.MakeUnbound();
            if (unboundTargetCurve.Contains(sourcePoint)) { return null; }  

            var targetPoints = new List<XYZ>()
            {
                targetCurve.GetEndPoint(0),
                targetCurve.GetEndPoint(1)
            };
            var targetPoint = targetPoints.OrderBy(p => p.DistanceTo(sourcePoint)).First();

            if (sourcePoint.IsPointAlmostEqualTo(targetPoint)) { return null; }

            return Line.CreateBound(sourcePoint, targetPoint);
        }

        private XYZVertex CreateVertex(XYZ location, ref int index)
        {
            index++;
            return new XYZVertex(index, location);
        }


        private XYZVertex TryGetVertex(XYZ location, IEnumerable<XYZVertex> verticesSet, ref int index)
            => NewExtensions.FindVertexByLocation(location, verticesSet) ?? CreateVertex(location, ref index);

    }
}
