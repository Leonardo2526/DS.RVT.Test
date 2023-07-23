using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation;
using DS.PathSearch.GridMap;
using FrancoGustavo;
using System.Collections.Generic;
using Location = DS.PathSearch.Location;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Various;
using System.Windows.Media.Media3D;
using DS.ClassLib.VarUtils.Points;
using DS.ClassLib.VarUtils.Directions;
using FrancoGustavo.Algorithm;
using System.Threading;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Geometry.Points;
using System;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        private int _tolerance = 3;

        public List<XYZ> PathCoords { get; set; }

        public List<PointPathFinderNode> AStarPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints,
            IDoublePathRequiment pathRequiment, CollisionDetectorByTrace collisionDetector, IDirectionFactory directionFactory,
            double step,
            OrthoBasis stepBasis, double offset,
            IPointVisualisator<Point3D> pointVisualisator = null)
        {
            //InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            //data.ConvertToPlane();
            var uCS2startPoint = new Point3D(startPoint.X, startPoint.Y, startPoint.Z);
            var uCS2endPoint = new Point3D(endPoint.X, endPoint.Y, endPoint.Z);

            var uCS2minPoint = new Point3D(ElementInfo.MinBoundPoint.X, ElementInfo.MinBoundPoint.Y, ElementInfo.MinBoundPoint.Z).Round(_tolerance);
            var uCS2maxPoint = new Point3D(ElementInfo.MaxBoundPoint.X, ElementInfo.MaxBoundPoint.Y, ElementInfo.MaxBoundPoint.Z).Round(_tolerance);


            List<PointPathFinderNode> path = new List<PointPathFinderNode>();

            var negateBasis = stepBasis.Negate();
            var orths = new List<Vector3D>() { stepBasis.X, stepBasis.Y, stepBasis.Z, negateBasis.X, negateBasis.Y, negateBasis.Z };

            var mHEstimate = 10;
            var fractPrec = 5;

            HeuristicFormula formula = GetFormula(stepBasis);
            //HeuristicFormula formula = HeuristicFormula.DiagonalShortCut;
            //HeuristicFormula formula = HeuristicFormula.Manhattan;

            var nodeBuilder = new NodeBuilder(formula, mHEstimate, uCS2startPoint, uCS2endPoint, step, stepBasis, orths, collisionDetector, offset, false, false);
            var mPathFinder = new TestPathFinder(uCS2maxPoint, uCS2minPoint, pathRequiment, collisionDetector, nodeBuilder, fractPrec, pointVisualisator)
            {
                PunishAngles = new List<int>() {  },
                TokenSource = new CancellationTokenSource()
                //TokenSource = new CancellationTokenSource(10000)
            };

            var userDirectionFactory = directionFactory as UserDirectionFactory;
            if (userDirectionFactory == null) { return null; }

            var dirs1 = userDirectionFactory.Plane1_Directions;
            var dirs2 = userDirectionFactory.Plane2_Directions;
            var alldirs = userDirectionFactory.Directions;

            var pathDirs = dirs2;

            var moveVectors = new List<Vector3D>();
            foreach (var dir in pathDirs)
            {
                var (vector, angle) = dir.GetWithMinAngle(orths);
                var length = vector.Length / Math.Cos(angle.DegToRad());
                var v = Vector3D.Multiply(dir, length);
                //var v = new Vector3D(dir.X * projStep.X , dir.Y * projStep.Y, dir.Z * projStep.Z);
                moveVectors.Add(v);
            }

            path = mPathFinder.FindPath(
                   uCS2startPoint,
                    uCS2endPoint, pathDirs);
            if (path != null)
                return path;

            return path;
        }

        private HeuristicFormula GetFormula(OrthoBasis stepBasis)
        {
            HeuristicFormula formula;

            var main = new XYZ(stepBasis.X.X, stepBasis.X.Y, stepBasis.X.Z).RoundVector(_tolerance);
            if (
                 XYZUtils.Collinearity(main, XYZ.BasisX) ||
                 XYZUtils.Collinearity(main, XYZ.BasisY) ||
                 XYZUtils.Collinearity(main, XYZ.BasisZ))
            {
                formula = HeuristicFormula.Manhattan;
            }
            else
            {
                formula = HeuristicFormula.DiagonalShortCut;
            }

            return formula;
        }
    }
}
