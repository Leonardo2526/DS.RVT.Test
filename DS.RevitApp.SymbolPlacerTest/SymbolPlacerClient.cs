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
        private readonly double _minElemDist = 50.mmToFyt2();


        public SymbolPlacerClient(List<FamilyInstance> familyInstances, List<MEPCurve> targerMEPCurves, Connector baseConnector = null)
        {
            _familyInstances = familyInstances;
            _targerMEPCurves = targerMEPCurves;
            _doc = targerMEPCurves.First().Document;
            _baseConnector = baseConnector;
        }

        public void Run()
        {
            int i = 0;
            XYZ point = null;
            MEPCurve mEPCurve = null;

            foreach (var family in _familyInstances)
            {
                if (i > _targerMEPCurves.Count)
                {
                    MessageBox.Show("No available MEPCurves exist for family insatance placement.");
                    break;
                }

                FamilySymbol familySymbol = family.GetFamilySymbol();
                double familyLength = new FamilySymbolUtils().GetLength(familySymbol, _doc, family);

                mEPCurve ??= GetAvailableMEPCurve(familyLength);
                if (mEPCurve is null)
                {
                    MessageBox.Show("No available MEPCurves exist for family insatance placement.");
                    break;
                }

                point ??= new PlacementPoint(mEPCurve, familyLength, _minElemDist).GetPoint(PlacementOption.Edge);

                //if (point is null)
                //{
                //    return null;
                //}


                var symbolPlacer = new SymbolPlacer(familySymbol, mEPCurve, point, familyLength, family,
                    new RollBackCommitter(), "autoMEP");
                symbolPlacer.Place();


                mEPCurve = symbolPlacer.SplittedMEPCurve;
                Connector baseConnector = symbolPlacer.BaseConnector;

                point = new PlacementPoint(mEPCurve, familyLength, _minElemDist).GetPoint(baseConnector);

                if (point is null)
                {
                    i++;
                    mEPCurve = _targerMEPCurves[i];
                }
            }
        }


        private MEPCurve GetAvailableMEPCurve(double familyLength)
        {
            foreach (var targerMEPCurve in _targerMEPCurves)
            {
                if (IsPlacementAvailable(targerMEPCurve, familyLength))
                {
                    return targerMEPCurve;
                }
            }

            return null;
        }

        private bool IsPlacementAvailable(MEPCurve mEPCurve, double familyLength)
        {
            double targetLength = mEPCurve.GetCenterLine().ApproximateLength;

            if (targetLength < familyLength + 2 * _minElemDist)
            {
                return false;
            }

            return true;
        }
    }
}
