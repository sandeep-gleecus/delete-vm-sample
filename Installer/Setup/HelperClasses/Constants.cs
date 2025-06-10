using System;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>Contants various application-wide constants</summary>
	public static class Constants
	{
		public const int SQL_ERROR_INSTANCE_NOT_FOUND = 20;
		public const int SQL_ERROR_LOGIN_FAILED = 18456;

		public const string REQUIRED_DB_ROLE = "db_owner";
		public const int REQUIRED_SQL_VERSION = 10;
		public const string REQUIRED_SQL_VERSION_NAME = "SQL Server 2008";
		public const string DATABASE_BACKUP_FILE_FORMAT = "{0}_Backup.bak";
		public const string DATABASE_BACKUP_FILE_EXTENSION = ".bak";
		public const string WINDOWS_AUTH_LOGIN = "NT AUTHORITY\\NETWORK SERVICE";
		public const string DATA_SYNC_EXE = "DataSyncService.exe";

		public const string WEB_CONFIG = "Web.Config";
		public const string DATASYNC_CONFIG = "DataSyncService.exe.config";

		//Event log sources
		public const string EVENT_LOG_APPLICATION = "Application";
		public const string EVENT_LOG_SOURCE_APPLICATION = "{0}";
		public const string EVENT_LOG_SOURCE_DATA_SYNC = "{0} Data Sync Service";

		//Title used by some of the background thread message boxes
		public const string MESSAGE_TITLE = "Setup Message";

		/// <summary>
		/// The SQL Server command timeout
		/// </summary>
		public const int COMMAND_TIMEOUT = 120;

		//Relative URLs for shortcuts
		public const string URL_USER_MANUAL = "/Help/?helpUrl=User-Manual";
		public const string URL_ADMIN_GUIDE = "/Help/?helpUrl=Administration-Guide";

		//Installer GUIDs
		public static Guid UNINSTALL_GUID = new Guid("{A3FAA5AB-5599-4406-B88C-283F198E4968}");
	}
}
