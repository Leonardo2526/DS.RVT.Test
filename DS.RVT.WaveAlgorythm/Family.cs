using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.WaveAlgorythm
{
    class Family
    {
        readonly Application App;
        readonly UIApplication Uiapp;
        readonly Document Doc;
        readonly UIDocument Uidoc;

        public Family(Application app, UIApplication uiapp, Document doc, UIDocument uidoc)
        {
            App = app;
            Uiapp = uiapp;
            Doc = doc;
            Uidoc = uidoc;
        }

        public void CreateCell(XYZ location)
        {

            // get the given view's level for beam creation
            Level level  = new FilteredElementCollector(Doc)
                .OfClass(typeof(Level)).Cast<Level>().FirstOrDefault();

            // get a family symbol
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel);

            FamilySymbol gotSymbol = collector.FirstElement() as FamilySymbol;
            FamilyInstance instance = null;
            
                using (Transaction transNew = new Transaction(Doc, "newTransaction"))
                {
                    try
                    {
                        transNew.Start();
                        instance = Doc.Create.NewFamilyInstance(location, gotSymbol,
                                                                                    level, StructuralType.NonStructural);
                 
                }

                    catch (Exception e)
                    {
                        transNew.RollBack();
                        TaskDialog.Show("Revit", e.ToString());
                    }
                    transNew.Commit();
                }

            Collision collision = new Collision(App, Uiapp, Doc, Uidoc);
            collision.FindCollision(gotSymbol, instance);


        }
    }
}
