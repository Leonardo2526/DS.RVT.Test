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
        private readonly Stack<List<XYZ>> _stackPoints;
        private List<List<XYZ>> _pointList;

        public StartWindowViewModel(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _model = new TransactionModel(_doc, _uiDoc);
            _stackPoints = new();
            _pointList = new();
        }

        public event EventHandler<EventType> Event;
        public event PropertyChangedEventHandler PropertyChanged;

        private int _currentSolutionInd;
        public int CurrentSolutionInd
        {
            get { return _currentSolutionInd; }
            set { _currentSolutionInd = value; OnPropertyChanged("CurrentSolutionInd"); }
        }

        private int _solutionsCount;
        public int SolutionsCount
        {
            get { return _solutionsCount; }
            set { _solutionsCount = value; OnPropertyChanged("SolutionsCount"); }
        }

        #region Commands

        public ICommand Start => new RelayCommand(async c =>
        {
            Debug.Print("\nCommand started");

            var points = new Points();
            _pointList = points.PointsLists;
            SolutionsCount = points.PointsLists.Count;

            TaskComplition taskEvent = null;
            var builder = new TrgEventBuilder(_doc);
            _stackPoints.Push(_pointList.First());
            CurrentSolutionInd = _stackPoints.Count;

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
            CurrentSolutionInd = _stackPoints.Count;
            Event.Invoke(this, EventType.Backward);
        }, o => _stackPoints.Count > 1);

        public ICommand Onward => new RelayCommand(c =>
        {
            _stackPoints.Push(_pointList.ElementAt(_stackPoints.Count));
            CurrentSolutionInd = _stackPoints.Count;
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
