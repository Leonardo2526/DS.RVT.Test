using DS.RevitApp.Test.ConnectionPointService.PointModel;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.ConnectionPointService.SearchersModel
{
    internal class SeracherByMEPCurves : SearcherAlgorithm
    {
        public SeracherByMEPCurves(IPathFinder pathFinder, ITransfromBuilder transfromBuilder, MEPSystemModel mEPSystemModel) : 
            base(pathFinder, transfromBuilder, mEPSystemModel)
        {
        }

        public override (IConnectionPoint Point1, IConnectionPoint Point2) GetConnectionPoints()
        {
            throw new NotImplementedException();
        }
    }
}
