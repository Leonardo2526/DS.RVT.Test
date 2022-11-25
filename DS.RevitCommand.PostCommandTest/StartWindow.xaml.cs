using Autodesk.Revit.UI;
using DS.RevitCommand.PostCommandTest.Models;
using System.Windows;

namespace DS.RevitCommand.PostCommandTest
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public NamesCollection MyObjects { get; set; } = new NamesCollection();

        private readonly UIApplication _uiApp;
        public StartWindow(UIApplication uiapp)
        {
            InitializeComponent();
            this.DataContext = this;
            _uiApp = uiapp;
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            string name = "1";
            MyObjects.Add(name);
            new TestClass(_uiApp).Run();
        }
    }
}
