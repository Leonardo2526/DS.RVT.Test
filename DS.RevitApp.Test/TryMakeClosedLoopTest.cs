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
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitApp.Test
{
    internal class TryMakeClosedLoopTest : ISerilogged
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly List<RevitLinkInstance> _allLoadedLinks;
        private readonly List<Document> _allFilteredDocs;

        public TryMakeClosedLoopTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _allLoadedLinks = _doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
            _allFilteredDocs = new List<Document>() { _doc };
            _allFilteredDocs.AddRange(_allLoadedLinks.Select(l => l.GetLinkDocument()));
        }


        public ITransactionFactory TransactionFactory { get; set; }
        public ILogger Logger { get; set; }


        public IEnumerable<ModelCurve> SelectCurves()
        {
            var mCurves = new List<ModelCurve>();

            int i = 0;
            while (true)
            {
                try
                {
                    i++;
                    Reference reference1 = _uiDoc.Selection
                        .PickObject(ObjectType.Element, $"Select curve {i}");
                    var mc = _doc.GetElement(reference1) as ModelCurve;
                    mCurves.Add(mc);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                    break;
                }

            }

            return mCurves;
        }

        public void TryMakeLoopClosed(IEnumerable<ModelCurve> modelCurves)
        {
            var curves = modelCurves.Select(m => m.GeometryCurve);

            var closedLoop = CurveUtils.TryConnect(curves, getConnectedCurveAtAnyPoint);
            //var closedLoop = CurveLoopUtils.TryCreateLoop(curves);
            if (closedLoop == null)
            {
                Logger?.Error("Failed to make loop closed!");
                return;
            }

            foreach (var mc in modelCurves)
            { DeleteCurve(mc); }

            foreach (var curve in closedLoop)
            { ShowCurve(curve); }

            static Curve getConnectedCurve(Curve current, Curve previous, Curve next)
            {
                var result = current.TrimOrExtend(previous, false, true, 1)
                  .FirstOrDefault();
                return result.TrimOrExtend(next, false, true, 0)
                    .FirstOrDefault();
            }

            static Curve getConnectedCurveAtAnyPoint(Curve current, Curve previous, Curve next)
            {
                var result = current.TrimOrExtendAnyPoint(previous, false, true)
                     .FirstOrDefault();
                return result.TrimOrExtendAnyPoint(next, false, true)
                    .FirstOrDefault();
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
