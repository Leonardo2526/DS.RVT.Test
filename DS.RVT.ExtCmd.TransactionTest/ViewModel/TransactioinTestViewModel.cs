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
            //await new TrgBuilder(_transactionWindow, _doc).Build(() => _model.CreateRevitTask());
            await new TransactionGroupBuilder(_transactionWindow, _doc).BuildAsync(() => _model.Create(), true);

        });

        public ICommand CommitOld => new RelayCommand(async c =>
        {
            using (TransactionGroup trg = new TransactionGroup(_doc))
            {
                trg.Start();


                Task task = Task.Run(() =>
                {
                    while (true)
                    {
                        _model.Create();
                        //_uiDoc.RefreshActiveView();

                        OnWindowAsync(_transactionWindow).Wait();
                        //MessageBox.Show($"I was clicked at {DateTime.Now:HH:mm:ss.fffff}!\r\n");
                        break;
                    }
                });

                await task;

                if (trg.HasStarted() && _transactionWindow.IsActive)
                {
                    trg.RollBack();
                }
                else
                {
                    trg.Commit();
                }
            }

            //MessageBox.Show("Task completed!");
        });
        private Task ClickAsync(Button button1)
        {
            var tcs = new TaskCompletionSource<object>();
            void handler(object s, EventArgs e) => tcs.TrySetResult(null);
            button1.Click += handler;
            return tcs.Task.ContinueWith(_ => button1.Click -= handler);
        }

        private Task OnWindowAsync(TransactionWindow window)
        {
            var tcs = new TaskCompletionSource<object>();
            void handler(object s, EventArgs e) => tcs.TrySetResult(null);
            window.Closed += handler;
            window.RollBack.Click += handler;

            return tcs.Task.ContinueWith(_ =>
            {
                window.Closed -= handler;
                window.RollBack.Click -= handler;
            });
        }

        private Task EventAsync(object obj, string eventName)
        {
            var eventInfo = obj.GetType().GetEvent(eventName);
            var tcs = new TaskCompletionSource<object>();
            EventHandler handler = delegate (object s, EventArgs e) { tcs.TrySetResult(null); };
            eventInfo.AddEventHandler(obj, handler);
            return tcs.Task.ContinueWith(_ => eventInfo.RemoveEventHandler(obj, handler));
        }

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
