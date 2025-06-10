using Inflectra.SpiraTest.Installer.UI;
using System;
using System.IO;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Creates the program menu shortcut entries</summary>
		private bool Uninstall_ProgramMenuEntries(StreamWriter streamWriter)
		{
			streamWriter.WriteLine("Uninstall_ProgramMenuEntries - Starting");

			int TaskDisplayLine = 13;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });

			try
			{
				//Get the properties
				string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
				string appStartMenuItem = Themes.Inflectra.Resources.Global_ManufacturerName + "\\" + App._installationOptions.ProductName;
				string appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs", appStartMenuItem);

				//Remove the directory, if it exists.
				if (Directory.Exists(appStartMenuPath))
					Directory.Delete(appStartMenuPath, true);

				streamWriter.WriteLine("Uninstall_ProgramMenuEntries - Finished");
				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to remove the program menu shortcut entries: " + Environment.NewLine + Logger.DecodeException(ex));

				string ErrorMsg = "Unable to remove the program menu shortcut entries:" + Environment.NewLine + ex.Message;
				streamWriter.WriteLine("Uninstall_DataSyncService - Finished");
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}
	}
}
