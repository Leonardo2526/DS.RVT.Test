using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Various;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class WallsTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trb;

        public WallsTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _trb = new ContextTransactionFactory(_doc);
            Run();
        }

        private void Run()
        {
            var selector = new ElementSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите элемент");

            var paramName = "OLP_БезПересечений";
            var oLPNoIntersection = element.GetParameters(paramName).FirstOrDefault();
            Debug.WriteLine(element.Id);
            Debug.WriteLine(oLPNoIntersection.Definition.Name);
            Debug.WriteLine(oLPNoIntersection.AsInteger());
            Debug.WriteLine(oLPNoIntersection.AsValueString());

            var line = element.GetCenterLine();
            Debug.WriteLine(line.Direction);
        }
    }
}
