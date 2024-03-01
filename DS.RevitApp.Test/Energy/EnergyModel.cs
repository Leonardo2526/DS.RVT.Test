using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using System.Collections.Generic;
using MoreLinq;

namespace DS.RevitApp.Test.Energy
{
    public class EnergyModel
    {
        public EnergyModel(EnergySpace space, IEnumerable<EnergySurface> energySurfaces)
        {
            EnergySpace = space;
            EnergySurfaces = energySurfaces;
        }


        public EnergySpace EnergySpace { get; }

        public IEnumerable<EnergySurface> EnergySurfaces { get; set; }

        public void Show(Document activeDoc)
        {
            EnergySpace.Show(activeDoc);
            EnergySurfaces.ForEach(e => { e.Show(activeDoc); });
        }
    }
}