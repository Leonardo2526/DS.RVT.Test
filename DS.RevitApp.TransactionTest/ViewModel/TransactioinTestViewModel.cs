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
using System.Security.Cryptography;
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

            Debug.Print("\nCommand started");
            Task task1 = Task.Run(async () =>
            {
            IWindowTaskEvent taskEvent = new HandlerTaskEvent(this);
                await new TrgEventBuilder_1(_doc, _uiDoc, taskEvent, 0).
                      BuildAsync(() => _model.Create(), false);

            });
            await task1;

            Debug.Print("Command executed");
        });

        public ICommand CommitOld => new RelayCommand(async c =>
        {
            //new RevitAsyncTest(_doc, _uiDoc).CreateTransaction();

            //var taskEvent = new WindowTaskEvent(_transactionWindow, new List<Button> { _transactionWindow.RollBack });
            //await new TrgEventBuilder(_doc, taskEvent).

            Debug.Print("\nCommand started");

            Task task1 = Task.Run(async () =>
            {
               await RunTransaction(1, 0, 4000);
            });
            Task task2 = Task.Run(async () =>
            {
                await RunTransaction(2, 5, 2000);
            });
            await task1;
            await task2;

            //IWindowTaskEvent taskEvent = new HandlerTaskEvent(this);

            //Debug.Print("Start Transaction");

            //using (var trg = new TransactionGroup(_doc, $"1"))
            //{
            //    trg.Start();
            //    await new TrgEventBuilder_1(_doc, taskEvent, 0).
            //        BuildAsync(() => _model.Create(), true);

            //    TrgCommitter(trg, taskEvent);
            //}

            Debug.Print("Command executed");
        });

        private async Task RunTransaction(int id, int offset, int sleepTime)
        {
            Debug.Print($"task {id} started");

            using (var trg = new TransactionGroup(_doc, $"1"))
            {
                await RevitTask.RunAsync(() =>
                {
                    trg.Start();

                    Debug.Print($"transaction {id} started");

                    _model.Create(offset);
                    _uiDoc.RefreshActiveView();

                    Debug.Print($"Thread {id} sleeped.");
                    Thread.Sleep(sleepTime);
                    Debug.Print($"Thread {id} waked.");

                    Debug.Print($"transaction {id} executed");

                    if (trg.HasStarted())
                    {
                        trg.RollBack();
                    }
                });
            }
        }

        /// <summary>
        /// Perform action to close transaction group.
        /// </summary>
        /// <param name="trg">Current opened transaction group.</param>
        private void TrgCommitter(TransactionGroup trg, IWindowTaskEvent taskEvent)
        {
            if (trg.HasStarted() && !taskEvent.WindowClosed)
            {
                trg.RollBack();
                //TaskDialog.Show($"{GetType().Name}", $"trg {_id} rolled");
            }
            else if (trg.HasStarted() && taskEvent.WindowClosed)
            {
                trg.Commit();
                //TaskDialog.Show($"{GetType().Name}", $"trg {_id} committed");
            }
            else
            {
                TaskDialog.Show($"{GetType().Name}", "trg is not closed due to it hasn't been started.");
            }
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
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
