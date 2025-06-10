using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
	/// <summary>
	/// Represents a full log event entry
	/// </summary>
	public class RemoteEvent2
	{
		/// <summary>
		/// The type of event log entry (Error = 1, Warning = 2, Information = 4, SuccessAudit = 8, FailureAudit = 16)
		/// </summary>
		public int EventTypeId;

		/// <summary>
		/// The name of the event type
		/// </summary>
		public string EventTypeName;

		/// <summary>
		/// The short message
		/// </summary>
		public string Message;

		/// <summary>
		/// The full stack-trace and details
		/// </summary>
		public string Details;
		/// <summary>
		/// The event time in UTC
		/// </summary>
		public DateTime EventTimeUtc;

		

		/// <summary>
		/// The event category
		/// </summary>
		public string EventCategory;

		/// <summary>
		/// The event code
		/// </summary>
		public int EventCode;

		/// <summary>
		/// The event id
		/// </summary>
		public string EventId;
	}
}
