using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS.RevitLib.ExternalEvents
{
    public partial class ExternalEventExampleDialog : Form
    {
        private ExternalEvent m_ExEvent;
        private ExternalEventExample m_Handler;

        public ExternalEventExampleDialog(ExternalEvent exEvent, ExternalEventExample handler)
        {
            InitializeComponent();
            m_ExEvent = exEvent;
            m_Handler = handler;
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
            m_ExEvent.Raise();
        }

    }
}
