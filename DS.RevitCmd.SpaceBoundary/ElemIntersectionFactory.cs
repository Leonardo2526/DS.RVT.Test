using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitApp.Test;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Graphs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
namespace DS.RevitCmd.SpaceBoundary
{
    internal class ElemIntersectionFactory
    {

        private readonly Document _activeDoc;
        private readonly SolidElementIntersectionFactoryBase<Element> _intersectionFactory;

        public ElemIntersectionFactory(
            Document activeDoc,
             SolidElementIntersectionFactoryBase<Element> intersectionFactory)
        {
            _activeDoc = activeDoc;
            _intersectionFactory = intersectionFactory;
        }


        public ILogger Logger { get; set; }

        public bool CanFakeIntersections { get; set; } = true;

        public IEnumerable<ElementXYZIntersection> GetIntersections(
            Element element)
        {

            var xYZIntersections = new List<ElementXYZIntersection>();

            Logger?.Information($"Try get : {element.Id} boundaries"); ;
            _intersectionFactory.ItemQuickFilters = [];
            var exclusionFilter = new ExclusionFilter(new List<ElementId>() { element.Id });
            _intersectionFactory.ItemQuickFilters.Add((exclusionFilter, null));

            var zoneSolid = GetZoneSolid(element);
            //if (zoneSolid != null)
            //{ ShowSolid(zoneSolid); }

            var intersections = _intersectionFactory.GetIntersections(zoneSolid);
            Logger?.Information("Intersections found: " + intersections.Count());

            var wallIntersections = intersections.OfType<Wall>();
            var wallXYZIntersections = GetxYZIntersection(element as Wall, wallIntersections);
            xYZIntersections.AddRange(wallXYZIntersections);
            return xYZIntersections;

            Solid GetZoneSolid(Element element)
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
        }


        private IEnumerable<ElementXYZIntersection> GetxYZIntersection(
          Wall parentWall,
          IEnumerable<Wall> intersectionElements)
        {
            var xYZIntersections = new List<ElementXYZIntersection>();
            var baseWallCurve = parentWall.GetLocationCurve();         

            var basePoints = new List<XYZ>()
                    {
                        baseWallCurve.GetEndPoint(0),
                        baseWallCurve.GetEndPoint(1)
                    };
            var baseOrigin = basePoints.First();
            foreach (var elem in intersectionElements)
            {
                var aCurve = elem.GetLocationCurve();
                var curve = baseWallCurve.GetEndPoint(0).Z.IsAlmostEqual(aCurve.GetEndPoint(0).Z)
                    ? aCurve :
                    ProjectOnBase(baseOrigin, aCurve);
                var intersectionCurve = curve
                    .GetClosestIntersection(baseWallCurve, true, true, out var intersectionPoint);
                intersectionPoint ??= basePoints.OrderBy(p => curve.GetDistance(p)).FirstOrDefault();
                var intersection = new ElementXYZIntersection(parentWall, elem, intersectionPoint);
                xYZIntersections.Add(intersection);
            }



            return xYZIntersections;

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
