using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitApp.Test.Energy;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Untested;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            curves = CurveUtils_Untested.FitEndToStart(curves);
            PrintCurvePoints(curves);
            //return;
            var closedLoop = CurveUtils_Untested.TryConnect(curves, getConnectedCurve);
            closedLoop = CurveUtils_Untested.TryCreateLoop(closedLoop);
            if (closedLoop == null || closedLoop.Count() == 0)
            {
                Logger?.Error("Failed to make loop closed!");
                return;
            }

            foreach (var mc in modelCurves)
            { DeleteCurve(mc); }

            foreach (var curve in closedLoop)
            { ShowCurve(curve); }

            static Curve getConnectedCurve(Curve current, Curve previous, Curve next)
            => current.TrimOrExtend(previous, next, true, true);
        }

        private void PrintCurvePoints(IEnumerable<Curve> curves)
        {
            int i = 0;
            foreach (var curve in curves)
            {
                i++;
                Debug.WriteLine($"Curve + {i}");
                var p1 = curve.GetEndPoint(0);
                //ShowPoint(p1);
                var p2 = curve.GetEndPoint(1);
                var dir = (p2 - p1).RoundVector().Normalize();
                Debug.WriteLine(p1);
                Debug.WriteLine(p2);
                Debug.WriteLine(dir);
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
