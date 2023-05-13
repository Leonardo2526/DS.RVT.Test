using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation;
using DS.PathSearch.GridMap;
using FrancoGustavo;
using System.Collections.Generic;
using Location = DS.PathSearch.Location;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Various;
using System.Windows.Media.Media3D;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        public List<XYZ> PathCoords { get; set; }

        public List<PathFinderNode> AStarPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints, 
            IPathRequiment pathRequiment, CollisionDetectorByTrace collisionDetector, PointConverter pointConverter)
        {
            InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            data.ConvertToPlane();
            var uCS2startPoint =  pointConverter.ConvertToUSC2(new Point3D(startPoint.X, startPoint.Y, startPoint.Z));
            var uCS2endPoint =  pointConverter.ConvertToUSC2(new Point3D(endPoint.X, endPoint.Y, endPoint.Z));

            var uCS2startPointInd = pointConverter.Round(uCS2startPoint);
            var uCS2endPointInd = pointConverter.Round(uCS2endPoint);

            IMap map = new MapCreator();
            map.Start = new Location((int)uCS2startPointInd.X, (int)uCS2startPointInd.Y, (int)uCS2startPointInd.Z);
            map.Goal = new Location((int)uCS2endPointInd.X, (int)uCS2endPointInd.Y, (int)uCS2endPointInd.Z);

            var uCS1BasePoint =new Point3D(ElementInfo.MaxBoundPoint.X, ElementInfo.MaxBoundPoint.Y, ElementInfo.MaxBoundPoint.Z);
            var uCS2maxPoint = pointConverter.ConvertToUSC2(new Point3D(uCS1BasePoint.X, uCS1BasePoint.Y, uCS1BasePoint.Z));
            var uCS2maxPointInt = pointConverter.Round(uCS2maxPoint);

            map.Matrix = new int[(int)uCS2maxPointInt.X, (int)uCS2maxPointInt.Y, (int)uCS2maxPointInt.Z];

            foreach (StepPoint unpass in InputData.UnpassStepPoints)
                map.Matrix[unpass.X, unpass.Y, unpass.Z] = 1;

            List<PathFinderNode> pathNodes = FGAlgorythm.GetPathByMap(map, pathRequiment, collisionDetector, pointConverter);

            return pathNodes;
        }
    }
}
