using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.SwitchSolution.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace DS.RevitApp.SwitchSolution
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly UIApplication _uiapp;

        public StartWindow(Document doc, UIDocument uiDoc, UIApplication uiapp)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _uiapp = uiapp;

            InitializeComponent();
            DataContext = new StartWindowViewModel(doc, uiDoc);
        }
    }
}
