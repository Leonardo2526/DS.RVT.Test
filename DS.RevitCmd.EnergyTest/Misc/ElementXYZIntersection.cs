using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Intersections;

namespace DS.RevitCmd.EnergyTest.SpaceBoundary
{
    internal readonly struct ElementXYZIntersection(Element item1, Element item2, XYZ intersectionResult)
    {
        public Element Item1 { get; } = item1;

        public Element Item2 { get; } = item2;
        public XYZ Result { get; } = intersectionResult;
    }
}
