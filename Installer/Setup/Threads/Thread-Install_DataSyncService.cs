using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using System;
using System.IO;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Installs (but does not start) the data synchronization service</summary>
		private bool Install_DataSyncService(StreamWriter streamWriter, out FinalStatusEnum status)
		{
			int TaskDisplayLine = 8;
			status = FinalStatusEnum.OK;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });
			streamWriter.WriteLine("Installing the data synchronization service");

			try
			{
				//Get the various properties
				string serviceFolder = Path.Combine(App._installationOptions.InstallationFolder, "DataSync");
				string productName = App._installationOptions.ProductName;
				string vdirName = App._installationOptions.IISApplication;
				string serviceEventLog = string.Format(Constants.EVENT_LOG_SOURCE_DATA_SYNC, productName);

				//Read in the web.config file
				streamWriter.WriteLine("Updating the data synchronization service configuration file");
				string configText;
				string serviceConfigFile = Path.Combine(serviceFolder, Constants.DATASYNC_CONFIG);
				string serviceExeFile = Path.Combine(serviceFolder, Constants.DATA_SYNC_EXE);
				using (StreamReader streamReader = File.OpenText(serviceConfigFile))
				{
					configText = streamReader.ReadToEnd();
					streamReader.Close();
				}

				//Replace the various tokens
				configText = configText.Replace("[VDIR]", vdirName);
				configText = configText.Replace("[EVENTLOGSOURCE]", serviceEventLog);

				//Write the file back
				File.Delete(serviceConfigFile);
				using (StreamWriter streamWriter2 = File.CreateText(serviceConfigFile))
				{
					streamWriter2.Write(configText);
					streamWriter2.Close();
				}

				//Now actually create the service
				string dotNetFolder = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
				streamWriter.WriteLine("Creating the data synchronization Windows service");

				//C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe "C:\Program Files\SpiraTeam\bin\DataSyncService.exe"
				string installUtility = Path.Combine(dotNetFolder, "installutil.exe");
				string res = Utilities.ExecuteShellCommand(installUtility, dotNetFolder, "\"" + serviceExeFile + "\"");
				streamWriter.WriteLine("Registering Data Sync Service:" + Environment.NewLine + res);

				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to install the data synchronization service: " + ex.Message);

				string ErrorMsg = "Unable to install the data synchronization service:" + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}
	}
}
