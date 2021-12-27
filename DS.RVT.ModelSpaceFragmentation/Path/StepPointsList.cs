using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class StepPointsList
    {
        public static List<StepPoint> XPoints { get; } = new List<StepPoint>()
            {
                 new StepPoint(-1, 0, 0),
                new StepPoint(1, 0, 0)
            };

        public static List<StepPoint> YPoints { get; } = new List<StepPoint>()
            {
                 new StepPoint(0, -1, 0),
                new StepPoint(0, 1, 0),
            };

        public static List<StepPoint> ZPoints { get;} = new List<StepPoint>()
            {
                  new StepPoint(0, 0, -1),
                new StepPoint(0, 0, 1)
            };

        private static List<StepPoint> allPoints;

        public static List<StepPoint> AllPoints
        {
            get
            {
                allPoints = new List<StepPoint>();
                allPoints.AddRange(XPoints);
                allPoints.AddRange(YPoints);
                allPoints.AddRange(ZPoints);
                return allPoints;
            }

        }
    }
}
