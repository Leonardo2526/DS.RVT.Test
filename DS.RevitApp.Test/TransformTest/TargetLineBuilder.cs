using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static DS.ClassLib.VarUtils.DirPathBuilder;

namespace DS.RevitApp.Test.TransformTest
{
    internal class TargetLineBuilder
    {
        private readonly LineModel _lineModel;
        private readonly double _placementLength;
        private readonly List<XYZ> _points;
        private readonly XYZ _p1;
        private readonly XYZ _p2;

        public TargetLineBuilder(LineModel lineModel, double placementLength, List<XYZ> points)
        {
            _lineModel = lineModel;
            _placementLength = placementLength;
            _points = points;
            _p1 = _lineModel.Line.GetEndPoint(0);
            _p2 = _lineModel.Line.GetEndPoint(1);
        }

        public TargetLineModel Build()
        {
            XYZ basePoint = GetBasePoint();
            XYZ startPlacementPoint = basePoint is null
                ? new PlacementPoint(_lineModel.Line, _placementLength).GetPoint(PlacementOption.Edge)
                : new PlacementPoint(_lineModel.Line, _placementLength).GetPoint(basePoint);

            XYZ endPoint = startPlacementPoint.DistanceTo(_p1) > startPlacementPoint.DistanceTo(_p2) ?
                _p1 : _p2;
            XYZ startPoint = Math.Round(endPoint.DistanceTo(_p1), 3) == 0 ?
                _p2 : _p1;

            XYZ vector = (endPoint - startPlacementPoint).RoundVector().Normalize();
            XYZ endPlacementPoint = endPoint - vector.Multiply(_placementLength / 2);

            return new TargetLineModel(_lineModel, startPlacementPoint, endPlacementPoint, startPoint, endPoint);
        }

        private XYZ GetBasePoint()
        {
            for (int i = 0; i < _points.Count - 1; i++)
            {
                //check if points conicidence
                XYZ coincidence = GetCoincidence(_p1, _p2, _points[i], _points[i + 1]);
                if (coincidence is not null)
                {
                    return coincidence;
                }

                if (_p2.IsBetweenPoints(_points[i], _points[i + 1]))
                {
                    if (_points[i].DistanceTo(_p1) > _points[i].DistanceTo(_p2))
                    {
                        return _p2;
                    }
                    else
                    { return _p1; }
                }
            }

            return null;
        }

        private XYZ GetCoincidence(XYZ p0, XYZ p1, XYZ closest, XYZ farest)
        {
            var v11 = closest - p0;
            var v12 = closest - p1;
            var v21 = farest - p0;
            var v22 = farest - p1;

            if (v11.IsZeroLength() | v22.IsZeroLength())
            {
                return p0;
            }
            if (v12.IsZeroLength() | v21.IsZeroLength())
            {
                return p1;
            }

            return null;
        }

    }
}
