using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitApp.Test.ConnectionPointService.SearchersModel;
using DS.RevitApp.Test.PathFinders;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.ModelCurveUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class ConnectionPointsClient
    {

        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly IPathFinder _pathFinder;
        private MEPSystemModel _mEPSystemModel;

        public ConnectionPointsClient(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = _uidoc.Document;
            _pathFinder = new SimplePathFinder(1, 1);
        }
        

        public void Run()
        {
            //Get MEPSystemModel
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element");
            MEPCurve element = _doc.GetElement(reference) as MEPCurve;

            var mEPSystemBuilder = new SimpleMEPSystemBuilder(element);
            _mEPSystemModel = mEPSystemBuilder.Build();

            //var (point1, point2) = GetPointsManually();
            //List<XYZ> points = pathGenerator.FindPath(point1.Value, point2.Value);
            List<XYZ>  path = GetPath();

            //Show path
            var creator = new ModelCurveCreator(_doc);
            for (int i = 0; i < path.Count - 1; i++)
            {
                creator.Create(path[i], path[i + 1]);
            }
        }

        private (KeyValuePair<Element, XYZ> point1, KeyValuePair<Element, XYZ> point2) GetPointsManually()
        {
            var selector = new ConnectionElementSelector(_uidoc);
            var element1 = selector.Select("Element1");
            var element2 = selector.Select("Element2");

            return (element1, element2);
        }

        private List<XYZ> GetPath()
        {
            var client = new ConnectionSearchClient(_pathFinder, null, _mEPSystemModel);
            var (Point1, Point2) = client.GetConnectionPoints();

            return client.Path;
        }
    }
}
