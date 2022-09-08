using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransactionTests
{
    internal class TransactionTest
    {
        readonly Document _doc;

        public TransactionTest(Document doc)
        {
            this._doc = doc;
        }

        public void Test1()
        {
            using (Transaction transNew = new Transaction(_doc))
            {
                transNew.Start();
                Line line = Line.CreateBound(new XYZ(-10, 0, 0), new XYZ(-10, 0, 0));

                transNew.Commit();
            }
        }
    }
}
