using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Events;
using DS.RevitApp.SwitchSolution.Models;
using DS.RevitLib.Utils.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DS.RevitApp.SwitchSolution.ViewModel
{
    public class StartWindowViewModel : INotifyPropertyChanged, IEvent<EventType>
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly TransactionModel _model;
        private readonly Stack<List<XYZ>> _stackPoints = new();
        private List<List<XYZ>> _pointList = new();

        public StartWindowViewModel(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _model = new TransactionModel(_doc, _uiDoc);
        }

        public event EventHandler<EventType> Event;
        public event PropertyChangedEventHandler PropertyChanged;

        #region Commands

        public ICommand Start => new RelayCommand(async c =>
        {
            Debug.Print("\nCommand started");

            var points = new Points();
            _pointList = points.PointsLists;            

            TaskComplition taskEvent = null;
            var builder = new TrgEventBuilder(_doc);
            _stackPoints.Push(_pointList.First());
            while (true && taskEvent?.EventType != EventType.Close)
            {
                taskEvent = new TaskComplition(this);
                await builder.BuildAsync(() => _model.Create(_stackPoints.Peek()), taskEvent, true);
            }

            Debug.Print("Command executed");
        });

        public ICommand Backward => new RelayCommand(c =>
        {
            _stackPoints.Pop();
            Event.Invoke(this, EventType.Backward);
        }, o => _stackPoints.Count > 0);

        public ICommand Onward => new RelayCommand(c =>
        {
            _stackPoints.Push(_pointList.ElementAt(_stackPoints.Count));
            Event?.Invoke(this, EventType.Onward);
        }, o => _stackPoints.Count != _pointList.Count);

        public ICommand Apply => new RelayCommand(c =>
        {
            Event.Invoke(this, EventType.Apply);
        });

        public ICommand CloseWindow => new RelayCommand(c =>
        {
            Event?.Invoke(this, EventType.Close);
        });

        #endregion


        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

    }
}
