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

        public void Run()
        {
            var rooms = GetRooms();
            //var spaceDocFilter = _globalFilter.Clone();

            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);

            var spaces = new List<Space>();
            foreach (Room room in rooms)
            {
                var space = CreateSpace(room, plane);
                spaces.Add(space);
            }

            var options = new SpatialElementBoundaryOptions();
            foreach (var space in spaces)
            {
                var segments = space.GetBoundarySegments(options);
                var curves = ToCurves(segments);
                _trb.CreateAsync(() => curves.ForEach(c => c.Show(_doc)), "showCurve").Wait();
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
                space.UpperLimit = room.UpperLimit;
                space.LimitOffset = room.LimitOffset;
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
            }

            return curves;
        }
    }
}
