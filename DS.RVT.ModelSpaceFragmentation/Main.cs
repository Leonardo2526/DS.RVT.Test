using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Various;
using FrancoGustavo;
using System;
using System.Collections.Generic;
using DS.RVT.ModelSpaceFragmentation;
using System.Xml.Linq;
using DS.RVT.ModelSpaceFragmentation.Points;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Elements;
using System.Windows.Media.Media3D;

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
            PointsStep = 20;
        }

        public static Element CurrentElement { get; set; }

        private MEPCurve _baseMEPCurve;

        public static int PointsStep { get; set; }

        public static double PointsStepF { get; set; }


        public void Implement()
        {

            PointsStepF = UnitUtils.Convert(PointsStep,
                                DisplayUnitType.DUT_MILLIMETERS,
                                DisplayUnitType.DUT_DECIMAL_FEET);

            ElementUtils elementUtils = new ElementUtils();
            CurrentElement = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));
            _baseMEPCurve = CurrentElement as MEPCurve;


            ElementInfo pointsInfo = new ElementInfo();
            pointsInfo.GetPoints(CurrentElement);

            var uCS1BasePoint =new Point3D(ElementInfo.MinBoundPoint.X, ElementInfo.MinBoundPoint.Y, ElementInfo.MinBoundPoint.Z);
            var uCS2BasePoint = new Point3D(0,0,0);
            var pointConverter = new PointConverter(uCS1BasePoint, uCS2BasePoint, PointsStepF);

            var (elements, linkElementsDict) = new ElementsExtractor(Doc).GetAll();
            var traceSettings = new TraceSettings();
            var collisionDetector = new CollisionDetectorByTrace(Doc, _baseMEPCurve, traceSettings, elements, linkElementsDict);
            collisionDetector.ObjectsToExclude = new List<Element>() { _baseMEPCurve };

            ElementSize elementSize = new ElementSize();
            elementSize.GetElementSizes(CurrentElement as MEPCurve);

            //SpaceFragmentator spaceFragmentator = new SpaceFragmentator(App, Uiapp, Uidoc, Doc);
            //spaceFragmentator.FragmentSpace(CurrentElement);

            var solidModel = new RevitLib.Utils.Solids.Models.SolidModel(CurrentElement.Solid());
            MEPCurveModel mEPCurveModel = new MEPCurveModel(CurrentElement as MEPCurve, solidModel);
            var radius = new ElbowRadiusCalc(mEPCurveModel).GetRadius(90.DegToRad());
            var minDistPoint = 2 * radius + 50.MMToFeet();
            minDistPoint = Math.Ceiling(minDistPoint / PointsStepF);

            var minDistPointByte = Convert.ToByte(minDistPoint);

            //return;
            var requirement = new BestPathRequirement(1, minDistPointByte);

            //Path finding initiation
            PathFinder pathFinder = new PathFinder();
            var unpassPoints = SpaceFragmentator.UnpassablePoints ?? new List<XYZ>();
            List<PathFinderNode> path = pathFinder.AStarPath(ElementInfo.StartElemPoint,
                ElementInfo.EndElemPoint, unpassPoints, requirement, collisionDetector, pointConverter);

            if (path == null)
                TaskDialog.Show("Error", "No available path exist!");
            else
            {
                List<XYZ> pathCoords = Path.PathRefinement(path, pointConverter);
                Path.ShowPath(pathCoords);
            }

            //CLZVisualizator.ShowCLZOfPoint(PointsInfo.StartElemPoint); 
        }
    }
}
