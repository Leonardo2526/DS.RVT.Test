using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitCmd.EnergyTest;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitCmd.EnergyTest
{
    internal class EnergyModelBuilderTest : ISerilogged
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly List<RevitLinkInstance> _allLoadedLinks;
        private readonly List<Document> _allFilteredDocs;
        private readonly DocumentFilter _globalFilter;
        private readonly ITransactionFactory _trf;       

        public EnergyModelBuilderTest(UIDocument uiDoc, DocumentFilter globalFilter)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _allLoadedLinks = _doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
            _allFilteredDocs = new List<Document>() { _doc };
            _allFilteredDocs.AddRange(_allLoadedLinks.Select(l => l.GetLinkDocument()));
            _globalFilter = globalFilter;
            _trf = new ContextTransactionFactory(_doc);
        }

        public ILogger Logger { get; set; }

        public IEnumerable<Room> GetRooms()
        {
            var roomDocFilter = _globalFilter.Clone();
            roomDocFilter.SlowFilters ??= new();
            roomDocFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = roomDocFilter.ApplyToAllDocs()
                .SelectMany(kv => kv.Value.ToElements(kv.Key))
                .OfType<Room>()
            .Where(r => r.Area > 0)
            .Where(r => r.Number == "18")
            ;
            //if (Logger != null)
            //{
            //    rooms.ForEach(r => Logger.Information(r.Name, r.Id));
            //}
            return rooms;
        }

        public void GetModels()
        {
            var rooms = GetRooms();

            var spaceFactory = new SpaceFactory(_doc);
            EnergySurfaceFactory energySurfaceFactory = new(_doc, _allLoadedLinks);
            var energyModelFactory =
               new BestEnergyModelFactory(_doc, _allLoadedLinks, energySurfaceFactory, _globalFilter)
            { TransactionFactory = _trf };
            var modelProcessor = new EnergyModelProcessor(_doc, _allLoadedLinks, _trf, spaceFactory, energyModelFactory)
            { Logger = Logger };
            var eModels = modelProcessor.Create(rooms);
            //return;

            var surfaces = eModels.SelectMany(m => m.EnergySurfaces);
            //return;
            _trf.Create(() => surfaces.ForEach(s => s.Show(_doc)), "ShowSpace");
            ShowInsertSurfaces(eModels);
            //ShowModels(eModels);
            //_trf.Create(() => eModels.ForEach(model => model.Show(_doc)), "ShowSpace");
            return;

            void ShowInsertSurfaces(IEnumerable<EnergyModel> energyModels)
            {
                var insertSurfaces = new List<EnergySurface>();
                foreach (var model in energyModels)
                {
                    var modelInsertSurfaces = model.EnergySurfaces.SelectMany(s => s.Inserts);
                    insertSurfaces.AddRange(modelInsertSurfaces);
                }

                _trf.Create(() => insertSurfaces.ForEach(s => s.Show(_doc)), "ShowSpace");
            }
        }

        public void CreateGraph()
        {
            var rooms = GetRooms();

            var spaceFactory = new SpaceFactory(_doc);
            //var spaces = _trf.Create(() => CreateSpaces(rooms), "CreateSpaces");
            //Logger?.Information($"Spaces created: {spaces.Count()}");
            //spaces.ForEach(s => Show(_doc, s));
            //_uiDoc.RefreshActiveView();
            //return;
            EnergySurfaceFactory energySurfaceFactory = new(_doc, _allLoadedLinks);
            var energyModelFactory = 
                new BestEnergyModelFactory(_doc, _allLoadedLinks, energySurfaceFactory, _globalFilter);
            //var energyModelFactory = new EnergyModelFactory(_doc, _allLoadedLinks, energySurfaceFactory);
            var modelProcessor = new EnergyModelProcessor(_doc, _allLoadedLinks, _trf, spaceFactory, energyModelFactory)
            { Logger = Logger };
            var eModels = modelProcessor.Create(rooms);
            //Logger?.Information($"Energy models created: {eModels.Count()}");

            var graphFactory = new EnergyGraphFactory(_doc, _allLoadedLinks, eModels, energyModelFactory)
            { Logger = Logger };
            var graph = graphFactory.CreateGraph();

            PrintGraph(graph, Logger);
            PrintGraphViz(graphFactory.Graph, Logger);   
            ShowVertices(graphFactory.Graph);
            ShowEdges(graphFactory.Graph);

            var energyVertices = graph.Vertices.Where(v => v.Tag != null);
            var v1 = energyVertices.ElementAt(0);
            PrintEdges(graphFactory.Graph, Logger, v1);
            var v2 = energyVertices.ElementAt(1);
            PrintEdges(graphFactory.Graph, Logger, v2);

            IEnumerable<Space> CreateSpaces(IEnumerable<Room> rooms)
            {
                var spaces = new List<Space>();
                foreach (var room in rooms)
                {
                    var space = spaceFactory.Create(room);
                    spaces.Add(space);
                }
                return spaces;
            }
        }

        private void PrintGraph(IVertexAndEdgeListGraph<EnergyVertex, TaggedEdge<EnergyVertex, EnergySurface>> graph, ILogger logger)
        {
            var energyVertices = graph.Vertices.Where(v => v.Tag != null);
            logger.Information($"Energy vertices count is : {energyVertices.Count()}");
            var airVertices = graph.Vertices.Where(v => v.Tag == null);
            logger.Information($"Air vertices count is : {airVertices.Count()}");
        }

        private void PrintGraphViz(BidirectionalGraph<EnergyVertex, TaggedEdge<EnergyVertex, EnergySurface>> graph, ILogger logger)
        {
            string dotGraph = graph.ToGraphviz(algorithm =>
            {
                // Custom init example
                algorithm.CommonVertexFormat.Shape = GraphvizVertexShape.Diamond;
                algorithm.CommonEdgeFormat.ToolTip = "Edge tooltip";
                algorithm.FormatVertex += (sender, args) =>
                {
                    if (args.Vertex.Tag != null)
                    {args.VertexFormatter.Label = $"Vertex {args.Vertex}, Room name: {args.Vertex.Tag.Space.Room.Name}";}
                    else
                    { args.VertexFormatter.Label = $"Vertex {args.Vertex}, Air"; }
                };
            });

            logger.Information(dotGraph);
        }

        private void PrintEdges(BidirectionalGraph<EnergyVertex, TaggedEdge<EnergyVertex, EnergySurface>> graph, ILogger logger, EnergyVertex vertex)
        {
            graph.TryGetInEdges(vertex, out var inEdges);
            graph.TryGetOutEdges(vertex, out var outEdges);

            var taggedEdges = new List<TaggedEdge<EnergyVertex, EnergySurface>>();
            var inTagged = inEdges.OfType<TaggedEdge<EnergyVertex, EnergySurface>>().ToList();
            var outTagged = outEdges.OfType<TaggedEdge<EnergyVertex, EnergySurface>>().ToList();
            taggedEdges.AddRange(inTagged);
            taggedEdges.AddRange(outTagged);

            var interiorEdges = taggedEdges
                .Where(e => e.Tag.SurfaceType 
                == Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.InteriorWall);
            logger.Information($"Vertex {vertex.Id} interior edges count is: {interiorEdges.Count()}");
            interiorEdges.ForEach(e => logger.Information(e.Tag.Host.Id.IntegerValue.ToString()));
            var exteriorEdges = taggedEdges
                .Where(e => e.Tag.SurfaceType
                == Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.ExteriorWall);
            logger.Information($"Vertex {vertex.Id} exterior edges count is: {exteriorEdges.Count()}");
            exteriorEdges.ForEach(e => logger.Information(e.Tag.Host.Id.IntegerValue.ToString()));
        }

        public void ShowVertices(BidirectionalGraph<EnergyVertex, TaggedEdge<EnergyVertex, EnergySurface>> graph)
        {
            foreach (var v in graph.Vertices.OfType<EnergyVertex>())
            {
                if (v.Tag == null) continue;
                _trf.Create(() => v.Tag.Show(_doc), "ShowSpace");
                _uiDoc.RefreshActiveView();
            }
        }

        public void ShowEdges(BidirectionalGraph<EnergyVertex, TaggedEdge<EnergyVertex, EnergySurface>> graph)
        {
            foreach (var e in graph.Edges)
            {
                _trf.Create(() => e.Tag.Show(_doc), "ShowSpace");
                _uiDoc.RefreshActiveView();
            }
        }

        private void Show(Document activeDoc, Space space)
        {
            var options = new SpatialElementBoundaryOptions();
            var calculator = new SpatialElementGeometryCalculator(activeDoc, options);
            var result = calculator.CalculateSpatialElementGeometry(space);
            var spaceSolid = result.GetGeometry();
            _trf.Create(() => spaceSolid.ShowShape(activeDoc), "CreateSpaces");
        }

        private void ShowModels(IEnumerable<EnergyModel> eModels)
            => _trf.Create(() => eModels.ForEach(model => model.Show(_doc)), "ShowSpace");

    }
}
