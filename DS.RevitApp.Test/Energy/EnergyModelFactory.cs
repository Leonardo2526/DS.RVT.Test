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

namespace DS.RevitApp.Test.Energy
{
    internal class EnergyModelFactory : IEnergyModelFactory
    {
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly IEnergySurfaceFactory _energySurfaceFactory;

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
            var wallSurfaces = GetWallEnergySurfaces(space);
            var floorSurfaces = GetFloorEnergySurfaces(space);
            var ceilingSurfaces = GetCeilingEnergySurfaces(space);
            energySurfaces.AddRange(wallSurfaces);
            energySurfaces.AddRange(floorSurfaces);
            energySurfaces.AddRange(ceilingSurfaces);

            return new EnergyModel(eSpace, energySurfaces);
        }

        private IEnumerable<EnergySurface> GetWallEnergySurfaces(Space space)
        {
            var eSurfaces = new List<EnergySurface>();

            var options = new SpatialElementBoundaryOptions();

            var boundarySegments = space.GetBoundarySegments(options);
            var boundaryCurves = boundarySegments.SelectMany(sl => sl.Select(s => s.GetCurve()));

            //var boundarySegments =  GetExternalBoundaries(space);
            //var boundaryCurves = boundarySegments.Select(s => s.GetCurve());
            //ShowBoundariesOneByOne(boundaryCurves);
            ShowBoundaries(boundaryCurves);
            return eSurfaces;

            var analyticalBoundary = GetAnalyticalBoundary(boundarySegments);
            TransactionFactory?.Create(() => analyticalBoundary.ForEach(c => c.Item1.Show(_doc)), "showCurve");
            foreach (var boundary in analyticalBoundary)
            {
                var eSurface = _energySurfaceFactory.CreateEnergySurface(boundary.Item2, boundary.Item1);
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

        private IEnumerable<EnergySurface> GetFloorEnergySurfaces(Space space)
        {
            var energySurfaces = new List<EnergySurface>();

            return energySurfaces;
        }

        private IEnumerable<EnergySurface> GetCeilingEnergySurfaces(Space space)
        {
            var energySurfaces = new List<EnergySurface>();

            return energySurfaces;
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

        private IEnumerable<(Curve, BoundarySegment)> GetAnalyticalBoundary(IList<IList<BoundarySegment>> segmentLists)
        {
            var boundaryCurves = new List<(Curve curve, BoundarySegment segment)>();
            foreach (var sl in segmentLists)
            {
                foreach (var segment in sl)
                {
                    var curve = segment.GetCurve();
                    var distanseToOffset = _doc.GetElement(segment.ElementId) is Wall wall ? 
                        wall.Width / 2 : 0;
                    curve = curve.CreateOffset(distanseToOffset, XYZ.BasisZ);
                    if (curve != null)
                    { boundaryCurves.Add((curve, segment)); }
                }
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
                        if(joints.Count() == 2)
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
