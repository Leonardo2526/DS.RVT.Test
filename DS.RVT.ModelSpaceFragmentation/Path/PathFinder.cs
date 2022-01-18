using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Path;
using System;
using System.Collections.Generic;
using AStarAlgorythm;
using Location = DS.System.Location;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        public List<XYZ> PathCoords { get; set; }

        public static ISpacePointsIterator spacePointsIterator { get; set; }

        public List<XYZ> GetPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints)
        {
            InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            data.ConvertToPlane();

            List<XYZ> pathCoords1 = new List<XYZ>();
            List<XYZ> pathCoords2 = new List<XYZ>();
            int len1 = 1000, len2 = 1000;

            WaveAlgorythm waveAlgorythm;
            if (InputData.Ax == InputData.Bx)
            {
                spacePointsIterator = new IteratorByYZPlane();
                waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
                pathCoords1 = waveAlgorythm.Implement();

                if (pathCoords1.Count > 0)
                    return PathCoords=pathCoords1;
            }
            else if (InputData.Ay == InputData.By)
            {
                spacePointsIterator = new IteratorByXZPlane();
                waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
                pathCoords1 = waveAlgorythm.Implement();

                if (pathCoords1.Count > 0)
                    return PathCoords=pathCoords1;
            }

            if (InputData.Az == InputData.Bz)
            {
                spacePointsIterator = new IteratorByXYPlane();
                waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
                pathCoords2 = waveAlgorythm.Implement();

                if (pathCoords2.Count > 0)
                    return PathCoords=pathCoords2;
            }

            if (pathCoords1.Count == 0 && pathCoords2.Count == 0)
            {
                spacePointsIterator = new IteratorBy3D();
                waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
                PathCoords = waveAlgorythm.Implement();

                if (PathCoords.Count == 0)
                    TaskDialog.Show("Revit", "Путь не найден!");

                return PathCoords;
            }



            //if (len1 > len2)
            //    PathCoords = pathCoords2;
            //else
            //    PathCoords = pathCoords1;


            return PathCoords;
        }

        public List<Location> AStarPath()
        {
            int z = 0;

            Location start = new Location(0, 0, 0);
            Location goal = new Location(9, 9, 9);
            Location maxGridPoint = new Location(10, 10, 10);

            int middleY = (int)Math.Round((double)(maxGridPoint.Y / 2));
            int smesh = 2;

            List<Location> unpassablelocations = new List<Location>();
            for (var x = 1; x < 4; x++)
            {
                for (var y = 0; y < middleY + smesh; y++)
                    unpassablelocations.Add(new Location(x, y, z));
            }

            for (var x = 6; x < 9; x++)
            {
                for (var y = middleY - smesh; y < maxGridPoint.Y; y++)
                    unpassablelocations.Add(new Location(x, y, z));
            }


            AStar aStar = new AStar(start, goal, maxGridPoint, unpassablelocations);
            //List<Location> AStarPath = aStar.GetPathWithVisualizition();
            List<Location> AStarPath = aStar.GetPath();

            return AStarPath;
        }

    }
}
