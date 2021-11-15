using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.PipesCollisionsElliminator
{
    class Collision
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;
        readonly ElementUtils ElemUtils;

        public Collision(Application app, UIApplication uiapp, UIDocument uidoc, Document doc, ElementUtils elemUtils)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
            ElemUtils = elemUtils;
        }


        public IList<Element> GetCollisions(Element element)
        {
            //Get all pipes in document
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(Pipe));

            //Get element's solid
            List<Solid> solids = ElemUtils.GetSolids(element);
            Solid elementSolid = null;
            foreach (Solid solid in solids)
                elementSolid = solid;

            ElementIntersectsSolidFilter intersectionFilter = new ElementIntersectsSolidFilter(elementSolid);

            // Apply intersection filter to find matches
            collector.WherePasses(intersectionFilter);

            // Use the selection to instantiate an exclusion filter
            ExclusionFilter exclusionFilter = new ExclusionFilter(ElemUtils.GetIdCollection(element));
            collector.WherePasses(exclusionFilter);

            return collector.ToElements();
        }




        public void EliminateCollision(Element SelectedElement, IList<Element> collisionElements)
        {
          
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

      



    }
}
