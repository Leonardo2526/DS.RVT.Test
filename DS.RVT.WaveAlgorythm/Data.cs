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

        public double CellSizeF { get; set; }
        public double ZoneOffsetF { get; set; }


        public void SetValues()
        {

            StartPoint = new XYZ(5, 5, 0);
            EndPoint = new XYZ(15, 10, 0);
            ZoneOffset = 1000;
            CellSize = 50;

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

            ZonePoint1 = new XYZ(StartPoint.X, Y1, StartPoint.Z);
            ZonePoint2 = new XYZ(EndPoint.X, Y2, StartPoint.Z);        
        }

        void ConvertToFeets()
        {
            CellSizeF = UnitUtils.Convert((double)CellSize / 1000,
                                DisplayUnitType.DUT_METERS,
                                DisplayUnitType.DUT_DECIMAL_FEET);
            ZoneOffsetF = UnitUtils.Convert((double)ZoneOffset / 1000,
                                DisplayUnitType.DUT_METERS,
                                DisplayUnitType.DUT_DECIMAL_FEET);
        }
    }
}
