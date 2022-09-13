using Autodesk.Revit.DB;
using DS.RevitApp.Test.ConnectionPointService.PointModel;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.ConnectionPointService.SearchersModel
{
    internal class ConnectionSearchClient : AbstractConnectionSearcher
    {
        private readonly Element _baseElement;

        private Dictionary<FamilyInstance, XYZ> _listNode1;
        private Dictionary<FamilyInstance, XYZ> _listNode2;
        private readonly int _baseIndex;

        public ConnectionSearchClient(IPathFinder pathFinder, ITransfromBuilder transfromBuilder, MEPSystemModel mEPSystemModel) :
            base(pathFinder, transfromBuilder, mEPSystemModel)
        {
            _baseElement = _mEPSystemModel.Root.BaseElement;
            _baseIndex = _mEPSystemModel.Root.Elements.FindIndex(obj => obj.Id == _baseElement.Id);
        }

        public override (IConnectionPoint Point1, IConnectionPoint Point2) GetConnectionPoints()
        {
            var (con1, con2) = ConnectorUtils.GetMainConnectors(_baseElement);
            _listNode1 = GetListNode(_baseElement, con1);
            _listNode2 = GetListNode(_baseElement, con2);


            var searcher1 = new SearcherByNodes(_pathFinder, _transfromBuilder, _mEPSystemModel);
            var searcher2 = new SeracherByMEPCurves(_pathFinder, _transfromBuilder, _mEPSystemModel);
            searcher1.SetSuccessor(searcher2);

            var result = searcher1.GetConnectionPoints();
            Path = searcher1.Path;
            FamInstTransforms = searcher1.FamInstTransforms;

            return result;
        }

        private Dictionary<FamilyInstance, XYZ> GetListNode(Element baseElement, Connector connector)
        {
            var listNode = new Dictionary<FamilyInstance, XYZ>();

            var connectedElem = ConnectorUtils.GetConnectedByConnector(connector, baseElement);
            if (connectedElem is null)
                return listNode;

            int connectedElemInd = _mEPSystemModel.Root.Elements.IndexOf(connectedElem);
            int dInd = _baseIndex - connectedElemInd;
            List<FamilyInstance> fittings = null;
            if (dInd > 0)
            {
                fittings = _mEPSystemModel.Root.Fittings.
                    Where(obj => _mEPSystemModel.Root.Elements.IndexOf(obj) < _baseIndex).ToList();
            }
            else
            {
                fittings = _mEPSystemModel.Root.Fittings.
                    Where(obj => _mEPSystemModel.Root.Elements.IndexOf(obj) > _baseIndex).ToList();
            }


            if (fittings is not null && fittings.Any())
            {
                foreach (var fam in fittings)
                {
                    XYZ lp = fam.GetLocationPoint();
                    listNode.Add(fam, lp);
                }
            }


            return listNode;
        }

    }
}
