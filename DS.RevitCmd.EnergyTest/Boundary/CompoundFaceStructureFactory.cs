using Autodesk.Revit.DB;
using DS.GraphUtils.Entities;
using MoreLinq;
using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using FaceVertex = DS.GraphUtils.Entities.TaggedVertex
    <DS.RevitCmd.EnergyTest.SpaceBoundary.BoundaryFace>;

namespace DS.RevitCmd.EnergyTest.SpaceBoundary
{
    public class CompoundFaceStructureFactory
    {
        private readonly Document _activeDoc;
        private readonly Func<BoundaryFace, IEnumerable<BoundaryFace>> _getInteractionFaces;

        public CompoundFaceStructureFactory(
            Document activeDoc, Func<BoundaryFace, IEnumerable<BoundaryFace>> getInteractionFaces)
        {
            _activeDoc = activeDoc;
            _getInteractionFaces = getInteractionFaces;
        }


        public IEnumerable<CompoundFaceStructure> Create(BoundaryFace sourceBoundaryFace)
        {
            var parentFace = sourceBoundaryFace.GetOpposite(_activeDoc);

            var graph = new AdjacencyGraph<FaceVertex, Edge<FaceVertex>>();
            var faceVertex = new FaceVertex(0, parentFace);
            graph.AddVertex(faceVertex);
            BuildGraph(graph, faceVertex);
            return ToFaceStructures(graph, sourceBoundaryFace);
        }


        private IVertexAndEdgeListGraph<FaceVertex, Edge<FaceVertex>> BuildGraph(
            AdjacencyGraph<FaceVertex, Edge<FaceVertex>> graph,
            FaceVertex parent)
        {
            var boundaryFaces = _getInteractionFaces.Invoke(parent.Tag);
            var children = boundaryFaces.Select(f => f.GetOpposite(_activeDoc)).ToList();

            foreach (var child in children)
            {
                var childVertex = new FaceVertex(graph.VertexCount, child);
                var childEdge = new Edge<FaceVertex>(parent, childVertex);
                graph.AddVerticesAndEdge(childEdge);
                BuildGraph(graph, childVertex);
            }

            return graph;
        }

        private IEnumerable<CompoundFaceStructure> ToFaceStructures(
            IVertexAndEdgeListGraph<FaceVertex, Edge<FaceVertex>> graph,
            BoundaryFace sourceBoundaryFace)
        {
            var faceSructures = new List<CompoundFaceStructure>();

            var root = graph.Roots().First();
            var sinks = graph.Sinks();
            foreach (var sink in sinks)
            {
                var path = graph.GetPath(root, sink);
                var pathVertices = path.Select(e => e.Source).ToList();
                pathVertices.Add(path.Last().Target);
                var structure = ToFaceStrucure(pathVertices);
                faceSructures.Add(structure);
            }

            return faceSructures;
            CompoundFaceStructure ToFaceStrucure(IEnumerable<FaceVertex> vertices)
            {
                var structure = new CompoundFaceStructure(sourceBoundaryFace);
                vertices.ForEach(v => structure.Add(v.Tag));
                return structure;
            }
        }
    }
}
