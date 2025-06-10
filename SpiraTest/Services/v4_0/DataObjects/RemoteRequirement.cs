using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents a single Requirement artifact in the system
    /// </summary>
    public class RemoteRequirement : RemoteArtifact
    {
        /// <summary>
        /// The id of the requirement
        /// </summary>
        public Nullable<int> RequirementId;

        /// <summary>
        /// The indentation level of the artifact
        /// </summary>
        /// <remarks>
        /// The system uses a set of three-letter segments to denote indent (e.g. AAA followed by AAB, etc.)
        /// </remarks>
        public string IndentLevel;

        /// <summary>
        /// The id of the requirement's status
        /// </summary>
        /// <remarks>
        /// If no value is provided, the default status is used
        /// </remarks>
        public Nullable<int> StatusId;

        /// <summary>
        /// The id of the user that wrote the requirement
        /// </summary>
        /// <remarks>
        /// If no value is provided, the authenticated user is used instead
        /// </remarks>
        public Nullable<int> AuthorId;

        /// <summary>
        /// The id of the user that the requirement is assigned-to
        /// </summary>
        public Nullable<int> OwnerId;

        /// <summary>
        /// The id of the importance of the requirement
        /// </summary>
        public Nullable<int> ImportanceId;

        /// <summary>
        /// The id of the release the requirement is scheduled to implemented in
        /// </summary>
        public Nullable<int> ReleaseId;

        /// <summary>
        /// The name of the requirement
        /// </summary>
        public String Name;

        /// <summary>
        /// The description of the requirement
        /// </summary>
        public String Description;

        /// <summary>
        /// The date/time the requirement was originally created
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// The date/time the requirement was last modified
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// Is this a summary requirement or not
        /// </summary>
        public bool Summary;

        /// <summary>
        /// How many test cases cover this requirement
        /// </summary>
        public Nullable<int> CoverageCountTotal;

        /// <summary>
        /// How many of the test cases that cover this requirement have passed
        /// </summary>
        public Nullable<int> CoverageCountPassed;

        /// <summary>
        /// How many of the test cases that cover this requirement have failed
        /// </summary>
        public Nullable<int> CoverageCountFailed;

        /// <summary>
        /// How many of the test cases that cover this requirement have been marked as caution
        /// </summary>
        public Nullable<int> CoverageCountCaution;

        /// <summary>
        /// How many of the test cases that cover this requirement have blocked
        /// </summary>
        public Nullable<int> CoverageCountBlocked;

        /// <summary>
        /// What was the original top-down level of effort estimated for this requirement
        /// </summary>
        public Nullable<int> PlannedEffort;

        /// <summary>
        /// What is the bottom-up estimated effort for all the tasks associated with this requirement
        /// </summary>
        public Nullable<int> TaskEstimatedEffort;

        /// <summary>
        /// What is the bottom-up actual effort for all the tasks associated with this requirement
        /// </summary>
        public Nullable<int> TaskActualEffort;

        /// <summary>
        /// How many tasks are associated with this requirement
        /// </summary>
        public Nullable<int> TaskCount;

        /// <summary>
        /// The version number string of the release that the requirement is scheduled for
        /// </summary>
        public string ReleaseVersionNumber;

        /// <summary>
        /// The display name of the user that wrote this requirement
        /// </summary>
        public string AuthorName;
        
        /// <summary>
        /// The display name of the user that this requirement is assigned-to
        /// </summary>
        public string OwnerName;

        /// <summary>
        /// The display name of the status the requirement is in
        /// </summary>
        public string StatusName;

        /// <summary>
        /// The display name of the importance that the requirement is in
        /// </summary>
        public string ImportanceName;

        /// <summary>
        /// The display name of the project that the requirement is associated with
        /// </summary>
        public string ProjectName;
    }
}
