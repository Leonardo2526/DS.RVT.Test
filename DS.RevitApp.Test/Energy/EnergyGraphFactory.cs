using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using MoreLinq;
using OLMP.RevitAPI.Tools.Extensions;
using QuickGraph;
using QuickGraph.Algorithms;
using Rhino;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace DS.RevitApp.Test.Energy
{
    internal class EnergyGraphFactory :
        //IGraphFactory<EnergyVertex>, 
        ISerilogged
    {
        private readonly static double _minIntersectionVolume = 1.CMToFeet(3);

        private static readonly double _at = RhinoMath.ToRadians(1);
        private static readonly EnergyVertex _nullVertex = new(0, null);
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
            Graph.AddVertex(_nullVertex);
        }


        public Func<Space, IEnumerable<Space>> GetBoxAdjacencies { get; set; }

        public BidirectionalGraph<EnergyVertex, TaggedEdge<EnergyVertex, EnergySurface>> Graph { get; } = new();

        public ILogger Logger { get; set; }



        public BidirectionalGraph<EnergyVertex, TaggedEdge<EnergyVertex, EnergySurface>> CreateGraph()
        {

            while (_openSpaces.Count > 0)
            {
                var currentSpace = _openSpaces.Pop();
                if (_close.Contains(currentSpace.Room.Id)) { continue; }

                var currentSpaceModel = _energyModelFactory.Create(currentSpace);
                _openModels.Push(currentSpaceModel);
                var currentVertex = new EnergyVertex(Graph.VertexCount, currentSpaceModel.EnergySpace);
                Graph.AddVertex(currentVertex);

                while (_openModels.Count > 0)
                {
                    var current = _openModels.Pop();
                    var sourceVertex = Graph.Sinks()
                       .FirstOrDefault(v => v.Tag !=null && v.Tag.Space.Equals(current.EnergySpace.Space));
                    Logger?.Verbose($"Current room is '{current.EnergySpace.Space.Room.Name}'.");
                    _close.Add(current.EnergySpace.Space.Room.Id);

                    List<EnergyModel> ajBoxModels = GetBoxAdjacencies is null ?
                        _spaces.Where(s => !_close.Contains(s.Room.Id))
                        .Select(s => _energyModelFactory.Create(s)).ToList() :
                        GetAdjacencyModels(current, _energyModelFactory, GetBoxAdjacencies, _close);
                    ajBoxModels.ForEach(_openModels.Push);
                    Logger?.Verbose($"{ajBoxModels.Count} box adjacencies spaces was found.");

                    var interiorEdges = GetInteriorEdges(
                        sourceVertex,
                        currentSpaceModel,
                        ajBoxModels,
                        out var interiorSurfaces);

                    Logger?.Verbose($"{interiorEdges.Count()} interiorEdges was found.");
                    if (interiorEdges.Count() > 0)
                    {
                        ajBoxModels.ForEach(m =>
                        m.EnergySurfaces = EnergySurfaceBooleanOperations
                        .Differences(m.EnergySurfaces, interiorSurfaces, ParallelCondition, MinVolumeCondition));
                        interiorEdges.ForEach(e => Graph.AddVerticesAndEdge(e));

                        current.EnergySurfaces = EnergySurfaceBooleanOperations
                        .Differences(current.EnergySurfaces, interiorSurfaces, ParallelCondition, MinVolumeCondition);
                    }

                    var exteriorEdges = GetExteriorEdges(sourceVertex, current.EnergySurfaces);                    
                    Logger?.Verbose($"{exteriorEdges.Count()} exteriorEdges.");
                    exteriorEdges.ForEach(e => Graph.AddVerticesAndEdge(e));
                }
            }

            Logger?.Information($"Graph was created with {Graph.VertexCount} vertices and {Graph.EdgeCount} edges.");
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
                var target = new EnergyVertex(Graph.VertexCount, model.EnergySpace);
                intersectionSurfaces =
                    EnergySurfaceBooleanOperations
                    .Intersections(parentModel.EnergySurfaces,
                    model.EnergySurfaces,
                    ParallelCondition,
                    MinVolumeCondition);
                intersectionSurfaces.ForEach(s => s.SurfaceType = 
                Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.InteriorWall);
                foreach (var surface in intersectionSurfaces)
                {
                    var edge = new TaggedEdge<EnergyVertex, EnergySurface>(
                        source, target, surface);
                    edges.Add(edge);
                }
            }

            return edges;

        }
          private  static bool ParallelCondition(EnergySurface s1, EnergySurface s2)
            {
                var point = s1.Solid.ComputeCentroid();

                var intersectionResult1 = s1.Face.Project(point);
                if (intersectionResult1 == null)
                { return false; }
                var n1 = s1.Face.ComputeNormal(intersectionResult1.UVPoint).ToVector3d();

                var intersectionResult2 = s2.Face.Project(point);
                if (intersectionResult2 == null)
                { return false; }
                var n2 = s2.Face.ComputeNormal(intersectionResult2.UVPoint).ToVector3d();

                return n1.IsParallelTo(n2, _at) != 0;
            }

        private static bool MinVolumeCondition(Solid solid)
        {
            double minIntersectionVolume = 1.CMToFeet(3);
            return solid != null && Math.Abs(solid.Volume) > minIntersectionVolume;
        }

        private IEnumerable<TaggedEdge<EnergyVertex, EnergySurface>> GetExteriorEdges(
            EnergyVertex parentVertex,
            IEnumerable<EnergySurface> exteriorSurfaces)
        {
            var edges = new List<TaggedEdge<EnergyVertex, EnergySurface>>();

            foreach (var surface in exteriorSurfaces)
            {
                var edge = new TaggedEdge<EnergyVertex, EnergySurface>(
                        parentVertex, _nullVertex, surface);
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
