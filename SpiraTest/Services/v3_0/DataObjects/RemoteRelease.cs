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
    /// Represents a single Release or Iteration artifact in the system
    /// </summary>
    /// <remarks>
    /// Although the fields refer to Release, they are the same fields for an Iteration
    /// </remarks>
    public class RemoteRelease : RemoteArtifact
    {
        /// <summary>
        /// The id of the release
        /// </summary>
        public Nullable<int> ReleaseId;

        /// <summary>
        /// The id of the user that created the release
        /// </summary>
        public Nullable<int> CreatorId;

        /// <summary>
        /// The id of the project the release belongs to
        /// </summary>
        public Nullable<int> ProjectId;
        
        /// <summary>
        /// The indentation level of the artifact
        /// </summary>
        /// <remarks>
        /// The system uses a set of three-letter segments to denote indent (e.g. AAA followed by AAB, etc.)
        /// </remarks>
        public string IndentLevel;

        /// <summary>
        /// The name of the release
        /// </summary>
        public String Name;

        /// <summary>
        /// The description of the release
        /// </summary>
        public String Description;

        /// <summary>
        /// The version number string of the release
        /// </summary>
        public String VersionNumber;

        /// <summary>
        /// The date the release was originally created
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// The date the release was last modified
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// Is this release a summary one (i.e. does it have child releases)
        /// </summary>
        public bool Summary;

        /// <summary>
        /// Is this release active for the project
        /// </summary>
        public bool Active;

        /// <summary>
        /// Is this an iteration (true) or a release (false)
        /// </summary>
        public bool Iteration;

        /// <summary>
        /// What is the start date for the release
        /// </summary>
        public DateTime StartDate;

        /// <summary>
        /// What is the end date for the release
        /// </summary>
        public DateTime EndDate;

        /// <summary>
        /// How many people are working on the release
        /// </summary>
        public int ResourceCount;

        /// <summary>
        /// How many non-working days are associated with the release
        /// </summary>
        public int DaysNonWorking;

        /// <summary>
        /// What is the estimated planned effort associated with the release
        /// </summary>
        public Nullable<int> PlannedEffort;

        /// <summary>
        /// How much effort is still available in the release for planning
        /// </summary>
        public Nullable<int> AvailableEffort;

        /// <summary>
        /// How much effort was estimated for all the tasks scheduled for this release
        /// </summary>
        public Nullable<int> TaskEstimatedEffort;

        /// <summary>
        /// How much effort was actually expended for all the tasks scheduled for this release
        /// </summary>
        public Nullable<int> TaskActualEffort;

        /// <summary>
        /// How many tasks are scheduled for this release
        /// </summary>
        public Nullable<int> TaskCount;

        /// <summary>
        /// What is the full display name of the person who created this release
        /// </summary>
        public String CreatorName;

        /// <summary>
        /// The full name and version number of the release combined
        /// </summary>
        public String FullName;
    }
}
