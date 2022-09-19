using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Transforms;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    internal class FamToLineTransformBuilder : AbstractTransformBuilder<FamilyInstance, LineModel>
    {
        private readonly List<ICollisionChecker> _collisionCheckers;
        private readonly double _placementLength;
        private readonly SolidModelExt _solidModelExt;
        private readonly List<XYZ> _points;
        private readonly MEPCurveModel _mEPCurveModel;
        private readonly double _minCurveLength;


        public FamToLineTransformBuilder(FamilyInstance sourceObject, LineModel targetObject,
            List<ICollisionChecker> collisionCheckers, double placementLength, SolidModelExt solidModelExt, List<XYZ> points, 
            MEPCurveModel mEPCurveModel, double minCurveLength) :
            base(sourceObject, targetObject)
        {
            _collisionCheckers = collisionCheckers;
            _placementLength = placementLength;
            _solidModelExt = solidModelExt;
            _points = points;
            _mEPCurveModel = mEPCurveModel;
            _minCurveLength = minCurveLength;
        }

        public override AbstractTransformModel<FamilyInstance, LineModel> Build()
        {
            SolidModelExt operationModel = _solidModelExt.Clone();
            TargetLineModel targetLine = new TargetLineBuilder(_targetObject, _placementLength, _points).Build();

            var finder = new PositionFinder(targetLine, operationModel, _collisionCheckers, _mEPCurveModel, _minCurveLength);
            finder.Find();

            var transformModel = new BasisTransformBuilder(_solidModelExt.Basis, operationModel.Basis).Build() as BasisTransformModel;
            var model = new FamToLineTransformModel(_sourceObject, _targetObject);
            model.Transforms = transformModel.Transforms;

            return model;
        }
    }
}
