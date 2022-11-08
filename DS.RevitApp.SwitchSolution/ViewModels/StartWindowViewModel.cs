using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.TaskEvents;
using DS.RevitApp.SwitchSolution.Models;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DS.RevitApp.SwitchSolution.ViewModel
{
    public class StartWindowViewModel : INotifyPropertyChanged, IEventHandler_1
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly TransactionModel _model;
        private readonly Window _transactionWindow;
        private List<List<XYZ>> _pointList { get; set; }
        private TrgEventBuilder_1 _builder;
        private bool WindowClosed = false;

        public StartWindowViewModel(Document doc, UIDocument uiDoc, Window transactionWindow)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _model = new TransactionModel(_doc, _uiDoc);
            _transactionWindow = transactionWindow;
        }

        public ICommand Start => new RelayCommand(async c =>
        {
            Debug.Print("\nCommand started");


            var points = new Points();
            _pointList = points.PointsLists;

            var taskEvent1 = new HandlerTaskEvent_1(this);

            _builder = new TrgEventBuilder_1(_doc, _uiDoc, 0, _model, _pointList);
            while (true && !WindowClosed)
            {               
                var taskEvent = new HandlerTaskEvent_1(this);
                await _builder.BuildAsync(() => _model.Create(points.PointsLists.ElementAt(_builder.IdCounter)), taskEvent, true);
            }

            Debug.Print("Command executed");
        });


        public event EventHandler BackwardHandler;
        public ICommand Backward => new RelayCommand(c =>
        {
            EventArgs eventArgs = null;
            BackwardHandler?.Invoke(this, eventArgs);
        });


        public event EventHandler OnwardHandler;
        public ICommand Onward => new RelayCommand(c =>
        {
            EventArgs eventArgs = null;
            OnwardHandler?.Invoke(this, eventArgs);
        });
        //}, o => _builder is null ? true : _builder?.IdCounter <= _pointList?.Count);


        public ICommand Apply => new RelayCommand(async c =>
        {
            
        });

        public event EventHandler CloseWindowHandler;
        public ICommand CloseWindow => new RelayCommand(c =>
        {
            WindowClosed = true;
            EventArgs eventArgs = null;
            CloseWindowHandler?.Invoke(this, eventArgs);
        });

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
