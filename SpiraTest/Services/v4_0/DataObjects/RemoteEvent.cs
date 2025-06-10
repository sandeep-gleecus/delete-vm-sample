using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents an error/info/warning event
    /// </summary>
    public class RemoteEvent
    {
        /// <summary>
        /// The type of event log entry (Error = 1, Warning = 2, Information = 4, SuccessAudit = 8, FailureAudit = 16)
        /// </summary>
        public int EventLogEntryType;
        
        /// <summary>
        /// The short message
        /// </summary>
        public string Message;

        /// <summary>
        /// The full stack-trace and details
        /// </summary>
        public string Details;
    }
}