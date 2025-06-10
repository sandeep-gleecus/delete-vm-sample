using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v2_2.DataObjects
{
    /// <summary>
    /// Represents a single data-sync plug-in entry
    /// </summary>
    public class RemoteDataSyncSystem
    {
        public int DataSyncSystemId;
        public int DataSyncStatusId;
        public String Name;
        public String Description;
        public String ConnectionString;
        public String Login;
        public String Password;
        public int TimeOffsetHours;
        public Nullable<DateTime> LastSyncDate;
        public String Custom01;
        public String Custom02;
        public String Custom03;
        public String Custom04;
        public String Custom05;
        public bool AutoMapUsers;
        public String DataSyncStatusName;
    }
}
