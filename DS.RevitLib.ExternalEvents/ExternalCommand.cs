using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.ExternalEvents
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        private ExternalEventExampleDialog m_MyForm;

        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            try
            {
                ShowForm();
                //ExternalEventExampleApp.thisApp.ShowForm(uiapp);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        public void ShowForm()
        {
            // If we do not have a dialog yet, create and show it
            if (m_MyForm == null || m_MyForm.IsDisposed)
            {
                // A new handler to handle request posting by the dialog
                ExternalEventExample handler = new ExternalEventExample();

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible for disposing them, eventually.
                m_MyForm = new ExternalEventExampleDialog(exEvent, handler);
                m_MyForm.Show();
            }
        }
    }
}
