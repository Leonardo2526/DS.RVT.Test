using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Various;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class FloorsTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trb;

        public FloorsTest(UIDocument uiDoc)
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
            var floor = element as Floor;
            //var b = floor.IsTraversable(XYZ.BasisZ.ToVector3d());

            //Debug.WriteLine(b);
        }
    }
}
