using DS.ClassLib.VarUtils.Events;
using DS.ClassLib.VarUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest.Model
{
    internal class Class1 : IEvent<EventType>
    {
        public event EventHandler<EventType> Event;

        public void Backward()
        {
            if (Event != null)
            {
                Event?.Invoke(this, EventType.Backward);
            }
        }

        public void Apply()
        {
            Event?.Invoke(this, EventType.Apply);
        }

        public void RollBack()
        {
            Event?.Invoke(this, EventType.Rollback);
        }

        public void Close()
        {
            Event?.Invoke(this, EventType.Close);
        }
    }
}
