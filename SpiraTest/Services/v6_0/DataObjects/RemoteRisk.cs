using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
	/// <summary>
	/// Represents a risk in the project
	/// </summary>
	public class RemoteRisk : RemoteArtifact
	{
		/// <summary>
		/// The id of the risk
		/// </summary>
		public int? RiskId;

		/// <summary>
		/// The date the risk was closed (optional) (in UTC)
		/// </summary>
		public DateTime? ClosedDate;

		/// <summary>
		/// The id of the component the risk is associated with (optional)
		/// </summary>
		public int? ComponentId;

		/// <summary>
		/// The name of the component (read-only)
		/// </summary>
		[ReadOnly]
		public string ComponentName;

		/// <summary>
		/// The date the risk was created (in UTC)
		/// </summary>
		public DateTime CreationDate;

		/// <summary>
		/// The id of the user that created the risk
		/// </summary>
		public int CreatorId;

		/// <summary>
		/// The name of the user that created the risk (read-only)
		/// </summary>
		[ReadOnly]
		public string CreatorName;

		/// <summary>
		/// The description of the risk
		/// </summary>
		public string Description;

		/// <summary>
		/// Is the risk deleted
		/// </summary>
		public bool IsDeleted;

		/// <summary>
		/// The date/time the risk was last updated (in UTC)
		/// </summary>
		public DateTime LastUpdateDate;

		/// <summary>
		/// The name of the risk
		/// </summary>
		public string Name;

		/// <summary>
		/// The id of the user that the risk is assigned to currently (optional)
		/// </summary>
		public int? OwnerId;

		/// <summary>
		/// The name of the user that the risk is assigned to currently (read-only)
		/// </summary>
		[ReadOnly]
		public string OwnerName;

		/// <summary>
		/// The id of the release that the risk is currently assigned to (optional)
		/// </summary>
		public int? ReleaseId;

		/// <summary>
		/// The name of the release that the risk is currently assigned to (read-only)
		/// </summary>
		[ReadOnly]
		public string ReleaseName;

		/// <summary>
		/// The version number of the release that the risk is currently assigned to (read-only)
		/// </summary>
		[ReadOnly]
		public string ReleaseVersionNumber;

		/// <summary>
		/// The date/time the risk needs to be reviewed (in UTC)
		/// </summary>
		public DateTime? ReviewDate;

		/// <summary>
		/// The id of the risk impact (optional)
		/// </summary>
		public int? RiskImpactId;

		/// <summary>
		/// The name of the risk impact (read-only)
		/// </summary>
		[ReadOnly]
		public string RiskImpactName;

		/// <summary>
		/// The id of the risk probability (optional)
		/// </summary>
		public int? RiskProbabilityId;

		/// <summary>
		/// The name of the risk probability (read-only)
		/// </summary>
		[ReadOnly]
		public string RiskProbabilityName;

		/// <summary>
		/// The id of the risk status (default if not populated)
		/// </summary>
		public int? RiskStatusId;

		/// <summary>
		/// The name of the risk status (read-only)
		/// </summary>
		[ReadOnly]
		public string RiskStatusName;

		/// <summary>
		/// The id of the risk type (default if not populated)
		/// </summary>
		public int? RiskTypeId;

		/// <summary>
		/// The name of the risk type (read-only)
		/// </summary>
		[ReadOnly]
		public string RiskTypeName;

		/// <summary>
		/// The calculated risk exposure score (read-only)
		/// </summary>
		[ReadOnly]
		public int? RiskExposure;

		#region For Future Use

		/// <summary>
		/// The id of the project group (not used)
		/// </summary>
		public int? ProjectGroupId;

		/// <summary>
		/// The id of the risk detectability (not used)
		/// </summary>
		public int? RiskDetectabilityId;

		/// <summary>
		/// The name of the risk detectability (not used)
		/// </summary>
		[ReadOnly]
		public string RiskDetectabilityName;

		/// <summary>
		/// The id of the project goal (not used)
		/// </summary>
		public int? GoalId;

		#endregion
	}
}
