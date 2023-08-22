using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Events;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest.Model
{
    /// <summary>
    /// An object to wrap transactions actions into transaction group with awaiting event.
    /// </summary>
    public class TrgEventBuilder_1
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly Task _task;
        private readonly TaskComplition _taskEvent;
        private readonly int _id;

        /// <summary>
        /// Create a new instance of object to wrap transactions actions into 
        /// transaction group with <paramref name="taskEvent"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="taskEvent"><see cref="WindowTaskEvent"/> to create a new event task.</param>
        public TrgEventBuilder_1(Document doc, UIDocument uiDoc, TaskComplition taskEvent, int id)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _taskEvent = taskEvent;
            _id = id;
            _task = taskEvent.Create();
        }

        /// <summary>
        /// Create a new transaction group and build a new task inside it with <paramref name="operation"/>. 
        /// </summary>
        /// <param name="operation">Transactions to perform.</param>
        /// <param name="revitAsync">Optional parameter to perform <paramref name="operation"/> outside of Revit API context.</param>
        /// <returns>Returns a new async Task to perform transaction group operations.</returns>
        public async Task BuildAsync(Action operation, bool revitAsync = false)
        {
            using (var trg = new TransactionGroup(_doc, $"{_id}"))
            {
                await RevitTask.RunAsync(async () =>
                {
                    trg.Start();
                    var st = trg.GetStatus();
                    await CreateTask(operation, revitAsync);
                    TrgCommitter(trg);
                });
            }
        }

        public async Task BuildAsync2(Action operation, bool revitAsync = false)
        {
            using (var trg = new TransactionGroup(_doc, $"{_id}"))
            {
                trg.Start();
                var st = trg.GetStatus();
                await CreateTask(operation, revitAsync);
                TrgCommitter(trg);
            }
        }

        public async Task BuildAsync1(Action operation, bool revitAsync = false)
        {
            using (var trg = new TransactionGroup(_doc, $"{_id}"))
            {
                trg.Start();
                var st = trg.GetStatus();
                Task transactionTask = CreateTaskAsync(operation, revitAsync);
                await transactionTask;
                TrgCommitter(trg);
            }
        }

        private async Task CreateTask(Action operation, bool wrapRevitAsync = false)
        {
            
                if (wrapRevitAsync)
                {
                    await RevitTask.RunAsync(() => operation.Invoke());
                }
                else
                {
                    operation.Invoke();
                }

                _uiDoc.RefreshActiveView();
                Debug.Print($"await action task {Task.CurrentId} started.");
                _task.Wait();
                //await task;
                Debug.Print($"Task {Task.CurrentId} executed.");               
            }
       

        private Task CreateTaskAsync(Action operation, bool wrapRevitAsync = false)
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
            if (trg.HasStarted() && _taskEvent.EventType == EventType.Rollback )
            {
                trg.RollBack();
                TaskDialog.Show($"{GetType().Name}", $"trg {_id} rolled");
            }
            else if (trg.HasStarted() && _taskEvent.EventType == EventType.Close)
            {
                trg.Commit();
                TaskDialog.Show($"{GetType().Name}", $"trg {_id} committed");
            }
            else if (trg.HasStarted() && _taskEvent.EventType == EventType.Apply)
            {
                trg.Commit();
                TaskDialog.Show($"{GetType().Name}", $"trg {_id} committed");
            }
            else
            {
                TaskDialog.Show($"{GetType().Name}", "trg is not closed due to it hasn't been started.");
            }
        }
    }

}
