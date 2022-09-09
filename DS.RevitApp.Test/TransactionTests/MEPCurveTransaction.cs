using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransactionTests
{
    internal class MEPCurveTransaction
    {
        private readonly Document _doc;
        private readonly MEPCurve _baseMEPCurve;

        public MEPCurveTransaction(Document doc, MEPCurve baseMEPCurve)
        {
            _doc = doc;
            _baseMEPCurve = baseMEPCurve;
        }


        #region Properties

        private ElementId MEPSystemTypeId
        {
            get
            {
                MEPCurve mEPCurve = _baseMEPCurve;
                return mEPCurve.MEPSystem.GetTypeId();
            }
        }
        private ElementId ElementTypeId
        {
            get
            {
                return _baseMEPCurve.GetTypeId();
            }
        }
        private ElementId MEPLevelId
        {
            get
            {
                return _baseMEPCurve.ReferenceLevel.Id;
            }
        }
        

        #endregion


        /// <summary>
        /// Create pipe between 2 points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public void CreateMEPCurveByPoints(XYZ p1, XYZ p2)
        {
            using (Transaction transNew = new Transaction(_doc, "CreateMEPCurveByPoints"))
            {
                Line line = Line.CreateBound(p1, p2);

                transNew.Start();

                Wall.Create(_doc, line, MEPLevelId, false);
                Wall.Create(_doc, line, MEPLevelId, false);

                transNew.Commit();

            }
        }

    }
}
