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
    /// Represents a single Test Case artifact in the system
    /// </summary>
    public class RemoteTestCase : RemoteArtifact
    {
        public Nullable<int> TestCaseId;
        public int ProjectId;
        public int ExecutionStatusId;
        public int AuthorId;
        public Nullable<int> OwnerId;
        public Nullable<int> TestCasePriorityId;
        public String Name;
        public String Description;
        public DateTime CreationDate;
        public DateTime LastUpdateDate;
        public Nullable<DateTime> ExecutionDate;
        public Nullable<int> EstimatedDuration;
        public bool Folder;
        public bool Active;
    }
}
