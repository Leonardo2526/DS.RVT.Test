using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DS.RevitApp.TransactionTest.Model
{
    class ExternalEventMy : IExternalEventHandler
    {
        public void Execute(UIApplication uiapp)
        {
            MessageBox.Show("My event");
        }
        public string GetName()
        {
            return "my event";
        }
    }
}
