using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.WaveAlgorythm
{
    class WaveAlgorythm
    {
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
        readonly Data data;
        readonly Cell cell;

        public WaveAlgorythm(Application app, UIApplication uiapp, Document doc, UIDocument uidoc,
            List<XYZ> impassableCellsLocations, Data data, Cell cl)
        {
            App = app;
            Uiapp = uiapp;
            Doc = doc;
            Uidoc = uidoc;
            ICLocations = impassableCellsLocations;
            this.data = data;
            cell = cl;
        }


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

            ax = (int)Math.Ceiling(axdbl);
            ay = (int)Math.Ceiling(aydbl);
            bx = (int)Math.Ceiling(bxdbl);
            by = (int)Math.Ceiling(bydbl);

            double Wdbl = (data.ZonePoint2.X - data.ZonePoint1.X) / data.CellSizeF;
            double Hdbl = (data.ZonePoint2.Y - data.ZonePoint1.Y) / data.CellSizeF;


            W = (int)Math.Ceiling(Wdbl);
            H = (int)Math.Ceiling(Hdbl);

            //координаты ячеек пути
            px = new int[W * H];
            py = new int[W * H];

            if (ICLocations.Count != 0)
            {
                foreach (XYZ xyz in ICLocations)
                {
                    int X = (int)Math.Round((xyz.X - data.ZonePoint1.X) / data.CellSizeF);
                    int Y = (int)Math.Round((xyz.Y - data.ZonePoint1.Y )/ data.CellSizeF);
                    icLocX.Add(X);
                    icLocY.Add(Y);
                }
            }

        }

        bool lee()
        {
            //рабочее поле
            int[,] grid = new int[W, H];

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

            int x = 0;
            int y = 0;
            int d = 0;
            int k;
            int a;

            Color color;

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

                                        byte c = (byte)(d * 2);
                                        color = new Color(0, c, 0);
                                        cell.OverwriteCell(ix, iy, color);
                                        //Uidoc.RefreshActiveView();
                                        a++;
                                    }
                                }
                            }
                        }
                    }
                }
            } while (grid[bx - 1, by - 1] == 0 && a != 0);

            
            if (grid[bx - 1, by - 1] == 0)
                return false;

            grid[ax, ay] = 0;
            //TaskDialog.Show("Revit", d.ToString());


            // восстановление пути
            len = grid[bx - 1, by - 1];            // длина кратчайшего пути из (ax, ay) в (bx, by)
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
                         grid[ix, iy] == d)
                    {

                        x += dx[k];
                        y += dy[k];           // переходим в ячейку, которая на 1 ближе к старту
                        color = new Color(0, 0, 255);
                        cell.OverwriteCell(x, y, color);
                        break;
                    }
                }
            }

            overWriteStartEndCell();

            if (x == ax && y == ay)
            {
                return true;
            }
            
            return false;
        }

        void overWriteStartEndCell()
        {
            Color colorStart = new Color(0, 255, 0);
            cell.OverwriteCell(ax, ay, colorStart);

            Color colorEnd = new Color(0, 255, 255);
            cell.OverwriteCell(bx, by, colorEnd, data.EndPoint);
            Uidoc.RefreshActiveView();
        }


        void restoreWave()
        {

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


    }
}
