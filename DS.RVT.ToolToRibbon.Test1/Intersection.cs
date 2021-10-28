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
        // Find intersections between elements and a selected element by solid
        {
            // Find intersections between elements and a selected element
            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, "Select element that will be checked for intersection with all elements");
            Element element = doc.GetElement(reference);

            Solid solidA = GetSolid(element);

            // Get all element ids which are current selected by users
            ICollection<ElementId> selectedIds = new List<ElementId>
            {
                element.Id
            };

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            collector.OfClass(typeof(Pipe));

            ElementIntersectsSolidFilter intersectionFilter = new ElementIntersectsSolidFilter(solidA);
            collector.WherePasses(intersectionFilter); // Apply intersection filter to find matches


            // Use the selection to instantiate an exclusion filter
            ExclusionFilter exclusionFilter = new ExclusionFilter(selectedIds);
            collector.WherePasses(exclusionFilter);

            IList<Element> elements = collector.ToElements();

            string IDS = "";
            string names = "";
            double volumeOfIntersection = 0;
            foreach (Element e in elements)
            {
                IDS += "\n" + e.Id.ToString();
                names += "\n" + e.Category.Name;

                Solid solidB = GetSolid(e);
                volumeOfIntersection = ComputeIntersectionVolume(doc, solidA, solidB);
            }

            TaskDialog.Show("Revit", collector.Count() +
                    " element intersect with the next elements \n (" + names + " id:" + IDS + "Volume: \n" + volumeOfIntersection + ")");
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


        private double ComputeIntersectionVolume(Document doc, Solid solidA, Solid solidB)
        {
            Solid intersection = BooleanOperationsUtils.ExecuteBooleanOperation(solidA, solidB, BooleanOperationsType.Intersect);
            double volumeOfIntersection = intersection.Volume;

            foreach (Edge edge in intersection.Edges)
            {
                XYZ startPoint = edge.Tessellate()[0];
                XYZ endPoint = edge.Tessellate()[1];
                //XYZ transformedStartPoint = instTransform.OfPoint(startPoint);
                //XYZ transformedEndPoint = instTransform.OfPoint(endPoint);

                RevitElements revitElements = new RevitElements();
                revitElements.CreateModelLine(doc, startPoint, endPoint);

            }


            return volumeOfIntersection;
        }

        private void GetAndTransformSolidInfo(Document doc, Element element, Options geoOptions)
        {
            // Get geometry element of the selected element
            Autodesk.Revit.DB.GeometryElement geoElement = element.get_Geometry(geoOptions);

            // Get geometry object
            foreach (GeometryObject geoObject in geoElement)
            {
                // Get the geometry instance which contains the geometry information
                Autodesk.Revit.DB.GeometryInstance instance = geoObject as Autodesk.Revit.DB.GeometryInstance;
                if (null != instance)
                {
                    foreach (GeometryObject instObj in instance.SymbolGeometry)
                    {
                        Solid solid = instObj as Solid;
                        if (null == solid || 0 == solid.Faces.Size || 0 == solid.Edges.Size)
                        {
                            continue;
                        }

                        
                        //Transform instTransform = instance.Transform;

                        /*
                        // Get the faces and edges from solid, and transform the formed points
                        foreach (Face face in solid.Faces)
                        {
                            Mesh mesh = face.Triangulate();
                            foreach (XYZ ii in mesh.Vertices)
                            {
                                XYZ point = ii;
                                XYZ transformedPoint = instTransform.OfPoint(point);
                            }
                        }
                        */

                        foreach (Edge edge in solid.Edges)
                        {
                            XYZ startPoint = edge.Tessellate()[0];
                            XYZ endPoint = edge.Tessellate()[1];
                            //XYZ transformedStartPoint = instTransform.OfPoint(startPoint);
                            //XYZ transformedEndPoint = instTransform.OfPoint(endPoint);

                            RevitElements revitElements = new RevitElements();
                            revitElements.CreateModelLine(doc, startPoint, endPoint);

                            /*
                            foreach (XYZ ii in edge.Tessellate())
                            {
                                XYZ point = ii;
                                //XYZ transformedPoint = instTransform.OfPoint(point);
                            }
                            */
                        }
                    }
                }
            }

        }
    }
}
