using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using OLMP.RevitAPI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using boundaryEdge = QuickGraph.TaggedEdge<OLMP.RevitAPI.Tools.Graphs.XYZVertex,
    DS.RevitCmd.EnergyTest.SpaceBoundary.BoundaryCurve>;
using OLMP.RevitAPI.Tools.Extensions;
using Autodesk.Revit.DB.Mechanical;
using DS.RevitCmd.EnergyTest;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Serilog;
using MoreLinq;
using DS.GraphUtils.Entities;
using QuickGraph;
using OLMP.RevitAPI.Tools.Graphs;
using OLMP.RevitAPI.Tools.Geometry.Points;
using QuickGraph.Algorithms;
using Serilog.Core;
using System.Diagnostics;

namespace DS.RevitCmd.EnergyTest.SpaceBoundary
{
    internal class BoundaryEdgeBuilderTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedLinks;
        private DocumentFilter _globalFilter;

        public BoundaryEdgeBuilderTest(UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            DocumentFilter globalFilter)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
            _globalFilter = globalFilter;
        }

        public ITransactionFactory TransactionFactory { get; set; }
        public ILogger Logger { get; set; }

        public IEnumerable<boundaryEdge> GetEdges()
        {
            var rooms = GetRooms();
            var spaceFactory = new SpaceFactory(_doc);
            var spaces = TransactionFactory
                .Create(() => CreateSpaces(rooms, spaceFactory), "CreateSpaces");
            var matchSpaces = spaces
                .Where(s => 
                //s.Room.Id.IntegerValue == 8911752
            s.Room.Number == "15"
            //&& s.Room.Name == "Коридор кладовых"
            );
            //matchSpaces.ForEach(s => Debug.WriteLine(s.Room.Name));
            var currentSpace = matchSpaces.First();
            var calculator = new SpatialElementGeometryCalculator(_doc, new SpatialElementBoundaryOptions());
            var result = calculator.CalculateSpatialElementGeometry(currentSpace);
            var spaceSolid = result.GetGeometry();

            var options = new SpatialElementBoundaryOptions()
            {
                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreCenter,
                //StoreFreeBoundaryFaces = true,
            };
            var boundarySegments = currentSpace.GetBoundarySegments(options).SelectMany(sl => sl);
            var boundaryCurves = boundarySegments.Select(s => s.GetCurve());
            boundaryCurves.ForEach(c => ShowCurve(c));
            Debug.WriteLine($"boundarySegments count is: {boundarySegments.Count()}");
            boundarySegments.ForEach(s => Debug.WriteLine(s.ElementId));
            return null;
            var boundaryElementIds = boundarySegments.Select(s => s.ElementId);
            var boundaryElements = boundaryElementIds
                .Select(id => _doc.GetElement(id))
                .Where(e => e is not null)
                .DistinctBy(e => e.Id);

            var solidItersectionFactory = GetIntersectionFactory(_globalFilter, boundaryElementIds);
            var validSegments = boundarySegments.Where(e => e is not null && e.ElementId.IntegerValue > 0);
            var edgeBuilder = new ElementEdgeFactory();
            var elementIntersectionFactory = new ElemIntersectionFactory(_doc, solidItersectionFactory);
            var graph = new BoundaryGraphFactory(_doc,
                edgeBuilder, validSegments,
                boundaryCurves, elementIntersectionFactory, spaceSolid)
            { Logger = Logger }
            .Create();
            //foreach (var be in boundaryElements)
            //{
            //    var bEdges = edgeBuilder.Create(be);
            //    graph.AddVerticesAndEdgeRange(bEdges);
            //}
            if (Logger != null)
            { PrintEdges(graph.Edges, Logger); }
            Show(graph);

            Logger?.Information($"Vertices count is: {graph.VertexCount}");
            Logger.Information($"Edges count is: {graph.Edges.Count()}");
            var sinks = graph.Sinks();
            Logger?.Information($"Sinks count is: {sinks.Count()}");
            var roots = graph.Roots();
            Logger?.Information($"Roots count is: {roots.Count()}");

            return graph.Edges;
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


        private void PrintEdges(IEnumerable<boundaryEdge> edges, ILogger logger)
        {
            int i = 0;
            foreach (var edge in edges)
            {
                i++;
                logger.Information($"Edge {i}: {edge.Tag}\n " +
                    $"v{edge.Source}: {edge.Source.Tag.RoundVector()} -> " +
                    $"v{edge.Target}: {edge.Target.Tag.RoundVector()})");
            }
        }

        #region Show

        private void ShowPoint(XYZ point)
         => TransactionFactory.Create(() => point.Show(_doc), "ShowPoint");

        private void ShowCurve(Curve curve)
      => TransactionFactory.Create(() => curve.Show(_doc), "ShowPoint");

        private void ShowVertex(TaggedVertex<XYZ> vertex)
            => ShowPoint(vertex.Tag);

        private void ShowEdge(boundaryEdge edge)
            => TransactionFactory.Create(() => edge.Tag.Curve.Show(_doc), "ShowEdge");

        private void Show(IVertexAndEdgeListGraph<XYZVertex, boundaryEdge> graph)
        {
            TransactionFactory.Create(() => Show(graph), "ShowVertices");


            void Show(IVertexAndEdgeListGraph<XYZVertex, boundaryEdge> graph)
            {
                var moveVector = new XYZ(0.5, 0.5, 0);
                var view = GetUIView(_doc);
                var xYZVisulalizator = new XYZVisualizator(new UIDocument(_doc));
                foreach (var vertex in graph.Vertices)
                {
                    foreach (var edge in graph.OutEdges(vertex))
                    {

                        var v1 = edge.Source;
                        var v2 = edge.Target;

                        var xyz1 = v1.GetLocation(_doc);
                        var xyz2 = v2.GetLocation(_doc);
                        var center = edge.Tag.Curve.GetCenter();

                        ElementId defaultTypeId = _doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);

                        //v1.Tag.Show(_doc);
                        TextNote.Create(_doc, view.ViewId, xyz1 + moveVector, v1.Id.ToString(), defaultTypeId);

                        //v2.Tag.Show(_doc);
                        TextNote.Create(_doc, view.ViewId, xyz2 + moveVector, v2.Id.ToString(), defaultTypeId);

                        xYZVisulalizator.ShowVectorWithoutTransaction(xyz1, xyz2);
                        //edge.Tag.Curve.Show(_doc);
                        //TextNote.Create(_doc, view.ViewId, center + moveVector, edge.Tag.ElementId.ToString(), defaultTypeId);
                    }
                }
                UIView GetUIView(Document doc)
                {
                    var uidoc = new UIDocument(doc);
                    var view = uidoc.ActiveGraphicalView;
                    UIView uiview = null;
                    var uiviews = uidoc.GetOpenUIViews();
                    foreach (UIView uv in uiviews)
                    {
                        if (uv.ViewId.Equals(view.Id))
                        {
                            uiview = uv;
                            break;
                        }
                    }
                    return uiview;
                }
            }
        }

        #endregion

    }
}
