using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

using Inflectra.SpiraTest.Installer.ControlForm;
using Inflectra.SpiraTest.Installer.HelperClasses;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>
	/// Interaction logic for cntrlConfirmation.xaml
	/// </summary>
	public partial class cntrlConfirmation : UserControl, IProcedureComponent
	{
		public cntrlConfirmation()
		{
			InitializeComponent();

			//Set up event.
			Loaded += cntrlConfirmation_Loaded;
		}

		/// <summary>Loads the summary text into the screen, so people know what the heel they're doing before clicking 'Install'.</summary>
		private void cntrlConfirmation_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			//First, handle some needed processing. If not an Advanced or uninstall, we need to
			//  specifiy the install path.
			if (!App._installationOptions.IsAdvancedInstall &&
				App._installationOptions.InstallationType == InstallationTypeOption.CleanInstall)
				App._installationOptions.InstallationFolder = Path.Combine(App._installationOptions.InstallationFolder, App._installationOptions.ProductName);

			//See if we need to display the DB Backup path..
			if (App._installationOptions.InstallationType == InstallationTypeOption.Upgrade ||
				App._installationOptions.InstallationType == InstallationTypeOption.DatabaseUpgrade ||
				App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
			{
				LoadDBBackupDirAndVersion();
			}

			//Set the intro text!
			string introBut = Themes.Inflectra.Resources.Global_Install;
			if (App._installationOptions.InstallationType == InstallationTypeOption.Upgrade ||
				App._installationOptions.InstallationType == InstallationTypeOption.DatabaseUpgrade)
				introBut = Themes.Inflectra.Resources.Global_Upgrade;
			else if (App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
				introBut = Themes.Inflectra.Resources.Global_Uninstall;
			txtIntro.Text = string.Format(Themes.Inflectra.Resources.Confirmation_Description, introBut);
			txtSubtitle.Text = string.Format(Themes.Inflectra.Resources.Confirmation_StartInstallLegend, introBut);

			//Generate the output string.
			string display = "";

			//What are we doing.
			display += "Install Type: ";
			switch (App._installationOptions.InstallationType)
			{
				case InstallationTypeOption.AddApplication:
					display += "Adding Application Server";
					break;
				case InstallationTypeOption.CleanInstall:
					display += "Full Install";
					break;
				case InstallationTypeOption.DatabaseUpgrade:
					display += "Updating Database";
					break;
				case InstallationTypeOption.Uninstall:
					display += "Uninstallation";
					break;
				case InstallationTypeOption.Upgrade:
					display += "Full Upgrade";
					break;
				case InstallationTypeOption.Unknown:
				default:
					display += "Ending The World";
					break;
			}
			display += Environment.NewLine + Environment.NewLine;

			//Application Information.
			if (App._installationOptions.InstallationType == InstallationTypeOption.Upgrade ||
				App._installationOptions.InstallationType == InstallationTypeOption.AddApplication ||
				App._installationOptions.InstallationType == InstallationTypeOption.CleanInstall ||
				App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
			{
				display += "Application Information:" + Environment.NewLine;

				//File Path install.
				display += "  -- Install Location: " + App._installationOptions.InstallationFolder + Environment.NewLine;
				display += "  -- Website/App Path: " + App._installationOptions.IISWebsite + App._installationOptions.IISApplication + Environment.NewLine;
				display += "  -- Website App Pool: " + App._installationOptions.IISApplicationPool + Environment.NewLine;

				//Add a line.
				display += Environment.NewLine;
			}

			//Database Information.
			if (App._installationOptions.InstallationType == InstallationTypeOption.Upgrade ||
				App._installationOptions.InstallationType == InstallationTypeOption.AddApplication ||
				App._installationOptions.InstallationType == InstallationTypeOption.CleanInstall ||
				App._installationOptions.InstallationType == InstallationTypeOption.Uninstall ||
				App._installationOptions.InstallationType == InstallationTypeOption.DatabaseUpgrade)
			{
				//App._installationOptions.DatabaseBackupPath
				//App._installationOptions.DatabaseSampleData

				display += "Database Information:" + Environment.NewLine;
				display += "  -- SQL Server/Instance:   " +
					((App._installationOptions.SQLServerAndInstance == ".") ?
					"<local>" :
					App._installationOptions.SQLServerAndInstance) + Environment.NewLine;
				display += "  -- Database Name:         " + App._installationOptions.DatabaseName + Environment.NewLine;
				string authTypeDisplay = "Unknown";
				if (App._installationOptions.DatabaseAuthentication == AuthenticationMode.SqlServer)
				{
					authTypeDisplay = "SQL Server Authentication";
				}
				if (App._installationOptions.DatabaseAuthentication == AuthenticationMode.Windows)
				{
					authTypeDisplay = "Windows Authentication";
				}
				display += "  -- SQL Login Type:        " + authTypeDisplay + Environment.NewLine;
				if (App._installationOptions.DatabaseAuthentication == AuthenticationMode.SqlServer)
					display += "  -- Installing User:       " + App._installationOptions.SQLInstallLogin + Environment.NewLine;
				display += "  -- Application SQL Login: " + App._installationOptions.DatabaseUser + Environment.NewLine;
				display += "  -- Application DB User:   " + App._installationOptions.DatabaseUser + Environment.NewLine;

				if (App._installationOptions.InstallationType == InstallationTypeOption.CleanInstall)
					display += "  -- Install Sample Data:   " + ((App._installationOptions.DatabaseSampleData) ? "Yes" : "No") + Environment.NewLine;

				if (App._installationOptions.InstallationType != InstallationTypeOption.CleanInstall &&
					!string.IsNullOrWhiteSpace(App._installationOptions.DBBackupDirectory) &&
					!App._installationOptions.NoBackupDB)
					display += "  -- DB Backup Location:    " + App._installationOptions.DBBackupDirectory + Environment.NewLine;

				//Display warning here!
				if (App._installationOptions.NoBackupDB && App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
					display += Environment.NewLine + Themes.Inflectra.Resources.Confirmation_UninstallNoDBBackup;
				else if (App._installationOptions.NoBackupDB &&
					(App._installationOptions.InstallationType == InstallationTypeOption.Upgrade || App._installationOptions.InstallationType == InstallationTypeOption.DatabaseUpgrade))
					display += Environment.NewLine + Themes.Inflectra.Resources.Confirmation_UpgradeNoDBBackup;
			}

			//Set the output.
			txbSummary.Text = display;

			//Add some data ONLY for the log file.
			// - Current Installer Version
			try
			{
				string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
				string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString();
				string ver = "Installer Version: " +
					assemblyVersion + "; " +
					fileVersion;
				string sver = "SQL Server: " + App._installationOptions.SQLServerVer;

				display += Environment.NewLine + ver + Environment.NewLine + sver;
			}
			catch (Exception) { }

			//Log it to the file, so we have record.
			App.logFile.WriteLine("Work Summary:" + Environment.NewLine + display);
			App.logFile.Flush(); //Force everything out to this point.
		}

		#region IProcedureComponent Members
		public string KeyText => Themes.Inflectra.Resources.Confirmation_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlConfirmation";
		public bool AllowBack => true;
		public bool AllowNext => true;

		public bool IsAvailable => true; //Always confirm.

		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		/// <summary>Can we proceed to the next step in the wizard</summary>
		/// <returns>True if ready to proceed and all settings OK</returns>
		public bool IsValid()
		{
			return true;
		}
		#endregion

		public void LoadDBBackupDirAndVersion()
		{
			//Force the cursor to 'wait' mode.
			Mouse.OverrideCursor = Cursors.Wait;

			//Get the various properties
			string dbServer = App._installationOptions.SQLServerAndInstance;
			AuthenticationMode dbAuthType = App._installationOptions.DatabaseAuthentication;
			string dbName = App._installationOptions.DatabaseName;
			string auditdbName = App._installationOptions.AuditDatabaseName;
			string dbUser = App._installationOptions.SQLInstallLogin;
			string dbPassword = App._installationOptions.SQLInstallPassword;
			string companyName = App._installationOptions.Organization;
			string licenseKey = App._installationOptions.LicenseKey;
			string productType = App._installationOptions.ProductName;
			string themeName = Themes.Inflectra.Resources.Global_ThemeName;
			string auditdbServer = App._installationOptions.AuditSQLServerAndInstance;
			AuthenticationMode auditdbAuthType = App._installationOptions.AuditDatabaseAuthentication;
			string auditdbUser = App._installationOptions.AuditSQLInstallLogin;
			string auditdbPassword = App._installationOptions.AuditSQLInstallPassword;

			//Get the backup location, if we don't know it yet.
			if (string.IsNullOrWhiteSpace(App._installationOptions.DBBackupDirectory))
			{
				//Load it from the command-line settings, if known. Otherwise, get it from server.
				if (!string.IsNullOrWhiteSpace(App._installationOptions.CommandLine?.DBBackupDir))
					App._installationOptions.DBBackupDirectory = App._installationOptions.CommandLine?.DBBackupDir;
				else
					App._installationOptions.DBBackupDirectory =
						SQLUtilities.GetSQLBackupDir(null, dbServer, dbAuthType, dbUser, dbPassword);

			}

			//Get the currect DB version.
			App._installationOptions.DBExistingRevision =
				SQLUtilities.GetExistingDBRevision(new DBConnection(dbName, auditdbName, dbServer, dbAuthType, dbUser, dbPassword, auditdbServer, auditdbAuthType, auditdbUser,auditdbPassword));

			App._installationOptions.SQLServerVer = SQLUtilities.GetSQLServerVer(new DBConnection(dbName, auditdbName, dbServer, dbAuthType, dbUser, dbPassword, auditdbServer, auditdbAuthType, auditdbUser, auditdbPassword));
			//TODO: Throw error if we have no DB revision. (i.e. Cannot find database.)

			//Clear the wait.
			Mouse.OverrideCursor = null;
		}

		public string SummaryText
		{
			get
			{
				return this.txbSummary.Text;
			}
			set
			{
				this.txbSummary.Text = value;
			}
		}
	}
}
