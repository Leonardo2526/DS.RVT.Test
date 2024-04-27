using Autodesk.Revit.DB;
using DS.RevitApp.Test.Energy;
using OLMP.RevitAPI.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OLMP.RevitAPI.Tools.Geometry;
using Rhino.Geometry;
using DS.RevitApp.Test.WallUtilsTests;
using OLMP.RevitAPI.Tools.Extensions.RhinoExtensions;

namespace DS.RevitApp.Test.OpeningSolidExtractors
{
    internal class FloorOpeningSolidExtractor : OpeningSolidExtractorBase<Floor>
    {
        public FloorOpeningSolidExtractor(Floor hostElement,
            Document activeDoc,
            IEnumerable<RevitLinkInstance> links = null) :
            base(hostElement, activeDoc, links)
        {
        }

        public override Solid GetSolid(Opening opening)
            => TryGetBox(opening, _activeDoc, _links, out var box) ? 
            box.GetSolid() : null;

        private bool TryGetBox(Opening opening, Document activeDoc, IEnumerable<RevitLinkInstance> links, out Box box)
        {
            box = default;

            var mainPlanarFaces = _hostElement.GetMainFaces(links);

            var basePlane0 = mainPlanarFaces.Item1.GetPlane().ToRhinoPlane();
            var basePlane1 = mainPlanarFaces.Item2.GetPlane().ToRhinoPlane();

            var boundaries = opening.BoundaryRect;
            if (opening.Document.IsLinked)
            {
                var realBoundaries = new List<XYZ>();
                foreach (var bound in boundaries)
                {
                    var linkBound = bound.ToActiveDoc(opening.Document, activeDoc, links);
                    realBoundaries.Add(linkBound);
                }
                boundaries = realBoundaries;
            }

            var corners = new List<Point3d>()
                {
                   basePlane0.ClosestPoint(boundaries[0].ToPoint3d()),
                   basePlane0.ClosestPoint(boundaries[1].ToPoint3d()),
                   basePlane1.ClosestPoint(boundaries[0].ToPoint3d()),
                   basePlane1.ClosestPoint(boundaries[1].ToPoint3d())
                };

            var rX = mainPlanarFaces.Item1.XVector.ToVector3d();
            var rY = mainPlanarFaces.Item1.YVector.ToVector3d();

           var plane =  new Rhino.Geometry.Plane(corners.First(), rX, rY);

            box = new Box(plane, corners);
            return true;

        }
    }
}
