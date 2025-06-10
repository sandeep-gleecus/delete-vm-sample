using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the Pull Request Task entity
    /// </summary>
    public partial class PullRequest : Artifact
    {
        public const string ARTIFACT_PREFIX = "TK";

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
