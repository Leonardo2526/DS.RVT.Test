using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest
{
    /// <summary>
    /// Interface to create task events.
    /// </summary>
    internal interface ITaskEvent
    {
        /// <summary>
        /// Create a new task event.
        /// </summary>
        /// <returns></returns>
        public Task Create();
    }
}
