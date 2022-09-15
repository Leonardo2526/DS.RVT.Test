using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFindTest.Solution.Creators
{
    internal interface ISolutionCreatorStrategy
    {
        public ICollisionSolutionModel GetSolution(List<IConnectionPoint> points1, List<IConnectionPoint> points2);
    }
}
