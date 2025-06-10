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
    /// Represents a single Task artifact in the system
    /// </summary>
    public class RemoteTask : RemoteArtifact
    {
        /// <summary>
        /// The id of the task
        /// </summary>
        public Nullable<int> TaskId;

        /// <summary>
        /// The id of the status of the task
        /// </summary>
        public int TaskStatusId;

        /// <summary>
        /// The id of the type of the task (null for default)
        /// </summary>
        public int? TaskTypeId;

        /// <summary>
        /// The of the folder the task is stored in (null for root)
        /// </summary>
        public int? TaskFolderId;

        /// <summary>
        /// The id of the parent requirement that the task belongs to
        /// </summary>
        public Nullable<int> RequirementId;

        /// <summary>
        /// The id of the release/iteration that the task is scheduled for
        /// </summary>
        public Nullable<int> ReleaseId;

        /// <summary>
        /// The id of the component that this task belongs to
        /// </summary>
        /// <remarks>Read-only</remarks>
        [ReadOnly]
        public int? ComponentId;

        /// <summary>
        /// The id of the user that originally created the task
        /// </summary>
        /// <remarks>
        /// If no value is provided, the authenticated user is used instead
        /// </remarks>
        public Nullable<int> CreatorId;

        /// <summary>
        /// The id of the user that the task is assigned-to
        /// </summary>
        public Nullable<int> OwnerId;

        /// <summary>
        /// The id of the priority of the task
        /// </summary>
        public Nullable<int> TaskPriorityId;

        /// <summary>
        /// The name of the task
        /// </summary>
        public String Name;

        /// <summary>
        /// The detailed description of the task
        /// </summary>
        public String Description;

        /// <summary>
        /// The date/time that the task was originally created
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// The date/time that the task was last modified
        /// </summary>
        /// <remarks>
        /// This field needs to match the values retrieved to ensure data-concurrency
        /// </remarks>
        public DateTime LastUpdateDate;

        /// <summary>
        /// The scheduled start date for the task
        /// </summary>
        public Nullable<DateTime> StartDate;

        /// <summary>
        /// The scheduled end date for the task
        /// </summary>
        public Nullable<DateTime> EndDate;

        /// <summary>
        /// The completion percentage (value = 0-100) of the task as calculated in the system from the remaining effort
        /// vs. the original estimated effort.
        /// </summary>
        /// <remarks>Read-Only</remarks>
        [ReadOnly]
        public int CompletionPercent;

        /// <summary>
        /// The originally estimated effort (in minutes) of the task
        /// </summary>
        public Nullable<int> EstimatedEffort;

        /// <summary>
        /// The actual effort expended so far (in minutes) for the task
        /// </summary>
        public Nullable<int> ActualEffort;

        /// <summary>
        /// The effort remaining as reported by the developer
        /// </summary>
        public Nullable<int> RemainingEffort;

        /// <summary>
        /// The projected actual effort of the task when it is completed
        /// </summary>
        /// <remarks>Read-Only</remarks>
        [ReadOnly]
        public Nullable<int> ProjectedEffort;

        /// <summary>
        /// The display name of the status of the task
        /// </summary>
        [ReadOnly]
        public String TaskStatusName;

        /// <summary>
        /// The display name of the type of the task
        /// </summary>
        [ReadOnly]
        public String TaskTypeName;

        /// <summary>
        /// The display name of the user who the task is assigned-to
        /// </summary>
        /// <remarks>Read-Only</remarks>
        [ReadOnly]
        public String OwnerName;

        /// <summary>
        /// The display name of the priority of the task
        /// </summary>
        /// <remarks>Read-Only</remarks>
        [ReadOnly]
        public String TaskPriorityName;

        /// <summary>
        /// The display name of the project the task belongs to
        /// </summary>
        /// <remarks>Read-Only</remarks>
        [ReadOnly]
        public String ProjectName;

        /// <summary>
        /// The version number of the release/iteration the task is scheduled for
        /// </summary>
        /// <remarks>Read-Only</remarks>
        [ReadOnly]
        public String ReleaseVersionNumber;

        /// <summary>
        /// The name of the requirement that the task is associated with
        /// </summary>
        /// <remarks>Read-Only</remarks>
        [ReadOnly]
        public String RequirementName;

        /// <summary>
        /// The risk that the task is associated with
        /// </summary>
        public int? RiskId;
    }
}
