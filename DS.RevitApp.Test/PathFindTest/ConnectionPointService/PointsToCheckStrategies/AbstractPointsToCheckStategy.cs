using Autodesk.Revit.DB;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointsToCheckStrategies;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFindTest.ConnectionPointService
{
    public abstract class AbstractPointsToCheckStategy : IPointsToCheckStrategy
    {
        protected readonly MEPSystemModel _mEPSystemModel;
        protected readonly Element _baseElement;
        protected readonly XYZ _collisionCenter;

        protected AbstractPointsToCheckStategy(MEPSystemModel mEPSystemModel, Element baseElement, XYZ collisionCenter)
        {
            _mEPSystemModel = mEPSystemModel;
            _baseElement = baseElement;
            _collisionCenter = collisionCenter;
        }

        public List<IConnectionPoint> PointsToCheck { get; protected set; } = new List<IConnectionPoint>();
        public IPointsToCheckStrategy Successor { get; protected set; }


        public abstract List<IConnectionPoint> GetPointsToCheck(Connector baseConnector);
        public void SetSuccessor(IPointsToCheckStrategy successor)
        {
            Successor = successor;
        }

        protected XYZ GetChildNodePoint(FamilyInstance fitting, List<Element> spanElements)
        {
            var node = _mEPSystemModel.Root.ChildrenNodes.Where(obj => obj.Element.Id == fitting.Id).First();
            var builder = new ChildPointBuilder(spanElements, node, _baseElement, _collisionCenter);
            return builder.Build();
        }
    }
}
