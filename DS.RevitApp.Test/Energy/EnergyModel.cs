using System.Collections.Generic;

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
    }
}