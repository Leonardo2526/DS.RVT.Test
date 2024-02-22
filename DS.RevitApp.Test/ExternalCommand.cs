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
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();

            var test = new GetOpeningsSolidTest(uidoc)
            { Logger = logger };
            test.GetWallSolid();

            //var test = new OpeningsUtilsTest(uidoc)
            //{ Logger = logger };
            //test.GetOpeningsSolids();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }


}
