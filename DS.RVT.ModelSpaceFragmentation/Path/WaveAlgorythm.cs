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
                    /*
                    if (y == By - 1)
                        break;
                    */
                    for (x = 0; x < InputData.W; x++)
                    {
                        if (grid[x, y] == 0)
                            continue;
                        /*
                        bool emptyCell = IsCellEmpty(x, y);
                        if (emptyCell == false)
                            continue;
                        */

                        // проходим по всем непомеченным соседям
                        for (k = 0; k < 4; ++k)                    
                        {
                            int iy = y + Dy[k], ix = x + Dx[k];
                            if (iy >= 0 && iy < InputData.H && ix >= 0 && ix < InputData.W)
                            {
                                if (grid[ix, iy] == 0)
                                {

                                    bool emptyCell = IsCellEmpty(ix, iy);
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

            //Uidoc.RefreshActiveView();
            grid[Ax, Ay] = 0;
            //TaskDialog.Show("Revit", d.ToString());

            // восстановление пути
            len = grid[Bx, By];            // длина кратчайшего пути из (ax, ay) в (bx, By)
            x = Bx;
            y = By;
            d = len;


            while (d >= 0)
            {
                InputData.Px[d] = x;
                InputData.Py[d] = y;                   // записываем ячейку (x, y) в путь
                WritePathPoints(x, y);

                d--;
                for (k = 0; k < 4; ++k)
                {
                    int iy = y + Dy[k], ix = x + Dx[k];
                    if (iy >= 0 && iy < InputData.H && ix >= 0 && ix < InputData.W &&
                         grid[ix, iy] == d)
                    {
                        x += Dx[k];
                        y += Dy[k];           // переходим в ячейку, которая на 1 ближе к старту
                        //color = new Color(0, 0, 255);
                        //cell.OverwriteCell(x, y, color);
                        //Uidoc.RefreshActiveView();
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

        bool IsCellEmpty(int ix, int iy)
        {
            if (data.UnpassablePoints.Count == 0)
                return true;

            for (int i = 0; i < InputData.UnpassLocX.Count; i++)
            {
                if (InputData.UnpassLocX[i] == ix && InputData.UnpassLocY[i] == iy)
                    return false;
            }

            return true;
        }

        private bool IsEven(int a)
        {
            return (a % 2) == 0;
        }

        bool IsStartCellEmpty()
        {
            bool emptyCell = IsCellEmpty(Ax, Ay);

            if (emptyCell == true)
                return true;

            //Try to move start point
            bool pointMoved = false;

            if (Math.Abs(Ax - Bx) >= Math.Abs(Ay - By))
            {
                //Get side for move
                if (Ay <= By)
                    pointMoved = MovePointUp(Ax, ref Ay);
                else if (Ay > By)
                    pointMoved = MovePointDown(Ax, ref Ay);
            }
            else
            {
                if (Ax < Bx)
                    pointMoved = MovePointRight(ref Ax, Ay);
                else if (Ax > Bx)
                    pointMoved = MovePointLeft(ref Ax, Ay);
            }

            //Check if moved
            if (pointMoved == false)
            {
                TaskDialog.Show("Revit", "Process aborted! \nStart point is busy. Try to move it to another location.");
                return false;
            }
            else
            {
                TaskDialog.Show("Revit", "Start point is busy but it have been moved successfully!");
                return true;
            }
        }

        bool IsEndCellEmpty()
        {
            bool emptyCell = IsCellEmpty(Bx, By);

            if (emptyCell == true)
                return true;

            //Try to move end point
            bool pointMoved = MoveEndPointToStart();

            if (Math.Abs(Ax - Bx) >= Math.Abs(Ay - By))
            {
                //Get side for move
                if (By <= Ay)
                    pointMoved = MovePointUp(Bx, ref By);
                else if (By > Ay)
                    pointMoved = MovePointDown(Bx, ref By);
            }
            else
            {
                if (Bx <= Ax)
                    pointMoved = MovePointRight(ref Bx, By);
                else if (Bx > Ax)
                    pointMoved = MovePointLeft(ref Bx, By);
            }           

            //Check if moved
            if (pointMoved == false)
            {
                TaskDialog.Show("Revit", "Process aborted! \nStart point is busy. Try to move it to another location.");
                return false;
            }
            else
            {
                TaskDialog.Show("Revit", "Start point is busy but it have been moved successfully!");
                return true;
            }
        }

        bool MovePointUp(int px, ref int py)
        {
            for (int y = py; y <= InputData.H; y++)
            {
                bool emptyCell = IsCellEmpty(px, y);
                if (emptyCell == true)
                {
                    py = y;
                    return true;
                }
            }
            return false;
        }

        bool MovePointDown(int px, ref int py)
        {
            for (int y = py; y >= 0; y--)
            {
                bool emptyCell = IsCellEmpty(px, y);
                if (emptyCell == true)
                {
                    py = y;
                    return true;
                }
            }
            return false;
        }

        bool MovePointRight(ref int px, int py)
        {
            for (int x = px; x <= InputData.W; x++)
            {
                bool emptyCell = IsCellEmpty(x, py);
                if (emptyCell == true)
                {
                    px = x;
                    return true;
                }
            }
            return false;
        }

        bool MovePointLeft(ref int px, int py)
        {
            for (int x = px; x >= 0; x--)
            {
                bool emptyCell = IsCellEmpty(x, py);
                if (emptyCell == true)
                {
                    px = x;
                    return true;
                }
            }
            return false;
        }

        bool MoveEndPointToStart()
        {
            bool emptyCell = IsCellEmpty(Bx, Ay);

            if (emptyCell == true)
            {
                By = Ay;
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
