using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.SwitchSolution.Models
{
    public interface IEventHandler_1
    {
        public event EventHandler BackwardHandler;
        public event EventHandler OnwardHandler;
        public event EventHandler CloseWindowHandler;
    }

}
