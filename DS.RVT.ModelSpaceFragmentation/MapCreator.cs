using DSUtils;
using DSUtils.GridMap;

namespace DS.RVT.ModelSpaceFragmentation
{
    class MapCreator : IMap
    {
        public Location Start { get; set; }
        public Location Goal { get; set; }
        public int[,,] Matrix { get; set; }
    }
}
