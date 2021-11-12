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
           
            double Y1 = Math.Min(StartPoint.Y - ZoneOffsetF, EndPoint.Y - ZoneOffsetF);
            double Y2 = Math.Max(StartPoint.Y + ZoneOffsetF, EndPoint.Y + ZoneOffsetF);
            double X1 = Math.Min(StartPoint.X - ZoneOffsetF, EndPoint.X - ZoneOffsetF);
            double X2 = Math.Max(StartPoint.X + ZoneOffsetF, EndPoint.X + ZoneOffsetF);

            if (Math.Abs(StartPoint.X - EndPoint.X) >= Math.Abs(StartPoint.Y - EndPoint.Y))
            {
                ZonePoint1 = new XYZ(StartPoint.X, Y1, StartPoint.Z);
                ZonePoint2 = new XYZ(EndPoint.X, Y2, EndPoint.Z);
            }
            else
            {
                ZonePoint1 = new XYZ(X1, StartPoint.Y , StartPoint.Z);
                ZonePoint2 = new XYZ(X2, EndPoint.Y, EndPoint.Z);
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
