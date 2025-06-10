using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Inflectra.SpiraTest.Installer
{
	public class Logger
	{
/*
		#region Properties
		/// <summary>Whether or not to log Informational events to the Application Event Log.</summary>
		public static bool TraceLogging
		{
			get;
			set;
		}

		/// <summary>Whether we're logging to an output file instead of the EventLog.</summary>
		public static bool LoggingToFile
		{
			get;
			set;
		}

		/// <summary>The application name to log events as.</summary>
		public static string ApplicationName
		{
			get;
			set;
		}

		/// <summary>The eventlog object created, or null if we're writing to the file.</summary>
		public static EventLog EventLog
		{
			get
			{
				return Eventlog;
			}
			set
			{
				Eventlog = value;
			}
		}
		#endregion

		#region Private Properties
		/// <summary>The filename that stores the file we're writing to.</summary>
		private static string _logFile
		{
			get;
			set;
		}

		/// <summary>The eventlog object.</summary>
		private static EventLog Eventlog
		{
			get;
			set;
		}
		#endregion

		/// <summary>Creates the log if needed.</summary>
		/// <param name="applicationname">The name of the application.</param>
		public Logger(string applicationName)
		{
			Exception createEx = null;
			Exception createFile = null;

			ApplicationName = applicationName;

			if (!string.IsNullOrWhiteSpace(applicationName))
			{
				if (Eventlog == null)
				{
					if (!LoggingToFile)
					{
						try
						{
							//Create event source if needed.
							if (!EventLog.SourceExists(applicationName))
								EventLog.CreateEventSource(applicationName, "Application");
							if (Eventlog == null)
								Eventlog = new System.Diagnostics.EventLog("Application", ".", applicationName);
						}
						catch (Exception ex)
						{
							LoggingToFile = true;
							createEx = ex;
						}

						if (Eventlog == null)
							LoggingToFile = true;
					}
				}

				if (LoggingToFile)
				{
					//Let's create the file if it doesn't exist, and write our launch.
					string LogPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + applicationName.Replace(" ", "") + ".log";
					Debug.WriteLine("Writing to logfile: " + LogPath);
					try
					{
						StreamWriter logFile = File.CreateText(LogPath);
						logFile.Close();

						_logFile = LogPath;
					}
					catch (Exception ex)
					{
						try
						{
							//Error creating user's profile, let's try the temp directory.
							LogPath = Path.GetTempPath() + applicationName.Replace(" ", "") + ".log";
							StreamWriter logFile = File.CreateText(LogPath);
							logFile.Close();

							_logFile = LogPath;
							createFile = ex;
						}
						catch (Exception ex2)
						{
							//We can't even log to file. Darnit. Throw an exception.
							throw new InvalidOperationException("Could not start logging engine.", ex2);
						}
					}
				}
			}
			else
			{
				throw new ArgumentNullException("application", "Could not start logging engine.");
			}

			//Write intro and any prevailing error messages.
			//TODO: Fix program name, etc.
			LogMessage("**********" + Environment.NewLine + "* " + applicationName + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ")" + Environment.NewLine + "**********", null, EventLogEntryType.Information);
			if (createEx != null)
			{
				LogMessage(createEx, null, "Could not create EventLog");
			}
			if (createFile != null)
			{
				LogMessage(createFile, null, "Could not create initial log file");
			}
		}

		#region Static Log Methods
		/// <summary>Logs a message to the EventLog using the specified type.</summary>
		/// <param name="message">The message to log.</param>
		/// <param name="messageType">The type of the message.</param>
		public static void LogMessage(string message, string method = null, EventLogEntryType messageType = EventLogEntryType.Information)
		{
			//Make log string
			string msg = "";
			if (!string.IsNullOrWhiteSpace(method))
				msg = "In " + method + ":" + Environment.NewLine + message;
			else
				msg = message;

			//Initialize if necessary.
			if (Eventlog == null && string.IsNullOrWhiteSpace(_logFile))
			{
				Logger log = new Logger(ApplicationName);
			}

			if (!LoggingToFile)
			{

				if ((messageType != EventLogEntryType.Information && messageType != EventLogEntryType.SuccessAudit) || TraceLogging)
					if (Eventlog != null)
					{
						Eventlog.WriteEntry(msg, messageType);
					}
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(_logFile))
				{
					//Log the message to a file.
					string timestring = DateTime.Now.ToString("HH:mm:ss.fffffff") + ": ";
					try
					{
						string strMsg = timestring + msg.Replace(Environment.NewLine, Environment.NewLine + "\t");
						File.AppendAllText(_logFile, strMsg + Environment.NewLine);
					}
					catch
					{ }
				}
			}

			Debug.WriteLine(msg);
			Debug.Flush();
		}

		/// <summary>Logs an exception to the EventLog as an Error type message.</summary>
		/// <param name="ex">The exception to log.</param>
		public static void LogMessage(Exception ex, string method = null, string message = null)
		{
			//Generate message.
			string strTrace = ex.StackTrace;
			string strMsg = "";

			if (!string.IsNullOrWhiteSpace(message))
				strMsg = message + Environment.NewLine + Environment.NewLine;

			//Add the first outer message.
			strMsg += "[" + ex.GetType().ToString() + "] -- " + ex.Message;
			while (ex.InnerException != null)
			{
				strMsg += "; " + Environment.NewLine + ex.InnerException.Message;
				ex = ex.InnerException;
			}
			strMsg += Environment.NewLine + Environment.NewLine + strTrace;

			LogMessage(method, strMsg, EventLogEntryType.Error);
		}

		/// <summary>Logs a debug message to the debug console and EventLog if enabled.</summary>
		/// <param name="Message">The string to log.</param>
		public static void LogTrace(string Message)
		{
			//Log it to the debug console.
			//Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.FFFFFFF") + ": " + Message);
			//Log it to the event log if enabled.
			if (TraceLogging || LoggingToFile)
				LogMessage(Message, null, EventLogEntryType.Information);
		}

		/// <summary>Logs a message for entering the specified function/method.</summary>
		/// <param name="methodName">The name of the Method.</param>
		public static void LogTrace_EnterMethod(string methodName)
		{
			LogTrace("Enter: " + methodName);
		}

		/// <summary>Logs a message for entering the specified function/method.</summary>
		/// <param name="methodName">The name of the Method.</param>
		/// <param name="className">The name of the Class.</param>
		public static void LogTrace_EnterMethod(string className, string methodName)
		{
			LogTrace_EnterMethod(className + ":" + methodName);
		}

		/// <summary>Logs a message for exiting the specified function/method.</summary>
		/// <param name="methodName">The name of the Method.</param>
		public static void LogTrace_ExitMethod(string methodName)
		{
			LogTrace("Exit: " + methodName);
		}

		/// <summary>Logs a message for exiting the specified function/method.</summary>
		/// <param name="methodName">The name of the Method.</param>
		/// <param name="className">The name of the Class.</param>
		public static void LogTrace_ExitMethod(string className, string methodName)
		{
			LogTrace_ExitMethod(className + ":" + methodName);
		}
*/

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
				List<string> httpMsgs = new List<string>();

				//Loop through each exception.
				for (Exception curEx = ex; curEx != null; curEx = curEx.InnerException)
				{
					//Get fusion log (if needed).
					if (string.IsNullOrWhiteSpace(strFusion))
					{
						try
						{
							strFusion = ((dynamic)curEx).FusionLog;
						}
						catch //Ignore any error.
						{ }
					}

					//Get any SQL errors (if needed).
					if (curEx is SqlException && lstSQLErrors.Count < 1)
					{
						int counter = 1;
						foreach (SqlError error in ((SqlException)curEx).Errors)
						{
							lstSQLErrors.Add(counter.ToString() + ": " + error.Message);
							counter++;
						}
					}

					//Now get the type and message.
					string exceptMsg = curEx.Message.Replace(crlf, "  "); //Strip out any newlines there.
					lstMsgs.Add(exceptMsg + " [" + curEx.GetType().ToString() + "]");
				}

				//Now combine them all to be pretty!
				retString = "Messages:" + crlf;
				retString += indent + string.Join(crlf + indent, lstMsgs).Trim();
				retString += crlf + crlf;
				if (lstSQLErrors.Count > 0)
				{
					retString += "SQL Messages:" + crlf;
					retString += indent + string.Join(crlf + indent, lstSQLErrors).Trim();
					retString += crlf + crlf;
				}
				if (!string.IsNullOrWhiteSpace(strFusion))
				{
					retString += "Fusion Log:" + crlf;
					retString += strFusion;
					retString += crlf + crlf;
				}
				if (httpMsgs.Count > 0)
				{
					retString += "HTTP Request:" + crlf;
					retString += indent + string.Join(crlf + indent, httpMsgs).Trim();
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
}
  