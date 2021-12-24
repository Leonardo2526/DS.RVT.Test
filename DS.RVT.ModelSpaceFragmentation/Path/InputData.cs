using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class InputData
    {
        /// <summary>
        /// Path coordinates by x
        /// </summary>
        public static int[] Px { get; set; }
        /// <summary>
        /// Path coordinates by y
        /// </summary>
        public static int[] Py { get; set; }

        public static int Ax { get; set; }
        public static int Ay { get; set; }
        public static int Bx { get; set; }
        public static int By { get; set; }
        public static int W { get; set; }
        public static int H { get; set; }
        public static List<int> UnpassLocX { get; set; }
        public static List<int> UnpassLocY { get; set; }
        public static double PointsStepF { get; set; }
        public static XYZ ZonePoint1 { get; set; }
        public static XYZ ZonePoint2 { get; set; }

        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }
        public List<XYZ> UnpassablePoints { get; set;}   

        public InputData (XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            UnpassablePoints = unpassablePoints;
        }

        public void ConvertToPlane()
        {
            double axdbl = (StartPoint.X - ZonePoint1.X) / PointsStepF;
            double aydbl = (StartPoint.Y - ZonePoint1.Y) / PointsStepF;
            double bxdbl = (EndPoint.X - ZonePoint1.X) / PointsStepF;
            double bydbl = (EndPoint.Y - ZonePoint1.Y) / PointsStepF;

            Ax = (int)Math.Round(axdbl);
            Ay = (int)Math.Round(aydbl);
            Bx = (int)Math.Round(bxdbl);
            By = (int)Math.Round(bydbl);

            W = ModelSpacePointsGenerator.Xcount;
            H = ModelSpacePointsGenerator.Ycount;

            if (Bx >= W)
                Bx = W - 1;
            else if (Ax < 0)
                Ax = 0;
            else if (Ay >= H)
                Ay = H;
            else if (By >= H)
                By = H;

            //координаты ячеек пути
            Px = new int[W * H];
            Py = new int[W * H];

            if (UnpassablePoints.Count != 0)
            {
                foreach (XYZ xyz in UnpassablePoints)
                {
                    int X = (int)Math.Round((xyz.X - ZonePoint1.X) / PointsStepF);
                    int Y = (int)Math.Round((xyz.Y - ZonePoint1.Y) / PointsStepF);
                    UnpassLocX.Add(X);
                    UnpassLocY.Add(Y);
                }
            }

            ZonePoint1  = PointsInfo.MinBoundPoint;
            ZonePoint2  = PointsInfo.MaxBoundPoint;
            PointsStepF = ModelSpacePointsGenerator.PointsStepF;
        }
    }
}
