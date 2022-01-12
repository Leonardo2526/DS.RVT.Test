using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class IteratorByYZPlane : ISpacePointsIterator
    {
        public void Iterate()
        {
            int x, y, z, a;
            do
            {
                a = 0;
                x = InputData.Ax;
                for (z = 0; z < InputData.Zcount; z++)
                {
                        for (y = 0; y < InputData.Ycount; y++)
                        {
                            if (!PointsMarkerIterator.Operation(x,y,z, ref a))
                                continue;
                        }
                }
            } while (!PointsMarkerIterator.Grid.ContainsKey(PointsMarkerIterator.EndStepPoint) && a != 0);

        }
    }
}
