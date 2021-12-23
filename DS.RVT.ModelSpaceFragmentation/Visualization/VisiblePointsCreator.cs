using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.ModelSpaceFragmentation
{
    class VisiblePointsCreator
    {
        public List<FamilyInstance> Instances { get; set; } = new List<FamilyInstance>();

        public void Create(Document Doc, List<XYZ> points)
        {
        
            
            int j;
            foreach (XYZ point in points)
            {
                AddCell(Doc, point);

            }

        }

        public void AddCell(Document Doc, XYZ location)
        {

            // get the given view's level for beam creation
            Level level = new FilteredElementCollector(Doc)
                .OfClass(typeof(Level)).Cast<Level>().FirstOrDefault();

            // get a family symbol
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel);

            FamilySymbol gotSymbol = collector.FirstElement() as FamilySymbol;

            using (Transaction transNew = new Transaction(Doc, "AddPoint"))
            {
                try
                {
                    transNew.Start();

                    Instances.Add(Doc.Create.NewFamilyInstance(location, gotSymbol,
                 level, StructuralType.NonStructural));
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
