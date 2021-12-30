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
        public List<XYZ> PathCoords { get; set; } = new List<XYZ>();

        int x, y, z, d, k, a, len;
        readonly int Ax = InputData.Ax;
        readonly int Ay = InputData.Ay;
        readonly int Az = InputData.Az;

        readonly int Bx = InputData.Bx;
        readonly int By = InputData.By;
        readonly int Bz = InputData.Bz;

        StepPoint StartStepPoint;
        StepPoint EndStepPoint;

        Dictionary<StepPoint, int> Grid;
        readonly Priority PriorityInstance = new Priority();

        public List<XYZ> Implement()
        {
            if (!MarkSpacePoints())
            {
                TaskDialog.Show("Revit", "Путь не найден!");
                return new List<XYZ>();
            }

            return PathCoords;
        }

        bool MarkSpacePoints()
        {
            StartStepPoint = new StepPoint(Ax, Ay, Az);
            EndStepPoint = new StepPoint(Bx, By, Bz);

            CLZCretor clzCreator = new CLZCretor();
            List<StepPoint> clzPoints = clzCreator.Create(new CLZByBoders());

            List<StepPoint> stepPointsList = new List<StepPoint>();
            stepPointsList.Add(StartStepPoint);

            Grid = new Dictionary<StepPoint, int>
            {
                { StartStepPoint, 1 }
            };

            List<StepPoint> initialPriorityList = PriorityInstance.GetPriorities();

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

                            int currentD;
                            if (!Grid.ContainsKey(currentPoint))
                                continue;
                            else
                                currentD = Grid[currentPoint];

                            //Create points cloud for next check from unpassible points
                            PointsCloud pointsCloud = new PointsCloud(SpaceFragmentator.UnpassablePoints);
                            List <StepPoint> unpassableByCLZPoints =
                                pointsCloud.GetStepPointByCenterPoint(PointConvertor.StepPointToXYZ(currentPoint));

                            PointMarker pointMarker = 
                                new PointMarker(currentPoint, clzPoints, unpassableByCLZPoints, initialPriorityList, currentD);
                            pointMarker.Mark(ref Grid, ref d, ref a);
                        }
                    }
                }
            } while (!Grid.ContainsKey(EndStepPoint) && a != 0);


            if (!Grid.ContainsKey(EndStepPoint))
                return false;

            //List<StepPoint> items = grid.Select(d => d.Key).ToList();
            //ShowPoints(items);

            GetPath();

            if (x == Ax && y == Ay && z == Az)
            {
                return true;
            }

            return false;
        }

        void GetPath()
        {
            Grid[StartStepPoint] = 0;

            // восстановление пути
            // длина кратчайшего пути из (ax, ay) в (bx, By)
            len = Grid[EndStepPoint];

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
                List<StepPoint> BackWayPriorityList = PriorityInstance.GetPrioritiesByPoint(currentPoint, EndStepPoint);

                for (k = 0; k < 6; ++k)
                {
                    int ix = x + BackWayPriorityList[k].X,
                        iy = y + BackWayPriorityList[k].Y,
                        iz = z + BackWayPriorityList[k].Z;

                    StepPoint nextPoint = new StepPoint(ix, iy, iz);

                    if (!Grid.ContainsKey(nextPoint))
                        continue;

                    if (ix >= 0 && ix < InputData.Xcount &&
                        iy >= 0 && iy < InputData.Ycount &&
                        iz >= 0 && iz < InputData.Zcount &&
                         Grid[nextPoint] == d)
                    {

                        // переходим в ячейку, которая на 1 ближе к старту
                        x += BackWayPriorityList[k].X;
                        y += BackWayPriorityList[k].Y;
                        z += BackWayPriorityList[k].Z;

                        break;
                    }
                }
            }
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