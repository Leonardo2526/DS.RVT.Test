using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using Nito.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    public abstract class AbstractAvailableCurveModel<T>
    {
        protected readonly List<T> _targetCurves;
        protected readonly double _minCurveLength;
        protected readonly double _minPlacementLength;
        private readonly bool _saveElementsOrder;

        protected AbstractAvailableCurveModel(List<T> targetCurves, double minCurveLength, double minPlacementLength, bool saveElementsOrder = false)
        {
            _targetCurves = targetCurves;
            _minCurveLength = minCurveLength;
            _minPlacementLength = minPlacementLength;
            _saveElementsOrder = saveElementsOrder;
            List<T> validMEPCurves = targetCurves.Where(obj => CheckMinPlacementLength(obj)).ToList();
            validMEPCurves.ForEach(obj => AvailableCurves.AddToBack(obj));
        }

        public Deque<T> AvailableCurves { get; private set; } = new Deque<T>();

        public T Get(double placementLength)
        {
            while (!CheckPlacementLength(AvailableCurves.First(), placementLength))
            { }
            return AvailableCurves.RemoveFromFront();
        }

        protected bool CheckPlacementLength(T curve, double placementLength)
        {
            double curveLength = GetLength(curve);
            if (curveLength < placementLength)
            {
                if (_saveElementsOrder)
                {
                    AvailableCurves.RemoveFromFront();
                }
                else
                {
                    AvailableCurves.AddToBack(AvailableCurves.RemoveFromFront());
                }
                return false;
            }

            return true;
        }

        protected bool CheckMinPlacementLength(T curve)
        {
            double curveLength = GetLength(curve);
            if (curveLength > _minPlacementLength)
            {
                return true;
            }

            return false;
        }

        protected abstract double GetLength(T curve);
    }
}
