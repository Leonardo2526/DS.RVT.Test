using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Events;
using DS.RevitApp.TransactionTest.Model;
using DS.RevitApp.TransactionTest.View;
using DS.RevitLib.Utils.Transactions;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DS.RevitApp.TransactionTest.ViewModel
{
    public class TransactioinTestViewModel : INotifyPropertyChanged, IEvent<EventType>
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

        public ICommand CommitOld => new RelayCommand(async c =>
        {

            Debug.Print("\nCommand started");
            Task task1 = Task.Run(async () =>
            {
                var taskEvent = new TaskComplition(this);
                await new TrgEventBuilder(_doc).
                      BuildAsync(() => _model.Create(), taskEvent, false);

            });
            await task1;

            Debug.Print("Command executed");
        });

        public ICommand Commit => new RelayCommand(async c =>
        {
            Debug.Print($"Command started in thread {Thread.CurrentThread.ManagedThreadId}");

            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select MEPCurve");
            MEPCurve mEPCurve = _doc.GetElement(reference) as MEPCurve;

            var obj = new Class1(mEPCurve);
            await RunDeleteTransactionAsync(mEPCurve, obj);

            Debug.Print("Command executed");
        });

        private async Task RunDeleteTransactionAsync(MEPCurve mEPCurve, Class1 class1)
        {
            Task task = Task.Run(() =>
            {
               RunDeleteTransaction(mEPCurve, class1);
            });
            await task;
        }

        private void RunDeleteTransaction(MEPCurve mEPCurve, Class1 class1)
        {
            RevitTask.RunAsync(() =>
            {
                using (var trg = new TransactionGroup(_doc, $"delete elem"))
                {
                    trg.Start();
                    Debug.Print($"TransactionGroup '{trg.GetName()}' started in thread {Thread.CurrentThread.ManagedThreadId}");

                    using (var tr = new Transaction(_doc, "del"))
                    {
                        tr.Start();
                        _doc.Delete(mEPCurve.Id);
                        tr.Commit();
                        _uiDoc.RefreshActiveView();
                    }

                    Debug.Print($"{mEPCurve.IsValidObject}, {class1.MEPCurve.IsValidObject}");

                    if (trg.HasStarted())
                    {
                        trg.RollBack();
                        Debug.Print($"TransactionGroup '{trg.GetName()}' rolled in thread {Thread.CurrentThread.ManagedThreadId}");
                    }
                    Debug.Print($"{mEPCurve.IsValidObject}, {class1.MEPCurve.IsValidObject}");
                }

                Debug.Print($"{mEPCurve.IsValidObject}, {class1.MEPCurve.IsValidObject}");
            });
        }



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
        public event EventHandler<EventType> Event;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
