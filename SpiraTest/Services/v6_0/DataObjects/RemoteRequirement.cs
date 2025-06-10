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
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single Requirement artifact in the system
    /// </summary>
    public class RemoteRequirement : RemoteArtifact
    {
        /// <summary>
        /// The id of the requirement (integer)
        /// </summary>
        public Nullable<int> RequirementId;

        /// <summary>
        /// The indentation level of the artifact (string)
        /// </summary>
        /// <remarks>
        /// The system uses a set of three-letter segments to denote indent (e.g. AAA followed by AAB, etc.)
        /// </remarks>
        public string IndentLevel;

        /// <summary>
        /// The id of the requirement's status (integer).
        /// </summary>
        /// <remarks>
        /// If no value is provided, the default status is used
        /// Relevant values: Accepted 5; Completed 10; Developed 4; Evaluated 7; In Progress 3; Obsolete 8; Planned 2; Rejected 6; Requested 1; Tested 9.
        /// </remarks>
        public Nullable<int> StatusId;

        /// <summary>
        /// The type of requirement (integer).
        /// </summary>
        /// <remarks>
        /// Relevant values: Package -1; Need 1; Feature 2; Use Case 3; User Story 4; Quality 5; Design Element 6
        /// Null can be passed when created if using the default type
        /// </remarks>
        public int? RequirementTypeId;

        /// <summary>
        /// The id of the user that wrote the requirement (integer)
        /// </summary>
        /// <remarks>
        /// If no value is provided, the authenticated user is used instead
        /// </remarks>
        public Nullable<int> AuthorId;

        /// <summary>
        /// The id of the user that the requirement is assigned-to (integer)
        /// </summary>
        public Nullable<int> OwnerId;

        /// <summary>
        /// The id of the importance of the requirement (integer)
        /// </summary>
        /// <remarks>
        /// Relevant values: 1 - Critical 1; 2 - High 2; 3 - Medium 3; 4 - Low 4
        /// </remarks>
        public Nullable<int> ImportanceId;

        /// <summary>
        /// The id of the release the requirement is scheduled to implemented in (integer)
        /// </summary>
        public Nullable<int> ReleaseId;

        /// <summary>
        /// The id of the component the requirement is a part of (integer - these are created on a per project user by an administrator)
        /// </summary>
        public int? ComponentId;

        /// <summary>
        /// The name of the requirement (string - required for POST)
        /// </summary>
        public String Name;

        /// <summary>
        /// The description of the requirement (string)
        /// </summary>
        public String Description;

        /// <summary>
        /// The date/time the requirement was originally created (date-time)
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// The date/time the requirement was last modified (date-time)
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// Is this a summary requirement or not (boolean)
        /// </summary>
        public bool Summary;

        /// <summary>
        /// How many test cases cover this requirement (integer)
        /// </summary>
        [ReadOnly]
        public Nullable<int> CoverageCountTotal;

        /// <summary>
        /// How many of the test cases that cover this requirement have passed (integer)
        /// </summary>
        [ReadOnly]
        public Nullable<int> CoverageCountPassed;

        /// <summary>
        /// How many of the test cases that cover this requirement have failed (integer)
        /// </summary>
        [ReadOnly]
        public Nullable<int> CoverageCountFailed;

        /// <summary>
        /// How many of the test cases that cover this requirement have been marked as caution (integer)
        /// </summary>
        [ReadOnly]
        public Nullable<int> CoverageCountCaution;

        /// <summary>
        /// How many of the test cases that cover this requirement have blocked (integer)
        /// </summary>
        [ReadOnly]
        public Nullable<int> CoverageCountBlocked;

        /// <summary>
        /// The estimate of the requirement (decimal - in story points)
        /// </summary>
        public decimal? EstimatePoints;

        /// <summary>
        /// What was the original top-down level of effort estimated for this requirement, calculated from the points estimate (integer)
        /// </summary>
        [ReadOnly]
        public Nullable<int> EstimatedEffort;

        /// <summary>
        /// What is the bottom-up estimated effort for all the tasks associated with this requirement (integer)
        /// </summary>
        [ReadOnly]
        public Nullable<int> TaskEstimatedEffort;

        /// <summary>
        /// What is the bottom-up actual effort for all the tasks associated with this requirement (integer)
        /// </summary>
        [ReadOnly]
        public Nullable<int> TaskActualEffort;

        /// <summary>
        /// How many tasks are associated with this requirement (integer)
        /// </summary>
        [ReadOnly]
        public Nullable<int> TaskCount;

        /// <summary>
        /// The version number string of the release that the requirement is scheduled for (string)
        /// </summary>
        [ReadOnly]
        public string ReleaseVersionNumber;

        /// <summary>
        /// The display name of the user that wrote this requirement (string)
        /// </summary>
        [ReadOnly]
        public string AuthorName;
        
        /// <summary>
        /// The display name of the user that this requirement is assigned-to (string)
        /// </summary>
        [ReadOnly]
        public string OwnerName;

        /// <summary>
        /// The display name of the status the requirement is in (string)
        /// </summary>
        [ReadOnly]
        public string StatusName;

        /// <summary>
        /// The display name of the importance that the requirement is in (string)
        /// </summary>
        [ReadOnly]
        public string ImportanceName;

        /// <summary>
        /// The display name of the project that the requirement is associated with (string)
        /// </summary>
        [ReadOnly]
        public string ProjectName;

        /// <summary>
        /// The display name of the type of requirement (string)
        /// </summary>
        [ReadOnly]
        public string RequirementTypeName;

        /// <summary>
        /// The list of scenarios steps (array - only available for Use Case requirement types)
        /// </summary>
        public List<RemoteRequirementStep> Steps;

        /// <summary>
        /// The start date of the requirement for planning purposes
        /// </summary>
        public DateTime? StartDate;

        /// <summary>
        /// The end date of the requirement for planning purposes
        /// </summary>
        public DateTime? EndDate;

        /// <summary>
        /// The percentage complete of the requirement
        /// </summary>
        public int? PercentComplete;

        /// <summary>
        /// The Id of the program theme that the requirement belongs to
        /// </summary>
        public int? ThemeId;

        /// <summary>
        /// The id of the goal that the requirement belongs to
        /// </summary>
        public int? GoalId;
    }
}
