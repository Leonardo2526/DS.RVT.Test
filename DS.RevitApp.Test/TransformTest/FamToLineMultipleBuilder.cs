using Autodesk.Revit.DB;
using DS.RevitApp.SymbolPlacerTest;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Transforms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    internal class FamToLineMultipleBuilder : AbstractMultipleTransformBuilder<FamilyInstance, Line>
    {
        private readonly double _minFamInstLength = 50.mmToFyt2();
        private readonly double _minCurveLength;
        private readonly double _minPlacementLength;

        public FamToLineMultipleBuilder(List<FamilyInstance> sourceObjects, List<Line> targetObjects, double minCurveLength) : 
            base(sourceObjects, targetObjects)
        {
            _minCurveLength = minCurveLength;
            _minPlacementLength = _minFamInstLength + 2 * minCurveLength;
        }

        public override AbstractTransformModel<FamilyInstance, Line> Build(FamilyInstance sourceObject, Line targetObject)
        {
            return null;
        }

        public override AbstractTransformModel<FamilyInstance, Line> Build(List<FamilyInstance> sourceObjects, List<FamilyInstance> targetObjects)
        {
            var linesModel = new AvailableLineModel(_targetObjects, _minCurveLength, _minPlacementLength);
            if (linesModel.AvailableCurves is null)
            {
                string errors = $"No available MEPCurves exist for family insatances placement.";
                //LogMessageCreator.CreateMessage(errors, TraceEventType.Error, SubType.General, _collision);
                return null;
            }

            foreach (var fam in _sourceObjects)
            {
                var sorceModel = new SolidModelExt(fam);
                SolidModelExt operationModel = sorceModel.Clone();

                double placementLength = operationModel.Length + 2 * _minCurveLength;
                Line line = linesModel.Get(placementLength);
                if (line is null)
                {
                    string errors = $"No available MEPCurves exist for family insatance id ({fam.Id}) placement.";
                    //LogMessageCreator.CreateMessage(errors, TraceEventType.Error, SubType.General, _collision);
                    break;
                }


            }
        }
    }
}
