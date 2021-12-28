using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class ZoneByCircle : IZonePoints
    {
        private static double ElemDiameter { get; } = ElementSize.ElemDiameter;

        private readonly double ElemDiameterInSteps = ElemDiameter / InputData.PointsStepF;
        

        public List<StepPoint> CreateZonePoints()
        {
            int fullZoneCleranceInSteps = PointClearanceZone.ZoneClearanceInSteps + (int)Math.Round(ElemDiameterInSteps / 2);

            List<StepPoint> ZonePoints = new List<StepPoint>();

            for (int z = 0; z <= fullZoneCleranceInSteps; z++)
            {
                int yCount = fullZoneCleranceInSteps - z;

                for (int y = 0; y <= yCount; y++)
                {
                    int xCount = fullZoneCleranceInSteps - y;

                    for (int x = 0; x <= xCount; x++)
                    {
                        if (x != 0 && y != 0 && z != 0)
                        {
                            StepPoint stepPoint = new StepPoint(x, y, z);
                            ZonePoints.Add(stepPoint);
                        }

                    }
                }
            }


            return ZonePoints;
        }
    }
}
