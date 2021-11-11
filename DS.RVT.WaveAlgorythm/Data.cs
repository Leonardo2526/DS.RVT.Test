using Autodesk.Revit.DB;
using System;

namespace DS.RVT.WaveAlgorythm
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



        public void SetValues()
        {

            StartPoint = new XYZ(5, 15, 0);
            EndPoint = new XYZ(10, 13 , 0);
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

            double XSP = StartPoint.X - ZoneOffsetF;
            double XEP = EndPoint.X - ZoneOffsetF;
            double X1 = Math.Min(XSP, XEP);

            XSP = StartPoint.X + ZoneOffsetF;
            XEP = EndPoint.X + ZoneOffsetF;

            double X2 = Math.Max(XSP, XEP);

            ZonePoint1 = new XYZ(X1, Y1, StartPoint.Z);
            ZonePoint2 = new XYZ(X2, Y2, EndPoint.Z);        
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
