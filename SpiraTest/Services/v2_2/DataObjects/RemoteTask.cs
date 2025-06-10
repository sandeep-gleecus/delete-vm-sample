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
    /// Represents a single Task artifact in the system
    /// </summary>
    public class RemoteTask : RemoteArtifact
    {
        public Nullable<int> TaskId;
        public int TaskStatusId;
        public int ProjectId;
        public Nullable<int> RequirementId;
        public Nullable<int> ReleaseId;
        public Nullable<int> OwnerId;
        public Nullable<int> TaskPriorityId;
        public String Name;
        public String Description;
        public DateTime CreationDate;
        public DateTime LastUpdateDate;
        public Nullable<DateTime> StartDate;
        public Nullable<DateTime> EndDate;
        public int CompletionPercent;
        public Nullable<int> EstimatedEffort;
        public Nullable<int> ActualEffort;
        public String TaskStatusName;
        public String OwnerName;
        public String TaskPriorityName;
        public String ProjectName;
        public String ReleaseVersionNumber;
    }
}
