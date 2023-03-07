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
using System.Windows.Threading;

namespace DS.RevitApp.TransactionTest.ViewModel
{
    public static class Context
    {
        public static object StartContext { get; set; }
        public static object CurrentContext { get; set; }
        public static Dispatcher Dispatcher { get; set; }
        public static ExternalEvent ExternalEvent { get; set; }
        public static ExternalEventHandler EventHandler { get; set; }
    }


    public class TestWindowViewModel : INotifyPropertyChanged, IEvent<EventType>
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly TestWindow _window;
        private CancellationTokenSource _cancelTokenSource;
        private readonly TestClass _testedClass;

        public TestWindowViewModel(Document doc, UIDocument uiDoc, TestWindow window)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _window = window;
            _testedClass = new TestClass(_doc, _uiDoc);
        }


        public ICommand RunTest1 => new RelayCommand(c =>
        {
            ExternalEventHandler handler = new ExternalEventHandler(_window._uiapp);

            // External Event for the dialog to use (to post requests)
            //Context.EventHandler = handler;
            ExternalEventRequest request = new ExternalEventRequest();
           try
            {
                var ev = ExternalEvent.CreateJournalable(handler);
                var exEvent = ExternalEvent.Create(handler);
                exEvent.Raise();

            }
            catch (Exception)
            {

            }

            //Task task = Task.Run(() => { Context.EventHandler.Execute(_window._uiapp); });
            //task.Wait();
            //Context.EventHandler.Execute(_window._uiapp);
            //Context.ExternalEvent.Raise();

            object scheduler = SynchronizationContext.Current;
            Context.CurrentContext = scheduler;

            if(Context.StartContext == Context.CurrentContext) 
            { 

            }


            if (scheduler is null)
            {
                if (TaskScheduler.Current != TaskScheduler.Default)
                {
                    scheduler = TaskScheduler.Current;
                }
            }

            if (TaskScheduler.Current != TaskScheduler.Default)
            {
                scheduler = TaskScheduler.Current;
            }

            var cesp = new System.Threading.Tasks.ConcurrentExclusiveSchedulerPair();
            var b1 = TaskScheduler.Current == cesp.ExclusiveScheduler;
            var b2 = TaskScheduler.Current == cesp.ConcurrentScheduler;
            var b3 = TaskScheduler.Default == cesp.ExclusiveScheduler;
            var b4 = TaskScheduler.Default == cesp.ConcurrentScheduler;

            Dispatcher dispatcher = Context.Dispatcher;
            //Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            if (dispatcher != null)
            {
                dispatcher.VerifyAccess();
                dispatcher.CheckAccess();
                //Debug.WriteLine(dispatcher.VerifyAccess());
            }

            var mod = _doc.IsModifiable;

            var current = TaskScheduler.FromCurrentSynchronizationContext();
            if (current is not null)
            {
                var b11 = current == TaskScheduler.Current;
                var b12 = current == TaskScheduler.Default;
            }

            try
            {
                _testedClass.RunTransaction();
                Debug.WriteLine($"'{nameof(RunTest1)}' completed!\n");
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        });

        public ICommand RunTest2 => new RelayCommand(c =>
        {
            Debug.WriteLine($"\n'{nameof(RunTest2)}' started!");

            _cancelTokenSource = new CancellationTokenSource();
            _cancelTokenSource.CancelAfter(5000);
            var token = _cancelTokenSource.Token;

            _testedClass.RunWithCancelation(token);

            Debug.WriteLine($"'{nameof(RunTest2)}' completed!");
        });

        public ICommand RunTest3 => new RelayCommand(c =>
        {
            Debug.WriteLine($"\n'{nameof(RunTest3)}' started!");

            _cancelTokenSource = new CancellationTokenSource();
            _cancelTokenSource.CancelAfter(10000);
            var token = _cancelTokenSource.Token;

            _testedClass.RunWithDelay(token);

            Debug.WriteLine($"'{nameof(RunTest3)}' completed!");
        });

        public ICommand RunTest4=> new RelayCommand(async c =>
        {           
            Debug.WriteLine($"\n'{nameof(RunTest4)}' started!");

            _cancelTokenSource = new CancellationTokenSource();
            _cancelTokenSource.CancelAfter(10000);
            var token = _cancelTokenSource.Token;
            Task task = Task.Run(() =>
            {
                _testedClass.RunWithDelay(token);
            });
            await task;

            if (token.IsCancellationRequested) { return; }

            Debug.WriteLine($"'{nameof(RunTest4)}' completed!\n");
        });

        public ICommand RunTest5 => new RelayCommand(async c =>
        {
            Debug.IndentLevel = 1;

            Debug.WriteLine($"\n'{nameof(RunTest5)}' started!");

            _cancelTokenSource = new CancellationTokenSource();
            _cancelTokenSource.CancelAfter(10000);
            var token = _cancelTokenSource.Token;

            Task task = Task.Run(async () =>
            {
                await _testedClass.RunWithDelayAsync(token);
            });
            await task;
            //await _testedClass.RunWithDelayAsync(token);

            Debug.IndentLevel = 0;
            if (token.IsCancellationRequested) 
            { Debug.WriteLine($"'{nameof(RunTest5)}' was terminated!"); return; }

            Debug.WriteLine($"'{nameof(RunTest5)}' completed!");
        });

        public ICommand RunTest6 => new RelayCommand(async c =>
        {
            Debug.IndentLevel = 1;

            Debug.WriteLine($"\n'{nameof(RunTest6)}' started!");

            _cancelTokenSource = new CancellationTokenSource();
            _cancelTokenSource.CancelAfter(10000);
            var token = _cancelTokenSource.Token;


            Task task = Task.Run(async () =>
            {
                await _testedClass.TestNewBuilderAsync(token);
            });
            await task;

            //await _testedClass.TestNewBuilderAsync(token);

            Debug.IndentLevel = 0;
            if (token.IsCancellationRequested)
            { Debug.WriteLine($"'{nameof(RunTest6)}' was terminated!"); return; }

            Debug.WriteLine($"'{nameof(RunTest6)}' completed!");
        });

        public ICommand RunTest7 => new RelayCommand(async c =>
        {
            Debug.WriteLine($"\n'{nameof(RunTest7)}' started!");

            _cancelTokenSource = new CancellationTokenSource();
            _cancelTokenSource.CancelAfter(5000);
            var token = _cancelTokenSource.Token;
            Task task = Task.Run(() => _testedClass.RunWithCancelation(token));
            await task;

            Debug.WriteLine($"'{nameof(RunTest7)}' completed!");
        });

        public ICommand RunTest8 => new RelayCommand(async c =>
        {
            Debug.WriteLine($"\n'{nameof(RunTest8)}' started!");

            await RevitTask.RunAsync(() =>
            {
                Debug.Print("Start transaction.");
                Task.Delay(3000).Wait();
                Debug.Print("End transaction.");
            });

            Debug.WriteLine($"'{nameof(RunTest8)}' completed!");
        });


        public ICommand RunTest9 => new RelayCommand(c =>
        {
            Debug.WriteLine($"\n'{nameof(RunTest9)}' started!");

            Task task = Task.Run(() => RevitTask.RunAsync(() =>
            {
                Debug.Print("Start transaction.");
                Task.Delay(3000).Wait();
                Debug.Print("End transaction.");
            }));
            task.ContinueWith(t =>
            {
                Debug.WriteLine($"transaction completed!");
            });

            Debug.WriteLine($"'{nameof(RunTest9)}' completed!");
        });

        public ICommand StopTest => new RelayCommand(c =>
        {
            _cancelTokenSource.Cancel();
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
