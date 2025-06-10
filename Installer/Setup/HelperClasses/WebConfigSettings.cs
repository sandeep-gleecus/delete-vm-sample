using System;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	class WebConfigSettings
	{
		/// <summary>The connection string.</summary>
		public string ConnectionString { get; set; }

		/// <summary>The connection string.</summary>
		public string AuditConnectionString { get; set; }

		/// <summary>The theme of the application.</summary>
		public string AppTheme { get; set; }

		/// <summary>The Event Log source.</summary>
		public string EventLogSource { get; set; }

		/// <summary>Whether the license is editable or not.</summary>
		public bool LicenseEditable { get; set; }

		/// <summary>The version of the application installed.</summary>
		public Version VersionProgram { get; set; }

		/// <summary>The version of the installer that installed this application.</summary>
		public Version VersionInstaller { get; set; }

		/// <summary>The flavor ("Team", "Test", "Plan", "KronoDesk", etc.) of the application.</summary>
		public string AppFlavor { get; set; }

		/// <summary>The install type. ("CleanInstall", "AppOnly")</summary>
		public string InstallType { get; set; }
	}
}
