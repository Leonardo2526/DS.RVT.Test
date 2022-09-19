using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using Nito.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace DS.RevitApp.Test.TransformTest
{
    internal class AvailableMEPCurveService : AbstractAvailableCurveService<MEPCurve>
    {
        public AvailableMEPCurveService(List<MEPCurve> targetCurves, double minCurveLength, double minPlacementLength, bool saveElementsOrder = false) : 
            base(targetCurves, minCurveLength, minPlacementLength, saveElementsOrder)
        {
        }

        protected override double GetLength(MEPCurve curve)
        {
            return curve.GetCenterLine().ApproximateLength;
        }
    }
}
