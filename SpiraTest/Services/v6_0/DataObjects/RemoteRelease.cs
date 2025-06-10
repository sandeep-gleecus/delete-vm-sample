using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
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
        /// The id of the user that the release is assigned to
        /// </summary>
        public int? OwnerId;

        /// <summary>
        /// The name of the user that the release is assigned to
        /// </summary>
        [ReadOnly]
        public string OwnerName;

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
        [ReadOnly]
        public bool Active;

        /// <summary>
        /// The status of the release
        /// </summary>
        /// <remarks>
        /// Planned = 1,
        /// InProgress = 2,
        /// Completed = 3,
        /// Closed = 4,
        /// Deferred = 5,
        /// Cancelled = 6
        /// </remarks>
        public int ReleaseStatusId;

        /// <summary>
        /// The type of the release
        /// </summary>
        /// <remarks>
        /// MajorRelease = 1,
        /// MinorRelease = 2,
        /// Iteration = 3,
        /// Phase = 4
        /// </remarks>
        public int ReleaseTypeId;

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
        public decimal ResourceCount;

        /// <summary>
        /// How many non-working days are associated with the release
        /// </summary>
        public decimal DaysNonWorking;

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
        [ReadOnly]
        public String CreatorName;

        /// <summary>
        /// The full name and version number of the release combined
        /// </summary>
        [ReadOnly]
        public String FullName;

        /// <summary>
        /// The display name for the release status
        /// </summary>
        [ReadOnly]
        public string ReleaseStatusName;

        /// <summary>
        /// The display name for the release type
        /// </summary>
        [ReadOnly]
        public string ReleaseTypeName;

        /// <summary>
        /// The count of blocked test cases in this release
        /// </summary>
        [ReadOnly]
        public int CountBlocked;

        /// <summary>
        /// The count of caution test cases in this release
        /// </summary>
        [ReadOnly]
        public int CountCaution;

        /// <summary>
        /// The count of failed test cases in this release
        /// </summary>
        [ReadOnly]
        public int CountFailed;

        /// <summary>
        /// The count of N/A test cases in this release
        /// </summary>
        [ReadOnly]
        public int CountNotApplicable;

        /// <summary>
        /// The count of not run test cases in this release
        /// </summary>
        [ReadOnly]
        public int CountNotRun;

        /// <summary>
        /// The count of passed test cases in this release
        /// </summary>
        [ReadOnly]
        public int CountPassed;

        /// <summary>
        /// The percentage complete of the project/sprint
        /// </summary>
        public int PercentComplete;

        /// <summary>
        /// The id of any program milestones that this release is associated with
        /// </summary>
        public int? MilestoneId;
    }
}
