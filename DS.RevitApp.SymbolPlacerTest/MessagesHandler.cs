using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
   
    internal class MessagesHandler
    {
        private readonly FailuresAccessor _failuresAccessor;
        private readonly string _transactionName;
        private readonly FailuresProcessingEventArgs _eventArgs;

        /// <summary>
        /// Handle transaction messages
        /// </summary>
        public MessagesHandler(FailuresProcessingEventArgs failuresProcessingEventArgs)
        {
            this._eventArgs = failuresProcessingEventArgs;
            _failuresAccessor = failuresProcessingEventArgs.GetFailuresAccessor();
            _transactionName = _failuresAccessor.GetTransactionName();

            List<FailureMessageAccessor> errorMessages =
                (List<FailureMessageAccessor>)_failuresAccessor.GetFailureMessages(FailureSeverity.Error);
            HandleErrors(errorMessages);

            List<FailureMessageAccessor> warningMessages =
                (List<FailureMessageAccessor>)_failuresAccessor.GetFailureMessages(FailureSeverity.Warning);
            HandleWarnings(warningMessages);
        }

        public bool ShowFails { get; private set; }

        public void HandleErrors(List<FailureMessageAccessor> errorMessages)
        {
            if (errorMessages != null && errorMessages.Any())
            {
                foreach (var m in errorMessages)
                {
                    var type = m.GetCurrentResolutionType();
                    m.SetCurrentResolutionType(type);

                        if (!ShowFails)
                        {
                            //Set option for failure handling
                            FailureHandlingOptions failureHandlingOptions = _failuresAccessor.GetFailureHandlingOptions();
                            failureHandlingOptions.SetClearAfterRollback(true);
                            _failuresAccessor.SetFailureHandlingOptions(failureHandlingOptions);

                            _eventArgs.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                        }
                        else
                        {
                            _eventArgs.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                        }
                        _failuresAccessor.SetTransactionName(_transactionName + "-RolledBack");
                        _failuresAccessor.DeleteWarning(m);                    
                }
            }
        }

        public void HandleWarnings(List<FailureMessageAccessor> warningMessages)
        {
            if (warningMessages != null && warningMessages.Any())
            {
                foreach (var m in warningMessages)
                {
                        _failuresAccessor.SetTransactionName(_transactionName + "-Warning");
                        _failuresAccessor.DeleteWarning(m);                  
                }
            }
        }
    }
}
