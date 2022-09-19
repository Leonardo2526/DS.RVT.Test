using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest.TransformModels
{
    internal class BasisLineTransformModel : AbstractTransformModel<Basis, Line>
    {
        public BasisLineTransformModel(Basis sourceObject, Line targetObject) : 
            base(sourceObject, targetObject)
        {
        }
    }
}
