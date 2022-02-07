using Autodesk.Revit.DB;
using DSUtils.GridMap;
using System;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PathRequiment : IPathRequiment
    {
        public byte Clearance
        {
            get
            {
                double ClearanceF = UnitUtils.Convert(ClearanceDistance,
                             DisplayUnitType.DUT_MILLIMETERS,
                             DisplayUnitType.DUT_DECIMAL_FEET);

                double clearanceFull = ClearanceF + ElementWidthHalf;

                return (byte)Math.Round(clearanceFull / Main.PointsStepF);
            }
        }
        public byte MinAngleDistance { get; }

        static double ElementWidthHalf = (ElementSize.ElemDiameterF / 2);
        static double ElementHeghtHalf = (ElementSize.ElemDiameterF / 2);

        static double ClearanceDistance = 100;
    }
}
