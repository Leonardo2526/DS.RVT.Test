using Autodesk.Revit.DB;

namespace DS.RevitApp.Test.Energy
{
    internal interface IEnergySurfaceFactory
    {
        EnergySurface CreateEnergySurface(BoundarySegment segment, Curve baseCurve);
    }
}