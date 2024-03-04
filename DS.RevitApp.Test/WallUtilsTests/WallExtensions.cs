using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Basis;
using DS.RevitApp.Test.WallUtilsTests;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Geometry;
using OLMP.RevitAPI.Tools.Lines;
using OLMP.RevitAPI.Tools.Models;
using OLMP.RevitAPI.Tools.Solids;
using OLMP.RevitAPI.Tools.Various.Bases;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Curve = Autodesk.Revit.DB.Curve;
using Line = Autodesk.Revit.DB.Line;

namespace DS.RevitApp.Test.WallUtilsTests
{
    /// <summary>
    /// Extension methods for <see cref="Wall"/>.
    /// </summary>
    public static class WallExtensionsTest
    {
        /// <summary>
        /// Get <paramref name="wall"/> <see cref="Autodesk.Revit.DB.Face"/>s and inserts <see cref="Autodesk.Revit.DB.Face"/>s
        /// if <paramref name="includeInserts"/> is <see langword="true"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="activeDoc"></param>
        /// <param name="geomOptions"></param>
        /// <param name="includeInserts"></param>
        /// <returns></returns>
        public static (List<Face> wallFaces, Dictionary<ElementId, List<Face>> insertsFacesCollection) GetFaces(
           this Wall wall, Document activeDoc, IEnumerable<RevitLinkInstance> links, Options geomOptions = null, bool includeInserts = false)
        {
            var insertsFaces = new Dictionary<ElementId, List<Face>>();
            var faceList = new List<Face>();

            List<Solid> solidList = wall.Document.IsLinked ?
                wall.GetTransformedSolids(wall.TryFindLink(links)) :
                SolidExtractor.GetSolids(wall, null, geomOptions);

            var insertsIds = includeInserts ? wall.FindInserts(true, false, true, true) : null;

            foreach (Solid solid in solidList)
            {
                foreach (Face face in solid.Faces)
                {
                    if (includeInserts)
                    {
                        if (!tryAddInserts(wall, insertsFaces, insertsIds, face))
                        { faceList.Add(face); }
                    }
                    else
                    { faceList.Add(face); }
                }
            }
            return (faceList, insertsFaces);

            static bool tryAddInserts(Wall wall,
                Dictionary<ElementId, List<Face>> insertsFaces, IList<ElementId> insertsIds,
                Face face)
            {
                var inserted = false;
                var genIds = wall.GetGeneratingElementIds(face);
                foreach (var gId in genIds)
                {
                    if (insertsIds.Contains(gId))
                    {
                        //add to dict
                        if (insertsFaces.TryGetValue(gId, out var valueFaces))
                        { valueFaces.Add(face); }
                        else
                        { insertsFaces.Add(gId, new List<Face>() { face }); }
                        inserted = true;
                    }
                }
                return inserted;
            }
        }

        /// <summary>
        /// Get <see cref="PlanarFace"/>s of the <paramref name="wall"/> that represent main vertical <paramref name="wall"/> faces
        /// with normals parallel to <paramref name="wall"/> Y basis.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// List of <see cref="PlanarFace"/>s.
        /// <para>
        /// Empty list if no <see cref="PlanarFace"/>s were found.
        /// </para>
        /// </returns>
        public static IEnumerable<PlanarFace> GetMainPlanarFaces(this Wall wall, Document activeDoc, IEnumerable<RevitLinkInstance> links)
        {
            return GeometryElementsUtils.GetFaces(wall, activeDoc, links).
                OfType<PlanarFace>().ToList().
                FindAll(FaceFilter.YNormal(wall, activeDoc, links));
        }

        /// <summary>
        /// Get all walls joint to <paramref name="wall"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="onlyEnd"></param>
        /// <returns>
        /// List of all walls joint to <paramref name="wall"/>.
        /// <para>
        /// Empty list if no jounts was found.
        /// </para>
        /// </returns>
        public static IEnumerable<ElementId> GetJoints(this Wall wall, bool onlyEnd = false)
        {
            var result = new List<ElementId>();
            var locationCurve = wall.Location as LocationCurve;
            int i = 0;
            int emptyResults = 0;
            while (emptyResults < 3)
            {
                //break;
                IEnumerable<ElementId> joints;
                try
                { joints = GetAdjoiningElements(locationCurve, wall.Id, i); }
                catch (Exception)
                { break; }
                if (joints is not null && joints.Count() > 0)
                { result.AddRange(joints); }
                else { emptyResults++; }
                i++;
                if (onlyEnd && i == 2) { break; }
            }
            return result;

            static List<ElementId> GetAdjoiningElements(
                LocationCurve locationCurve,
                ElementId wallId,
                int index)
            {
                var result = new List<ElementId>();
                ElementArray a = locationCurve.get_ElementsAtJoin(index);
                foreach (Element element in a)
                    if (element.Id != wallId)
                        result.Add(element.Id);
                return result;
            }
        }

        public static Line GetHeightLine(this Wall wall, Document activeDoc, IEnumerable<RevitLinkInstance> links)
        {
            PlanarFace planarFace = wall.GetMainPlanarFaces(activeDoc, links).FirstOrDefault();
            var curveLoops = planarFace.GetEdgesAsCurveLoops();

            var at = 3.DegToRad();
            foreach (var loop in curveLoops)
            {
                foreach (var item in loop)
                {
                    var line = item as Line;
                    if (line != null)
                    {
                        var rLine = line.ToRhinoLine();
                        if (rLine.Direction.IsParallelTo(Vector3d.ZAxis, at) != 0)
                        { return line; }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Try get <see cref="Line"/> from <see cref="LocationCurve"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="activeDoc"></param>
        /// <param name="line"></param>
        /// <returns>
        /// <see cref="Line"/> if <paramref name="wall"/> was created from <see cref="PlanarFace"/>s.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static Curve GetLocationCurve(this Wall wall, IEnumerable<RevitLinkInstance> links = null)
        {
            var wallLocCurve = wall.Location as LocationCurve;
            var wallCurve = wallLocCurve.Curve;

            if (wall.Document.IsLinked)
            {
                var linkTransform = wall.TryGetTransform(links);
                if (linkTransform != null)
                { wallCurve = wallCurve.CreateTransformed(linkTransform); }
            }

            return wallCurve;
        }


        public static IEnumerable<Face> GetFaces(this Wall wall,
            Document activeDoc, IEnumerable<RevitLinkInstance> links)
            => GeometryElementsUtils.GetFaces(wall, activeDoc, links);

        /// <summary>
        /// Get the <see cref="Basis3d"/> of the <paramref name="wall"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="parameter"></param>
        /// <param name="links"></param>
        /// <remarks>
        /// Auto transformation if <paramref name="wall"/> has a transform of <see cref="RevitLinkInstance"/>.
        /// </remarks>
        /// <returns>
        /// The basis containing the origin as the point at <paramref name="parameter"/> on the curve, 
        /// BasisX as the tangent vector at origin, 
        /// BasisZ as <see cref="Autodesk.Revit.DB.XYZ.BasisZ"/>, 
        /// BasisY as BasisZ and BasisX cross product.
        /// </returns>
        public static Basis3d GetBasis(this Wall wall, double parameter = 0, IEnumerable<RevitLinkInstance> links = null)
        {
            var wallCurve = wall.GetLocationCurve(links);
            parameter = parameter == 0 ? wallCurve.GetEndParameter(0) : parameter;
            var transform = wallCurve.ComputeDerivatives(parameter, true);

            var x = transform.BasisX.Normalize();
            var y = transform.BasisY.Normalize();
            var z = XYZ.BasisZ;
            if (y.IsZeroLength())
            { y = z.CrossProduct(x); }          

            return new Basis3d(transform.Origin.ToPoint3d(), x.ToVector3d(), y.ToVector3d(), z.ToVector3d());
        }

        /// <summary>
        /// Get edges from all <paramref name="wall"/>'s <see cref="Autodesk.Revit.DB.Face"/>s.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="activeDoc"></param>
        /// <param name="geomOptions"></param>
        /// <returns>
        /// List of <paramref name="wall"/>'s edges.
        /// <para>
        /// Empty list if no edges occured.
        /// </para>
        /// </returns>
        public static IEnumerable<Curve> GetEdges(
            this Wall wall,
            Document activeDoc,
            IEnumerable<RevitLinkInstance> links,
            Options geomOptions = null)
        {
            var curves = new List<Curve>();
            var (wallFaces, insertsFacesCollection) = wall.GetFaces(activeDoc, links, geomOptions);
            wallFaces.ForEach(f => curves.AddRange(f.GetEdges()));
            return curves;
        }

        /// <summary>
        /// Check if the <paramref name="wall"/> can be traversed of <paramref name="traverseDirection"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="traverseDirection"></param>
        /// <param name="activeDoc"></param>
        /// <param name="links"></param>
        /// <param name="angleTolerance"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="wall"/>'s Y face normal is parallel to
        /// <paramref name="traverseDirection"/> with <paramref name="angleTolerance"/>.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool IsTraversable(
            this Wall wall,
            Vector3d traverseDirection,
            Document activeDoc,
            IEnumerable<RevitLinkInstance> links,
            double angleTolerance = RhinoMath.DefaultAngleTolerance)
        {
            if (!wall.TryGetBasis(activeDoc, links, out var basis))
            { return false; }

            return traverseDirection.IsParallelTo(basis.Y, angleTolerance) != 0;
        }

    }

}
