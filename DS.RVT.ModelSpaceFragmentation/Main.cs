using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using DS.RVT.ModelSpaceFragmentation.Path;
using DS.RVT.ModelSpaceFragmentation.Lines;
using DS.RVT.ModelSpaceFragmentation.Visualization;
using DS.RVT.ModelSpaceFragmentation.Points;
using DS.RVT.ModelSpaceFragmentation.Path.CLZ;

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

        public static Element CurrentElement { get; set; }

        public void Implement()
        {
            ElementUtils elementUtils = new ElementUtils();
            CurrentElement = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            ElementSize elementSize = new ElementSize();
            elementSize.GetElementSizes(CurrentElement as MEPCurve);

            SpaceFragmentator spaceFragmentator = new SpaceFragmentator(App, Uiapp, Uidoc, Doc);
            spaceFragmentator.FragmentSpace(CurrentElement);

            PathFinder pathFinder = new PathFinder();

            List<XYZ> pathCoords = pathFinder.GetPath(
                PointsInfo.StartElemPoint, PointsInfo.EndElemPoint, SpaceFragmentator.UnpassablePoints);

            LineCreator lineCreator = new LineCreator();
            lineCreator.CreateCurves(new CurvesByPointsCreator(pathCoords));

            //CLZVisualizator.ShowCLZOfPoint(PointsInfo.StartElemPoint);
        }

    }
}
