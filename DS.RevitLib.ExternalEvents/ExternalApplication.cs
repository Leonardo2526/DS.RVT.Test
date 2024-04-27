using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.ExternalEvents
{
    public class ExternalApplication : IExternalApplication
    {
        /// Implement this method to subscribe to event.
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Register event. 
                TaskDialog.Show("Revit","ExternalApp started");
                application.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;
                application.ControlledApplication.DocumentChanged += ControlledApplication_DocumentChanged;

            }
            catch (Exception)
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void ControlledApplication_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            var added = e.GetAddedElementIds();
            var mod = e.GetModifiedElementIds();
            var tra = e.GetTransactionNames();
            var del = e.GetDeletedElementIds();
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // remove the event.
            application.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;
            return Result.Succeeded;
        }

        private void ControlledApplication_FailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor fa = e.GetFailuresAccessor();
            var failList = fa.GetFailureMessages();
        }

    }

}
