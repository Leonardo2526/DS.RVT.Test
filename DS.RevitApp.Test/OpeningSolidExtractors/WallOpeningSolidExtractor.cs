using Autodesk.Revit.DB;
using DS.RevitApp.Test.Energy;
using OLMP.RevitAPI.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OLMP.RevitAPI.Tools.Geometry;

namespace DS.RevitApp.Test.OpeningSolidExtractors
{
    internal class WallOpeningSolidExtractor : OpeningSolidExtractorBase<Wall>
    {
        private readonly Curve _wallCurve;

        public WallOpeningSolidExtractor(
            Wall hostElement,
            Document activeDoc,
            IEnumerable<RevitLinkInstance> links = null) : 
            base(hostElement, activeDoc, links)
        {
            _wallCurve = hostElement.GetLocationCurve(links);

        }

        public override Solid GetSolid(Opening opening)
        {
            var profile = GetProfile(opening, _hostElement, _wallCurve);
            if(profile.IsOpen())
            { throw new Exception("Profile isn't close."); }
            var profileLoops = new List<CurveLoop>()
            { profile };
            var sweepPathCurve = GetSweepPath(opening, _wallCurve);
            var sweepPath = CurveLoop.Create(new List<Curve>() { sweepPathCurve });
            var startParam = sweepPathCurve.GetEndParameter(0);


            return GeometryCreationUtilities.CreateSweptGeometry(sweepPath, 0, startParam, profileLoops);
        }

        private CurveLoop GetProfile(Opening opening, Wall wall, Curve wallCurve)
        {
            var boundPoints = opening.BoundaryRect;
            
            var curve2 = OffsetToPoint(wallCurve, boundPoints[1]);

            var mainWallYFaces = wall.GetMainYFaces(_links);
            var p1 = mainWallYFaces.Item1.Project(boundPoints[0], true).ToPoint3d();
            var proj02 = curve2.Project(boundPoints[0]).XYZPoint;
            var p2 = mainWallYFaces.Item2.Project(proj02, true).ToPoint3d();
            var offsetCurve = wallCurve.CreateOffset(-1, XYZ.BasisZ);
            var origin = mainWallYFaces.Item1.Project(p2.ToXYZ(), true).ToPoint3d();

            var plane = new Rhino.Geometry.Plane(origin, p1, p2);
            var rectangle = new Rhino.Geometry.Rectangle3d(plane, p1, p2);
            var lines = rectangle.ToRevitLines();

            var curves = new List<Curve>();
            lines.ForEach(l => curves.Add(l));
            return CurveLoop.Create(curves);

            static Curve OffsetToPoint(Curve curve, XYZ boundPoints)
            {
                var proj = curve.Project(boundPoints);
                var moveVector = boundPoints - proj.XYZPoint;
                var transform = Autodesk.Revit.DB.Transform.CreateTranslation(moveVector);
                return curve.CreateTransformed(transform);
            }
        }


        private Curve GetSweepPath(Opening opening, Curve wallCurve)
        {
            var boundPoints = opening.BoundaryRect;

            var p1 = boundPoints[0];
            var result1 = wallCurve.Project(p1);
            var param1 = result1.Parameter;

            var p2 = boundPoints[1];
            var result2 = wallCurve.Project(p2);
            var param2 = result2.Parameter;
            var sweepCurve = wallCurve.Clone();
            sweepCurve.MakeBound(param1, param2);

            return sweepCurve;
        }
    }
}
