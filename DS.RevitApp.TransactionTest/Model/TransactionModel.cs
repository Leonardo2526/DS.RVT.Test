using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.ModelCurveUtils;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace DS.RevitApp.TransactionTest.Model
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

        public void Create(int offset = 0, string trName = "showLines")
        {
            var path = new List<XYZ>
            {
                new XYZ(0 + offset,0,0),
                new XYZ(5 + offset,0,0),
                new XYZ(5 + offset,5,0),
                new XYZ(10 + offset,5,0),
                new XYZ(10 + offset,0,0)
            };

            Debug.Print($"Transaction started");
            var trb = new TransactionBuilder(_doc);
            trb.Build(() => ShowLines(path), trName);
            Debug.Print($"Transaction executed");

            //trb.Build(() => ShowcCrves(path), "show curves");
        }

        public void CreateRevitTask()
        {
            var path = new List<XYZ>
            {
                new XYZ(0,0,0),
                new XYZ(5,0,0),
                new XYZ(5,5,0),
                new XYZ(10,5,0),
                new XYZ(10,0,0)
            };

            RevitTask.RunAsync(() =>
            {
                var trb = new TransactionBuilder(_doc);
                trb.Build(() => ShowLines(path), "show lines");
                //trb.Build(() => ShowcCrves(path), "show curves");
            });
        }

        private void ShowLines(List<XYZ> path)
        {
            var mcreator = new ModelCurveCreator(_doc);
            for (int i = 0; i < path.Count - 1; i++)
            {
                mcreator.Create(path[i], path[i + 1]);
            }
        }

        private void ShowcCrves(List<XYZ> path)
        {
            Reference reference = _uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Select element");
            var mEPCurve = _doc.GetElement(reference) as MEPCurve;

            var builder = new BuilderByPoints(mEPCurve, path).BuildMEPCurves().WithElbows();
        }

        /// <summary>
        /// Regenerate transaction with delay.
        /// </summary>
        /// <param name="token"></param>
        public void RegenerateDocumentWithDelay(CancellationToken token)
        {
            if (!RunDelay(token)) { return; }

            var st = new StackTrace();
            var sf = st.GetFrame(0);
            var currentMethodName = sf.GetMethod().Name;

            _doc.Regenerate();

            Debug.WriteLine($"'{currentMethodName}' executed.");
        }


        private bool RunDelay(CancellationToken token)
        {
            //inititate delay
            Debug.WriteLine("Delay started.");
            try
            {
                Task.Delay(5000, token).Wait();
            }
            catch (Exception)
            {
                if (token.IsCancellationRequested)  // проверяем наличие сигнала отмены задачи
                {
                    Debug.WriteLine($"Delay was stopped.");
                    return false;     //  выходим из метода и тем самым завершаем задачу
                }
            }
            Debug.WriteLine("Delay completed.");
            return true;
        }
    }
}
