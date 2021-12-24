using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;


namespace DS.RVT.ModelSpaceFragmentation.WaveAlgorythm
{
    class InputDataConverator
    {
        //list of path coordinates
        public static int[] px;
        public static int[] py;

        public static int ax {get; set;}
        public static int ay { get; set;}
        public static int bx { get; set;}
        public static int by { get; set;}
        public static int W { get; set;}
        public static int H { get; set;}
        public static List<int> UnpassLocX { get; set; }
        public static List<int> UnpassLocY { get; set; }

        readonly InputData data;
        public InputDataConverator(InputData inputData)
        {
            data = inputData;
        }

        public void ConvertToPlane()
        {
            double axdbl = (data.StartPoint.X - data.ZonePoint1.X) / InputData.PointsStepF;
            double aydbl = (data.StartPoint.Y - data.ZonePoint1.Y) / InputData.PointsStepF;
            double bxdbl = (data.EndPoint.X - data.ZonePoint1.X) / InputData.PointsStepF;
            double bydbl = (data.EndPoint.Y - data.ZonePoint1.Y) / InputData.PointsStepF;

            ax = (int)Math.Round(axdbl);
            ay = (int)Math.Round(aydbl);
            bx = (int)Math.Round(bxdbl);
            by = (int)Math.Round(bydbl);

            W = data.W;
            H = data.H;

            if (bx >= W)
                bx = W - 1;
            else if (ax < 0)
                ax = 0;
            else if (ay >= H)
                ay = H;
            else if (by >= H)
                by = H;

            //координаты ячеек пути
            px = new int[W * H];
            py = new int[W * H];

            if (data.UnpassablePoints.Count != 0)
            {
                foreach (XYZ xyz in data.UnpassablePoints)
                {
                    int X = (int)Math.Round((xyz.X - data.ZonePoint1.X) / InputData.PointsStepF);
                    int Y = (int)Math.Round((xyz.Y - data.ZonePoint1.Y) / InputData.PointsStepF);
                    UnpassLocX.Add(X);
                    UnpassLocY.Add(Y);
                }
            }

        }
    }
}
