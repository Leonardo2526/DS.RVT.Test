using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class ConnectMEPCurveTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public ConnectMEPCurveTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public void Run()
        {
            Reference reference1 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select MEPCurve1");
            MEPCurve mEPCurve1 = _doc.GetElement(reference1) as MEPCurve;

            Reference reference2 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select MEPCurve2");
            MEPCurve mEPCurve2 = _doc.GetElement(reference2) as MEPCurve;

            //Reference reference3 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select MEPCurve3");
            //MEPCurve mEPCurve3 = _doc.GetElement(reference3) as MEPCurve;

            var transactionBuilder = new TransactionBuilder(_doc);
            transactionBuilder.Build(() =>
            {
                IConnectionFactory factory = new MEPCurveConnectionFactory(_doc, mEPCurve1, mEPCurve2, null);
                factory.Connect();
            }, "connectMEPCurve");

        }
    }
}
