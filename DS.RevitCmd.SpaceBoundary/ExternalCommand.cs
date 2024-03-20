using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OLMP.RevitAPI.Tools.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using OLMP.RevitAPI.Tools.Extensions;
using Serilog;
using OLMP.RevitAPI.Tools.Creation.Transactions;

namespace DS.RevitCmd.SpaceBoundary
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

            var edgeTest = new BoundaryEdgeBuilderTest(uiDoc, allLoadedLinks)
            { Logger = logger, TransactionFactory = trf };
            edgeTest.GetEdges();


            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}