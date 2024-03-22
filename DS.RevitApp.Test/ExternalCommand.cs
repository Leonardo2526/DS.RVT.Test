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
using OLMP.RevitAPI.Tools.Creation.Transactions;
using DS.RevitApp.Test.CurvesTests;

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
            var allLoadedLinks = doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();

            var logger = new LoggerConfiguration()
             .MinimumLevel.Verbose()
             .WriteTo.Debug()
             .CreateLogger();
            var trf = new ContextTransactionFactory(doc);


            //var test = new MakeBoundTest(uidoc)
            //{ Logger = logger, TransactionFactory = trf };


            //var curve = test
            //    .SelectCurve()?
            //    .SelectFirstPoint()?
            //    .SelectSecondPoint()?
            //    .CreateNewCurve();
            //var test = new TryMakeClosedLoopTest(uidoc)
            //{ Logger = logger, TransactionFactory = trf };
            //var loop = test.SelectCurves();
            //test.TryMakeLoopClosed(loop);
            //return Autodesk.Revit.UI.Result.Succeeded;


            //var connectorTest = new CurveConnectorTest(uidoc)
            //{ Logger = logger, TransactionFactory = trf };
            ////connectorTest.CreateCurve();
            //var curves = connectorTest.SelectTWoCurves();
            //connectorTest.ConnectTwoCurves(curves.Item1, curves.Item2);
            //connectorTest.CreateCurve();
            //connectorTest.GetBases(curves.Item1, curves.Item2);
            //return Autodesk.Revit.UI.Result.Succeeded;

            //var openingTest = new GetOpeningsSolidTest(uidoc)
            //{ Logger = logger };
            //openingTest.TestGetBestOpeningSolid();
            ////openingTest.GetWallSolid();
            //return Autodesk.Revit.UI.Result.Succeeded;


            //var test = new DirectShapeTest(doc, uidoc);
            //test.SelectWall();

            //var wallTest = new WallsTest(uidoc);
            //wallTest
            //    .SelectWall()?
            //    .GetSortedFaces();
            //.GetSortedFaces();
            //new WallsTest(uidoc).GetWallOpenings();
            //return Autodesk.Revit.UI.Result.Succeeded;


            //var test = new EnergyModelBuilderTest(uidoc)
            //{ Logger = logger };
            //test.GetModels();


            ////test.CreateGraph();
            ///

            //var test = new SolidOperationTest(doc, uidoc, allLoadedLinks)
            //{  TransactionFactory = trf };
            //test.GetAllJoints();
            //test.IntersectionTest();

            new ClosestIntersectionTest(uidoc, allLoadedLinks)
            { Logger = logger, TransactionFactory = trf }
            //.SelectTWoWalls()
            .SelectTWoCurves()
            //.GetClosestIntersection();
            .GetClosestIntersectionCurve();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }


}
