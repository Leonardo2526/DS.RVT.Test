using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PointClearanceZone
    {
        private static double ZoneClearance = 100;
        public static int ZoneClearanceInSteps { get; set; }

        public List<StepPoint> ClerancePoints { get; set; }

        void GetZoneClearanceInSteps()
        {
            double ZoneClearanceF = UnitUtils.Convert(ZoneClearance / 1000,
                                DisplayUnitType.DUT_METERS,
                                DisplayUnitType.DUT_DECIMAL_FEET);
            ZoneClearanceInSteps = (int)Math.Round(ZoneClearanceF / InputData.PointsStepF);
        }


        public List<StepPoint> Create(IZonePoints zonePoints)
        {
            GetZoneClearanceInSteps();
            ClerancePoints = new List<StepPoint>();
            ClerancePoints = zonePoints.CreateZonePoints();
            return ClerancePoints;
        }

        List<XYZ> PointsConverter(XYZ basePoint)
        {
            List<XYZ> modelSpacePoints = new List<XYZ>();

            foreach(StepPoint stepPoint in ClerancePoints)
            {
                XYZ point = new XYZ(basePoint.X + stepPoint.X * InputData.PointsStepF,
                    basePoint.Y + stepPoint.Y * InputData.PointsStepF,
                    basePoint.Z + stepPoint.Z * InputData.PointsStepF);
                modelSpacePoints.Add(point);
            }

            return modelSpacePoints;
        }


        public void ShowPoints(XYZ basePoint)
        {
            List<XYZ> modelSpacePoints = PointsConverter(basePoint);

            Visualizator visualizator = new Visualizator(Main.Doc);

            visualizator.ShowPoints(new SpacePointsVisualization(modelSpacePoints));

        }
    }
}
