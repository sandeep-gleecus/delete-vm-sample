using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the Requirement entity
    /// </summary>
    public partial class Requirement : Artifact
    {
        #region Constants

        public const string ARTIFACT_PREFIX = "RQ";
        public const int REQUIREMENT_TYPE_PACKAGE = -1;   //The other types are dynamic

        #endregion

        #region Enumerations

        /// <summary>
        /// Statuses
        /// </summary>
        public enum RequirementStatusEnum
        {
            Requested = 1,
            Planned = 2,
            InProgress = 3,
            Developed = 4,
            Accepted = 5,
            Rejected = 6,
            UnderReview = 7,
            Obsolete = 8,
            Tested = 9,
            Completed = 10,
            ReadyForReview = 11,
            ReadyForTest = 12,
            Released = 13,
            DesignInProcess = 14,
            DesignApproval = 15,
            Documented = 16,
			Approved = 17,
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
                return ArtifactTypeEnum.Requirement;
            }
        }

		/// <summary>The Artifact's token.</summary>
		public override string ArtifactToken
		{
			get
			{
				return this.ArtifactPrefix + ":" + this.RequirementId;
			}
		}

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.RequirementId;
            }
        }
    }
}
