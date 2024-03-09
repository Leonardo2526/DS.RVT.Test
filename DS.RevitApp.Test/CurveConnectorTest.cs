using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitApp.Test.Energy;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitApp.Test
{
    internal class CurveConnectorTest : ISerilogged
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly List<RevitLinkInstance> _allLoadedLinks;
        private readonly List<Document> _allFilteredDocs;

        public CurveConnectorTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _allLoadedLinks = _doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
            _allFilteredDocs = new List<Document>() { _doc };
            _allFilteredDocs.AddRange(_allLoadedLinks.Select(l => l.GetLinkDocument()));
        }


        public ITransactionFactory TransactionFactory { get; set; }
        public ILogger Logger { get; set; }

        public (Wall, Wall) SelectTWoWalls()
        {
            Reference reference1 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select wall1");
            var w1 = _doc.GetElement(reference1) as Wall;
            Reference reference2 = _uiDoc.Selection
                .PickObject(ObjectType.Element, "Select wall2");
            var w2 = _doc.GetElement(reference2) as Wall;
            return (w1, w2);
        }


        public void ConnectTwoWallCurves(Wall wall1, Wall wall2)
        {
            var curve1 = wall1.GetLocationCurve(_allLoadedLinks);
            //ShowCurve(curve1);
            var curve2 = wall2.GetLocationCurve(_allLoadedLinks);
            //ShowCurve(curve2);
            //_uiDoc.RefreshActiveView();

            curve1 = curve1.CreateReversed();
            CurveExtensionsTest.TransactionFactory = TransactionFactory;
            var resulstCurves = curve1.Extend(curve2, true);
            //var resulstCurves = curve1.Trim(curve2, true);
            //var resultCurve = resulstCurves.LastOrDefault();
            var resultCurve = resulstCurves.FirstOrDefault();
            if (resultCurve != null)
            {
                ShowCurve(resultCurve);
                var resultPoint = resultCurve.GetEndPoint(1);
                ShowPoint(resultPoint);
            }
        }

        private void ShowCurve(Curve curve)
        => TransactionFactory.Create(() => curve.Show(_doc), "ShowCurve");

        private void ShowPoint(XYZ point)
       => TransactionFactory.Create(() => point.Show(_doc), "ShowPoint");
    }
}
