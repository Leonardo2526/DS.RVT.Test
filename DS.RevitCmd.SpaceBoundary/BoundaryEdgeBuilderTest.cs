using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using OLMP.RevitAPI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using boundaryEdge = QuickGraph.TaggedEdge<
    DS.GraphUtils.Entities.TaggedVertex<Autodesk.Revit.DB.XYZ>,
    DS.RevitCmd.SpaceBoundary.BoundaryCurve>;
using OLMP.RevitAPI.Tools.Extensions;
using Autodesk.Revit.DB.Mechanical;
using DS.RevitApp.Test.Energy;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Serilog;
using MoreLinq;

namespace DS.RevitCmd.SpaceBoundary
{
    internal class BoundaryEdgeBuilderTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedLinks;
        private DocumentFilter _globalFilter;

        public BoundaryEdgeBuilderTest(UIDocument uiDoc, IEnumerable<RevitLinkInstance> allLoadedLinks)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
            _globalFilter = GetFilter(_doc, _allLoadedLinks);
        }

        public ITransactionFactory TransactionFactory { get; set; }
        public ILogger Logger { get; set; }

        public IEnumerable<boundaryEdge> GetEdges()
        {
            var rooms = GetRooms();
            var spaceFactory = new SpaceFactory(_doc);
            var spaces = TransactionFactory
                .Create(() => CreateSpaces(rooms, spaceFactory), "CreateSpaces");
            var currentSpace = spaces.First(s => s.Room.Number == "15");
            var options = new SpatialElementBoundaryOptions();
            var boundarySegments = currentSpace.GetBoundarySegments(options);
            var boundaryCurves = boundarySegments.SelectMany(sl => sl.Select(s => s.GetCurve()));
            var boundaryElementIds = boundarySegments.SelectMany(sl => sl.Select(s => s.ElementId));
            var boundaryElements = boundaryElementIds.Select(id => _doc.GetElement(id));
            _globalFilter = GetFilter(_doc, _allLoadedLinks);

            var elementItersectionFactory = GetIntersectionFactory(_globalFilter, boundaryElementIds);
            var builder = new BoundaryEdgeBuilder(_doc, elementItersectionFactory)
            {
                Logger = Logger,
                TransactionFactory = TransactionFactory
            };

            var edges = builder.Create(boundaryElements
                .First());
                //.First(b => b is Wall wall && wall.GetLocationCurve() is Arc));
            edges.ForEach(e => ShowEdge(e));

            if (Logger != null)
            { PrintEdges(edges, Logger); }

            return edges;
        }

        private IEnumerable<Room> GetRooms()
        {
            var roomDocFilter = _globalFilter.Clone();
            roomDocFilter.SlowFilters ??= new();
            roomDocFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = roomDocFilter.ApplyToAllDocs()
                .SelectMany(kv => kv.Value.ToElements(kv.Key))
                .OfType<Room>()
            .Where(r => r.Area > 0);
            //if (Logger != null)
            //{
            //    rooms.ForEach(r => Logger.Information(r.Name, r.Id));
            //}
            return rooms;
        }

        private IEnumerable<Space> CreateSpaces(IEnumerable<Room> rooms, ISpaceFactory spaceFactory)
        {
            var spaces = new List<Space>();
            foreach (var room in rooms)
            {
                var space = spaceFactory.Create(room);
                spaces.Add(space);
            }
            return spaces;
        }

        private SolidElementIntersectionFactory GetIntersectionFactory(DocumentFilter globalFilter,
            IEnumerable<ElementId> elementIdsSet)
        {
            var localFilter = globalFilter.Clone();
            localFilter.ElementIdsSet = elementIdsSet.ToList();
            var types = new List<Type>()
            {
                typeof(Wall)
            };
            var multiclassFilter = new ElementMulticlassFilter(types);
            localFilter.QuickFilters.Add((multiclassFilter, null));

            return new SolidElementIntersectionFactory(_doc, _allLoadedLinks, localFilter)
            {
                Logger = null,
                TransactionFactory = null
            };
        }

        private DocumentFilter GetFilter(Document activeDoc,
            IEnumerable<RevitLinkInstance> allLoadedLinks)
        {
            //create global filter
            var docs = new List<Document>() { activeDoc };
            var excludedCategories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_GenericAnnotation,
            BuiltInCategory.OST_TelephoneDevices,
            BuiltInCategory.OST_Materials,
            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_Massing
        };
            var globalFilter = new DocumentFilter(docs, activeDoc, allLoadedLinks);
            globalFilter.QuickFilters =
            [
                (new ElementMulticategoryFilter(excludedCategories, true), null),
                (new ElementIsElementTypeFilter(true), null),
            ];
            return globalFilter;
        }

        private void ShowEdge(boundaryEdge edge)
            => TransactionFactory.Create(() => edge.Tag.Curve.Show(_doc), "ShowEdge");


        private void PrintEdges(IEnumerable<boundaryEdge> edges, ILogger logger)
        {
            logger.Information("Total edges created: " + edges.Count());
            int i = 0;
            foreach (var edge in edges)
            {
                i++;
                logger.Information($"Edge {i}: {edge.Tag}\n " +
                    $"v{edge.Source}: {edge.Source.Tag.RoundVector()} -> " +
                    $"v{edge.Target}: {edge.Target.Tag.RoundVector()})");
            }
        }
    }
}
