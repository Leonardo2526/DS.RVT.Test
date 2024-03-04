using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Basis;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Models;
using Rhino.Geometry;

namespace DS.RevitApp.Test.WallUtilsTests
{
    /// <summary>
    /// An object that represents filters for <see cref="Autodesk.Revit.DB.Face"/>.
    /// </summary>
    public static class FaceFilterTest
    {
        private static readonly double _at = 1.DegToRad();

        /// <summary>
        /// Get <see cref="Predicate{T}"/> to filter <see cref="Autodesk.Revit.DB.Face"/>s 
        /// with normal vectors that are parallel to <paramref name="wall"/> curve direction.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// <see langword="true"/> if <see cref="Autodesk.Revit.DB.Face"/> BasisZ 
        /// is parallel to <paramref name="wall"/> curve BasisX.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static Predicate<Face> XNormal(Wall wall, IEnumerable<RevitLinkInstance> links = null)
          => f => !(ZNormal().Invoke(f) || YNormal(wall, links).Invoke(f));

        /// <summary>
        /// Get <see cref="Predicate{T}"/> to filter <see cref="Autodesk.Revit.DB.Face"/>s 
        /// with normal vectors that are perpendicular to <paramref name="wall"/> 
        /// curve direction but not to BasisZ.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// <see langword="true"/> if <see cref="Autodesk.Revit.DB.Face"/> BasisZ 
        /// is parallel to <paramref name="wall"/> curve BasisY.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static Predicate<Face> YNormal(Wall wall,IEnumerable<RevitLinkInstance> links)
          => f => TryGetBases(wall, f, links, out var bases)
            && bases.wallBasis.Y.IsParallelTo(bases.faceBasis.Z, _at) != 0;

        /// <summary>
        /// Get <see cref="Predicate{T}"/> to filter <see cref="Autodesk.Revit.DB.Face"/>s 
        /// with normal vectors that are parallel to <see cref="Autodesk.Revit.DB.XYZ.BasisZ"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// <see langword="true"/> if <see cref="Autodesk.Revit.DB.Face"/> BasisZ 
        /// is parallel to <see cref="Autodesk.Revit.DB.XYZ.BasisZ"/>.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static Predicate<Face> ZNormal()
            => f => f.ComputeNormal(new UV()).ToVector3d().IsParallelTo(Vector3d.ZAxis, _at) != 0;


        private static bool TryGetBases(Wall wall, 
            Face face,
            IEnumerable<RevitLinkInstance> links, 
            out (Basis3d wallBasis, Basis3d faceBasis) bases)
        {
            bases = default;

            var wallCurve = wall.GetLocationCurve(links);
            var parameter = wallCurve.ComputeCenterNormalizedParameter();
            bases.wallBasis = wall.GetBasis(parameter, links);

            var projection = face.Project(bases.wallBasis.Origin.ToXYZ());
            if (projection == null) { return false; }
            var transformation = face.ComputeDerivatives(projection.UVPoint);
            bases.faceBasis = new Basis3d(
                transformation.Origin.ToPoint3d(),
                transformation.BasisX.ToVector3d(),
                transformation.BasisY.ToVector3d(),
                transformation.BasisZ.ToVector3d());
            return true;
        }

    }
}
