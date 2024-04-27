using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitCmd.EnergyTest.CompoundStructures;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Filtering;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitCmd.EnergyTest
{
    [Transaction(TransactionMode.Manual)]
    public class BoundatyFaceExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;

            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiApp.ActiveUIDocument.Document;

            var allLoadedLinks = doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();

            var logger = new LoggerConfiguration()
             .MinimumLevel.Verbose()
             .WriteTo.Debug()
             .CreateLogger();
            var trf = new ContextTransactionFactory(doc);

            var docsToApply = new List<Document>() { doc };
            var elementFilter =
                new ElementFilterBuilder(docsToApply, doc, allLoadedLinks)
                .Create();

            var itemSelector = new ItemSelector(uiDoc);
            var wall = itemSelector.SelectWall();
            var face = itemSelector.SelectFace();

            var test = new CompoundFaceStructureTest(uiDoc, elementFilter)
            { TransactionFactory = trf, Logger = logger };
            var results = test.CreateFaceStructures(wall, face);
            test.PrintResults(results);
            //test.ShowResults(results);
            //return Autodesk.Revit.UI.Result.Succeeded;
            //var resultFaces = test.ComputeResultFaces(results);
            //test.ShowFaces(resultFaces);

            var resultEnergyFaces = test.GetEnergyFaces(results);
            var eFaces = resultEnergyFaces.Select(ef => ef.Face);
            test.ShowFaces(eFaces);
            test.PrintEnergyResults(resultEnergyFaces);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}