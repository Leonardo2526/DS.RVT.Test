using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    internal class AvailableLineService : AbstractAvailableCurveService<LineModel>
    {
        public AvailableLineService(List<LineModel> targetCurves, double minCurveLength, double minPlacementLength, bool saveElementsOrder = false) : 
            base(targetCurves, minCurveLength, minPlacementLength, saveElementsOrder)
        {
        }

        protected override double GetLength(LineModel curve)
        {
            return curve.Line.ApproximateLength;
        }
    }
}
