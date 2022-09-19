using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Transforms;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Models
{
    public class BasisTransformModel : AbstractTransformModel<Basis, Basis>
    {
        public BasisTransformModel(Basis sourceObject, Basis targetObject) : base(sourceObject, targetObject)
        {
        }

        public XYZ MoveVector { get; set; }
        public List<RotationModel> Rotations { get; set; } = new List<RotationModel>();
    }
}
