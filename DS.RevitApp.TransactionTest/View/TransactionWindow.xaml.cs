using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using DS.RevitApp.TransactionTest.ViewModel;
using System;
using System.Windows;

namespace DS.RevitApp.TransactionTest.View
{
    /// <summary>
    /// Interaction logic for TransactionWindow.xaml
    /// </summary>
    public partial class TransactionWindow : Window
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly UIApplication _uiapp;
        private readonly TransactionGroup _trg;

        public TransactionWindow(Document doc, UIDocument uiDoc, UIApplication uiapp)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _uiapp = uiapp;

            InitializeComponent();
            DataContext = new TransactioinTestViewModel(doc, uiDoc);
        }

        private void OnIdling(object sender, IdlingEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RollBack_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
