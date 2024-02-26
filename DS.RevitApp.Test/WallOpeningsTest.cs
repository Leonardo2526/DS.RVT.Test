using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
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
using static MongoDB.Driver.WriteConcern;

namespace DS.RevitApp.Test
{
    internal class WallOpeningsTest : ISerilogged
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


        public WallOpeningsTest(UIDocument uiDoc)
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

        public void ShowShape()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var element = _doc.GetElement(reference);

            if (element is not Wall wall)
            { return; }
            var solid = wall.Solid(_allLoadedLinks);
            _trb.CreateAsync(() => solid.ShowShape(_doc), "ShowSurfaceSolid");
        }

        public void Run()
        {
            var rooms = GetRooms();


            var spaceDocFilter = _globalFilter.Clone();
            var existSpaces = spaceDocFilter.ApplyToAllDocs()
                .SelectMany(kv => kv.Value.ToElements(kv.Key)).OfType<EnergyAnalysisSpace>();

            var settings = EnergyDataSettings.GetFromDocument(_doc);
            _trb.CreateAsync(() =>
            {
                settings.AnalysisType = AnalysisMode.BuildingElements;
                settings.EnergyModel = true;
                settings.DividePerimeter = false;
                settings.CoreOffset = 100; 
                settings.SetCreateAnalyticalModel(false);

            }, 
            "SetSettings").Wait();

            var p1Name = "Analytical Space Resolution";
            var p1 = settings.GetParameters(p1Name).First();
            var p2Name = "Analytical Surface Resolution";
            var p2 = settings.GetParameters(p2Name).First();
            _trb.CreateAsync(() =>
            {
                p1.Set(0.5);
                p2.Set(0.5);

            },
             "SetSettings").Wait();

            //var room1 = rooms.FirstOrDefault();
            //var model = room1.GetAnalyticalModel();
            var opt = new EnergyAnalysisDetailModelOptions()
            {
                ExportMullions = true,
                IncludeShadingSurfaces = true,
                Tier = EnergyAnalysisDetailModelTier.Final,
                SimplifyCurtainSystems = false
            };

            //var m = EnergyAnalysisDetailModel.GetMainEnergyAnalysisDetailModel(_doc);           
            var eModel = _trb.CreateAsync(() => EnergyAnalysisDetailModel.Create(_doc, opt), "CreateModel").Result;
            var spaces = eModel.GetAnalyticalSpaces();
            var modelOpenings = eModel.GetAnalyticalOpenings();
            var modelSurfaces = eModel.GetAnalyticalSurfaces();


            var s = GetStringValues(settings.Parameters);
            Logger?.Information(s);
            //return;


            var exludeTypes = new List<EnergyAnalysisSurfaceType>()
            {
                //EnergyAnalysisSurfaceType.Air,
                // EnergyAnalysisSurfaceType.Shading,
                //  EnergyAnalysisSurfaceType.Underground
            };
            double extrusionValue = 0.001;
            foreach (var space in spaces)
            {
                ShowSpace(space);
                var surfaces = space.GetAnalyticalSurfaces()
                    .Where(surface => !exludeTypes.Contains(surface.SurfaceType));
                foreach (var surface in surfaces)
                {
                    var surfaceSolid = GetSolid(extrusionValue, surface);

                    var openingsSolids = GetOpeningsSolids(surface, surfaceSolid);
                    var windowsAndDoorsSolids = GetWindowAndDoorsSolids(extrusionValue, surface);
                    _trb.CreateAsync(() => windowsAndDoorsSolids.ForEach(s => s.ShowShape(_doc)), "ShowOpenings");

                    var dedutibleSolids = new List<Solid>();
                    dedutibleSolids.AddRange(openingsSolids);
                    dedutibleSolids.AddRange(windowsAndDoorsSolids);

                    var resultSolid = Substract(surfaceSolid, dedutibleSolids);
                    _trb.CreateAsync(() => surfaceSolid.ShowShape(_doc), "ShowSurfaceSolid");
                    //var mainFace = GetMainFace(surfaceSolid);
                    //Logger?.Information($"Main face area is: {mainFace.Area}");
                }
            }
        }

        private List<Solid> GetWindowAndDoorsSolids(double extrusionValue, EnergyAnalysisSurface surface)
        {
            var oSolids = new List<Solid>();

            var polyLoop = surface.GetPolyloop();
            var openings = surface.GetAnalyticalOpenings();
            foreach (var opening in openings)
            {
                var oPolyLoop = opening.GetPolyloop();
                var oLoop = ToLoop(oPolyLoop);
                var oPolyLoops = new List<CurveLoop>() { oLoop };
                var oSolid = GeometryCreationUtilities
               .CreateExtrusionGeometry(oPolyLoops, polyLoop.Direction, extrusionValue);
                oSolids.Add(oSolid);
            }

            return oSolids;
        }

        private IEnumerable<Solid> GetOpeningsSolids(EnergyAnalysisSurface surface, Solid surfaceSolid)
        {
            var oSolids = new List<Solid>();

            var inseretsIds = new List<ElementId>();
            if (TryGetOriginateElement(surface) is not Wall wall) { return oSolids; }


            var solidExtractor = new GetOpeningsSolidTest(_uiDoc);
            var wallInseretsIds = wall.FindInserts(true, false, false, false) ?? new List<ElementId>();
            inseretsIds.AddRange(wallInseretsIds);

            var inserts = inseretsIds.Select(i => _doc.GetElement(i)).ToList();
            var openings = inserts.OfType<Opening>();
            foreach (var opening in openings)
            {
                var oSolid = solidExtractor.GetSolid(opening, wall);
                //var geom = opening.get_Geometry(new Options());
                //var oSolid = opening.TryGetSolid(_doc, _allLoadedLinks);
                oSolids.Add(oSolid);
            }

            return oSolids;
        }

        private Solid GetSolid(double extrusionValue, EnergyAnalysisSurface surface)
        {
            var polyLoop = surface.GetPolyloop();
            var surfaceLoop = ToLoop(polyLoop);
            var surfaceLoops = new List<CurveLoop>() { surfaceLoop };
            return GeometryCreationUtilities
                .CreateExtrusionGeometry(surfaceLoops, polyLoop.Direction, extrusionValue);
        }

        private Solid Substract(Solid sourceSolid, IEnumerable<Solid> deductibleSolids)
        {
            var resultSolid = BooleanOperationsUtils
                .ExecuteBooleanOperation(sourceSolid, sourceSolid, BooleanOperationsType.Union);
            foreach (var oSolid in deductibleSolids)
            {
                resultSolid = BooleanOperationsUtils
                    .ExecuteBooleanOperation(resultSolid, oSolid, BooleanOperationsType.Difference);
            }

            return resultSolid;
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


        private ElementId TryGetOriginatingId(EnergyAnalysisSurface surface)
        {
            var description = surface.OriginatingElementDescription;

            var ind1 = description.IndexOf("[") + 1;
            var ind2 = description.IndexOf("]");
            var result = description.Substring(ind1, ind2 - ind1);
            var id = int.Parse(result);
            return id >= 0 ? new ElementId(id) : null;

        }

        private Element TryGetOriginateElement(EnergyAnalysisSurface surface)
            => _doc.GetElement(TryGetOriginatingId(surface));


        private Face GetMainFace(Solid solid)
        {
            var faces = new List<Face>();
            foreach (Face face in solid.Faces)
            {
                faces.Add(face);
            }

            return faces.OrderByDescending(f => f.Area).FirstOrDefault();
            //foreach (Face face in solid.Faces)
            //{
            //    face.ComputeNormal()
            //}
        }

        private IEnumerable<Face> GetMainFaces(Solid solid)
        {
            var faces = new List<Face>();
            foreach (Face face in solid.Faces)
            {
                faces.Add(face);
            }

            return faces.OrderByDescending(f => f.Area);
            //foreach (Face face in solid.Faces)
            //{
            //    face.ComputeNormal()
            //}
        }


        private string GetStringValues(ParameterSet parameters)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            int i = 0;
            foreach (Parameter parameter in parameters)
            {
                i++;
                string value = null;
                switch (parameter.StorageType)
                {
                    case StorageType.None:
                        break;
                    case StorageType.Integer:
                        value = parameter.AsInteger().ToString();
                        break;
                    case StorageType.Double:
                        value = parameter.AsDouble().ToString();
                        break;
                    case StorageType.String:
                        value = parameter.AsValueString();
                        break;
                    case StorageType.ElementId:
                        break;
                    default:
                        break;
                }
                sb.AppendLine($"Parameter {i} '{parameter.Definition.Name}': {value}.");
            }

            return sb.ToString();
        }

               
        private void ShowSpace(EnergyAnalysisSpace space) => 
            _trb.CreateAsync(() => ShowPolyLoop(space.GetBoundary().First(), 26.316), "ShowSpaces");
    }
}
