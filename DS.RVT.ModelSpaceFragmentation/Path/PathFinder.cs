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

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        public List<XYZ> PathCoords { get; set; }

        public List<PointPathFinderNode> AStarPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints,
            IDoublePathRequiment pathRequiment, CollisionDetectorByTrace collisionDetector, IDirectionFactory directionFactory, Vector3D stepVector)
        {
            //InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            //data.ConvertToPlane();
            var uCS2startPoint = new Point3D(startPoint.X, startPoint.Y, startPoint.Z);
            var uCS2endPoint = new Point3D(endPoint.X, endPoint.Y, endPoint.Z);

            var uCS2minPoint = new Point3D(ElementInfo.MinBoundPoint.X, ElementInfo.MinBoundPoint.Y, ElementInfo.MinBoundPoint.Z).Round(3);
            var uCS2maxPoint = new Point3D(ElementInfo.MaxBoundPoint.X, ElementInfo.MaxBoundPoint.Y, ElementInfo.MaxBoundPoint.Z).Round(3);


            List<PointPathFinderNode> path = new List<PointPathFinderNode>();

            var mHEstimate = 100;
            var fractPrec = 7;
            var nodeBuilder = new NodeBuilder(HeuristicFormula.Manhattan, mHEstimate, uCS2startPoint, uCS2endPoint, stepVector, true, fractPrec);
            var mPathFinder = new TestPathFinder(uCS2maxPoint, uCS2minPoint, pathRequiment, collisionDetector, stepVector, nodeBuilder, fractPrec)
            {
                PunishAngles = new List<int>() { 90 },
                TokenSource = new CancellationTokenSource(10000)
            };

            var userDirectionFactory = directionFactory as UserDirectionFactory;
            if (userDirectionFactory == null) { return null; }

            var YZdirs = userDirectionFactory.Plane1_Directions;
            path = mPathFinder.FindPath(
                   uCS2startPoint,
                    uCS2endPoint, YZdirs);
            if (path != null)
                return path;

            return path;
        }
    }
}
