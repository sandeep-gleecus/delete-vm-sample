using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v3_0.DataObjects
{
    /// <summary>
    /// Represents a single data-sync plug-in entry
    /// </summary>
    public class RemoteDataSyncSystem
    {
        /// <summary>
        /// The id the data-sync plug-in
        /// </summary>
        public int DataSyncSystemId;

        /// <summary>
        /// The id of the synchronization status (for the last time it ran)
        /// </summary>
        public int DataSyncStatusId;

        /// <summary>
        /// The name of the data-sync plug-in
        /// </summary>
        public String Name;

        /// <summary>
        /// The description of the data-sync plug-in
        /// </summary>
        public String Description;

        /// <summary>
        /// The connection string (often a URL) for accessing the external system
        /// </summary>
        public String ConnectionString;

        /// <summary>
        /// The username / login for accessing the external system
        /// </summary>
        public String Login;

        /// <summary>
        /// The password for accessing the external system
        /// </summary>
        public String Password;

        /// <summary>
        /// The number of hours to add to the last-updated date/times when making comparisons
        /// </summary>
        public int TimeOffsetHours;

        /// <summary>
        /// The date/time that the data-sync last ran
        /// </summary>
        public Nullable<DateTime> LastSyncDate;

        /// <summary>
        /// Custom value, its use is dependent on the specific data-sync plug-in
        /// </summary>
        public String Custom01;

        /// <summary>
        /// Custom value, its use is dependent on the specific data-sync plug-in
        /// </summary>
        public String Custom02;

        /// <summary>
        /// Custom value, its use is dependent on the specific data-sync plug-in
        /// </summary>
        public String Custom03;

        /// <summary>
        /// Custom value, its use is dependent on the specific data-sync plug-in
        /// </summary>
        public String Custom04;

        /// <summary>
        /// Custom value, its use is dependent on the specific data-sync plug-in
        /// </summary>
        public String Custom05;

        /// <summary>
        /// Should we attempt to auto-map user in the two systems
        /// </summary>
        public bool AutoMapUsers;

        /// <summary>
        /// The display name of the synchronization status
        /// </summary>
        public String DataSyncStatusName;
    }
}
