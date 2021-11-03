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

        public List<FamilyInstance> familyInstances = new List<FamilyInstance>();
        public ICollection<ElementId> cellElementsIds = new List<ElementId>();

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

                        instance = Doc.Create.NewFamilyInstance(location, gotSymbol,
                            level, StructuralType.NonStructural);
            familyInstances.Add(instance);
            cellElementsIds.Add(instance.Id);
        }
    }
}
