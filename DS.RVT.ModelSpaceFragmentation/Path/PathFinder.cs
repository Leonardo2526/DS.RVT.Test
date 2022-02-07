using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Path;
using System;
using System.Collections.Generic;
using FrancoGustavo;
using DSUtils;
using DSUtils.GridMap;
using DS.RVT.ModelSpaceFragmentation.CLZ;
using Location = DSUtils.Location;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        public List<XYZ> PathCoords { get; set; }

        public List<PathFinderNode> AStarPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints)
        {
            InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            data.ConvertToPlane();

            MapCreator map = new MapCreator();
            map.Start = new Location(InputData.Ax, InputData.Ay, InputData.Az);
            map.Goal = new Location(InputData.Bx, InputData.By, InputData.Bz);

            map.Matrix = new int[InputData.Xcount, InputData.Ycount, InputData.Zcount];

            foreach (StepPoint unpass in InputData.UnpassStepPoints)
                map.Matrix[unpass.X, unpass.Y, unpass.Z] = 1;
           
            List<PathFinderNode> pathNodes = FrancoGustavo.FGAlgorythm.GetPathByMap(map);

            //AStar aStar = new AStar(start, goal, maxGridPoint, unpassablelocations);


            //AStar.WidthClearanceRCS = CLZInfo.WidthClearanceRCS;
            //AStar.HeightClearanceRCS = CLZInfo.HeightClearanceRCS;
            //AStar.WidthClearanceRCS = 0;
            //AStar.HeightClearanceRCS = 0;

            //List<Location> AStarPath = ConvertPath(pathNodes);

            return pathNodes;
        }
    }
}
