using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Points;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Geometry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitApp.Test
{
    internal class GetOpeningsSolidTest : ISerilogged
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly List<RevitLinkInstance> _allLoadedLinks;
        private readonly List<Document> _allFilteredDocs;
        private readonly DocumentFilter _globalFilter;
        private readonly ContextTransactionFactory _trb;
        //create global filter
        private readonly List<BuiltInCategory> _excludedCategories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_GenericAnnotation,
            BuiltInCategory.OST_TelephoneDevices,
            BuiltInCategory.OST_Materials,
            BuiltInCategory.OST_Massing
        };
        private static double _feetsToMeters = Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Feet, Rhino.UnitSystem.Meters);
        private static double _squareFeetsToMeters = Math.Pow(_feetsToMeters, 2);


        public GetOpeningsSolidTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _allLoadedLinks = _doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
            _allFilteredDocs = new List<Document>() { _doc };
            _allFilteredDocs.AddRange(_allLoadedLinks.Select(l => l.GetLinkDocument()));
            _globalFilter = new DocumentFilter(_allFilteredDocs, _doc, _allLoadedLinks);
            _globalFilter.QuickFilters =
            [
                (new ElementMulticategoryFilter(_excludedCategories, true), null),
                (new ElementIsElementTypeFilter(true), null),
            ];
            _trb = new ContextTransactionFactory(_doc);
        }

        public ILogger Logger { get; set; }


        public void GetWallSolid()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var element = _doc.GetElement(reference);

            if (element is not Wall wall)
            { return; }

            var wallInseretsIds = wall.FindInserts(true, false, false, false) ?? new List<ElementId>();
            var inserts = wallInseretsIds.Select(i => _doc.GetElement(i)).ToList();
            var openings = inserts.OfType<Opening>();

            foreach (var opening in openings)
            {
                var oSolid = GetSolid(opening, wall);
                if (oSolid != null)
                { _trb.CreateAsync(() => oSolid.ShowShape(_doc), "ShowSurfaceSolid"); }
            }
        }

        private Solid GetSolid(Opening opening, Wall wall)
        {
            var wallLocCurve = wall.Location as LocationCurve;
            var wallCurve = wallLocCurve.Curve;

            var rect = GetRectangle(opening, wallCurve, wall);
            _trb.CreateAsync(() => rect.Show(_doc), "ShowSurfaceSolid");
            //return null;

            var lines = rect.ToRevitLines();

            var boundPoints = opening.BoundaryRect;

            var p1 = boundPoints[0];
            var result1 = wallCurve.Project(p1);
            var param1 = result1.Parameter;

            var p2 = boundPoints[1];
            var result2 = wallCurve.Project(p2);
            var param2 = result2.Parameter;
            wallCurve.MakeBound(param1, param2);


            var profileLoops = new List<CurveLoop>();
            var curves = new List<Curve>();
            lines.ForEach(l => curves.Add(l));
            var profileLoop = CurveLoop.Create(curves);
            profileLoops.Add(profileLoop);
            var isOpen = profileLoop.IsOpen();

            var sweepPath = CurveLoop.Create(new List<Curve>() { wallCurve });
            var startParam = wallCurve.GetEndParameter(0);
            //var solid = GeometryCreationUtilities.CreateExtrusionGeometry(profileLoops, XYZ.BasisY, 1);
            var solid = GeometryCreationUtilities.CreateSweptGeometry(sweepPath, 0, startParam, profileLoops);

            return solid;
        }

        private Rhino.Geometry.Rectangle3d GetRectangle(Opening opening, Curve wallCurve, Wall wall)
        {
            var boundPoints = opening.BoundaryRect;

            var mainFaces = GetMainFaces(wall).ToList();
            //var curve1 = OffsetToPoint(wallCurve, boundPoints[0]);
            //_trb.CreateAsync(() => curve1.Show(_doc), "ShowSurfaceSolid");
            var curve2 = OffsetToPoint(wallCurve, boundPoints[1]);
            _trb.CreateAsync(() => curve2.Show(_doc), "ShowSurfaceSolid");
            //return default;

            var p1 = mainFaces[0].Project(boundPoints[0]).XYZPoint.ToPoint3d();
            var proj02 = curve2.Project(boundPoints[0]).XYZPoint;
            var p2 = mainFaces[1].Project(proj02).XYZPoint.ToPoint3d();
            var offsetCurve = wallCurve.CreateOffset(-1, XYZ.BasisZ);
            var origin = mainFaces[0].Project(p2.ToXYZ()).XYZPoint.ToPoint3d();

            var plane = new Rhino.Geometry.Plane(origin, p1, p2);
            return new Rhino.Geometry.Rectangle3d(plane, p1, p2);
        }



        private Curve OffsetToPoint(Curve curve, XYZ boundPoints)
        {
            var proj = curve.Project(boundPoints);
            var moveVector = boundPoints - proj.XYZPoint;
            var transform = Autodesk.Revit.DB.Transform.CreateTranslation(moveVector);
            return curve.CreateTransformed(transform);
        }



        private IEnumerable<Face> GetMainFaces(Wall wall)
        {
            var faces = new List<Face>();


            var solid = wall.Solid(_allLoadedLinks);
            foreach (Face item in solid.Faces)
            {
                faces.Add(item);
            }

            faces = faces.OrderByDescending(f => f.Area).ToList();

            return new List<Face>() { faces[0], faces[1] };
        }
    }
}
