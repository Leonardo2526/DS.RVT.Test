using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest
{
    internal class ElementaryTransaction
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly TransactionBuilder _transactionBuilder;

        public ElementaryTransaction(Document doc, UIDocument uiDoc)
        {
            Debug.IndentLevel = 1;
            _doc = doc;
            _uiDoc = uiDoc;
            _transactionBuilder = new TransactionBuilder(doc);
        }

        public void RegenerateDocument()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(0);
            var currentMethodName = sf.GetMethod().Name;

            _transactionBuilder.Build(_doc.Regenerate, currentMethodName);
            Debug.WriteLine($"'{currentMethodName}' executed.");
        }
    }
}
