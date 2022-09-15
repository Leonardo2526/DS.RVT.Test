using Autodesk.Revit.DB;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointsToCheckStrategies
{
    internal class FittingPointStrategy : AbstractPointsToCheckStategy
    {
        private List<Element> _spanElements;
        private readonly int _baseIndex;

        public FittingPointStrategy(MEPSystemModel mEPSystemModel, Element baseElement, XYZ collisionCenter) : 
            base(mEPSystemModel, baseElement, collisionCenter)
        {
            _baseIndex = _mEPSystemModel.Root.Elements.FindIndex(obj => obj.Id == _baseElement.Id);
        }

        public override List<IConnectionPoint> GetPointsToCheck(Connector baseConnector)
        {
            //Check all fittings by this direction
            List<FamilyInstance> fittings = GetFittings(baseConnector);
            var childIds = _mEPSystemModel.Root.ChildrenNodes?.Select(obj => obj.Element.Id);
            if (fittings is not null && fittings.Any())
            {
                //Exclude childs
                var fittingIds = fittings.Select(obj => obj.Id);
                var childElemsIds = fittingIds.Intersect(childIds);
                if (childElemsIds.Any())
                {
                    var childElem = fittings.Where(obj => obj.Id == childElemsIds.First()).First();
                    XYZ lp = GetChildNodePoint(childElem, _spanElements);
                    PointsToCheck.Add(new ConnectionPoint(lp, childElem));
                    return PointsToCheck;
                }

                //add all fitting points
                foreach (var fam in fittings)
                {
                    XYZ lp = fam.GetLocationPoint();
                    var parentIds = _mEPSystemModel.Root.ParentNodes.Select(obj => obj.Element.Id).ToList();
                    if (parentIds.Contains(fam.Id) && fam.IsSpud())
                    {
                        var node = _mEPSystemModel.Root.ParentNodes.Where(obj => obj.Element.Id == fam.Id).First();
                        MEPCurve mEPCurve = node.RelationElement as MEPCurve;
                        Line line = mEPCurve.GetCenterLine();
                        lp = line.Project(lp).XYZPoint;
                    }
                    PointsToCheck.Add(new ConnectionPoint(lp, fam));
                }
            }

            if (!PointsToCheck.Any())
            {
                PointsToCheck = Successor.GetPointsToCheck(baseConnector);
            }

            return PointsToCheck;
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
    }
}
