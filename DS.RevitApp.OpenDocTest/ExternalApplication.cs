using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.OpenDocTest
{
    public class ExternalApplication : IExternalApplication
    {
        /// Implement this method to subscribe to event.
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Register event. 
                application.ControlledApplication.DocumentOpened += application_DocumentOpened;
                application.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;

            }
            catch (Exception)
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void ControlledApplication_FailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor fa = e.GetFailuresAccessor();
            var failList = fa.GetFailureMessages();
            TaskDialog.Show("Fails: ", failList.Count.ToString());
        }

        //public Result OnStartup(UIControlledApplication application)
        //{
        //    try
        //    {
        //        // Register event. 
        //        application.ControlledApplication.DocumentOpened += new EventHandler
        //             <Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(application_DocumentOpened);
        //    }
        //    catch (Exception)
        //    {
        //        return Result.Failed;
        //    }

        //    return Result.Succeeded;
        //}

        public Result OnShutdown(UIControlledApplication application)
        {
            // remove the event.
            application.ControlledApplication.DocumentOpened -= application_DocumentOpened;
            return Result.Succeeded;
        }

        public void application_DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            // get document from event args.
            Document doc = args.Document;
            TaskDialog.Show("Message", "DocumentOpened");
            //using (Transaction transaction = new Transaction(doc, "Edit Address"))
            //{
            //    if (transaction.Start() == TransactionStatus.Started)
            //    {
            //        doc.ProjectInformation.Address =
            //            "United States - Massachusetts - Waltham - 1560 Trapelo Road";
            //        transaction.Commit();
            //    }
            //}
        }

    }

}
