using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FrancoGustavo;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class Main
    {
        public static Application App;
        public static UIDocument Uidoc;
        public static Document Doc { get; set; }
        public static UIApplication Uiapp;

        public Main(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }

        public static Element CurrentElement { get; set; }

        public static int PointsStep { get; } = 50;

        public static double PointsStepF { get; set; }


        public void Implement()
        {
            PointsStepF = UnitUtils.Convert(PointsStep,
                                DisplayUnitType.DUT_MILLIMETERS,
                                DisplayUnitType.DUT_DECIMAL_FEET);

            ElementUtils elementUtils = new ElementUtils();
            CurrentElement = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            ElementSize elementSize = new ElementSize();
            elementSize.GetElementSizes(CurrentElement as MEPCurve);

            SpaceFragmentator spaceFragmentator = new SpaceFragmentator(App, Uiapp, Uidoc, Doc);
            spaceFragmentator.FragmentSpace(CurrentElement);

            //Path finding initiation
            PathFinder pathFinder = new PathFinder();
            List<PathFinderNode> path = pathFinder.AStarPath(ElementInfo.StartElemPoint,
                ElementInfo.EndElemPoint, SpaceFragmentator.UnpassablePoints);

            if (path == null)
                TaskDialog.Show("Error", "No available path exist!");
            else
            {
                List<XYZ> pathCoords = Path.PathRefinement(path);
                Path.ShowPath(pathCoords);
            }

            //CLZVisualizator.ShowCLZOfPoint(PointsInfo.StartElemPoint); 
        }
    }
}
