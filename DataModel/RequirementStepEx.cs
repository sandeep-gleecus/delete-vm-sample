using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the RequirementStep entity
    /// </summary>
    public partial class RequirementStep : Artifact
    {
        public const string ARTIFACT_PREFIX = "RS";

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
                return ArtifactTypeEnum.RequirementStep;
            }
        }

        /// <summary>
        /// Used to store the project id in-memory (used for history tracking)
        /// </summary>
        public int ProjectId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns a pseudo-name for the step (used in History)
        /// </summary>
        public string Name
        {
            get
            {
                return "[" + ARTIFACT_PREFIX + ":" + RequirementStepId + "]";
            }
        }

		/// <summary>The Artifact's token.</summary>
		public override string ArtifactToken
		{
			get
			{
				return this.ArtifactPrefix + ":" + this.RequirementStepId;
			}
		}

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.RequirementStepId;
            }
        }
    }
}
