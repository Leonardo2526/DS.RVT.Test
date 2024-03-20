using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;

namespace DS.RevitApp.Test.Energy
{
    public interface ISpaceFactory
    {
        Space Create(Room room);
    }
}