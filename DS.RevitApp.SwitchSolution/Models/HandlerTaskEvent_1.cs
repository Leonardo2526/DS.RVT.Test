using DS.ClassLib.VarUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.SwitchSolution.Models
{
    /// <summary>
    /// An object to create a new task event for <see cref="IEventHandler"/>.
    /// </summary>
    public class HandlerTaskEvent_1 : IWindowTaskEvent_1
    {
        private readonly IEventHandler_1 _objectHandler;

        /// <summary>
        /// Create a new instance of object to build a new task events with the <paramref name="objectHandler"/>.
        /// </summary>
        /// <param name="objectHandler"></param>
        public HandlerTaskEvent_1(IEventHandler_1 objectHandler)
        {
            _objectHandler = objectHandler;
        }

        ///<inheritdoc/>
        public bool WindowClosed { get; private set; }

        public bool Backward { get; private set; }

        public bool Onward { get; private set; }


        /// <summary>
        /// Create a new task event.
        /// </summary>
        /// <returns></returns>
        public Task Create()
        {
            var tcs = new TaskCompletionSource<object>();
            void onwardHandler(object s, EventArgs e)
            {
                Onward = true;
                tcs.TrySetResult(null);
            };

            void backwardHandler(object s, EventArgs e)
            {
                Backward = true;
                tcs.TrySetResult(null);
            };

            void windhandler(object s, EventArgs e)
            {
                WindowClosed = true;
                tcs.TrySetResult(null);
            };
            _objectHandler.OnwardHandler += onwardHandler;
            _objectHandler.BackwardHandler += backwardHandler;
            _objectHandler.CloseWindowHandler += windhandler;
                       
            return tcs.Task.ContinueWith(_ =>
            {
                _objectHandler.BackwardHandler -= backwardHandler;
                _objectHandler.OnwardHandler -= onwardHandler;
                _objectHandler.CloseWindowHandler -= windhandler;
            });
        }
    }


}
