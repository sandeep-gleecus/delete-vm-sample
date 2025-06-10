using Inflectra.SpiraTest.Installer.HelperClasses.CommandLine;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>Base options available no matter what verb is used.</summary>
	public class CommonOptions
	{
		#region Base Options
		/// <summary>If true, we output more information. Not currently implemented.</summary>
		[Option('v', "verbose", Default = false, HelpText = "Output debugging info. Not implemented.")]
		public bool Verbose { get; set; }

		/// <summary>If true, we assume the user is running in Advanced mode.</summary>
		[Option('a', "advanced", Default = false, HelpText = "Enable advanced options.")]
		public bool Advanced { get; set; }

		/// <summary>Returns the processed install type.</summary>
		public InstallationTypeOption InstallType
		{
			get
			{
				InstallationTypeOption retVal = InstallationTypeOption.Unknown;

				if (this is InstallOptions)
				{
					retVal = ((InstallOptions)this).AppOnly ?
						InstallationTypeOption.AddApplication :
						InstallationTypeOption.CleanInstall;
				}
				else if (this is UpgradeOptions)
				{
					retVal = ((UpgradeOptions)this).OnlyDB ?
						InstallationTypeOption.DatabaseUpgrade :
						InstallationTypeOption.Upgrade;
				}
				else if (this is UninstallOptions)
				{
					retVal = InstallationTypeOption.Uninstall;
				}

				return retVal;
			}
		}

		/// <summary>The organization name.</summary>
		[Option("orgname", Default = null, HelpText = "The organization name the license key is issued for.")]
		public string OrgName { get; set; }

		/// <summary>The organization name.</summary>
		[Option("orgkey", Default = null, HelpText = "The license key.")]
		public string LicenseKey { get; set; }

		/// <summary>Whether to skip the license check or not.</summary>
		[Option("skiplic", Default = false, HelpText = "Will skip the license check, requiring it to be entered into the application after install.")]
		public bool SkipLicenseCheck { get; set; }
		#endregion Base options

		#region Paths
		/// <summary>The locaiton to Install To, Uninstall From, or Upgrade - if specified.</summary>
		[Option("installdir", HelpText = "The location of the installation.")]
		public string InstallDir { get; set; }

		/// <summary>The locaiton to save the database backup. Used in upgrading & uninstall.</summary>
		[Option("dbbackupdir", HelpText = "The location on the SQL Server where to store the database backup.")]
		public string DBBackupDir { get; set; }
		#endregion Paths

		#region SQLLoginInfo
		/// <summary>The instance name of the SQL Server.</summary>
		[Option("sqlinstance", HelpText = "The server and instance name of the SQl Server to install to.")]
		public string SQLServerInstance { get; set; }

		/// <summary>The login to use for the SQL Server. If empty, uses Windows Auth</summary>
		[Option("sqllogin", HelpText = "The admin login of the SQL Server to create and modify the database.")]
		public string SQLServerLogin { get; set; }

		/// <summary>The password to use for the SQL Server login.</summary>
		[Option("sqlpass", HelpText = "The password for the admin login of the SQL Server.")]
		public string SQLServerPassword { get; set; }

		#endregion
	}

	[Verb("uninstall", HelpText = "Uninstall the application.")]
	public class UninstallOptions : CommonOptions
	{
		/// <summary>The locaiton to save the database backup. Used in upgrading & uninstall.</summary>
		[Option("backupdb", Default = false, HelpText = "Whether to backup the database on uninstall.")]
		public bool BackupDB { get; set; }
	}

	[Verb("upgrade", HelpText = "Upgrade the application.")]
	public class UpgradeOptions : CommonOptions
	{
		/// <summary>The locaiton to save the database backup. Used in upgrading & uninstall.</summary>
		[Option("dbonly", Default = false, HelpText = "Whether we're only doing a database upgrade.")]
		public bool OnlyDB { get; set; }
	}

	[Verb("install", HelpText = "Install the application.")]
	public class InstallOptions : CommonOptions
	{
		/// <summary>Whether this install will be an application only install.</summary>
		[Option("apponly", Default = false, HelpText = "Whether or not we are only installing an application server.")]
		public bool AppOnly { get; set; }

		/// <summary>The name of the website to install the application to.</summary>
		[Option("website", Default = null, HelpText = "The name of the configured website to install the application into.")]
		public string WebsiteName { get; set; }

		/// <summary>The virtual directory name to install the application into.</summary>
		[Option("webpath", Default = null, HelpText = "The path to install the application into.")]
		public string VirtualDirectory { get; set; }

		/// <summary>The name of the Application pool, if needed to change.</summary>
		[Option("webpoolname", Default = null, HelpText = "The name of the Application Pool to run the application.")]
		public string AppPoolName { get; set; }

		/// <summary>The name of the user to connect to the database. Ignored is using Windows Auth</summary>
		[Option("dbuser", Default = null, HelpText = "The name of the user that the application will connect as.")]
		public string DatabaseUser { get; set; }

		/// <summary>The name of the database.</summary>
		[Option("dbname", Default = null, HelpText = "The name of the database to install.")]
		public string DatabaseName { get; set; }

		/// <summary>Whether to install Sample Data or not.</summary>
		[Option("sampledata", Default = true, HelpText = "Wither or not to install Sample data on a clean install.")]
		public bool InstallSampleData { get; set; }
	}
}
