using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.WaveAlgorythm
{
    class WaveAlgorythm
    {
        public static double Width { get; set; }
        public static double Height { get; set; }
        double cellSizeF { get; set; }
        List<XYZ> cellsLocations { get; set; }
        List<XYZ> impassableCellsLocations { get; set; }
        XYZ startPoint { get; set; }
        XYZ endPoint { get; set; }


        const int WALL = -1;
        const int BLANK = -2;
        int len;

        int[] px;
        int[] py;

        int ax, ay, bx, by, W, H;

        List<int> icLocX = new List<int>();
        List<int> icLocY = new List<int>();

        readonly Application App;
        readonly UIApplication Uiapp;
        readonly Document Doc;
        readonly UIDocument Uidoc;

        public WaveAlgorythm(Application app, UIApplication uiapp, Document doc, UIDocument uidoc)
        {
            App = app;
            Uiapp = uiapp;
            Doc = doc;
            Uidoc = uidoc;
        }



        public void FindPath()
        {
            ConvertToPlane();
            lee();
        }

        void ConvertToPlane()
        {
            ax = (int)Math.Round((startPoint.X / cellSizeF),0);
            ay = (int)Math.Round(startPoint.Y / cellSizeF);
            bx = (int)Math.Round(endPoint.X / cellSizeF);
            by = (int)Math.Round(endPoint.Y / cellSizeF);

            W = (int)Math.Round(Width / cellSizeF);
            H = (int)Math.Round(Height / cellSizeF);

            //координаты ячеек пути
            px = new int[W * H];
            py = new int[W * H];

            foreach (XYZ xyz in impassableCellsLocations)
            {
                int X = (int)Math.Round(xyz.X / cellSizeF);
                int Y = (int)Math.Round(xyz.X / cellSizeF);
                icLocX.Add(X);
                icLocY.Add(Y);
            }
        }

        bool lee()
        {
            Cell cell = new Cell(App, Uiapp, Doc, Uidoc);
        


            //list of path coordinates
            //List<XYZ> pathXYZ = new List<XYZ>();


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

            int d, x, y, k;
            bool stop;

            if (grid[ay, ax] == WALL || grid[by, bx] == WALL) return false;  // ячейка (ax, ay) или (bx, by) - стена

          

            // распространение волны
            d = 0;
            do
            {
                // предполагаем, что все свободные клетки уже помечены
                stop = true;

                //reset color
                Color color;
                if (IsEven(d) == true)
                {
                    color = new Color(255, 0, 0);
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
                                    if (IsCellEmpty(ix, iy) == true)
                                    {
                                        stop = false;              // найдены непомеченные клетки
                                        grid[iy, ix] = d + 1;      // распространяем волну
                                        cell.OverwriteCell(ix, iy, color);
                                    }
                                    
                                }
                            }
                        }
                    }
                    d++;
                }
            } while (!stop && grid[by, bx] == BLANK);

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


            return true;
        }


        bool IsCellEmpty(int ix, int iy)
        {
            foreach (int x in icLocX)
            {
                if (x == ix)
                {
                    foreach (int y in icLocY)
                    {
                        if (y == iy)
                        {
                            return false;
                        }

                    }
                }

            }
            return true;
        }


        private bool IsEven(int a)
        {
            return (a % 2) == 0;
        }
    }
}
