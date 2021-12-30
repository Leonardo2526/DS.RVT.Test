using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PointMarker
    {
        public PointMarker(StepPoint currentPoint, 
            List<StepPoint> cLZPoints, List<StepPoint> unpassableByCLZPoints, List<StepPoint> initialPriorityList, 
            int currentD)
        {
            CurrentPoint = currentPoint;
            CLZPoints = cLZPoints;
            UnpassableByCLZPoints = unpassableByCLZPoints;
            InitialPriorityList = initialPriorityList;
            CurrentD = currentD;
        }

        public StepPoint CurrentPoint { get; set; }
        public List<StepPoint> CLZPoints { get; set; }
        public List<StepPoint> UnpassableByCLZPoints { get; set; }
        public List<StepPoint> InitialPriorityList { get; set; }
        public int CurrentD { get; set; }

        public void Mark(ref Dictionary<StepPoint, int> Grid, ref int d, ref int a)
        {
            int k;
            // проходим по всем непомеченным соседям
            for (k = 0; k < 6; ++k)
            {
                int ix = CurrentPoint.X + InitialPriorityList[k].X,
                    iy = CurrentPoint.Y + InitialPriorityList[k].Y,
                    iz = CurrentPoint.Z + InitialPriorityList[k].Z;

                if (ix >= 0 && ix < InputData.Xcount &&
                    iy >= 0 && iy < InputData.Ycount &&
                    iz >= 0 && iz < InputData.Zcount)
                {
                    StepPoint nextPoint = new StepPoint(ix, iy, iz);

                    GridPointChecker gridPointChecker =
                        new GridPointChecker(nextPoint, CLZPoints, UnpassableByCLZPoints, CurrentD);
                    gridPointChecker.Check(ref Grid, ref d, ref a);
                }
            }
        }
    }
}
