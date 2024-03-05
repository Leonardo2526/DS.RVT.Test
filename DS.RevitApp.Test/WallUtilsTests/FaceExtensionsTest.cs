using Autodesk.Revit.DB;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.WallUtilsTests
{
    internal static class FaceExtensionsTest
    {
        public static Solid GetSolid(this Face face)
        {
            var surfaceLoops = face.GetEdgesAsCurveLoops();
            var dir = face.ComputeNormal(new UV());
            var extrusionValue = Math.Pow(0.1, 7);
            return GeometryCreationUtilities
                .CreateExtrusionGeometry(surfaceLoops, dir, extrusionValue);
        }
    }
}
