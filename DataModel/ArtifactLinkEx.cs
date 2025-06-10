using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>Add custom extensions to the ArtifactLink entity</summary>
    public partial class ArtifactLink : Artifact
    {
		public const string ARTIFACT_PREFIX = "AL";

        /// <summary>Enumeration of the various artifact link types</summary>
        public enum ArtifactLinkTypeEnum
        {
            RelatedTo = 1,
            DependentOn = 2,
            Implicit = 3,
            SourceCodeCommit = 4,
            Gantt_FinishToStart = 5,
            Gantt_StartToStart = 6,
            Gantt_FinishToFinish = 7,
            Gantt_StartToFinish = 8

        }

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
                return ArtifactTypeEnum.None;
            }
        }

        /// <summary>The Artifact's token.</summary>
        public override string ArtifactToken
        {
            get
            {
                return this.ArtifactPrefix + ":" + this.ArtifactLinkId;
            }
        }

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.ArtifactLinkId;
            }
        }
    }
}
