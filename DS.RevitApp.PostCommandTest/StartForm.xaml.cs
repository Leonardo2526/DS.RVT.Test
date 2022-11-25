using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace DS.RevitApp.PostCommandTest
{
    /// <summary>
    /// Interaction logic for StartForm.xaml
    /// </summary>
    public partial class StartForm : Window
    {
        private ExternalEvent m_ExEvent;
        private ExternalEventHandler m_Handler;
        public UIApplication _uiApp;

        public StartForm(UIApplication app, ExternalEvent exEvent, ExternalEventHandler handler)
        {
            InitializeComponent();
            this._uiApp = app;
            m_ExEvent = exEvent;
            m_Handler = handler;
        }
        private void Button_Hello_Click(object sender, RoutedEventArgs e)
        {
            //Start loading process
            //m_ExEvent.Raise();

            RunCommand();
        }

        private void RunCommand()
        {
            string name = "8b231223-c983-49c8-b3a4-ab5c3ad81cda";

            RevitCommandId id_addin = RevitCommandId.LookupCommandId(name);
            _uiApp.PostCommand(id_addin);
            Debug.WriteLine("Command posted.");
        }


    }
}
