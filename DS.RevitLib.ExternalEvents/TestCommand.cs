using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.ExternalEvents
{
    internal class TestCommand : IExternalEventHandler
    {
        private readonly ExternalEvent _externalEvent;
        private Action _action;

        public TestCommand()
        {
            _externalEvent = ExternalEvent.Create(this);
        }

        public void Execute(UIApplication app)
        {
            _action();
        }

        public string GetName()
        {
            return "External Event Example";
        }

        public void Run(Action action)
        {
            _action = action;
            _externalEvent.Raise();
        }
    }
}
