using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitApp.Test.Energy;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitApp.Test
{
    internal class CurveConnectorTest : ISerilogged
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly List<RevitLinkInstance> _allLoadedLinks;
        private readonly List<Document> _allFilteredDocs;

        public CurveConnectorTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _allLoadedLinks = _doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
            _allFilteredDocs = new List<Document>() { _doc };
            _allFilteredDocs.AddRange(_allLoadedLinks.Select(l => l.GetLinkDocument()));
        }


        public ITransactionFactory TransactionFactory { get; set; }
        public ILogger Logger { get; set; }

        public (ModelCurve, ModelCurve) SelectTWoCurves()
        {
            Reference reference1 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select curve1");
            var w1 = _doc.GetElement(reference1) as ModelCurve;
            Reference reference2 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select curve2");
            var w2 = _doc.GetElement(reference2) as ModelCurve;
            return (w1, w2);
        }

        public void ConnectTwoCurves(ModelCurve mCurve1, ModelCurve mCurve2)
        {
            var curve1 = mCurve1.GeometryCurve; var curve2 = mCurve2.GeometryCurve;

            var p1 = curve1.GetEndPoint(0);
            //ShowPoint(p1);
            var p2 = curve1.GetEndPoint(1);

            //curve1 = curve1.CreateReversed();
            NewCurveExtensions.TransactionFactory = TransactionFactory;
            curve1 = CurveUtils.IsBaseEndFitted(curve1, curve2) ?
               curve1 :
               curve1.CreateReversed();

            var fp1 = curve1.GetEndPoint(0);
            //ShowPoint(fp1);
            var fp2 = curve1.GetEndPoint(1);

            var p21 = curve2.GetEndPoint(0);
            //ShowPoint(p21);

            //var resulstCurves = curve1.Trim(curve2, true);
            //var resulstCurves = curve1.Extend(curve2, true, 0);
            //var resulstCurves = curve1.Connect(curve2, true, 0);
            //var resulstCurves = curve1.TrimOrExtend(curve2, true, true);
            var resulstCurves = curve1.TrimOrExtend(curve2, true, true, 0);
            //var resulstCurves = curve1.Trim(curve2, true);
            //var resultCurve = resulstCurves.LastOrDefault();
            var resultCurve = resulstCurves.FirstOrDefault();
            //resultCurve = resultCurve.TrimOrExtend(curve2, true, true, 1).FirstOrDefault();
            if (resultCurve != null)
            {
                ShowCurve(resultCurve);
                var resultPoint = resultCurve.GetEndPoint(0);
                //ShowPoint(resultPoint);
                DeleteCurve(mCurve1);

                var r1 = resultCurve.GetEndPoint(0);
                var r2 = resultCurve.GetEndPoint(1);
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
