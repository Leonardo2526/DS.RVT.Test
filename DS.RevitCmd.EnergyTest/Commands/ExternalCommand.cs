using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitCmd.EnergyTest.CompoundStructures;
using DS.RevitCmd.EnergyTest.SpaceBoundary;
using DS.RevitCmd.EnergyTest.TestRunners;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitCmd.EnergyTest
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

            var allLoadedLinks = doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();

            var logger = new LoggerConfiguration()
             .MinimumLevel.Verbose()
             .WriteTo.Debug()
             .CreateLogger();
            var trf = new ContextTransactionFactory(doc);
            var globalFilter = DocFilterBuilder.GetFilter(doc, allLoadedLinks);


            //var edgeTest = new BoundaryEdgeBuilderTest(uiDoc, allLoadedLinks, globalFilter)
            //{ Logger = logger, TransactionFactory = trf };
            //edgeTest.GetEdges();


            //var test = new EnergyModelBuilderTest(uiDoc, globalFilter)
            //{ Logger = logger };
            //test.GetModels();
            ////test.CreateGraph();

            // var roomNumberts = new List<string>()
            // {
            //     "0"
            // };
            // var rooms = new RoomExtractor(uiDoc, globalFilter)
            //     .GetRooms();
            //var test = new CompoundStructureTest(uiDoc, globalFilter)
            //{ Logger = logger };
            // test.CreateStructures(rooms.First());


            new FaceFitElementsTest(uiDoc, globalFilter)
            { TransactionFactory = trf, Logger = logger }
         .SelectWall()
         .GetElements();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}