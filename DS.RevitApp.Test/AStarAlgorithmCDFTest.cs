using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Bases;
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Connections.PointModels.PointModels;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Points;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.PathCreators;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Various;
using DS.RevitLib.Utils.Various.Bases;
using DS.RevitLib.Utils.Various.Selections;
using DS.RevitLib.Utils.Visualisators;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using DS.RevitLib.Utils.Various.Bases;
using System.Threading;

namespace DS.RevitApp.Test
{
    internal class AStarAlgorithmCDFTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ITransactionFactory _trb;
        private MEPCurve _baseMEPCurve;
        List<BuiltInCategory> _exludedCathegories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctFittingInsulation,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_DuctCurvesInsulation,
                BuiltInCategory.OST_PipeFittingInsulation,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PipeCurvesInsulation,
                BuiltInCategory.OST_TelephoneDevices,
                BuiltInCategory.OST_Materials,
                BuiltInCategory.OST_Rooms
            };

        public AStarAlgorithmCDFTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = _uiDoc.Document;
            _trb = new ContextTransactionFactory(_doc);

            Log.Logger = new LoggerConfiguration()
                  .WriteTo.Debug()
                  .CreateLogger();

            var path = Run();

            if (path != null && path.Count != 0)
            {

                ShowLines(path);
                //ShowMEPCurves(path, _baseMEPCurve);
            }
        }

        private List<XYZ> Run()
        {
            var startConnectionPoint =GetPoint(1);
            var endConnectionPoint = GetPoint(2);
            //var (startConnectionPoint, endConnectionPoint) = GetEdgePoints();
            //var (startConnectionPoint, endConnectionPoint) = GetPointsByConnectors();
            if (startConnectionPoint is null || endConnectionPoint is null) { return null; }

            //_baseMEPCurve = startConnectionPoint.GetMEPCurve();

            var basisStrategy = new TwoMEPCurvesBasisStrategy();
            basisStrategy.Build(_uiDoc).GetBasis();
            _baseMEPCurve = basisStrategy.MEPCurve1;
            var traceSettings = GetTraceSettings(_baseMEPCurve);
            var planes = new List<PlaneType>()
            {
                //PlaneType.XY,
                //PlaneType.XZ,
                //PlaneType.YZ,
            };

            //var outline = GetOutline(startConnectionPoint.Point, endConnectionPoint.Point);
            //var outline = GetOutline();
            //outline.MinimumPoint.Show(_doc);
            //_uiDoc.RefreshActiveView();
            //outline.MaximumPoint.Show(_doc);
            //_uiDoc.RefreshActiveView();

            //var bb = new BoundingBoxXYZ();
            //bb.Min = outline.MinimumPoint;
            //bb.Max = outline.MaximumPoint;
            //new TransactionBuilder(_doc).Build(() =>
            //{
            //    var visualizator = new BoundingBoxVisualisator(bb, _doc);
            //    visualizator.Show();
            //}, "show BoundingBox");          

            var mEPSystem = new SimpleMEPSystemBuilder(_baseMEPCurve).Build();
            var span = mEPSystem.Root.GetElements(startConnectionPoint.Element, endConnectionPoint.Element, true);
            var objectsToExclude = new List<Element>();
            objectsToExclude.AddRange(span);

            //var objectsToExclude = new List<Element>() { startConnectionPoint.Element };
            //if (startConnectionPoint.Element.Id != endConnectionPoint.Element.Id)
            //{ objectsToExclude.Add(endConnectionPoint.Element); }

            var pathFindFactory = new xYZPathFinder(basisStrategy, traceSettings).AddDoc(_uiDoc);
            pathFindFactory.Build(_baseMEPCurve, objectsToExclude, _exludedCathegories, true, planes, basisStrategy.MEPCurve1, basisStrategy.MEPCurve2);
            pathFindFactory.TokenSource = new CancellationTokenSource();
            return pathFindFactory.FindPath(startConnectionPoint, endConnectionPoint);
        }

        public ConnectionPoint GetPoint(int pointId)
        {
            XYZ point = null;
            Element element = null;

            try
            {
                (element, point) = CenterPoint(pointId);
            }
            catch (OperationCanceledException)
            { (element, point) = PickPoint(pointId); }

            return new ConnectionPoint(element, point);
        }

        public (Element element, XYZ point) CenterPoint(int pointId)
        {
            var selector = new ElementSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите элемент для получения точки присоединения {pointId}. " +
                "Или нажмите 'ESC', чтобы выбрать точку на элементе.");
            var centerPoint = ElementUtils.GetLocationPoint(element);
            //centerPoint.Show(_doc);
            Debug.WriteLine($"Selected element is: {element.Id}.");
            return (element, centerPoint);
        }

        public (Element element, XYZ point) PickPoint(int pointId)
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите точку присоединения {pointId} на элементе.");
            //selector.Point.Show(_doc);
            Debug.WriteLine($"Selected element is: {element.Id}.");
            return (element, selector.Point);
        }

        private (ConnectionPoint startPoint, ConnectionPoint endPoint) GetEdgePoints()
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = false };

            var element = selector.Pick($"Укажите точку присоединения 1 на элементе.");
            if(element == null) return (null, null);
            ConnectionPoint connectionPoint1 = new ConnectionPoint(element, selector.Point);
            if (connectionPoint1.IsValid)
            {
                connectionPoint1.Point.Show(_doc, 200.MMToFeet());
                element = selector.Pick($"Укажите точку присоединения 2 на элементе.");
                if (element == null) return (null, null);
                ConnectionPoint connectionPoint2 = new ConnectionPoint(element, selector.Point);
                connectionPoint2.Point.Show(_doc, 200.MMToFeet());
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
                A = 30,
                B = 50.MMToFeet(),
                //AList = new List<int>() {30}
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

        private Outline GetOutline(XYZ startPoint = null, XYZ endPoint = null)
        {
            XYZ p1;
            if(startPoint == null)
            {
                p1 = _uiDoc.Selection.PickPoint("Укажите первую точку зоны поиска.");
                p1.Show(_doc, 200.MMToFeet());
                _uiDoc.RefreshActiveView();
            }
            else
            { p1 = startPoint; }

            XYZ p2;
            if (startPoint == null)
            {
                p2 = _uiDoc.Selection.PickPoint("Укажите вторую точку зоны поиска.");
                p2.Show(_doc, 200.MMToFeet());
                _uiDoc.RefreshActiveView();
            }
            else
            { p2 = endPoint; }

            var (minPoint, maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ>() { p1, p2 });

            double offsetX = startPoint is null ? 0 : 5000.MMToFeet();
            double offsetY = startPoint is null ? 0 : 5000.MMToFeet();
            double offsetZ = 5000.MMToFeet();

            var moveVector = new XYZ(XYZ.BasisX.X * offsetX, XYZ.BasisY.Y * offsetY, XYZ.BasisZ.Z * offsetZ);

            var p11 = minPoint + moveVector;
            var p12 = minPoint - moveVector;
            (XYZ minP1, XYZ maxP1) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p11, p12 });

            var p21 = maxPoint + moveVector;
            var p22 = maxPoint - moveVector;
            (XYZ minP2, XYZ maxP2) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p21, p22 });

            return new Outline(minP1, maxP2);
        }
    }
}
