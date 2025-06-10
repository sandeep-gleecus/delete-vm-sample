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
    /// Represents a single Requirement artifact in the system
    /// </summary>
    public class RemoteRequirement : RemoteArtifact
    {
        public Nullable<int> RequirementId;
        public int ProjectId;
        public int StatusId;
        public int AuthorId;
        public Nullable<int> OwnerId;
        public Nullable<int> ImportanceId;
        public Nullable<int> ReleaseId;
        public String Name;
        public String Description;
        public DateTime CreationDate;
        public DateTime LastUpdateDate;
        public bool Summary;
        public int CoverageCountTotal;
        public int CoverageCountPassed;
        public int CoverageCountFailed;
        public int CoverageCountCaution;
        public int CoverageCountBlocked;
        public Nullable<int> PlannedEffort;
        public Nullable<int> TaskEstimatedEffort;
        public Nullable<int> TaskActualEffort;
        public int TaskCount;
    }
}
