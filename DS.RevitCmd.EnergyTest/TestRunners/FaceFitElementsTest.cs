using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.RevitApp.Test;
using DS.RevitCmd.EnergyTest.SpaceBoundary;
using DS.RhinoInside.Revit.Convert.Geometry;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Geometry.Points;
using OLMP.RevitAPI.Tools.Visualisators;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DS.RevitApp.Test.FaceTests;
using System.Diagnostics;

namespace DS.RevitCmd.EnergyTest.TestRunners
{
    internal class FaceFitElementsTest : ISerilogged
    {
        private readonly UIDocument _uiDoc;
        private readonly DocumentFilter _globalFilter;
        private readonly Document _doc;
        private readonly XYZVisualizator visualizator;

        public FaceFitElementsTest(UIDocument uiDoc, DocumentFilter globalFilter)
        {
            _uiDoc = uiDoc;
            _globalFilter = globalFilter;
            _doc = _uiDoc.Document;
            visualizator = new XYZVisualizator(uiDoc);
        }

        private Wall _baseWall { get; set; }

        public FaceFitElementsTest SelectWall()
        {
            Reference reference = _uiDoc.Selection
                      .PickObject(ObjectType.Element, $"Select wall");
            _baseWall = _doc.GetElement(reference) as Wall;
            return this;
        }

        public ITransactionFactory TransactionFactory { get; set; }
        public ILogger Logger { get; set; }

        public void GetElements()
        {
            //var yFaces = _baseWall
            //   .GetFaces(null).ToList()
            //   .FindAll(
            //    FaceFilter.YNormal(_baseWall, null)
            //    //FaceFilter.XNormal(_baseWall, null)
            //    //FaceFilter.ZNormal()
            //    );
            var extSide1 = HostObjectUtils.GetSideFaces(_baseWall, ShellLayerType.Exterior);
            var yFaces = GetFaces(_baseWall);
            var baseFace = yFaces.ElementAt(0);
            var normal = GetNormal(baseFace);
            var centroid = ComputeCentroid(baseFace);
            visualizator.ShowVectorByDirection(centroid, normal);
            centroid.Show(_doc, 0, TransactionFactory);
            //return;
            //double solidWidth = 0.02;
            //var faceSolid = baseFace.CreateSolid(solidWidth);
            //ShowSolid(faceSolid);
            //return;
            //ShowFace(yFace);
            var wallsolid = _baseWall.GetFullSolid();

            var adjacancies = _baseWall.GetJoints();
            var solidItersectionFactory = GetIntersectionFactory(_globalFilter, null, adjacancies);
            var wallIntersectionFactory = new WallIntersectionFactory(_doc, solidItersectionFactory)
            { Logger = Logger };
            var intersections = wallIntersectionFactory.GetIntersections(_baseWall);
            var intersectionElements = intersections.Select(id => _doc.GetElement(id));
            var interWalls = intersectionElements.OfType<Wall>();

            var interFaces = new List<(ElementId, Face)>();
            foreach (var wall in interWalls)
            {
                var wallFaces = GetFaces(wall);
                var fittedFaces = yFaces.ElementAt(0).GetFittedFaces(wallFaces);
                //var fittedFace = GetFittedFace(yFaces.ElementAt(0), wall);
                if (fittedFaces.Count() == 1)
                { interFaces.Add((wall.Id, fittedFaces.ElementAt(0))); }
                fittedFaces = yFaces.ElementAt(1).GetFittedFaces(wallFaces);
                if (fittedFaces != null)
                { interFaces.Add((wall.Id, fittedFaces.ElementAt(0))); }
            }

            Logger?.Warning("Found interElements: " + interFaces.Count);
            interFaces.ForEach(e => Debug.WriteLine(e.Item1));

            var fitFace = interFaces.FirstOrDefault().Item2;
            if (fitFace != null)
            { ShowFace(fitFace); }
            else
            { Logger?.Warning("Failed to get fit face."); }
        }

        private void ShowFace(Face yFace)
        {
            TransactionFactory.Create(() => yFace.ShowEdges(_doc), "showFace");
        }

        private void ShowSolid(Solid solid)
        {
            TransactionFactory.Create(() => solid.ShowShape(_doc), "showFace");
        }

        private IEnumerable<Face> GetFaces(Wall wall, ShellLayerType shellLayerType)
        {
            var faces = new List<Face>();
            var extSide = HostObjectUtils.GetSideFaces(wall, shellLayerType);
            foreach (var r in extSide)
            {
                var extface = wall.GetGeometryObjectFromReference(r) as Face;
                faces.Add(extface);
            }
            return faces;
        }

        private IEnumerable<Face> GetFaces(Wall wall)
        {
            var faces = new List<Face>();
            var references = new List<Reference>();
            var extSide1 = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Interior);
            references.AddRange(extSide1);
            var extSide2 = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Exterior);
            references.AddRange(extSide2);
            foreach (var r in references)
            {
                var extface = wall.GetGeometryObjectFromReference(r) as Face;
                faces.Add(extface);
            }
            return faces;
        }

        private Face GetFittedFace(Face faceToFit, Wall targetWall)
        {
            var solidWidth = 0.01;
            var at = 1.DegToRad();
            var fitFaceSolid = faceToFit.CreateSolid(solidWidth);
            var yFaces = GetFaces(targetWall);
            //yFaces.ForEach(f => ShowFace(f));
            foreach (var yFace in yFaces)
            {
                var targetSolid = yFace.CreateSolid(solidWidth);
                var result = BooleanOperationsUtils
                    .ExecuteBooleanOperation(fitFaceSolid, targetSolid, BooleanOperationsType.Intersect);
                if (result == null || result.Volume == 0) { continue; }
                var maxArea = result.Faces.ToList().OrderBy(f => f.Area).First();
                var centroid = result.ComputeCentroid();

                var proj1 = faceToFit.Project(centroid);
                var proj2 = yFace.Project(centroid);
                if (proj1 == null || proj2 == null) continue;
                var n1 = faceToFit.ComputeNormal(proj1.UVPoint).ToVector3d();
                var n2 = yFace.ComputeNormal(proj2.UVPoint).ToVector3d();
                if (n1.IsParallelTo(n2, at) != 0)
                { return yFace; }

                //var d1 = faceToFit.ComputeDerivatives(proj1.UVPoint);
                //var d2 = yFace.ComputeDerivatives(proj2.UVPoint);

            }

            return null;
        }

        SolidElementIntersectionFactory GetIntersectionFactory(DocumentFilter globalFilter,
            IEnumerable<ElementId> elementIdsSet = null,
            IEnumerable<ElementId> excludedIds = null)
        {
            var localFilter = globalFilter.Clone();
            if (elementIdsSet != null)
            { localFilter.ElementIdsSet = elementIdsSet.ToList(); }
            var types = new List<Type>()
            {
                typeof(Wall)
            };
            var multiclassFilter = new ElementMulticlassFilter(types);
            localFilter.QuickFilters.Add((multiclassFilter, null));
            if (excludedIds != null && excludedIds.Count() > 0)
            { localFilter.QuickFilters.Add((new ExclusionFilter(excludedIds.ToList()), null)); }


            return new SolidElementIntersectionFactory(_doc, null, localFilter)
            {
                Logger = null,
                TransactionFactory = null
            };
        }

        private XYZ GetNormal(Face face)
        {
            var planar = face as PlanarFace;
            if (planar != null)
            { return planar.FaceNormal; }

            var centroid = ComputeCentroid(face);
            var proj = face.Project(centroid);
            return proj == null ?
                null :
                face.ComputeNormal(proj.UVPoint).Normalize();
        }

        private XYZ ComputeCentroid(Face face)
        {
            var points = TesselateEdges(face);
            var average = XYZUtils.GetAverage(points);

            var proj = face.Project(average);

            return proj?.XYZPoint ?? average;
        }

        private IEnumerable<XYZ> TesselateEdges(Face face)
        {
            var points = new List<XYZ>();
            var loop = face.GetOuterLoop();
            if (loop.IsOpen()) { throw new Exception("Loop is not closed!"); }
            loop.ForEach(c => points.AddRange(c.Tessellate()));
            return points;
        }

    }
}
