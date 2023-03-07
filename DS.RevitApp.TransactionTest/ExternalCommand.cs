using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.Test;
using DS.RevitApp.TransactionTest.View;
using DS.RevitApp.TransactionTest.ViewModel;
using Revit.Async;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DS.RevitApp.TransactionTest
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

          

            return Autodesk.Revit.UI.Result.Succeeded;

            ExternalEventHandler handler = new ExternalEventHandler(uiapp);

            // External Event for the dialog to use (to post requests)
            Context.EventHandler = handler;
            Context.ExternalEvent = ExternalEvent.Create(handler);

            object scheduler = SynchronizationContext.Current;
            Context.StartContext = scheduler;
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

            Context.Dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            if (Context.Dispatcher != null)
            {
                Context.Dispatcher.VerifyAccess();
                Context.Dispatcher.CheckAccess();
                //Debug.WriteLine(dispatcher.VerifyAccess());
            }
            var mod = doc.IsModifiable;

            //Context.EventHandler.Execute(uiapp);

            //Context.ExternalEvent.Raise();
            //Context.ExternalEvent.Raise();

            var startWindow = new TestWindow(doc, uidoc, uiapp);
            //var startWindow = new TransactionWindow(doc, uidoc, uiapp);
            startWindow.Show();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}