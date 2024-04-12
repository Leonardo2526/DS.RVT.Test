using Autodesk.Revit.DB;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitCmd.EnergyTest.SpaceBoundary
{
    public class CompoundFaceStructure : List<BoundaryFace>
    { }

    public class CompoundFaceStructureFactory
    {
        private readonly Document _activeDoc;
        private readonly Func<BoundaryFace, IEnumerable<BoundaryFace>> _getInteractionFaces;

        public CompoundFaceStructureFactory(
            Document activeDoc, Func<BoundaryFace,IEnumerable<BoundaryFace>> getInteractionFaces)
        {
            _activeDoc = activeDoc;
            _getInteractionFaces = getInteractionFaces;
        }


        public IEnumerable<CompoundFaceStructure> Create(BoundaryFace boundaryFace)
        {
            var parentFace = boundaryFace.GetOpposite(_activeDoc);
            var parent = new CompoundFaceStructure() { parentFace };
            return GetChildren(parent);
        }

        private IEnumerable<CompoundFaceStructure> GetChildren(CompoundFaceStructure parent)
        {
            var faceSructures = new List<CompoundFaceStructure>() { parent };

            var boundaryFaces = _getInteractionFaces.Invoke(parent.Last());
            var children = boundaryFaces.Select(f => f.GetOpposite(_activeDoc));
            foreach (var child in children)
            {
                var list = new CompoundFaceStructure();
                list.AddRange(parent);
                var nextChildren = GetChildren(list);
                faceSructures.AddRange(nextChildren);
            }

            return faceSructures;
        }
    }
}
