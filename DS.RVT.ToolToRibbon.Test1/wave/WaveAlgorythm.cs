using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.AutoPipesCoordinarion
{
    class WaveAlgorythm
    {
        int len;

        //list of path coordinates
        int[] px;
        int[] py;
        public List<XYZ> pathCoords = new List<XYZ>();

        int ax, ay, bx, by, W, H;

        List<int> icLocX = new List<int>();
        List<int> icLocY = new List<int>();

        readonly UIDocument Uidoc;
        readonly List<XYZ> ICLocations;
        readonly Data data;
        readonly Cell cell;

        public WaveAlgorythm(UIDocument uidoc, List<XYZ> impassableCellsLocations, Data data, Cell cl)
        {
            Uidoc = uidoc;
            ICLocations = impassableCellsLocations;
            this.data = data;
            cell = cl;
        }

        // смещения, соответствующие соседям ячейки слева, сверху, справа, снизу
        List<int> dx = new List<int>
            {
                -1,
                0,
                1,
                0
            };

        List<int> dy = new List<int>
            {
                0,
                1,
                0,
                -1
            };

        public void FindPath()
        {
            ConvertToPlane();
            bool pathFind = lee();

            if (pathFind != true)
            {
                TaskDialog.Show("Revit", "Путь не найден!");
            }
        }

        void ConvertToPlane()
        {
            double axdbl = (data.StartPoint.X - data.ZonePoint1.X) / data.CellSizeF;
            double aydbl = (data.StartPoint.Y - data.ZonePoint1.Y) / data.CellSizeF;
            double bxdbl = (data.EndPoint.X - data.ZonePoint1.X) / data.CellSizeF;
            double bydbl = (data.EndPoint.Y - data.ZonePoint1.Y) / data.CellSizeF;

            ax = (int)Math.Round(axdbl);
            ay = (int)Math.Round(aydbl);
            bx = (int)Math.Round(bxdbl);
            by = (int)Math.Round(bydbl);

            W = cell.W;
            H = cell.H;

            if (bx >= W)
                bx = W - 1;
            else if (ax >= W)
                ax = W - 1;
            else if (ay >= H)
                ay = H;
            else if (by >= H)
                by = H;

            //координаты ячеек пути
            px = new int[W * H];
            py = new int[W * H];

            if (ICLocations.Count != 0)
            {
                foreach (XYZ xyz in ICLocations)
                {
                    int X = (int)Math.Round((xyz.X - data.ZonePoint1.X) / data.CellSizeF);
                    int Y = (int)Math.Round((xyz.Y - data.ZonePoint1.Y) / data.CellSizeF);
                    icLocX.Add(X);
                    icLocY.Add(Y);
                }
            }

        }

        bool lee()
        {
            //рабочее поле
            int[,] grid = new int[W, H];



            int x = 0;
            int y = 0;
            int d = 0;
            int k;
            int a;

            //Check start cell
            if (IsStartCellEmpty() == false)
                return false;
            //Check end cell
            if (IsEndCellEmpty() == false)
                return false;

            // стартовая ячейка
            grid[ax, ay] = 1;

            do
            {
                a = 0;
                for (y = 0; y < H; y++)
                {
                    /*
                    if (y == by - 1)
                        break;
                    */
                    for (x = 0; x < W; x++)
                    {
                        /*
                        color = new Color(255, 255, 0);
                        cell.OverwriteCell(x, y, color);
                        Uidoc.RefreshActiveView();
                        */
                        if (grid[x, y] == 0)
                            continue;
                        /*
                        bool emptyCell = IsCellEmpty(x, y);
                        if (emptyCell == false)
                            continue;
                        */

                        for (k = 0; k < 4; ++k)                    // проходим по всем непомеченным соседям
                        {
                            int iy = y + dy[k], ix = x + dx[k];
                            if (iy >= 0 && iy < H && ix >= 0 && ix < W)
                            {
                                if (grid[ix, iy] == 0)
                                {

                                    bool emptyCell = IsCellEmpty(ix, iy);
                                    if (emptyCell == true)
                                    {
                                        // распространяем волну
                                        d = grid[x, y] + 1;
                                        grid[ix, iy] = d;

                                        /*
                                        byte c = (byte)(d * 2);
                                        color = new Color(0, c, 0);
                                        cell.OverwriteCell(ix, iy, color);
                                        Uidoc.RefreshActiveView();
                                        */
                                        a++;
                                    }
                                }
                            }
                        }
                    }
                }
            } while (grid[bx, by] == 0 && a != 0);


            if (grid[bx, by] == 0)
                return false;

            //Uidoc.RefreshActiveView();
            grid[ax, ay] = 0;
            //TaskDialog.Show("Revit", d.ToString());

            // восстановление пути
            len = grid[bx, by];            // длина кратчайшего пути из (ax, ay) в (bx, by)
            x = bx;
            y = by;
            d = len;


            while (d >= 0)
            {
                px[d] = x;
                py[d] = y;                   // записываем ячейку (x, y) в путь
                WritePathPoints(x, y);

                d--;
                for (k = 0; k < 4; ++k)
                {
                    int iy = y + dy[k], ix = x + dx[k];
                    if (iy >= 0 && iy < H && ix >= 0 && ix < W &&
                         grid[ix, iy] == d)
                    {
                        x += dx[k];
                        y += dy[k];           // переходим в ячейку, которая на 1 ближе к старту
                        //color = new Color(0, 0, 255);
                        //cell.OverwriteCell(x, y, color);
                        //Uidoc.RefreshActiveView();
                        break;
                    }
                }
            }

            ShowPath();
            //overWriteStartEndCell();

            if (x == ax && y == ay)
            {
                return true;
            }

            return false;
        }

        bool IsCellEmpty(int ix, int iy)
        {
            if (ICLocations.Count == 0)
                return true;

            for (int i = 0; i < icLocX.Count; i++)
            {
                if (icLocX[i] == ix && icLocY[i] == iy)
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
            bool emptyCell = IsCellEmpty(ax, ay);

            if (emptyCell == true)
                return true;

            //Try to move start point
            bool pointMoved;

            //Get side for move
            if (ay < by)
            {
                pointMoved = MovePointUp(ax, ref ay);
            }
            else
                pointMoved = MovePointDown(ax, ref ay);

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
            bool emptyCell = IsCellEmpty(bx, by);

            if (emptyCell == true)
                return true;

            //Try to move end point
            bool pointMoved = MoveEndPointToStart();
            if (pointMoved == false)
            {
                //Get side for move
                if (by < ay)
                    pointMoved = MovePointUp(bx, ref by);
                else
                    pointMoved = MovePointDown(bx, ref by);
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
            for (int y = py; y <= H; y++)
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

        bool MoveEndPointToStart()
        {
            bool emptyCell = IsCellEmpty(bx, ay);

            if (emptyCell == true)
            {
                by = ay;
                return true;
            }

            return false;
        }

        void WritePathPoints(int x, int y)
        {
                XYZ point = new XYZ(data.ZonePoint1.X + x * data.CellSizeF, data.ZonePoint1.Y + y * data.CellSizeF, 0);              
                pathCoords.Add(point);
        }

        void ShowPath()
        {
            Color color;
            int i = 0;

            foreach (XYZ point in pathCoords)
            {
                if (i==0) 
                    color = new Color(0, 255, 0);
                else if (i== pathCoords.Count - 1)
                    color = new Color(0, 255, 255);
                else
                    color = new Color(0, 0, 255);

                cell.OverwriteCell(color, 0, 0, point);
                i++;
            }
        }
    }
}
