using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;

namespace DS.RevitApp.Test.TransformTest
{
    internal class LineModel
    {
        public LineModel(Line line, Basis basis)
        {
            Line = line;
            Basis = basis;
        }

        public LineModel(MEPCurve mEPCurve)
        {
            Line = MEPCurveUtils.GetLine(mEPCurve);
            Basis = mEPCurve.GetBasis();
        }

        public Line Line { get; }
        public Basis Basis { get; private set; }

    }
}
