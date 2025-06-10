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
    /// Represents a single Incident artifact in the system
    /// </summary>
    public class RemoteIncident : RemoteArtifact
    {
        /// <summary>
        /// The id of the incident
        /// </summary>
        public Nullable<int> IncidentId;

        /// <summary>
        /// The id of the priority of the incident
        /// </summary>
        public Nullable<int> PriorityId;

        /// <summary>
        /// The id of the severity of the incident
        /// </summary>
        public Nullable<int> SeverityId;

        /// <summary>
        /// The id of the status of the incident
        /// </summary>
        /// <remarks>
        /// If no value is provided, the default status for the workflow is used
        /// </remarks>
        public Nullable<int> IncidentStatusId;

        /// <summary>
        /// The id of the type of the incident
        /// </summary>
        /// <remarks>
        /// If no value is provided, the default type for the project is used
        /// </remarks>
        public Nullable<int> IncidentTypeId;

        /// <summary>
        /// The id of the user who detected the incident
        /// </summary>
        /// <remarks>
        /// If a value is not provided, the authenticated user is used
        /// </remarks>
        public Nullable<int> OpenerId;

        /// <summary>
        /// The id of the user to the incident is assigned-to
        /// </summary>
        public Nullable<int> OwnerId;

        /// <summary>
        /// The id of the test run step that the incident relates to
        /// </summary>
        public Nullable<int> TestRunStepId;

        /// <summary>
        /// The id of the release/iteration that the incident was detected in
        /// </summary>
        public Nullable<int> DetectedReleaseId;

        /// <summary>
        /// The id of the release/iteration that the incident will be fixed in
        /// </summary>
        public Nullable<int> ResolvedReleaseId;
        
        /// <summary>
        /// The id of the release/iteration that the incident was retested in
        /// </summary>
        public Nullable<int> VerifiedReleaseId;

        /// <summary>
        /// The name of the incident
        /// </summary>
        public string Name;

        /// <summary>
        /// The description of the incident
        /// </summary>
        public string Description;

        /// <summary>
        /// The date/time that the incident was originally created
        /// </summary>
        /// <remarks>
        /// If no value is provided, the current date/time on the server is used
        /// </remarks>
        public Nullable<DateTime> CreationDate;

        /// <summary>
        /// The date that work started on the incident
        /// </summary>
        public Nullable<DateTime> StartDate;

        /// <summary>
        /// The date that the incident was closed
        /// </summary>
        public Nullable<DateTime> ClosedDate;

        /// <summary>
        /// The completion percentage (value = 0-100) of the incident as calculated in the system from the remaining effort
        /// vs. the original estimated effort.
        /// </summary>
        /// <remarks>Read-Only</remarks>
        public int CompletionPercent;

        /// <summary>
        /// The estimated effort (in minutes) to resolve the incident
        /// </summary>
        public Nullable<int> EstimatedEffort;

        /// <summary>
        /// The actual effort (in minutes) it took to resolve the incident
        /// </summary>
        public Nullable<int> ActualEffort;

        /// <summary>
        /// The effort remaining as reported by the developer
        /// </summary>
        public Nullable<int> RemainingEffort;

        /// <summary>
        /// The projected actual effort of the incident when it is completed
        /// </summary>
        /// <remarks>Read-Only</remarks>
        public Nullable<int> ProjectedEffort;

        /// <summary>
        /// The date/time that the incident was last modified
        /// </summary>
        /// <remarks>
        /// This field needs to match the values retrieved to ensure data-concurrency
        /// </remarks>
        public DateTime LastUpdateDate;

        /// <summary>
        /// The display name of the priority of the incident
        /// </summary>
        public String PriorityName;

        /// <summary>
        /// The display name of the severity of the incident
        /// </summary>
        public String SeverityName;

        /// <summary>
        /// The display name of the status of the incident
        /// </summary>
        public String IncidentStatusName;

        /// <summary>
        /// The display name of the type of the incident
        /// </summary>
        public String IncidentTypeName;

        /// <summary>
        /// The display name of the user that detected the incident
        /// </summary>
        public String OpenerName;

        /// <summary>
        /// The display name of the user that the incident is assigned to
        /// </summary>
        public String OwnerName;

        /// <summary>
        /// The display name of the project the incident belongs to
        /// </summary>
        public String ProjectName;

        /// <summary>
        /// The version number of the release/iteration that the incident was detected in
        /// </summary>
        public String DetectedReleaseVersionNumber;

        /// <summary>
        /// The version number of the release/iteration that the incident will be resolved in
        /// </summary>
        public String ResolvedReleaseVersionNumber;

        /// <summary>
        /// The version number of the release/iteration that the incident was retested in
        /// </summary>
        public String VerifiedReleaseVersionNumber;

        /// <summary>
        /// Is the incident in an 'open' status or not?
        /// </summary>
        public Nullable<bool> IncidentStatusOpenStatus;

        /// <summary>
        /// The id of the build that the incident was fixed in
        /// </summary>
        public Nullable<int> FixedBuildId;

        /// <summary>
        /// The name of the build that the incident was fixed in
        /// </summary>
        public String FixedBuildName;

    }
}
