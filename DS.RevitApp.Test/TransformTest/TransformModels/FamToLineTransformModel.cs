using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    internal class FamToLineTransformModel : AbstractTransformModel<FamilyInstance, LineModel>
    {
        public FamToLineTransformModel(FamilyInstance sourceObject, LineModel targetObject) : 
            base(sourceObject, targetObject)
        {
        }
    }
}
