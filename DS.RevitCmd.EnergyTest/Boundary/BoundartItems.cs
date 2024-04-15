using Autodesk.Revit.DB;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Geometry.Faces;

namespace DS.RevitCmd.EnergyTest.SpaceBoundary
{
    public record BoundaryCurve(ElementId ElementId, Curve Curve);

    public record BoundaryFace(ElementId ElementId, Face Face)
    {
        public BoundaryFace GetOpposite(Document doc)
        {
            var element = doc.GetElement(ElementId);
            var solid = element.Solid();
            var oppositeFace = FaceUtils
                .ProjectAtCenterOnOpposite(Face, solid, out var offsetVector);
            return this with { Face = oppositeFace };
        }
    }
}
