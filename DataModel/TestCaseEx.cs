namespace Inflectra.SpiraTest.DataModel
{
	/// <summary>
	/// Adds custom extensions to the TestCase entity
	/// </summary>
	public partial class TestCase : Artifact
	{
		public const string ARTIFACT_PREFIX = "TC";

		#region Enumerations

		/// <summary>
		/// The execution statuses
		/// </summary>
		public enum ExecutionStatusEnum
		{
			Failed = 1,
			Passed = 2,
			NotRun = 3,
			NotApplicable = 4,
			Blocked = 5,
			Caution = 6,
			InProgress = 7
		}

		/// <summary>
		/// Test case statuses (not execution status)
		/// </summary>
		public enum TestCaseStatusEnum
		{
			Draft = 1,
			ReadyForReview = 2,
			Rejected = 3,
			Approved = 4,
			ReadyForTest = 5,
			Obsolete = 6,
			Tested = 7,
			Verified = 8
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
				return ArtifactTypeEnum.TestCase;
			}
		}

		/// <summary>The Artifact's token.</summary>
		public override string ArtifactToken
		{
			get
			{
				return this.ArtifactPrefix + ":" + this.TestCaseId;
			}
		}

		/// <summary>The Artifact's ID</summary>
		public override int ArtifactId
		{
			get
			{
				return this.TestCaseId;
			}
		}

		/// <summary>
		/// Returns the name of the execution status
		/// </summary>
		public string ExecutionStatusName
		{
			get
			{
				if (this.ExecutionStatus != null)
				{
					return this.ExecutionStatus.Name;
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the name of the review status
		/// </summary>
		public string TestCaseStatusName
		{
			get
			{
				if (this.Status != null)
				{
					return this.Status.Name;
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the name of the type
		/// </summary>
		public string TestCaseTypeName
		{
			get
			{
				if (this.Type != null)
				{
					return this.Type.Name;
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the name of the priority
		/// </summary>
		public string TestCasePriorityName
		{
			get
			{
				if (this.Priority != null)
				{
					return this.Priority.Name;
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the name of the priority
		/// </summary>
		public string TestCasePreparationName
		{
			get
			{
				if (this.TestCasePreparationName != null)
				{
					return this.TestCasePreparationName;  //.Name;
				}
				return null;
			}
		}

		/// <summary>
		/// Is the test case in an active status?
		/// </summary>
		public bool IsActive
		{
			get
			{
				return (TestCaseStatusId == (int)TestCase.TestCaseStatusEnum.Draft || TestCaseStatusId == (int)TestCase.TestCaseStatusEnum.Approved || TestCaseStatusId == (int)TestCase.TestCaseStatusEnum.ReadyForReview || TestCaseStatusId == (int)TestCase.TestCaseStatusEnum.ReadyForTest || TestCaseStatusId == (int)TestCase.TestCaseStatusEnum.Tested || TestCaseStatusId == (int)TestCase.TestCaseStatusEnum.Verified);
			}
		}
	}
}
