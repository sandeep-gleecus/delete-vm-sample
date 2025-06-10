using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
	/// <summary>
	/// Adds custom extensions to the ProjectForUserView entity
	/// </summary>
	public partial class ProjectForUserView : Artifact
	{
		public const string ARTIFACT_PREFIX = "PR";

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
				return ArtifactTypeEnum.Project;
			}
		}

		/// <summary>The Artifact's token.</summary>
		public override string ArtifactToken
		{
            get
            {
                return this.ArtifactPrefix + ":" + this.ProjectId;
            }
        }

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.ProjectId;
            }
        }

        /// <summary>
        /// Used to store when the project was last accessed
        /// </summary>
        public DateTime? LastAccessedDate
        {
            get;
            set;
        }

    }
}
