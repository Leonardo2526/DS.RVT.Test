using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
//using ClassLibrary1;
using ConsoleApp2;
using OLMP.RevitAPI.Tools.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using OLMP.RevitAPI.Tools.Extensions;
using Serilog;

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
         
           var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Debug()
            .CreateLogger();

            var openingTest = new GetOpeningsSolidTest(uidoc)
            { Logger = logger };
            openingTest.TestGetBestOpeningSolid();
            //openingTest.GetWallSolid();
            return Autodesk.Revit.UI.Result.Succeeded;


            //var test = new DirectShapeTest(doc, uidoc);
            //test.SelectWall();

            //var wallTest = new WallsTest(uidoc);
            //wallTest
            //    .SelectWall()?
            //    .GetSortedFaces();
            //.GetSortedFaces();
            //new WallsTest(uidoc).GetWallOpenings();
            return Autodesk.Revit.UI.Result.Succeeded;


            var test = new EnergyModelBuilderTest(uidoc)
            { Logger = logger };
            //test.GetModels();
            test.CreateGraph();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }


}
