using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit.Async;
using System;

namespace DS.RevitCmd.MVVMTemplate3
{
    [Transaction(TransactionMode.Manual)]
    //[Regeneration(RegenerationOption.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            RevitTask.Initialize(uiapp);


            var testModel = new TestViewModel(uidoc);
            StartWindow startWindow = new StartWindow(testModel);
            startWindow.Show();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}