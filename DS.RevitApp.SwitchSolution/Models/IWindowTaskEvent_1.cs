using DS.ClassLib.VarUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.SwitchSolution.Models
{
    public interface IWindowTaskEvent_1 : ITaskEvent
    {
        /// <summary>
        /// Check if window is closed.
        /// </summary>
        public bool WindowClosed { get; }
        public bool Backward { get; }
        public bool Onward { get; }
    }
}
