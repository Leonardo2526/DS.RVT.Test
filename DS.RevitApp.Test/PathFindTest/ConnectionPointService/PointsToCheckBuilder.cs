using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointsToCheckStrategies;
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

namespace DS.RevitApp.Test.PathFindTest.ConnectionPointService
{
    internal class PointsToCheckBuilder
    {
        private readonly IPointsToCheckStrategy _checkStategy;

        public PointsToCheckBuilder(IPointsToCheckStrategy checkStategy)
        {           
            _checkStategy = checkStategy;
        }

        public List<IConnectionPoint> Build(Connector baseConnector)
        {
            List<IConnectionPoint> result = _checkStategy.GetPointsToCheck(baseConnector);
            result.Reverse();
            return result;
        }
    }
}
