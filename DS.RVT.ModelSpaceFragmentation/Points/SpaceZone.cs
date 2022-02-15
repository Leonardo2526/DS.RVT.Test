using Autodesk.Revit.DB;
using System;

namespace DS.RVT.ModelSpaceFragmentation
{
    class SpaceZone
    {
        private static int ZoneSize = 500;
        private static double ZoneSizeF
        {
            get
            {
                return UnitUtils.Convert(ZoneSize,
                                 DisplayUnitType.DUT_MILLIMETERS,
                                 DisplayUnitType.DUT_DECIMAL_FEET);
            }
        }
        private static XYZ DeltaBounds
        {
            get
            {
                return new XYZ(ElementInfo.MaxBoundPoint.X - ElementInfo.MinBoundPoint.X,
                    ElementInfo.MaxBoundPoint.Y - ElementInfo.MinBoundPoint.Y,
                    ElementInfo.MaxBoundPoint.Z - ElementInfo.MinBoundPoint.Z);
            }
        }

        public static int ZoneXCount
        {
            get
            {
                return (int)Math.Round(DeltaBounds.X / ZoneSizeF);
            }
        }

        public static double ZoneSizeX
        {
            get
            {
                return DeltaBounds.X / ZoneXCount;
            }
        }

        public static int ZoneYCount
        {
            get
            {
                return (int)Math.Round(DeltaBounds.Y / ZoneSizeF);
            }
        }

        public static double ZoneSizeY
        {
            get
            {
                return DeltaBounds.Y / ZoneYCount;
            }
        }
        public static int ZoneZCount
        {
            get
            {
                return (int)Math.Round(DeltaBounds.Z / ZoneSizeF);
            }
        }

        public static double ZoneSizeZ
        {
            get
            {
                return DeltaBounds.Z / ZoneZCount;
            }
        }
    }
}
