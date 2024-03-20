using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.MEP;
using Rhino;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitApp.Test
{
    internal class SolidOperationTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedLinks;
        private readonly DocumentFilter _globalFilter;

        public SolidOperationTest(Document doc, UIDocument uiDoc, IEnumerable<RevitLinkInstance> allLoadedLinks)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _allLoadedLinks = allLoadedLinks;

            //create global filter
            var docs = new List<Document>() { _doc };
            var excludedCategories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_GenericAnnotation,
            BuiltInCategory.OST_TelephoneDevices,
            BuiltInCategory.OST_Materials,
            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_Massing
        };
            _globalFilter = new DocumentFilter(docs, _doc, _allLoadedLinks);
            _globalFilter.QuickFilters =
            [
                (new ElementMulticategoryFilter(excludedCategories, true), null),
                (new ElementIsElementTypeFilter(true), null),
            ];
        }

        public ITransactionFactory TransactionFactory { get; set; }

        public void Run()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var element = _doc.GetElement(reference);
            Solid solid = ElementUtils.GetSolid(element);

            MEPCurve mEPCurve = element is MEPCurve ? element as MEPCurve : null;

            XYZ pickPoint = _uiDoc.Selection.PickPoint(ObjectSnapTypes.Points, "Select startPoint");
            XYZ startPoint = MEPCurveUtils.GetLine(mEPCurve).Project(pickPoint).XYZPoint;
            startPoint.Show(_doc);

            pickPoint = _uiDoc.Selection.PickPoint(ObjectSnapTypes.Points, "Select endPoint");
            XYZ endPoint = MEPCurveUtils.GetLine(mEPCurve).Project(pickPoint).XYZPoint;
            endPoint.Show(_doc);

            //_uiDoc.RefreshActiveView();

            if (mEPCurve is not null)
            {
                double offset = -50.mmToFyt2();
                Solid offsetSolid = mEPCurve.GetOffsetSolid(offset, startPoint, endPoint);
                TransactionFactory.Create(() => offsetSolid.ShowShape(_doc), "show offsetSolid");
            }
            return;
        }

        public void IntersectionTest()
        {
            Reference reference1 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var wall1 = _doc.GetElement(reference1) as Wall;
            Solid solid1 = ElementUtils.GetSolid(wall1);

            Reference reference2 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element2");
            var element2 = _doc.GetElement(reference2);
            Solid solid2 = ElementUtils.GetSolid(element2);



            //var result = BooleanOperationsUtils
            //    .ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);
            var joints = wall1.GetJoints();
            Debug.WriteLine("Joints count is: " + joints.Count());
            AllowJoint(wall1);

            joints = wall1.GetJoints();
            Debug.WriteLine("Joints count is: " + joints.Count());
            DisallowJoint(wall1);
        }


        public void GetAllJoints()
        {
            Reference reference1 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var wall = _doc.GetElement(reference1) as Wall;
            var wallCurve = wall.GetLocationCurve();
            ShowCurve(wallCurve);
            Solid solid = ElementUtils.GetSolid(wall);

            var zoneSolid = GetZoneSolid(wall);
            if (zoneSolid != null)
            { ShowSolid(zoneSolid); }

            var elementItersectionFactory = GetIntersectionFactory();

            elementItersectionFactory.ItemQuickFilters = [];
            var exclusionFilter = new ExclusionFilter(new List<ElementId>() { wall.Id });
            elementItersectionFactory.ItemQuickFilters.Add((exclusionFilter, null));

            var intersections = elementItersectionFactory.GetIntersections(zoneSolid);
            Debug.WriteLine("Intersections found: " + intersections.Count());
            _uiDoc.Selection.SetElementIds(intersections.Select(i => i.Id).ToList());


            var wallIntersections = intersections.OfType<Wall>();
            var intersectionPoints = GetProjPoints(wallCurve, wallIntersections);
            intersectionPoints.ForEach(p => p.Show(_doc, 0, TransactionFactory));
        }

        private IEnumerable<XYZ> GetProjPoints(Curve baseCurve, IEnumerable<Wall> walls)
        {
            var baseOrigin = baseCurve.GetEndPoint(0);

            var points = new List<XYZ>();

            foreach (var wall in walls)
            {
                var curve = wall.GetLocationCurve();
                curve = Project(baseOrigin, curve);
                var p1 = curve.GetEndPoint(0);
                var p2 = curve.GetEndPoint(1);
                int staticIndex = baseCurve.Distance(p1) > baseCurve.Distance(p2) ? 0 : 1;
                curve = curve.Extend(baseCurve, false, staticIndex);         
                if(curve == null) { continue; }
                var intersectionPoint = baseCurve.Distance(curve.GetEndPoint(0)) < RhinoMath.ZeroTolerance ? 
                    curve.GetEndPoint(0) : 
                    curve.GetEndPoint(1);
                points.Add(intersectionPoint);
            }

            return points;

            static Curve Project(XYZ baseOrigin, Curve curve)
            {
                var curveOrigin = curve.GetEndPoint(0);
                var vector = new XYZ(0, 0, curveOrigin.Z - baseOrigin.Z);
                var transform = Transform.CreateTranslation(vector);
                curve = curve.CreateTransformed(transform);
                return curve;
            }
        }

        private SolidElementIntersectionFactory GetIntersectionFactory()
        {
            var localFilter = _globalFilter.Clone();
            var types = new List<Type>()
            {
                typeof(Wall)
            };
            var multiclassFilter = new ElementMulticlassFilter(types);
            localFilter.QuickFilters.Add((multiclassFilter, null));

            return new SolidElementIntersectionFactory(_doc, _allLoadedLinks, localFilter)
            {
                Logger = null,
                TransactionFactory = null
            };
        }

        private Solid GetZoneSolid(Wall wall)
        {
            var offsetDist = 0.001;
            var profile = wall.GetBottomProfile();
            ShowCurves(profile);
            //return null;

            profile = CurveLoop.CreateViaOffset(profile, offsetDist, -XYZ.BasisZ);

            return GeometryCreationUtilities
               .CreateExtrusionGeometry(
               new List<CurveLoop> { profile },
               XYZ.BasisZ, wall.GetHeigth());
        }

        private void AllowJoint(Wall wall)
        {
            var trName = nameof(this.AllowJoint);
            TransactionFactory.Create(() =>
           {
               WallUtils.AllowWallJoinAtEnd(wall, 0);
               WallUtils.AllowWallJoinAtEnd(wall, 1);
               Debug.WriteLine(trName);
           },
               trName);
        }

        private void DisallowJoint(Wall wall)
        {
            var trName = nameof(this.DisallowJoint);
            TransactionFactory.Create(() =>
            {
                WallUtils.DisallowWallJoinAtEnd(wall, 0);
                WallUtils.DisallowWallJoinAtEnd(wall, 1);
                Debug.WriteLine(trName);
            },
               trName);
        }

        private void ShowSolid(Solid solid)
        {
            using (Transaction transaction = new(_doc, "ShowSolid"))
            {
                transaction.Start();
                solid.ShowShape(_doc);

                if (transaction.HasStarted())
                { transaction.Commit(); }
            }

        }

        private void ShowCurve(Curve curve)
            => TransactionFactory.Create(() => curve.Show(_doc), "ShowCurve");

        private void ShowCurves(IEnumerable<Curve> curves)
            => TransactionFactory.Create(() =>
            curves.AsEnumerable<Curve>().ToList()
            .ForEach(c => c.Show(_doc)),
                "ShowCurve");
    }
}
