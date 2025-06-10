using Inflectra.SpiraTest.DataModel;
using System;
using static Inflectra.SpiraTest.Business.HistoryManager;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business.DataObjects
{
	/// <summary>This class contains information on artifacts that have changed during a time period. (Usually between two History Changesets.) </summary>
	public class ChangedArtifact
	{
		#region Properties
		/// <summary>The type of this artifact.</summary>
		public ArtifactTypeEnum ArtifactTypeId
		{ get; set; }

		/// <summary>The ID of the Artifact.</summary>
		public int ArtifactId { get; set; }

		/// <summary>The overall type of changes that occured. This is hierarchael. meaning that if more than one change occurs, the most important change overrides other.</summary>
		/// <remarks>
		/// See Requirement [RQ:2670] https://spira.inflectra.com/6/Requirement/2670.aspx for details on the ChangeType.
		/// </remarks>
		public ChangeSetTypeEnum ChangeTypeId { get; set; }

		/// <summary>The full data object of the ChangeType.</summary>
		public HistoryChangeSetType ChangeType { get; set; }

		/// <summary>The last date of activity on this artifact.</summary>
		public DateTime LastActionDate { get; set; }

		/// <summary>The last person to act on this artifact.</summary>
		public User LastActionUser { get; set; }

		/// <summary>The definition of the Artifact Type</summary>
		public ArtifactType ArtifactType { get; set; }

		/// <summary>A metadata field for the DataGrid. Divide the number by "1000000000" to get the ArtifactTypeId, then what is rest is the ArtifactID.</summary>
		public int UniqueId { get; set; }

		/// <summary>The name of the artifact, as last seen.</summary>
		public string ArtifactName { get; set; }

		public int LastActionUserId
		{
			get
			{
				return LastActionUser.UserId;
			}
		}
		#endregion Properties

	}
}
