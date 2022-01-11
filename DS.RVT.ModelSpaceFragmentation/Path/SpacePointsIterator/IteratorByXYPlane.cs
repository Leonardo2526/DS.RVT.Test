using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class IteratorByXYPlane : ISpacePointsIterator
    {
        public void Iterate()
        {
            int x, y, z, a;
            do
            {
                a = 0;

                z = InputData.Az;
                    for (y = 0; y < InputData.Ycount; y++)
                    {
                        for (x = 0; x < InputData.Xcount; x++)
                        {
                            if (!PointsMarkerIterator.Operation(x,y,z, ref a))
                                continue;
                        }
                    }
                
            } while (!PointsMarkerIterator.Grid.ContainsKey(PointsMarkerIterator.EndStepPoint) && a != 0);

        }
    }
}
