using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.TransactionTest.Model;
using DS.RevitApp.TransactionTest.View;
using DS.RevitApp.TransactionTest.ViewModel;
using Revit.Async;

namespace DS.RevitApp.TransactionTest
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            var tr = new TransactioinTestViewModel(doc, uidoc);
            tr.CreateTransaction(doc, uiapp);
            //tr.SynchronizeWithCentralWindow(doc, uiapp);


            //RevitTask.Initialize(uiapp);

            //var startWindow = new TransactionWindow(doc, uidoc, uiapp);
            //startWindow.Show();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}