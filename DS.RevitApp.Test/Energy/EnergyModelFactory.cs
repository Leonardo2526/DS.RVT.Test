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
using System.Windows.Forms;

namespace DS.RevitApp.Test.Energy
{
    internal class EnergyModelFactory : IEnergyModelFactory
    {
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly IEnergySurfaceFactory _energySurfaceFactory;
        private readonly double _eSurfaceSolidThickness = 0.01;


        public EnergyModelFactory(
            Document activeDoc,
            IEnumerable<RevitLinkInstance> links,
            IEnergySurfaceFactory energySurfaceFactory)
        {
            _doc = activeDoc;
            _links = links;
            _energySurfaceFactory = energySurfaceFactory;
        }


        public ITransactionFactory TransactionFactory { get; set; }


        public EnergyModel Create(Space space)
        {
            var eSpace = new EnergySpace(space);

            var energySurfaces = new List<EnergySurface>();

            var bottomTransform = GetBottomTransform(space, out Floor floor);
            var topTransform = GetTopTransform(space, out Element ceiling);
            topTransform = topTransform.Multiply(bottomTransform.Inverse);

            var wallSurfaces = GetWallEnergySurfaces(space, bottomTransform, topTransform, out CurveLoop closedLoop);
            energySurfaces.AddRange(wallSurfaces);
            var floorSurfaces = GetFloorEnergySurface(floor, closedLoop, _eSurfaceSolidThickness);
            energySurfaces.Add(floorSurfaces);
            closedLoop.Transform(topTransform);
            var ceilingSurface = GetCeilingEnergySurface(space, closedLoop, _eSurfaceSolidThickness);
            energySurfaces.Add(ceilingSurface);

            return new EnergyModel(eSpace, energySurfaces);
        }

        private IEnumerable<EnergySurface> GetWallEnergySurfaces(Space space,
            Transform bottomTransform, Transform topTransform,
            out CurveLoop closedLoop)
        {
            var eSurfaces = new List<EnergySurface>();

            var options = new SpatialElementBoundaryOptions();

            var boundarySegments = space.GetBoundarySegments(options);
            var boundaryCurves = boundarySegments.SelectMany(sl => sl.Select(s => s.GetCurve()));

            //var boundarySegments =  GetExternalBoundaries(space);
            //var boundaryCurves = boundarySegments.Select(s => s.GetCurve());
            //ShowBoundariesOneByOne(boundaryCurves);
            //ShowBoundaries(boundaryCurves);
            //return eSurfaces;

            var analyticalBoundary = GetAnalyticalBoundary(boundarySegments, bottomTransform);
            //analyticalBoundary.ForEach(c => ShowCurve(c.Item1));

            var connectedCurves = analyticalBoundary.Select(b => b.Item1);
            closedLoop = CurveUtils.TryCreateLoop(connectedCurves);
            closedLoop.ForEach(ShowCurve);
            Debug.WriteLine("Loop close status: " + !closedLoop.IsOpen());

            var p1 = closedLoop.First().GetEndPoint(0);
            var p2 = topTransform?.OfPoint(p1);
            var wallHeight = p2 is null ? space.UnboundedHeight : p2.Z - p1.Z;

            foreach (var boundary in analyticalBoundary)
            {
                var eSurface = _energySurfaceFactory
                    .CreateEnergySurface(boundary.Item2, boundary.Item1, wallHeight);
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
            var bSolid = GeometryCreationUtilities
               .CreateExtrusionGeometry(
               new List<CurveLoop> { boundary },
               -XYZ.BasisZ, solidThickness);

            return new EnergySurface(bSolid, floor)
            { SurfaceType = Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.ExteriorFloor };
        }
        private EnergySurface GetCeilingEnergySurface(Element ceiling, CurveLoop boundary, double solidThickness)
        {
            var bSolid = GeometryCreationUtilities
                .CreateExtrusionGeometry(
                new List<CurveLoop> { boundary },
                XYZ.BasisZ, solidThickness);

            return new EnergySurface(bSolid, ceiling)
            { SurfaceType = Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.Ceiling };
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


        private IEnumerable<(Curve, BoundarySegment)> GetAnalyticalBoundary(IEnumerable<BoundarySegment> segmentLists)
        {
            var boundaryCurves = new List<(Curve curve, BoundarySegment segment)>();
            foreach (var segment in segmentLists)
            {
                var curve = segment.GetCurve();
                var distanseToOffset = _doc.GetElement(segment.ElementId) is Wall wall ?
                    wall.Width / 2 : 0;
                curve = curve.CreateOffset(distanseToOffset, XYZ.BasisZ);
                if (curve != null)
                { boundaryCurves.Add((curve, segment)); }
            }
            //return boundaryCurves;
            var connectedBoundaryCurves = CurveUtils
                .TryConnect<BoundarySegment>(boundaryCurves, getConnectedCurve);
            return connectedBoundaryCurves;

            static Curve getConnectedCurve(Curve current, Curve previous, Curve next)
            {
                var result = current.TrimOrExtend(previous, true, true, 1)
                  .FirstOrDefault();
                return result?.TrimOrExtend(next, true, true, 0)
                    .FirstOrDefault();
            }
        }

        private IEnumerable<(Curve, BoundarySegment)> GetAnalyticalBoundary(IList<IList<BoundarySegment>> segmentLists, Transform transform)
        {
            var boundaryCurves = new List<(Curve curve, BoundarySegment segment)>();
            foreach (var sl in segmentLists)
            {
                foreach (var segment in sl)
                {
                    var id = segment.ElementId;
                    Wall wall = id.IntegerValue > 0 ? _doc.GetElement(id) as Wall : null;
                    if (wall is null || wall.GetJoints(true).Count() < 2) { continue; }

                    var curve = segment.GetCurve();
                    var distanseToOffset = wall.Width / 2;
                    curve = curve.CreateOffset(distanseToOffset, XYZ.BasisZ);
                    if (curve != null)
                    { curve = curve.CreateTransformed(transform); boundaryCurves.Add((curve, segment)); }
                }
            }
            //return boundaryCurves;
            var connectedBoundaryCurves = CurveUtils
                .TryConnect<BoundarySegment>(boundaryCurves, getConnectedCurve);
            return connectedBoundaryCurves;

            static Curve getConnectedCurve(Curve current, Curve previous, Curve next)
            => current.TrimOrExtend(previous, next, true, true);
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
    }
}
