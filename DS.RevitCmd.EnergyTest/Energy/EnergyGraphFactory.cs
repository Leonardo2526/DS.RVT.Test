using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using MoreLinq;
using OLMP.RevitAPI.Tools.Extensions;
using QuickGraph;
using Rhino;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using EnergyEdge = QuickGraph.TaggedEdge<
    DS.RevitCmd.EnergyTest.EnergyVertex,
    DS.RevitCmd.EnergyTest.EnergySurface>;

namespace DS.RevitCmd.EnergyTest
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
        private readonly IEnumerable<EnergyModel> _models;
        private readonly IEnergyModelFactory _energyModelFactory;
        //private Stack<Space> _openSpaces = new();
        //private Stack<EnergyModel> _openModels = new();
        private List<Space> _close = new();


        public EnergyGraphFactory(
            Document activeDoc, IEnumerable<RevitLinkInstance> links,
            IEnumerable<EnergyModel> models,
            IEnergyModelFactory energyModelFactory)
        {
            _activeDoc = activeDoc;
            _links = links;
            _models = models;
            //spaces.ForEach(_openSpaces.Push);
            _energyModelFactory = energyModelFactory;
            Graph.AddVertex(_nullVertex);
        }


        public Func<Space, IEnumerable<Space>> GetBoxAdjacencies { get; set; }

        public BidirectionalGraph<EnergyVertex, TaggedEdge<EnergyVertex, EnergySurface>> Graph { get; } = new();

        public ILogger Logger { get; set; }



        public BidirectionalGraph<EnergyVertex, TaggedEdge<EnergyVertex, EnergySurface>> CreateGraph()
        {
            var currentModel = _models.First();
            var edges = GetEdges(_models, 0);
            edges.ForEach(e => Graph.AddVerticesAndEdge(e));

            Logger?.Information($"Graph was created with {Graph.VertexCount} vertices and {Graph.EdgeCount} edges.");
            return Graph;
        }

        private IEnumerable<EnergyEdge> GetEdges(
            IEnumerable<EnergyModel> energyModels,
            int ind)
        {
            var edges = new List<EnergyEdge>();

            foreach (var eModel in energyModels)
            {
                Logger?.Verbose($"Current room is '{eModel.EnergySpace.Space.Room.Name}'.");

                _close.Add(eModel.EnergySpace.Space);

                var currentModels = energyModels
                    .Where(m => !_close.Contains(m.EnergySpace.Space));
                Logger?.Verbose($"{currentModels.Count()} box adjacencies spaces was found.");
                (var sourceModel, var adjacencies) = Intersect(eModel, currentModels);

                var sourceVertex = GetVertex(edges, sourceModel);
                sourceModel.EnergySurfaces
               .Where(s => s.SurfaceType ==
               Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.ExteriorWall)
               .ForEach(s => edges.Add(new EnergyEdge(sourceVertex, _nullVertex, s)));

                foreach (var adjacency in adjacencies)
                {
                    ind++;
                    var targetVertex = GetVertex(edges, adjacency.Key);
                    adjacency.Value.ForEach(s => edges.Add(new EnergyEdge(sourceVertex, targetVertex, s)));
                }
                //break;
            }

            return edges;

            EnergyVertex GetVertex(IEnumerable<EnergyEdge> edges, EnergyModel eModel)
            {
                var vertex = edges
                    .Select(e => e.Target)
                .Where(t => t.Tag is not null)
                    .FirstOrDefault(t => t.Tag.Equals(eModel.EnergySpace));
                if (vertex == null)
                {
                    ind++;
                    vertex = new EnergyVertex(ind, eModel.EnergySpace);
                }

                return vertex;
            }
        }

        private (EnergyModel result, Dictionary<EnergyModel, IEnumerable<EnergySurface>> adjacencies) Intersect(
            EnergyModel sourceModel,
            IEnumerable<EnergyModel> energyModels)
        {
            var intersectionDict = GetInteriorSurfaces(
                sourceModel,
                energyModels);
            var interiorModels = intersectionDict.Select(kv => kv.Key);
            var interiorSurfaces = intersectionDict.SelectMany(kv => kv.Value);
            Logger?.Verbose($"{intersectionDict
                .SelectMany(kv => kv.Value).Count()} interiorEdges was found.");

            var sourceExteriorSurfaces = EnergySurfaceBooleanOperations
               .Differences(sourceModel.EnergySurfaces, interiorSurfaces,
               ParallelCondition, MinVolumeCondition).ToList();
            sourceModel.EnergySurfaces.Clear();
            sourceModel.EnergySurfaces.AddRange(sourceExteriorSurfaces);
            sourceModel.EnergySurfaces.AddRange(interiorSurfaces);

            energyModels.ForEach(m =>
                      m.EnergySurfaces = EnergySurfaceBooleanOperations
                      .Differences(m.EnergySurfaces, interiorSurfaces,
                      ParallelCondition, MinVolumeCondition).ToList());

            return (sourceModel, intersectionDict);
        }


        private Dictionary<EnergyModel, IEnumerable<EnergySurface>> GetInteriorSurfaces(
            EnergyModel sourceModel,
            IEnumerable<EnergyModel> ajBoxModels)
        {

            var edges = new List<TaggedEdge<EnergyVertex, EnergySurface>>();
            var intersectionDict = new Dictionary<EnergyModel, IEnumerable<EnergySurface>>();

            //get interior edges
            foreach (var model in ajBoxModels)
            {
                var target = new EnergyVertex(Graph.VertexCount, model.EnergySpace);
                var intersectionSurfaces =
                    EnergySurfaceBooleanOperations
                    .Intersections(sourceModel.EnergySurfaces,
                    model.EnergySurfaces,
                    ParallelCondition,
                    MinVolumeCondition);
                if (intersectionSurfaces.Count() > 0)
                {
                    intersectionSurfaces.ForEach(s => s.SurfaceType =
                    Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.InteriorWall);
                    intersectionDict.Add(model, intersectionSurfaces);
                }
            }

            return intersectionDict;

        }
        private static bool ParallelCondition(EnergySurface s1, EnergySurface s2)
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
