using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitApp.Test.ConnectionPointService.PointModel;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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
        private XYZ _collisionCenter;

        public CheckPointsBuilder(MEPSystemModel mEPSystemModel, Element baseElement)
        {
            _mEPSystemModel = mEPSystemModel;
            _baseElement = baseElement;
            _baseIndex = _mEPSystemModel.Root.Elements.FindIndex(obj => obj.Id == _baseElement.Id);
            _collisionCenter = _baseElement.GetLocationPoint();
        }

        public List<IConnectionPoint> Build(Connector baseConnector)
        {
            var points = new List<IConnectionPoint>();

            //Check spuds connected to baseMEPCurve
            List<FamilyInstance> spudsToBase = _mEPSystemModel.Root.GetConnectedSpuds(_baseElement as MEPCurve);
            if (spudsToBase is not null && spudsToBase.Any())
            {
                var closestSpud = GetClosest(spudsToBase, baseConnector);
                if (closestSpud is not null)
                {
                    List<Element> spanElements = new List<Element>() { _baseElement, closestSpud }; 
                    XYZ lp = GetChildNodePoint(closestSpud, spanElements);
                    points.Add(new ConnectionPoint(lp, closestSpud));
                    return points;
                }
            }

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
                    points.Add(new ConnectionPoint(lp, childElem));
                    return points;
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

        private XYZ GetChildNodePoint(FamilyInstance fitting, List<Element> spanElements)
        {
            var node = _mEPSystemModel.Root.ChildrenNodes.Where(obj => obj.Element.Id == fitting.Id).First();
            var builder = new ChildPointBuilder(spanElements, node, _baseElement, _collisionCenter); 
            return builder.Build(); 
        }

        private FamilyInstance GetClosest(List<FamilyInstance> spuds, Connector baseConnector)
        {
            XYZ dir = (baseConnector.Origin - _collisionCenter).Normalize();
            Line line = _baseElement.GetCenterLine();

            double dist = 1000;
            FamilyInstance familyInstance = null;

            foreach (var fam in spuds)
            {
                XYZ lp = fam.GetLocationPoint();
                XYZ projectLp = line.Project(lp).XYZPoint;
                XYZ curDir = (projectLp - _collisionCenter).Normalize();

                if (curDir.IsAlmostEqualTo(dir, 3.DegToRad()))
                {
                    double currentDist = _collisionCenter.DistanceTo(projectLp);
                    if (currentDist < dist)
                    {
                        dist = currentDist;
                        familyInstance = fam;
                    }
                }

            }

            return familyInstance;
        }

    }
}
