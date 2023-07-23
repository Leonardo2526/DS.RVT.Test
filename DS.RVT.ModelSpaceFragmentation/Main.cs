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
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Models;
using DS.ClassLib.VarUtils.Directions;
using System.Diagnostics;
using System.Reflection;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Geometry.Points;

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
            PointsStep = 400;
            _trb = new TransactionBuilder(doc);
        }

        public static Element CurrentElement { get; set; }

        private MEPCurve _baseMEPCurve;
        private MEPCurve _mEPCurve2;
        private readonly int _tolarance = 3;

        public static int PointsStep { get; set; }


        public static double PointsStepF { get; set; }


        public void Implement()
        {

            PointsStepF = UnitUtils.Convert(PointsStep,
                                DisplayUnitType.DUT_MILLIMETERS,
                                DisplayUnitType.DUT_DECIMAL_FEET);



            CurrentElement = new PickedElement(Uidoc, Doc).GetElement();
            _baseMEPCurve = CurrentElement as MEPCurve;


            var element2 = new PickedElement(Uidoc, Doc).GetElement();
            _mEPCurve2 = element2 as MEPCurve;

            //get basis
            var line1 = _baseMEPCurve.GetCenterLine();
            var line2 = _mEPCurve2.GetCenterLine();
            var x = line1.Direction;
            var z = x.CrossProduct(line2.Direction);
            //var z = x.CrossProduct(line2.Direction).RoundVector(_tolarance);
            var y = x.CrossProduct(z);
            //var y = x.CrossProduct(z).RoundVector(_tolarance);
            var main = new Vector3D(x.X, x.Y, x.Z).Round();
            var normal = new Vector3D(z.X, z.Y, z.Z).Round();
            var crossProduct = new Vector3D(y.X, y.Y, y.Z).Round();
            var basis = new OrthoNormBasis(main, normal, crossProduct);


            //Get bound points
            Element startElement = null; Element endElement= null;
            //var (startPoint, endPoint) = GetEdgePoints(out startElement, out endElement);
            var (startPoint, endPoint) = GetPointsByConnectors(_baseMEPCurve);
            if (startPoint is null || endPoint is null) { return; }

            OrthoBasis stepVector = GetStepBasis(PointsStepF, startPoint, endPoint, basis);
            //var stepVector = new Vector3D(PointsStepF, PointsStepF, PointsStepF);
            //var stepVector = GetStepVector(PointsStepF, startPoint, endPoint);

            ElementInfo pointsInfo = new ElementInfo(basis, startPoint, endPoint);
            pointsInfo.GetPoints(CurrentElement);


            var uCS1BasePoint = new Point3D(ElementInfo.MinBoundPoint.X, ElementInfo.MinBoundPoint.Y, ElementInfo.MinBoundPoint.Z);
            var uCS2BasePoint = new Point3D(0, 0, 0);

            var (elements, linkElementsDict) = new ElementsExtractor(Doc).GetAll();
            var traceSettings = new TraceSettings()
            {
                B = 100.MMToFeet()
            };

            double offset = _baseMEPCurve.GetMaxSize() / 2 + _baseMEPCurve.GetInsulationThickness() + traceSettings.B;

            var collisionDetector = new CollisionDetectorByTrace(Doc, _baseMEPCurve, traceSettings, elements, linkElementsDict);
            collisionDetector.ObjectsToExclude = new List<Element>() { _baseMEPCurve,};
            if(startElement is not null && 
                !collisionDetector.ObjectsToExclude.Select(obj => obj.Id).Contains(startElement.Id)) 
            { collisionDetector.ObjectsToExclude.Add(startElement); }
            if(endElement is not null &&
                !collisionDetector.ObjectsToExclude.Select(obj => obj.Id).Contains(endElement.Id)) 
            { collisionDetector.ObjectsToExclude.Add(endElement); }

            ElementSize elementSize = new ElementSize();
            elementSize.GetElementSizes(CurrentElement as MEPCurve);

            var solidModel = new RevitLib.Utils.Solids.Models.SolidModel(CurrentElement.Solid());
            MEPCurveModel mEPCurveModel = new MEPCurveModel(CurrentElement as MEPCurve, solidModel);
            var radius = new ElbowRadiusCalc(mEPCurveModel).GetRadius(90.DegToRad()).Result;
            var minDistPoint = 2 * radius + 50.MMToFeet();

            //return;
            var requirement = new BestPathRequirement(0, minDistPoint);

            var angles = new List<int> {  };
         

            IDirectionFactory directionFactory = new UserDirectionFactory();
            directionFactory.Build(basis, angles);

            //Path finding initiation
            PathFinder pathFinder = new PathFinder();
            var unpassPoints = SpaceFragmentator.UnpassablePoints ?? new List<XYZ>();
            IPointVisualisator<Point3D> pointVisualisator = new PointVisualisator(Uidoc, 100.MMToFeet(), null, true);
            List<PointPathFinderNode> path = pathFinder.AStarPath(ElementInfo.StartElemPoint,
                ElementInfo.EndElemPoint, unpassPoints, requirement, collisionDetector, directionFactory, PointsStepF, stepVector, offset, pointVisualisator);

            if (path == null || path.Count == 0)
                TaskDialog.Show("Error", "No available path exist!");
            else
            {
                var pathCoords = Path.Refine(path);
                List<XYZ> xYZPathCoords = Path.Convert(pathCoords);
                Path.ShowPath(xYZPathCoords);
                if(xYZPathCoords.Count == 0) { return; }
                //var builder = new BuilderByPoints(_baseMEPCurve, xYZPathCoords);
                //var mEPElements = builder.BuildSystem(_trb);
                return;
                _trb.Build(() => Doc.Delete(_baseMEPCurve.Id), "delete baseMEPCurve");
            }

            //CLZVisualizator.ShowCLZOfPoint(PointsInfo.StartElemPoint); 
        }

        private Vector3D GetStepVector(double step, XYZ startPoint, XYZ endPoint)
        {
            var vector = endPoint - startPoint;
            double length = vector.GetLength();
            int newCount = (int) Math.Round(length / step);
            var realStep = length / newCount;
            return new Vector3D(realStep, realStep, realStep);

            int xs = (int)Math.Round(vector.X / step);
            double x = xs == 0 ? step : vector.X / xs;

            int ys = (int)Math.Round(vector.Y / step);
            double y = ys == 0 ? step : vector.Y / ys;

            int zs = (int)Math.Round(vector.Z / step);
            double z = zs == 0 ? step : vector.Z / zs;

            return new Vector3D(x, y, z);
        }

        private OrthoBasis GetStepBasis(double step, XYZ startPoint, XYZ endPoint, OrthoNormBasis basis)
        {          
            var main = new Vector3D(basis.X.X, basis.X.Y, basis.X.Z).Round();
            var normal = new Vector3D(basis.Y.X, basis.Y.Y, basis.Y.Z).Round();
            var cross = new Vector3D(basis.Z.X, basis.Z.Y, basis.Z.Z).Round();

            var vector = endPoint - startPoint;
            var start_end_Vector = new Vector3D(vector.X, vector.Y, vector.Z).Round();

            double xs = GetStep(start_end_Vector, main, step);
            double ys = GetStep(start_end_Vector, normal, step);
            double zs = GetStep(start_end_Vector, cross, step);

            return new OrthoBasis(
                Vector3D.Multiply(main, xs),
                Vector3D.Multiply(normal, ys),
                Vector3D.Multiply(cross, zs)
                );
        }

        private double GetStep(Vector3D start_end_Vector, Vector3D orthVector, double step)
        {
            var angle = Vector3D.AngleBetween(start_end_Vector, orthVector);
            double vectorLength = start_end_Vector.Length * Math.Cos(angle.DegToRad());

            if (Math.Round(vectorLength, _tolarance) == 0)
            { return step;}

            int stepsCount = (int)Math.Floor(vectorLength / step);
            return stepsCount == 0 ? step : vectorLength / stepsCount;
        }

        private (XYZ startPoint, XYZ endPoint) GetEdgePoints(out Element startElement, out Element endElement)
        {
            startElement = null;
            endElement = null;

            var pointStrategy = new PointCreator(Uidoc);
            ConnectionPoint connectionPoint1 = pointStrategy.GetPoint(1) as ConnectionPoint;
            if (connectionPoint1.IsValid)
            {
                ConnectionPoint connectionPoint2 = pointStrategy.GetPoint(2) as ConnectionPoint;
                startElement = connectionPoint1.Element;
                endElement = connectionPoint2.Element;
                return (connectionPoint1.Point, connectionPoint2.Point);
            }

            return (null, null);
        }

        private (XYZ startPoint, XYZ endPoint) GetPointsByConnectors(MEPCurve mEPCurve)
        {
            ElementUtils.GetPoints(mEPCurve, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);
            return (startPoint, endPoint);
        }
    }
}
