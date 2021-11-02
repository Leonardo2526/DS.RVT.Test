using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.RVT.Pipe
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit,
           ref string message, ElementSet elements)
        {
            TaskDialog.Show("Revit", "Hello Pipe");
            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}