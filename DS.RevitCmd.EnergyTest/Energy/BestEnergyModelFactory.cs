using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using MoreLinq;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Extensions;
using Autodesk.Revit.UI;
using System.Diagnostics;
using Autodesk.Revit.DB.Architecture;
using Rhino.UI;
using DS.ClassLib.VarUtils;
using static System.Net.Mime.MediaTypeNames;
using Serilog.Core;
using Serilog;
using DS.RevitApp.Test;
using DS.RevitCmd.EnergyTest.SpaceBoundary;
using OLMP.RevitAPI.Tools.Geometry.Points;
using OLMP.RevitAPI.Tools.Graphs;
using QuickGraph;
using boundaryEdge = QuickGraph.TaggedEdge<OLMP.RevitAPI.Tools.Graphs.XYZVertex,
    DS.RevitCmd.EnergyTest.SpaceBoundary.BoundaryCurve>;

namespace DS.RevitCmd.EnergyTest
{
    internal class BestEnergyModelFactory : IEnergyModelFactory, ISerilogged
    {
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly EnergySurfaceFactory _energySurfaceFactory;
        private readonly DocumentFilter _globalFilter;
        private readonly double _eSurfaceSolidThickness = 0.01;


        public BestEnergyModelFactory(
            Document activeDoc,
            IEnumerable<RevitLinkInstance> links,
            EnergySurfaceFactory energySurfaceFactory, DocumentFilter globalFilter)
        {
            _doc = activeDoc;
            _links = links;
            _energySurfaceFactory = energySurfaceFactory;
            _globalFilter = globalFilter;
        }


        public ITransactionFactory TransactionFactory { get; set; }
        public ILogger Logger { get; set; }

        public EnergyModel Create(Space space)
        {
            var eSpace = new EnergySpace(space);

            var energySurfaces = new List<EnergySurface>();

            var bottomTransform = GetBottomTransform(space, out Floor floor);
            var topTransform = GetTopTransform(space, out Element ceiling);
            topTransform = topTransform.Multiply(bottomTransform.Inverse);

            var wallSurfaces = GetWallEnergySurfaces(space, bottomTransform, topTransform, out CurveLoop closedLoop);
            energySurfaces.AddRange(wallSurfaces);
            var floorSurface = GetFloorEnergySurface(floor, closedLoop, _eSurfaceSolidThickness);
            if (closedLoop == null) { return null; }
            if (floorSurface != null)
            { energySurfaces.Add(floorSurface); }
            closedLoop.Transform(topTransform);
            var ceilingSurface = GetCeilingEnergySurface(space, closedLoop, _eSurfaceSolidThickness);
            if (ceilingSurface != null)
            { energySurfaces.Add(ceilingSurface); }

            return new EnergyModel(eSpace, energySurfaces);
        }

        private IEnumerable<EnergySurface> GetWallEnergySurfaces(Space space,
            Transform bottomTransform, Transform topTransform,
            out CurveLoop closedLoop)
        {
            closedLoop = null;

            var eSurfaces = new List<EnergySurface>();

            var options = new SpatialElementBoundaryOptions();

            var boundarySegments = space.GetBoundarySegments(options).SelectMany(sl => sl);
            var boundaryCurves = boundarySegments.Select(s => s.GetCurve());

            //var boundarySegments =  GetExternalBoundaries(space);
            //var boundaryCurves = boundarySegments.Select(s => s.GetCurve());
            //ShowBoundariesOneByOne(boundaryCurves);
            //ShowBoundaries(boundaryCurves);
            //return eSurfaces;

            var analyticalBoundary = GetGraphAnalyticalBoundary(space, boundarySegments, bottomTransform);
            //var analyticalBoundary = GetAnalyticalBoundary(boundarySegments, bottomTransform);
            //analyticalBoundary.ForEach(c => ShowCurve(c.Item1));
            return eSurfaces;


            var connectedCurves = analyticalBoundary.Select(b => b.Item1).ToList();
            closedLoop = CurveLoop.Create(connectedCurves);
            //closedLoop = CurveUtils.TryCreateLoop(connectedCurves);
            closedLoop.ForEach(ShowCurve);
            Debug.WriteLine("Loop close status: " + !closedLoop.IsOpen());
            if (closedLoop.Count() == 0) { return eSurfaces; }

            var p1 = closedLoop.First().GetEndPoint(0);
            var p2 = topTransform?.OfPoint(p1);
            var wallHeight = p2 is null ? space.UnboundedHeight : p2.Z - p1.Z;

            foreach (var boundary in analyticalBoundary)
            {
                var eSurface = _energySurfaceFactory
                    .CreateEnergySurface(boundary.Item2.Id, boundary.Item1, wallHeight);
                //var eSurface = ToEnergySurface(boundary);
                if (eSurface == null)
                {
                    Debug.WriteLine("Failed to get Energy surface!");
                    continue;
                }
                else
                { eSurfaces.Add(eSurface); }
            }

            return eSurfaces;

            void ShowBoundaries(IEnumerable<Curve> curves)
                => TransactionFactory?.Create(() => curves.ForEach(c => c.Show(_doc)), "showCurve");

            void ShowBoundariesOneByOne(IEnumerable<Curve> curves)
            {
                var uiDoc = new UIDocument(_doc);
                foreach (var curve in curves)
                {
                    ShowCurve(curve);
                    ShowPoint(curve.GetEndPoint(0));
                    uiDoc.RefreshActiveView();
                }
            }
        }

        private EnergySurface GetFloorEnergySurface(Floor floor, CurveLoop boundary, double solidThickness)
        {
            try
            {
                var bSolid = GeometryCreationUtilities
                   .CreateExtrusionGeometry(
                   new List<CurveLoop> { boundary },
                   -XYZ.BasisZ, solidThickness);

                return new EnergySurface(bSolid, floor)
                { SurfaceType = Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.ExteriorFloor };
            }
            catch (Exception)
            {

                return null;
            }
        }
        private EnergySurface GetCeilingEnergySurface(Element ceiling, CurveLoop boundary, double solidThickness)
        {
            try
            {
                var bSolid = GeometryCreationUtilities
                    .CreateExtrusionGeometry(
                    new List<CurveLoop> { boundary },
                    XYZ.BasisZ, solidThickness);

                return new EnergySurface(bSolid, ceiling)
                { SurfaceType = Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.Ceiling };
            }
            catch (Exception)
            {

                return null;
            }
        }


        private Transform GetBottomTransform(Space space, out Floor floor)
        {
            double loopWidth = 0.01;
            floor = space.Room.FindNearestFloor(_doc);
            var offsetHeigth = floor.GetThickness() / 2 - loopWidth;
            var moveVector = new XYZ(0, 0, -offsetHeigth);
            return Transform.CreateTranslation(moveVector);
        }

        private Transform GetTopTransform(Space space, out Element ceiling)
        {
            double loopWidth = 0.01;
            ceiling = space.Room.FindNearestCeiling(_doc);

            double ceilingThickness = 0;
            switch (ceiling)
            {
                case Floor floor:
                    ceilingThickness = floor.GetThickness(); break;
                case Ceiling ceilingFloor:
                    ceilingThickness = ceilingFloor.GetThickness(); break;
                default:
                    break;
            }
            var offsetHeigth = space.UnboundedHeight + ceilingThickness / 2 - loopWidth;
            var moveVector = new XYZ(0, 0, offsetHeigth);
            return Transform.CreateTranslation(moveVector);
        }


        private IEnumerable<(Curve, BoundarySegment)> GetAnalyticalBoundary(IList<IList<BoundarySegment>> segmentLists, Transform transform)
        {
            var boundaryCurves = new List<(Curve curve, BoundarySegment segment)>();
            var elems = segmentLists.SelectMany(sl => sl.Select(s => _doc.GetElement(s.ElementId)));
            foreach (var sl in segmentLists)
            {
                foreach (var segment in sl)
                {
                    var id = segment.ElementId;
                    Wall wall = id.IntegerValue > 0 ? _doc.GetElement(id) as Wall : null;
                    //if (wall is null || wall.GetJoints(true).Count() < 2) 
                    //{ continue; }
                    //var c = wall.GetJoints(true).Count();
                    if (wall is null) { continue; }

                    var curve = segment.GetCurve();
                    var distanseToOffset = wall.Width / 2;
                    curve = curve.CreateOffset(distanseToOffset, XYZ.BasisZ);
                    if (curve != null)
                    { curve = curve.CreateTransformed(transform); boundaryCurves.Add((curve, segment)); }
                }
            }
            //return boundaryCurves;
            //if(boundaryCurves.Count < 2) {  return boundaryCurves; }
            var connectedBoundaryCurves = CurveUtils
                .TryConnect<BoundarySegment>(boundaryCurves, getConnectedCurve);
            return connectedBoundaryCurves;

            static Curve getConnectedCurve(Curve current, Curve previous, Curve next)
            => current.TrimOrExtend(previous, next, true, true);
        }

        private IEnumerable<(Curve, Element)> GetGraphAnalyticalBoundary(Space space, IEnumerable<BoundarySegment> boundarySegments, Transform transform)
        {
            var boundaries = new List<(Curve, Element)>();

            var calculator = new SpatialElementGeometryCalculator(_doc, new SpatialElementBoundaryOptions());
            var result = calculator.CalculateSpatialElementGeometry(space);
            var spaceSolid = result.GetGeometry();
            var boundaryElementIds = boundarySegments.Select(s => s.ElementId);
            var boundaryCurves = boundarySegments.Select(s => s.GetCurve());

            var boundaryElements = boundaryElementIds
               .Select(id => _doc.GetElement(id))
               .Where(e => e is not null)
               .DistinctBy(e => e.Id);

            var solidItersectionFactory = GetIntersectionFactory(_globalFilter, boundaryElementIds);
            var validSegments = boundarySegments.Where(e => e is not null && e.ElementId.IntegerValue > 0);
            var edgeBuilder = new ElementEdgeFactory();
            var elementIntersectionFactory = new ElemIntersectionFactory(_doc, solidItersectionFactory);
            var graph = new BoundaryGraphFactory(_doc,
                edgeBuilder, validSegments,
                boundaryCurves, elementIntersectionFactory, spaceSolid)
            { Logger = Logger }
            .Create();
            Show(graph);
            foreach (var edge in graph.Edges)
            {
                var wall = _doc.GetElement(edge.Tag.ElementId) as Wall;
                var curve = edge.Tag.Curve;
                curve = curve.CreateTransformed(transform);
                boundaries.Add((curve, wall));
            }
            return boundaries;

            SolidElementIntersectionFactory GetIntersectionFactory(DocumentFilter globalFilter,
           IEnumerable<ElementId> elementIdsSet)
            {
                var localFilter = globalFilter.Clone();
                localFilter.ElementIdsSet = elementIdsSet.ToList();
                var types = new List<Type>()
            {
                typeof(Wall)
            };
                var multiclassFilter = new ElementMulticlassFilter(types);
                localFilter.QuickFilters.Add((multiclassFilter, null));

                return new SolidElementIntersectionFactory(_doc, _links, localFilter)
                {
                    Logger = null,
                    TransactionFactory = null
                };
            }
        }

        private Floor GetFloor(Document doc, Room room)
        {

            LocationPoint roomPoint;
            ReferenceIntersector intersector;
            ReferenceWithContext rwC;
            Element el = null;
            GeometryObject geoObj;
            Face _face;


            roomPoint = room.Location as LocationPoint;
            try
            {
                IEnumerable<View3D> enumerable()
                {
                    foreach (var v in new FilteredElementCollector(doc).OfClass(typeof(View3D)).Cast<View3D>())
                    {
                        if (v.IsTemplate == false && v.IsPerspective == false)
                        {
                            yield return v;
                        }
                    }
                }
                var view3D = enumerable().First();
                intersector = new ReferenceIntersector(
               new ElementCategoryFilter(BuiltInCategory.OST_Floors),
               FindReferenceTarget.All, view3D);

                rwC = intersector.FindNearest(roomPoint.Point, XYZ.BasisZ);
                el = doc.GetElement(rwC.GetReference().ElementId);

            }
            catch (Exception fl)
            {

                string ctch = fl.ToString();
            }

            var floor = el as Floor;

            return floor;
        }

        private void ShowCurve(Curve curve)
       => TransactionFactory.Create(() => curve.Show(_doc), "ShowCurve");

        private void ShowPoint(XYZ point)
     => TransactionFactory.Create(() => point.Show(_doc), "ShowPoint");

        private IEnumerable<BoundarySegment> GetExternalBoundaries(Space space)
        {
            //var calculator = new SpatialElementGeometryCalculator(_doc);
            //var results = calculator.CalculateSpatialElementGeometry(space); 
            //Solid spaceSolid = results.GetGeometry();
            //ShowSolid(spaceSolid);

            //var faces =spaceSolid.Faces.ToList().OfType<PlanarFace>();
            //var at = 1.DegToRad();
            //var bottomFace = faces
            //    .Where(f=> f.FaceNormal.ToVector3d().IsParallelTo(Rhino.Geometry.Vector3d.ZAxis, at) !=0)
            //    .OrderBy(f => f.Origin.Z)
            //    .First();

            var options = new SpatialElementBoundaryOptions();
            var segmentLists = space.GetBoundarySegments(options);
            //return segmentLists;
            var boundaryCurves = new List<BoundarySegment>();
            foreach (var sl in segmentLists)
            {
                foreach (var segment in sl)
                {
                    var curve = segment.GetCurve();
                    if (_doc.GetElement(segment.ElementId) is Wall wall)
                    {
                        var joints = wall.GetJoints(true);
                        if (joints.Count() == 2)
                        { boundaryCurves.Add(segment); }
                    }

                }
            }

            return boundaryCurves;
        }

        private void ShowSolid(Solid solid)
        {
            using (Transaction transaction = new(_doc, "ShowSolid"))
            {
                transaction.Start();
                solid.ShowShape(_doc);

                if (transaction.HasStarted())
                { transaction.Commit(); }
            }

        }

        private void Show(IVertexAndEdgeListGraph<XYZVertex, boundaryEdge> graph)
        {
            TransactionFactory.Create(() => Show(graph), "ShowVertices");


            void Show(IVertexAndEdgeListGraph<XYZVertex, boundaryEdge> graph)
            {
                var moveVector = new XYZ();
                var view = GetUIView(_doc);
                var xYZVisulalizator = new XYZVisualizator(new UIDocument(_doc));
                foreach (var vertex in graph.Vertices)
                {
                    foreach (var edge in graph.OutEdges(vertex))
                    {

                        var v1 = edge.Source;
                        var v2 = edge.Target;

                        var xyz1 = v1.GetLocation(_doc);
                        var xyz2 = v2.GetLocation(_doc);
                        var center = edge.Tag.Curve.GetCenter();

                        ElementId defaultTypeId = _doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);

                        v1.Tag.Show(_doc);
                        TextNote.Create(_doc, view.ViewId, xyz1 + moveVector, v1.Id.ToString(), defaultTypeId);

                        v2.Tag.Show(_doc);
                        TextNote.Create(_doc, view.ViewId, xyz2 + moveVector, v2.Id.ToString(), defaultTypeId);

                        //xYZVisulalizator.ShowVectorWithoutTransaction(xyz1, xyz2);
                        edge.Tag.Curve.Show(_doc);
                        //TextNote.Create(_doc, view.ViewId, center + moveVector, edge.Tag.ElementId.ToString(), defaultTypeId);
                    }
                }
                UIView GetUIView(Document doc)
                {
                    var uidoc = new UIDocument(doc);
                    var view = uidoc.ActiveGraphicalView;
                    UIView uiview = null;
                    var uiviews = uidoc.GetOpenUIViews();
                    foreach (UIView uv in uiviews)
                    {
                        if (uv.ViewId.Equals(view.Id))
                        {
                            uiview = uv;
                            break;
                        }
                    }
                    return uiview;
                }
            }
        }
    }
}
