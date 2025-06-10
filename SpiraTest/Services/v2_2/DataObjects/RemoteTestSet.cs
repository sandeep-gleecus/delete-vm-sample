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
    /// Represents a single Test Set artifact in the system
    /// </summary>
    public class RemoteTestSet : RemoteArtifact
    {
        public Nullable<int> TestSetId;
        public int ProjectId;
        public int TestSetStatusId;
        public int CreatorId;
        public Nullable<int> OwnerId;
        public Nullable<int> ReleaseId;
        public String Name;
        public String Description;
        public DateTime CreationDate;
        public DateTime LastUpdateDate;
        public Nullable<DateTime> PlannedDate;
        public Nullable<DateTime> ExecutionDate;
        public bool Folder;
        public Nullable<int> CountPassed;
        public Nullable<int> CountFailed;
        public Nullable<int> CountCaution;
        public Nullable<int> CountBlocked;
        public Nullable<int> CountNotRun;
        public Nullable<int> CountNotApplicable;
    }
}
