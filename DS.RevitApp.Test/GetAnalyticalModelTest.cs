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

namespace DS.RevitApp.Test
{
    internal class GetAnalyticalModelTest : ISerilogged
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


        public GetAnalyticalModelTest(UIDocument uiDoc)
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

        public IEnumerable<Room> GetRooms()
        {
            var roomDocFilter = _globalFilter.Clone();
            roomDocFilter.SlowFilters ??= new();
            roomDocFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = roomDocFilter.ApplyToAllDocs()
                .SelectMany(kv => kv.Value.ToElements(kv.Key))
                .OfType<Room>();
            if (Logger != null)
            {
                rooms.ForEach(r => Logger.Information(r.Name, r.Id));
            }
            return rooms;
        }


        public void Run()
        {
            var rooms = GetRooms();


            var spaceDocFilter = _globalFilter.Clone();
            var existSpaces = spaceDocFilter.ApplyToAllDocs()
                .SelectMany(kv => kv.Value.ToElements(kv.Key)).OfType<EnergyAnalysisSpace>();

            var room1 = rooms.FirstOrDefault();
            var model = room1.GetAnalyticalModel();
            //var b = EnergyDataSettings.CheckAnalysisType(AnalysisMode.ConceptualMassesAndBuildingElements);
            var opt = new EnergyAnalysisDetailModelOptions()
            {
                //ExportMullions = true,
                //IncludeShadingSurfaces = true,
                //Tier = EnergyAnalysisDetailModelTier.Final
            };
            //var m = EnergyAnalysisDetailModel.GetMainEnergyAnalysisDetailModel(_doc);           
            var eModel = _trb.CreateAsync(() => EnergyAnalysisDetailModel.Create(_doc, opt), "CreateModel").Result;
            var spaces = eModel.GetAnalyticalSpaces();
            var modelOpenings = eModel.GetAnalyticalOpenings();
            var modelSurfaces = eModel.GetAnalyticalSurfaces();

            var classFilter = new ElementMulticlassFilter(new List<Type>() { typeof(Opening) });
            var collector = new FilteredElementCollector(_doc)
                .WherePasses(classFilter);
            var elements = collector.ToElements();
            var ops = elements.OfType<Opening>();

            foreach (Parameter parameter in ops.Last().Parameters)
            {
                var sValue = parameter.AsValueString();
                var dValue = parameter.AsDouble();
            }


            double extrusionValue = 0.001;
            foreach (var space in spaces)
            {
                var surfaces = space.GetAnalyticalSurfaces();
                _trb.CreateAsync(() => ShowPolyLoop(space.GetBoundary().First(), 26.316), "ShowSpaces");
                //_trb.CreateAsync(() => surfaces.ForEach(s => ShowPolyLoop(s.GetPolyloop(), 0.001)), "ShowSurfaces");


                foreach (var surface in surfaces)
                {
                    var a2 = surface.GetParameters("Area");
                    var polyLoop = surface.GetPolyloop();
                    var surfaceLoop = ToLoop(polyLoop);
                    var surfaceLoops = new List<CurveLoop>() { surfaceLoop };

                    var value = a2.First().AsDouble() * _squareFeetsToMeters;
                    var surfaceSolid = GeometryCreationUtilities
                        .CreateExtrusionGeometry(surfaceLoops, polyLoop.Direction, extrusionValue);

                    var inseretsIds = new List<ElementId>();
                    var wall = TryGetWall(surface);
                    var wallInseretsIds = wall?.FindInserts(true, false, true, false) ?? new List<ElementId>();
                    inseretsIds.AddRange(wallInseretsIds);
                    var floor = TryGetFloor(surface);
                    var floorInseretsIds = floor?.FindInserts(true, true, true, true) ?? new List<ElementId>();
                    inseretsIds.AddRange(floorInseretsIds);
                    var geom = floor?.get_Geometry(new Options() { DetailLevel = ViewDetailLevel.Fine });
                    if (floor != null)
                    {
                        GetFace(floor);
                        var solids = SolidExtractor.GetSolids(floor).OrderByDescending(s => s.Volume);
                        if (solids.Count() > 1)
                        {
                            surfaceSolid = BooleanOperationsUtils
                                .ExecuteBooleanOperation(surfaceSolid, solids.Last(), BooleanOperationsType.Difference);
                        }
                    }

                    var wallHoles = GetOpeningHolesInWall(surface, surfaceSolid);
                    var floorHoles = GetOpeningHolesInFloor(surface, surfaceSolid);
                    var inserts = inseretsIds.Select(i => _doc.GetElement(i)).ToList();
                    var rOpenings = inserts.OfType<Opening>();

                    //show surfaces

                    var oSolids = new List<Solid>();
                    //var loops = new List<CurveLoop>() { surfaceLoop };
                    //var surfaceSolid = surface.get_Geometry(new Options()).FirstOrDefault() as Solid;

                    var openings = surface.GetAnalyticalOpenings();
                    foreach (var opening in openings)
                    {
                        //var oGeom = opening.get_Geometry(new Options());
                        //var oSolid = oGeom?.FirstOrDefault() as Solid;
                        var oPolyLoop = opening.GetPolyloop();
                        var oArea = oPolyLoop.ComputeArea();
                        var oLoop = ToLoop(oPolyLoop);
                        var oPolyLoops = new List<CurveLoop>() { oLoop };
                        //loops.Add(oLoop);
                        var oSolid = GeometryCreationUtilities
                       .CreateExtrusionGeometry(oPolyLoops, polyLoop.Direction, extrusionValue);
                        oSolids.Add(oSolid);
                    }

                    _trb.CreateAsync(() => oSolids.ForEach(s => s.ShowShape(_doc)), "ShowOpenings");

                    foreach (var oSolid in oSolids)
                    {
                        surfaceSolid = BooleanOperationsUtils
                            .ExecuteBooleanOperation(surfaceSolid, oSolid, BooleanOperationsType.Difference);
                    }

                    foreach (var rOpening in rOpenings)
                    {
                        var rArea = rOpening.GetParameters("Width");
                        foreach (Parameter param in rOpening.ParametersMap)
                        {
                            var sValue = param.AsValueString();
                            var dValue = param.AsValueString();
                            var s = param.AsString();
                        }
                        var oSolid = rOpening.TryGetSolid(_doc, _allLoadedLinks);
                        surfaceSolid = BooleanOperationsUtils
                            .ExecuteBooleanOperation(surfaceSolid, oSolid, BooleanOperationsType.Difference);
                    }
                    _trb.CreateAsync(() => surfaceSolid.ShowShape(_doc), "ShowSurfaceSolid");
                }
            }
        }


        private IEnumerable<Element> GetSpaces()
        {
            var spaceDocFilter = _globalFilter.Clone();
            var spaces = spaceDocFilter.ApplyToAllDocs()
                .SelectMany(kv => kv.Value.ToElements(kv.Key));

            return spaces;
        }

        private void ShowPolyLoop(Polyloop polyloop, double dist)
        {
            var loop = ToLoop(polyloop);
            var loops = new List<CurveLoop>() { loop };
            ShowLoops(loops, polyloop.Direction, dist);
        }

        private void ShowLoops(List<CurveLoop> loops, XYZ direction, double dist)
        {
            var solid = GeometryCreationUtilities.CreateExtrusionGeometry(loops, direction, dist);
            solid.ShowShape(_doc);
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

        public void Run1()
        {
            // Collect space and surface data from the building's analytical thermal model
            EnergyAnalysisDetailModelOptions options = new EnergyAnalysisDetailModelOptions();
            options.Tier = EnergyAnalysisDetailModelTier.SecondLevelBoundaries; // include constructions, schedules, and non-graphical data in the computation of the energy analysis model
            options.EnergyModelType = EnergyModelType.SpatialElement;   // Energy model based on rooms or spaces

            // Create a new energy analysis detailed model from the physical model
            var eadm = _trb.CreateAsync(() => EnergyAnalysisDetailModel.Create(_doc, options), "CreateModel").Result;
            IList<EnergyAnalysisSpace> spaces = eadm.GetAnalyticalSpaces();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Spaces: " + spaces.Count);
            foreach (EnergyAnalysisSpace space in spaces)
            {
                SpatialElement spatialElement = _doc.GetElement(space.CADObjectUniqueId) as SpatialElement;
                ElementId spatialElementId = spatialElement == null ? ElementId.InvalidElementId : spatialElement.Id;
                builder.AppendLine("   >>> " + space.SpaceName + " related to " + spatialElementId);
                IList<EnergyAnalysisSurface> surfaces = space.GetAnalyticalSurfaces();
                builder.AppendLine("       has " + surfaces.Count + " surfaces.");
                foreach (EnergyAnalysisSurface surface in surfaces)
                {
                    builder.AppendLine("            +++ Surface from " + surface.OriginatingElementDescription);
                }
            }
            TaskDialog.Show("EAM", builder.ToString());
        }

        private int TryGetId(EnergyAnalysisSurface surface)
        {
            //if (surface.SurfaceType != EnergyAnalysisSurfaceType.ExteriorWall &&
            //     surface.SurfaceType != EnergyAnalysisSurfaceType.InteriorWall)
            //{ return -1; }

            var description = surface.OriginatingElementDescription;

            var ind1 = description.IndexOf("[") + 1;
            var ind2 = description.IndexOf("]");
            var result = description.Substring(ind1, ind2 - ind1);
            var id = int.Parse(result);
            return id;

        }

        private Wall TryGetWall(EnergyAnalysisSurface surface)
        {
            var id = TryGetId(surface);
            if (id <= 0) { return null; }

            var wall = _doc.GetElement(new ElementId(id)) as Wall;

            return wall;
        }

        private Floor TryGetFloor(EnergyAnalysisSurface surface)
        {
            var id = TryGetId(surface);
            if (id <= 0) { return null; }

            var floor = _doc.GetElement(new ElementId(id)) as Floor;

            return floor;
        }

        private Face GetFace(Floor floor)
        {
            Solid solid = floor.Solid();

            foreach (Face face in solid.Faces)
            {
                var genIds = floor.GetGeneratingElementIds(face);
                genIds.ForEach(id => Debug.WriteLine(id.IntegerValue.ToString()));
                foreach (var id in genIds)
                {
                    var elem = _doc.GetElement(id);
                    if (elem is not Floor)
                    {
                        var s = elem as Sketch;
                        var plane = s.SketchPlane;
                        var profile = s.Profile;
                        CurveArray first = profile.get_Item(0);

                    }
                }
            }
            return null;
        }



        private IEnumerable<Opening> GetOpeningHolesInWall(EnergyAnalysisSurface analysisSurface, Solid surfaceSolid)
        {
            var wall = TryGetWall(analysisSurface);
            var wallInseretsIds = wall?.FindInserts(true, false, true, false) ?? new List<ElementId>();
            var inserts = wallInseretsIds.Select(i => _doc.GetElement(i)).ToList();
            var openingsHoles = inserts.OfType<Opening>();

            openingsHoles = openingsHoles.Where(o =>
            {
                var oSolid = o.TryGetSolid(_doc, _allLoadedLinks);
                var intersectionSolid = BooleanOperationsUtils
                            .ExecuteBooleanOperation(surfaceSolid, oSolid, BooleanOperationsType.Intersect);
                return intersectionSolid != null && Math.Abs(intersectionSolid.Volume) > 0;
            });
            return openingsHoles;
        }

        private IEnumerable<Opening> GetOpeningHolesInFloor(EnergyAnalysisSurface analysisSurface, Solid surfaceSolid)
        {
            var openingsHoles = new List<Opening>();

            var floor = TryGetFloor(analysisSurface);
            var floorInseretsIds = floor?.FindInserts(true, true, true, true) ?? new List<ElementId>();

            if (floor == null)
            { return openingsHoles; }

            var fg = floor.get_Geometry(new Options());
            var g0 = fg.First();
            var genIds = floor.GetGeneratingElementIds(g0);
            if (genIds.Count == 1 && genIds.First() == floor.Id)
            { return new List<Opening>(); }

            var classFilter = new ElementMulticlassFilter(new List<Type>() { typeof(Opening) });
            var collector = new FilteredElementCollector(_doc)
                .WherePasses(classFilter);
            var elements = collector.ToElements();
            openingsHoles = elements.OfType<Opening>().ToList();

            openingsHoles = openingsHoles.Where(o =>
            {
                var oSolid = TryGetSolid(o, floor);
                if(oSolid == null) { return false; }
                var geom = o.get_Geometry(new Options());
                var intersectionSolid = BooleanOperationsUtils
                            .ExecuteBooleanOperation(surfaceSolid, oSolid, BooleanOperationsType.Intersect);
                return intersectionSolid != null && Math.Abs(intersectionSolid.Volume) > 0;
            }).ToList();

            return openingsHoles;
        }


        private Solid TryGetSolid(Floor floor)
        {
            var floorSolid = floor.Solid();
            var genIds = floor.GetGeneratingElementIds(floorSolid);
            var id = genIds.First();
            if (genIds.Count == 1 && id == floor.Id) { return null; }

            var at = Rhino.RhinoMath.ToRadians(3);
            var zDir = XYZ.BasisZ;
            var faceArray = floorSolid.Faces;
            var faces = new List<Face>();
            foreach (Face face in faceArray)
            {
                var fd = face.ComputeNormal(new UV(0, 0));
                if (fd.ToVector3d().IsParallelTo(zDir.ToVector3d(), at) == 1)
                { faces.Add(face); }
            }
            
            var loops = new List<CurveLoop>();
            Face oFace = null;
            foreach (Face face in faces)
            {
                var faceGenIds = floor.GetGeneratingElementIds(face);
                var faceId = faceGenIds.First();
                if (faceGenIds.Count == 1 && faceId != floor.Id)
                {
                    var fLoops = face.GetEdgesAsCurveLoops();
                    loops.AddRange(fLoops);
                    oFace = face;
                }
            }

            double floorThickness = floor.get_Parameter(BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS).AsDouble();
            var surfaceSolid = GeometryCreationUtilities
                       .CreateExtrusionGeometry(loops, zDir.Negate(), floorThickness);

            return surfaceSolid;
        }

        private Solid TryGetSolid(Opening opening, Floor floor)
        {
            var points = opening.BoundaryRect;
            if(points == null)
            {
                points = new List<XYZ>();
                var curves =  opening.BoundaryCurves;
                foreach (Curve item in curves)
                {
                    var point = item.GetEndPoint(0);
                    points.Add(point);
                }
            }

            foreach (Parameter param in opening.ParametersMap)
            {
                var sValue = param.AsValueString();
                var dValue = param.AsDouble();
                var s = param.AsString();
            }
            CurveLoop loop = null;
            //var (minPoint, maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ>() { points.First(), points.Last() });
            //var rhinoPoints = new List<Rhino.Geometry.Point3d>() { minPoint.ToPoint3d(), maxPoint.ToPoint3d() };
            var rhinoPoints = points.Select(p => p.ToPoint3d()).ToList();
            var box = new Rhino.Geometry.Box(Rhino.Geometry.Plane.WorldXY, rhinoPoints);
            if(box.Volume < 0.001) { return null; }
            var oSolid = box.GetSolid();

            return oSolid;
        }

    }
}
