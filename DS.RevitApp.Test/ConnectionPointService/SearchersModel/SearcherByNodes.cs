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
    internal class SearcherByNodes : SearcherAlgorithm
    {
        private readonly List<IConnectionPoint> _points1;
        private readonly List<IConnectionPoint> _points2;

        public SearcherByNodes(IPathFinder pathFinder, ITransfromBuilder transfromBuilder, MEPSystemModel mEPSystemModel, 
            List<IConnectionPoint> points1, List<IConnectionPoint> points2) : 
            base(pathFinder, transfromBuilder, mEPSystemModel)
        {
            _points1 = points1;
            _points2 = points2;
        }

        public override (IConnectionPoint Point1, IConnectionPoint Point2) GetConnectionPoints()
        {

            foreach (ConnectionPoint p1 in _points1)
            {
                foreach (ConnectionPoint p2 in _points2)
                {
                    List<XYZ> path = GetPath(p1.Point, p2.Point);
                    if (path is not null && path.Any())
                    {
                        Path = path;
                        var elements = _mEPSystemModel.Root.GetElements(p1.Element, p2.Element);
                        var famInstances = elements.OfType<FamilyInstance>().ToList();
                        return (p1, p2);

                        //if (!famInstances.Any())
                        //{
                        //    return (p1, p2);
                        //}
                        //else
                        //{
                        //    var ft = GetFamInstTransform(famInstances, path);
                        //    if (ft is not null)
                        //    {
                        //        FamInstTransforms = ft;
                        //        return (p1, p2);
                        //    }
                        //}
                    }
                }
            }


            if (Path is null && Successor is not null)
            {
                return Successor.GetConnectionPoints();
            }

            return (null, null);
        }
    }
}
