using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Creates the uninstall entry in the Add/Remove programs section of the Windows control panel</summary>
		private bool Install_RegisterInControlPanel(StreamWriter streamWriter, out FinalStatusEnum status)
		{
			const string UninstallRegKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

			int TaskDisplayLine = 10;
			status = FinalStatusEnum.OK;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });
			streamWriter.WriteLine("Creating the uninstall entry in the Add/Remove programs section of the Windows control panel");

			try
			{
				//Copy the installer application to our storage folder..
				//The location of the exe. Make sure it exists.
				string storeFolder = Path.Combine(new string[] {
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					Themes.Inflectra.Resources.Global_ManufacturerName,
					"Installs" }
				);
				Directory.CreateDirectory(storeFolder);

				//Get the name of the final exe, and make the fuill path.
				string uninstExe = Properties.Settings.Default.ProductTemplateName + "_" + Properties.Settings.Default.ProductVersion + ".exe";
				string storeExe = Path.Combine(new string[] { storeFolder, uninstExe });

				//Copy the file over.
				string currentLocation = Assembly.GetExecutingAssembly().Location;
				File.Copy(currentLocation, storeExe, true);

				//Next, write the uninstall entries into the registry for display in the control panel
				streamWriter.WriteLine("Writing the uninstall registry entries.");
				using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(UninstallRegKeyPath, true))
				{
					if (parent == null)
					{
						throw new Exception("Uninstall registry key not found.");
					}
					try
					{
						RegistryKey key = null;

						try
						{
							string guidText = Constants.UNINSTALL_GUID.ToString("B");
							key = parent.OpenSubKey(guidText, true) ??
								  parent.CreateSubKey(guidText);

							if (key == null)
							{
								throw new Exception(String.Format("Unable to create uninstaller '{0}\\{1}'", UninstallRegKeyPath, guidText));
							}

							Assembly asm = GetType().Assembly;
							string exe = "\"" + asm.CodeBase.Substring(8).Replace("/", "\\\\") + "\"";

							key.SetValue("DisplayName", App._installationOptions.ProductName);
							key.SetValue("ApplicationVersion", Properties.Settings.Default.ProductVersion);
							key.SetValue("Publisher", Themes.Inflectra.Resources.Global_ManufacturerLegalName);
							key.SetValue("DisplayIcon", exe);
							key.SetValue("DisplayVersion", Properties.Settings.Default.ProductVersion);
							key.SetValue("Contact", Themes.Inflectra.Resources.Global_ManufacturerLegalName);
							key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
							key.SetValue("UninstallString", storeExe + " uninstall");
							key.SetValue("ModifyPath", storeExe);
							key.SetValue("URLInfoAbout", Themes.Inflectra.Resources.Global_ManufacturerWebsite + "/" + App._installationOptions.ProductName);
							key.SetValue("Comments", App._installationOptions.ProductName + " - " + Themes.Inflectra.Resources.Global_Tagline);
							key.SetValue("HelpLink", Themes.Inflectra.Resources.Global_HelpLink);
							key.SetValue("HelpTelephone", Themes.Inflectra.Resources.Global_HelpTelephone);
							key.SetValue("DisplayIcon", Path.Combine(App._installationOptions.InstallationFolder, "App_Themes", Themes.Inflectra.Resources.Global_ThemeName, App._installationOptions.ProductName + "-FavIcon.ico"));
						}
						finally
						{
							if (key != null)
							{
								key.Close();
							}
						}
					}
					catch (Exception ex)
					{
						throw new Exception(
							"An error occurred writing uninstall information to the registry.  The product is fully installed but can only be uninstalled manually through the command line.",
							ex);
					}
				}

                //Finally we also add a shortcut to the installer in the program files
                string manufacturer = Themes.Inflectra.Resources.Global_ManufacturerName;
                string product = App._installationOptions.ProductName;
                Utilities.AddAppShortcutToStartMenu(manufacturer + "/" + product, "Setup", storeExe);

                ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to create the control panel uninstall entry: " + ex.Message);

				string ErrorMsg = "Unable to create the control panel uninstall entry:" + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}
	}
}
