using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the Task entity
    /// </summary>
    public partial class Task : Artifact
    {
        public const string ARTIFACT_PREFIX = "TK";

        #region Enumerations

        /// <summary>
        /// Task statuses
        /// </summary>
        public enum TaskStatusEnum
        {
            NotStarted = 1,
            InProgress = 2,
            Completed = 3,
            Blocked = 4,
            Deferred = 5,
            Rejected = 6,
            Duplicate = 7,
            UnderReview = 8,
            Obsolete = 9
        }

        #endregion

        /// <summary>
        /// Returns the artifact prefix
        /// </summary>
        public override string ArtifactPrefix
        {
            get
            {
                return ARTIFACT_PREFIX;
            }
        }

        /// <summary>
        /// Returns the artifact type enumeration
        /// </summary>
        public override Artifact.ArtifactTypeEnum ArtifactType
        {
            get
            {
                return ArtifactTypeEnum.Task;
            }
        }

		/// <summary>The Artifact's token.</summary>
		public override string ArtifactToken
		{
			get
			{
				return this.ArtifactPrefix + ":" + this.TaskId;
			}
		}

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.TaskId;
            }
        }
    }
}
