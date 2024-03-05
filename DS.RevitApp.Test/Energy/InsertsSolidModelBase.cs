using Autodesk.Revit.DB;
using OLMP.RevitAPI.Tools.Extensions;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DS.RevitApp.Test.Energy
{
    internal abstract class InsertsSolidModelBase<TElement> where TElement : Element
    {
        private readonly IEnumerable<RevitLinkInstance> _links;
        private Solid _solid;

        public InsertsSolidModelBase(TElement element, IEnumerable<RevitLinkInstance> links = null)
        {
            Element = element;
            _links = links;
        }

        public TElement Element { get; }

        public Solid Solid => _solid ??= Element.Solid(_links);

        public abstract IEnumerable<Opening> Openings { get; }
        public abstract IEnumerable<Element> Inserts { get; }

        public Solid GetFullSolid()
        {
            var addedSolids = new List<Solid>();

            var openingsModels = GetOpeningsSolids();
            addedSolids.AddRange(openingsModels.Select(m => m.solid));
            var insertsModels = GetInsertsSolids();
            addedSolids.AddRange(insertsModels.Select(m => m.solid));

            var fullSolid = SolidUtils.Clone(Solid);
            addedSolids.ForEach(solidToAdd => fullSolid = BooleanOperationsUtils
            .ExecuteBooleanOperation(fullSolid, solidToAdd,
                        BooleanOperationsType.Union));

            return fullSolid;
        }

        public IEnumerable<(Opening opening, Solid solid)> GetOpeningsSolids()
        {
            var models = new List<(Opening opening, Solid solid)>();
            foreach (var op in Openings)
            {
                var solid = op.Solid(_links);
                models.Add((op, solid));
            }
            return models;
        }

        public IEnumerable<(Element insert, Solid solid)> GetInsertsSolids()
        {
            var models = new List<(Element insert, Solid solid)>();
            foreach (var insert in Inserts)
            {
                var solid = insert.Solid(_links);
                models.Add((insert, solid));
            }
            return models;
        }
    }
}
