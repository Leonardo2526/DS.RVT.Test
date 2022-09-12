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
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.ModelCurveUtils;

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

            //Reference reference = uidoc.Selection.PickObject(ObjectType.Element, "Select element");
            //MEPCurve element = doc.GetElement(reference) as MEPCurve;


            var selector = new ElementSelector(uidoc);
            var element1 = selector.Select("Element1");
            var element2 = selector.Select("Element2");

            var creator = new ModelCurveCreator(doc);
            creator.Create(element1.Value, element2.Value);


            return Autodesk.Revit.UI.Result.Succeeded;
        }


        private void Application_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            FailuresAccessor fa = e.GetFailuresAccessor();
            var failList = fa.GetFailureMessages();
            TaskDialog.Show("Fails: ", failList.Count.ToString());

            var messagesHandler = new MessagesHandler(e);
        }
    }


}
