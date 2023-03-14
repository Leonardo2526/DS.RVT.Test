using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.Test;
using DS.RevitApp.TransactionTest.View;
using DS.RevitApp.TransactionTest.ViewModel;
using Revit.Async;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DS.RevitApp.TransactionTest
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
          ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            //var test = new RevitAsyncTest(doc, uidoc);
            //test.RunRevitTask();

            var startWindow = new TestWindow(doc, uidoc, uiapp);
            startWindow.Show();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}