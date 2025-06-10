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
    /// Represents a single Release artifact in the system
    /// </summary>
    public class RemoteRelease : RemoteArtifact
    {
        public Nullable<int> ReleaseId;
        public int CreatorId;
        public int ProjectId;
        public String Name;
        public String Description;
        public String VersionNumber;
        public DateTime CreationDate;
        public DateTime LastUpdateDate;
        public bool Summary;
        public bool Active;
        public bool Iteration;
        public DateTime StartDate;
        public DateTime EndDate;
        public int ResourceCount;
        public int DaysNonWorking;
        public int PlannedEffort;
        public int AvailableEffort;
        public Nullable<int> TaskEstimatedEffort;
        public Nullable<int> TaskActualEffort;
        public int TaskCount;
        public String CreatorName;
    }
}
