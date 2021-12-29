using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class ZoneByCircle : IZonePoints
    {
        private static double ElemDiameterF { get; } = ElementSize.ElemDiameterF;
        public static int FullZoneCleranceInSteps { get; set; }

        int GetFullZoneCleranceInSteps()
        {           
            double ElemDiameterInSteps = (ElemDiameterF /2) / InputData.PointsStepF;
            int t = (int)Math.Round(ElemDiameterInSteps);
            FullZoneCleranceInSteps = PointClearanceZone.ZoneClearanceInSteps + t;

            return FullZoneCleranceInSteps;
        }

        public List<StepPoint> CreateZonePoints()
        {
            int fullZoneCleranceInSteps = GetFullZoneCleranceInSteps();

            List<StepPoint> ZonePoints = new List<StepPoint>();

            for (int z = -fullZoneCleranceInSteps; z <= fullZoneCleranceInSteps; z++)
            {
                int yCount = fullZoneCleranceInSteps - Math.Abs(z);

                for (int y = -yCount; y <= yCount; y++)
                {
                    int xCount = fullZoneCleranceInSteps - Math.Abs(y);

                    for (int x = -xCount; x <= xCount; x++)
                    {
                        if (x == 0 && y == 0 && z == 0)
                            continue;

                            StepPoint stepPoint = new StepPoint(x, y, z);
                            ZonePoints.Add(stepPoint);
                    }

                }
            }
            return ZonePoints;
        }

        public List<StepPoint> CreateZonePointsOld()
        {
            int fullZoneCleranceInSteps = GetFullZoneCleranceInSteps();

            List<StepPoint> ZonePoints = new List<StepPoint>();

            for (int z = -fullZoneCleranceInSteps; z <= fullZoneCleranceInSteps; z += fullZoneCleranceInSteps)
            {
                int yCount = fullZoneCleranceInSteps - Math.Abs(z);

                for (int y = -yCount; y <= yCount; y += fullZoneCleranceInSteps)
                {
                    int xCount = yCount - Math.Abs(y);

                    for (int x = -xCount; x <= xCount; x += fullZoneCleranceInSteps)
                    {
                        if (x == 0 && y == 0 && z == 0)
                            continue;

                        StepPoint stepPoint = new StepPoint(x, y, z);
                        ZonePoints.Add(stepPoint);
                    }

                }
            }
            return ZonePoints;
        }
    }
}
