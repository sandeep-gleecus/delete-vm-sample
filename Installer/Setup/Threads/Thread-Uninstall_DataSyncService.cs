using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using System;
using System.IO;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Installs (but does not start) the data synchronization service</summary>
		private bool Uninstall_DataSyncService(StreamWriter streamWriter)
		{
			streamWriter.WriteLine("Uninstall_DataSyncService - Starting");

			int TaskDisplayLine = 16;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });

			try
			{
				//Get the application to remove the entry.
				string dotNetFolder = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
				string serviceFolder = Path.Combine(App._installationOptions.InstallationFolder, "DataSync");
				string serviceExeFile = Path.Combine(serviceFolder, Constants.DATA_SYNC_EXE);

				//C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe /u "C:\Program Files\SpiraTeam\bin\DataSyncService.exe"
				string installUtility = Path.Combine(dotNetFolder, "installutil.exe");
				string res = Utilities.ExecuteShellCommand(installUtility, dotNetFolder, "/u \"" + serviceExeFile + "\"");
				streamWriter.WriteLine("Unregistering Data Sync Service:" + Environment.NewLine + res);

				streamWriter.WriteLine("Uninstall_DataSyncService - Finished");
				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to install the data synchronization service: " + Environment.NewLine + Logger.DecodeException(ex));

				string ErrorMsg = "Unable to install the data synchronization service:" + Environment.NewLine + ex.Message;

				streamWriter.WriteLine("Uninstall_DataSyncService - Finished");
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}
	}
}
