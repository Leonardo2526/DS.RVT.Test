using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.ModelCurveUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class ConnectionPointsClient
    {

        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public ConnectionPointsClient(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = _uidoc.Document;
        }
        

        public void Run()
        {
            //Get MEPSystemModel
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element");
            MEPCurve element = _doc.GetElement(reference) as MEPCurve;
            var mEPSystemBuilder = new SimpleMEPSystemBuilder(element);
            MEPSystemModel resolvingSystemModel = mEPSystemBuilder.Build();


            var (point1, point2) = GetPointsManually();

            PathGenerator pathGenerator = new PathGenerator(point1.Value, point2.Value, 1, 1);
            List<XYZ> points = pathGenerator.Generate();

            //Show path
            var creator = new ModelCurveCreator(_doc);
            for (int i = 0; i < points.Count - 1; i++)
            {
                creator.Create(points[i], points[i + 1]);
            }
        }

        private (KeyValuePair<Element, XYZ> point1, KeyValuePair<Element, XYZ> point2) GetPointsManually()
        {
            var selector = new ConnectionElementSelector(_uidoc);
            var element1 = selector.Select("Element1");
            var element2 = selector.Select("Element2");

            return (element1, element2);
        }

        private (KeyValuePair<Element, XYZ> point1, KeyValuePair<Element, XYZ> point2) GetPoints()
        {
            var selector = new ConnectionElementSelector(_uidoc);
            var element1 = selector.Select("Element1");
            var element2 = selector.Select("Element2");

            return (element1, element2);
        }
    }
}
