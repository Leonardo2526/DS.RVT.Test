using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.TransactionTest.ViewModel;
using System.Windows;

namespace DS.RevitApp.TransactionTest.View
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        public readonly UIApplication _uiapp;

        public TestWindow(Document doc, UIDocument uiDoc, UIApplication uiapp)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _uiapp = uiapp;
            InitializeComponent();
            DataContext = new TestWindowViewModel(doc, uiDoc, this);
        }
    }
}
