using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using DS.RVT.ModelSpaceFragmentation.Path;
using DS.RVT.ModelSpaceFragmentation.Visualization;
using System;

namespace DS.RVT.ModelSpaceFragmentation
{
    class SpaceFragmentator
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public SpaceFragmentator(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }


        public void FragmentSpace()
        {
            ElementUtils elementUtils = new ElementUtils();
            Element element = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            PointsInfo pointsInfo = new PointsInfo();
            pointsInfo.GetPoints(element);

            ModelSpacePointsGenerator modelSpacePointsGenerator =
            new ModelSpacePointsGenerator(PointsInfo.MinBoundPoint, PointsInfo.MaxBoundPoint);
            List<XYZ> spacePoints = modelSpacePointsGenerator.Generate();

            ModelSolid modelSolid = new ModelSolid(Doc);
            Dictionary<Element, List<Solid>> solids = modelSolid.GetSolids();

            PointsSeparator pointsSeparator = new PointsSeparator(spacePoints);
            pointsSeparator.Separate(Doc);

            Visualize(pointsSeparator);
        }

        private void Visualize(PointsSeparator pointsSeparator)
        {
            PointsVisualizator visualizator = new PointsVisualizator(Doc);
            IPointsVisualization passablePointsVisualization = new SpacePointsVisualization(pointsSeparator.PassablePoints);
            visualizator.Show(passablePointsVisualization);

            IPointsVisualization unpassablePointsVisualization = new SpacePointsVisualization(pointsSeparator.UnpassablePoints);
            unpassablePointsVisualization.
            visualizator.Show(new SpacePointsVisualization(pointsSeparator.UnpassablePoints));
        }
    }
}
