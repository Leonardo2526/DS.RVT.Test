using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Document = Autodesk.Revit.DB.Document;

namespace DS.RevitApp.ElementsTransferTest
{
    internal class ElementsTransfer
    {
        private readonly List<FamilyInstance> _familyInstances;
        private readonly MEPCurve _targerMEPCurve;
        private readonly Document _doc;

        public ElementsTransfer(List<FamilyInstance> familyInstances, MEPCurve targerMEPCurve)
        {
            _familyInstances = familyInstances;
            _targerMEPCurve = targerMEPCurve;
            _doc = targerMEPCurve.Document;
        }


        public void Transfer()
        {
            XYZ dir = MEPCurveUtils.GetDirection(_targerMEPCurve);
            XYZ point = ElementUtils.GetLocationPoint(_targerMEPCurve);
                var creator = new MEPCurveCreator(_targerMEPCurve);

            foreach (var family in _familyInstances)
            {

                FamilySymbol familySymbol = family.GetFamilySymbol();
                FamilyInstance newFamInst = CreateFamilyInstane(familySymbol, point, _targerMEPCurve) as FamilyInstance;


                //Connect
                MEPCurve splittedMEPCurve = creator.SplitElement(point) as MEPCurve;


                var freeCon = ConnectorUtils.GetFreeConnector(_targerMEPCurve);
                List<Connector> splittedCons = ConnectorUtils.GetConnectors(splittedMEPCurve);
                List<Connector> targetCons = ConnectorUtils.GetConnectors(_targerMEPCurve);
                var connectedCons = splittedCons.Where(obj => obj.IsConnectedTo(targetCons.First()) || obj.IsConnectedTo(targetCons.Last()));

                Connector targetCon = targetCons.Where(obj => (obj.Origin - point).IsZeroLength()).First();
                Connector splittedCon = splittedCons.Where(obj => (obj.Origin - point).IsZeroLength()).First();
             




                List<Connector> famInstCons = ConnectorUtils.GetConnectors(newFamInst);

                SetParameters(newFamInst, targetCon.Radius * 2);

                SelectConnector(famInstCons.First(), famInstCons.Last(), _targerMEPCurve, targetCon, splittedMEPCurve, splittedCon);

                ConnectConnectors(connectors1.First(), connectors1.Last());
                ConnectConnectors(connectors2.First(), connectors2.Last());


                //Connector famInstCon1 = famInstCons.First();


                //ConnectConnectors(famInstCon1, targetCon, splittedCon);
                //foreach (var famInstCon in famInstCons)
                //{
                //    ConnectConnectors(famInstCon, targetCon, splittedCon);
                //}
                creator.Move(new XYZ(0, 0.001, 0));
                creator.Move(new XYZ(0, -0.001, 0));
                point += dir.Multiply(2);
            }
        }

        public ICollection<ElementId> CreateFamilyInstane2(FamilySymbol familySymbol, XYZ point, MEPCurve targerMEPCurve)
        {
            ICollection<ElementId> elements = null;
            using (Transaction transNew = new Transaction(_doc, "CreateFamilyInstance"))
            {
                try
                {
                    transNew.Start();

                    var data = new FamilyInstanceCreationData(point, familySymbol, targerMEPCurve,
                        targerMEPCurve.ReferenceLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    //var lc = targerMEPCurve.Location as LocationCurve;
                    //var data = new FamilyInstanceCreationData(lc.Curve, familySymbol,
                    //   targerMEPCurve.ReferenceLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    var dates = new List<FamilyInstanceCreationData>() { data};

                    elements = _doc.Create.NewFamilyInstances2(dates);
                }

                catch (Exception e)
                {
                }
                transNew.Commit();
                return elements;
            }

        }

        public Element CreateFamilyInstane(FamilySymbol familySymbol, XYZ point, MEPCurve targerMEPCurve)
        {
            Element element = null;

            Level level = _doc.GetElement(targerMEPCurve.LevelId) as Level;

            using (Transaction transNew = new Transaction(_doc, "CreateFamilyInstance"))
            {
                try
                {
                    transNew.Start();

                    element = _doc.Create.NewFamilyInstance(point, familySymbol, targerMEPCurve,
                        level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                }

                catch (Exception e)
                {
                }
                transNew.Commit();
                return element;
            }

        }

        private void ConnectConnectors(Connector c1, Connector c2)
        {
            using (Transaction transNew = new Transaction(_doc, "autoMEP_ConnectConnectors"))
            {
                try
                {
                    transNew.Start();
                    c1.ConnectTo(c2);
                }

                catch (Exception e)
                {
                    //c1.ConnectTo(c2opt);
                }
                if (transNew.HasStarted())
                {
                  transNew.Commit();               
                }
            }
        }

        private List<Connector> connectors1;
        private List<Connector> connectors2;

        private Connector SelectConnector(Connector familyInstanceCon1, Connector familyInstanceCon2, MEPCurve mEPCurve1, Connector mEPCurveCon1,
            MEPCurve mEPCurve2, Connector mEPCurveCon2)
        {
            connectors1 = new List<Connector>();
            connectors2 = new List<Connector>();

            XYZ lp1 = ElementUtils.GetLocationPoint(mEPCurve1);
            XYZ lp2 = ElementUtils.GetLocationPoint(mEPCurve2);

            if (lp1.DistanceTo(familyInstanceCon1.Origin) > lp1.DistanceTo(familyInstanceCon2.Origin))
            {
                connectors1.Add(mEPCurveCon1);
                connectors1.Add(familyInstanceCon2);
                connectors2.Add(mEPCurveCon2);
                connectors2.Add(familyInstanceCon1);
            }
            else
            {
                connectors1.Add(mEPCurveCon1);
                connectors1.Add(familyInstanceCon1);
                connectors2.Add(mEPCurveCon2);
                connectors2.Add(familyInstanceCon2);
            }


            return null;
        }


        private void SetParameters(FamilyInstance famInst, double diam)
        {
            Parameter a = MEPElementUtils.GetAssociatedParameter(famInst, BuiltInParameter.CONNECTOR_DIAMETER);
            var parameterSetter = new ParameterSetter(famInst, new RollBackCommitter());
            parameterSetter.SetValue(a, diam);
        }
    }
}
