using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Events;
using DS.RevitApp.TransactionTest.Model;
using DS.RevitApp.TransactionTest.View;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.Transactions;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DS.RevitApp.TransactionTest.ViewModel
{
    public class TransactioinTestViewModel : INotifyPropertyChanged, IEvent<EventType>
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly TransactionModel _model;
        private readonly TransactionWindow _transactionWindow;
        private NodeElement _spud;

        public TransactioinTestViewModel(Document doc, UIDocument uiDoc, TransactionWindow transactionWindow)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _model = new TransactionModel(_doc, _uiDoc);
            _transactionWindow = transactionWindow;
        }

        public ICommand Commit => new RelayCommand(async c =>
        {
            Debug.Print($"Command started in thread {Thread.CurrentThread.ManagedThreadId}");

            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select MEPCurve");
            MEPCurve mEPCurve = _doc.GetElement(reference) as MEPCurve;

            var mEPSystemBuilder = new SimpleMEPSystemBuilder(mEPCurve);
            var sourceMEPModel = mEPSystemBuilder.Build();
            _spud = sourceMEPModel.Root.ChildrenNodes.First();


            await RunTransactionAsync(mEPCurve);

            var elements = sourceMEPModel.AllElements;
            foreach (var elem in elements)
            {
                if (!elem.IsValidObject)
                {
                    Debug.WriteLine($"{elem.Id} is not valid");
                    break;
                }
            }


            Debug.Print("Command executed");
        });

        private async Task RunTransactionAsync(MEPCurve mEPCurve)
        {
            Task task = Task.Run(() =>
            {
                RunSplitTransaction(mEPCurve);
               //RunDeleteTransaction(mEPCurve);
            });
            await task;
        }

        private void RunDeleteTransaction(MEPCurve mEPCurve)
        {
            RevitTask.RunAsync(() =>
            {
                using (var trg = new TransactionGroup(_doc, $"delete elem"))
                {
                    trg.Start();
                    Debug.Print($"TransactionGroup '{trg.GetName()}' started in thread {Thread.CurrentThread.ManagedThreadId}");

                    using (var tr = new Transaction(_doc, "del"))
                    {
                        tr.Start();
                        _doc.Delete(mEPCurve.Id);
                        tr.Commit();
                        _uiDoc.RefreshActiveView();
                    }

                    Debug.Print($"MEPCurve - {mEPCurve.IsValidObject}, spud - {_spud.Element.IsValidObject}");

                    if (trg.HasStarted())
                    {
                        trg.RollBack();
                        Debug.Print($"TransactionGroup '{trg.GetName()}' rolled in thread {Thread.CurrentThread.ManagedThreadId}");
                    }
                    Debug.Print($"MEPCurve - {mEPCurve.IsValidObject}, spud - {_spud.Element.IsValidObject}");
                }

                Debug.Print($"MEPCurve - {mEPCurve.IsValidObject}, spud - {_spud.Element.IsValidObject}");
            });
        }

        private void RunSplitTransaction(MEPCurve mEPCurve)
        {
            RevitTask.RunAsync(() =>
            {
                using (var trg = new TransactionGroup(_doc, $"split elem"))
                {
                    trg.Start();
                    Debug.Print($"TransactionGroup '{trg.GetName()}' started in thread {Thread.CurrentThread.ManagedThreadId}");

                    using (var tr = new Transaction(_doc, "del"))
                    {
                        tr.Start();
                        XYZ point = mEPCurve.GetCenterPoint();
                        mEPCurve.Split(point);
                        tr.Commit();
                        _uiDoc.RefreshActiveView();
                    }

                    Debug.Print($"MEPCurve - {mEPCurve.IsValidObject}, spud - {_spud.Element.IsValidObject}");

                    if (trg.HasStarted())
                    {
                        trg.Commit();
                        //trg.RollBack();
                        Debug.Print($"TransactionGroup '{trg.GetName()}' rolled in thread {Thread.CurrentThread.ManagedThreadId}");
                    }
                    Debug.Print($"MEPCurve - {mEPCurve.IsValidObject}, spud - {_spud.Element.IsValidObject}");
                }

                Debug.Print($"MEPCurve - {mEPCurve.IsValidObject}, spud - {_spud.Element.IsValidObject}");
            });
        }

        public event EventHandler RollBackHandler;
        public ICommand RollBack => new RelayCommand(c =>
        {
            EventArgs eventArgs = null;
            RollBackHandler?.Invoke(this, eventArgs);
        });


        public event EventHandler CloseWindowHandler;
        public ICommand CloseWindow => new RelayCommand(c =>
        {
            EventArgs eventArgs = null;
            CloseWindowHandler?.Invoke(this, eventArgs);
            //TaskDialog.Show("revit", "CloseWindows");
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
