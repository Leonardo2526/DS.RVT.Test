using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using DS.System;
using ClassLibrary1;

namespace NewTool1
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    { 
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit,
           ref string message, ElementSet elements) 
        {
            DS.System.Location location = new DS.System.Location(1,2,3);
            Class1 class1 = new Class1();

            TaskDialog.Show("Revit", location.X.ToString());
            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}