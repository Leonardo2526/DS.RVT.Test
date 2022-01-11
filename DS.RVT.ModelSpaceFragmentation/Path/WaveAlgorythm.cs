﻿using Autodesk.Revit.DB;
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

        int x, y, z, d, k, len;

        public List<XYZ> Implement()
        {
            if (!IfPathExists())
            {
                TaskDialog.Show("Revit", "Путь не найден!");
                return new List<XYZ>();
            }

            return PathCoords;
        }

        bool IfPathExists()
        {
            if (!PointsMarkerIterator.IfWaveReachedEndPoint())
                return false;
           
            //List<StepPoint> items = grid.Select(d => d.Key).ToList();
            //ShowPoints(items);

            GetPath();

            if (x == InputData.Ax && y == InputData.Ay && z == InputData.Az)
            {
                return true;
            }

            return false;
        }

        void GetPath()
        {
            PointsMarkerIterator.Grid[PointsMarkerIterator.StartStepPoint] = 0;

            // восстановление пути
            // длина кратчайшего пути из (ax, ay) в (bx, By)
            len = PointsMarkerIterator.Grid[PointsMarkerIterator.EndStepPoint];

            x = InputData.Bx;
            y = InputData.By;
            z = InputData.Bz;
            d = len;

            while (d >= 0)
            {
                // записываем ячейку (x, y) в путь
                WritePathPoints(x, y, z);

                d--;

                StepPoint currentPoint = new StepPoint(x, y, z);
                Priority PriorityInstance = new Priority();
                List<StepPoint> BackWayPriorityList = 
                    PriorityInstance.GetPrioritiesByPoint(currentPoint, PointsMarkerIterator.EndStepPoint);

                for (k = 0; k < 6; ++k)
                {
                    int ix = x + BackWayPriorityList[k].X,
                        iy = y + BackWayPriorityList[k].Y,
                        iz = z + BackWayPriorityList[k].Z;

                    StepPoint nextPoint = new StepPoint(ix, iy, iz);

                    if (!PointsMarkerIterator.Grid.ContainsKey(nextPoint))
                        continue;

                    if (ix >= 0 && ix < InputData.Xcount &&
                        iy >= 0 && iy < InputData.Ycount &&
                        iz >= 0 && iz < InputData.Zcount &&
                         PointsMarkerIterator.Grid[nextPoint] == d)
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