using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.ConnectionPointService.SearchersModel
{
    public abstract class SearcherAlgorithm : AbstractConnectionSearcher
    {
        protected SearcherAlgorithm Successor { get; set; }

        protected SearcherAlgorithm(IPathFinder pathFinder, ITransfromBuilder transfromBuilder, MEPSystemModel mEPSystemModel) :
            base(pathFinder, transfromBuilder, mEPSystemModel)
        {
        }

        public void SetSuccessor(SearcherAlgorithm successor)
        {
            this.Successor = successor;
        }

        protected List<XYZ> GetPath(XYZ point1, XYZ point2)
        {
            return _pathFinder.FindPath();
        }
        protected Dictionary<FamilyInstance, Transform> GetFamInstTransform(List<FamilyInstance> familyInstances, List<XYZ> path)
        {
            return _transfromBuilder.Build();
        }
    }
}
