using Autodesk.Revit.DB;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using DS.RevitApp.Test.PathFindTest.PathFinders;
using DS.RevitApp.Test.PathFindTest.Solution.Creators;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitApp.Test.PathFindTest.Solution
{
    internal class LoopCheckPointsStrategy : ISolutionCreatorStrategy
    {
        private readonly IPathFinder _pathFinder;
        private readonly ITransfromBuilder _transfromBuilder;
        private readonly MEPSystemModel _mEPSystemModel;
        private bool FamInstTransformAvailable = true;

        public LoopCheckPointsStrategy(IPathFinder pathFinder, ITransfromBuilder transfromBuilder, MEPSystemModel mEPSystemModel)
        {
            _pathFinder = pathFinder;
            _transfromBuilder = transfromBuilder;
            _mEPSystemModel = mEPSystemModel;
        }

        public ICollisionSolutionModel GetSolution(List<IConnectionPoint> points1, List<IConnectionPoint> points2)
        {
            foreach (var p1 in points1)
            {
                foreach (var p2 in points2)
                {
                    List<XYZ> path = GetPath(p1.Point, p2.Point);
                    if (path is not null && path.Any())
                    {
                        //var transform = TransformBuilder(p1, p2, path);
                        if (FamInstTransformAvailable)
                        {
                            return new SketchSolutionModel(p1, p2, path, null);
                        }
                    }
                }
            }

            return null;
        }


        private List<XYZ> GetPath(XYZ point1, XYZ point2)
        {
            return _pathFinder.FindPath(point1, point2);
        }

        private Dictionary<FamilyInstance, Transform> GetFamInstTransform(List<FamilyInstance> familyInstances, List<XYZ> path)
        {
            var dict = _transfromBuilder.Build();
            //FamInstTransformAvailable = _transfromBuilder.Available;
            return dict;
        }

        private Dictionary<FamilyInstance, Transform> TransformBuilder(IConnectionPoint p1, IConnectionPoint p2, List<XYZ> path)
        {
            ConnectionPoint cp1 = p1 as ConnectionPoint;
            ConnectionPoint cp2 = p2 as ConnectionPoint;

            var elements = _mEPSystemModel.Root.GetElements(cp1.Element, cp2.Element);
            var famInstances = elements.OfType<FamilyInstance>().ToList();

            if (!famInstances.Any())
            {
                return new Dictionary<FamilyInstance, Transform>();
            }
            else
            {
                return GetFamInstTransform(famInstances, path);
            }
        }
    }
}
