using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using DS.ClassLib.VarUtils;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.Energy
{
    internal class EnergyModelBuilder : ISerilogged
    {
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly ITransactionFactory _trf;
        private readonly ISpaceFactory _spaceFactory;


        public EnergyModelBuilder(
            Document doc,
            IEnumerable<RevitLinkInstance> links,
            ITransactionFactory transactionFactory,
            ISpaceFactory spaceFactory)
        {
            _doc = doc;
            _links = links;
            _trf = transactionFactory;
            _spaceFactory = spaceFactory;
        }

        public ILogger Logger { get; set; }


        public IEnumerable<EnergyModel> Create(IEnumerable<Room> rooms)
        {
            var eModels = new List<EnergyModel>();

            var spaces = _trf.CreateAsync(() => CreateSpaces(rooms), "CreateSpaces").Result;
            var eSpaces = spaces.Select(s => new EnergySpace(s));

            var options = new SpatialElementBoundaryOptions();
            foreach (var eSpace in eSpaces)
            {
                var energySurfaces = new List<EnergySurface>();
                var wallSurfaces = GetWallEnergySurfaces(eSpace);
                var floorSurfaces = GetFloorEnergySurfaces(eSpace);
                var ceilingSurfaces = GetCeilingEnergySurfaces(eSpace);
                energySurfaces.AddRange(wallSurfaces);
                energySurfaces.AddRange(floorSurfaces);
                energySurfaces.AddRange(ceilingSurfaces);

                var eModel = new EnergyModel(eSpace, energySurfaces);
                eModels.Add(eModel);
            }

            return eModels;
        }


        private IEnumerable<Space> CreateSpaces(IEnumerable<Room> rooms)
        {
            var spaces = new List<Space>();
            foreach (var room in rooms)
            {
                var space = _spaceFactory.Create(room);
                spaces.Add(space);
            }
            return spaces;
        }

        private IEnumerable<EnergySurface> GetWallEnergySurfaces(EnergySpace energySpace)
        {
            var energySurfaces = new List<EnergySurface>();

            return energySurfaces;
        }

        private IEnumerable<EnergySurface> GetFloorEnergySurfaces(EnergySpace energySpace)
        {
            var energySurfaces = new List<EnergySurface>();

            return energySurfaces;
        }

        private IEnumerable<EnergySurface> GetCeilingEnergySurfaces(EnergySpace energySpace)
        {
            var energySurfaces = new List<EnergySurface>();

            return energySurfaces;
        }
    }
}
