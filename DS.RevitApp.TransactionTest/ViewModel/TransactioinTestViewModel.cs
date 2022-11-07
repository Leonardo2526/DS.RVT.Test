using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.TaskEvents;
using DS.RevitApp.TransactionTest.Model;
using DS.RevitApp.TransactionTest.View;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DS.RevitApp.TransactionTest.ViewModel
{
    public class TransactioinTestViewModel : INotifyPropertyChanged, IEventHandler
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
            new RevitAsyncTest(_doc, _uiDoc).CreateTransaction();

            //var taskEvent = new WindowTaskEvent(_transactionWindow, new List<Button> { _transactionWindow.RollBack });
            //await new TrgEventBuilder(_doc, taskEvent).

            //IWindowTaskEvent taskEvent = new HandlerTaskEvent(this);
            //await new TrgEventBuilder(_doc, taskEvent, 0).
            //BuildAsync(() => _model.Create(), true);

        });

        public event EventHandler RollBackHandler;
        public ICommand RollBack => new RelayCommand(c =>
        {
            EventArgs eventArgs = null;
            RollBackHandler?.Invoke(this, eventArgs);
        });


        public event EventHandler CloseWindowHandler;
        public ICommand CloseWindow => new RelayCommand(c =>
        {
            EventArgs eventArgs = null;
            CloseWindowHandler?.Invoke(this, eventArgs);
            //TaskDialog.Show("revit", "CloseWindows");
        });


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
