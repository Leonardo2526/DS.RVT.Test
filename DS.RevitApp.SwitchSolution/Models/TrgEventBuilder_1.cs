using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.TaskEvents;
using Revit.Async;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DS.RevitApp.SwitchSolution.Models
{
    /// <summary>
    /// An object to wrap transactions actions into transaction group with awaiting event.
    /// </summary>
    internal class TrgEventBuilder_1
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private Task _task;
        private HandlerTaskEvent_1 _taskEvent;
        private readonly int _id;
        private readonly TransactionModel _model;
        private readonly List<List<XYZ>> _pointsList;
        public int IdCounter { get; private set; } = 0;  

        /// <summary>
        /// Create a new instance of object to wrap transactions actions into 
        /// transaction group with <paramref name="taskEvent"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="taskEvent"><see cref="WindowTaskEvent"/> to create a new event task.</param>
        public TrgEventBuilder_1(Document doc, UIDocument uiDoc, int id, TransactionModel model, List<List<XYZ>> pointsList)
        {
            _doc = doc;
            _id = id;
            _model = model;
            _pointsList = pointsList;
            _uiDoc = uiDoc;
        }

        /// <summary>
        /// Create a new transaction group and build a new task inside it with <paramref name="operation"/>. 
        /// </summary>
        /// <param name="operation">Transactions to perform.</param>
        /// <param name="revitAsync">Optional parameter to perform <paramref name="operation"/> outside of Revit API context.</param>
        /// <returns>Returns a new async Task to perform transaction group operations.</returns>
        public async Task BuildAsync(Action operation, HandlerTaskEvent_1 taskEvent, bool revitAsync = false)
        {
            _taskEvent = taskEvent;
            _task = taskEvent.Create();

            Debug.Print($"task {_task.Id} to wait event created.");

            using (var trg = new TransactionGroup(_doc, $"{IdCounter}"))
            {
                trg.Start();
                Debug.Print($"\nTransactionGroup {trg.GetName()} started");

                Task transactionTask = CreateTask(operation, revitAsync);
                await transactionTask;

                TrgCommitter(trg);
            }

            Debug.Print($"task {_task.Id} to wait event complete status: {_task.IsCompleted}.");
            Debug.Print($"BuildAsync executed.");
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

                    _task.Wait();
                    break;
                }
            });
            return task;
        }

        /// <summary>
        /// Perform action to close transaction group.
        /// </summary>
        /// <param name="trg">Current opened transaction group.</param>
        private void TrgCommitter(TransactionGroup trg)
        {
            if (trg.HasStarted() && _taskEvent.Onward)
            {
                IdCounter++;
                trg.RollBack();
                Debug.Print($"TransactionGroup {trg.GetName()} rolled");

                //_task = _taskEvent.Create();

                //List<XYZ> currentList = _pointsList.ElementAt(IdCounter);
                //BuildAsync(() => _model.Create(currentList));
            }       
            else if (trg.HasStarted() && _taskEvent.Backward)
            {
                IdCounter--;
                trg.RollBack();
                Debug.Print($"TransactionGroup {trg.GetName()} rolled");
            }
            else if (trg.HasStarted() && _taskEvent.WindowClosed)
            {
                trg.Commit();
                Debug.Print($"TransactionGroup {trg.GetName()} committed");
            }
            else
            {
                TaskDialog.Show($"{GetType().Name}", "trg is not closed due to it hasn't been started.");
            }
        }
    }
}
