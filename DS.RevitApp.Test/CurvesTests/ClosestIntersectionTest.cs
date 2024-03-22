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

        private ModelCurve _modelCurve1;
        private ModelCurve _modelCurve2;

        public ClosestIntersectionTest SelectTWoCurves()
        {
            Reference reference1 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select curve1");
            _modelCurve1 = _doc.GetElement(reference1) as ModelCurve;
            Reference reference2 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select curve2");
            _modelCurve2 = _doc.GetElement(reference2) as ModelCurve;

            //ShowPoint(_modelCurve1.GeometryCurve.GetEndPoint(0));
            //ShowPoint(_modelCurve2.GeometryCurve.GetEndPoint(0));

            return this;
        }

        public void GetClosestIntersection()
        {
            var curve1 = _modelCurve1.GeometryCurve; var curve2 = _modelCurve2.GeometryCurve;
            var intersectionPoint = curve1.ClosestIntersection(curve2, true, true, out var IntersectionResult);
            if (intersectionPoint != null) { ShowPoint(intersectionPoint); }
        }

        private void DeleteCurve(ModelCurve mCurve)
            => TransactionFactory.Create(() => _doc.Delete(mCurve.Id), "ShowCurve");

        private void ShowCurve(Curve curve)
        => TransactionFactory.Create(() => curve.Show(_doc), "ShowCurve");

        private void ShowPoint(XYZ point)
       => TransactionFactory.Create(() => point.Show(_doc), "ShowPoint");
    }
}
