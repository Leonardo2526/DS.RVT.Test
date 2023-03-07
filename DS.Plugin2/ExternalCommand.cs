using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.Test;
using System;

namespace DS.Plugin2
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;

            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiApp.ActiveUIDocument.Document;

            MessageCaller.Call();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}