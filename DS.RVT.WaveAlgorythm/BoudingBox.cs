using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;


namespace DS.RVT.WaveAlgorythm
{
    class BoudingBox
    {
        readonly Application App;
        readonly UIApplication Uiapp;
        readonly Document Doc;
        readonly UIDocument Uidoc;

        public BoudingBox(Application app, UIApplication uiapp, Document doc, UIDocument uidoc)
        {
            App = app;
            Uiapp = uiapp;
            Doc = doc;
            Uidoc = uidoc;
        }


        void FindCollision(XYZ centerPoint)
        {
            

            // Create a Outline, uses a minimum and maximum XYZ point to initialize the outline. 
            Outline myOutLn = new Outline(new XYZ(0, 0, 0), new XYZ(10, 10, 10));

            // Create a BoundingBoxIntersects filter with this Outline
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);


            //Get all pipes in document
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(Pipe));

            collector.WherePasses(filter); // Apply intersection filter to find matches

            IList<Element> elements = collector.ToElements();

            string elCount = "";
            string IDS = "";
            string names = "";

            foreach (Element elementB in elements)
            {
                    IDS += "\n" + elementB.Id.ToString();
                    names += "\n" + elementB.Category.Name;
                    elCount += 1;                
            }


            TaskDialog.Show("Revit", elCount +
            " element intersect with the next elements \n (" + names + " id:" + IDS + ")");

        }


    }
}
