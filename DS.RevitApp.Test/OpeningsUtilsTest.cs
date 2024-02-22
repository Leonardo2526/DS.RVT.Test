using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools;
using DS.ClassLib.VarUtils;
using Serilog;
using MoreLinq;
using Autodesk.Revit.DB.Analysis;
using OLMP.RevitAPI.Tools.Extensions.RhinoExtensions;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Autodesk.Revit.DB.Mechanical;
using OLMP.RevitAPI.Tools.Openings;
using System.Diagnostics;
using System.Xml.Linq;
using OLMP.RevitAPI.Tools.Solids;
using Autodesk.Revit.UI.Selection;
using OLMP.RevitAPI.Tools.Lines;
using static System.Windows.Forms.LinkLabel;
using System.Security.Cryptography;
using OLMP.RevitAPI.Tools.Intersections;

namespace DS.RevitApp.Test
{
    internal class OpeningsUtilsTest
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
        private static double _at = Rhino.RhinoMath.ToRadians(1);

        public OpeningsUtilsTest(UIDocument uiDoc)
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



        public IEnumerable<Solid> GetOpeningsSolids()
        {
            var openingsSolids = new List<Solid>();

            var floor = GetFloor();
            if (floor == null) { return openingsSolids; }

            var floorSolid = floor.Solid(_allLoadedLinks);
            var faces = GetTopPlanarFaces(floor, floorSolid).ToList();
            faces.OrderByDescending(f => f.Area).ToList();
            var loops = faces[0].GetEdgesAsCurveLoops();



            //var sketches = GetSketches(floor);
            if (faces == null || faces.Count() == 0) { return openingsSolids; };

            var boundaryPlane = GetBoundary(floor);
            var polyLines = Project(faces, boundaryPlane);
            //polyLines = polyLines.Where(pl => pl.IsValid && pl.IsClosed);

            var lines = polyLines.SelectMany(pl => pl.GetSegments().Select(s => s.ToXYZ()));

            _trb.CreateAsync(() => lines.ForEach(l => l.Show(_doc)), "ShowSurfaceSolid");

            return openingsSolids;
        }

        private IEnumerable<Sketch> GetSketches(Floor floor, Solid floorSolid)
        {
            var genIds = new List<ElementId>();

           
            foreach (Face face in floorSolid.Faces)
            {
                var faceGenenIds = floor.GetGeneratingElementIds(face);
                faceGenenIds.ForEach(id =>
                {
                    if (!genIds.Contains(id)) { genIds.Add(id); }
                });
            }

            return genIds.Select(s => _doc.GetElement(s)).OfType<Sketch>();
        }

        private IEnumerable<PlanarFace> GetTopPlanarFaces(Floor floor, Solid floorSolid)
        {

            var floorNormal = XYZ.BasisZ.ToVector3d();
            var planarFaces = floorSolid.Faces.OfType<PlanarFace>().ToList();

            return planarFaces
                .Where(pf => pf.FaceNormal.ToVector3d()
                .IsParallelTo(floorNormal, _at) == 1);
        }


        public ILogger Logger { get; set; }


        private Floor GetFloor()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var element = _doc.GetElement(reference);

            if (element is Floor)
            {
                return (Floor)element;
            }
            return null;
        }

        private PlanarFace GetBoundary(Floor floor)
        {
            var opt = new EnergyAnalysisDetailModelOptions();
            var eModel = _trb.CreateAsync(() => EnergyAnalysisDetailModel.Create(_doc, opt), "CreateModel").Result;
            var spaces = eModel.GetAnalyticalSpaces();
            var modelOpenings = eModel.GetAnalyticalOpenings();
            var modelSurfaces = eModel.GetAnalyticalSurfaces();

            var floorSurface = modelSurfaces
                .FirstOrDefault(s =>
                {
                    var id = TryGetOriginatingId(s);
                    return id != null && id == floor.Id;
                });
            var surfaceSolid = GetSolid(floorSurface, 0.001);
            var floorNormal = floorSurface.GetPolyloop().Direction.ToVector3d();
            _trb.CreateAsync(() => surfaceSolid.ShowShape(_doc), "ShowSurfaceSolid");

            var planarFaces = surfaceSolid.Faces.OfType<PlanarFace>().ToList();
            var wall1YFaces = planarFaces.
                FirstOrDefault(pf => pf.FaceNormal.ToVector3d().IsParallelTo(floorNormal, _at) == -1);

            //var polyLoop = floorSurface..GetPolyloop();
            //var points = polyLoop.GetPoints();

            return wall1YFaces;
        }

        private Solid GetSolid(EnergyAnalysisSurface analysisSurface, double extrusionValue)
        {
            var polyLoop = analysisSurface.GetPolyloop();
            var surfaceLoop = ToLoop(polyLoop);
            var surfaceLoops = new List<CurveLoop>() { surfaceLoop };
            return GeometryCreationUtilities
                .CreateExtrusionGeometry(surfaceLoops, polyLoop.Direction, extrusionValue);

        }


        private void Show(EnergyAnalysisSurface analysisSurface)
        {
            var surfaceSolid = GetSolid(analysisSurface, 0.001);
            _trb.CreateAsync(() => surfaceSolid.ShowShape(_doc), "ShowSurfaceSolid");
        }

        private static CurveLoop ToLoop(Polyloop polyloop)
        {
            var points = polyloop.GetPoints();
            var curves = new List<Curve>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                var line = Line.CreateBound(points[i], points[i + 1]);
                curves.Add(line);
            }
            var lastline = Line.CreateBound(points.Last(), points.First());
            curves.Add(lastline);
            return CurveLoop.Create(curves); ;
        }


        private IEnumerable<Rhino.Geometry.Polyline> Project(IEnumerable<Face> faces, Face pFace)
        {
            var polyLines = new List<Rhino.Geometry.Polyline>();

            foreach (var face in faces)
            {
                var sPolyLines = Project(face, pFace);
                polyLines.AddRange(sPolyLines);
            }

            return polyLines;

            static IEnumerable<Rhino.Geometry.Polyline> Project(Face face, Face pFace)
            {
                var polyLines = new List<Rhino.Geometry.Polyline>();

                var profiles = face.GetEdgesAsCurveLoops();
                foreach (var profile in profiles)
                {
                    foreach (Curve curve in profile)
                    {
                        var points = curve.Tessellate();
                        var projRhonoPoints = points.Select(p => pFace.Project(p).XYZPoint.ToPoint3d());
                        var polyline = new Rhino.Geometry.Polyline(projRhonoPoints);
                        polyLines.Add(polyline);
                    }
                }

                return polyLines;
            }
        }

        static IEnumerable<Rhino.Geometry.Polyline> Project(IList<CurveLoop> loops, Face pFace)
        {
            var polyLines = new List<Rhino.Geometry.Polyline>();

            foreach (var loop in loops)
            {
                foreach (Curve curve in loop)
                {
                    var points = curve.Tessellate();
                    var projRhonoPoints = points.Select(p => p.ToPoint3d());
                    var polyline = new Rhino.Geometry.Polyline(projRhonoPoints);
                    polyLines.Add(polyline);
                }
            }

            return polyLines;
        }


        private IEnumerable<Rhino.Geometry.Polyline> Project(IEnumerable<Sketch> sketches, PlanarFace pFace)
        {
            var polyLines = new List<Rhino.Geometry.Polyline>();

            foreach (var sketch in sketches)
            {
                var sPolyLines = Project(sketch, pFace);
                polyLines.AddRange(sPolyLines);
            }

            return polyLines;

            static IEnumerable<Rhino.Geometry.Polyline> Project(Sketch sketch, PlanarFace pFace)
            {
                var polyLines = new List<Rhino.Geometry.Polyline>();

                var profile = sketch.Profile;
                foreach (CurveArray curveArray in profile)
                {
                    foreach (Curve curve in curveArray)
                    {
                        var points = curve.Tessellate();
                        var projRhonoPoints = points.Select(p => pFace.Project(p).XYZPoint.ToPoint3d());
                        var polyline = new Rhino.Geometry.Polyline(projRhonoPoints);
                        polyLines.Add(polyline);
                    }
                }

                return polyLines;
            }
        }

        private ElementId TryGetOriginatingId(EnergyAnalysisSurface surface)
        {
            var description = surface.OriginatingElementDescription;

            var ind1 = description.IndexOf("[") + 1;
            var ind2 = description.IndexOf("]");
            var result = description.Substring(ind1, ind2 - ind1);
            var id = int.Parse(result);
            return id >= 0 ? new ElementId(id) : null;

        }
    }


}
