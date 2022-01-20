using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Path;
using System;
using System.Collections.Generic;
using AStarAlgorythm;
using Location = DS.System.Location;
using DS.RVT.ModelSpaceFragmentation.CLZ;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        public List<XYZ> PathCoords { get; set; }

        //public static ISpacePointsIterator spacePointsIterator { get; set; }

        //public List<XYZ> GetPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints)
        //{
        //    InputData data = new InputData(startPoint, endPoint, unpassablePoints);
        //    data.ConvertToPlane();

        //    List<XYZ> pathCoords1 = new List<XYZ>();
        //    List<XYZ> pathCoords2 = new List<XYZ>();
        //    int len1 = 1000, len2 = 1000;

        //    WaveAlgorythm waveAlgorythm;
        //    if (InputData.Ax == InputData.Bx)
        //    {
        //        spacePointsIterator = new IteratorByYZPlane();
        //        waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
        //        pathCoords1 = waveAlgorythm.Implement();

        //        if (pathCoords1.Count > 0)
        //            return PathCoords=pathCoords1;
        //    }
        //    else if (InputData.Ay == InputData.By)
        //    {
        //        spacePointsIterator = new IteratorByXZPlane();
        //        waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
        //        pathCoords1 = waveAlgorythm.Implement();

        //        if (pathCoords1.Count > 0)
        //            return PathCoords=pathCoords1;
        //    }

        //    if (InputData.Az == InputData.Bz)
        //    {
        //        spacePointsIterator = new IteratorByXYPlane();
        //        waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
        //        pathCoords2 = waveAlgorythm.Implement();

        //        if (pathCoords2.Count > 0)
        //            return PathCoords=pathCoords2;
        //    }

        //    if (pathCoords1.Count == 0 && pathCoords2.Count == 0)
        //    {
        //        spacePointsIterator = new IteratorBy3D();
        //        waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
        //        PathCoords = waveAlgorythm.Implement();

        //        if (PathCoords.Count == 0)
        //            TaskDialog.Show("Revit", "Путь не найден!");

        //        return PathCoords;
        //    }



        //    //if (len1 > len2)
        //    //    PathCoords = pathCoords2;
        //    //else
        //    //    PathCoords = pathCoords1;


        //    return PathCoords;
        //}

        public List<Location> AStarPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints)
        {
            InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            data.ConvertToPlane();

            Location start = new Location(InputData.Ax, InputData.Ay, InputData.Az); 
            Location goal = new Location(InputData.Bx, InputData.By, InputData.Bz);
            Location maxGridPoint = new Location(InputData.Xcount, InputData.Ycount, InputData.Zcount);

            List<Location> unpassablelocations = new List<Location>();

            foreach (StepPoint unpass in InputData.UnpassStepPoints)
            {
                Location point = new Location(unpass.X, unpass.Y, unpass.Z);
                unpassablelocations.Add(point);
            }

            AStar aStar = new AStar(start, goal, maxGridPoint, unpassablelocations);


            AStar.WidthClearanceRCS = CLZInfo.WidthClearanceRCS;
            AStar.HeightClearanceRCS = CLZInfo.HeightClearanceRCS;
            //AStar.WidthClearanceRCS = 0;
            //AStar.HeightClearanceRCS = 0;

            List<Location> AStarPath = aStar.GetPath();

            return AStarPath;
        }

    }
}
