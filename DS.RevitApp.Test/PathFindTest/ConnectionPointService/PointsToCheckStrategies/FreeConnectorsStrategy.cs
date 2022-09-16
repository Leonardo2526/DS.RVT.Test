using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;

namespace DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointsToCheckStrategies
{
    internal class FreeConnectorsStrategy : IPointsToCheckStrategy
    {
        protected readonly MEPSystemModel _mEPSystemModel;

        public FreeConnectorsStrategy(MEPSystemModel mEPSystemModel)
        {
            _mEPSystemModel = mEPSystemModel;
        }

        public List<IConnectionPoint> PointsToCheck { get; protected set; } = new List<IConnectionPoint>();
        public IPointsToCheckStrategy Successor { get; protected set; }

        public List<IConnectionPoint> GetPointsToCheck(Connector baseConnector)
        {
            PointsToCheck = new List<IConnectionPoint>();
            var elems = new List<Element>();
            elems.Add(_mEPSystemModel.Root.BaseElement);
            var span = _mEPSystemModel.Root.GetElementsSpan(baseConnector) ?? new List<Element>();           
            elems.AddRange(span);
            var elem = elems.Last();
            var c = ConnectorUtils.GetFreeConnector(elem).First();
            PointsToCheck.Add(new ConnectionPoint(c.Origin, elem));

            if (!PointsToCheck.Any())
            {
                PointsToCheck = Successor.GetPointsToCheck(baseConnector);
            }

            return PointsToCheck;
        }

        public void SetSuccessor(IPointsToCheckStrategy successor)
        {
            Successor = successor;
        }
    }
}
