using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.AutoPipesCoordinarion
{
    class Collision
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public Collision(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }

        public void FindCollisions()
        // Find collisions between elements and a selected element by solid
        {
            // Find collisions between elements and a selected element
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select element that will be checked for intersection with all elements");
            Element elementA = Doc.GetElement(reference);

            Solid solidA = GetSolid(elementA);

            // Get all element ids which are current selected by users
            ICollection<ElementId> selectedIds = new List<ElementId>
            {
                elementA.Id
            };

            //Get all pipes in document
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(Pipe));            
          
            ElementIntersectsSolidFilter intersectionFilter = new ElementIntersectsSolidFilter(solidA);
            collector.WherePasses(intersectionFilter); // Apply intersection filter to find matches


            // Use the selection to instantiate an exclusion filter
            ExclusionFilter exclusionFilter = new ExclusionFilter(selectedIds);
            collector.WherePasses(exclusionFilter);

            IList<Element> elements = collector.ToElements();

            string elCount = "";
            string IDS = "";
            string names = "";

            RevitElements revitElements = new RevitElements(App, Uiapp, Uidoc , Doc);

            foreach (Element elementB in elements)
            {
                
                Pipe pipeB = elementB as Pipe;
                revitElements.GetPoints(pipeB, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);

                Data data = new Data();
                data.SetValues(startPoint, endPoint);

                Cell cell = new Cell(App, Uiapp, Doc, Uidoc, data);
                cell.GetCells();
                cell.GetElementZonePoints(elementB);

                //Uidoc.RefreshActiveView();

                List<XYZ> ICLocations = cell.FindCollisions(elementB);

                Uidoc.RefreshActiveView();

                WaveAlgorythm waveAlgorythm = new WaveAlgorythm(Uidoc, ICLocations, data, cell);
                waveAlgorythm.FindPath();


                IDS += "\n" + elementB.Id.ToString();
                    names += "\n" + elementB.Category.Name;
                    elCount += 1;
              
                      
            }


            //TaskDialog.Show("Revit", elCount +
            //" element intersect with the next elements \n (" + names + " id:" + IDS + ")");
        }

        private Solid GetSolid(Element element)
        {
            GeometryElement geomElement = element.get_Geometry(new Options());

            Solid solid = null;
            foreach (GeometryObject geomObj in geomElement)
            {
                solid = geomObj as Solid;
                if (solid != null) break;
            }

            return solid;
        }


        private void GetCollisionLines(Solid solidA, Solid solidB)
        {
            Solid intersection = BooleanOperationsUtils.ExecuteBooleanOperation(solidA, solidB, BooleanOperationsType.Intersect);

            foreach (Edge edge in intersection.Edges)
            {
                XYZ startPoint = edge.Tessellate()[0];
                XYZ endPoint = edge.Tessellate()[1];

                RevitElements revitElements = new RevitElements(App, Uiapp, Uidoc, Doc);
                revitElements.CreateModelLine(startPoint, endPoint);
            }
        }

        bool CheckElementForMove(Element elementA, Element elementB)
        {
            bool elementForMove = false;

            RevitElements revitElements = new RevitElements(App, Uiapp, Uidoc, Doc);
            revitElements.GetPoints(elementA, out XYZ startPointA, out XYZ endPointA, out XYZ centerPointA);
            revitElements.GetPoints(elementB, out XYZ startPointB, out XYZ endPointB, out XYZ centerPointB);

            double tgA = (endPointA.Y - startPointA.Y) / (endPointA.X - startPointA.X);
            double tgB = (endPointB.Y - startPointB.Y) / (endPointB.X - startPointB.X);

            double radA = Math.Atan(tgA);
            double angleA = radA * (180 / Math.PI);

            double radB = Math.Atan(tgB);
            double angleB = radB * (180 / Math.PI);

            double deltaAndle = Math.Abs(angleA - angleB);

            if (deltaAndle < 15 | (180 - deltaAndle) < 15)
                elementForMove = true;

            return elementForMove;
        }



        public bool CheckCollisionsWithModifiedElements(ICollection<ElementId> modifiedElementsIds)
        {
            //Get all modified pipes
            FilteredElementCollector modifiedPipes = new FilteredElementCollector(Doc, modifiedElementsIds);
            ElementClassFilter elementFilter = new ElementClassFilter(typeof(Pipe));
            modifiedPipes.WherePasses(elementFilter);
            IList<Element> modifiedElements = modifiedPipes.ToElements();

            //Get all pipes in document
            FilteredElementCollector allDocPipes = new FilteredElementCollector(Doc);
            allDocPipes.OfClass(typeof(Pipe));

            //Exclude modified pipes
            ExclusionFilter exclusionFilter = new ExclusionFilter(modifiedElementsIds);
            allDocPipes.WherePasses(exclusionFilter);          

            foreach (Element me in modifiedElements)
            {
                Solid solidME = GetSolid(me);
                ElementIntersectsSolidFilter intersectionFilterME = new ElementIntersectsSolidFilter(solidME);

                //Get pipes with collisions
                allDocPipes.WherePasses(intersectionFilterME);
                IList<Element> collisionElements = allDocPipes.ToElements();

                if (collisionElements.Count > 0)
                {
                    return true;
                }
            }

                return false;
        }

    }
}
