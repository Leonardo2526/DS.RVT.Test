using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    internal class AvailableLineModel : AbstractAvailableCurveModel<Line>
    {
        public AvailableLineModel(List<Line> targetCurves, double minCurveLength, double minPlacementLength, bool saveElementsOrder = false) : 
            base(targetCurves, minCurveLength, minPlacementLength, saveElementsOrder)
        {
        }

        protected override double GetLength(Line curve)
        {
            return curve.ApproximateLength;
        }
    }
}
