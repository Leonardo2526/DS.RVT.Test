using Autodesk.Revit.DB;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OLMP.RevitAPI.Tools.Extensions.RhinoExtensions;
using OLMP.RevitAPI.Tools.Extensions;

namespace DS.RevitCmd.EnergyTest
{
    internal class WallInsertSolidModel : InsertsSolidModelBase<Wall>
    {
        public WallInsertSolidModel(
            Wall element,
            Document activeDoc,
            IEnumerable<RevitLinkInstance> links = null) 
            : base(element, activeDoc, links)
        {
        }

        protected override IEnumerable<Element> GetInserts()
        {
            var wallInseretsIds = Element.FindInserts(true, false, false, false) ?? new List<ElementId>();
            return wallInseretsIds.Select(_activeDoc.GetElement).ToList();
        }

        protected override Solid GetInsertSolid(Solid solid)
        {
            var points = solid.ExtractPoints();
            var rPoints = points.Select(p => p.ToPoint3d());

            var centroid = solid.ComputeCentroid();

            var wallCurve = Element.GetLocationCurve(_links);
            var projection = wallCurve.Project(centroid);

            var basis = this.Element.GetBasis(projection.Parameter, _links);

            var origin = projection.XYZPoint.ToPoint3d();
            var v1 = basis.X;
            var v2 = basis.Y;

            var plane = new Rhino.Geometry.Plane(origin, v1, v2);
            var box = new Box(plane, rPoints);
            return box.GetSolid();
        }

    }
}
