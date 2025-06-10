using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the automation host artifact
    /// </summary>
    public partial class AutomationHostView : Artifact
    {
        /// <summary>
        /// Returns the artifact prefix
        /// </summary>
        public override string ArtifactPrefix
        {
            get
            {
                return AutomationHost.ARTIFACT_PREFIX;
            }
        }

        /// <summary>
        /// Returns the artifact type enumeration
        /// </summary>
        public override Artifact.ArtifactTypeEnum ArtifactType
        {
            get
            {
                return ArtifactTypeEnum.AutomationHost;
            }
        }

        /// <summary>The Artifact's token.</summary>
        public override string ArtifactToken
        {
            get
            {
                return this.ArtifactPrefix + ":" + this.AutomationHostId;
            }
        }

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.AutomationHostId;
            }
        }
    }
}
