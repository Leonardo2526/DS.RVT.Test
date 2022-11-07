using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.TaskEvents;
using global::DS.ClassLib.VarUtils;
using Revit.Async;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest
{
    /// <summary>
    /// An object to wrap transactions actions into transaction group with awaiting event.
    /// </summary>
    public class SleepTrgEventBuilder
    {
        private readonly Document _doc;
        private readonly Task _task;
        private readonly int _id;

        /// <summary>
        /// Create a new instance of object to wrap transactions actions into 
        /// transaction group with <paramref name="taskEvent"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="taskEvent"><see cref="WindowTaskEvent"/> to create a new event task.</param>
        public SleepTrgEventBuilder(Document doc, Task task, int id)
        {
            _doc = doc;
            _task = task;
            _id = id;
        }

        public SleepTrgEventBuilder(Document doc, int id)
        {
            _doc = doc;
            _id = id;
        }

        /// <summary>
        /// Create a new transaction group and build a new task inside it with <paramref name="operation"/>. 
        /// </summary>
        /// <param name="operation">Transactions to perform.</param>
        /// <param name="revitAsync">Optional parameter to perform <paramref name="operation"/> outside of Revit API context.</param>
        /// <returns>Returns a new async Task to perform transaction group operations.</returns>
        public void BuildAsync(Action operation, bool commit = false)
        {
            using (var trg = new TransactionGroup(_doc, $"{_id}"))
            {
                trg.Start($"{_id}");
                string startMes = $"trg {trg.GetName()} started.";
                Debug.Print(startMes);
                //TaskDialog.Show($"{GetType().Name}", $"trg {trg.GetName()} started");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //operation.Invoke();
                RevitTask.RunAsync(() => operation.Invoke());
                //Thread.Sleep(3000);

                stopwatch.Stop();

                string rollMes = null;
                if (commit)
                {
                    trg.Commit();
                    rollMes = $"trg {trg.GetName()} commited. Elapsed Time is {stopwatch.ElapsedMilliseconds} ms";
                }
                else
                {
                    trg.RollBack(); 
                    rollMes = $"trg {trg.GetName()} rolled. Elapsed Time is {stopwatch.ElapsedMilliseconds} ms";
                }
                Debug.Print(rollMes);

                //TaskDialog.Show($"{GetType().Name}", $"trg {trg.GetName()} rolled");
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

                    _task.Wait();
                    break;
                }
            });
            return task;
        }
    
    }
}


