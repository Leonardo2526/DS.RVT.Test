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
    public class TestWindowViewModel : INotifyPropertyChanged
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

        //valid async
        public ICommand RunTest1 => new RelayCommand(async c =>
        {
            Debug.WriteLine($"\n'{nameof(RunTest1)}' started!");

            await RevitTask.RunAsync(() => _testedClass.RunTransaction());

            Debug.Unindent();
            Debug.WriteLine($"'{nameof(RunTest1)}' completed!\n");

        });

        //valid async
        public ICommand RunTest2 => new RelayCommand(async c =>
        {
            Debug.WriteLine($"\n'{nameof(RunTest2)}' started!");

            Task task = Task.Run(async () =>
            {
                await Task.Delay(1000);
               await RevitTask.RunAsync(() => _testedClass.RunTransaction());
            });
            await task;


            Debug.Unindent();
            Debug.WriteLine($"'{nameof(RunTest2)}' completed!\n");
        });

        //Sync. Transaction will be performed at the end.
        public ICommand RunTest3 => new RelayCommand(c =>
        {
            Debug.WriteLine($"\n'{nameof(RunTest3)}' started!");

            //not awaited!
            RevitTask.RunAsync(() => _testedClass.RunTransaction());

            Debug.Unindent();
            Debug.WriteLine($"'{nameof(RunTest3)}' completed!\n");
        });

        public ICommand RunTest4 => new RelayCommand(async c =>
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
