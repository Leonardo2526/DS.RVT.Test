using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Transforms;
//using OLMP.RevitLib.MEPAC.LogMessage;
//using Revit.Async;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer
{
    internal class BasisTransformBuilder : AbstractTransformBuilder<Basis, Basis>
    {
        public BasisTransformBuilder(Basis sourceObject, Basis targetObject) : base(sourceObject, targetObject)
        {
        }

        /// <summary>
        /// Build transform to align two baseses
        /// </summary>
        /// <returns></returns>
        public override AbstractTransformModel<Basis, Basis> Build()
        {
            var transformModel = new BasisTransformModel(_sourceObject, _targetObject);

            transformModel.MoveVector = _targetObject.Point - _sourceObject.Point;
            Transform transform = Transform.CreateTranslation(transformModel.MoveVector);
            _sourceObject.Transform(transform);
            transformModel.Transforms.Add(transform);

            if (!_sourceObject.IsOrthogonal() | !_targetObject.IsOrthogonal())
            {
                string errors = "Basisis are not orthogonal.";
                //LogMessageCreator.CreateMessage(errors, TraceEventType.Error, SubType.General, _collision);
                return null;
            }
            bool targetOrthogonality = _targetObject.IsOrthogonal();
            if (_sourceObject.GetOrientaion() != _sourceObject.GetOrientaion())
            {
                string errors = "Orientaions are not equal.";
                //LogMessageCreator.CreateMessage(errors, TraceEventType.Error, SubType.General, _collision);
                return null;
            }

            int i = 0;
            (XYZ basis1, XYZ basis2) = GetNotEqualBasises(_sourceObject, _targetObject);
            while (basis1 is not null && i < 3)
            {
                double angle;
                XYZ axis = basis1.CrossProduct(basis2).RoundVector();
                if (axis.IsZeroLength())
                {
                    angle = 180.DegToRad();
                    axis = XYZUtils.GetPerpendicular(basis1,
                        new List<XYZ>() { _sourceObject.X, _sourceObject.Y, _sourceObject.Z }).First();
                }
                else
                {
                    angle = basis1.AngleTo(basis2);
                }
                transform = Transform.CreateRotationAtPoint(axis, angle, _sourceObject.Point);
                _sourceObject.Transform(transform);
                transformModel.Transforms.Add(transform);

                Line axisLine = Line.CreateBound(_sourceObject.Point, _sourceObject.Point + axis);
                transformModel.Rotations.Add(new RotationModel(axisLine, angle));
                (basis1, basis2) = GetNotEqualBasises(_sourceObject, _targetObject);
                i++;
            }
            if (i > 2)
            {
                string errors = "Failed to get transform model: number of rotation steps > 2";
                //LogMessageCreator.CreateMessage(errors, TraceEventType.Error, SubType.General, _collision);
                return null;
            }


            return transformModel;
        }

        private (XYZ basis1, XYZ basis2) GetNotEqualBasises(Basis basis1, Basis basis2)
        {
            double angleTolerance = 3.DegToRad();

            List<XYZ> basises1 = new List<XYZ>()
            {
                basis1.X, basis1.Y, basis1.Z
            };
            List<XYZ> basises2 = new List<XYZ>()
            {
                basis2.X, basis2.Y, basis2.Z
            };

            for (int i = 0; i < 3; i++)
            {
                if (!basises1[i].IsAlmostEqualTo(basises2[i], angleTolerance))
                {
                    return (basises1[i], basises2[i]);
                }
            }

            return (null, null);
        }

    }
}
