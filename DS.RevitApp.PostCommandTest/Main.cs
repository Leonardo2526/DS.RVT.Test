using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.PostCommandTest
{
    internal class Main
    {
        public UIApplication App;

        public Main(UIApplication app)
        {
            this.App = app;
        }

        public void ExecuteLoadProcess()
        {
            TaskDialog.Show("Revit", "Hello!");

            ExternalApplication.thisApp.m_MyForm.Close();
        }
    }
}
