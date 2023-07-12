using DS.PathSearch.GridMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    internal class BestPathRequirement : IDoublePathRequiment
    {
        public BestPathRequirement(double clearance, double minAngleDistance)
        {
            Clearance = clearance;
            MinAngleDistance = minAngleDistance;
        }

        public double Clearance { get; }

        public double MinAngleDistance {get; }
    }
}
