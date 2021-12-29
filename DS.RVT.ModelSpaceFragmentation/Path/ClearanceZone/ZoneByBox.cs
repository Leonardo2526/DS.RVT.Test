using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class ZoneByBox : IZonePoints
    {
        private static double ElemDiameterF { get; } = ElementSize.ElemDiameterF;
        public static int FullZoneCleranceInSteps { get; set; }

        int GetFullZoneCleranceInSteps()
        {
            double ElemDiameterInSteps = (ElemDiameterF / 2) / InputData.PointsStepF;
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
                for (int y = -fullZoneCleranceInSteps; y <= fullZoneCleranceInSteps; y++)
                {
                    for (int x = -fullZoneCleranceInSteps; x <= fullZoneCleranceInSteps; x++)
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