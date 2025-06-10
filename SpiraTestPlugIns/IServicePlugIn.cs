using System;
using System.Diagnostics;

namespace Inflectra.SpiraTest.PlugIns
{
	/// <summary>
	/// All DataSync service plug-ins need to implement this interface
	/// </summary>
    /// <remarks>
    /// Used for legacy plugins that expect their dates in local time. Use the newer IDataSyncPlugIn interface for new data-syncs
    /// </remarks>
	public interface IServicePlugIn : IDisposable
	{
		void Setup (
            EventLog eventLog,
            bool traceLogging,
            int dataSyncSystemId,
            string webServiceBaseUrl,
            string internalLogin,
            string internalPassword,
            string connectionString,
            string externalLogin,
            string externalPassword,
            int timeOffsetHours,
            bool autoMapUsers,
            string custom01,
            string custom02,
            string custom03,
            string custom04,
            string custom05
            );

        ServiceReturnType Execute(Nullable<DateTime> lastSyncDate, DateTime serverDateTime);
	}
}
