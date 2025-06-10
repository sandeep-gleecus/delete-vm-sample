using Inflectra.SpiraTest.Web.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a single set of changes made to an artifact
    /// </summary>
    public class RemoteHistoryChangeSet
    {
        /// <summary>
        /// The ID of the change set
        /// </summary>
        public int HistoryChangeSetId;

        /// <summary>
        /// The ID of the project
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The ID of the user that made the change
        /// </summary>
        public int UserId;

        /// <summary>
        /// The full name of the user that made the change
        /// </summary>
        [ReadOnly]
        public string UserFullName;

        /// <summary>
        /// The ID of the artifact being changed
        /// </summary>
        public int ArtifactId;

        /// <summary>
        /// The name of the artifact being changed
        /// </summary>
        [ReadOnly]
        public string ArtifactName;

        /// <summary>
        /// The id of the type of artifact
        /// </summary>
        public int ArtifactTypeId;

        /// <summary>
        /// The name of the type of artifact
        /// </summary>
        [ReadOnly]
        public string ArtifactTypeName;

        /// <summary>
        /// The date/time of the change
        /// </summary>
        public DateTime ChangeDate;

        /// <summary>
        /// The id of the type of change
        /// </summary>
        /// <remarks>
       	/// Modified = 1,
	    /// Deleted = 2,
		/// Added = 3,
		/// Purged = 4,
		/// Rollback = 5,
		/// Undelete = 6,
		/// Imported = 7,
		/// Exported = 8,
		/// Deleted_Parent = 9,
		/// Added_Parent = 10,
		/// Purged_Parent = 11,
        /// Undelete_Parent = 12
        /// </remarks>
        public int ChangeTypeId;

        /// <summary>
        /// The name of the type of change
        /// </summary>
        [ReadOnly]
        public string ChangeTypeName;

        /// <summary>
        /// Whether this change was signed by an electronic signature
        /// </summary>
        /// <remarks>
        /// NotSigned = 1,
        /// Valid = 2,
        /// Invalid = 3
        /// </remarks>
        public int SignedId;

        /// <summary>
        /// The collection of field changes in this set
        /// </summary>
        public List<RemoteHistoryChange> Changes;
    }
}