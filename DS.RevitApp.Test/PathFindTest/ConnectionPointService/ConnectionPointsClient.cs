using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using DS.RevitApp.Test.PathFindTest.PathFinders;
using DS.RevitApp.Test.PathFindTest.Solution;
using DS.RevitApp.Test.PathFindTest.Solution.Creators;
using DS.RevitApp.Test.TransformTest;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.TransactionCommitter;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;
using DS.RevitLib.Utils.FamilyInstances;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Extensions;

namespace DS.RevitApp.Test.PathFindTest.ConnectionPointService
{
    internal class ConnectionPointsClient
    {

        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly IPathFinder _pathFinder;
        private MEPSystemModel _mEPSystemModel;
        private SketchSolutionModel _sketchSolutionModel;
        private ConnectionPoint _point1;
        private ConnectionPoint _point2;
        private string _transactionPrefix;
        private List<Element> _elementsToDelete;
        private readonly Committer _committer;
        private MEPCurve _baseMEPCurve;


        public ConnectionPointsClient(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = _uidoc.Document;
            _pathFinder = new SimplePathFinder(2, 2);
        }
        

        public void Run()
        {
            //Get MEPSystemModel
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element");
            _baseMEPCurve = _doc.GetElement(reference) as MEPCurve;

            var mEPSystemBuilder = new SimpleMEPSystemBuilder(_baseMEPCurve);
            _mEPSystemModel = mEPSystemBuilder.Build();

            //var (point1, point2) = GetPointsManually();
            //List<XYZ> points = pathGenerator.FindPath(point1.Value, point2.Value);
            List<XYZ>  path = GetPath();
            _elementsToDelete = _mEPSystemModel.Root.GetElements(_point1.Element, _point2.Element);

            //Show path
            //var creator = new ModelCurveCreator(_doc);
            //for (int i = 0; i < path.Count - 1; i++)
            //{
            //    creator.Create(path[i], path[i + 1]);
            //    var line = Line.CreateBound(path[i], path[i + 1]);
            //}

            //_uidoc.RefreshActiveView();


            var lineBuilder = new LineModelBuilder(path, _sketchSolutionModel, 225.mmToFyt2(), _elementsToDelete);
            var lineModels = lineBuilder.Build();

            var allAccIds = _mEPSystemModel.Root.Accessories.Select(a => a.Id);
            var accecoriesSpan = _elementsToDelete.Where(obj => allAccIds.Contains(obj.Id)).OfType<FamilyInstance>().ToList();

            var accecoriesExt = accecoriesSpan.Select(obj => new SolidModelExt(obj)).ToList();

            var collisionCheckers = GetCheckers();
            var builder = new FamToLineMultipleBuilder(accecoriesExt, lineModels, path, 50.mmToFyt2(), collisionCheckers, lineBuilder.StartMEPCurveModel);
            var transformModels = builder.Build().Cast<FamToLineTransformModel>().ToList();

            //create MEPSystem
            var builderByPoints = new BuilderByPoints(_baseMEPCurve, path, new RollBackCommitter(), _transactionPrefix);
            var MEPSystemModel = builderByPoints.BuildMEPCurves().WithFittings();
            List<MEPCurve> mEPCurves = MEPSystemModel.MEPCurves.Cast<MEPCurve>().ToList();

            List<MEPCurve> currentMEPCurves = new List<MEPCurve>();
            currentMEPCurves.AddRange(mEPCurves);

            //place families
            var creator = new FamInstCreator(_doc, _committer, _transactionPrefix);
            foreach (var model in transformModels)
            {
                MEPElementUtils.Disconnect(model.SourceObject.Element);
                TransformElement(model.SourceObject.Element, model.MoveVector, model.Rotations);

                //Connect families
                var fam = model.SourceObject.Element as FamilyInstance;
                MEPCurve mc = fam.GetMEPCuveToInsert(mEPCurves);
                creator.Insert(fam, mc, out List<MEPCurve> splittedMEPCurves);
                currentMEPCurves.AddRange(splittedMEPCurves);
            }
        }

        private (KeyValuePair<Element, XYZ> point1, KeyValuePair<Element, XYZ> point2) GetPointsManually()
        {
            var selector = new ConnectionElementSelector(_uidoc);
            var element1 = selector.Select("Element1");
            var element2 = selector.Select("Element2");

            return (element1, element2);
        }

        private List<XYZ> GetPath()
        {
            var strategy = new LoopCheckPointsStrategy(_pathFinder, null, _mEPSystemModel);
            var solutionCreator = new SketchSolutionCreator(strategy, _mEPSystemModel);
            _sketchSolutionModel = solutionCreator.Create() as SketchSolutionModel;
            _point1 = _sketchSolutionModel.Point1 as ConnectionPoint;
            _point2 = _sketchSolutionModel.Point2 as ConnectionPoint;

            return _sketchSolutionModel.Path;
        }
        private Element TransformElement(Element element, XYZ moveVector, List<RotationModel> rotationModels)
        {
            using (Transaction transNew = new Transaction(_doc, _transactionPrefix + "Transform " + element.Id))
            {
                try
                {
                    transNew.Start();
                    if (moveVector is not null)
                    {
                        ElementTransformUtils.MoveElement(_doc, element.Id, moveVector);
                    }
                    foreach (var rot in rotationModels)
                    {
                        ElementTransformUtils.RotateElement(_doc, element.Id, rot.Axis, rot.Angle);
                    }
                }

                catch (Exception e)
                { return null; }

                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return element;
        }

        private List<ICollisionChecker> GetCheckers()
        {
            //Create collisions checker
            var modelElements = GetGeometryElements(_doc);
            var excludedObjects = new List<Element> { _baseMEPCurve };

            var collisionCheckers = new List<ICollisionChecker>();

            //Get model checker
            var sm = new SolidCollisionChecker(modelElements, excludedObjects);
            collisionCheckers.Add(sm);

            //get link checkers
            //foreach (var link in DocModel.GetInstance().Links)
            //{
            //    List<Element> linkElements = link.ElementSolids.Keys.ToList();
            //    collisionCheckers.Add(new LinkCollisionChecker(linkElements, link.LinkInsatance, null));
            //}

            return collisionCheckers;
        }

        private List<Element> GetGeometryElements(Document doc, Transform tr = null)
        {
            if (doc == null || !doc.IsValidObject)
                return null;

            var categories = doc.Settings.Categories.Cast<Category>().Where(x => x.CategoryType == CategoryType.Model).Select(x => x.Id)
                .Where(x => !x.IntegerValue.Equals((int)BuiltInCategory.OST_Materials)).ToList();
            var filter = new ElementMulticategoryFilter(categories);
            var elems = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                    .WherePasses(filter).Where(x => ContainsSolidChecker(x)).ToList();

            elems = elems.Where(x => x.Category.GetBuiltInCategory().ToString() !=
            BuiltInCategory.OST_TelephoneDevices.ToString()).ToList();

            return elems;
        }
        private bool ContainsSolidChecker(Element x)
        {
            var g = x.get_Geometry(new Options() { ComputeReferences = false, DetailLevel = ViewDetailLevel.Fine, IncludeNonVisibleObjects = false })
                ?.Cast<GeometryObject>().ToList();

            return CheckGeometry(g);
        }

        private bool CheckGeometry(List<GeometryObject> g)
        {
            if (g is null) return false;

            foreach (var elem in g)
            {
                if (elem is Solid s && s.Volume > 1e-6)
                    return true;
                else if (elem is GeometryInstance gi)
                {
                    var go = gi.GetInstanceGeometry();
                    return CheckGeometry(go?.Cast<GeometryObject>().ToList());
                }
            }
            return false;
        }

    }
}
