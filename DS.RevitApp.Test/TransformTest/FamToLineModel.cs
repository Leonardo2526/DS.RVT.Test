using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    internal class FamToLineModel : AbstractTransformModel<FamilyInstance, Line>
    {
        public FamToLineModel(FamilyInstance sourceObject, Line targetObject) : 
            base(sourceObject, targetObject)
        {
        }
    }
}
