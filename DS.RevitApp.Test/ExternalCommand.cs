using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Diagnostics;

namespace DS.RevitApp.Test
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

            TaskDialog.Show("message","Hello");

            //var path = new PathCreatorClient(uidoc).GetPath();
            //Debug.WriteLine(path?.Count);

            //new TestClass().SomeTest();

            //var test = new PathFinerTest(doc, uidoc);
            //test.Run();

            //var test = new DirectShapeTest(doc, uidoc);
            //test.CreateSphereDirectShape();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }


}
