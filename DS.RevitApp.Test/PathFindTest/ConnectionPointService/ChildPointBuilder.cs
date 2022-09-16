using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFindTest.ConnectionPointService
{
    internal class ChildPointBuilder
    {
        private readonly List<Element> _spanElements;
        private readonly NodeElement _nodeElement;
        private readonly Element _baseElement;
        private readonly XYZ _collisionCenter;

        public ChildPointBuilder(List<Element> spanElements, NodeElement nodeElement, Element baseElement, XYZ collisionCenter)
        {
            _spanElements = spanElements;
            _nodeElement = nodeElement;
            _baseElement = baseElement;
            _collisionCenter = collisionCenter;
        }
        public XYZ Build()
        {
            List<Element> elemsToNode = ConnectorUtils.GetConnectedElements(_nodeElement.Element);
            MEPCurve rootMEPCurve = GetRootMEPCurve(elemsToNode);
            if (rootMEPCurve is null)
            {
                throw new InvalidOperationException("rootMEPCurve is null");
            }

            if (ElementUtils.GetPartType(_nodeElement.Element) == PartType.Tee)
            {
                (Connector elem1Con, Connector elem2Con) = ConnectorUtils.GetCommonConnectors(rootMEPCurve, _nodeElement.Element);
                return elem2Con.Origin;
            }

            XYZ lp = GetSpudLocationPoint(_nodeElement.Element, rootMEPCurve);
            XYZ dir = GetOffsetDirection(rootMEPCurve, lp);

            double offsetDist = ElementUtils.GetSizeByVector(_nodeElement.Element, dir) / 2;

            return lp + dir.Multiply(offsetDist);
        }

        private MEPCurve GetRootMEPCurve(List<Element> elemsToNode)
        {
            ElementId elemId = null;
            int elemInd = 1000;
            if (elemsToNode.Count ==1)
            {
                return elemsToNode.First() as MEPCurve;
            }

            for (int i = 0; i < elemsToNode.Count; i++)
            {
                ElementId currentElemId = elemsToNode[i].Id;
                int currentElemInd = _spanElements.FindIndex(el => el.Id == currentElemId);

                if (currentElemInd < elemInd && currentElemInd != -1)
                {
                    elemId = currentElemId;
                    elemInd = currentElemInd;
                }
            }

            return elemsToNode.Where(obj => obj.Id == elemId).First() as MEPCurve;
        }

        private XYZ GetOffsetDirection(MEPCurve mEPCurve, XYZ lp)
        {
            XYZ dir = null;

            if (mEPCurve.Id == _baseElement.Id)
            {
                Line line = mEPCurve.GetCenterLine();
                XYZ colCenterProject = line.Project(_collisionCenter).XYZPoint;
                dir = colCenterProject - lp;
            }
            else
            {
                var (con1, con2) = ConnectorUtils.GetMainConnectors(mEPCurve);

                //Chose connector by elem index
                var elem1 = ConnectorUtils.GetConnectedByConnector(con1, mEPCurve);
                int elem1Ind = _spanElements.FindIndex(el => el.Id == elem1.Id);

                var elem2 = ConnectorUtils.GetConnectedByConnector(con2, mEPCurve);
                int elem2Ind = _spanElements.FindIndex(el => el.Id == elem2.Id);

                Connector connector = elem1Ind < elem2Ind ? con1 : con2;

                dir = connector.Origin - lp;
            }         

            if (!XYZUtils.Collinearity(mEPCurve.GetCenterLine().Direction, dir))
            {
                throw new InvalidOperationException("Direction collinearity error!");
            }

            return dir;
        }

        private XYZ GetSpudLocationPoint(FamilyInstance familyInstance, MEPCurve rootMEPCurve)
        {
            if (familyInstance.IsSpud())
            {
                XYZ cp = familyInstance.GetLocationPoint();
                Line line = rootMEPCurve.GetCenterLine();
                return line.Project(cp).XYZPoint;
            }

            return null;
        }


    }
}
