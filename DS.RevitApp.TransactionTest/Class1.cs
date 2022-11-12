using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest
{
    internal class Class1
    {
        public MEPCurve MEPCurve { get; set; }

        public Class1(MEPCurve mEPCurve)
        {
            MEPCurve = mEPCurve;
        }
    }
}
