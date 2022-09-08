using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.MEP.SystemTree;
using System.Xml.Linq;
using DS.RevitLib.Utils.MEP;
using DS.RevitApp.Test.TransactionTests;

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

            application.FailuresProcessing += Application_FailuresProcessing;

            var tr = new TransactionTest(doc);
            tr.Test1();


            //var selector = new FamiliesSelectorTest(uidoc, doc, uiapp);
            //selector.RunTest();
            //List<MEPCurve> _targerMEPCurves = new List<MEPCurve>();
            //_targerMEPCurves.AddRange(selector.MEPCurves);

            //SymbolPlacerClient symbolPlacer = new SymbolPlacerClient(selector.Families, _targerMEPCurves, selector.Points);
            //symbolPlacer.Run();
             
            return Autodesk.Revit.UI.Result.Succeeded;
        }

        private void Application_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            var messagesHandler = new MessagesHandler(e);
        }
    }


}
