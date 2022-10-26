using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class SolidOperationTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public SolidOperationTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public void Run()
        {
            var trb = new TransactionBuilder<Element>(_doc);

            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element,"Select element");
            var element = _doc.GetElement(reference);

            var solid = ElementUtils.GetSolid(element);
            XYZ dir = ElementUtils.GetMainDirection(element);

            XYZ center = solid.ComputeCentroid();
            center.Show(_doc);

            var clonedSolid = SolidUtils.Clone(solid);
            XYZ clonedCenter = clonedSolid.ComputeCentroid();
            clonedCenter.Show(_doc);
            Plane plane = Plane.CreateByNormalAndOrigin(dir, center);
            BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(clonedSolid, plane);

            trb.Build(() => clonedSolid.ShowBB(_doc), "Show BB");
        }
    }
}
