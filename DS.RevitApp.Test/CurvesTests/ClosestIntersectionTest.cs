using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Basis;
using DS.GraphUtils.Entities;
using DS.RevitApp.Test.Energy;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitApp.Test.CurvesTests
{
    internal class ClosestIntersectionTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedLinks;
        private DocumentFilter _globalFilter;

        public ClosestIntersectionTest(UIDocument uiDoc, IEnumerable<RevitLinkInstance> allLoadedLinks)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
        }

        public ITransactionFactory TransactionFactory { get; set; }
        public ILogger Logger { get; set; }

        private ModelCurve _mCurve1;
        private Curve _curve1;
        private Curve _curve2;

        public ClosestIntersectionTest SelectTWoCurves()
        {
            Reference reference1 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select curve1");
            _mCurve1 = _doc.GetElement(reference1) as ModelCurve;
            _curve1 = _mCurve1.GeometryCurve;
            Reference reference2 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select curve2");
            var mCurve2 = _doc.GetElement(reference2) as ModelCurve;
            _curve2 = mCurve2.GeometryCurve;

            //ShowPoint(_modelCurve1.GeometryCurve.GetEndPoint(0));
            //ShowPoint(_modelCurve2.GeometryCurve.GetEndPoint(0));

            return this;
        }

        public ClosestIntersectionTest SelectTWoWalls()
        {
            Reference reference1 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select wall1");
            var w1 = _doc.GetElement(reference1) as Wall;
            _curve1 = w1.GetLocationCurve();
            Reference reference2 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select wall2");
            var w2 = _doc.GetElement(reference2) as Wall;
            _curve2 = w2.GetLocationCurve();

            //ShowPoint(_modelCurve1.GeometryCurve.GetEndPoint(0));
            //ShowPoint(_modelCurve2.GeometryCurve.GetEndPoint(0));

            return this;
        }

        public void GetClosestIntersectionCurve()
        {
            var intersectionCurve = _curve1.GetClosestIntersection(_curve2, true, true, out var intersectionPoint);
            if (intersectionCurve != null) 
            {
                DeleteCurve(_mCurve1);
                ShowCurve(intersectionCurve);
                ShowPoint(intersectionPoint);
            }
        }

        private void DeleteCurve(ModelCurve mCurve)
            => TransactionFactory.Create(() => _doc.Delete(mCurve.Id), "ShowCurve");

        private void ShowCurve(Curve curve)
        => TransactionFactory.Create(() => curve.Show(_doc), "ShowCurve");

        private void ShowPoint(XYZ point)
       => TransactionFactory.Create(() => point.Show(_doc), "ShowPoint");
    }
}
