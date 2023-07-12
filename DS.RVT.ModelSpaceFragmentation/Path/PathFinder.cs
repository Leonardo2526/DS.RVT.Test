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

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        public List<XYZ> PathCoords { get; set; }

        public List<FloatPathFinderNode> AStarPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints,
            IDoublePathRequiment pathRequiment, CollisionDetectorByTrace collisionDetector, IDirectionFactory directionFactory, Vector3D stepVector)
        {
            //InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            //data.ConvertToPlane();
            var uCS2startPoint =  new Point3D(startPoint.X, startPoint.Y, startPoint.Z).Round(3);
            var uCS2endPoint =  new Point3D(endPoint.X, endPoint.Y, endPoint.Z).Round(3);

            var uCS2minPoint = new Point3D(ElementInfo.MinBoundPoint.X, ElementInfo.MinBoundPoint.Y, ElementInfo.MinBoundPoint.Z).Round(3);
            var uCS2maxPoint = new Point3D(ElementInfo.MaxBoundPoint.X, ElementInfo.MaxBoundPoint.Y, ElementInfo.MaxBoundPoint.Z).Round(3);        


            List<FloatPathFinderNode> pathNodes = FGAlgorythm.GetFloatPathByMap(
                uCS2maxPoint, uCS2minPoint, uCS2startPoint, uCS2endPoint,
                pathRequiment, collisionDetector, directionFactory, stepVector);

            return pathNodes;
        }
    }
}
