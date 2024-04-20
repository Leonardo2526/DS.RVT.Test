using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using System.Collections.Generic;
using MoreLinq;
using System.Linq;

namespace DS.RevitCmd.EnergyTest
{
    public class EnergyModel
    {
        public EnergyModel(EnergySpace space, IEnumerable<EnergySurface> energySurfaces)
        {
            EnergySpace = space;
            EnergySurfaces = energySurfaces.ToList();
        }


        public EnergySpace EnergySpace { get; }

        public List<EnergySurface> EnergySurfaces { get; set; }

        public void Show(Document activeDoc)
        {
            EnergySpace.Show(activeDoc);
            EnergySurfaces.ForEach(e => { e.Show(activeDoc); });
        }
    }
}