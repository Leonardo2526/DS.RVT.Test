using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.WaveAlgorythm
{
    class Collision
    {
        readonly Application App;
        readonly UIApplication Uiapp;
        readonly Document Doc;
        readonly UIDocument Uidoc;

        public Collision(Application app, UIApplication uiapp, Document doc, UIDocument uidoc)
        {
            App = app;
            Uiapp = uiapp;
            Doc = doc;
            Uidoc = uidoc;
        }


        public void FindCollision(FamilySymbol gotSymbol, FamilyInstance instance)
        {
            //Solid solidCell = GetSolid(instance);
            ElementIntersectsElementFilter elementIntersectsElementFilter = new ElementIntersectsElementFilter(instance);
            //ElementIntersectsSolidFilter intersectionFilter = new ElementIntersectsSolidFilter(solidCell);
            //FamilyInstanceFilter familyInstanceFilter = new FamilyInstanceFilter(Doc, gotSymbol.Id);


            //Get all pipes in document
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            //collector.OfClass(typeof(Pipe));
            collector.WherePasses(elementIntersectsElementFilter); // Apply intersection filter to find matches

            IList<Element> elements = collector.ToElements();

            if (elements.Count >0)
            {
                string elCount = "";
                string IDS = "";
                string names = "";               

                Color color = new Color(255, 0, 0); // RGB


                var patternCollector = new FilteredElementCollector(Doc);
                patternCollector.OfClass(typeof(FillPatternElement));
                FillPatternElement solidFillPattern = patternCollector.ToElements().Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);

                OverrideGraphicSettings pGraphics = new OverrideGraphicSettings();

                pGraphics.SetSurfaceForegroundPatternId(solidFillPattern.Id);
                pGraphics.SetSurfaceBackgroundPatternColor(color);
                //pGraphics.SetProjectionLineColor(color);


                using (Transaction transNew = new Transaction(Doc, "newTransaction"))
                {
                    try
                    {
                        transNew.Start();
                        Doc.ActiveView.SetElementOverrides(instance.Id, pGraphics);
                    }

                    catch (Exception e)
                    {
                        transNew.RollBack();
                        TaskDialog.Show("Revit", e.ToString());
                    }
                    transNew.Commit();
                }
               
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


        private Solid GetSolid(FamilyInstance instance)
        {
            GeometryElement geomElement = instance.get_Geometry(new Options());

            Solid solid = null;
            foreach (GeometryObject geomObj in geomElement)
            {
                solid = geomObj as Solid;
                if (solid != null) break;
            }

            return solid;
        }
    }
}
