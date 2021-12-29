using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Points;
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
            ZoneClearanceInSteps = (int)Math.Ceiling(ZoneClearanceF / InputData.PointsStepF);
        }


        public List<StepPoint> Create(IZonePoints zonePoints)
        {
            GetZoneClearanceInSteps();
            ClerancePoints = zonePoints.CreateZonePoints();
            return ClerancePoints;
        }



        public void ShowPoints(XYZ basePoint)
        {
            ClearancePointsConvertor clearancePointsConvertor = new ClearancePointsConvertor();
            List<XYZ> modelSpacePoints = clearancePointsConvertor.GetClearancePointsByXYZ(basePoint, ClerancePoints);

            Visualizator visualizator = new Visualizator(Main.Doc);

            visualizator.ShowPoints(new SpacePointsVisualization(modelSpacePoints));

        }
    }
}
