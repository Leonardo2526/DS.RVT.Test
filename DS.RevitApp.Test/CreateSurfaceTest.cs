using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools;
using DS.ClassLib.VarUtils;
using Serilog;
using MoreLinq;
using Autodesk.Revit.DB.Analysis;
using OLMP.RevitAPI.Tools.Extensions.RhinoExtensions;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Autodesk.Revit.DB.Mechanical;
using OLMP.RevitAPI.Tools.Openings;
using System.Diagnostics;
using System.Xml.Linq;
using OLMP.RevitAPI.Tools.Solids;
using Autodesk.Revit.UI.Selection;
using Rhino.UI;

namespace DS.RevitApp.Test
{
    internal class CreateSurfaceTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly List<RevitLinkInstance> _allLoadedLinks;
        private readonly List<Document> _allFilteredDocs;
        private readonly DocumentFilter _globalFilter;
        private readonly ContextTransactionFactory _trb;
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


        public CreateSurfaceTest(UIDocument uiDoc)
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
        }

        public ILogger Logger { get; set; }


        private Form CreateLoftForm(Autodesk.Revit.DB.Document document)
        {
            Form loftForm = null;

            ReferencePointArray rpa = new ReferencePointArray();
            ReferenceArrayArray ref_ar_ar = new ReferenceArrayArray();
            ReferenceArray ref_ar = new ReferenceArray();
            ReferencePoint rp = null;
            XYZ xyz = null;

            // make first profile curve for loft          
            xyz = document.Application.Create.NewXYZ(0, 0, 0);
            rp = document.FamilyCreate.NewReferencePoint(xyz);
            rpa.Append(rp);

            xyz = document.Application.Create.NewXYZ(0, 50, 10);
            rp = document.FamilyCreate.NewReferencePoint(xyz);
            rpa.Append(rp);

            xyz = document.Application.Create.NewXYZ(0, 100, 0);
            rp = document.FamilyCreate.NewReferencePoint(xyz);
            rpa.Append(rp);

            CurveByPoints cbp = document.FamilyCreate.NewCurveByPoints(rpa);
            ref_ar.Append(cbp.GeometryCurve.Reference);
            ref_ar_ar.Append(ref_ar);
            rpa.Clear();
            ref_ar = new ReferenceArray();

            // make second profile curve for loft
            xyz = document.Application.Create.NewXYZ(50, 0, 0);
            rp = document.FamilyCreate.NewReferencePoint(xyz);
            rpa.Append(rp);

            xyz = document.Application.Create.NewXYZ(50, 50, 30);
            rp = document.FamilyCreate.NewReferencePoint(xyz);
            rpa.Append(rp);

            xyz = document.Application.Create.NewXYZ(50, 100, 0);
            rp = document.FamilyCreate.NewReferencePoint(xyz);
            rpa.Append(rp);

            cbp = document.FamilyCreate.NewCurveByPoints(rpa);
            ref_ar.Append(cbp.GeometryCurve.Reference);
            ref_ar_ar.Append(ref_ar);
            rpa.Clear();
            ref_ar = new ReferenceArray();

            // make third profile curve for loft
            xyz = document.Application.Create.NewXYZ(75, 0, 0);
            rp = document.FamilyCreate.NewReferencePoint(xyz);
            rpa.Append(rp);

            xyz = document.Application.Create.NewXYZ(75, 50, 5);
            rp = document.FamilyCreate.NewReferencePoint(xyz);
            rpa.Append(rp);

            xyz = document.Application.Create.NewXYZ(75, 100, 0);
            rp = document.FamilyCreate.NewReferencePoint(xyz);
            rpa.Append(rp);

            cbp = document.FamilyCreate.NewCurveByPoints(rpa);
            ref_ar.Append(cbp.GeometryCurve.Reference);
            ref_ar_ar.Append(ref_ar);

            loftForm = document.FamilyCreate.NewLoftForm(true, ref_ar_ar);

            return loftForm;
        }

        public void Run1()
        {
            Form form;
            using (Transaction transaction = new(_doc, "test"))
            {
                transaction.Start();
                form = CreateLoftForm(_doc);


                if (transaction.HasStarted())
                {
                    transaction.Commit();
                }
            }
            var ids = form.GetMaterialIds(false);
            var a = form.GetMaterialArea(ids.First(), false);
        }

        public void Run()
        => _trb.CreateAsync(() => CreateLoftForm(_doc), "");

        public void SelectForm()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var form = _doc.GetElement(reference) as Form;

            var ids = form.GetMaterialIds(false);
            var a = form.GetMaterialArea(ids.First(), false) * _squareFeetsToMeters;

            var geom = form.get_Geometry(new Options());
        }

        public void SelectMass()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var mass = _doc.GetElement(reference) as FamilyInstance;
           
        }


        public void CreateShape()
        {
          
            var line1 = Line.CreateBound(new XYZ(), new XYZ(0,5,0));
            var line2 = Line.CreateBound(new XYZ(0, 5, 0), new XYZ(5, 5, 0));
            var line3 = Line.CreateBound(new XYZ(5, 5, 0), new XYZ(5, 0, 0));
            var line4 = Line.CreateBound(new XYZ(5, 0, 0), new XYZ());

            var curves = new List<Curve>()
            {
                line1, line2, line3, line4
            };

            var loop = CurveLoop.Create(curves);

            var coords = new List<XYZ>()
            {
                new XYZ(),
                new XYZ(0,5,0),
                new XYZ(5,5,0),
                new XYZ(5,0,0),
                new XYZ()
            };
            var solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>() { loop }, XYZ.BasisZ, Math.Pow(0.1, 7));
            var face = solid.Faces.GetFace(new XYZ());

            var polyline = PolyLine.Create(coords);

            List<GeometryObject> geomObjects = new List<GeometryObject>()
            {
                solid
            };

            DirectShape ds;
            using (Transaction transaction = new(_doc, "test"))
            {
                transaction.Start();               
                ds = DirectShape.CreateElement(_doc, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(geomObjects);


                if (transaction.HasStarted())
                {
                    transaction.Commit();
                }
            }

        }
    }
}
