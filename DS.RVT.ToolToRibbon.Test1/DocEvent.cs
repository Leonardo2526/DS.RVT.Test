using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;


namespace DS.RVT.AutoPipesCoordinarion
{
    class DocEvent
    {

        readonly UIApplication Uiapp;

        public ICollection<ElementId> modifiedElementsIds = new List<ElementId>();

        public DocEvent(UIApplication uiapp)
        {
            Uiapp = uiapp;
        }

        public void RegisterEvent()
        {
            try
            {
                // Register event. 
                Uiapp.Application.DocumentChanged += new EventHandler<
    Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(application_DocumentChanged);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.ToString());
            }
        }

        public void application_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            Document Doc = e.GetDocument();

            //Instantiate a new class instance for element iteration and filtering
            ElementClassFilter filter = new ElementClassFilter(typeof(Pipe));

            ICollection<ElementId> colElID = e.GetModifiedElementIds(filter);

            string IDS = "";
            foreach (ElementId elID in colElID)
            {
                modifiedElementsIds.Add(elID);
                IDS += "\n" + elID.ToString();
            }

            //TaskDialog.Show("Revit", "Modified elements IDs: \n" + IDS);

        }

    }
}
