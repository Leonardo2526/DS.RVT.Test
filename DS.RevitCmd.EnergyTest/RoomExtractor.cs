using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using DS.GraphUtils.Entities;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
namespace DS.RevitCmd.EnergyTest
{
    internal class RoomExtractor
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly List<RevitLinkInstance> _allLoadedLinks;
        private readonly List<Document> _allFilteredDocs;
        private readonly DocumentFilter _globalFilter;

        public RoomExtractor(UIDocument uiDoc, DocumentFilter globalFilter)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _allLoadedLinks = _doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
            _allFilteredDocs = new List<Document>() { _doc };
            _allFilteredDocs.AddRange(_allLoadedLinks.Select(l => l.GetLinkDocument()));
            _globalFilter = globalFilter;
        }

        public IEnumerable<Room> GetRooms(IEnumerable<string> numbers = null)
        {
            var roomDocFilter = _globalFilter.Clone();
            roomDocFilter.SlowFilters ??= new();
            roomDocFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = roomDocFilter.ApplyToAllDocs()
                .SelectMany(kv => kv.Value.ToElements(kv.Key))
                .OfType<Room>()
                .Where(r => r.Area > 0);

            rooms = numbers != null ?
                rooms.Where(r => numbers.Contains(r.Number)) :
                rooms;
            return rooms;
        }
    }
}
