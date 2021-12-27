using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using DS.RVT.ModelSpaceFragmentation.Path;
using DS.RVT.ModelSpaceFragmentation.Lines;

namespace DS.RVT.ModelSpaceFragmentation
{
    class Main
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        public static Document Doc { get; set; }
        readonly UIApplication Uiapp;

        public Main(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }


        public void Implement()
        {
            ElementUtils elementUtils = new ElementUtils();
            Element element = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            SpaceFragmentator spaceFragmentator = new SpaceFragmentator(App, Uiapp, Uidoc, Doc);
            spaceFragmentator.FragmentSpace(element);

            PathFinder pathFinder = new PathFinder();
            elementUtils.GetPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);

            XYZ newstartPoint = new XYZ(startPoint.X-2, startPoint.Y, startPoint.Z);
            XYZ newendPoint = new XYZ(endPoint.X + 2, endPoint.Y, endPoint.Z);

            List<XYZ> pathCoords = pathFinder.GetPath(newstartPoint, newendPoint, spaceFragmentator.UnpassablePoints);

            LineCreator lineCreator = new LineCreator();
            lineCreator.CreateCurves(new CurvesByPointsCreator(pathCoords));
        }

    }
}
