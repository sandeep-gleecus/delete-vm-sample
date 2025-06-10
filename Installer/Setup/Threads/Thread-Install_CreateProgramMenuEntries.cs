using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using System;
using System.IO;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Creates the program menu shortcut entries</summary>
		private bool Install_CreateProgramMenuEntries(StreamWriter streamWriter, out FinalStatusEnum status)
		{
			int TaskDisplayLine = 9;
			status = FinalStatusEnum.OK;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });
			streamWriter.WriteLine("Creating the program menu shortcut entries");

			try
			{
				//Get the properties
				string manufacturer = Themes.Inflectra.Resources.Global_ManufacturerName;
				string product = App._installationOptions.ProductName;
				string iconFolder = Path.Combine(App._installationOptions.InstallationFolder, "Icons");

				string appRoot = "http://localhost";
				if (!string.IsNullOrEmpty(App._installationOptions.IISApplicationPool) && App._installationOptions.IISApplicationPool != "/")
				{
					if (!App._installationOptions.IISApplication.StartsWith("/"))
						appRoot += "/";
					appRoot += App._installationOptions.IISApplication;
				}

				//Create the various program menu shortcuts
				//Home Page of the application
				Utilities.AddUrlShortcutToStartMenu(manufacturer + "/" + product,
					product + " Home Page",
					appRoot,
					Path.Combine(iconFolder, "ProductIcon.ico")
				);

				//Shortcut to online documentation
				Utilities.AddUrlShortcutToStartMenu(manufacturer + "/" + product,
					"Documentation Online",
					Themes.Inflectra.Resources.Global_OnlineDocumentationUrl,
					Path.Combine(iconFolder, "Html.ico")
				);

				//New SpiraDoc Quick-Start Guide
				Utilities.AddUrlShortcutToStartMenu(manufacturer + "/" + product,
					"Quick Start Guide",
					"http://spiradoc.inflectra.com/" + product + "-Quick-Start-Guide/",
					Path.Combine(iconFolder, "Help.ico")
				);

				//New SpiraDoc User Manual.
				Utilities.AddUrlShortcutToStartMenu(manufacturer + "/" + product,
					"Online User Manual",
					"https://spiradoc.inflectra.com/Spira-User-Manual/",
					Path.Combine(iconFolder, "Help.ico")
				);

				//New SpiraDoc Admin Manual.
				Utilities.AddUrlShortcutToStartMenu(manufacturer + "/" + product,
					"Online Administration Setup",
					"http://spiradoc.inflectra.com/Spira-Administration-Guide/",
					Path.Combine(iconFolder, "Help.ico")
				);

				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to create the program menu shortcut entries: " + ex.Message);

				string ErrorMsg = "Unable to create the program menu shortcut entries:" + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}
	}
}
