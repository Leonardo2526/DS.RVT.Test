using DS.PathSearch.GridMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    internal class BestPathRequirement : IPathRequiment
    {
        public BestPathRequirement(byte clearance, byte minAngleDistance)
        {
            Clearance = clearance;
            MinAngleDistance = minAngleDistance;
        }

        public byte Clearance { get; }

        public byte MinAngleDistance {get; }
    }
}
