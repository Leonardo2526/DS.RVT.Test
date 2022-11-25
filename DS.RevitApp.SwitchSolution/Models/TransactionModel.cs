using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.ModelCurveUtils;
using Revit.Async;
using System.Collections.Generic;
using System.Diagnostics;

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

        public void Create(List<XYZ> path)
        {
            Debug.Print($"Transaction showLines started");
            var trb = new TransactionBuilder<Element>(_doc);
            trb.Build(() => ShowLines(path), "showLines");
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

    }
}
