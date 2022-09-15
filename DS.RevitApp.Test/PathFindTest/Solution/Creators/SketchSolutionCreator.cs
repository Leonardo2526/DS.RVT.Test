using Autodesk.Revit.DB;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointsToCheckStrategies;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFindTest.Solution.Creators
{
    internal class SketchSolutionCreator : ISolutionCreator
    {
        private readonly Element _baseElement;
        private readonly ISolutionCreatorStrategy _creatorStrategy;       
        private readonly MEPSystemModel _mEPSystemModel;
        private readonly XYZ _collisionCenter;

        public SketchSolutionCreator(ISolutionCreatorStrategy creatorStrategy, MEPSystemModel mEPSystemModel)
        {
            _creatorStrategy = creatorStrategy;
            _mEPSystemModel = mEPSystemModel;
            _baseElement = _mEPSystemModel.Root.BaseElement;
            _collisionCenter = _baseElement.GetLocationPoint();
        }

        public ICollisionSolutionModel Create()
        {
            (List<IConnectionPoint> pointsToCheck1, List<IConnectionPoint> pointsToCheck2) = GetPointsToCheck();
            var solution = _creatorStrategy.GetSolution(pointsToCheck1, pointsToCheck2);
            return solution;          
        }

        private (List<IConnectionPoint> pointsToCheck1, List<IConnectionPoint> pointsToCheck2) GetPointsToCheck()
        {
            //get all available points to check collision solution
            var (con1, con2) = ConnectorUtils.GetMainConnectors(_baseElement);

            var mainSrategy = new SpudPointsStrategy(_mEPSystemModel, _baseElement, _collisionCenter);
            var fittingStrategy = new FittingPointStrategy(_mEPSystemModel, _baseElement, _collisionCenter);
            var freeConStrategy = new FreeConnectorsStrategy(_mEPSystemModel);
            mainSrategy.SetSuccessor(fittingStrategy);
            fittingStrategy.SetSuccessor(freeConStrategy);

            var builer = new PointsToCheckBuilder(mainSrategy);
            List<IConnectionPoint> pointsToCheck1 = builer.Build(con1);
            List<IConnectionPoint> pointsToCheck2 = builer.Build(con2);

            return (pointsToCheck1, pointsToCheck2);
        }

    }
}

