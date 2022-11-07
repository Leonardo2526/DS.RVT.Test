using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.ModelCurveUtils;
using Revit.Async;
using System.Collections.Generic;

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

            var trb = new TransactionBuilder<Element>(_doc);
            trb.Build(() => ShowLines(path), trName);
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
                var trb = new TransactionBuilder<Element>(_doc);
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

    }
}
