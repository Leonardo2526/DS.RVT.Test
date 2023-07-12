using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils.Various;
using DS.RVT.ModelSpaceFragmentation.Lines;
using FrancoGustavo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DS.RVT.ModelSpaceFragmentation
{
    static class Path
    {
        public static List<XYZ> PathRefinement(List<PathFinderNode> path, IPointConverter pointConverter)
        {

            //Convert path to revit coordinates                
            List<XYZ> pathCoords = new List<XYZ>();
            //pathCoords.Add(ElementInfo.StartElemPoint);

            foreach (PathFinderNode item in path)
            {
                Point3D point = new Point3D(item.X, item.Y, item.Z);
                Point3D ucs1Point = pointConverter.ConvertToUCS1(point);
                var xYZ = new XYZ(ucs1Point.X, ucs1Point.Y, ucs1Point.Z);
                pathCoords.Add(xYZ);
            }

            return pathCoords;

        }

        public static List<Point3D> Refine(List<FloatPathFinderNode> path)
        {
            List<Point3D> points = new List<Point3D>();

            var firstNode = path[0];
            Point3D basePoint = new(firstNode.X, firstNode.Y, firstNode.Z);
            Vector3D baseDir = firstNode.Dir;

            for (int i = 1; i < path.Count; i++)
            {
                var currentNode = path[i];
                var currentPoint = new Point3D(currentNode.X, currentNode.Y, currentNode.Z);
                var currentDir = path[i].Dir;
                if (currentDir.Length != 0)
                { currentDir.Normalize(); }

                if(baseDir.Length == 0 || !currentDir.IsAlmostEqualTo(baseDir))
                {
                    points.Add(basePoint);
                    baseDir = currentDir;
                }
                basePoint = currentPoint; 
            }

            points.Add(basePoint);

            return points;
        }

        public static List<XYZ> Convert(List<Point3D> path, IPointConverter pointConverter)
        {
            //Convert path to revit coordinates                
            List<XYZ> pathCoords = new List<XYZ>();
            //pathCoords.Add(ElementInfo.StartElemPoint);

            foreach (var point in path)
            {
                Point3D ucs1Point = point;
                //Point3D ucs1Point = pointConverter.ConvertToUCS1(point);
                var xYZ = new XYZ(ucs1Point.X, ucs1Point.Y, ucs1Point.Z);
                pathCoords.Add(xYZ);
            }

            return pathCoords;
        }


        public static List<XYZ> OldPathRefinement(List<PathFinderNode> path)
        {

            //Convert path to revit coordinates                
            List<XYZ> pathCoords = new List<XYZ>();
            pathCoords.Add(ElementInfo.StartElemPoint);

            foreach (PathFinderNode item in path)
            {
                XYZ point = new XYZ(item.PX, item.PY, item.PZ);
                XYZ pathpoint = ConvertToModel(point);

                double xx = Math.Abs(pathCoords[pathCoords.Count - 1].X - pathpoint.X);
                double xy = Math.Abs(pathCoords[pathCoords.Count - 1].Y - pathpoint.Y);
                double xz = Math.Abs(pathCoords[pathCoords.Count - 1].Z - pathpoint.Z);

                if (xx > 0.01 || xy > 0.01 || xz > 0.01)
                    pathCoords.Add(pathpoint);

            }

            //check min distance
            double minDist = 1.5 * ElementSize.ElemDiameterF;
            if (Math.Abs(pathCoords[pathCoords.Count - 2].X - pathCoords[pathCoords.Count - 1].X) <= minDist &&
                Math.Abs(pathCoords[pathCoords.Count - 2].Y - pathCoords[pathCoords.Count - 1].Y) <= minDist &&
                Math.Abs(pathCoords[pathCoords.Count - 2].Z - pathCoords[pathCoords.Count - 1].Z) <= minDist)
                pathCoords.RemoveAt(pathCoords.Count - 2);

            return pathCoords;

        }

        public static void ShowPath(List<XYZ> pathCoords)
        {
            //pathCoords.Add(ElementInfo.EndElemPoint);

            //Show path with lines
            LineCreator lineCreator = new LineCreator();
            lineCreator.CreateCurves(new CurvesByPointsCreator(pathCoords));

            //MEP system changing
            //RevitUtils.MEP.PypeSystem pypeSystem = new RevitUtils.MEP.PypeSystem(Main.Uiapp, Main.Uidoc, Main.Doc, Main.CurrentElement);
            //pypeSystem.CreatePipeSystem(pathCoords);
        }


        private static XYZ ConvertToModel(XYZ point)
        {
            XYZ newpoint = point.Multiply(InputData.PointsStepF);
            newpoint += InputData.ZonePoint1;

            return newpoint;
        }
    }
}
