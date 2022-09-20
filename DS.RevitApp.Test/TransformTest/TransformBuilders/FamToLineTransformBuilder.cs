using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
    internal class FamToLineTransformBuilder : AbstractTransformBuilder<SolidModelExt, LineModel>
    {
        private readonly List<ICollisionChecker> _collisionCheckers;
        private readonly double _placementLength;
        private readonly List<XYZ> _points;
        private readonly MEPCurveModel _mEPCurveModel;
        private readonly double _minCurveLength;


        public FamToLineTransformBuilder(SolidModelExt sourceObject, LineModel targetObject,
            List<ICollisionChecker> collisionCheckers, double placementLength, List<XYZ> points, 
            MEPCurveModel mEPCurveModel, double minCurveLength) :
            base(sourceObject, targetObject)
        {
            _collisionCheckers = collisionCheckers;
            _placementLength = placementLength;
            _points = points;
            _mEPCurveModel = mEPCurveModel;
            _minCurveLength = minCurveLength;
            _operationObject = sourceObject.Clone();
        }

        public override AbstractTransformModel<SolidModelExt, LineModel> Build()
        {
            TargetPlacementModel targetModel = new TargetModelBuilder(_targetObject, _placementLength, _points).Build();

            var finder = new PositionFinder(targetModel, _sourceObject, _collisionCheckers, _mEPCurveModel, _minCurveLength);
            finder.Find();
            _sourceObject.Basis.Show(_operationObject.Element.Document);
            _operationObject.Basis.Show(_operationObject.Element.Document);
            UIDocument uIDocument = new UIDocument(_operationObject.Element.Document);
            uIDocument.RefreshActiveView();

            var transformModel = new BasisTransformBuilder(_operationObject.Basis, _sourceObject.Basis).Build() as BasisTransformModel;
            var model = new FamToLineTransformModel(_sourceObject, _targetObject);
            model.Transforms = transformModel.Transforms;
            model.MoveVector = transformModel.MoveVector;
            model.Rotations = transformModel.Rotations;

            return model;
        }
    }
}
