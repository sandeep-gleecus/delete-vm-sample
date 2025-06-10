using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a single change made to a field
    /// </summary>
    public class RemoteHistoryChange
    {
        /// <summary>
        /// The id of the change
        /// </summary>
        public int HistoryChangeId;

        /// <summary>
        /// The id of the change set this belongs to
        /// </summary>
        public int ChangeSetId;

        /// <summary>
        /// The id of the custom property if this was a custom property that was changed (NULL = standard field)
        /// </summary>
        public int? CustomPropertyId;

        /// <summary>
        /// The id of the artifact field if this was a standard field that was changed (NULL = custom property)
        /// </summary>
        public int? ArtifactFieldId;

        /// <summary>
        /// The system name of either the standard field or the custom property
        /// </summary>
        public string FieldName;

        /// <summary>
        /// The old value of the field serialized as a string
        /// </summary>
        public string OldValue;

        /// <summary>
        /// The new value of the field serialized as a string
        /// </summary>
        public string NewValue;

        /// <summary>
        /// The display name of either the standard field or the custom property
        /// </summary>
        public string FieldCaption;

        /// <summary>
        /// The changeset information
        /// </summary>
        public RemoteHistoryChangeSet ChangeSet;
    }
}