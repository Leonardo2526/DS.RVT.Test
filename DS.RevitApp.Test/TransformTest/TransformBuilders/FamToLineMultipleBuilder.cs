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
    internal class FamToLineMultipleBuilder : AbstractMultipleTransformBuilder<FamilyInstance, LineModel>
    {
        private readonly double _minFamInstLength = 50.mmToFyt2();
        private readonly double _minCurveLength;
        private readonly List<ICollisionChecker> _collisionCheckers;
        private readonly double _minPlacementLength;
        private AvailableLineService _lineService;
        private double _currentPlacementLength;
        private SolidModelExt _sourceSolidModel;
        private readonly List<XYZ> _points;
        private readonly MEPCurveModel _mEPCurveModel;

        public FamToLineMultipleBuilder(List<FamilyInstance> sourceObjects, List<LineModel> targetObjects, List<XYZ> points, double minCurveLength,
            List<ICollisionChecker> collisionCheckers, MEPCurveModel mEPCurveModel) :
            base(sourceObjects, targetObjects)
        {
            _points = points;
            _minCurveLength = minCurveLength;
            _collisionCheckers = collisionCheckers;
            _minPlacementLength = _minFamInstLength + 2 * minCurveLength;
            _mEPCurveModel = mEPCurveModel;
        }

        public override AbstractTransformModel<FamilyInstance, LineModel> Build(FamilyInstance sourceObject, LineModel targetObject)
        { 
            var builder = new FamToLineTransformBuilder(sourceObject, targetObject, 
                _collisionCheckers, _currentPlacementLength, _sourceSolidModel, _points, _mEPCurveModel, _minCurveLength);
            var model = builder.Build();

            var (line1, line2) = 
                targetObject.Line.Cut(_sourceSolidModel.ConnectorsPoints.First(), _sourceSolidModel.ConnectorsPoints.Last(), out Line cuttedLine);


            Line maxLine = line1.ApproximateLength > line2.ApproximateLength ? line1 : line2;

            var lineModel = new LineModel(maxLine, targetObject.Basis);

            //add splitted mEPCurve to stack
            if (_lineService.CheckMinPlacementLength(lineModel))
            {
                _lineService.AvailableCurves.AddToFront(lineModel);            
            }

            return model;
        }

        public override List<AbstractTransformModel<FamilyInstance, LineModel>> Build()
        {
            _lineService = new AvailableLineService(_targetObjects, _minCurveLength, _minPlacementLength);
            if (_lineService.AvailableCurves is null)
            {
                string errors = $"No available MEPCurves exist for family insatances placement.";
                //LogMessageCreator.CreateMessage(errors, TraceEventType.Error, SubType.General, _collision);
                return null;
            }

            var transforms = new List<AbstractTransformModel<FamilyInstance, LineModel>>();   
            foreach (var fam in _sourceObjects)
            {
                _sourceSolidModel = new SolidModelExt(fam);
                _currentPlacementLength = GetFamInsLength(fam) + 2 * _minCurveLength;
                LineModel lineModel = _lineService.Get(_currentPlacementLength);
                if (lineModel is null)
                {
                    string errors = $"No available MEPCurves exist for family insatance id ({fam.Id}) placement.";
                    //LogMessageCreator.CreateMessage(errors, TraceEventType.Error, SubType.General, _collision);
                    return null;
                }

                var model = Build(fam, lineModel);
                transforms.Add(model);
            }

            return transforms;
        }


        private double GetFamInsLength(FamilyInstance fam)
        {
            (Connector con1, Connector con2) = ConnectorUtils.GetMainConnectors(fam);
            return con1.Origin.DistanceTo(con2.Origin);
        }
    }
}
