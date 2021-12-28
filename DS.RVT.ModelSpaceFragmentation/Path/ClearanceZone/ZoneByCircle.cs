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
        private static double ElemDiameter { get; } = ElementSize.ElemDiameter;

        int GetFullZoneCleranceInSteps()
        {
            double ElemDiameterF = UnitUtils.Convert(ElemDiameter / 1000,
                                DisplayUnitType.DUT_METERS,
                                DisplayUnitType.DUT_DECIMAL_FEET);

            double ElemDiameterInSteps = ElemDiameterF / InputData.PointsStepF;
            return PointClearanceZone.ZoneClearanceInSteps + (int)Math.Round(ElemDiameterInSteps / 2);
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
    }
}
