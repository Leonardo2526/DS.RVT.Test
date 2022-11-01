using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.TransactionTest.View;
using Revit.Async;
using System;
using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest
{
    /// <summary>
    /// An object to wrap transaction actions to transaction group.
    /// </summary>
    internal class TransactionGroupBuilder
    {
        private readonly TransactionWindow _transactionWindow;
        private readonly Document _doc;
        private bool _windowClosed = false;

        /// <summary>
        /// Create a new instance of object to handle transaction group commit result
        /// with window's controls.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="doc"></param>
        public TransactionGroupBuilder(TransactionWindow window, Document doc)
        {
            _transactionWindow = window;
            _doc = doc;
        }

        /// <summary>
        /// Create a new transaction group and build a new task inside it with <paramref name="operation"/>. 
        /// </summary>
        /// <param name="operation">Transaction to perform.</param>
        /// <param name="revitAsync">Optional parameter to perform <paramref name="operation"/> outside of Revit API context.</param>
        /// <returns>Returns a new async Task to perform transaction group operations.</returns>
        public async Task BuildAsync(Action operation, bool revitAsync = false)
        {
            using (var trg = new TransactionGroup(_doc))
            {
                trg.Start();
                Task task = CreateTask(operation, revitAsync);
                await task;

                //transacion group committer
                if (trg.HasStarted() && !_windowClosed)
                {
                    trg.RollBack();
                    TaskDialog.Show($"{GetType().Name}", "trg rolled");
                }
                else if (trg.HasStarted() && _windowClosed)
                {
                    trg.Commit();
                    TaskDialog.Show($"{GetType().Name}", "trg committed");
                }
                else
                {
                    TaskDialog.Show($"{GetType().Name}", "trg is not closed due to it hasn't been started.");
                }
            }
        }


        private Task CreateTask(Action operation, bool wrapRevitAsync = false)
        {
            Task task = Task.Run(() =>
            {
                while (true)
                {
                    if (wrapRevitAsync)
                    {
                        RevitTask.RunAsync(() => operation.Invoke());
                    }
                    else
                    {
                        operation.Invoke();
                    }

                    GetTaskHandler(_transactionWindow).Wait();
                    break;
                }
            });
            return task;
        }

        private Task GetTaskHandler(TransactionWindow window)
        {
            var tcs = new TaskCompletionSource<object>();
            void windhandler(object s, EventArgs e)
            {
                _windowClosed = true;
                tcs.TrySetResult(null);
            };
            void handler(object s, EventArgs e) => tcs.TrySetResult(null);

            window.Closed += windhandler;
            window.RollBack.Click += handler;

            return tcs.Task.ContinueWith(_ =>
            {
                window.Closed -= windhandler;
                window.RollBack.Click -= handler;
            });
        }

    }
}
