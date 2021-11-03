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


        public void FindCollision(XYZ corner1, XYZ corner2)
        {
            

            // Create a Outline, uses a minimum and maximum XYZ point to initialize the outline. 
            Outline myOutLn = new Outline(corner1, corner2);

            // Create a BoundingBoxIntersects filter with this Outline
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);


            //Get all pipes in document
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(Pipe));

            collector.WherePasses(filter);

            IList<Element> elements = collector.ToElements();

            if (elements.Count >0)
            {
                string elCount = "";
                string IDS = "";
                string names = "";

                Cell cell = new Cell(App, Uiapp, Doc, Uidoc);
                cell.CreateModelLine(corner1, corner2);

                /*
                foreach (Element elementB in elements)
                {
                    IDS += "\n" + elementB.Id.ToString();
                    names += "\n" + elementB.Category.Name;
                    elCount += 1;
                }


                TaskDialog.Show("Revit", elCount +
                " element intersect with the next elements \n (" + names + " id:" + IDS + ")");
                */
            }


          

        }


    }
}
