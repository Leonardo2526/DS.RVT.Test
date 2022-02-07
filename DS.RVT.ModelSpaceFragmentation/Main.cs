using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Lines;
using DS.RVT.ModelSpaceFragmentation.Path;
using FrancoGustavo;
using System.Collections.Generic;

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

            //Convert path to revit coordinates
            List<XYZ> pathCoords = new List<XYZ>();
            foreach (PathFinderNode item in path)
            {
                XYZ point = new XYZ(item.X, item.Y, item.Z);
                point = point.Multiply(InputData.PointsStepF);
                point += InputData.ZonePoint1;
                pathCoords.Add(point);
            }

            //Path visualization
            LineCreator lineCreator = new LineCreator();
            lineCreator.CreateCurves(new CurvesByPointsCreator(pathCoords));

            //CLZVisualizator.ShowCLZOfPoint(PointsInfo.StartElemPoint); 
        }

    }
}
