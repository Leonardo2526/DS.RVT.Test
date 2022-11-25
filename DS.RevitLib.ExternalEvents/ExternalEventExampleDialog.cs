using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using System;
using System.Windows.Forms;

namespace DS.RevitLib.ExternalEvents
{
    public partial class ExternalEventExampleDialog : Form
    {
        private ExternalEvent m_ExEvent;
        private IExternalEventHandler m_Handler;
        private UIApplication _uiApp;

        public ExternalEventExampleDialog(ExternalEvent exEvent, IExternalEventHandler handler, UIApplication uiApp)
        {
            InitializeComponent();
            m_ExEvent = exEvent;
            m_Handler = handler;
            this._uiApp = uiApp;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // we own both the event and the handler
            // we should dispose it before we are closed
            m_ExEvent.Dispose();
            m_ExEvent = null;
            m_Handler = null;

            // do not forget to call the base class
            base.OnFormClosed(e);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void showMessageButton_Click(object sender, EventArgs e)
        {
            var command = (TestCommand)m_Handler;
            command.Run(() => new TransactionTest(_uiApp));

            //new TransactionTest(_uiApp);
            //var command = new RelayCommand(p =>
            //{
            //    new TransactionTest(_uiApp);
            //});
            //command.Execute(this);
            //m_ExEvent.Raise();
        }

    }
}
