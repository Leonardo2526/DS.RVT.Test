using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.MEP;
using OLMP.RevitAPI.Tools.ModelCurveUtils;
using PathFinderLib;

namespace DS.RevitApp.Test
{
    internal class PathFinerTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uIDoc;

        public PathFinerTest(Document doc, UIDocument uIDoc = null)
        {
            _doc = doc;
            _uIDoc = uIDoc;
        }

        public void Run()
        {
            XYZ startPoint;
            XYZ endPoint;
            ObjectSnapTypes snapTypes = ObjectSnapTypes.Endpoints | ObjectSnapTypes.Intersections;
            startPoint = _uIDoc.Selection.PickPoint(snapTypes, "Select startPoint");
            endPoint = _uIDoc.Selection.PickPoint(snapTypes, "Select endPoint");

            Reference reference = _uIDoc.Selection.PickObject(ObjectType.Element, "Select MEPCurve1");
            MEPCurve mEPCurve = _doc.GetElement(reference) as MEPCurve;
            (double width, double heigth) = MEPCurveUtils.GetWidthHeight(mEPCurve);
            ElementUtils.GetPoints(mEPCurve, out XYZ p11, out XYZ p12, out XYZ c1);


            //reference = _uIDoc.Selection.PickObject(ObjectType.Element, "Select MEPCurve2");
            //mEPCurve = _doc.GetElement(reference) as MEPCurve;
            //ElementUtils.GetPoints(mEPCurve, out XYZ p21, out XYZ p22, out XYZ c2);
            //startPoint = p11;
            //endPoint = p12;

            startPoint.Show(_doc);
            endPoint.Show(_doc);
            _uIDoc.RefreshActiveView();

            //создаем опции поиска
            //параметр в конструкторе это Ширина отвода от оси до грани
            //исходя из этого параметра будет подбираться минимальный шаг поиска так, что
            //минимальная длина прямого участка 50 мм + 2* Ширина отвода
            var options = new FinderOptions(new List<int>());

            //класс анализирует геометрию
            GeometryDocuments geometryDocuments = null;
            //var geometryDocuments = new GeometryDocuments(_doc, options, null);

            //класс для поиска пути
            var finder = new PathFinderToOnePoint(startPoint, endPoint,
                         heigth, width, geometryDocuments, options);

            //ищем путь
            List<XYZ> path = new List<XYZ>();
            Task<List<XYZ>> task = finder.FindPath(new CancellationTokenSource().Token);        
            task.Wait();
            path = task.Result;

            if (path == null)
            {
                Debug.Print("не удалось найти путь");
            }

            //объединяем прямые последовательные участки пути в один сегмент
            path = Optimizer.MergeStraightSections(path, options);

            var trb = new TransactionBuilder(_doc);
            trb.Build(() => ShowPath(path), "show path");
        }
        private void ShowPath(List<XYZ> path)
        {
            var mcreator = new ModelCurveCreator(_doc);
            for (int i = 0; i < path.Count - 1; i++)
            {
                mcreator.Create(path[i], path[i + 1]);
                var line = Line.CreateBound(path[i], path[i + 1]);
            }
        }
    }
}
