using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.WaveAlgorythm
{
    class WaveAlgorythm
    {
        const int WALL = -1;
        const int BLANK = -2;
        int len;

        //list of path coordinates
        int[] px;
        int[] py;

        int ax, ay, bx, by, W, H;

        List<int> icLocX = new List<int>();
        List<int> icLocY = new List<int>();

        readonly Application App;
        readonly UIApplication Uiapp;
        readonly Document Doc;
        readonly UIDocument Uidoc;
        readonly List<XYZ> ICLocations;
        readonly XYZ StartPoint;
        readonly XYZ EndPoint;
        readonly double Width;
        readonly double Height;
        readonly double CellSize;
        readonly Cell CellClass;

        public WaveAlgorythm(Application app, UIApplication uiapp, Document doc, UIDocument uidoc,
            List<XYZ> impassableCellsLocations, XYZ startPoint, XYZ endPoint,
            double width, double height, double cellSize, Cell cellClass)
        {
            App = app;
            Uiapp = uiapp;
            Doc = doc;
            Uidoc = uidoc;

            ICLocations = impassableCellsLocations;
            StartPoint = startPoint;
            EndPoint = endPoint;
            Width = width;
            Height = height;
            CellSize = cellSize;
            CellClass = cellClass;
        }



        public void FindPath()
        {
            ConvertToPlane();
            lee();
        }

        void ConvertToPlane()
        {
            ax = (int)Math.Round((StartPoint.X / CellSize), 0);
            ay = (int)Math.Round(StartPoint.Y / CellSize);
            bx = (int)Math.Round(EndPoint.X / CellSize);
            by = (int)Math.Round(EndPoint.Y / CellSize);

            W = (int)Math.Round(Width / CellSize);
            H = (int)Math.Round(Height / CellSize);

            //координаты ячеек пути
            px = new int[W * H];
            py = new int[W * H];

            double CellSizeF = UnitUtils.Convert(CellSize / 1000,
                                  DisplayUnitType.DUT_METERS,
                                  DisplayUnitType.DUT_DECIMAL_FEET);


            foreach (XYZ xyz in ICLocations)
            {
                int X = (int)Math.Round(xyz.X / CellSizeF);
                int Y = (int)Math.Round(xyz.Y / CellSizeF);
                icLocX.Add(X);
                icLocY.Add(Y);
            }
        }

        bool lee()
        {
            //рабочее поле
            int[,] grid = new int[W, H];

            // смещения, соответствующие соседям ячейки справа, снизу, слева и сверху
            List<int> dx = new List<int>
            {
                1,
                0,
                -1,
                0
            };

            List<int> dy = new List<int>
            {
                0,
                -1,
                0,
                1
            };

            List<XYZ> freeCells = new List<XYZ>();

            int x = 0;
            int y = 0;
            int d = 0;
            int k;

            Color color;

            for (y = ay; y < H; y++)
            {
                for (x = ax; x < W; x++)
                {

                    for (k = 0; k < 4; ++k)                    // проходим по всем непомеченным соседям
                    {
                        int iy = y + dy[k], ix = x + dx[k];
                        if (iy >= 0 && iy < H && ix >= 0 && ix < W)
                        {
                            if (grid[ix, iy] == 0)
                            {
                                d = x + y + 1;
                                grid[ix, iy] = d;      // распространяем волну

                                bool emptyCell = IsCellEmpty(ix, iy);
                                if (emptyCell == true)
                                {
                                    XYZ xYZ = new XYZ(ix, iy, 0);
                                    freeCells.Add(xYZ);


                                    bool even = IsEven(d);
                                    if (even == true)
                                    {
                                        color = new Color(0, 0, 255);
                                    }
                                    else
                                        color = new Color(0, 255, 0);
                                    CellClass.OverwriteCell(ix, iy, color);
                                    //Uidoc.RefreshActiveView();                                    
                                }
                            }
                        }
                    }
                }
                if (y == by && x == bx)
                    break;
            }

            grid[ax, ay] = 0;

            //TaskDialog.Show("Revit", freeCells.Count().ToString());

            return true;
        }

        bool leeold()
        {
            //рабочее поле
            int[,] grid = new int[W, H];

            // смещения, соответствующие соседям ячейки справа, снизу, слева и сверху
            List<int> dx = new List<int>
            {
                1,
                0,
                -1,
                0
            };

            List<int> dy = new List<int>
            {
                0,
                -1,
                0,
                1
            };


            int x = 0;
            int y = 0;
            int d, k;

            bool stop;

            //if (grid[ay, ax] == WALL || grid[by, bx] == WALL) return false;  // ячейка (ax, ay) или (bx, by) - стена          

            // распространение волны
            d = 0;
            do
            {
                // предполагаем, что все свободные клетки уже помечены
                stop = true;
                Color color;
                bool even = IsEven(d);
                if (even == true)
                {
                    color = new Color(0, 0, 255);
                }
                else
                    color = new Color(0, 255, 0);

                for (y = 0; y < H; y++)
                {
                    for (x = 0; x < W; x++)
                    {
                        if (grid[y, x] == d)                         // ячейка (x, y) помечена числом d
                        {
                            for (k = 0; k < 4; ++k)                    // проходим по всем непомеченным соседям
                            {
                                int iy = y + dy[k], ix = x + dx[k];
                                if (iy >= 0 && iy < H && ix >= 0 && ix < W)
                                {
                                    bool emptyCell = IsCellEmpty(ix, iy);
                                    if (emptyCell == true)
                                    {
                                        stop = false;              // найдены непомеченные клетки
                                        grid[iy, ix] = d + 1;      // распространяем волну
                                        CellClass.OverwriteCell(ix, iy, color);
                                        //Uidoc.RefreshActiveView();
                                    }

                                }
                            }
                        }
                        d++;
                    }
                }

            } while (!stop && y != by && x != bx);

            //} while (!stop && grid[by, bx] == BLANK);
            /*
            if (grid[by, bx] == BLANK) return false;  // путь не найден

            // восстановление пути
            len = grid[by, bx];            // длина кратчайшего пути из (ax, ay) в (bx, by)
            x = bx;
            y = by;
            d = len;
            while (d > 0)
            {
                px[d] = x;
                py[d] = y;                   // записываем ячейку (x, y) в путь
                d--;
                for (k = 0; k < 4; ++k)
                {
                    int iy = y + dy[k], ix = x + dx[k];
                    if (iy >= 0 && iy < H && ix >= 0 && ix < W &&
                         grid[iy, ix] == d)
                    {
                        x = x + dx[k];
                        y = y + dy[k];           // переходим в ячейку, которая на 1 ближе к старту
                        break;
                    }
                }
            }
            px[0] = ax;
            py[0] = ay;                    // теперь px[0..len] и py[0..len] - координаты ячеек пути

            */
            return true;
        }


        bool IsCellEmpty(int ix, int iy)
        {
            for (int i = 0; i< icLocX.Count; i++)
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
    }
}
