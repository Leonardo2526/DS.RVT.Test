using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.TransactionTest.Model;
using DS.RevitApp.TransactionTest.View;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
