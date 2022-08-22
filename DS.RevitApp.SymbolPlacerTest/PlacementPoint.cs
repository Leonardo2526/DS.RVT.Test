using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;

namespace DS.RevitLib.SymbolPlacerTest
{
    public class PlacementPoint
    {
        private readonly MEPCurve _mEPCurve;
        private readonly double _familyLength;
        private readonly double _minElemDist;
        private readonly Connector _con1;
        private readonly Connector _con2;

        public PlacementPoint(MEPCurve mEPCurve, double familyLength, double minElemDist)
        {
            _mEPCurve = mEPCurve;
            _familyLength = familyLength;
            _minElemDist = minElemDist;
            (Connector con1, Connector con2) = ConnectorUtils.GetMainConnectors(mEPCurve);
            _con1 = con1;
            _con2 = con2;
        }

        public XYZ GetPoint(Connector baseConnector)
        {
            XYZ placementDirection = GetPlacementDirection(baseConnector);
            return baseConnector.Origin + placementDirection.Multiply(_minElemDist + _familyLength / 2);
        }

        public XYZ GetPoint(PlacementOption placementOption)
        {
            switch (placementOption)
            {
                case PlacementOption.Center:                  
                    return ElementUtils.GetLocationPoint(_mEPCurve);
                case PlacementOption.Edge:
                    return GetPoint(_con1);
                default:
                    break;
            }

            return null;
        }

        private XYZ GetPlacementDirection(Connector baseCon)
        {
            XYZ direction;

            direction = (_con1.Origin - baseCon.Origin).Normalize();
            if (direction.IsZeroLength())
            {
                return (_con2.Origin - baseCon.Origin).Normalize();
            }

            return direction;
        }

       
    }
}
