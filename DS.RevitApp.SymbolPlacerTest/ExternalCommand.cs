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

namespace DS.RevitApp.SymbolPlacerTest
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

            var testedClass = new FamiliesSelectorTest(uidoc, doc, uiapp);
            testedClass.RunTest();
            List<MEPCurve> _targerMEPCurves = new List<MEPCurve>();
            _targerMEPCurves.Add(testedClass.MEPCurve);

            SymbolPlacerClient symbolPlacer = new SymbolPlacerClient(testedClass.Families, _targerMEPCurves);
            symbolPlacer.Run();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }


}
