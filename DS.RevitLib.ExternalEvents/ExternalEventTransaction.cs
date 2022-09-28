using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.ExternalEvents
{
    public class ExternalEventTransaction : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            var uidoc = app.ActiveUIDocument;
            var doc = uidoc.Document;
            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var baseMEPCurve = doc.GetElement(reference) as MEPCurve;

            ElementUtils.DeleteElement(doc, baseMEPCurve);
        }

        public string GetName()
        {
            return "External Event Example";
        }
    }
}
