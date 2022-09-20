using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitApp.Test.TransformTest.TransformModels;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Lines;
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
    internal class BasisLineTransformBuilder : AbstractTransformBuilder<Basis, Line>
    {
        public BasisLineTransformBuilder(Basis sourceObject, Line targetObject) : base(sourceObject, targetObject)
        {
        }

        /// <summary>
        /// Build transform to align centerPoints and sourceBasis.X direction with line direction.
        /// </summary>
        /// <returns></returns>
        public override AbstractTransformModel<Basis, Line> Build()
        {
            var transformModel = new BasisLineTransformModel(_sourceObject, _targetObject);

            //add move transform
            var moveVector = _targetObject.GetCenter() - _sourceObject.Point;
            Transform transform = Transform.CreateTranslation(moveVector);
            transformModel.Transforms.Add(transform);
          
            //add rotate transform
                double angle;
                XYZ axis = _sourceObject.X.CrossProduct(_targetObject.Direction).RoundVector();
                if (axis.IsZeroLength())
                {
                    return transformModel;
                }
                else
                {
                    angle = _sourceObject.X.AngleTo(_targetObject.Direction);
                }
                transform = Transform.CreateRotationAtPoint(axis, angle, _sourceObject.Point);
                transformModel.Transforms.Add(transform);

            return transformModel;
        }
    }
}
