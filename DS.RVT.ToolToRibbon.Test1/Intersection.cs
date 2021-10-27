using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.ToolToRibbon.Test1
{
    class Intersection
    {

        public void FindIntersections(UIDocument uidoc, Document doc)
        {

            // Find intersections between elements and a selected element
            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, "Select element that will be checked for intersection with all elements");
            Element element = doc.GetElement(reference);
            GeometryElement geomElement = element.get_Geometry(new Options());

            // Get all element ids which are current selected by users
            ICollection<ElementId> selectedIds = new List<ElementId>
            {
                element.Id
            };

            Solid solid = null;
            foreach (GeometryObject geomObj in geomElement)
            {
                solid = geomObj as Solid;
                if (solid != null) break; 
            }

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            collector.OfClass(typeof(Pipe));

            ElementIntersectsSolidFilter intersectionFilter = new ElementIntersectsSolidFilter(solid);
            collector.WherePasses(intersectionFilter); // Apply intersection filter to find matches
            

            // Use the selection to instantiate an exclusion filter
            ExclusionFilter exclusionFilter = new ExclusionFilter(selectedIds);
            collector.WherePasses(exclusionFilter);

            IList<Element> elements = collector.ToElements();

            foreach (Element e in elements)
            {

                TaskDialog.Show("Revit", collector.Count() + 
                    " family instances intersect with the selected element (" + e.Category.Name + " id:" + e.Id.ToString() + ")");

            }



        }
    }
}
