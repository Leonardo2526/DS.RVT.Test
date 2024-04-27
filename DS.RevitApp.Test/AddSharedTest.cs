using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Various;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS.RevitApp.Test
{
    internal class AddSharedTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trb;

        public AddSharedTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _trb = new ContextTransactionFactory(_doc);
            Run();
        }

        private void Run()
        {
            var checker = new ParameterChecker(_doc);
            checker.Check();

            var m = _doc.GetElement(new ElementId(712494));
            var p = m.LookupParameter(ParameterChecker.ParamName1);
            if (p != null && !p.IsReadOnly)
            {
                using (var tr = new Transaction(_doc, "Insert parameters"))
                {
                    tr.Start();

                    p.Set($"{Environment.UserName}#{DateTime.UtcNow}");

                    tr.Commit();
                }

            }

            Debug.WriteLine("");
        }
    }
}
