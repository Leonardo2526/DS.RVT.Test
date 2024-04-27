using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using MoreLinq;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Extensions.RhinoExtensions;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using OLMP.RevitAPI.Develop;

namespace DS.RevitCmd.EnergyTest
{
    internal abstract class InsertsSolidModelBase<TElement> where TElement : Element
    {
        protected readonly Document _activeDoc;
        protected readonly IEnumerable<RevitLinkInstance> _links;
        private Solid _solid;
        private IEnumerable<Element> _inserts;

        public InsertsSolidModelBase(TElement element, Document activeDoc, IEnumerable<RevitLinkInstance> links = null)
        {
            Element = element;
            _activeDoc = activeDoc;
            _links = links;
        }

        public TElement Element { get; }

        public Solid Solid => _solid ??= Element.Solid(_links);

        public IEnumerable<Element> Inserts => _inserts ??= GetInserts();

        public Solid GetFullSolid()
        {
            var solidsToAdd = GetAllInsertsSolidModels().Select(m => m.solid);

            var fullSolid = SolidUtils.Clone(Solid);
            solidsToAdd.ForEach(solidToAdd => fullSolid = BooleanOperationsUtils
            .ExecuteBooleanOperation(fullSolid, solidToAdd,
                        BooleanOperationsType.Union));

            return fullSolid;
        }

        public IEnumerable<(Element insert, Solid solid)> GetAllInsertsSolidModels()
        {
            var models = new List<(Element insert, Solid solid)>();

            var openingsModels = GetOpeningsSolidModels();
            openingsModels.ForEach(m => models.Add(m));
            var insertsModels = GetWindowsAndDoorsSolidModels();
            insertsModels.ForEach(m => models.Add(m));

            return models;
        }

        public IEnumerable<(Opening opening, Solid solid)> GetOpeningsSolidModels()
        {
            var models = new List<(Opening opening, Solid solid)>();
            var openings = Inserts.OfType<Opening>();
            foreach (var op in openings)
            {
                var solid = op.TryGetBestSolid(_activeDoc, _links);
                if (solid != null)
                { models.Add((op, solid)); }
            }
            return models;
        }

        public IEnumerable<(Element insert, Solid solid)> GetWindowsAndDoorsSolidModels()
        {
            var models = new List<(Element insert, Solid solid)>();
            var windowsAndDoors = Inserts.Where(e => e is not Opening);
            foreach (var insert in windowsAndDoors)
            {
                var iSolid = insert.Solid(_links);
                if (iSolid != null)
                {
                    var boxSolid = GetInsertSolid(iSolid);
                    models.Add((insert, boxSolid));
                }
            }
            return models;

        }

        protected abstract Solid GetInsertSolid(Solid solid);

        protected abstract IEnumerable<Element> GetInserts();
    }
}
