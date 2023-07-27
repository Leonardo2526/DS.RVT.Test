using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Bases;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.PathCreators;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Various;
using DS.RevitLib.Utils.Various.Selections;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitApp.Test
{
    internal class AStarAlgorithmCDFTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ITransactionFactory _trb;
        private MEPCurve _baseMEPCurve;

        public AStarAlgorithmCDFTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = _uiDoc.Document;
            _trb = new ContextTransactionFactory(_doc);
            var path = Run();

            if (path != null && path.Count != 0)
            {

                ShowLines(path);
                //ShowMEPCurves(path, _baseMEPCurve);
            }
        }

        private List<XYZ> Run()
        {
            var (startConnectionPoint, endConnectionPoint) = GetEdgePoints();
            //var (startConnectionPoint, endConnectionPoint) = GetPointsByConnectors();
            if (startConnectionPoint is null || endConnectionPoint is null) { return null; }

            _baseMEPCurve = startConnectionPoint.Element as MEPCurve;

            var basisStrategy = new TwoMEPCurvesBasisStrategy(_uiDoc);
            var traceSettings = GetTraceSettings(_baseMEPCurve);
            var planes = new List<PlaneType>()
            {
                //PlaneType.XY,
                //PlaneType.XZ,
                //PlaneType.YZ,
            };

            var (docElements, linkElementsDict) = new ElementsExtractor(_doc).GetAll();
            var objectsToExclude = new List<Element>() { startConnectionPoint.Element };
            if (startConnectionPoint.Element.Id != endConnectionPoint.Element.Id)
            { objectsToExclude.Add(endConnectionPoint.Element); }

            var startMEPCurve = startConnectionPoint.Element as MEPCurve;
            var endMEPCurve = endConnectionPoint.Element as MEPCurve;

            var pathFindFactory = new xYZPathFinder(_uiDoc, basisStrategy, traceSettings, docElements, linkElementsDict);
            pathFindFactory.Build(startMEPCurve, endMEPCurve, objectsToExclude, true, planes);

            return pathFindFactory.FindPath(startConnectionPoint.Point, endConnectionPoint.Point);
        }

        private (ConnectionPoint startPoint, ConnectionPoint endPoint) GetEdgePoints()
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = false };

            var element = selector.Pick($"Укажите точку присоединения 1 на элементе.");
            if(element == null) return (null, null);
            ConnectionPoint connectionPoint1 = new ConnectionPoint(element, selector.Point);
            if (connectionPoint1.IsValid)
            {
                element = selector.Pick($"Укажите точку присоединения 2 на элементе.");
                if (element == null) return (null, null);
                ConnectionPoint connectionPoint2 = new ConnectionPoint(element, selector.Point);
                return (connectionPoint1, connectionPoint2);
            }

            return (null, null);
        }

        private (ConnectionPoint startPoint, ConnectionPoint endPoint) GetPointsByConnectors()
        {
            var mEPCurve = new MEPCurveSelector(_uiDoc) { AllowLink = false }.Pick("Выберите элемент для получения точек нахождения пути.");
            ElementUtils.GetPoints(mEPCurve, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);

            ConnectionPoint connectionPoint1 = new ConnectionPoint(mEPCurve, startPoint);
            ConnectionPoint connectionPoint2 = new ConnectionPoint(mEPCurve, endPoint);

            return (connectionPoint1, connectionPoint2);
        }

        private ITraceSettings GetTraceSettings(MEPCurve mEPCurve)
        {
            ITraceSettings traceSettings = new TraceSettings()
            {
                B = 100.MMToFeet(),
                AList = new List<int>() {30}
            };
            var solidModel = new SolidModel(mEPCurve.Solid());
            var mEPCurveModel = new MEPCurveModel(mEPCurve, solidModel);
            var maxAngle = traceSettings.AList.Max();
            var radius = new ElbowRadiusCalc(mEPCurveModel).GetRadius(maxAngle.DegToRad()).Result;
            traceSettings.F = 2 * radius + traceSettings.D;

            return traceSettings;
        }

        private void ShowLines(List<XYZ> path)
        {
            _trb.CreateAsync(() =>
            {
                var mcreator = new ModelCurveCreator(_doc);
                for (int i = 0; i < path.Count - 1; i++)
                { mcreator.Create(path[i], path[i + 1]); }
            }, "ShowSolution");
        }

        private void ShowMEPCurves(List<XYZ> path, MEPCurve baseMEPCurve)
        {
            var builder = new BuilderByPoints(baseMEPCurve, path);
            var mEPElements = builder.BuildSystem(new TransactionBuilder(_doc));
            return;
            _trb.CreateAsync(() => _doc.Delete(baseMEPCurve.Id), "delete baseMEPCurve");
        }
    }
}
