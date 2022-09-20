using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    internal class FamToLineTransformModel : AbstractTransformModel<SolidModelExt, LineModel>
    {
        public FamToLineTransformModel(SolidModelExt sourceObject, LineModel targetObject) : 
            base(sourceObject, targetObject)
        {
        }


        public XYZ MoveVector { get; set; }
        public List<RotationModel> Rotations { get; set; } = new List<RotationModel>();
    }
}
