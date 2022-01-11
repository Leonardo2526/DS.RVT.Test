using DS.RVT.ModelSpaceFragmentation.Points;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PointsMarkerIterator
    {
        static int d;
    
        public static StepPoint StartStepPoint;
        public static StepPoint EndStepPoint;

        public static Dictionary<StepPoint, int> Grid;
        private static List<StepPoint> initialPriorityList;
        private static List<StepPoint> clzPoints;

        public static bool IfWaveReachedEndPoint()
        {
            FillData();
            Iterate(new IteratorByXYPlane());

            if (!Grid.ContainsKey(EndStepPoint))
                Iterate(new IteratorBy3D());


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

           
            Priority PriorityInstance = new Priority();
            initialPriorityList = PriorityInstance.GetPriorities();
        }

        static void Iterate(ISpacePointsIterator iteratorByPlane)
        {
            Grid = new Dictionary<StepPoint, int>
            {
                { StartStepPoint, 1 }
            };
            iteratorByPlane.Iterate();
        }

        public static bool Operation(int x, int y, int z, ref int a)
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
