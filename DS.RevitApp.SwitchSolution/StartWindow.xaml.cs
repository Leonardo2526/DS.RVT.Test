using System.Windows;

namespace DS.RevitApp.SwitchSolution
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public NamesCollection MyObjects { get; set; } = new NamesCollection();

        public StartWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            string name = "1";
            MyObjects.Add(name);
        }
    }
}
