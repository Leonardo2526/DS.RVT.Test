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

        public List<XYZ> PassablePoints { get; set; }
        public List<XYZ> UnpassablePoints { get; set; }

        public void FragmentSpace(Element element)
        {
            
            PointsInfo pointsInfo = new PointsInfo();
            pointsInfo.GetPoints(element);

            ModelSpacePointsGenerator modelSpacePointsGenerator =
            new ModelSpacePointsGenerator(PointsInfo.MinBoundPoint, PointsInfo.MaxBoundPoint);
            List<XYZ> spacePoints = modelSpacePointsGenerator.Generate();

            ModelSolid modelSolid = new ModelSolid(Doc);
            Dictionary<Element, List<Solid>> solids = modelSolid.GetSolids();

            PointsSeparator pointsSeparator = new PointsSeparator(spacePoints);
            pointsSeparator.Separate(Doc);

            UnpassablePoints = pointsSeparator.UnpassablePoints;
            PassablePoints = pointsSeparator.PassablePoints;

            //Visualize(pointsSeparator);
        }

        private void Visualize(PointsSeparator pointsSeparator)
        {
            Visualizator visualizator = new Visualizator(Doc);

            visualizator.ShowPoints(new SpacePointsVisualization(pointsSeparator.PassablePoints));

            IPointsVisualization unpassablePointsVisualization = new SpacePointsVisualization(pointsSeparator.UnpassablePoints)
            {
                OverwriteGraphic = true
            };
            visualizator.ShowPoints(unpassablePointsVisualization);
        }
    }
}
