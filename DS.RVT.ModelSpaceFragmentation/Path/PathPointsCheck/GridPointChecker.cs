using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class GridPointChecker
    {
        public GridPointChecker(StepPoint nextPoint, List<StepPoint> cLZPoints, List<StepPoint> unpassableByCLZPoints, int currentValue)
        {
            NextPoint = nextPoint;
            CLZPoints = cLZPoints;
            UnpassableByCLZPoints = unpassableByCLZPoints;
            CurrentValue = currentValue;
        }

        public StepPoint NextPoint { get; set; }
        public List<StepPoint> CLZPoints { get; set; }
        public List<StepPoint> UnpassableByCLZPoints { get; set; }
        public int CurrentValue { get; set; }

        public void Check(ref Dictionary<StepPoint, int> Grid, ref int d, ref int a)
        {
            PointsCheker pointsCheker = new PointsCheker();

            if (!Grid.ContainsKey(NextPoint))
            {

                bool checkUnpassablePoint = pointsCheker.IsPointPassable(NextPoint);
                bool checkClearancePoint = pointsCheker.IsClearanceZoneAvailable(NextPoint, CLZPoints, UnpassableByCLZPoints);
                if (checkUnpassablePoint & checkClearancePoint)
                {
                    // распространяем волну
                    d = CurrentValue + 1;
                    Grid.Add(NextPoint, d);
                    a++;
                }
            }
        }
    }
}
