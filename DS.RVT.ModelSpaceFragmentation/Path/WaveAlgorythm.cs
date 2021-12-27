using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using DS.RVT.ModelSpaceFragmentation.Visualization;

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
                            StepsPriority stepsPriority = new StepsPriority();
                            stepsPriority.GetPointsList(1);

                            for (k = 0; k < 6; ++k)
                            {
                                int ix = x + stepsPriority.PriorityList[k].X,
                                    iy = y + stepsPriority.PriorityList[k].Y,                                    
                                    iz = z + stepsPriority.PriorityList[k].Z;

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
            } while (!grid.ContainsKey(endRefPoint)  && a != 0);


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

                StepsPriority stepsPriority = new StepsPriority();
                stepsPriority.GetPointsList(1);

                for (k = 0; k < 6; ++k)
                {
                    int ix = x + stepsPriority.PriorityList[k].X,
                        iy = y + stepsPriority.PriorityList[k].Y,                         
                        iz = z + stepsPriority.PriorityList[k].Z;

                    RefPoint nextPoint = new RefPoint(ix, iy, iz);

                    if (!grid.ContainsKey(nextPoint))
                        continue;

                    if (ix >= 0 && ix < InputData.Xcount &&
                        iy >= 0 && iy < InputData.Ycount &&
                        iz >= 0 && iz < InputData.Zcount &&
                         grid[nextPoint] == d)
                    {
                        // переходим в ячейку, которая на 1 ближе к старту
                        x += stepsPriority.PriorityList[k].X;
                        y += stepsPriority.PriorityList[k].Y;
                        z += stepsPriority.PriorityList[k].Z;
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
