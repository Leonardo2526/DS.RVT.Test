using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Basis;
using DS.GraphUtils.Entities;
using DS.RevitApp.Test.Energy;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitApp.Test
{
    internal class MakeBoundTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly List<RevitLinkInstance> _allLoadedLinks;
        private readonly List<Document> _allFilteredDocs;
        private ModelCurve _modelCurve;
        private XYZ _point1;
        private XYZ _point2;

        public MakeBoundTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _allLoadedLinks = _doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
            _allFilteredDocs = new List<Document>() { _doc };
            _allFilteredDocs.AddRange(_allLoadedLinks.Select(l => l.GetLinkDocument()));
        }


        public ITransactionFactory TransactionFactory { get; set; }
        public ILogger Logger { get; set; }


        public MakeBoundTest SelectCurve()
        {
            Reference reference1 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select curve1");
            _modelCurve = _doc.GetElement(reference1) as ModelCurve;



            return _modelCurve != null ? this : null;
        }

        public MakeBoundTest SelectFirstPoint()
        {
            Reference reference = _uiDoc.Selection
                .PickObject(ObjectType.PointOnElement, "Select point1");
            _point1 = reference.GlobalPoint;

            return _point1 != null ? this : null;
        }

        public MakeBoundTest SelectSecondPoint()
        {
            Reference reference = _uiDoc.Selection
                .PickObject(ObjectType.PointOnElement, "Select point2");
            _point2 = reference.GlobalPoint;

            return _point2 != null ? this : null;
        }

        public Curve CreateNewCurve()
        {
            var sourceCurve = _modelCurve.GeometryCurve;

            var plane = _modelCurve.SketchPlane.GetPlane();
            var pNormal = plane.Normal.Negate();
            var sourceRotation = sourceCurve.TryGetRotation(pNormal);

            var curves = sourceCurve.MakeBound(_point1, _point2);
            //var curves = sourceCurve.MakeBound(_point1, _point2);
            //var result = curves.FirstOrDefault();
            if (curves.Count() > 0)
            { DeleteCurve(_modelCurve); }
            foreach (var curve in curves)
            {
                var rotation = curve.TryGetRotation(pNormal);
                Debug.WriteLine(rotation);
                ShowCurve(curve);
            }
            return null;

            var result = curves.FirstOrDefault(c => c.TryGetRotation(pNormal) == sourceRotation);

            if (result != null)
            {
                DeleteCurve(_modelCurve);
                ShowCurve(result);
            }

            return null;
        }


        private void DeleteCurve(ModelCurve mCurve)
          => TransactionFactory.Create(() => _doc.Delete(mCurve.Id), "ShowCurve");

        private void ShowCurve(Curve curve)
        => TransactionFactory.Create(() => curve.Show(_doc), "ShowCurve");

        private void ShowPoint(XYZ point)
       => TransactionFactory.Create(() => point.Show(_doc), "ShowPoint");
    }
}
