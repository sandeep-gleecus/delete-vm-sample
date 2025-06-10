using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Web.Management;

namespace Inflectra.SpiraTest.Common
{
	/// <summary>
	/// This is the generic logging component for SpiraTeam. It wraps the .NET
	/// Framework tracing and error logging functions
	/// </summary>
	//[DebuggerStepThrough]
	public class Logger
	{
		private static TraceSwitch generalSwitch;
		private static TraceSwitch queriesSwitch;
		protected static EventLog applicationEventLog = null;

		//Event categories
		public const string EVENT_CATEGORY_APPLICATION = "Application";
		public const string EVENT_CATEGORY_DATA_SYNCHRONIZATION = "Data Synchronization";
		public const string EVENT_CATEGORY_VERSION_CONTROL = "Version Control";

		private static DateTime retryDate = DateTime.MinValue; // Won't try sending unless DateTime.UtcNow is > _retryDate
		private const int MAX_MESSAGE_SIZE = 1073741823;

		/// <summary>
		/// Returns a handle to the application event log
		/// </summary>
		public static EventLog ApplicationEventLog
		{
			get
			{
				return applicationEventLog;
			}
		}

		/// <summary>
		/// This is the constructor for the class
		/// </summary>
		/// <remarks>This is a class method</remarks>
		static Logger()
		{
			//Need to get the correct event log source for logging events against
			//If no event log name specified, it means that the security on the hosted
			//environment doesn't permit event log access
			string eventLogSourceName = Properties.Settings.Default.EventLogSource;
			if (!String.IsNullOrEmpty(eventLogSourceName))
			{
				applicationEventLog = new EventLog("Application", ".", eventLogSourceName);
			}

			generalSwitch = new TraceSwitch("generalSwitch", "Switch for General Tracing");
			queriesSwitch = new TraceSwitch("queriesSwitch", "Switch for SQL Tracing");
		}

		/// <summary>
		/// This methods logs a data command event
		/// </summary>
		/// <param name="dbCommand">The database Command to be logged</param>
		/// <remarks>This is a class method</remarks>
		public static void LogDataCommandEvent(string queryInformation)
		{
			if (Properties.Settings.Default.TraceLogging_Enable)
			{
				//Display the query info
				System.Diagnostics.Trace.WriteLineIf(queriesSwitch.TraceInfo,
					System.DateTime.Now.ToShortDateString() + " " +
					System.DateTime.Now.ToString("HH:mm:ss.FFF") + "   " +
					"TRACE   " +
					queryInformation);
			}
		}

		/// <summary>
		/// This methods logs a data command event
		/// </summary>
		/// <param name="dbCommand">The database Command to be logged</param>
		/// <remarks>This is a class method</remarks>
		public static void LogDataCommandEvent(DbCommand dbCommand)
		{
			if (Properties.Settings.Default.TraceLogging_Enable)
			{
				//Display the connection info
				System.Diagnostics.Trace.WriteLineIf(queriesSwitch.TraceInfo,
					System.DateTime.Now.ToShortDateString() + " " +
					System.DateTime.Now.ToString("HH:mm:ss.FFF") + "   " +
					"TRACE   " +
					"Connection String: " +
					dbCommand.Connection.ConnectionString);

				//Display the database command
				System.Diagnostics.Trace.WriteLineIf(queriesSwitch.TraceInfo,
					System.DateTime.Now.ToShortDateString() + " " +
					System.DateTime.Now.ToString("HH:mm:ss.FFF") + "   " +
					"TRACE   " +
					"Command String: " +
					dbCommand.CommandText);

				//Display any parameters if there are any
				foreach (DbParameter dbParameter in dbCommand.Parameters)
				{
					System.Diagnostics.Trace.WriteLineIf(queriesSwitch.TraceInfo,
						System.DateTime.Now.ToShortDateString() + " " +
						System.DateTime.Now.ToString("HH:mm:ss.FFF") + "   " +
						"TRACE   " +
						"Parameter: " +
						dbParameter.ParameterName + ", Value: " +
						dbParameter.Value + ", Type: " +
						dbParameter.DbType + ", Size: " +
						dbParameter.Size
						);
				}
			}
		}

		/// <summary>
		/// This methods logs a general trace event message
		/// </summary>
		/// <param name="source">The class/method name</param>
		/// <param name="message">The message to be logged</param>
		/// <remarks>This is a class method</remarks>
		[DebuggerStepThrough]
		public static void LogTraceEvent(string source, string message)
		{
			if (Properties.Settings.Default.TraceLogging_Enable)
			{
				System.Diagnostics.Trace.WriteLineIf(generalSwitch.TraceInfo, System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToString("HH:mm:ss.FFF") + "   TRACE   " + source + ": " + message);
			}
		}

		/// <summary>Removes any vendor specific branding from logged messages</summary>
		/// <param name="input">The string to clean.</param>
		/// <returns>A string stripped of developer-specific information.</returns>
		private static string CleanMessages(string input)
		{
			string cleanMessage = "";
			if (!String.IsNullOrWhiteSpace(input))
				cleanMessage = input.Replace("Inflectra.SpiraTest", "APPLICATION");

			return cleanMessage;
		}

		/// <summary>
		/// This methods logs a warning event message
		/// </summary>
		/// <param name="source">The class/method name</param>
		/// <param name="message">The message to be logged</param>
		/// <remarks>This is a class method</remarks>
		public static void LogWarningEvent(string source, Exception exception)
		{
			//Get the exception text..
			string strMsg = "In " + source + ":" + Environment.NewLine + Environment.NewLine + Logger.DecodeException(exception);

			if (Properties.Settings.Default.TraceLogging_Enable)
			{
				//Log the warning then the entire stack-trace
				System.Diagnostics.Trace.WriteLineIf(generalSwitch.TraceWarning, System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToString("HH:mm:ss.FFF") + "  *WARNING*  " + strMsg);
			}
			//Regardless of the trace settings we always log to the event viewer directly
			try
			{
				if (applicationEventLog != null)
				{

					applicationEventLog.WriteEntry(CleanMessages(strMsg), EventLogEntryType.Warning);
				}
			}
			catch (Exception)
			{
				//Fail Quietly (if event log not available)
			}

			//We also try logging to the database internal log
			try
			{
				WriteToDatabase(source, exception.Message, exception.StackTrace, EventLogEntryType.Warning, exception.GetType());
			}
			catch (Exception)
			{
				//Fail quietly
			}
		}

		/// <summary>This methods logs a warning event message</summary>
		/// <param name="source">The class/method name</param>
		/// <param name="message">The message to be logged</param>
		/// <param name="exception">Exception to be logged.</param>
		/// <remarks>This is a class method</remarks>
		public static void LogWarningEvent(string source, Exception exception, string message)
		{
			//Get the exception text..
			string strMsg = "In " + source + ":" + Environment.NewLine + Environment.NewLine + Logger.DecodeException(exception);

			if (Properties.Settings.Default.TraceLogging_Enable)
			{
				//Log the warning then the entire stack-trace
				System.Diagnostics.Trace.WriteLineIf(generalSwitch.TraceWarning, System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToString("HH:mm:ss.FFF") + "  *WARNING*  " + strMsg);
			}
			//Regardless of the trace settings we always log to the event viewer directly
			try
			{
				if (applicationEventLog != null)
				{

					applicationEventLog.WriteEntry(CleanMessages(strMsg), EventLogEntryType.Warning);
				}
			}
			catch (Exception)
			{
				//Fail Quietly (if event log not available)
			}

			//We also try logging to the database internal log
			try
			{
				WriteToDatabase(source, exception.Message, exception.StackTrace, EventLogEntryType.Warning, exception.GetType());
			}
			catch (Exception)
			{
				//Fail quietly
			}

		}

		/// <summary>
		/// This methods logs a warning event message
		/// </summary>
		/// <param name="source">The class/method name</param>
		/// <param name="message">The message to be logged</param>
		/// <param name="category">The message category</param>
		/// <remarks>This is a class method</remarks>
		public static void LogWarningEvent(string source, string message, string category = EVENT_CATEGORY_APPLICATION)
		{
			if (Properties.Settings.Default.TraceLogging_Enable)
			{
				System.Diagnostics.Trace.WriteLineIf(generalSwitch.TraceWarning, System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToString("HH:mm:ss.FFF") + "   TRACE   " + source + ": " + message);
			}
			//Regardless of the trace settings we always log to the event viewer directly
			try
			{
				if (applicationEventLog != null)
				{

					applicationEventLog.WriteEntry(CleanMessages(source) + ": " + CleanMessages(message), EventLogEntryType.Warning);
				}
			}
			catch (Exception)
			{
				//Fail Quietly (if event log not available)
			}

			//We also try logging to the database internal log
			try
			{
				WriteToDatabase(source, message, null, EventLogEntryType.Warning, null, category);
			}
			catch (Exception)
			{
				//Fail quietly
			}

		}

		/// <summary>
		/// This methods logs a failure audit event message
		/// </summary>
		/// <param name="source">The class/method name</param>
		/// <param name="message">The message to be logged</param>
		/// <remarks>This is a class method</remarks>
		public static void LogFailureAuditEvent(string source, string message)
		{
			if (Properties.Settings.Default.TraceLogging_Enable)
			{
				System.Diagnostics.Trace.WriteLineIf(generalSwitch.TraceWarning, System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToString("HH:mm:ss.FFF") + "   TRACE   " + source + ": " + message);
			}
			//Regardless of the trace settings we always log to the event viewer directly
			try
			{
				if (applicationEventLog != null)
				{

					applicationEventLog.WriteEntry(CleanMessages(source) + ": " + CleanMessages(message), EventLogEntryType.FailureAudit);
				}
			}
			catch (Exception)
			{
				//Fail Quietly (if event log not available)
			}

			//We also try logging to the database internal log
			try
			{
				WriteToDatabase(source, message, null, EventLogEntryType.FailureAudit);
			}
			catch (Exception)
			{
				//Fail quietly
			}

		}

        /// <summary>
        /// This methods logs a success audit event message
        /// </summary>
        /// <param name="logWhenNotInTraceMode">Should we log even if not in trace mode</param>
        /// <param name="source">The class/method name</param>
        /// <param name="message">The message to be logged</param>
        /// <remarks>This is a class method</remarks>
        public static void LogSuccessAuditEvent(string source, string message, bool logWhenNotInTraceMode = false)
        {
            if (Properties.Settings.Default.TraceLogging_Enable || logWhenNotInTraceMode)
            {
                System.Diagnostics.Trace.WriteLineIf(generalSwitch.TraceWarning, System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToString("HH:mm:ss.FFF") + "   TRACE   " + source + ": " + message);
                try
                {
                    if (applicationEventLog != null)
                    {

                        applicationEventLog.WriteEntry(CleanMessages(source) + ": " + CleanMessages(message), EventLogEntryType.SuccessAudit);
                    }
                }
                catch (Exception)
                {
                    //Fail Quietly (if event log not available)
                }

                //We also try logging to the database internal log
                try
                {
                    WriteToDatabase(source, message, null, EventLogEntryType.SuccessAudit);
                }
                catch (Exception)
                {
                    //Fail quietly
                }
            }
        }

		/// <summary>
		/// This methods logs an informational event message
		/// </summary>
		/// <param name="source">The class/method name</param>
		/// <param name="message">The message to be logged</param>
		/// <remarks>This is a class method</remarks>
		public static void LogInformationalEvent(string source, string message)
		{
			if (Properties.Settings.Default.TraceLogging_Enable)
			{
				System.Diagnostics.Trace.WriteLineIf(generalSwitch.TraceInfo, System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToString("HH:mm:ss.FFF") + "   TRACE   " + source + ": " + message);
			}
			//Regardless of the trace settings we always log to the event viewer directly
			try
			{
				if (applicationEventLog != null)
				{

					applicationEventLog.WriteEntry(CleanMessages(source) + ": " + CleanMessages(message), EventLogEntryType.Information);
				}
			}
			catch (Exception)
			{
				//Fail Quietly (if event log not available)
			}

			//We also try logging to the database internal log
			try
			{
				WriteToDatabase(source, message, null, EventLogEntryType.Information);
			}
			catch (Exception)
			{
				//Fail quietly
			}

		}

		/// <summary>
		/// This methods logs an error event message (when we don't have an exception object)
		/// </summary>
		/// <param name="source">The class/method name</param>
		/// <param name="message">The message to be logged</param>
		/// <remarks>This is a class method</remarks>
		public static void LogErrorEvent(string source, string message)
		{
			if (Properties.Settings.Default.TraceLogging_Enable)
			{
				System.Diagnostics.Trace.WriteLineIf(generalSwitch.TraceError, System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToString("HH:mm:ss.FFF") + "   TRACE   " + source + ": " + message);
			}
			//Regardless of the trace settings we always log to the event viewer directly
			try
			{
				if (applicationEventLog != null)
				{
					applicationEventLog.WriteEntry(CleanMessages(source) + ": " + CleanMessages(message), EventLogEntryType.Error);
				}
			}
			catch (Exception)
			{
				//Fail Quietly (if event log not available)
			}

			//We also try logging to the database internal log
			try
			{
				WriteToDatabase(source, message, null, EventLogEntryType.Error);
			}
			catch (Exception)
			{
				//Fail quietly
			}

		}

		/// <summary>This methods logs a general error event message</summary>
		/// <param name="source">The class/method name</param>
		/// <param name="exception">The exception to be logged</param>
		/// <remarks>This is a class method</remarks>
		public static void LogErrorEvent(string source, System.Exception exception)
		{
			Logger.LogErrorEvent(source, exception, "");
		}

		/// <summary>This methods logs a general error event message</summary>
		/// <param name="source">The class/method name</param>
		/// <param name="exception">The exception to be logged</param>
		/// <param name="message">Extra message to log.</param>
		/// <param name="category">The category of event to log</param>
		/// <remarks>This is a class method</remarks>
		public static void LogErrorEvent(string source, Exception exception, string message, string category = EVENT_CATEGORY_APPLICATION)
		{
			try
			{
				//Get the exception text..
				string shortMessage = exception.Message;
				string strMsg = "In " + source + ":" + Environment.NewLine + Environment.NewLine + Logger.DecodeException(exception);

				//Add on any custom message
				if (!String.IsNullOrEmpty(message))
				{
					shortMessage += " " + message;
					strMsg += Environment.NewLine + message;
				}

				//Log the error then the entire stack-trace
				System.Diagnostics.Trace.WriteLineIf(generalSwitch.TraceError, System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToString("HH:mm:ss.FFF") + "  *ERROR*  " + source + ": " + strMsg);

				//If we have a thread aborted exception, don't log as those are generated during redirects
				if (exception.GetType() == typeof(ThreadAbortException))
					return;
				else
				{
					//Regardless of the trace settings we always log to the event viewer directly
					try
					{
						if (applicationEventLog != null)
						{
							applicationEventLog.WriteEntry(Logger.CleanMessages(strMsg), EventLogEntryType.Error);
						}
					}
					catch (Exception)
					{
						//Fail Quietly (if event log not available)
					}

					//We also try logging to the database internal log
					try
					{
						WriteToDatabase(source, shortMessage, Logger.CleanMessages(strMsg), EventLogEntryType.Error, exception.GetType(), category);
					}
					catch (Exception)
					{
						//Fail quietly
					}
				}
			}
			catch
			{
				//Catch everything quietly so that we don't get the function accidentally calling itself in an infinite loop!
			}
		}

		/// <summary>
		/// This method logs a standard trace for entering a method
		/// </summary>
		/// <param name="source">The class/method name</param>
		/// <remarks>This is a class method</remarks>
		public static void LogEnteringEvent(string source)
		{
			Logger.LogTraceEvent(source, "Entering");
		}

		/// <summary>
		/// This method logs a standard trace for exiting a method
		/// </summary>
		/// <param name="source">The class/method name</param>
		[DebuggerStepThrough]
		public static void LogExitingEvent(string source)
		{
			Logger.LogTraceEvent(source, "Exiting");
		}

		/// <summary>
		/// This method simply wraps the built-in listener flush method
		/// </summary>
		public static void Flush()
		{
			System.Diagnostics.Trace.Flush();
		}

		/// <summary>
		/// Writes a series of events to the database-based log. This overload is used by the SpiraEventProvider
		/// for events logged directly by ASP.NET. Typically application events are logged through one of the
		/// LogError, LogWarning public methods instead
		/// </summary>
		/// <param name="events">List of ASP.NET web events</param>
		/// <param name="eventsDiscardedByBuffer">Events discarded by buffer</param>
		/// <param name="lastNotificationUtc">The last notification date/time</param>
		public static void WriteToDatabase(ICollection events, int eventsDiscardedByBuffer, DateTime lastNotificationUtc)
		{
			// We don't want to send any more events until we've waited until the _retryDate (which defaults to minValue)
			if (retryDate > DateTime.UtcNow)
			{
				return;
			}

			try
			{
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					if (eventsDiscardedByBuffer != 0)
					{
						WebBaseEvent infoEvent = new SpiraWebBaseEvent(
							String.Format("{0} events were discarded since last notification was made at {1} because the event buffer capacity was exceeded.",
								eventsDiscardedByBuffer.ToString(CultureInfo.InstalledUICulture),
								lastNotificationUtc.ToString("r", CultureInfo.InstalledUICulture)),
								null,
								WebEventCodes.WebEventProviderInformation,
								WebEventCodes.SqlProviderEventsDropped);

						Event evt = new Event();
						context.Events.AddObject(evt);
						PopulateEvent(evt, infoEvent);
					}

					foreach (WebBaseEvent eventRaised in events)
					{
						//Write new event to database
						Event evt = new Event();
						context.Events.AddObject(evt);
						PopulateEvent(evt, eventRaised);
					}

					//Commit events
					context.SaveChanges();

					//Now delete any events that are too old
					DateTime lastDateToKeep = DateTime.UtcNow.AddDays(-ConfigurationSettings.Default.Logging_MaximumNumberDaysRetained);
					context.Logger_DeleteOld(lastDateToKeep);
				}
			}
			catch
			{
				// For any failure, we will wait at least 30 seconds before trying again
				double timeout = 30;
				retryDate = DateTime.UtcNow.AddSeconds(timeout);
				throw;
			}
		}

		/// <summary>
		/// Writes a single application event to the database-based log. This overload is used by the internal application logging methods
		/// </summary>
		/// <param name="events">List of ASP.NET web events</param>
		/// <param name="eventsDiscardedByBuffer">Events discarded by buffer</param>
		/// <param name="lastNotificationUtc">The last notification date/time</param>
		public static void WriteToDatabase(string source, string message, string details, EventLogEntryType eventlogEntryType, Type exceptionType = null, string eventCategory = EVENT_CATEGORY_APPLICATION)
		{
			// We don't want to send any more events until we've waited until the _retryDate (which defaults to minValue)
			if (retryDate > DateTime.UtcNow)
			{
				return;
			}

			try
			{
				//If we have no details, add a note
				if (String.IsNullOrEmpty(details))
				{
					details = Resources.Main.Nothing;
				}
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Write new event to database
					Event evt = new Event();
					context.Events.AddObject(evt);
					evt.EventId = Guid.NewGuid().ToString("N", CultureInfo.InstalledUICulture);
					evt.EventTimeUtc = DateTime.UtcNow;
					evt.EventTime = DateTime.Now;
					evt.EventTypeId = (int)eventlogEntryType;
					evt.EventCategory = eventCategory;
					evt.EventSequence = 0;
					evt.EventOccurrence = 0;
					if (exceptionType != null)
					{
						evt.ExceptionType = exceptionType.ToString();
					}
					evt.EventCode = 0;
					evt.EventDetail = 0;
					evt.Message = message.MaxLength(1024);
					evt.Details = details.MaxLength(MAX_MESSAGE_SIZE);
					evt.MachineName = Environment.MachineName;

					//Commit events
					context.SaveChanges();

					//Now delete any events that are too old
					DateTime lastDateToKeep = DateTime.UtcNow.AddDays(-ConfigurationSettings.Default.Logging_MaximumNumberDaysRetained);
					context.Logger_DeleteOld(lastDateToKeep);
				}
			}
			catch
			{
				// For any failure, we will wait at least 30 seconds before trying again
				double timeout = 30;
				retryDate = DateTime.UtcNow.AddSeconds(timeout);
				throw;
			}
		}

		/// <summary>
		/// Populates the event
		/// </summary>
		/// <param name="evt">The event entity</param>
		/// <param name="eventRaised">The raised web event</param>
		private static void PopulateEvent(Event evt, WebBaseEvent eventRaised)
		{
			Exception exception = null;
			WebRequestInformation reqInfo = null;
			string details = null;
			WebApplicationInformation appInfo = WebBaseEvent.ApplicationInformation;

			evt.EventId = eventRaised.EventID.ToString("N", CultureInfo.InstalledUICulture);   // @EventId
			evt.EventTimeUtc = eventRaised.EventTimeUtc;      // @EventTimeUtc
			evt.EventTime = eventRaised.EventTime;         // @EventTime
			evt.EventCategory = eventRaised.GetType().ToString();  // @EventType
			evt.EventSequence = eventRaised.EventSequence;     // @EventSequence
			evt.EventOccurrence = eventRaised.EventOccurrence;     // @EventOccurrence
			evt.EventCode = eventRaised.EventCode;         // @EventCode
			evt.EventDetail = eventRaised.EventDetailCode;   // @EventDetailCode
			evt.Message = eventRaised.Message;           // @Message
			evt.ApplicationPath = appInfo.ApplicationPath;       // @ApplicationPath
			evt.ApplicationVirtualPath = appInfo.ApplicationVirtualPath; // @ApplicationVirtualPath
			evt.MachineName = appInfo.MachineName; // @MachineName

			// @RequestUrl
			if (eventRaised is WebRequestEvent)
			{
				reqInfo = ((WebRequestEvent)eventRaised).RequestInformation;
			}
			else if (eventRaised is WebRequestErrorEvent)
			{
				reqInfo = ((WebRequestErrorEvent)eventRaised).RequestInformation;
			}
			else if (eventRaised is WebErrorEvent)
			{
				reqInfo = ((WebErrorEvent)eventRaised).RequestInformation;
			}
			else if (eventRaised is WebAuditEvent)
			{
				reqInfo = ((WebAuditEvent)eventRaised).RequestInformation;
			}
			if (reqInfo != null)
			{
				evt.RequestUrl = reqInfo.RequestUrl;
			}

			// @ExceptionType
			if (eventRaised is WebBaseErrorEvent)
			{
				exception = ((WebBaseErrorEvent)eventRaised).ErrorException;
			}
			evt.ExceptionType = (exception != null) ? exception.GetType().ToString() : null;

			// @Details
			details = eventRaised.ToString();
			if (details.Length > MAX_MESSAGE_SIZE)
			{
				details = details.Substring(0, MAX_MESSAGE_SIZE);
			}
			evt.Details = details;

			//Map to Event Log types
			switch (eventRaised.GetType().Name)
			{
				case "WebBaseEvent":
					evt.EventTypeId = (int)EventLogEntryType.Information;
					break;

				case "WebManagementEvent":
					evt.EventTypeId = (int)EventLogEntryType.Information;
					break;

				case "WebApplicationLifetimeEvent":
					evt.EventTypeId = (int)EventLogEntryType.Information;
					break;

				case "WebRequestEvent":
					evt.EventTypeId = (int)EventLogEntryType.Information;
					break;

				case "WebHeartbeatEvent":
					evt.EventTypeId = (int)EventLogEntryType.Information;
					break;

				case "WebBaseErrorEvent":
					evt.EventTypeId = (int)EventLogEntryType.Warning;
					break;

				case "WebRequestErrorEvent":
					evt.EventTypeId = (int)EventLogEntryType.Error;
					break;

				case "WebErrorEvent":
					evt.EventTypeId = (int)EventLogEntryType.Error;
					break;

				case "WebAuditEvent":
					evt.EventTypeId = (int)EventLogEntryType.Information;
					break;

				case "WebSuccessAuditEvent":
					evt.EventTypeId = (int)EventLogEntryType.SuccessAudit;
					break;

				case "WebAuthenticationSuccessAuditEvent":
					evt.EventTypeId = (int)EventLogEntryType.SuccessAudit;
					break;

				case "WebFailureAuditEvent":
					evt.EventTypeId = (int)EventLogEntryType.FailureAudit;
					break;

				case "WebAuthenticationFailureAuditEvent":
					evt.EventTypeId = (int)EventLogEntryType.FailureAudit;
					break;

				case "WebViewStateFailureAuditEvent":
					evt.EventTypeId = (int)EventLogEntryType.FailureAudit;
					break;

				default:
					evt.EventTypeId = (int)EventLogEntryType.Error;
					break;
			}
		}

		/// <summary>Decode the given Exception into a nice pretty string.</summary>
		/// <param name="ex">The exception to decode.</param>
		/// <returns>String used for logging.</returns>
		public static string DecodeException(Exception ex)
		{
			//Constants
			const string indent = "   ";
			string crlf = Environment.NewLine;

			string retString = "";
			if (ex != null)
			{
				//Get the stack trace, and initialize out other variables.
				string strStackTrace = ex.StackTrace;
				List<string> lstSQLErrors = new List<string>();
				string strFusion = "";
				List<string> lstMsgs = new List<string>();

				//Loop through each exception.
				for (Exception curEx = ex; curEx != null; curEx = curEx.InnerException)
				{
					//Get fusion log (if needed).
					if (string.IsNullOrWhiteSpace(strFusion) &&
						curEx.GetType().GetProperty("FusionLog") != null &&
						((dynamic)curEx).FusionLog != null)
					{
						strFusion = ((dynamic)curEx).FusionLog;
					}

					//Get any SQL errors (if needed).
					if (curEx is SqlException && lstSQLErrors.Count < 1)
					{
						List<string> listSQLErrors = new List<string>();
						int counter = 1;
						foreach (SqlError error in ((SqlException)curEx).Errors)
						{
							listSQLErrors.Add(counter.ToString() + ": " + error.Message);
							counter++;
						}
					}

					//Now get the type and message.
					lstMsgs.Add(curEx.Message + " [" + curEx.GetType().ToString() + "]");
				}

				//Now combine them all to be pretty!
				retString = "Messages:" + crlf;
				retString += indent + String.Join(crlf + indent, lstMsgs).Trim();
				retString += crlf + crlf;
				if (lstSQLErrors.Count > 0)
				{
					retString += "SQL Messages:" + crlf;
					retString += indent + String.Join(crlf + indent, lstSQLErrors).Trim();
					retString += crlf + crlf;
				}
				if (!string.IsNullOrWhiteSpace(strFusion))
				{
					retString += "Fusion Log:" + crlf;
					retString += strFusion;
					retString += crlf + crlf;
				}
				retString += "Stack Trace:" + crlf;
				retString += strStackTrace;

				//Trim off any extras..
				retString = retString.Trim();
			}

			//Send it back!
			return retString;
		}
	}

	public class SpiraWebBaseEvent : WebBaseEvent
	{
		public SpiraWebBaseEvent(string message, Object eventSource, int eventCode, int eventDetailCode)
			: base(message, eventSource, eventCode, eventDetailCode)
		{ }
	}
	public sealed class SpiraWebBaseEventCollection : ReadOnlyCollectionBase
	{
		public SpiraWebBaseEventCollection(ICollection events)
		{
			if (events == null)
			{
				throw new ArgumentNullException("events");
			}

			foreach (WebBaseEvent eventRaised in events)
			{
				InnerList.Add(eventRaised);
			}
		}

		public SpiraWebBaseEventCollection(WebBaseEvent eventRaised)
		{
			if (eventRaised == null)
			{
				throw new ArgumentNullException("eventRaised");
			}

			InnerList.Add(eventRaised);
		}

		// overloaded collection access methods 
		public WebBaseEvent this[int index]
		{
			get
			{
				return (WebBaseEvent)InnerList[index];
			}
		}

		public int IndexOf(WebBaseEvent value)
		{
			return InnerList.IndexOf(value);
		}

		public bool Contains(WebBaseEvent value)
		{
			return InnerList.Contains(value);
		}
	}
}
