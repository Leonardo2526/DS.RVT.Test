using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            PointsCheker startEndPointsCheker = new PointsCheker(data);

            List<RefPoint> RefPointsList = new List<RefPoint>();

            int x = 0;
            int y = 0;
            int z = 0;
            int d = 0;
            int k;
            int a;

            // стартовая ячейка
            RefPoint startRefPoint = new RefPoint(Ax, Ay, Az);
            RefPoint endRefPoint = new RefPoint(Bx, By, Bz);

            RefPointsList.Add(startRefPoint);
            Dictionary<RefPoint, int> grid = new Dictionary<RefPoint, int>
            {
                { startRefPoint, 1 }
            };

            List<StepPoint> priorityList = SetPriorities();

            do
            {
                a = 0;
                for (z = 0; z < InputData.Zcount; z++)
                {
                    for (y = 0; y < InputData.Ycount; y++)
                    {
                        for (x = 0; x < InputData.Xcount; x++)
                        {
                            RefPoint currentPoint = new RefPoint(x, y, z);

                            int currentValue;
                            if (!grid.ContainsKey(currentPoint))
                                continue;
                            else
                                currentValue = grid[currentPoint];

                            // проходим по всем непомеченным соседям
                            for (k = 0; k < 6; ++k)
                            {
                                int ix = x + priorityList[k].X,
                                    iy = y + priorityList[k].Y,
                                    iz = z + priorityList[k].Z;

                                if (ix >= 0 && ix < InputData.Xcount &&
                                    iy >= 0 && iy < InputData.Ycount &&
                                    iz >= 0 && iz < InputData.Zcount)
                                {
                                    RefPoint nextPoint = new RefPoint(ix, iy, iz);

                                    if (!grid.ContainsKey(nextPoint))
                                    {

                                        bool emptyCell = startEndPointsCheker.IsCellEmpty(ix, iy, iz);
                                        if (emptyCell == true)
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
            } while (!grid.ContainsKey(endRefPoint) && a != 0);


            if (!grid.ContainsKey(endRefPoint))
                return false;

            grid[startRefPoint] = 0;

            // восстановление пути
            // длина кратчайшего пути из (ax, ay) в (bx, By)
            len = grid[endRefPoint];
            x = Bx;
            y = By;
            z = Bz;
            d = len;

            while (d >= 0)
            {
                // записываем ячейку (x, y) в путь
                WritePathPoints(x, y, z);

                d--;

                List<StepPoint> BackWaypriorityList = SetBackWayPriorities(x, y, z);

                for (k = 0; k < 6; ++k)
                {
                    int ix = x + BackWaypriorityList[k].X,
                        iy = y + BackWaypriorityList[k].Y,
                        iz = z + BackWaypriorityList[k].Z;

                    RefPoint nextPoint = new RefPoint(ix, iy, iz);

                    if (!grid.ContainsKey(nextPoint))
                        continue;

                    if (ix >= 0 && ix < InputData.Xcount &&
                        iy >= 0 && iy < InputData.Ycount &&
                        iz >= 0 && iz < InputData.Zcount &&
                         grid[nextPoint] == d)
                    {
                        // переходим в ячейку, которая на 1 ближе к старту
                        x += BackWaypriorityList[k].X;
                        y += BackWaypriorityList[k].Y;
                        z += BackWaypriorityList[k].Z;
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

        List<StepPoint> SetPriorities()
        {
            StepsPriority stepsPriority = new StepsPriority();

            if (Math.Abs(PointsInfo.StartElemPoint.X - PointsInfo.EndElemPoint.X) < 0.01)
                return stepsPriority.GetPointsList(2);
            else
                return stepsPriority.GetPointsList(1);
        }

        List<StepPoint> SetBackWayPriorities(int x, int y, int z)
        {
            StepsPriority stepsPriority = new StepsPriority();

            if (Math.Abs(PointsInfo.StartElemPoint.Z - PointsInfo.EndElemPoint.Z) < 0.01 &&
            Math.Abs(PointsInfo.StartElemPoint.Y - PointsInfo.EndElemPoint.Y) < 0.01)
            {
                if (y != By)
                    return stepsPriority.GetPointsList(2);
                else if (z != Bz)
                    return stepsPriority.GetPointsList(3);
                else
                    return stepsPriority.GetPointsList(1);
            }
            else if (Math.Abs(PointsInfo.StartElemPoint.Z - PointsInfo.EndElemPoint.Z) < 0.01 &&
            Math.Abs(PointsInfo.StartElemPoint.X - PointsInfo.EndElemPoint.X) < 0.01)
            {
                if (x != Bx)
                    return stepsPriority.GetPointsList(1);
                else if (z != Bz)
                    return stepsPriority.GetPointsList(3);
                else
                    return stepsPriority.GetPointsList(2);
            }

            if (Math.Abs(PointsInfo.StartElemPoint.X - PointsInfo.EndElemPoint.X) < 0.01 && x != Bx)
                return stepsPriority.GetPointsList(1);

            if (Math.Abs(PointsInfo.StartElemPoint.Y - PointsInfo.EndElemPoint.Y) < 0.01 && y != By)
                return stepsPriority.GetPointsList(2);

            return stepsPriority.GetPointsList(1);
        }

    }
}
