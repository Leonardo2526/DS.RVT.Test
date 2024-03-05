using Autodesk.Revit.DB;
using DS.RevitApp.Test.OpeningSolidExtractors;
using MoreLinq;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Extensions.RhinoExtensions;
using OLMP.RevitAPI.Tools.Geometry;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitApp.Test.WallUtilsTests
{
    /// <summary>
    /// Extension methods for <see cref="Autodesk.Revit.DB.Opening"/>.
    /// </summary>
    public static class OpeningExtensionsTest
    {
        /// <summary>
        /// Check if <paramref name="opening"/> contains <paramref name="point"/>.
        /// </summary>
        /// <param name="opening"></param>
        /// <param name="point"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="point"/> 
        /// is inside <paramref name="opening"/>'s bounds.
        /// <para>
        /// <see langword="false"/> if the <paramref name="point"/> 
        /// is ouside of the <paramref name="opening"/> bounds or it was failed to get bound.
        /// </para>
        /// </returns>
        public static bool IsPointContains(this Opening opening, XYZ point, Document activeDoc, IEnumerable<RevitLinkInstance> links)
        {
            if (!TryGetBox(opening, activeDoc, links, out var box)) return false;
            return box.Contains(point.ToPoint3d());
        }

        /// <summary>
        /// Try to get <see cref="Rhino.Geometry.Box"/> from <paramref name="opening"/> that represents bounds of <paramref name="opening"/>.
        /// </summary>
        /// <param name="opening"></param>
        /// <param name="activeDoc"></param>
        /// <param name="box"></param>
        /// <returns>
        /// <see langword="true"/> if operation was succefully completed.
        /// <para>
        /// <see langword="false"/> if <paramref name="opening"/> host element is not a <see cref="Wall"/> or it hasn't rectangle boundary.
        /// </para>
        /// </returns>
        public static bool TryGetBox(this Opening opening, Document activeDoc, IEnumerable<RevitLinkInstance> links, out Box box)
        {
            box = default;

            var rhinoCorners = TryGetRectCorners(opening, activeDoc, links);
            if (rhinoCorners is null)
            { return false; }

            var basePlane = GetMainPlanarFaces(opening, activeDoc, links).FirstOrDefault().
                GetPlane().
                ToRhinoPlane();
            box = new Box(basePlane, rhinoCorners);
            return true;

        }

        /// <summary>
        /// Get <see cref="PlanarFace"/>s of the <paramref name="opening"/> that represent main vertical faces
        /// with normals parallel to <paramref name="opening"/> Y basis.
        /// </summary>
        /// <param name="opening"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// List of <see cref="PlanarFace"/>s.
        /// <para>
        /// Empty list if no <see cref="PlanarFace"/>s were found.
        /// </para>
        /// <para>
        /// <see langword="null"/> if <paramref name="opening"/> host element is not a <see cref="Wall"/>.
        /// </para>
        /// </returns>
        public static IEnumerable<PlanarFace> GetMainPlanarFaces(this Opening opening, Document activeDoc, IEnumerable<RevitLinkInstance> links)
            => opening.Host is not Wall wall ?
            null :
            wall.GetMainPlanarFaces(activeDoc, links);

        /// <summary>
        /// Try to get <paramref name="opening"/> corners if <paramref name="opening"/> has rectangle boundary.
        /// </summary>
        /// <param name="opening"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// List of <see cref="Point3d"/>s.
        /// <para>
        /// <see langword="null"/> if <paramref name="opening"/> host element is not a <see cref="Wall"/> or it hasn't rectangle boundary.
        /// </para>
        /// </returns>
        public static IEnumerable<Point3d> TryGetRectCorners(this Opening opening, Document activeDoc, IEnumerable<RevitLinkInstance> links)
        {
            if (!opening.IsRectBoundary || opening.Host is not Wall wall)
            { return null; }

            var mainPlanarFaces = wall.GetMainPlanarFaces(activeDoc, links).ToList();
            var basePlane0 = mainPlanarFaces[0].GetPlane().ToRhinoPlane();
            var basePlane1 = mainPlanarFaces[1].GetPlane().ToRhinoPlane();

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

            return new List<Point3d>()
                {
                   basePlane0.ClosestPoint(boundaries[0].ToPoint3d()),
                   basePlane0.ClosestPoint(boundaries[1].ToPoint3d()),
                   basePlane1.ClosestPoint(boundaries[0].ToPoint3d()),
                   basePlane1.ClosestPoint(boundaries[1].ToPoint3d())
                };
        }

        /// <summary>
        /// Try to get <see cref="Solid"/> from <paramref name="opening"/>.
        /// </summary>
        /// <param name="opening"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// <see cref="Solid"/> if operation was successfull.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static Solid TryGetSolid(this Opening opening, Document activeDoc, IEnumerable<RevitLinkInstance> links)
            => opening.TryGetBox(activeDoc, links, out var box) ? box.GetSolid() : null;


        /// <summary>
        /// Try to get <see cref="Solid"/> from <paramref name="opening"/>.
        /// </summary>
        /// <param name="opening"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// <see cref="Solid"/> if operation was successfull.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static Solid TryGetBestSolid(this Opening opening, Document activeDoc, IEnumerable<RevitLinkInstance> links)
        {
            Solid solid = null;
            switch (opening.Host)
            {
                case Wall wall:
                    {
                        solid = new WallOpeningSolidExtractor(wall, activeDoc, links)
                            .GetSolid(opening);
                        break;
                    }
                case Floor floor:
                    {
                        solid = new FloorOpeningSolidExtractor(floor, activeDoc, links)
                            .GetSolid(opening);
                        break;
                    }
                default:
                    break;
            }

            return solid;
        }
    }
}
