using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the Component entity
    /// </summary>
    public partial class Component : Artifact
    {
        public const string ARTIFACT_PREFIX = "CP";

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
				return this.ArtifactPrefix + ":" + this.ComponentId;
			}
		}

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.ComponentId;
            }
        }
    }
}
