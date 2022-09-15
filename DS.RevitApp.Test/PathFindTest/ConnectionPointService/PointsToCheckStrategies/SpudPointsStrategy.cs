using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointsToCheckStrategies
{
    internal class SpudPointsStrategy : AbstractPointsToCheckStategy
    {
        public SpudPointsStrategy(MEPSystemModel mEPSystemModel, Element baseElement, XYZ collisionCenter) : 
            base(mEPSystemModel, baseElement, collisionCenter)
        {
        }

        public override List<IConnectionPoint> GetPointsToCheck(Connector baseConnector)
        {
            //Check spuds connected to baseMEPCurve
            List<FamilyInstance> spudsToBase = _mEPSystemModel.Root.GetConnectedSpuds(_baseElement as MEPCurve);
            if (spudsToBase is not null && spudsToBase.Any())
            {
                var closestSpud = GetClosest(spudsToBase, baseConnector);
                if (closestSpud is not null)
                {
                    List<Element> spanElements = new List<Element>() { _baseElement, closestSpud };
                    XYZ lp = GetChildNodePoint(closestSpud, spanElements);
                    PointsToCheck.Add(new ConnectionPoint(lp, closestSpud));
                }
            }

            if (!PointsToCheck.Any())
            {
                PointsToCheck = Successor.GetPointsToCheck(baseConnector);
            }

            return PointsToCheck;
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
