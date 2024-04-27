using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;

namespace DS.RevitApp.PostCommandTest
{
    public class ExternalApplication : IExternalApplication
    {
        // class instance
        internal static ExternalApplication thisApp = null;
        // ModelessForm instance
        public StartForm m_MyForm;


        public Result OnShutdown(UIControlledApplication application)
        {
            if (m_MyForm != null && m_MyForm.IsVisible)
            {
                m_MyForm.Close();
            }

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            Debug.IndentLevel = 1;
            try
            {
                // Register event. 
                application.ControlledApplication.ApplicationInitialized += OnApplicationInitialized;
                application.ControlledApplication.DocumentOpened += application_DocumentOpened;

            }
            catch (Exception)
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public void application_DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            // get document from event args.
            Debug.WriteLine("Document Opened.");

            RunCommand();          
        }

        UIApplication uiapp;
        void OnApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            Application app = sender as Application;
            uiapp = new UIApplication(app);
            Debug.WriteLine("Application Initialized.");
        }

        private void RunCommand()
        {
            string name = "8b231223-c983-49c8-b3a4-ab5c3ad81cda";
          
            RevitCommandId id_addin  = RevitCommandId.LookupCommandId(name);
            uiapp.PostCommand(id_addin);
            Debug.WriteLine("Command posted.");
        }

        public void ShowForm(UIApplication uiapp)
        {

            // If we do not have a dialog yet, create and show it
            if (m_MyForm == null || !m_MyForm.IsActive)
            {
                // A new handler to handle request posting by the dialog
                ExternalEventHandler handler = new ExternalEventHandler(uiapp);

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible fore disposing them, eventually.
                m_MyForm = new StartForm(uiapp, exEvent, handler);
                m_MyForm.Show();
            }
        }
    }
}
