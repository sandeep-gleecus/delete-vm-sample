using Inflectra.SpiraTest.Installer.UI;
using System;
using System.IO;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Backs up the database.</summary>
		private bool Final_CleanupInstallFiles(StreamWriter streamWriter)
		{
			int TaskDisplayLine = 99;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });

			try
			{
				streamWriter.WriteLine("Cleaning up installation location.");

				//Remove the ZIP file and extracted contents.
				string sourceZip = Path.Combine(TempFolder, Properties.Settings.Default.ApplicationPackage);
				if (File.Exists(sourceZip))
					File.Delete(sourceZip);
				string sourceExtracted = Path.Combine(Path.GetDirectoryName(sourceZip), Path.GetFileNameWithoutExtension(sourceZip));
				if (Directory.Exists(sourceExtracted))
					Directory.Delete(sourceExtracted, true);
			}
			catch (Exception ex)
			{
				//Log error.
				string msg = "Error cleaning up install files: ";
				streamWriter.WriteLine(msg + ex.Message);

				string ErrorMsg = msg + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}

			return false;
		}
	}
}
