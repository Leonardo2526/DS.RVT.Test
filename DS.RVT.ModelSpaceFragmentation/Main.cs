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
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils;
using System.Linq;
using System.Net;
using System.Windows;

namespace DS.RVT.ModelSpaceFragmentation
{
    class Main
    {
        public static Application App;
        public static UIDocument Uidoc;
        public static Document Doc { get; set; }
        public static UIApplication Uiapp;
        private readonly TransactionBuilder _trb;

        public Main(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
            PointsStep = 300;
            _trb = new TransactionBuilder(doc);
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

            //Get bound points
            elementUtils.GetPoints(CurrentElement, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);

            var stepVector = GetStepVector(PointsStepF, startPoint, endPoint);

            ElementInfo pointsInfo = new ElementInfo(stepVector, startPoint, endPoint);
            pointsInfo.GetPoints(CurrentElement);


            var uCS1BasePoint = new Point3D(ElementInfo.MinBoundPoint.X, ElementInfo.MinBoundPoint.Y, ElementInfo.MinBoundPoint.Z);
            var uCS2BasePoint = new Point3D(0, 0, 0);
            var pointConverter = new BestPointConverter(uCS1BasePoint, uCS2BasePoint, stepVector);

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
            var radius = new ElbowRadiusCalc(mEPCurveModel).GetRadius(90.DegToRad()).Result;
            var minDistPoint = 2 * radius + 50.MMToFeet();
            minDistPoint = Math.Ceiling(minDistPoint / PointsStepF);

            var minDistPointByte = Convert.ToByte(minDistPoint);

            //return;
            var requirement = new BestPathRequirement(0, minDistPointByte);

            //Path finding initiation
            PathFinder pathFinder = new PathFinder();
            var unpassPoints = SpaceFragmentator.UnpassablePoints ?? new List<XYZ>();
            List<PathFinderNode> path = pathFinder.AStarPath(ElementInfo.StartElemPoint,
                ElementInfo.EndElemPoint, unpassPoints, requirement, collisionDetector, pointConverter);

            if (path == null)
                TaskDialog.Show("Error", "No available path exist!");
            else
            {
                var pathCoords = Path.Refine(path);
                List<XYZ> xYZPathCoords = Path.Convert(pathCoords, pointConverter);
                //List<XYZ> xYZPathCoords = Path.PathRefinement(path, pointConverter);
                Path.ShowPath(xYZPathCoords);
                var builder = new BuilderByPoints(_baseMEPCurve, xYZPathCoords);
                var mEPElements = builder.BuildSystem(_trb);

                _trb.Build(() => Doc.Delete(_baseMEPCurve.Id), "delete baseMEPCurve");
            }

            //CLZVisualizator.ShowCLZOfPoint(PointsInfo.StartElemPoint); 
        }

        private Vector3D GetStepVector(double step, XYZ startPoint, XYZ endPoint)
        {
            var vector = endPoint - startPoint;
            //vector = vector.RoundVector();

            int xs = (int)Math.Round(vector.X / step);
            double x = xs == 0 ? step : vector.X / xs;

            int ys = (int)Math.Round(vector.Y / step);
            double y = ys == 0 ? step : vector.Y / ys;

            int zs = (int)Math.Round(vector.Z / step);
            double z = zs == 0 ? step : vector.Z / zs;

            return new Vector3D(x, y, z);
        }
    }
}
