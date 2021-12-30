using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Lines;
using DS.RVT.ModelSpaceFragmentation.Points;
using DS.RVT.ModelSpaceFragmentation.Visualization;
using DS.RVT.ModelSpaceFragmentation.Path.CLZ;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class WaveAlgorythm
    {
        public List<XYZ> PathCoords = new List<XYZ>();

        int len;

        int Ax = InputData.Ax;
        int Ay = InputData.Ay;
        int Az = InputData.Az;

        int Bx = InputData.Bx;
        int By = InputData.By;
        int Bz = InputData.Bz;

        public List<XYZ> FindPath()
        {
            if (!LaunchAlgorythm())
            {
                TaskDialog.Show("Revit", "Путь не найден!");
                return new List<XYZ>();
            }

            return PathCoords;
        }

        bool LaunchAlgorythm()
        {
            PointsCheker pointsCheker = new PointsCheker();

            List<StepPoint> StepPointsList = new List<StepPoint>();
            int x, y, z, d, k, a;

            // стартовая ячейка
            StepPoint startStepPoint = new StepPoint(Ax, Ay, Az);
            StepPoint endStepPoint = new StepPoint(Bx, By, Bz);

            CLZCretor clzCreator = new CLZCretor();
            List<StepPoint> clzPoints = clzCreator.Create(new CLZByBoders());

            //if (!pointsCheker.IsStartEndPointAvailable(startStepPoint, clzPoints) |
            //    !pointsCheker.IsStartEndPointAvailable(endStepPoint, clzPoints))
            //    return false;

            StepPointsList.Add(startStepPoint);
            Dictionary<StepPoint, int> grid = new Dictionary<StepPoint, int>
            {
                { startStepPoint, 1 }
            };

            Priority priority = new Priority();
            List<StepPoint> initialPriorityList = priority.GetPriorities();

            do
            {
                a = 0;
                for (z = 0; z < InputData.Zcount; z++)
                {
                    for (y = 0; y < InputData.Ycount; y++)
                    {
                        for (x = 0; x < InputData.Xcount; x++)
                        {
                            StepPoint currentPoint = new StepPoint(x, y, z);

                            int currentValue;
                            if (!grid.ContainsKey(currentPoint))
                                continue;
                            else
                                currentValue = grid[currentPoint];

                            //Create points cloud for next check from unpassible points
                            PointsCloud pointsCloud = new PointsCloud(SpaceFragmentator.UnpassablePoints);
                            List <StepPoint> unpassableByCLZPoints =
                                pointsCloud.GetStepPointByCenterPoint(PointConvertor.StepPointToXYZ(currentPoint));                                

                            // проходим по всем непомеченным соседям
                            for (k = 0; k < 6; ++k)
                            {
                                int ix = x + initialPriorityList[k].X,
                                    iy = y + initialPriorityList[k].Y,
                                    iz = z + initialPriorityList[k].Z;

                                if (ix >= 0 && ix < InputData.Xcount &&
                                    iy >= 0 && iy < InputData.Ycount &&
                                    iz >= 0 && iz < InputData.Zcount)
                                {
                                    StepPoint nextPoint = new StepPoint(ix, iy, iz);

                                    if (!grid.ContainsKey(nextPoint))
                                    {

                                        bool checkUnpassablePoint = pointsCheker.IsPointPassable(nextPoint);
                                        bool checkClearancePoint = pointsCheker.IsClearanceZoneAvailable(nextPoint, clzPoints, unpassableByCLZPoints);
                                        if (checkUnpassablePoint && checkClearancePoint)
                                        {
                                            // распространяем волну
                                            d = currentValue + 1;
                                            grid.Add(nextPoint, d);
                                            a++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } while (!grid.ContainsKey(endStepPoint) && a != 0);


            if (!grid.ContainsKey(endStepPoint))
                return false;

            //List<StepPoint> items = grid.Select(d => d.Key).ToList();
            //ShowPoints(items);

            grid[startStepPoint] = 0;

            // восстановление пути
            // длина кратчайшего пути из (ax, ay) в (bx, By)
            len = grid[endStepPoint];
            x = Bx;
            y = By;
            z = Bz;
            d = len;

            //List<StepPoint> BackWayPriorityList = InitialPriorityList;

            while (d >= 0)
            {
                // записываем ячейку (x, y) в путь
                WritePathPoints(x, y, z);

                d--;

                StepPoint currentPoint = new StepPoint(x, y, z);
                List<StepPoint> BackWayPriorityList = priority.GetPrioritiesByPoint(currentPoint, endStepPoint);

                for (k = 0; k < 6; ++k)
                {
                    int ix = x + BackWayPriorityList[k].X,
                        iy = y + BackWayPriorityList[k].Y,
                        iz = z + BackWayPriorityList[k].Z;

                    StepPoint nextPoint = new StepPoint(ix, iy, iz);

                    if (!grid.ContainsKey(nextPoint))
                        continue;

                    if (ix >= 0 && ix < InputData.Xcount &&
                        iy >= 0 && iy < InputData.Ycount &&
                        iz >= 0 && iz < InputData.Zcount &&
                         grid[nextPoint] == d)
                    {

                        // переходим в ячейку, которая на 1 ближе к старту
                        x += BackWayPriorityList[k].X;
                        y += BackWayPriorityList[k].Y;
                        z += BackWayPriorityList[k].Z;

                        break;
                    }
                }
            }

            if (x == Ax && y == Ay && z == Az)
            {
                return true;
            }

            return false;
        }

        void WritePathPoints(int x, int y, int z)
        {
            XYZ point = new XYZ(InputData.ZonePoint1.X + x * InputData.PointsStepF,
                InputData.ZonePoint1.Y + y * InputData.PointsStepF,
                InputData.ZonePoint1.Z + z * InputData.PointsStepF);
            PathCoords.Add(point);
        }


        void ShowPoints(List<StepPoint> points)

        {
            List<XYZ> XYZpoints = new List<XYZ>();

            foreach (StepPoint stepPoint in points)
            {
                PointConvertor pointConvertor = new PointConvertor();
                XYZ XYZpoint = PointConvertor.StepPointToXYZ(stepPoint);

                XYZpoints.Add(XYZpoint);
            }

            Visualizator.ShowPoints(new SpacePointsVisualization(XYZpoints));

        }


    }
}