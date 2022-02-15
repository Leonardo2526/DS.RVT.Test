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

        public static List<XYZ> PassablePoints { get; set; }
        public static List<XYZ> UnpassablePoints { get; set; }

        public void FragmentSpace(Element element)
        {
            
            ElementInfo pointsInfo = new ElementInfo();
            pointsInfo.GetPoints(element);

            ModelSpacePointsGenerator modelSpacePointsGenerator =
            new ModelSpacePointsGenerator(ElementInfo.MinBoundPoint, ElementInfo.MaxBoundPoint);
            List<XYZ> spacePoints = modelSpacePointsGenerator.Generate();


           int c1 =SpaceZone.ZoneCountX;
           double s1 =SpaceZone.ZoneSizeX;

            List<BoundingBoxXYZ> boundingBoxes = BoundingBoxCreator.Create();

            ModelSolid modelSolid = new ModelSolid(Doc);
            Dictionary<Element, List<Solid>> solids = modelSolid.GetSolids();

            PointsSeparator pointsSeparator = new PointsSeparator(spacePoints);
            pointsSeparator.Separate();

            UnpassablePoints = pointsSeparator.UnpassablePoints;
            PassablePoints = pointsSeparator.PassablePoints;

            //Visualize(pointsSeparator);
        }

        private void Visualize(PointsSeparator pointsSeparator)
        {           
            Visualizator.ShowPoints(new PointsVisualizator(pointsSeparator.PassablePoints));

            IPointsVisualization unpassablePointsVisualization = new PointsVisualizator(pointsSeparator.UnpassablePoints)
            {
                OverwriteGraphic = true
            };
            Visualizator.ShowPoints(unpassablePointsVisualization);
        }
    }
}
