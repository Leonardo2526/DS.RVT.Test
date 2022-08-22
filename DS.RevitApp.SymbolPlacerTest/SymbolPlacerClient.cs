using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Symbols;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.TransactionCommitter;
using System.Security.Cryptography;
using System.Windows.Forms;
using DS.RevitLib.SymbolPlacerTest;

namespace DS.RevitApp.SymbolPlacerTest
{
    public class SymbolPlacerClient
    {
        private readonly List<FamilyInstance> _familyInstances;
        private readonly List<MEPCurve> _targerMEPCurves;
        private readonly Document _doc;
        private readonly Connector _baseConnector;
        private readonly double _minFamInstLength = 50.mmToFyt2();
        private readonly double _minMEPCurveLength = 50.mmToFyt2();
        private double _minPlacementLength;


        public SymbolPlacerClient(List<FamilyInstance> familyInstances, List<MEPCurve> targerMEPCurves, Connector baseConnector = null)
        {
            _familyInstances = familyInstances;
            _targerMEPCurves = targerMEPCurves;
            _doc = targerMEPCurves.First().Document;
            _baseConnector = baseConnector;
            _minPlacementLength = _minFamInstLength + 2*_minMEPCurveLength;
        }

        public void Run()
        {
            var availableMEPCurves = new AvailableMEPCurves(_targerMEPCurves, _minMEPCurveLength, _minPlacementLength);
            if (availableMEPCurves.CurrentStack is null)
            {
                MessageBox.Show($"No available MEPCurves exist for family insatances placement.");
                return;
            }

            foreach (var family in _familyInstances)
            {
                FamilySymbol familySymbol = family.GetFamilySymbol();
                double familyLength = new FamilySymbolUtils().GetLength(familySymbol, _doc, family);
                double placementLength = familyLength + 2 * _minMEPCurveLength;

                MEPCurve mEPCurve = availableMEPCurves.Get(placementLength);
                if (mEPCurve is null)
                {
                    MessageBox.Show($"No available MEPCurves exist for family insatance id ({family.Id}) placement.");
                    break;
                }

                XYZ point = _baseConnector is null
                    ? new PlacementPoint(mEPCurve, placementLength).GetPoint(PlacementOption.Edge)
                    : new PlacementPoint(mEPCurve, placementLength).GetPoint(_baseConnector);

                var symbolPlacer = new SymbolPlacer(familySymbol, mEPCurve, point, familyLength, family,
                    new RollBackCommitter(), "autoMEP");
                symbolPlacer.Place();

                //add splitted mEPCurve to stack
                if (availableMEPCurves.CheckMinLength(symbolPlacer.SplittedMEPCurve))
                {
                    availableMEPCurves.CurrentStack.Push(symbolPlacer.SplittedMEPCurve);
                }


                //mEPCurve = symbolPlacer.SplittedMEPCurve;
                //Connector baseConnector = symbolPlacer.BaseConnector;

                //point = new PlacementPoint(mEPCurve, familyLength, _minElemDist).GetPoint(baseConnector);

                //if (point is null)
                //{
                //    i++;
                //    mEPCurve = _targerMEPCurves[i];
                //}
            }
        }


    }
}
