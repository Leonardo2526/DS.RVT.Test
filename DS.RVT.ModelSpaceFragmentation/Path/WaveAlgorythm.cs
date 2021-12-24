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
        int Bx = InputData.Bx;
        int Ay = InputData.Ay;
        int By = InputData.By;

        readonly InputData data;
        public WaveAlgorythm(InputData inputData)
        {
            data = inputData;
        }

        // смещения, соответствующие соседям ячейки слева, сверху, справа, снизу
        readonly List<int> Dx = new List<int>
            {
                -1,
                0,
                1,
                0
            };
        readonly List<int> Dy = new List<int>
            {
                0,
                1,
                0,
                -1
            };

        public List<XYZ> FindPath()
        {
            if (!lee())
            {
                TaskDialog.Show("Revit", "Путь не найден!");
                return new List<XYZ>();
            }

            return PathCoords;
        }      

        bool lee()
        {
            PointsCheker startEndPointsCheker = new PointsCheker(data);

            //рабочее поле
            int[,] grid = new int[InputData.W, InputData.H];

            int x = 0;
            int y = 0;
            int d = 0;
            int k;
            int a;

            ////Check start cell
            //if (IsStartCellEmpty() == false)
            //    return false;
            ////Check end cell
            //if (IsEndCellEmpty() == false)
            //    return false;

            // стартовая ячейка
            grid[Ax, Ay] = 1;

            do
            {
                a = 0;
                for (y = 0; y < InputData.H; y++)
                {
                    for (x = 0; x < InputData.W; x++)
                    {
                        if (grid[x, y] == 0)
                            continue;
                        // проходим по всем непомеченным соседям
                        for (k = 0; k < 4; ++k)                    
                        {
                            int iy = y + Dy[k], ix = x + Dx[k];
                            if (iy >= 0 && iy < InputData.H && ix >= 0 && ix < InputData.W)
                            {
                                if (grid[ix, iy] == 0)
                                {

                                    bool emptyCell = startEndPointsCheker.IsCellEmpty(ix, iy);
                                    if (emptyCell == true)
                                    {
                                        // распространяем волну
                                        d = grid[x, y] + 1;
                                        grid[ix, iy] = d;
                                        a++;
                                    }
                                }
                            }
                        }
                    }
                }
            } while (grid[Bx, By] == 0 && a != 0);


            if (grid[Bx, By] == 0)
                return false;

            grid[Ax, Ay] = 0;

            // восстановление пути
            // длина кратчайшего пути из (ax, ay) в (bx, By)
            len = grid[Bx, By];            
            x = Bx;
            y = By;
            d = len;

            while (d >= 0)
            {
                InputData.Px[d] = x;
                InputData.Py[d] = y;

                // записываем ячейку (x, y) в путь
                WritePathPoints(x, y);

                d--;
                for (k = 0; k < 4; ++k)
                {
                    int iy = y + Dy[k], ix = x + Dx[k];
                    if (iy >= 0 && iy < InputData.H && ix >= 0 && ix < InputData.W &&
                         grid[ix, iy] == d)
                    {
                        // переходим в ячейку, которая на 1 ближе к старту
                        x += Dx[k];
                        y += Dy[k]; 
                        break;
                    }
                }
            }

            if (x == Ax && y == Ay)
            {
                return true;
            }

            return false;
        }      

        void WritePathPoints(int x, int y)
        {
            XYZ point = new XYZ(InputData.ZonePoint1.X + x * InputData.PointsStepF, InputData.ZonePoint1.Y + y * InputData.PointsStepF, 0);
            PathCoords.Add(point);
        }
        
    }
}
