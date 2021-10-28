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

            Solid solid = null;
            foreach (GeometryObject geomObj in geomElement)
            {
                solid = geomObj as Solid;
                if (solid != null) break;
            }

            BoundingBoxXYZ bb = solid.GetBoundingBox();

            // Find the bounding box from the selected 
            // object and convert to outline.
            //BoundingBoxXYZ bb = element.get_BoundingBox(doc.ActiveView);

            Transform trf = bb.Transform;

            XYZ maxInModelCoords = trf.OfPoint(bb.Max);
            XYZ minInModelCoords = trf.OfPoint(bb.Min);

            double offset = 100;

            XYZ solidMax = new XYZ(maxInModelCoords.X + offset, maxInModelCoords.Y + offset, maxInModelCoords.Z + offset);
            XYZ solidMin = new XYZ(minInModelCoords.X - offset, minInModelCoords.Y - offset, minInModelCoords.Z - offset);

            XYZ solidMax1 = new XYZ(bb.Max.X, bb.Max.Y, bb.Max.Z);
            XYZ solidMin1 = new XYZ(bb.Min.X, bb.Min.Y, bb.Min.Z);

            Outline outline = new Outline(solidMin, solidMax);
            //Outline outline = new Outline(bb.Min, bb.Max);

            // Create a BoundingBoxIntersectsFilter to 
            // find everything intersecting the bounding 
            // box of the selected element.

            BoundingBoxIntersectsFilter bbfilter = new BoundingBoxIntersectsFilter(outline);

            // Use a view to construct the filter so we 
            // get only visible elements. For example, 
            // the analytical model will be found otherwise.
            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);

            //ElementIntersectsSolidFilter intersectionFilter = new ElementIntersectsSolidFilter(solid);

            collector.OfClass(typeof(Pipe));

            // Apply intersection filter to find matches
            collector.WherePasses(bbfilter); 


            // Get all element ids which are current selected by users
            ICollection<ElementId> selectedIds = new List<ElementId>
            {
                element.Id
            };

            // Use the selection to instantiate an exclusion filter
            ExclusionFilter exclusionFilter = new ExclusionFilter(selectedIds);
            collector.WherePasses(exclusionFilter);
           
            IList<Element> elements = collector.ToElements();


            string IDS = "";
            string names = "";
            foreach (Element e in elements)
            {
                IDS += "\n" + e.Id.ToString();
                names += "\n"  + e.Category.Name;

            }

            TaskDialog.Show("Revit", collector.Count() +
                    " element intersect with the next elements (" + names + " id:" + IDS + ")");
        }
    }
}
