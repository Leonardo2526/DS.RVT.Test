using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP.SystemTree;
using OLMP.RevitLib.MEPAC.SystemConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLMP.RevitLib.MEPAC.Test.TestedClasses
{
    internal class SystemConnectionServiceTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly TransactionBuilder _trb;

        public SystemConnectionServiceTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _trb = new TransactionBuilder(doc);
        }

        public void Run()
        {
            MEPCurve mEPCurve1 = _doc.GetElement(new ElementId(709096)) as MEPCurve;
            var mEPSystem1 = new SimpleMEPSystemBuilder(mEPCurve1).Build();
            MEPCurve mEPCurve2 = _doc.GetElement(new ElementId(709865)) as MEPCurve;
            var mEPSystem2 = new SimpleMEPSystemBuilder(mEPCurve1).Build();
            _trb.Build(() => new SystemConnectionService(mEPSystem1, mEPSystem2).Connect(), "Commit connection");
        }
    }
}
