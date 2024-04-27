using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Diagnostics;

namespace DS.RevitApp.TransactionTest
{
    public class ExternalEventHandler : IExternalEventHandler
    {
        public UIApplication App;
        public ExternalEventHandler(UIApplication app)
        {
            this.App = app;
        }

        public string GetName()
        {
            return "";
        }

        public void Execute(UIApplication app)
        {
            var message = "handler Executed!";
            Debug.WriteLine(message);
            TaskDialog.Show("Revit", message);
        }
    }

}
