using System;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	class SettingsDataSync
	{
		/// <summary>How often the DataSync should poll.</summary>
		public int PollingInterval { get; set; }

		/// <summaryThe URL to the Spira service.</summary>
		public Uri WebServiceUrl { get; set; }

		/// <summary>The user to log onto Spira with.</summary>
		public string SpiraLogin { get; set; }

		/// <summary>The password for the Spira user.</summary>
		public string SpiraPassword { get; set; }

		/// <summary>The event log to record under.</summary>
		public string EventLogSource { get; set; }

		/// <summary>Whether or not to enable Trace Logging.</summary>
		public bool TraceLogging { get; set; }
	}
}
