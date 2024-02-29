using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using MoreLinq;
using OLMP.RevitAPI.Tools.Extensions;
using QuickGraph;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace DS.RevitApp.Test.Energy
{
    internal class EnergyGraphFactory :
        IGraphFactory<EnergyVertex>, ISerilogged
    {
        private readonly static double _minIntersectionVolume =
           UnitConvertionExtenstions.MMToFeet(1, 3);

        private readonly Document _activeDoc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly IEnumerable<Space> _spaces;
        private readonly IEnergyModelFactory _energyModelFactory;
        private Stack<Space> _openSpaces = new();
        private Stack<EnergyModel> _openModels = new();
        private List<ElementId> _close = new();


        public EnergyGraphFactory(
            Document activeDoc, IEnumerable<RevitLinkInstance> links,
            IEnumerable<Space> spaces,
            IEnergyModelFactory energyModelFactory)
        {
            _activeDoc = activeDoc;
            _links = links;
            _spaces = spaces;
            spaces.ForEach(_openSpaces.Push);
            _energyModelFactory = energyModelFactory;
        }


        public Func<Space, IEnumerable<Space>> GetBoxAdjacencies { get; set; }

        public BidirectionalGraph<EnergyVertex, Edge<EnergyVertex>> Graph { get; } = new();

        public ILogger Logger { get; set; }



        public IVertexAndEdgeListGraph<EnergyVertex, Edge<EnergyVertex>> CreateGraph()
        {

            while (_openSpaces.Count > 0)
            {
                var currentSpace = _openSpaces.Pop();
                if (_close.Contains(currentSpace.Room.Id)) { continue; }

                var currentSpaceModel = _energyModelFactory.Create(currentSpace);
                _openModels.Push(currentSpaceModel);

                while (_openModels.Count > 0)
                {
                    var current = _openModels.Pop();
                    _close.Add(current.EnergySpace.Space.Room.Id);
                    var currentVertex = new EnergyVertex(Graph.VertexCount, current.EnergySpace);

                    List<EnergyModel> ajBoxModels = GetBoxAdjacencies is null ?
                        _spaces.Where(s => !_close.Contains(s.Room.Id))
                        .Select(s => _energyModelFactory.Create(s)).ToList() :
                        GetAdjacencyModels(current, _energyModelFactory, GetBoxAdjacencies, _close);
                    ajBoxModels.ForEach(_openModels.Push);

                    var interiorEdges = GetInteriorEdges(
                        currentVertex,
                        currentSpaceModel,
                        ajBoxModels,
                        out var interiorSurfaces);

                    if (interiorEdges.Count() > 0)
                    {
                        ajBoxModels.ForEach(m =>
                        m.EnergySurfaces = EnergySurfaceBooleanOperations
                        .Differences(m.EnergySurfaces, interiorSurfaces, _minIntersectionVolume));
                        interiorEdges.ForEach(e => Graph.AddEdge(e));

                        current.EnergySurfaces = EnergySurfaceBooleanOperations
                        .Differences(current.EnergySurfaces, interiorSurfaces, _minIntersectionVolume);
                    }

                    var exteriorEdges = GetExteriorEdges(currentVertex, current.EnergySurfaces);
                    exteriorEdges.ForEach(e => Graph.AddEdge(e));
                }
            }


            return Graph;
        }

        private IEnumerable<TaggedEdge<EnergyVertex, EnergySurface>> GetInteriorEdges(
            EnergyVertex source,
            EnergyModel parentModel,
            IEnumerable<EnergyModel> ajBoxModels,
            out IEnumerable<EnergySurface> intersectionSurfaces)
        {
            var edges = new List<TaggedEdge<EnergyVertex, EnergySurface>>();
            intersectionSurfaces = new List<EnergySurface>();

            //get interior edges
            foreach (var model in ajBoxModels)
            {
                _openModels.Push(model);
                var target = new EnergyVertex(Graph.VertexCount, model.EnergySpace);
                intersectionSurfaces =
                    EnergySurfaceBooleanOperations
                    .Intersections(parentModel.EnergySurfaces, model.EnergySurfaces, _minIntersectionVolume);

                foreach (var surface in intersectionSurfaces)
                {
                    var edge = new TaggedEdge<EnergyVertex, EnergySurface>(
                        source, target, surface);
                    edges.Add(edge);
                }
            }

            return edges;


        }

        private IEnumerable<TaggedEdge<EnergyVertex, EnergySurface>> GetExteriorEdges(
            EnergyVertex parentVertex,
            IEnumerable<EnergySurface> exteriorSurfaces)
        {
            var edges = new List<TaggedEdge<EnergyVertex, EnergySurface>>();

            foreach (var surface in exteriorSurfaces)
            {
                var edge = new TaggedEdge<EnergyVertex, EnergySurface>(
                        parentVertex, null, surface);
                edges.Add(edge);
            }

            return edges;
        }

        private List<EnergyModel> GetAdjacencyModels(
            EnergyModel current,
            IEnergyModelFactory energyModelFactory,
            Func<Space, IEnumerable<Space>> getBoxAdjacencies, IEnumerable<ElementId> closed)
        {
            var ajBoxModels = new List<EnergyModel>();
            var ajBoxSpaces = getBoxAdjacencies.Invoke(current.EnergySpace.Space);
            foreach (var space in ajBoxSpaces)
            {
                if (!closed.Contains(space.Room.Id))
                { ajBoxModels.Add(energyModelFactory.Create(space)); }
            }

            return ajBoxModels;
        }
    }
}
