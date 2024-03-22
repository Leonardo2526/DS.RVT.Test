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

                var wallCurve = wall.GetLocationCurve();
                //var wp1 = wallCurve.GetEndPoint(0);
                //var wp2 = wallCurve.GetEndPoint(1);
                //var wparam1 = wallCurve.GetEndParameter(0);
                //var wparam2 = wallCurve.GetEndParameter(1);

                var adjacancyCurves = wallIntersections.Select(w => w.GetLocationCurve()).ToList();
                var intersectionPoints = GetIntersectionPoints(wallCurve, adjacancyCurves);

                var (point1, point2) = XYZUtils.GetMaxDistancePoints(intersectionPoints.ToList(), out double maxDist);
                intersectionPoints = intersectionPoints.OrderBy(p => p.DistanceTo(point1));
                Curve boundWallCurve = GetBoundWallCurve(wallCurve, point1, point2);
                var parameters = intersectionPoints
                    .Select(p => boundWallCurve.Project(p).Parameter)                  
                    .OrderBy(p => p).ToList();
                var rhinoCurve = boundWallCurve.ToCurve();
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

            static Curve GetBoundWallCurve(Curve wallCurve, XYZ point1, XYZ point2)
            {
                var uWallCurve = wallCurve.Clone();
                uWallCurve.MakeUnbound();
                var resultCurves = uWallCurve.MakeBound(point1, point2);
                return resultCurves.FirstOrDefault(c => wallCurve.Contains(c.GetCenter()));
            }
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


        private IEnumerable<XYZ> GetIntersectionPoints(
            Curve baseWallCurve, 
            IEnumerable<Curve> adjacancyCurves)
        {
            var intersectionPoints = new List<XYZ>();

            var basePoints = new List<XYZ>()
                    {
                        baseWallCurve.GetEndPoint(0),
                        baseWallCurve.GetEndPoint(1)
                    };
            var baseOrigin = basePoints.First();
            foreach (var aCurve in adjacancyCurves)
            {
                var curve = baseWallCurve.GetEndPoint(0).Z.IsAlmostEqual(aCurve.GetEndPoint(0).Z)
                    ? aCurve :
                    ProjectOnBase(baseOrigin, aCurve);
                var intersectionCurve = curve
                    .GetClosestIntersection(baseWallCurve, true, true, out var intersectionPoint);
                intersectionPoint ??= basePoints.OrderBy(p => curve.GetDistance(p)).FirstOrDefault();
                intersectionPoints.Add(intersectionPoint);
            }

            return intersectionPoints;

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
