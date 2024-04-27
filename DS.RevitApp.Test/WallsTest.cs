using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Various;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitApp.Test.WallUtilsTests;
using OLMP.RevitAPI.Tools.Connections.PointModels;
using OLMP.RevitAPI.Tools.Various.Selections;
using OLMP.RevitAPI.Tools.Geometry.Points;
using DS.ClassLib.VarUtils.Basis;
using DS.RevitApp.Test.WallUtilsTests;
using MoreLinq;

namespace DS.RevitApp.Test
{
    internal class WallsTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly List<RevitLinkInstance> _allLoadedLinks;
        private readonly List<Document> _allFilteredDocs;
        private readonly DocumentFilter _globalFilter;
        private readonly ContextTransactionFactory _trb;
        private readonly XYZVisualizator _viz;

        //create global filter
        private readonly List<BuiltInCategory> _excludedCategories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_GenericAnnotation,
            BuiltInCategory.OST_TelephoneDevices,
            BuiltInCategory.OST_Materials,
            BuiltInCategory.OST_Massing
        };
        private static double _feetsToMeters = Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Feet, Rhino.UnitSystem.Meters);
        private static double _squareFeetsToMeters = Math.Pow(_feetsToMeters, 2);
        private Wall _wall;

        public WallsTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _allLoadedLinks = _doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
            _allFilteredDocs = new List<Document>() { _doc };
            _allFilteredDocs.AddRange(_allLoadedLinks.Select(l => l.GetLinkDocument()));
            _globalFilter = new DocumentFilter(_allFilteredDocs, _doc, _allLoadedLinks);
            _globalFilter.QuickFilters =
            [
                (new ElementMulticategoryFilter(_excludedCategories, true), null),
                (new ElementIsElementTypeFilter(true), null),
            ];
            _trb = new ContextTransactionFactory(_doc);
            _viz = new XYZVisualizator(uiDoc, 0, _trb);
        }


        public WallsTest SelectWall()
        {
            var el = new ElementSelector(_uiDoc) { AllowLink = true }.Pick("Выберите стену.");
            _wall = el as Wall;
            return _wall is null ? null : this;
        }

        public void Run()
        {
            var selector = new ElementSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите элемент");

            var paramName = "OLP_БезПересечений";
            var oLPNoIntersection = element.GetParameters(paramName).FirstOrDefault();
            Debug.WriteLine(element.Id);
            Debug.WriteLine(oLPNoIntersection.Definition.Name);
            Debug.WriteLine(oLPNoIntersection.AsInteger());
            Debug.WriteLine(oLPNoIntersection.AsValueString());

            var line = element.GetCenterLine();
            Debug.WriteLine(line.Direction);
        }

        public void GetWallOpenings()
        {
            var solid = _wall.Solid(_allLoadedLinks);
            var wallInseretsIds = _wall.FindInserts(true, false, false, false) ?? new List<ElementId>();
            var inserts = wallInseretsIds.Select(i => _doc.GetElement(i)).ToList();
            var openings = inserts.OfType<Opening>();
        }


        public Basis3d GetBasis(bool autoBasis = true)
        {
            var wallCurve = _wall.GetLocationCurve();
            double parameter = autoBasis ?
                wallCurve.GetEndParameter(0) : SetParameter(wallCurve);
            parameter = wallCurve.ComputeNormalizedParameter(parameter);
            Show(parameter);
            //return;

            var basis = _wall.GetBasis(parameter, _allLoadedLinks);
            _viz.Show(basis);
            return basis;

            double SetParameter(Curve wallCurve)
            {
                var p = _uiDoc.Selection
                    .PickObject(ObjectType.PointOnElement).GlobalPoint;
                var projection = wallCurve.Project(p);
                return projection.Parameter;
            }

            void Show(double parameter)
            {
                var origin = wallCurve.Evaluate(parameter, true);
                origin.Show(_doc, 0, _trb);
            }
        }

        public void GetSortedFaces()
        {
            //var wallCurve = _wall.GetLocationCurve(_allLoadedLinks);
            //var center = GetCenter(wallCurve);
            //center.Show(_doc, 0, _trb);
            //return;
             var mYfaces =  _wall.GetMainYFaces(_allLoadedLinks);
            if (mYfaces.Item1 != null && mYfaces.Item2 != null)
            { ShowFaces(new List<Face>() { mYfaces.Item1, mYfaces.Item2 }); }
            return;

            var faces = GeometryElementsUtils.GetFaces(_wall, _allLoadedLinks).ToList();
            var xFaces = faces.FindAll(FaceFilter.XNormal(_wall, _allLoadedLinks)).ToList();
            //ShowFaces(xFaces);
            var yFaces = faces.FindAll(FaceFilter.YNormal(_wall, _allLoadedLinks)).ToList();
            //ShowFaces(yFaces);
            var zFaces = faces.FindAll(FaceFilter.ZNormal()).ToList();
            //ShowFaces(zFaces);


            //_trb.Create(() => 
            //xFaces.ForEach(f => FaceExtensionsTest.GetSolid(f).ShowShape(_doc)), 
            //"ShowFaces");

        }
        private void ShowFaces(IEnumerable<Face> faces)
             => _trb.Create(() => faces.ForEach(f => f.ShowEdges(_doc)), "ShowFaces");

        private XYZ GetCenter(Curve wallCurve)
        {
            var centerParameter = GetCenterParameter(wallCurve);
            return wallCurve.Evaluate(centerParameter, true);
        }


        private double GetCenterParameter(Curve wallCurve)
        {
            var p1 = wallCurve.GetEndParameter(0);
            p1 = wallCurve.ComputeNormalizedParameter(p1);
            var p2 = wallCurve.GetEndParameter(1);
            p2 = wallCurve.ComputeNormalizedParameter(p2);

            return (p1 + p2) / 2;
        }
    }
}
