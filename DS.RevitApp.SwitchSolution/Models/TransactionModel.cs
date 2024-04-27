using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.ModelCurveUtils;
using Revit.Async;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitApp.SwitchSolution.Models
{
    internal class TransactionModel
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public TransactionModel(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public async Task CreateAsync(List<XYZ> path)
        {
            Debug.Print($"Transaction showLines started");
            var trb = new TransactionBuilder(_doc);
           await trb.BuildRevitTask(() => ShowLines(path), "showLines");
            await RevitTask.RunAsync(() => _uiDoc.RefreshActiveView());
            Debug.Print($"Transaction showLines executed");

            //trb.Build(() => ShowcCrves(path), "show curves");
        }

        public void Create(List<XYZ> path)
        {
            Debug.Print($"Transaction showLines started");

            RevitTask.RunAsync(() =>
            {
                using (var tr = new Transaction(_doc, "del"))
                {
                    tr.Start();
                    ShowLines(path);
                    tr.Commit();
                    _uiDoc.RefreshActiveView();
                }

            });



            Debug.Print($"Transaction showLines executed");

            //trb.Build(() => ShowcCrves(path), "show curves");
        }

        private void ShowLines(List<XYZ> path)
        {
            var mcreator = new ModelCurveCreator(_doc);
            for (int i = 0; i < path.Count - 1; i++)
            {
                mcreator.Create(path[i], path[i + 1]);
            }
        }

        private void ShowLinesLists(List<List<XYZ>> pointsLists)
        {
            var mcreator = new ModelCurveCreator(_doc);
            foreach (var list in pointsLists)
            {
                ShowLines(list);
            }
        }

        private void ShowcCrves(List<XYZ> path)
        {
            Reference reference = _uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Select element");
            var mEPCurve = _doc.GetElement(reference) as MEPCurve;

            var builder = new BuilderByPoints(mEPCurve, path).BuildMEPCurves().WithElbows();
        }

        public void RunSplitTransaction(MEPCurve mEPCurve, Element spud)
        {
            
            RevitTask.RunAsync(() =>
            {
                using (var trg = new TransactionGroup(_doc, $"split elem"))
                {
                    trg.Start();
                    Debug.Print($"TransactionGroup '{trg.GetName()}' started in thread {Thread.CurrentThread.ManagedThreadId}");

                    using (var tr = new Transaction(_doc, "del"))
                    {
                        tr.Start();
                        XYZ point = mEPCurve.GetCenterPoint();
                        mEPCurve.Split(point);
                        tr.Commit();
                        _uiDoc.RefreshActiveView();
                    }

                    Debug.Print($"MEPCurve - {mEPCurve.IsValidObject}, spud - {spud.IsValidObject}");

                    if (trg.HasStarted())
                    {
                        //trg.Commit();
                        trg.RollBack();
                        Debug.Print($"TransactionGroup '{trg.GetName()}' rolled in thread {Thread.CurrentThread.ManagedThreadId}");
                    }
                    Debug.Print($"MEPCurve - {mEPCurve.IsValidObject}, spud - {spud.IsValidObject}");
                }

                Debug.Print($"MEPCurve - {mEPCurve.IsValidObject}, spud - {spud.IsValidObject}");
            });
        }

    }
}
