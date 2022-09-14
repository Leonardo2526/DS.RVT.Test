using Autodesk.Revit.DB;
using DS.RevitApp.Test.ConnectionPointService.PointModel;
using DS.RevitApp.Test.PathFinders;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.ConnectionPointService.SearchersModel
{
    public abstract class AbstractConnectionSearcher
    {
        protected readonly IPathFinder _pathFinder;
        protected readonly ITransfromBuilder _transfromBuilder;
        protected readonly MEPSystemModel _mEPSystemModel;

        protected AbstractConnectionSearcher(IPathFinder pathFinder, ITransfromBuilder transfromBuilder, MEPSystemModel mEPSystemModel)
        {
            _pathFinder = pathFinder;
            _transfromBuilder = transfromBuilder;
            _mEPSystemModel = mEPSystemModel;
        }

        public IConnectionPoint Point1 { get; protected set; }
        public IConnectionPoint Point2 { get; protected set; }
        public List<XYZ> Path { get; protected set; }
        public Dictionary<FamilyInstance, Transform> FamInstTransforms { get; protected set; }

        public abstract (IConnectionPoint Point1, IConnectionPoint Point2) GetConnectionPoints();
    }
}
