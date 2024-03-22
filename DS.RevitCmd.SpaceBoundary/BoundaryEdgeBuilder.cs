using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Intersections;
using DS.GraphUtils.Entities;
using OLMP.RevitAPI.Tools;
using QuickGraph;
using DS.RevitApp.Test;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using DS.ClassLib.VarUtils;
using Serilog;
using MoreLinq;
using DS.RhinoInside;
using boundaryEdge = QuickGraph.TaggedEdge<
    DS.GraphUtils.Entities.TaggedVertex<Autodesk.Revit.DB.XYZ>,
    DS.RevitCmd.SpaceBoundary.BoundaryCurve>;
using DS.RhinoInside.Revit.Convert.Geometry;
using QuickGraph.Algorithms;
using RG = Rhino.Geometry;
using RhinoMath = Rhino.RhinoMath;
using ARDB = Autodesk.Revit.DB;

namespace DS.RevitCmd.SpaceBoundary
{
    internal class BoundaryEdgeBuilder : ISerilogged
    {
        private readonly Document _activeDoc;
        private readonly SolidElementIntersectionFactoryBase<Element> _intersectionFactory;

        public BoundaryEdgeBuilder(Document activeDoc,
            SolidElementIntersectionFactoryBase<Element> intersectionFactory)
        {
            _activeDoc = activeDoc;
            _intersectionFactory = intersectionFactory;
        }

        public ITransactionFactory TransactionFactory { get; set; }
        public ILogger Logger { get; set; }

        public IEnumerable<boundaryEdge> Create(Element element)
        {
            var result = new List<boundaryEdge>();

            _intersectionFactory.ItemQuickFilters = [];
            var exclusionFilter = new ExclusionFilter(new List<ElementId>() { element.Id });
            _intersectionFactory.ItemQuickFilters.Add((exclusionFilter, null));

            var zoneSolid = GetZoneSolid(element);
            //if (zoneSolid != null)
            //{ ShowSolid(zoneSolid); }

            var intersections = _intersectionFactory.GetIntersections(zoneSolid);
            Logger?.Information("Intersections found: " + intersections.Count());


            if (element is Wall wall)
            {
                var wallIntersections = intersections.OfType<Wall>();
                if (wallIntersections.Count() < 2)
                { 
                    Logger?.Warning("Failed to get boundary edge.");
                    return null; 
                }
                var intersectionPoints = GetProjPoints(wall, wallIntersections);

                var (point1, point2) = XYZUtils.GetMaxDistancePoints(intersectionPoints.ToList(), out double maxDist);
                intersectionPoints = intersectionPoints.OrderBy(p => p.DistanceTo(point1));
                //intersectionPoints.ForEach(p => p.Show(_activeDoc, 0, TransactionFactory));

                var wallCurve = wall.GetLocationCurve();
                var rhinoCurve = wallCurve.ToCurve();
                //var unboundCurve = wallCurve.Clone();
                //unboundCurve.MakeUnbound();
                var parameters = intersectionPoints
                    .Select(p => wallCurve.Project(p).Parameter)
                    .OrderBy(p => p);
                rhinoCurve = rhinoCurve.Trim(parameters.First(), parameters.Last());
                var splitted = rhinoCurve.Split(parameters);
                foreach (var sCurve in splitted)
                {
                    var boundaryCurve = new BoundaryCurve(element.Id, sCurve.ToCurve());
                    var p1 = boundaryCurve.Curve.GetEndPoint(0);
                    var v1 = new TaggedVertex<XYZ>(0, p1);
                    var p2 = boundaryCurve.Curve.GetEndPoint(1);
                    var v2 = new TaggedVertex<XYZ>(1, p2);
                    var edge = new boundaryEdge(v1, v2, boundaryCurve);
                    result.Add(edge);
                }
            }
            else
            { throw new NotImplementedException(); }


            return result;
        }

        private Solid GetZoneSolid(Element element)
        {
            var offsetDist = 0.001;
            double height;
            CurveLoop profile;
            switch (element)
            {
                case Wall wall:
                    {
                        profile = wall.GetBottomProfile();
                        height = wall.GetHeigth();
                    }
                    break;
                default:
                    { throw new NotImplementedException(); }
            }
            if (profile == null) { return null; }

            //ShowCurves(profile);
            //return null;

            profile = CurveLoop.CreateViaOffset(profile, offsetDist, -XYZ.BasisZ);

            return GeometryCreationUtilities
               .CreateExtrusionGeometry(
               new List<CurveLoop> { profile },
               XYZ.BasisZ, height);
        }


        private IEnumerable<XYZ> GetProjPoints(Wall baseWall, IEnumerable<Wall> walls)
        {
            var points = new List<XYZ>();

            var baseWallCurve = baseWall.GetLocationCurve();
            var basePoints = new List<XYZ>()
                    {
                        baseWallCurve.GetEndPoint(0),
                        baseWallCurve.GetEndPoint(1)
                    };
            var baseOrigin = basePoints.First();
            foreach (var wall in walls)
            {
                var curve = wall.GetLocationCurve();
                curve = ProjectOnBase(baseOrigin, curve);
                var intersectionPoint = curve
                    .ClosestIntersection(baseWallCurve, true, true, out var IntersectionResult);
                intersectionPoint ??= curve.GetDistance(basePoints.First()) < curve.GetDistance(basePoints.Last()) ?
                        basePoints.First() : basePoints.Last();
                points.Add(intersectionPoint);
            }

            return points;

            static Curve ProjectOnBase(XYZ baseOrigin, Curve curve)
            {
                var curveOrigin = curve.GetEndPoint(0);
                var vector = new XYZ(0, 0, curveOrigin.Z - baseOrigin.Z);
                var transform = Transform.CreateTranslation(vector);
                curve = curve.CreateTransformed(transform);
                return curve;
            }
        }
    }
}
