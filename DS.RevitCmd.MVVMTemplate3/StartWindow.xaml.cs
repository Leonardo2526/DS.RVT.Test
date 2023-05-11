using System.Windows;

namespace DS.RevitCmd.MVVMTemplate3
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow(TestViewModel testViewModel)
        {
            InitializeComponent();
            this.DataContext = testViewModel;
        }
    }
}
