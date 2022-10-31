using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.TransactionTest.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DS.RevitApp.TransactionTest.View
{
    /// <summary>
    /// Interaction logic for TransactionWindow.xaml
    /// </summary>
    public partial class TransactionWindow : Window
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public TransactionWindow(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;

            InitializeComponent();
            DataContext = new TransactioinTestViewModel(doc, uiDoc);
        }
    }
}
