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
            //TransactionFactory?.Create(() => boundaryCurves.ForEach(c => c.Show(_doc)), "showCurve");
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
            var offsetted = new List<(BoundarySegment, Curve)>();
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
                        { offsetted.Add((segment, curve)); }
                    }
                }
            }

            return Connect(offsetted);
        }


        private IEnumerable<(BoundarySegment, Curve)> Connect(IEnumerable<(BoundarySegment, Curve)> segementCurves)
        {
            var lastCurve = segementCurves.LastOrDefault();
            var connectedsCurves = new List<(BoundarySegment, Curve)>()
            {
                lastCurve
            };

            for (int i = 0; i < segementCurves.Count() - 1; i++)
            {
                var segmentCurve1 = segementCurves.ElementAt(i);
                var result = FitToIntersection(segmentCurve1.Item2, connectedsCurves.Last().Item2, true);

                var curve2 = segementCurves.ElementAt(i + 1);
                result = FitToIntersection(result, curve2.Item2, false);
                connectedsCurves.Add((segmentCurve1.Item1, result));
            }

            connectedsCurves.RemoveAt(0);

            var lastResult = FitToIntersection(lastCurve.Item2, connectedsCurves.First().Item2, false);
            lastResult = FitToIntersection(lastResult, connectedsCurves.Last().Item2, true);
            connectedsCurves.Add((lastCurve.Item1, lastResult));

            return connectedsCurves;

            Curve FitToIntersection(Curve curve1, Curve curve2, bool fromStart)
            {
                var cloned1 = curve1.Clone();
                var cloned2 = curve2.Clone();
                cloned1.MakeUnbound();
                cloned2.MakeUnbound();
                var intersection = cloned1.Intersect(cloned2, out var resultArray);
                if (intersection == SetComparisonResult.Overlap)
                {
                    var interectionResult = resultArray.get_Item(0);
                    var interectionPoint = interectionResult.XYZPoint;

                    var result = cloned1.Project(interectionPoint);

                    var p1 = curve1.GetEndParameter(0);
                    var p2 = curve1.GetEndParameter(1);

                    var param11 = fromStart ? result.Parameter : p1;
                    var param12 = fromStart ? p2 : result.Parameter;
                    if (param11 > param12) { return curve1; }
                    cloned1.MakeBound(param11, param12);
                    return cloned1;
                }
                else if (intersection == SetComparisonResult.Equal)
                {
                    return curve1;
                }

                return null;
            }
        }

        //private EnergySurface ToEnergySurface((BoundarySegment, Curve) boundary)
        //{
        //    var loop = CreateLoop(boundary.Item2, 0.01, XYZ.BasisZ);
        //    var isClosed = !loop.IsOpen();
        //    if (!isClosed) { throw new Exception("Loop is not closed!"); }

        //    var id = boundary.Item1.ElementId;
        //    Wall wall = id.IntegerValue > 0 ? _doc.GetElement(id) as Wall : null;
        //    if(wall is null) { return null; }
        //    var wModel = new WallInsertSolidModel(wall, _doc, _links);
        //    var bSolid = GetBoundarySolid(wall, loop, wModel);
        //    var insertsSurfaces = GetInsertsSurfaces();
        //    return bSolid is null ?
        //        null :
        //        new EnergySurface(bSolid, _doc.GetElement(boundary.Item1.ElementId))
        //        { SurfaceType = Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.ExteriorWall };

        //    CurveLoop CreateLoop(Curve curve, double loopWidth, XYZ referenceVector)
        //    {
        //        var offsetCurve1 = curve.CreateOffset(loopWidth, -referenceVector);
        //        var offsetCurve2 = curve.CreateOffset(loopWidth, referenceVector).CreateReversed();

        //        var p1 = offsetCurve1.GetEndPoint(0);
        //        var p2 = offsetCurve1.GetEndPoint(1);
        //        var p3 = offsetCurve2.GetEndPoint(0);
        //        var p4 = offsetCurve2.GetEndPoint(1);

        //        var line1 = Line.CreateBound(p2, p3);
        //        var line2 = Line.CreateBound(p4, p1);
        //        return CurveLoop.Create(new List<Curve>() { offsetCurve1, line1, offsetCurve2, line2 });
        //    }

        //    Solid GetBoundarySolid(Wall wall, CurveLoop loop, InsertsSolidModelBase<Wall> insertsModel)
        //    {
        //        var heigth = wall.GetHeigth();
        //        var bSolid = GeometryCreationUtilities
        //            .CreateExtrusionGeometry(new List<CurveLoop> { loop }, XYZ.BasisZ, heigth);

        //        var insertsSolids = insertsModel.GetAllInsertsSolidModels()
        //            .Select(m => m.solid);

        //        insertsSolids.ForEach(insSolid => bSolid = BooleanOperationsUtils
        //                   .ExecuteBooleanOperation(bSolid, insSolid,
        //                               BooleanOperationsType.Difference));

        //        return bSolid;
        //    }
        //}
    }
}
