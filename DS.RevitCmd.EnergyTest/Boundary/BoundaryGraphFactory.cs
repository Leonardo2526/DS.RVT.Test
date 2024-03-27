using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitApp.Test;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Graphs;
using QuickGraph;
using Rhino;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using boundaryEdge = QuickGraph.TaggedEdge<OLMP.RevitAPI.Tools.Graphs.XYZVertex,
    DS.RevitCmd.EnergyTest.SpaceBoundary.BoundaryCurve>;

namespace DS.RevitCmd.EnergyTest.SpaceBoundary
{
    internal class BoundaryGraphFactory : ISerilogged
    {
        private static double _tolerance = RhinoMath.ZeroTolerance;
        private readonly Document _activeDoc;
        private readonly ElementEdgeFactory _edgeFactory;
        private readonly IEnumerable<BoundarySegment> _boundarySegments;
        private readonly IEnumerable<Curve> _boundaryCurves;
        private readonly ElemIntersectionFactory _intersectionFactory;
        private readonly Solid _spaceSolid;
        private readonly AdjacencyGraph<XYZVertex, boundaryEdge> _graph = new();
        private readonly List<boundaryEdge> _boundaryEdges = [];
        private readonly List<ElementId> _closedIds = [];

        public BoundaryGraphFactory(
            Document activeDoc,
            ElementEdgeFactory edgeFactory,
            IEnumerable<BoundarySegment> boundarySegments,
            IEnumerable<Curve> allBoundaryCurves,
             ElemIntersectionFactory intersectionFactory,
             Solid spaceSolid)
        {
            _activeDoc = activeDoc;
            edgeFactory.Initiate(_graph);
            _edgeFactory = edgeFactory;
            _boundarySegments = boundarySegments;
            _boundaryCurves = allBoundaryCurves;
            _intersectionFactory = intersectionFactory;
            _spaceSolid = spaceSolid;
        }

        public ILogger Logger { get; set; }


        public IVertexAndEdgeListGraph<XYZVertex, boundaryEdge> Create()
        {
            _graph.Clear();
            _boundaryEdges.Clear();
            _closedIds.Clear();

            //IEnumerable<boundaryEdge> edges = null;
            BoundarySegment segment;
            XYZVertex parentVertex = null;
            segment = _boundarySegments.FirstOrDefault();
            while (segment != null)
            {
                var element = _activeDoc.GetElement(segment.ElementId);
                var edges = GetEdges(element, parentVertex);
                var closedIds = _graph.Edges.Select(e => e.Tag.ElementId);
                segment = _boundarySegments
                    .FirstOrDefault(s => !closedIds.Contains(s.ElementId));
            }
            return _graph;
        }

        private IEnumerable<boundaryEdge> GetEdges(Element element, XYZVertex parentVertex)
        {
            //if (element.Id.IntegerValue == 216571)
            //{ }

            _closedIds.Add(element.Id);
            var allIntersections = new List<ElementXYZIntersection>();
            var xYZIntersections = _intersectionFactory.GetIntersections(element);
            allIntersections.AddRange(xYZIntersections);
            var fakeIntersections = GetFakeIntersections(xYZIntersections, element as Wall);
            allIntersections.AddRange(fakeIntersections);

            parentVertex ??= GetNewParent(xYZIntersections, element);
            allIntersections = allIntersections
                .OrderBy(p => p.Result.DistanceTo(parentVertex.Tag)).ToList();

            var eEdges = _edgeFactory.CreateEdges(allIntersections, parentVertex);
            _graph.AddVerticesAndEdgeRange(eEdges);

            xYZIntersections = xYZIntersections
                .Where(e => !_closedIds.Contains(e.Item2.Id));
            var elementVertices = new List<XYZVertex>();
            elementVertices.AddRange(eEdges.Select(e => e.Source));
            elementVertices.AddRange(eEdges.Select(e => e.Target));
            foreach (var intersection in xYZIntersections)
            {
                var foundAxEdge = eEdges
                    .FirstOrDefault(e => e.Tag.ElementId == intersection.Item2.Id);
                var nextParent = foundAxEdge != null ?
                    foundAxEdge.Target :
                    elementVertices.Find(v => v.Tag.IsPointAlmostEqualTo(intersection.Result));
                var edges = GetEdges(intersection.Item2, nextParent);
            }
            return eEdges;

        }
        private XYZVertex GetNewParent(
            IEnumerable<ElementXYZIntersection> xYZIntersections, Element element)
        {
            XYZVertex parentVertex;
            var id = _graph.Vertices.LastOrDefault()?.Id is null ? 0 : 1;
            var intersectionPoints = xYZIntersections
                                    .Select(e => e.Result).ToList();
            if (intersectionPoints.Count == 0 && element is Wall wall)
            {
                var curve = wall.GetLocationCurve();
                parentVertex = new XYZVertex(id, curve.GetEndPoint(0));
            }
            if (intersectionPoints.Count == 1)
            {
                parentVertex = new XYZVertex(id, intersectionPoints.First());
            }
            else
            {
                var (point1, point2) = XYZUtils
                    .GetMaxDistancePoints(intersectionPoints, out double maxDist);
                parentVertex = new XYZVertex(id, point1);
            }

            _graph.AddVertex(parentVertex);
            return parentVertex;
        }

        private IEnumerable<ElementXYZIntersection> GetFakeIntersections(
            IEnumerable<ElementXYZIntersection> xYZIntersections,
            Wall wall)
        {
            var fakeIntersections = new List<ElementXYZIntersection>();

            if (xYZIntersections.Count() == 0)
            {
                var wallCurve = wall.GetLocationCurve();
                var p1 = wallCurve.GetEndPoint(0);
                var fakeIntersection1 = new ElementXYZIntersection(wall, null, p1);
                fakeIntersections.Add(fakeIntersection1);

                var p2 = wallCurve.GetEndPoint(1);
                var fakeIntersection2 = new ElementXYZIntersection(wall, null, p2);
                fakeIntersections.Add(fakeIntersection2);
            }
            else if (xYZIntersections.Count() == 1)
            {
                var wallCurve = wall.GetLocationCurve();
                var endPoints = new List<XYZ>()
                { wallCurve.GetEndPoint(0), wallCurve.GetEndPoint(1) };
                var otherPoints = endPoints
                    .Where(p => !xYZIntersections.First().Result.IsAlmostEqualTo(p));
                var point2 = otherPoints.Count() == 1 ?
                    otherPoints.First() :
                    GetClosest(otherPoints, _spaceSolid);
                //otherPoints.First(p => _boundaryCurves.Any(c => c.Contains(p)));
                //otherPoints.First(p => _boundaryCurves.Any(c => c.Distance(p) < 1));
                var fakeIntersection1 = new ElementXYZIntersection(wall, null, point2);
                fakeIntersections.Add(fakeIntersection1);
            }

            return fakeIntersections;
        }

        /// <summary>
        /// Check if <paramref name="solid"/> contains <paramref name="point"/>.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="point"></param>
        /// <param name="allowOnSurface"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="point"/> is inside <paramref name="solid"/> or 
        /// on it surface if <paramref name="allowOnSurface"/> is set to <see langword="true"/>.</returns>
        public static double DistanceTo(XYZ point, Solid solid)
        {
            var faces = solid.Faces;
            var projections = new List<IntersectionResult>();
            foreach (Face face in faces)
            {
                var prj = face.Project(point);
                if (prj is not null)
                { projections.Add(prj); }
            }

            return projections.Count == 0 ?
                double.NaN :
                projections.OrderBy(p => p.Distance).First().Distance;
        }

        public XYZ GetClosest(IEnumerable<XYZ> points, Solid solid)
        {
            var closestPoint = points.FirstOrDefault();
            var minDist = double.MaxValue;

            foreach (var point in points)
            {
                var dist = DistanceTo(point, solid);
                if (dist < minDist)
                { closestPoint = point; minDist = dist; }
            }

            return closestPoint;
        }
    }
}

