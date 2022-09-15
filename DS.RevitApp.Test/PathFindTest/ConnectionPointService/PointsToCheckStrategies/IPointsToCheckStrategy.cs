using Autodesk.Revit.DB;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointsToCheckStrategies
{
    public interface IPointsToCheckStrategy
    {
        public IPointsToCheckStrategy Successor { get; }
        public List<IConnectionPoint> PointsToCheck { get; }

        public void SetSuccessor(IPointsToCheckStrategy successor);
        public List<IConnectionPoint> GetPointsToCheck(Connector baseConnector);

    }
}
