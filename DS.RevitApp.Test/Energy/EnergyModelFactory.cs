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


            var segments = space.GetBoundarySegments(options);

            var boundaryCurves = segments.SelectMany(sl => sl.Select(s => s.GetCurve()));
            ShowBoundaries(boundaryCurves);
            //return eSurfaces;

            var analyticalBoundary = GetAnalyticalBoundary(segments);
            TransactionFactory?.Create(() => analyticalBoundary.ForEach(c => c.Item2.Show(_doc)), "showCurve");
            foreach (var boundary in analyticalBoundary)
            {
                var eSurface = _energySurfaceFactory.CreateEnergySurface(boundary.Item1, boundary.Item2);
                //var eSurface = ToEnergySurface(boundary);
                if (eSurface == null)
                { throw new Exception("Failed to get Energy surface!"); }
                else
                { eSurfaces.Add(eSurface); }
            }

            return eSurfaces;

            void ShowBoundaries(IEnumerable<Curve> curves)
                => TransactionFactory?.Create(() => curves.ForEach(c => c.Show(_doc)), "showCurve");
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

        private IEnumerable<(BoundarySegment, Curve)> GetAnalyticalBoundary(IList<IList<BoundarySegment>> segmentLists)
        {
            var boundaryCurves = new List<(BoundarySegment segment, Curve curve)>();
            foreach (var sl in segmentLists)
            {
                foreach (var segment in sl)
                {
                    if (_doc.GetElement(segment.ElementId) is Wall wall)
                    {
                        var curve = segment.GetCurve();
                        var distanseToOffset = wall.Width / 2;
                        curve = curve.CreateOffset(distanseToOffset, XYZ.BasisZ);
                        if (curve != null)
                        { boundaryCurves.Add((segment, curve)); }
                    }
                }
            }
            //return boundaryCurves;
            var connectedCurves = CurveUtils.TryConnect(boundaryCurves.Select(x => x.curve), getConnectedCurve);
            //var closedLoop = curveLoop.TryCreateLoop() ?? throw new Exception();
            var result = new List<(BoundarySegment, Curve)>();
            for (int i = 0; i < boundaryCurves.Count; i++)
            {
                var connectedCurve = connectedCurves.ElementAt(i);
                var (segment, curve) = boundaryCurves[i];
                result.Add((segment, connectedCurve));
            }
            return result;
            //return Connect(offsetted);

            static Curve getConnectedCurve(Curve current, Curve previous, Curve next)
            {
                var result = current.TrimOrExtend(previous, true, true, 1)
                  .FirstOrDefault();
                return result?.TrimOrExtend(next, true, true, 0)
                    .FirstOrDefault();
            }
        }
    }
}
