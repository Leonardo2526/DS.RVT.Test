using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.ModelSpaceFragmentation
{
    class Main
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public Main(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }


        public void StartProcess()
        {
            ElementUtils elementUtils = new ElementUtils();
            Element element = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            BoundPoints boundPoints = new BoundPoints();
            boundPoints.GetPoints(element);

            ModelSpacePointsGenerator modelSpacePointsGenerator = 
                new ModelSpacePointsGenerator(BoundPoints.Point1, BoundPoints.Point2);
            List<XYZ> spacePoints = modelSpacePointsGenerator.Generate();

            PointsSeparator pointsSeparator = new PointsSeparator(spacePoints);
            pointsSeparator.Separate(Doc);

            VisiblePointsCreator visiblePointsCreator = new VisiblePointsCreator();
            visiblePointsCreator.Create(Doc, pointsSeparator.PassablePoints);

            visiblePointsCreator = new VisiblePointsCreator();
            visiblePointsCreator.Create(Doc, pointsSeparator.UnpassablePoints);

            GraphicOverwriter graphicOverwriter = new GraphicOverwriter();
            Color color = new Color(255, 0, 0);
            graphicOverwriter.OverwriteElementsGraphic(visiblePointsCreator.Instances, color);
        }


    }
}
