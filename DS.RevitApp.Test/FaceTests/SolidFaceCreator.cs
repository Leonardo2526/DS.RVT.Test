using Autodesk.Revit.DB;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Extensions;

namespace DS.RevitApp.Test.FaceTests
{
    internal class SolidFaceCreator
    {
        public static Solid CreateSolid(Face face, double solidWidth)
        {
            var loop = CreateLoop(face, solidWidth, out XYZ extrusionVector);
            return CreateSolid(loop, extrusionVector);
        }

        public static Solid CreateSolid(PlanarFace face, double solidWidth)
        {
            var loop = CreateLoop(face, solidWidth, out XYZ extrusionVector);
            return CreateSolid(loop, extrusionVector);
        }

        private static Solid CreateSolid(CurveLoop loop, XYZ extrusionVector)
        {
            if (loop.IsOpen()) 
            { throw new Exception("Loop is not closed!"); }

            var extrustionDir = extrusionVector.Normalize();
            var extrusionDist = extrusionVector.GetLength();
            return GeometryCreationUtilities
                .CreateExtrusionGeometry(
                new List<CurveLoop> { loop },
                extrustionDir, extrusionDist);
        }

        private static CurveLoop CreateLoop(
            PlanarFace planarFace,
            double solidWidth,
            out XYZ extrusionVector)
        {
            extrusionVector = planarFace.FaceNormal.Multiply(solidWidth);
            var transform = Autodesk.Revit.DB.Transform
                .CreateTranslation(-extrusionVector.Multiply(0.5));
            var outerLoop = planarFace.GetOuterLoop();
            outerLoop.Transform(transform);
            return outerLoop;
        }

        private static CurveLoop CreateLoop(
            Face face,
            double solidWidth,
            out XYZ extrusionVector)
        {
            var outerLoop = face.GetOuterLoop();
            var extrusionCurves = outerLoop
                .Where(c => c is not Autodesk.Revit.DB.Line);
            var rhinoCurves = extrusionCurves.Select(c => c.ToCurve());
            var result = Rhino.Geometry.Curve.JoinCurves(rhinoCurves);
            if (result.Count() != 2)
            { throw new Exception("extrusionCurves invalid count."); }

            var curve1 = result.First().ToCurve();
            var curve2 = result.Last().ToCurve();
            var centerP1 = curve1.GetCenter();
            var proj = curve2.Project(centerP1);
            if (proj == null)
            { throw new Exception("Failed to get loop."); }
            extrusionVector = proj.XYZPoint - centerP1;

            return curve1.CreateLoop(solidWidth, extrusionVector.Normalize());
        }

    }
}
