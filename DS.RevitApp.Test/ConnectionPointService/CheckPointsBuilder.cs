using Autodesk.Revit.DB;
using DS.RevitApp.Test.ConnectionPointService.PointModel;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.ConnectionPointService
{
    internal class CheckPointsBuilder
    {
        private readonly MEPSystemModel _mEPSystemModel;
        private readonly Element _baseElement;
        private readonly int _baseIndex;
        private List<Element> _spanElements;

        public CheckPointsBuilder(MEPSystemModel mEPSystemModel, Element baseElement)
        {
            _mEPSystemModel = mEPSystemModel;
            _baseElement = baseElement;
            _baseIndex = _mEPSystemModel.Root.Elements.FindIndex(obj => obj.Id == _baseElement.Id);
        }

        public List<IConnectionPoint> Build(Connector baseConnector)
        {
            List<IConnectionPoint> points = new List<IConnectionPoint>();

            List<FamilyInstance> fittings = GetFittings(baseConnector);

            var childIds = _mEPSystemModel.Root.ChildrenNodes.Select(obj => obj.Element.Id);

            if (fittings is not null && fittings.Any())
            {
                //Exclude childs
                var fittingIds = fittings.Select(obj => obj.Id);
                var childElemsIds = fittingIds.Intersect(childIds);
                if (childElemsIds.Any())
                {
                    var childElem = fittings.Where(obj => obj.Id == childElemsIds.First()).First();
                    XYZ lp = GetChildNodePoint(childElem);
                    points.Add(new ConnectionPoint(lp, childElem));
                    return points;
                }

                //add all fitting points
                foreach (var fam in fittings)
                {
                    XYZ lp = fam.GetLocationPoint();
                    points.Add(new ConnectionPoint(lp, fam));
                }
            }

            points.Reverse();

            return points;
        }

        public List<FamilyInstance> GetFittings(Connector baseConnector)
        {
            var points = new List<IConnectionPoint>();
            _spanElements = new List<Element>();

            var connectedElem = ConnectorUtils.GetConnectedByConnector(baseConnector, _baseElement);
            if (connectedElem is null)
                return null;

            int connectedElemInd = _mEPSystemModel.Root.Elements.FindIndex(obj => obj.Id == connectedElem.Id);
            int dInd = _baseIndex - connectedElemInd;
            List<FamilyInstance> fittings = null;
            if (dInd > 0)
            {
                _spanElements.AddRange(_mEPSystemModel.Root.GetElements(_mEPSystemModel.Root.Elements.First(), connectedElem));
                _spanElements.Add(_baseElement);
                _spanElements.Reverse();
                var fIds = _spanElements.Select(obj => obj.Id).Intersect(_mEPSystemModel.Root.Fittings.Select(obj => obj.Id))?.ToList();
                fittings = _mEPSystemModel.Root.Fittings.Where(obj => fIds.Contains(obj.Id))?.Reverse().ToList();
            }
            else
            {
                _spanElements.Add(_baseElement);
                _spanElements.AddRange(_mEPSystemModel.Root.GetElements(connectedElem, _mEPSystemModel.Root.Elements.Last()));
                var fIds = _spanElements.Select(obj => obj.Id).Intersect(_mEPSystemModel.Root.Fittings.Select(obj => obj.Id))?.ToList();
                fittings = _mEPSystemModel.Root.Fittings.Where(obj => fIds.Contains(obj.Id))?.ToList();
            }

            return fittings;
        }

        private XYZ GetChildNodePoint(FamilyInstance fitting)
        {
            var node = _mEPSystemModel.Root.ChildrenNodes.Where(obj => obj.Element.Id == fitting.Id).First();
            var builder = new ChildPointBuilder(_spanElements, node, _baseElement, _baseElement.GetLocationPoint()); 
            return builder.Build(); 
        }
    }
}
