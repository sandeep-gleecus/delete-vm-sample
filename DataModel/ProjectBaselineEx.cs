namespace Inflectra.SpiraTest.DataModel
{
	/// <summary>
	/// Adds custom extensions to the Build entity
	/// </summary>
	public partial class ProjectBaseline : Artifact
	{
		public const string ARTIFACT_PREFIX = "BH";

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
				return ArtifactPrefix + ":" + BaselineId;
			}
		}

		/// <summary>The Artifact's ID</summary>
		public override int ArtifactId
		{
			get
			{
				return BaselineId;
			}
		}

		#region Lookup Properties

		/// <summary>
		/// Returns the Creator User's name
		/// </summary>
		public string CreatorUserName
		{
			get
			{
				if (Creator != null)
				{
					return Creator.FullName;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>Returns the full display string of the release.</summary>
		public string ReleaseFullName
		{
			get
			{
				if (Release != null)
					return Release.FullName;
				else
					return "";
			}
		}

		#endregion
	}
}
