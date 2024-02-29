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
using System.Security.Cryptography;

namespace DS.RevitApp.Test
{
    internal class GetSpacesTest : ISerilogged
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


        public GetSpacesTest(UIDocument uiDoc)
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



        public void SelectWall()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var element = _doc.GetElement(reference);

            if (element is not Wall wall)
            { return; }

            var result = GetStringValues(wall.Parameters);
            Logger?.Information(result);
        }

        public void Run()
        {

            var rooms = GetRooms();
            //var spaceDocFilter = _globalFilter.Clone();

            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);

            var spaces = new List<Space>();
            foreach (Room room in rooms)
            {
                if (room.Location == null
                    //|| room.Number != "1"
                    //|| room.Number == "3"
                    ) { continue; }
                var space = CreateSpace(room, plane);
                spaces.Add(space);
            }

            var options = new SpatialElementBoundaryOptions();
            foreach (var space in spaces)
            {
                var segments = space.GetBoundarySegments(options);
                var curves = ToCurves(segments);
                _trb.CreateAsync(() => curves.ForEach(c => c.Show(_doc)), "showCurve").Wait();

                var offsetted = new List<(BoundarySegment, Curve)>();
                foreach (var sl in segments)
                {
                    foreach (var s in sl)
                    {
                        var curve = Offset(s);
                        if (curve != null)
                        { offsetted.Add((s, curve)); }
                    }
                }

                var connected = ConnectCurves(offsetted);
                _trb.CreateAsync(() => connected.ForEach(c => c.Item2.Show(_doc)), "showCurve").Wait();

                var loops = GetLoops(connected);
                var solids = GetSolids(loops);
                _trb.CreateAsync(() => solids.ForEach(s => s.ShowShape(_doc)), "ShowSolids").Wait();
            }

        }

        private Space CreateSpace(Room room, Plane basePlane)
        {
            var lp = room.Location as LocationPoint;
            var roomLocationPoint = lp.Point;

            basePlane.Project(roomLocationPoint, out var uv, out var d1);
            return _trb.CreateAsync(() =>
            {

                var space = _doc.Create.NewSpace(room.Level, uv);
                if (room.Level.Id != room.UpperLimit.Id)
                { 
                    space.UpperLimit = room.UpperLimit; 
                    space.LimitOffset = room.LimitOffset; 
                }
                return space;
            }, "CreateSpace").Result;
        }

        private IEnumerable<Curve> ToCurves(IList<IList<BoundarySegment>> segmentsList)
        {
            var curves = new List<Curve>();
            foreach (var sl in segmentsList)
            {
                var sCurves = sl.Select(s => s.GetCurve());
                curves.AddRange(sCurves);
                //curves.ForEach(ShowCurve);
            }

            return curves;
        }

        private void ShowCurve(Curve curve)
        {
            _trb.CreateAsync(() => curve.Show(_doc), "showCurve").Wait();
            _uiDoc.RefreshActiveView();
        }

        private Curve Offset(BoundarySegment segment, IList<BoundarySegment> segmentsList = null)
        {
            var elId = segment.ElementId;
            var wall = _doc.GetElement(elId) as Wall;
            if (wall == null)
            { return null; }

            var curve = segment.GetCurve();
            var distanseToOffset = wall.Width / 2;
            var dir = GetDirection(curve);
            var referenceVector = XYZ.BasisZ;
            //var referenceVector = dir.CrossProduct(XYZ.BasisZ);

            var offsetCurve = curve.CreateOffset(distanseToOffset, referenceVector);
            if (offsetCurve == null)
            { return null; }
            return offsetCurve;

            XYZ GetDirection(Curve curve1, Curve curve2 = null)
            {
                var p1 = curve1.GetEndPoint(0);
                var p2 = curve1.GetEndPoint(1);

                return (p2 - p1).Normalize();
            }
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

        private IEnumerable<(BoundarySegment, Curve)> ConnectCurves(IEnumerable<(BoundarySegment, Curve)> segementCurves)
        {
            var lastCurve = segementCurves.LastOrDefault();
            var connectedsCurves = new List<(BoundarySegment, Curve)>()
            {
                lastCurve
            };

            for (int i = 0; i < segementCurves.Count() - 1; i++)
            {
                var segmentCurve1 = segementCurves.ElementAt(i);
                var result = FitToIntersection(segmentCurve1.Item2, connectedsCurves.Last().Item2, true);

                var curve2 = segementCurves.ElementAt(i + 1);
                result = FitToIntersection(result, curve2.Item2, false);
                connectedsCurves.Add((segmentCurve1.Item1, result));
            }

            connectedsCurves.RemoveAt(0);

            var lastResult = FitToIntersection(lastCurve.Item2, connectedsCurves.First().Item2, false);
            lastResult = FitToIntersection(lastResult, connectedsCurves.Last().Item2, true);
            connectedsCurves.Add((lastCurve.Item1, lastResult));

            return connectedsCurves;

            Curve FitToIntersection(Curve curve1, Curve curve2, bool fromStart)
            {
                var p11 = curve1.GetEndPoint(0);
                var p12 = curve1.GetEndPoint(1);
                var p21 = curve2.GetEndPoint(0);
                var p22 = curve2.GetEndPoint(1);

                var cloned1 = curve1.Clone();
                var cloned2 = curve2.Clone();
                cloned1.MakeUnbound();
                cloned2.MakeUnbound();
                var intersection = cloned1.Intersect(cloned2, out var resultArray);
                if (intersection == SetComparisonResult.Overlap)
                {
                    var interectionResult = resultArray.get_Item(0);
                    var interectionPoint = interectionResult.XYZPoint;

                    var result = cloned1.Project(interectionPoint);

                    var param11 = fromStart ? result.Parameter : curve1.GetEndParameter(0);
                    var param12 = fromStart ? curve1.GetEndParameter(1) : result.Parameter;
                    cloned1.MakeBound(param11, param12);
                    return cloned1;
                }
                else if (intersection == SetComparisonResult.Equal)
                {
                    return curve1;
                }

                return null;
            }
        }

        private IEnumerable<(BoundarySegment, CurveLoop)> GetLoops(IEnumerable<(BoundarySegment, Curve)> segementCurves)
        {
            var loops = new List<(BoundarySegment, CurveLoop)>();

            var distanseToOffset = 0.01;
            var referenceVector = XYZ.BasisZ;

            foreach (var curveSegms in segementCurves)
            {
                var loop = CreateLoop(curveSegms.Item2);
                var isClosed = !loop.IsOpen();
                loops.Add((curveSegms.Item1, loop));
            }

            return loops;

            CurveLoop CreateLoop(Curve curve)
            {
                var dir = GetDirection(curve);

                var offsetCurve = curve.CreateOffset(distanseToOffset, referenceVector).CreateReversed();

                var p1 = curve.GetEndPoint(0);
                var p2 = curve.GetEndPoint(1);
                var p3 = offsetCurve.GetEndPoint(0);
                var p4 = offsetCurve.GetEndPoint(1);



                var line1 = Line.CreateBound(p4, p1);
                var line2 = Line.CreateBound(p2, p3);
                //return CurveLoop.Create(new List<Curve>() { curve, offsetCurve});
                return CurveLoop.Create(new List<Curve>() { curve, line2, offsetCurve, line1 });
            }

            XYZ GetDirection(Curve curve1)
            {
                var p1 = curve1.GetEndPoint(0);
                var p2 = curve1.GetEndPoint(1);

                return (p2 - p1).Normalize();
            }
        }

        private IEnumerable<Solid> GetSolids(IEnumerable<(BoundarySegment, CurveLoop)> segementCurve)
        {
            var solids = new List<Solid>();

            foreach (var item in segementCurve)
            {
                var id = item.Item1.ElementId;
                if(id.IntegerValue > 0 && _doc.GetElement(id) is Wall wall)
                {
                    //var heigth = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsDouble();
                    var heigth = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                   var solid = GeometryCreationUtilities
                 .CreateExtrusionGeometry(new List<CurveLoop> { item.Item2}, XYZ.BasisZ, heigth);
                    solids.Add(solid);
                }
            }

            return solids;
        }


        public void TestCurve()
        {
            var p11 = new XYZ();
            var p12 = new XYZ(5, 0, 0);
            var dir1 = (p12 - p11).Normalize();
            Curve curve1 = Line.CreateUnbound(p11, dir1);
            //Curve curve1 = Line.CreateBound(p11, p12);

            var p21 = new XYZ(3, 3, 0);
            var p22 = new XYZ(3, 1, 0);
            var dir2 = (p21 - p22).Normalize();
            Curve curve2 = Line.CreateUnbound(p21, dir2);
            //Curve curve2 = Line.CreateBound(p21, p22);

            var intersection = curve1.Intersect(curve2, out var resultArray);
        }
    }
}
