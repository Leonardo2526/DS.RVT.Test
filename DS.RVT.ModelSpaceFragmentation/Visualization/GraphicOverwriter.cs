using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.ModelSpaceFragmentation.Visualization
{ 

    class GraphicOverwriter
    {
        public void OverwriteElementsGraphic(List<FamilyInstance> instances, Color color)
        {
            foreach (FamilyInstance instance in instances)
            {
                OverwriteGraphic(instance, color);
            }
        }


        void OverwriteGraphic(FamilyInstance instance, Color color)
        {
            Element element = instance as Element;
            Document doc = element.Document;
            UIDocument uIDocument = new UIDocument(doc);

            OverrideGraphicSettings pGraphics = new OverrideGraphicSettings();
            pGraphics.SetProjectionLineColor(color);


            var patternCollector = new FilteredElementCollector(doc);
            patternCollector.OfClass(typeof(FillPatternElement));
            FillPatternElement solidFillPattern = patternCollector.ToElements().Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);

            View3D view3D = Get3dView(doc);
            
            pGraphics.SetSurfaceForegroundPatternId(solidFillPattern.Id);
            pGraphics.SetSurfaceBackgroundPatternColor(color);

            using (Transaction transNew = new Transaction(doc, "OverwriteGraphic"))
            {
                try
                {
                    transNew.Start();
                    doc.ActiveView.SetElementOverrides(instance.Id, pGraphics);
                    view3D.SetElementOverrides(instance.Id, pGraphics);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }

        }

        /// <summary>
        /// Retrieve a suitable 3D view from document.
        /// </summary>
        View3D Get3dView(Document doc)
        {
            FilteredElementCollector collector
              = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D));

            foreach (View3D v in collector)
            {
                if (!v.IsTemplate)
                {
                    return v;
                }
            }
            return null;
        }
    }
}
