using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitApp.Test.TransformTest;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer
{

   
    internal class PositionFinder
    {
        private readonly TargetPlacementModel _targetModel;
        private SolidModelExt _operationModel;
        private readonly List<ICollisionChecker> _collisionCheckers;
        private readonly MEPCurveModel _mEPCurveModel;
        private readonly double _minCurveLength;


        //private readonly MEPCollision _collision;

        public PositionFinder(TargetPlacementModel targetLine, SolidModelExt opertationModel,
            List<ICollisionChecker> collisionCheckers, MEPCurveModel _mEPCurveModel, double minCurveLength)
        {
            _targetModel = targetLine;
            _operationModel = opertationModel;
            _collisionCheckers = collisionCheckers;
            this._mEPCurveModel = _mEPCurveModel;
            _minCurveLength = minCurveLength;
            //this._collision = _collision;
        }

        /// <summary>
        /// Find available operation model's position on targerMEPCurve.
        /// </summary>
        public void Find()
        {
            //Place and align solid in point
            Basis targetBasis = GetTargetBasis(_targetModel);

            var transformModel = new BasisTransformBuilder(_operationModel.Basis, targetBasis).Build();
            _operationModel.Transform(transformModel.Transforms);
            //_operationModel.ShowBoundingBox();
            //DocModel.UiDoc.RefreshActiveView();

            if (_collisionCheckers is null)
            {
                return;
            }

            var checkedObjects1 = new List<SolidModelExt>() { _operationModel };
            var collisions = new List<ICollision>();
            foreach (ICollisionChecker checker in _collisionCheckers)
            {
                List<ICollision> col = null;
                if (checker is SolidCollisionChecker)
                {
                    SolidCollisionChecker solidChecker = (SolidCollisionChecker)checker;
                    col = solidChecker.GetCollisions(checkedObjects1);
                }
                else if(checker is LinkCollisionChecker)
                {
                    LinkCollisionChecker solidChecker = (LinkCollisionChecker)checker;
                    col = solidChecker.GetCollisions(checkedObjects1);
                }
                collisions.AddRange(col);
            }

            if (!collisions.Any())
            {
                return;
            }

            //Search available position for solid
            var solidElemCollisions = collisions.Cast<SolidElemCollision>().ToList();
            var solidCollisionClient = new SolidCollisionClient(solidElemCollisions, _collisionCheckers, _targetModel, _minCurveLength);
            solidCollisionClient.Resolve();
        }

        private Basis GetTargetBasis(TargetPlacementModel targetModel)
        {
            return
                  new Basis(targetModel.LineModel.Basis.X, targetModel.LineModel.Basis.Y, targetModel.LineModel.Basis.Z,
                  _targetModel.StartPlacementPoint);

            Basis basis = _operationModel.Basis.Clone();
           
            if (_mEPCurveModel.Width == _mEPCurveModel.Height)
            {
                var transforms = GetTransforms(_operationModel, targetModel);
                foreach (var transform in transforms)
                {
                    basis.Transform(transform);
                }
                return basis;
            }
            else
            {
                return
                    new Basis(targetModel.LineModel.Basis.X, targetModel.LineModel.Basis.Y, targetModel.LineModel.Basis.Z, 
                    _targetModel.StartPlacementPoint);
            }

        }

        private List<Transform> GetTransforms(SolidModelExt operationModel, TargetPlacementModel targetModel)
        {
            var transforms = new List<Transform>();

            //get move transform
            var moveVector = _targetModel.StartPlacementPoint - operationModel.Basis.Point;
            Transform moveTransform = Transform.CreateTranslation(moveVector);
            transforms.Add(moveTransform);

            //get rotate transform
            if (operationModel.Basis.X.IsAlmostEqualTo(targetModel.LineModel.Basis.X, 3.DegToRad()))
            {
                return transforms;
            }
            else
            {
                double angle;
                XYZ axis = operationModel.Basis.X.CrossProduct(targetModel.LineModel.Basis.X).RoundVector().Normalize();
                if (axis.IsZeroLength())
                {
                    angle = 180.DegToRad();
                    axis = XYZUtils.GetPerpendicular(operationModel.Basis.X,
                        new List<XYZ>() { operationModel.Basis.X, operationModel.Basis.Y, operationModel.Basis.Z }).First();
                }
                else
                {
                    angle = operationModel.Basis.X.AngleTo(targetModel.LineModel.Basis.X);
                }
                Transform rotateTransform = Transform.CreateRotationAtPoint(axis, angle, new XYZ(0,0,0));
                transforms.Add(rotateTransform);
            }

            return transforms;
        }
    }
}
