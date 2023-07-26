using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
//using ClassLibrary1;
using ConsoleApp2;
using System.Net.Http;

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

            new AStarAlgorithmCDFTest(uidoc);

            //PersonClient.Test5(new HttpClient(), "addj");


            //new MongoTest();

            //new SerilogTest(uidoc);

            //var test = new PathFinerTest(doc, uidoc);
            //test.Run();

            //var test = new DirectShapeTest(doc, uidoc);
            //test.CreateSphereDirectShape();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }


}
