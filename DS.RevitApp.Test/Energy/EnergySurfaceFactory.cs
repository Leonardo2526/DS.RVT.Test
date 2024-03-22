using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OLMP.RevitAPI.Tools.Extensions;
using MoreLinq;
using System.Collections;
using Rhino.UI;

namespace DS.RevitApp.Test.Energy
{
    internal class EnergySurfaceFactory : IEnergySurfaceFactory
    {
        private readonly static double _loopWidth = 0.01;

        private readonly Document _activeDoc;
        private readonly IEnumerable<RevitLinkInstance> _links;

        public EnergySurfaceFactory(Document activeDoc, IEnumerable<RevitLinkInstance> links)
        {
            _activeDoc = activeDoc;
            _links = links;
        }

        public EnergySurface CreateEnergySurface(BoundarySegment segment, Curve baseCurve, double height)
        {
            var id = segment.ElementId;
            Wall wall = id.IntegerValue > 0 ? _activeDoc.GetElement(id) as Wall : null;
            if (wall is null) { return null; }
            //if (wall is null || wall.GetJoints(true).Count() < 2) { return null; }

            var loop = CreateLoop(baseCurve, _loopWidth, XYZ.BasisZ);
            if (loop.IsOpen()) { throw new Exception("Loop is not closed!"); }

            var wModel = new WallInsertSolidModel(wall, _activeDoc, _links);
            var bSolid = GeometryCreationUtilities
                .CreateExtrusionGeometry(
                new List<CurveLoop> { loop },
                XYZ.BasisZ, height);

            var allInsertsSolidModels = wModel.GetAllInsertsSolidModels();
            //allInsertsSolidModels.ForEach(x => ShowSolid(x.solid));
            var windowsAndDoorsModels = wModel.GetWindowsAndDoorsSolidModels();
            var hostEnergySolid = GetHostEnergySolid(bSolid, allInsertsSolidModels.Select(m => m.solid));
            var windowsAndDoorsSurfaces = GetInsertSurface(windowsAndDoorsModels, bSolid);

            return new EnergySurface(hostEnergySolid, wall, windowsAndDoorsSurfaces)
            { SurfaceType = Autodesk.Revit.DB.Analysis.EnergyAnalysisSurfaceType.ExteriorWall };
        }

        private CurveLoop CreateLoop(Curve baseCurve, double loopWidth, XYZ referenceVector)
        {
            var offsetCurve1 = baseCurve.CreateOffset(loopWidth, -referenceVector);
            var offsetCurve2 = baseCurve.CreateOffset(loopWidth, referenceVector).CreateReversed();

            var p1 = offsetCurve1.GetEndPoint(0);
            var p2 = offsetCurve1.GetEndPoint(1);
            var p3 = offsetCurve2.GetEndPoint(0);
            var p4 = offsetCurve2.GetEndPoint(1);

            var line1 = Line.CreateBound(p2, p3);
            var line2 = Line.CreateBound(p4, p1);
            return CurveLoop.Create(new List<Curve>() { offsetCurve1, line1, offsetCurve2, line2 });
        }

        private Solid GetHostEnergySolid(Solid bSolid, IEnumerable<Solid> insertsSolids)
        {
            var resultSolid = SolidUtils.Clone(bSolid);
            insertsSolids.ForEach(insSolid => resultSolid = BooleanOperationsUtils
                       .ExecuteBooleanOperation(resultSolid, insSolid,
                                   BooleanOperationsType.Difference));
            return resultSolid;
        }


        private IEnumerable<EnergySurface> GetInsertSurface(
            IEnumerable<(Element insert, Solid solid)> insertstModels, Solid bSolid)
        {
            var eSurfaces = new List<EnergySurface>();

            foreach (var (insert, solid) in insertstModels)
            {
                var eSolid = BooleanOperationsUtils
                    .ExecuteBooleanOperation(bSolid, solid,
                    BooleanOperationsType.Intersect);
                if (eSolid != null)
                {
                    var eSurface = new EnergySurface(eSolid, insert);
                    eSurfaces.Add(eSurface);
                }
            }

            return eSurfaces;
        }


        private void ShowSolid(Solid solid)
        {
            using (Transaction transaction = new(_activeDoc, "ShowSolid"))
            {
                transaction.Start();
                solid.ShowShape(_activeDoc);

                if (transaction.HasStarted())
                {transaction.Commit();}
            }

        } 
    }
}
