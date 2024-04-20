using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using OLMP.RevitAPI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using boundaryEdge = QuickGraph.TaggedEdge<OLMP.RevitAPI.Tools.Graphs.XYZVertex,
    DS.RevitCmd.EnergyTest.SpaceBoundary.BoundaryCurve>;
using OLMP.RevitAPI.Tools.Extensions;
using Autodesk.Revit.DB.Mechanical;
using DS.RevitCmd.EnergyTest;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Serilog;
using MoreLinq;
using DS.GraphUtils.Entities;
using QuickGraph;
using OLMP.RevitAPI.Tools.Graphs;
using OLMP.RevitAPI.Tools.Geometry.Points;
using QuickGraph.Algorithms;
using Serilog.Core;
using System.Diagnostics;

namespace DS.RevitCmd.EnergyTest.CompoundStructures
{
    internal class CompoundStructureTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly List<RevitLinkInstance> _allLoadedLinks;
        private readonly List<Document> _allFilteredDocs;
        private readonly DocumentFilter _globalFilter;
        private readonly ITransactionFactory _trf;

        public CompoundStructureTest(UIDocument uiDoc, DocumentFilter globalFilter)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _allLoadedLinks = _doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
            _allFilteredDocs = new List<Document>() { _doc };
            _allFilteredDocs.AddRange(_allLoadedLinks.Select(l => l.GetLinkDocument()));
            _globalFilter = globalFilter;
            _trf = new ContextTransactionFactory(_doc);
        }

        public ILogger Logger { get; set; }

        public void CreateStructures(Room room)
        {
            var rooms = new List<Room>() { room };
            var spaceFactory = new SpaceFactory(_doc);
            var space = _trf
                .Create(() => spaceFactory.Create(room), "CreateSpace");

            //var calculator = new SpatialElementGeometryCalculator(_doc, new SpatialElementBoundaryOptions());
            //var result = calculator.CalculateSpatialElementGeometry(space);
            //var spaceSolid = result.GetGeometry();

            var options = new SpatialElementBoundaryOptions()
            {
                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreCenter,
                //StoreFreeBoundaryFaces = true,
            };
            var boundarySegments = space.GetBoundarySegments(options).SelectMany(sl => sl);
            //var boundaryCurves = boundarySegments.Select(s => s.GetCurve());
            //boundaryCurves.ForEach(c => ShowCurve(c));
            //Debug.WriteLine($"boundarySegments count is: {boundarySegments.Count()}");
            //boundarySegments.ForEach(s => Debug.WriteLine(s.ElementId));

            var structures = CreateStructures(boundarySegments.First());
        }

        public IEnumerable<CompoundStructure> CreateStructures(BoundarySegment boundary)
        {
            var structures = new List<CompoundStructure>();

            var layers = new List<CompoundStructureLayer>();
            var wall = _doc.GetElement(boundary.ElementId) as Wall;
            double width = wall.Width;
            MaterialFunctionAssignment materialFunction = MaterialFunctionAssignment.Structure;
            var id = boundary.ElementId;
            var l1 = new CompoundStructureLayer(width, materialFunction, id);
            layers.Add(l1);
            var s1 = CompoundStructure.CreateSimpleCompoundStructure(layers);
            var s1Layers =  s1.GetLayers();       
            structures.Add(s1);
            return structures;
        }

    }
}
