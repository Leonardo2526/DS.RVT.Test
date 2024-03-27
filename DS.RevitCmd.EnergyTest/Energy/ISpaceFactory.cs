using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;

namespace DS.RevitCmd.EnergyTest
{
    public interface ISpaceFactory
    {
        Space Create(Room room);
    }
}