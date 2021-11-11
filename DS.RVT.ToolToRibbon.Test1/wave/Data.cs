﻿using Autodesk.Revit.DB;
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
            EndPoint = ep;
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

            ZonePoint1 = new XYZ(StartPoint.X, Y1, StartPoint.Z);
            ZonePoint2 = new XYZ(EndPoint.X, Y2, EndPoint.Z);        
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
