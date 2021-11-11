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
            double YSP = StartPoint.Y - ZoneOffsetF;
            double YEP = EndPoint.Y - ZoneOffsetF;
            double Y1 = Math.Min(YSP, YEP);

            YSP = StartPoint.Y + ZoneOffsetF;
            YEP = EndPoint.Y + ZoneOffsetF;

            double Y2 = Math.Max(YSP, YEP);

            int minx = (int)Math.Min(StartPoint.X, EndPoint.X);
            int maxx = (int)Math.Max(StartPoint.X, EndPoint.X);
            int miny = (int)Math.Min(Y1, Y2);
            int maxy = (int)Math.Max(Y1, Y2);

            if (Math.Abs(StartPoint.Z - EndPoint.Z) < 0.01)
            {
                ZonePoint1 = new XYZ(minx, miny, StartPoint.Z);
                ZonePoint2 = new XYZ(maxx, maxy, EndPoint.Z + CellSizeF);
            }
                  
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
