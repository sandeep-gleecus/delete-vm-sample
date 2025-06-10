using Inflectra.SpiraTest.Installer.ControlForm;
using Inflectra.SpiraTest.Installer.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for wpfMasterForm.xaml</summary>
	public partial class wpfMasterForm : Window
	{
		// Main Wizard form.
		private ControlForm.ProcedureDialog wizard;

		// Settings pages.
		private cntrlIntroduction wiz_Intro;
		private cntrlConfirmation wiz_Confirm;
		private cntrlInstallProgress wiz_Progress;
		private cntrlResults wiz_Results;

		private cntrlPrerequisites wiz_Prerequisites;
		private cntrlLicenseKey wiz_LicenseKey;
		private cntrlEula wiz_Eula;
		private cntrlInstallationType wiz_InstallationType;
		private cntrlChooseLocation wiz_ChooseLocation;
		private cntrlWebServer wiz_WebServer;
		private cntrlDatabaseServer wiz_DatabaseServer;
		private cntrlDatabaseBackup wiz_DatabaseBackup;

		/// <summary>Creates the wizard window.</summary>
		private void SetWizardWindowProperties()
		{
			//Add config forms..
			wiz_Intro = new cntrlIntroduction();
			wiz_Confirm = new cntrlConfirmation();
			wiz_Progress = new cntrlInstallProgress();
			wiz_Results = new cntrlResults();
			wiz_Prerequisites = new cntrlPrerequisites();
			wiz_LicenseKey = new cntrlLicenseKey();
			wiz_Eula = new cntrlEula();
			wiz_InstallationType = new cntrlInstallationType();
			wiz_ChooseLocation = new cntrlChooseLocation();
			wiz_WebServer = new cntrlWebServer();
			wiz_DatabaseServer = new cntrlDatabaseServer();
			wiz_DatabaseBackup = new cntrlDatabaseBackup();

			wiz_Eula = new cntrlEula();
			List<IProcedureComponent> wizard_Steps = new List<ControlForm.IProcedureComponent>
			{
				wiz_InstallationType,	//Always, cept when command-line given?
				wiz_Eula,				//QAl except Uninstall
				wiz_Prerequisites,		//NewInstall
				wiz_LicenseKey,			//NewInstall
				wiz_ChooseLocation,		//NewInstall, AddApplication
				wiz_WebServer,			//NewInstall, AddApplication
				wiz_DatabaseServer,		//NewInstall, AddApplication, DatabaseUpgrade
				wiz_DatabaseBackup		//FullUpgrade, DatabaseUpgrade, Uninstall
				//wiz_Uninstall?
			};

			//Individual page events.
			wiz_Progress.IsVisibleChanged += wiz_Progress_IsVisibleChanged;

			//Get the installer version.
			Version instVer = new Version(Properties.Settings.Default.ProductVersion);
			string title = string.Format(Themes.Inflectra.Resources.Procedure_Title, Utilities.GetTruncatedVersion(instVer));

			wizard = new ProcedureDialog(title,
				wiz_Intro,
				wizard_Steps,
				wiz_Confirm,
				wiz_Progress,
				wiz_Results)
			{
				ShowProcessWarning = false,
				HeaderTitle = title,
				HeaderDescription = Themes.Inflectra.Resources.Procedure_Description
			};
			wizard.NavigationChanging += wizard_NavigationChange;
			wizard.Deactivated += wizard_Deactivated;
			wizard.Closed += wizard_Closed;
			wizard.Closing += wizard_Closing;
			wizard.ShowProcessWarning = false;
			IProcedureComponent proc = wizard.ConfigurationComponents.FirstOrDefault(t => t.UniqueName == "cntrlInstallationType");
			if (proc is cntrlInstallationType)
			{
				cntrlInstallationType scr = (cntrlInstallationType)proc;
				scr.AdvanceFunction = wizard.JumpToScreen;
			}
		}

		#region Wizard Events

		#region Prevent hidden applications taking focus.
		/* To stop the Wizard from losing focus */
		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();
		[DllImport("user32.dll")]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
		[DllImport("kernel32.dll")]
		static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);
		[DllImport("psapi.dll")]
		static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
		void wizard_Deactivated(object sender, EventArgs e)
		{
			const int nChars = 1024;
			IntPtr handle = IntPtr.Zero;
			uint access = 1040;
			//Get the window handle.
			handle = GetForegroundWindow();

			//Get the process.
			uint processId = 0;
			StringBuilder filename = new StringBuilder(nChars);
			GetWindowThreadProcessId(handle, out processId);
			IntPtr hProcess = OpenProcess(access, 0, processId);
			GetModuleFileNameEx(hProcess, IntPtr.Zero, filename, nChars);

			if (filename.ToString().ToLowerInvariant().EndsWith("ea.exe"))
			{
				wizard.Activate();
				wizard.Focus();
			}
		}
		#endregion

		/// <summary>Hit when the Progress first becomes visible.</summary>
		/// <param name="sender">wiz_Progress</param>
		/// <param name="e">EventArgs</param>
		void wiz_Progress_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (wiz_Progress.IsVisible)
			{
				//Set the action, in case they wanted to use the egg.
				//this.wiz_Progress.SpiraProject = this.wiz_optSpira.SelectedProject;
				//this.wiz_Progress.EAFile = this.wiz_optEA.SelectedFile;
			}
		}

		/// <summary>Hit when the wizard is navigated to another screen.</summary>
		/// <param name="sender">What to do. If an int, next/back/start. If a string, it jumps to the page specified by name.</param>
		private void wizard_NavigationChange(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			//Check that it was a forward move (i.e. clicked on Next)
			if (sender is int)
			{
				if ((int)sender > -1)
				{
					//Get the current component and check if is valid
					if (!wizard.CurrentComponent.IsValid())
					{
						e.Handled = false;
					}
				}
			}
			else if (sender is string)
			{
				//TODO: Jump to page.
			}
		}

		/// <summary>Hit when the wizard form is closing.</summary>
		/// <param name="sender">ProgressForm</param>
		/// <param name="e">EventArgs</param>
		private void wizard_Closed(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>Hit when the wizard is about to close.</summary>
		/// <param name="sender">wizard</param>
		/// <param name="e">EventArgs</param>
		private void wizard_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//Do Nothing
		}
		#endregion
	}
}
