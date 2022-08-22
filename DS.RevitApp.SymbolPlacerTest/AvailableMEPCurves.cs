using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitApp.SymbolPlacerTest
{
    internal class AvailableMEPCurves
    {
        private readonly double _minMEPCurveLength;
        private readonly bool _orederedFamInstSequence;
        private readonly double _minPlacementLength;
        private double _placementLength;

        public AvailableMEPCurves(List<MEPCurve> targetMEPCurves, double minMEPCurveLength, double minPlacementLength,
            bool orederedFamInstSequence = true)
        {
            _minMEPCurveLength = minMEPCurveLength;
            _minPlacementLength = minPlacementLength;
            _orederedFamInstSequence = orederedFamInstSequence;

            List<MEPCurve> validMEPCurves = targetMEPCurves.Where(obj => CheckMinLength(obj)).ToList();
            validMEPCurves.ForEach(CurrentStack.Push);
        }

        public Stack<MEPCurve> ReserveStack { get; private set; }
        public Stack<MEPCurve> CurrentStack { get; private set; }

        public MEPCurve Get(double placementLength)
        {
            _placementLength = placementLength;
            MEPCurve mEPCurve = CurrentStack.Where(obj => AvailableForPlacement(obj)).FirstOrDefault();
            if (mEPCurve is null && !_orederedFamInstSequence)
            {
                mEPCurve = ReserveStack.Where(obj => AvailableForPlacement(obj)).FirstOrDefault();
            }

            return mEPCurve;
        }

        public bool CheckMinLength(MEPCurve mEPCurve)
        {
            double length = mEPCurve.GetCenterLine().ApproximateLength;
            if (length > _minPlacementLength)
            {
                return true;
            }

            return false;
        }

        private bool AvailableForPlacement(MEPCurve mEPCurve)
        {
            if (CheckCollisions(mEPCurve) && CheckLength(mEPCurve))
            {
                return true;
            }

            return false;
        }

        private bool CheckLength(MEPCurve mEPCurve)
        {
            double targetLength = mEPCurve.GetCenterLine().ApproximateLength;

            if (targetLength < _placementLength)
            {
                if (targetLength > _minPlacementLength)
                {
                    ReserveStack.Push(mEPCurve);
                }
                return false;
            }

            return true;
        }

        private bool CheckCollisions(MEPCurve mEPCurve)
        {
            //code here

            return true;
        }
    }
}
