using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;

namespace DS.RevitApp.Test.Energy
{
    internal interface ISpaceFactory
    {
        Space Create(Room room);
    }
}