using Autodesk.Revit.DB;
using System;

namespace DS.RVT.AutoPipesCoordinarion
{
    class Data
    {
        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }
        public int ZoneOffset { get; set; }
        public XYZ ZonePoint1 { get; set; }
        public XYZ ZonePoint2 { get; set; }
        public int CellSize { get; set; }
        public double ElementOffset { get; set; }

        public double CellSizeF { get; set; }
        public double ZoneOffsetF { get; set; }
        public double ElementOffsetF { get; set; }



        public void SetValues(XYZ sp, XYZ ep)
        {
            StartPoint = sp;
            EndPoint =ep;
            ZoneOffset = 1000;
            CellSize = 50;
            ElementOffset = 50;

            ConvertToFeets();

            GetZonePoints();

        }

        void GetZonePoints()
        {
            double Y1 = Math.Min(StartPoint.Y - ZoneOffsetF, EndPoint.Y - ZoneOffsetF);
            double Y2 = Math.Max(StartPoint.Y + ZoneOffsetF, EndPoint.Y + ZoneOffsetF);
            double X1 = Math.Min(StartPoint.X - ZoneOffsetF, EndPoint.X - ZoneOffsetF);
            double X2 = Math.Max(StartPoint.X + ZoneOffsetF, EndPoint.X + ZoneOffsetF);

            int minx, miny, maxx, maxy;

            if (Math.Abs(StartPoint.X - EndPoint.X) >= Math.Abs(StartPoint.Y - EndPoint.Y))
            {
                minx = (int)Math.Min(StartPoint.X, EndPoint.X);
                maxx = (int)Math.Max(StartPoint.X, EndPoint.X);
                miny = (int)Math.Min(Y1, Y2);
                maxy = (int)Math.Max(Y1, Y2);

                ZonePoint1 = new XYZ(minx, miny, StartPoint.Z);
                ZonePoint2 = new XYZ(maxx, maxy, EndPoint.Z);
            }
            else
            {
                minx = (int)Math.Min(X1, X2);
                maxx = (int)Math.Max(X1, X2);
                miny = (int)Math.Min(StartPoint.Y, EndPoint.Y);
                maxy = (int)Math.Max(StartPoint.Y, EndPoint.Y);

                ZonePoint1 = new XYZ(minx, miny, StartPoint.Z);
                ZonePoint2 = new XYZ(maxx, maxy, EndPoint.Z);
            }

            if (Math.Abs(StartPoint.Z - EndPoint.Z) < 0.01)
                ZonePoint2 = new XYZ(ZonePoint2.X, ZonePoint2.Y, ZonePoint2.Z + CellSizeF);
        }

        void ConvertToFeets()
        {
            CellSizeF = UnitUtils.Convert((double)CellSize / 1000,
                                DisplayUnitType.DUT_METERS,
                                DisplayUnitType.DUT_DECIMAL_FEET);
            ZoneOffsetF = UnitUtils.Convert((double)ZoneOffset / 1000,
                                DisplayUnitType.DUT_METERS,
                                DisplayUnitType.DUT_DECIMAL_FEET);
            ElementOffsetF = UnitUtils.Convert((double)ElementOffset / 1000,
                                DisplayUnitType.DUT_METERS,
                                DisplayUnitType.DUT_DECIMAL_FEET);
        }
    }
}
