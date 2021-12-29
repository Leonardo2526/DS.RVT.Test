using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Points;
using System;
using System.Collections.Generic;

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

        readonly InputData data;
        public WaveAlgorythm(InputData inputData)
        {
            data = inputData;
        }

        public static int InitialPriority { get; set; }

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

            int x = 0;
            int y = 0;
            int z = 0;
            int d = 0;
            int k;
            int a;

            // стартовая ячейка
            StepPoint startStepPoint = new StepPoint(Ax, Ay, Az);
            StepPoint endStepPoint = new StepPoint(Bx, By, Bz);

            PointClearanceZone pointClearanceZone = new PointClearanceZone();
            List<StepPoint> clearancePoints = pointClearanceZone.Create(new ZoneByCircle());

            if (!pointsCheker.IsStartEndPointAvailable(startStepPoint, clearancePoints) |
                !pointsCheker.IsStartEndPointAvailable(endStepPoint, clearancePoints))
                return false;

            StepPointsList.Add(startStepPoint);
            Dictionary<StepPoint, int> grid = new Dictionary<StepPoint, int>
            {
                { startStepPoint, 1 }
            };

            Priority priority = new Priority();
            List<StepPoint> initialPriorityList = priority.GetPriorities();

            InitialPriority = StepsPriority.CurrentPriority;

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
                                        bool checkClearancePoint = pointsCheker.IsClearanceZoneAvailable(nextPoint, clearancePoints);
                                        if (checkUnpassablePoint && checkClearancePoint)
                                        //if (checkUnpassablePoint)
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

            grid[startStepPoint] = 0;

            // восстановление пути
            // длина кратчайшего пути из (ax, ay) в (bx, By)
            len = grid[endStepPoint];
            x = Bx;
            y = By;
            z = Bz;
            d = len;

            while (d >= 0)
            {
                // записываем ячейку (x, y) в путь
                WritePathPoints(x, y, z);

                d--;

                StepPoint currentPoint = new StepPoint(x, y, z);
                List<StepPoint> BackWayPriorityList = priority.GetPrioritiesByPointOld(currentPoint, endStepPoint);

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

                        pointClearanceZone.Create(new ZoneByCircle());

                        //PointConvertor pointConvertor = new PointConvertor();
                        //pointClearanceZone.ShowPoints(pointConvertor.StepPointToXYZ(nextPoint));
                        //////bool clearanceAvailable = IsClearanceAvailable(nextPoint, BackWaypriorityList[k], grid);
                        //if (!clearanceAvailable)
                        //    continue;

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

      


       
    }
}