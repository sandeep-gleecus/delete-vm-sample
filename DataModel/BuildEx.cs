using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
	/// <summary>
	/// Adds custom extensions to the Build entity
	/// </summary>
	public partial class Build : Artifact
	{
		public const string ARTIFACT_PREFIX = "BL";

		/// <summary>
		/// The list of possible build statuses
		/// </summary>
		public enum BuildStatusEnum
		{
			Failed = 1,
			Succeeded = 2,
			Unstable = 3,
			Aborted = 4
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
				return this.ArtifactPrefix + ":" + this.BuildId;
			}
		}

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.BuildId;
            }
        }

		#region Lookup Properties

		/// <summary>
		/// Returns the Build Status Name
		/// </summary>
		public string BuildStatusName
		{
			get
			{
				if (Status != null)
				{
					return Status.Name;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion
	}
}
