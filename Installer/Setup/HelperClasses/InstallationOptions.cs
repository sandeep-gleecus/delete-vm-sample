using System;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>Stores the installation options chosen by the user</summary>
	public class InstallationOptions
	{
		/// <summary>Constructor</summary>
		public InstallationOptions()
		{
			ProductName = Themes.Inflectra.Resources.Global_DefaultProduct;
			Organization = "";
			LicenseKey = "";
			EulaAccepted = false;
			InstallationType = InstallationTypeOption.Unknown;
			DatabaseAuthentication = AuthenticationMode.Unknown;
			DatabaseSampleData = true;

			//Get the default install root folder.
			Environment.SpecialFolder folder =
				(Environment.Is64BitOperatingSystem) ?
				Environment.SpecialFolder.ProgramFilesX86 :
				Environment.SpecialFolder.ProgramFiles;
			InstallationFolder = Environment.GetFolderPath(folder);
		}

		#region Entered Information
		/// <summary>The name of the product chosen to be installed (SpiraTest, SpiraTeam, SpiraPlan)</summary>
		public string ProductName { get; set; }

		/// <summary>The name of the organization the license key is for</summary>
		public string Organization { get; set; }

		/// <summary>The license key being used</summary>
		public string LicenseKey { get; set; }

		/// <summary>Has the EULA been accepted</summary>
		public bool EulaAccepted { get; set; }

		/// <summary>The IIS Application Path (/ = Website root)</summary>
		public string IISApplication { get; set; }

		/// <summary>The IIS Application Pool Name</summary>
		public string IISApplicationPool { get; set; }

		/// <summary>The IIS Web Site Name</summary>
		/// <remarks>Null/Empty String = Default Web Site</remarks>
		public string IISWebsite { get; set; }

		/// <summary>The Installation Folder (e.g. C:\Program Files\SpiraTeam)</summary>
		public string InstallationFolder { get; set; }

		/// <summary>The Database server (and instance) (e.g. MYSERVER or MYSERVER\SQLEXPRESS)</summary>
		public string SQLServerAndInstance { get; set; }

		/// <summary>The name of the database to create (e.g. SpiraTeam)</summary>
		/// <remarks>Only changeable in Advanced installations</remarks>
		public string DatabaseName { get; set; }

		/// <summary>The name of the database to create (e.g. SpiraTeam)</summary>
		/// <remarks>Only changeable in Advanced installations</remarks>
		public string AuditDatabaseName { get; set; }

		/// <summary>The name of the database user to create (e.g. SpiraTeam)</summary>
		/// <remarks>Only changeable in Advanced installations</remarks>
		public string DatabaseUser { get; set; }

		public string AuditSQLServerAndInstance { get; set; }

		public string AuditDatabaseUser { get; set; }

		public AuthenticationMode AuditDatabaseAuthentication { get; set; }

		/// <summary>The database login to use for connecting to install</summary>
		/// <remarks>Only used for SQL Server Authentication</remarks>
		public string AuditSQLInstallLogin { get; set; }

		/// <summary>The database password to use for connecting to install</summary>
		/// <remarks>Only used for SQL Server Authentication</remarks>
		public string AuditSQLInstallPassword { get; set; }

		/// <summary>Are we using SQL Server or Windows authentication</summary>
		public AuthenticationMode DatabaseAuthentication { get; set; }

		/// <summary>The database login to use for connecting to install</summary>
		/// <remarks>Only used for SQL Server Authentication</remarks>
		public string SQLInstallLogin { get; set; }

		/// <summary>The database password to use for connecting to install</summary>
		/// <remarks>Only used for SQL Server Authentication</remarks>
		public string SQLInstallPassword { get; set; }

		/// <summary>The database login to create</summary>
		/// <remarks>Only used for SQL Server Authentication and only changeable for Advanced installations</remarks>
		public string SQLLoginToCreate { get; set; }

		/// <summary>Should the database include sample data</summary>
		public bool DatabaseSampleData { get; set; }

		/// <summary>Whether the installation is Advanced or not.</summary>
		public bool IsAdvancedInstall { get; set; }

		/// <summary>The type of installation</summary>
		public InstallationTypeOption InstallationType { get; set; }

		/// <summary>The directory that the database will be backuped up into.</summary>
		/// <remarks>NOT the name, as the name will (always) be uniquely generated.</remarks>
		public string DBBackupDirectory { get; set; }

		/// <summary>Contains the three-digit version of an existing DB when upgrading.</summary>
		/// <remarks>Used for backup database name currently. In future, used for Upgrade process.</remarks>
		public int DBExistingRevision { get; set; }

		/// <summary>If true, user selected to forego the database backup.</summary>
		public bool NoBackupDB { get; set; }

		/// <summary>The SQL Version that we're talking to.</summary>
		public string SQLServerVer { get; set; }

		/// <summary>Temporarily used for Upgrade situations. Loaded from existing web.config</summary>
		public string ExistingConnectionString { get; set; }
		#endregion Entered Information

		#region Command Line Options
		public CommonOptions CommandLine { get; set; }
		#endregion Command Line options

		#region DataSync Options
		internal SettingsDataSync DataSyncFile { get; set; }
		#endregion DataSync Options

		#region Existing WebConfig Values
		/// <summary>
		/// If an upgrade, this will contain all the previous installation settings.
		/// </summary>
		internal WebConfigSettings existingSettings { get; set; }
		#endregion Existing WebConfig Values
	}

	/// <summary>The different installation types</summary>
	public enum InstallationTypeOption
	{
		Unknown = 0,
		CleanInstall = 1,
		Upgrade = 2,
		AddApplication = 3,
		DatabaseUpgrade = 4,
		Uninstall = 5
	}

	/// <summary>The different database authentication modes</summary>
	public enum AuthenticationMode
	{
		Unknown = 0,
		Windows = 1,
		SqlServer = 2
	}
}
