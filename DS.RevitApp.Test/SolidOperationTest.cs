using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;

namespace DS.RevitApp.Test
{
    internal class SolidOperationTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private TransactionBuilder<Element> _trb;

        public SolidOperationTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public void Run()
        {
            _trb = new TransactionBuilder<Element>(_doc);

            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var element = _doc.GetElement(reference);
            MEPCurve mEPCurve = element is MEPCurve ? element as MEPCurve : null;

            XYZ pickPoint = _uiDoc.Selection.PickPoint(ObjectSnapTypes.Points, "Select startPoint");
            XYZ startPoint = MEPCurveUtils.GetLine(mEPCurve).Project(pickPoint).XYZPoint;
            startPoint.Show(_doc);

            pickPoint = _uiDoc.Selection.PickPoint(ObjectSnapTypes.Points, "Select endPoint");
            XYZ endPoint = MEPCurveUtils.GetLine(mEPCurve).Project(pickPoint).XYZPoint;
            endPoint.Show(_doc);

            _uiDoc.RefreshActiveView();

            if (mEPCurve is not null)
            {
                double offset = 50.mmToFyt2();
                Solid offsetSolid = mEPCurve.GetOffsetSolid(offset, startPoint, endPoint);

                _trb.Build(() => offsetSolid.ShowEdges(_doc), "show solid");
            }
            return;
        }
    }
}
