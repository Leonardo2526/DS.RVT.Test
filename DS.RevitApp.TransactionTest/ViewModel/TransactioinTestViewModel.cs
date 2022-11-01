using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitApp.TransactionTest.Model;
using DS.RevitApp.TransactionTest.View;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace DS.RevitApp.TransactionTest.ViewModel
{
    internal class TransactioinTestViewModel : INotifyPropertyChanged
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly TransactionModel _model;
        private readonly TransactionWindow _transactionWindow;

        public TransactioinTestViewModel(Document doc, UIDocument uiDoc, TransactionWindow transactionWindow)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _model = new TransactionModel(_doc, _uiDoc);
            _transactionWindow = transactionWindow;
        }

        public ICommand Commit => new RelayCommand(async c =>
        {
            var taskEvent = new WindowTaskEvent(_transactionWindow, new List<Button> { _transactionWindow.RollBack });
            await new TrgEventBuilder(_doc, taskEvent).
            BuildAsync(() => _model.Create(), true);

        });

        public ICommand RollBack => new RelayCommand(c =>
        {
            //roll = true;
            //_exEvent.Raise();
            //Trg.RollBack();
        });


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
