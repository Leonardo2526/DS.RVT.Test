using Autodesk.Revit.DB;
using OLMP.RevitAPI.Tools.MEP.Symbols;
using OLMP.RevitAPI.Tools.MEP;
using OLMP.RevitAPI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.TransactionCommitter;
using System.Security.Cryptography;
using System.Windows.Forms;
using DS.RevitLib.SymbolPlacerTest;
using OLMP.RevitAPI.Tools.Solids.Models;

namespace DS.RevitApp.SymbolPlacerTest
{
    public class SymbolPlacerClient
    {
        private readonly List<FamilyInstance> _familyInstances;
        private readonly List<MEPCurve> _targerMEPCurves;
        private readonly Document _doc;
        private readonly double _minFamInstLength = 50.mmToFyt2();
        private readonly double _minMEPCurveLength = 50.mmToFyt2();
        private double _minPlacementLength;
        private readonly List<XYZ> _points;


        public SymbolPlacerClient(List<FamilyInstance> familyInstances, List<MEPCurve> targerMEPCurves, List<XYZ> points)
        {
            _familyInstances = familyInstances;
            _targerMEPCurves = targerMEPCurves;
            _doc = targerMEPCurves.First().Document;
            _minPlacementLength = _minFamInstLength + 2*_minMEPCurveLength;
            _points = points;
        }

        public void Run()
        {
            var availableMEPCurves = new AvailableMEPCurvesService(_targerMEPCurves, _minMEPCurveLength, _minPlacementLength);
            if (availableMEPCurves.AvailableMEPCurves is null)
            {
                MessageBox.Show($"No available MEPCurves exist for family insatances placement.");
                return;
            }

            foreach (var family in _familyInstances)
            {
                var solidModelExt = new SolidModelExt(family);
                double placementLength = solidModelExt.Length + 2 * _minMEPCurveLength;

                MEPCurve mEPCurve = availableMEPCurves.Get(placementLength);
                if (mEPCurve is null)
                {
                    MessageBox.Show($"No available MEPCurves exist for family insatance id ({family.Id}) placement.");
                    break;
                }
                Connector baseConnector = GetBaseConnector(_points, mEPCurve);
                XYZ point = baseConnector is null
                    ? new PlacementPoint(mEPCurve, placementLength).GetPoint(PlacementOption.Edge)
                    : new PlacementPoint(mEPCurve, placementLength).GetPoint(baseConnector);


            }
        }

        //public void Run()
        //{
        //    var availableMEPCurves = new AvailableMEPCurvesService(_targerMEPCurves, _minMEPCurveLength, _minPlacementLength);
        //    if (availableMEPCurves.AvailableMEPCurves is null)
        //    {
        //        MessageBox.Show($"No available MEPCurves exist for family insatances placement.");
        //        return;
        //    }

        //    foreach (var family in _familyInstances)
        //    {
        //        FamilySymbol familySymbol = family.GetFamilySymbol();
        //        double familyLength = new FamilySymbolUtils().GetLength(familySymbol, _doc, family);
        //        double placementLength = familyLength + 2 * _minMEPCurveLength;

        //        MEPCurve mEPCurve = availableMEPCurves.Get(placementLength);
        //        if (mEPCurve is null)
        //        {
        //            MessageBox.Show($"No available MEPCurves exist for family insatance id ({family.Id}) placement.");
        //            break;
        //        }
        //        Connector baseConnector = GetBaseConnector(_points, mEPCurve);
        //        XYZ point = baseConnector is null
        //            ? new PlacementPoint(mEPCurve, placementLength).GetPoint(PlacementOption.Edge)
        //            : new PlacementPoint(mEPCurve, placementLength).GetPoint(baseConnector);

        //        var symbolPlacer = new SymbolPlacer(familySymbol, mEPCurve, point, familyLength, family,
        //            new RollBackCommitter(), "autoMEP");
        //        symbolPlacer.Place();

        //        //add splitted mEPCurve to stack
        //        if (availableMEPCurves.CheckMinLength(symbolPlacer.SplittedMEPCurve))
        //        {
        //            availableMEPCurves.AvailableMEPCurves.AddToFront(symbolPlacer.SplittedMEPCurve);
        //        }
        //    }
        //}

        private Connector GetBaseConnector(List<XYZ> points, MEPCurve mEPCurve)
        {
           var (con1, con2) = ConnectorUtils.GetMainConnectors(mEPCurve);

            for (int i = 0; i < points.Count - 1; i++)
            {
                //check if points conicidence
                Connector coincidenceCon = GetCoincidence(con1, con2, points[i], points[i + 1]);
                if (coincidenceCon is not null)
                {
                    return coincidenceCon;
                }

                if (con1.Origin.IsBetweenPoints(points[i], points[i + 1]))
                {
                    if (points[i].DistanceTo(con1.Origin) > points[i].DistanceTo(con2.Origin))
                    {
                        return con2;
                    }
                    else
                    { return con1; }
                }
            }

            return null;
        }

        private Connector GetCoincidence(Connector con1, Connector con2, XYZ closest, XYZ farest)
        {
            var v11 = closest - con1.Origin;
            var v12 = closest - con2.Origin;
            var v21 = farest - con1.Origin;
            var v22 = farest - con2.Origin;


            if (v11.IsZeroLength() | v22.IsZeroLength())
            {
                return con1;
            }
            if (v12.IsZeroLength() | v21.IsZeroLength())
            {
                return con2;
            }

            return null;
        }
    }
}
