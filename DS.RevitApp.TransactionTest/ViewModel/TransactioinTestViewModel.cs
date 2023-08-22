using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitApp.TransactionTest.Model;
using DS.RevitLib.Utils.Elements;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DS.RevitApp.TransactionTest.ViewModel
{
    public class TransactioinTestViewModel
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly TransactionModel _model;

        public TransactioinTestViewModel(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _model = new TransactionModel(_doc, _uiDoc);
        }


        public ICommand Commit => new RelayCommand(async c =>
        {
            await CreateTransactionAsync();
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


        public ICommand Apply => new RelayCommand(c =>
        {
            _model?.Apply();
        });

        public ICommand RollBack => new RelayCommand(c =>
        {
            _model?.RollBack();
        });

        public ICommand CloseWindow => new RelayCommand(c =>
        {
            _model?.Close();
        });


        public async Task CreateTransactionAsync()
        {
            Debug.Print("\nCommand started");

            List<Element> docElements = new List<Element>();
            Dictionary<RevitLinkInstance, List<Element>> linkElementsDict = new Dictionary<RevitLinkInstance, List<Element>>();
            (docElements, linkElementsDict) = new ElementsExtractor(_doc).GetAll();

            Debug.WriteLine("docElements: " + docElements.Count());

            var elements = _model.MEPSystem.AllElements.Where(obj => obj.IsValidObject).ToList();
            Debug.WriteLine("validElements: " + elements.Count());
            //try
            //{
            //}
            //catch (System.Exception ex)
            //{
            //    Debug.WriteLine(ex);
            //}

            await _model.DeleteElementsAsync();


            elements = elements.Where(obj => obj.IsValidObject).ToList();
            Debug.WriteLine("validElements: " + elements.Count());

            //(docElements, linkElementsDict) = new ElementsExtractor(_doc).GetAll();
            docElements = docElements.Where(obj => obj.IsValidObject).ToList();
            Debug.WriteLine("docElements: " + docElements.Count());

            Debug.Print("Command executed");
        }

        public void CreateTransaction(Document doc, UIApplication uiapp)
        {
            Debug.Print("\nCommand started");

            List<Element> docElements = new List<Element>();
            Dictionary<RevitLinkInstance, List<Element>> linkElementsDict = new Dictionary<RevitLinkInstance, List<Element>>();
            (docElements, linkElementsDict) = new ElementsExtractor(_doc).GetAll();

            docElements = docElements.Where(obj => obj.IsValidObject).ToList();
            Debug.WriteLine("docElements: " + docElements.Count());

            //var elements = _model.MEPSystem.AllElements.Where(obj => obj.IsValidObject).ToList();
            //Debug.WriteLine("validElements: " + elements.Count());
            //try
            //{
            //}
            //catch (System.Exception ex)
            //{
            //    Debug.WriteLine(ex);
            //}

            //_model.DeleteElementsWtihSingleTransactionAndDisconnect(docElements);
            //_model.DeleteElementsWithDisconnect(docElements);
            _model.DeleteElementsWtihSingleTransaction(docElements);

            SynchronizeWithCentralWindow(doc, uiapp);

            Debug.WriteLine("Transaction was rolled back");
            //elements = elements.Where(obj => obj.IsValidObject).ToList();
            //Debug.WriteLine("validElements: " + elements.Count());

            //(docElements, linkElementsDict) = new ElementsExtractor(_doc).GetAll();
            docElements = docElements.Where(obj => obj.IsValidObject).ToList();
            Debug.WriteLine("docElements: " + docElements.Count());

            Debug.Print("Command executed");
        }

        public void SynchronizeWithCentralWindow(Document doc, UIApplication uiapp)
        {
            var syncCmd = RevitCommandId.LookupPostableCommandId(PostableCommand.Undo);
            try
            {
                uiapp.PostCommand(syncCmd);

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
