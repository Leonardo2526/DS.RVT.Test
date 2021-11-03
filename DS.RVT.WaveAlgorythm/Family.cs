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
        public void CreateCell(Document document, XYZ location)
        {

            // get the given view's level for beam creation
            Level level  = new FilteredElementCollector(document)
                .OfClass(typeof(Level)).Cast<Level>().FirstOrDefault();

            // get a family symbol
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel);

            FamilySymbol gotSymbol = collector.FirstElement() as FamilySymbol;

           
                using (Transaction transNew = new Transaction(document, "newTransaction"))
                {
                    try
                    {
                        transNew.Start();
                        // create a new beam
                        FamilyInstance instance = document.Create.NewFamilyInstance(location, gotSymbol,
                                                                                    level, StructuralType.NonStructural);
                    }

                    catch (Exception e)
                    {
                        transNew.RollBack();
                        TaskDialog.Show("Revit", e.ToString());
                    }
                    transNew.Commit();
                }
          
        }
    }
}
