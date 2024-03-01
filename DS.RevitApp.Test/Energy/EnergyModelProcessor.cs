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
    internal class EnergyModelProcessor : ISerilogged
    {
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly ITransactionFactory _trf;
        private readonly ISpaceFactory _spaceFactory;
        private readonly IEnergyModelFactory _energyModelFactory;

        public EnergyModelProcessor(
            Document doc,
            IEnumerable<RevitLinkInstance> links,
            ITransactionFactory transactionFactory,
            ISpaceFactory spaceFactory, 
            IEnergyModelFactory energyModelFactory)
        {
            _doc = doc;
            _links = links;
            _trf = transactionFactory;
            _spaceFactory = spaceFactory;
            _energyModelFactory = energyModelFactory;
        }

        public ILogger Logger { get; set; }


        public IEnumerable<EnergyModel> Create(IEnumerable<Room> rooms)
        {
            var eModels = new List<EnergyModel>();

            var spaces = _trf.Create(() => CreateSpaces(rooms), "CreateSpaces");
            foreach (var space in spaces)
            {
                var eModel = _energyModelFactory.Create(space);
                eModels.Add(eModel);
            }
            Logger?.Information($"Energy models created: {eModels.Count}.");
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
    }
}
