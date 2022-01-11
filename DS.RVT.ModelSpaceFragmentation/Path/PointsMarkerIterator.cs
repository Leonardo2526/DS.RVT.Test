using DS.RVT.ModelSpaceFragmentation.Points;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PointsMarkerIterator
    {
        static int x, y, z, d, a;
    
        public static StepPoint StartStepPoint;
        public static StepPoint EndStepPoint;

        public static Dictionary<StepPoint, int> Grid;
        private static List<StepPoint> initialPriorityList;
        private static List<StepPoint> clzPoints;

        public static bool IfWaveReachedEndPoint()
        {
            FillData();
            Iterate();

            if (!Grid.ContainsKey(EndStepPoint))
                return false;

            return true;
        }

        static void FillData()
        {
         
            StartStepPoint = new StepPoint(InputData.Ax, InputData.Ay, InputData.Az);
            EndStepPoint = new StepPoint(InputData.Bx, InputData.By, InputData.Bz);

            CLZCretor clzCreator = new CLZCretor();
            clzPoints = clzCreator.Create(new CLZByBoders());

            List<StepPoint> stepPointsList = new List<StepPoint>();
            stepPointsList.Add(StartStepPoint);

            Grid = new Dictionary<StepPoint, int>
            {
                { StartStepPoint, 1 }
            };
            Priority PriorityInstance = new Priority();
            initialPriorityList = PriorityInstance.GetPriorities();
        }

        static void Iterate()
        {
            do
            {
                a = 0;
                for (z = 0; z < InputData.Zcount; z++)
                {
                    for (y = 0; y < InputData.Ycount; y++)
                    {
                        for (x = 0; x < InputData.Xcount; x++)
                        {
                            if (!Operation())
                                continue;
                        }
                    }
                }
            } while (!Grid.ContainsKey(EndStepPoint) && a != 0);
        }

        static bool Operation()
        {
            StepPoint currentPoint = new StepPoint(x, y, z);

            int currentD;
            if (!Grid.ContainsKey(currentPoint))
                return false;
            else
                currentD = Grid[currentPoint];

            //Create points cloud for next check from unpassible points
            PointsCloud pointsCloud = new PointsCloud(SpaceFragmentator.UnpassablePoints);
            List<StepPoint> unpassableByCLZPoints =
                pointsCloud.GetStepPointByCenterPoint(PointConvertor.StepPointToXYZ(currentPoint));

            PointMarker pointMarker =
                new PointMarker(currentPoint, clzPoints, unpassableByCLZPoints, initialPriorityList, currentD);
            pointMarker.Mark(ref Grid, ref d, ref a);

            return true;
        }
    }
}
