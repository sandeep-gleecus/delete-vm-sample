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
    /// Represents a single Incident artifact in the system
    /// </summary>
    public class RemoteIncident : RemoteArtifact
    {
        public Nullable<int> IncidentId;
        public int ProjectId;
        public Nullable<int> PriorityId;
        public Nullable<int> SeverityId;
        public int IncidentStatusId;
        public int IncidentTypeId;
        public int OpenerId;
        public Nullable<int> OwnerId;
        public Nullable<int> TestRunStepId;
        public Nullable<int> DetectedReleaseId;
        public Nullable<int> ResolvedReleaseId;
        public Nullable<int> VerifiedReleaseId;
        public string Name;
        public string Description;
        public DateTime CreationDate;
        public Nullable<DateTime> StartDate;
        public Nullable<DateTime> ClosedDate;
        public int CompletionPercent;
        public Nullable<int> EstimatedEffort;
        public Nullable<int> ActualEffort;
        public DateTime LastUpdateDate;
        public String PriorityName;
        public String SeverityName;
        public String IncidentStatusName;
        public String IncidentTypeName;
        public String OpenerName;
        public String OwnerName;
        public String ProjectName;
        public String DetectedReleaseVersionNumber;
        public String ResolvedReleaseVersionNumber;
        public String VerifiedReleaseVersionNumber;
        public Nullable<bool> IncidentStatusOpenStatus;
    }
}
