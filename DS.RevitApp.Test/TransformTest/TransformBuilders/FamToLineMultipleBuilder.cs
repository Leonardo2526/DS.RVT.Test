using Autodesk.Revit.DB;
using DS.RevitApp.SymbolPlacerTest;
using DS.RevitApp.Test.PathFindTest;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Transforms;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    internal class FamToLineMultipleBuilder : AbstractMultipleTransformBuilder<SolidModelExt, LineModel>
    {
        private readonly double _minFamInstLength = 50.mmToFyt2();
        private readonly double _minCurveLength;
        private readonly List<ICollisionChecker> _collisionCheckers;
        private readonly double _minPlacementLength;
        private AvailableLineService _lineService;
        private double _currentPlacementLength;
        private readonly List<XYZ> _points;
        private readonly MEPCurveModel _mEPCurveModel;

        public FamToLineMultipleBuilder(List<SolidModelExt> sourceObjects, List<LineModel> targetObjects, List<XYZ> points, double minCurveLength,
            List<ICollisionChecker> collisionCheckers, MEPCurveModel mEPCurveModel) :
            base(sourceObjects, targetObjects)
        {
            _points = points;
            _minCurveLength = minCurveLength;
            _collisionCheckers = collisionCheckers;
            _minPlacementLength = _minFamInstLength + 2 * minCurveLength;
            _mEPCurveModel = mEPCurveModel;
        }

        public override AbstractTransformModel<SolidModelExt, LineModel> Build(SolidModelExt operationObject, LineModel targetObject)
        { 
            var builder = new FamToLineTransformBuilder(operationObject, targetObject, 
                _collisionCheckers, _currentPlacementLength, _points, _mEPCurveModel, _minCurveLength);
            var model = builder.Build();
         
            var (line1, line2) = 
                targetObject.Line.Cut(operationObject.ConnectorsPoints.First(), operationObject.ConnectorsPoints.Last(), out Line cuttedLine);


            Line maxLine = line1.ApproximateLength > line2.ApproximateLength ? line1 : line2;

            var lineModel = new LineModel(maxLine, targetObject.Basis);

            //add splitted mEPCurve to stack
            if (_lineService.CheckMinPlacementLength(lineModel))
            {
                _lineService.AvailableCurves.AddToFront(lineModel);            
            }

            return model;
        }

        public override List<AbstractTransformModel<SolidModelExt, LineModel>> Build()
        {
            _lineService = new AvailableLineService(_targetObjects, _minCurveLength, _minPlacementLength);
            if (_lineService.AvailableCurves is null)
            {
                string errors = $"No available MEPCurves exist for family insatances placement.";
                //LogMessageCreator.CreateMessage(errors, TraceEventType.Error, SubType.General, _collision);
                return null;
            }

            var transforms = new List<AbstractTransformModel<SolidModelExt, LineModel>>();   
            foreach (var sObj in _sourceObjects)
            {
                var operationObj = sObj.Clone();
                _currentPlacementLength = GetFamInsLength(sObj.Element) + 2 * _minCurveLength;
                LineModel lineModel = _lineService.Get(_currentPlacementLength);
                if (lineModel is null)
                {
                    string errors = $"No available MEPCurves exist for family insatance id ({sObj.Element.Id}) placement.";
                    //LogMessageCreator.CreateMessage(errors, TraceEventType.Error, SubType.General, _collision);
                    return null;
                }

                var model = Build(operationObj, lineModel);
                transforms.Add(model);
            }

            return transforms;
        }


        private double GetFamInsLength(Element fam)
        {
            (Connector con1, Connector con2) = ConnectorUtils.GetMainConnectors(fam);
            return con1.Origin.DistanceTo(con2.Origin);
        }
    }
}
