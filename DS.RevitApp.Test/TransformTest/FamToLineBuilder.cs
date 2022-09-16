using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    internal class FamToLineBuilder : AbstractTransformBuilder<FamilyInstance, Line>
    {
        public FamToLineBuilder(FamilyInstance sourceObject, Line targetObject) : 
            base(sourceObject, targetObject)
        {
        }

        public override AbstractTransformModel<FamilyInstance, Line> Build(FamilyInstance sourceObject, Line targetObject)
        {
            throw new NotImplementedException();
        }
    }
}
