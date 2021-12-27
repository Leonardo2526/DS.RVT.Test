using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class StepPoint
    {
        public StepPoint(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

    }
}
