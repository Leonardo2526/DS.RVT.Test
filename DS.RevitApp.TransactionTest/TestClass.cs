using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.TransactionTest.Model;
using DS.RevitLib.Utils;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DS.RevitApp.TransactionTest
{
    internal class TestClass
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly ElementaryTransaction _transaction;

        public TestClass(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _transaction = new ElementaryTransaction(_doc, _uiDoc);
        }

        public void RunTransaction()
        {
            _transaction.RegenerateDocument();
        }

        /// <summary>
        /// Test with cancelation hanging transaction
        /// </summary>
        public void RunWithCancelation(CancellationToken token)
        {
            try
            {
                Debug.WriteLine($"Trying to commit transaction...");
                RevitTask.RunAsync(_transaction.RegenerateDocument).Wait(token);
            }
            catch (Exception)
            {
                Debug.WriteLine($"Transaction was canceled.");
            }
        }

        /// <summary>
        /// Async test without cancelation hanging transaction
        /// </summary>
        public async Task RunTransactionAsync()
        {
            try
            {
                Debug.WriteLine($"Trying to commit transaction...");
                await RevitTask.RunAsync(_transaction.RegenerateDocument);
            }
            catch (Exception)
            {
                Debug.WriteLine($"Transaction was canceled.");
            }
        }

        /// <summary>
        /// Test with delay
        /// </summary>
        public void RunWithDelay(CancellationToken token)
        {
            if (!RunDelay(token)) { return; }
            RunWithCancelation(token);
        }

        /// <summary> 
        /// Async test with delay
        /// </summary>
        public async Task RunWithDelayAsync(CancellationToken token)
        {
           if(!RunDelay(token)) { return; }
            await RunTransactionAsync();
        }

        /// <summary> 
        /// Async test with delay and new transactionBuilder.
        /// </summary>
        public async Task TestNewBuilderAsync(CancellationToken token)
        {
            if (!RunDelay(token)) { return; }
            await new TransactionBuilder(_doc).
                BuildRevitTask(_doc.Regenerate, "Regenerate document");
            //Action action = () =>
            //{
            //    var trModel = new TransactionModel(_doc, _uiDoc);
            //    trModel.RegenerateDocumentWithDelay(token);
            //};
            //await new NewTransactionBuilder(_doc).
            //   BuildRevitTask(action, "Regenerate document with delay");
        }

        private bool RunDelay(CancellationToken token)
        {
            //inititate delay
            Debug.WriteLine("Delay started.");
            try
            {
                Task.Delay(5000, token).Wait();
            }
            catch (Exception)
            {
                if (token.IsCancellationRequested)  // проверяем наличие сигнала отмены задачи
                {
                    Debug.WriteLine($"Delay was stopped.");
                    return false;     //  выходим из метода и тем самым завершаем задачу
                }
            }
            Debug.WriteLine("Delay completed.");
            return true;
        }

        public async Task RunMisc(CancellationToken token)
        {
            //await RunWithDelayAsync(token);
            //await RunTest2Async();
            //Task task = Task.Run(() => { RunTest3(token); });
            //await task;
            await RevitTask.RunAsync(_transaction.RegenerateDocument);
            //await RevitTask.RunAsync(() => RunTest3(token));
        }
    }
}
