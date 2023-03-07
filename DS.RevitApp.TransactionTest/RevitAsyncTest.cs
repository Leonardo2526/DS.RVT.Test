using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.TransactionTest.Model;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest
{
    internal class RevitAsyncTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public RevitAsyncTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public void CreateTransaction_DeadLock()
        {
            var trModel = new TransactionModel(_doc, _uiDoc);

            Debug.Print("Start transaction.");
            Task task = Task.Run(() => RevitTask.RunAsync(() => trModel.Create(0, "line1")));
            task.Wait();

            _uiDoc.RefreshActiveView();
            Debug.Print("Transaction executed.");

            Debug.Print("Start sleeping.");
            Thread.Sleep(3000);
            Debug.Print("End of sleeping.");

            Debug.Print("End of method.");

        }

        public void CreateTransaction()
        {
            var trModel = new TransactionModel(_doc, _uiDoc);


            Task task = Task.Run(() => RevitTask.RunAsync(() =>
            {
                Debug.Print("Start transaction.");
                Task.Delay(3000).Wait();
                trModel.Create(0, "line1");
            }
            ));
            task.ContinueWith(t =>
            {
                _uiDoc.RefreshActiveView();
                Debug.Print("Transaction executed.");

                Debug.Print("Start sleeping.");
                Thread.Sleep(3000);
                Debug.Print("End of sleeping.");

                Debug.Print("End of method.");

            });
        }

        public void RunRevitTask()
        {
            var trModel = new TransactionModel(_doc, _uiDoc);


           var task = RevitTask.RunAsync(() =>
            {
                Debug.Print("Start transaction.");
                Task.Delay(3000).Wait();
                trModel.Create(0, "line1");
            });
            task.ContinueWith(t =>
            {
                _uiDoc.RefreshActiveView();
                Debug.Print("Transaction executed.");

                Debug.Print("Start sleeping.");
                Thread.Sleep(3000);
                Debug.Print("End of sleeping.");

                Debug.Print("End of method.");

            });
        }

        public async Task CreateTransactionAsync()
        {
            var trModel = new TransactionModel(_doc, _uiDoc);

            Debug.Print("Start transaction.");
            await RevitTask.RunAsync(() => trModel.Create(0, "line1"));
            _uiDoc.RefreshActiveView();
            Debug.Print("Transaction executed.");

            Debug.Print("Start sleeping.");
            Thread.Sleep(3000);
            Debug.Print("End of sleeping.");

            Debug.Print("End of method.");
        }
    }
}
