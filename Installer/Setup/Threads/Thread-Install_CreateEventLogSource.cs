using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using System;
using System.IO;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Creates the Event Log source for the product</summary>
		private bool Install_EventLogSource(StreamWriter streamWriter, out FinalStatusEnum status)
		{
			int TaskDisplayLine = 6;
			status = FinalStatusEnum.OK;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });
			streamWriter.WriteLine("Creates the Event Log source for the product");

			try
			{
				string eventLogSourceApp = String.Format(Constants.EVENT_LOG_SOURCE_APPLICATION, App._installationOptions.ProductName);
				if (!System.Diagnostics.EventLog.SourceExists(eventLogSourceApp))
				{
					streamWriter.WriteLine("Creating the application event log source");
					System.Diagnostics.EventLog.CreateEventSource(eventLogSourceApp, Constants.EVENT_LOG_APPLICATION);
				}

				string eventLogSourceDataSync = String.Format(Constants.EVENT_LOG_SOURCE_DATA_SYNC, App._installationOptions.ProductName);
				if (!System.Diagnostics.EventLog.SourceExists(eventLogSourceDataSync))
				{
					streamWriter.WriteLine("Creating the data sync service event log source");
					System.Diagnostics.EventLog.CreateEventSource(eventLogSourceDataSync, Constants.EVENT_LOG_APPLICATION);
				}

				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Failed to create the event log source for the product: " + ex.Message);

				string ErrorMsg = "Failed to create the event log source for the product:" + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}
	}
}
