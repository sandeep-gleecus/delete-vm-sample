namespace Inflectra.SpiraTest.DataModel
{
	public partial class HistoryChangeSetNetChangeSquashed : Artifact
	{
		public override ArtifactTypeEnum ArtifactType
		{
			get
			{
				return (ArtifactTypeEnum)ArtifactTypeId;
			}
		}

		public override string ArtifactPrefix
		{
			get
			{
				string retStr = "";

				switch ((ArtifactTypeEnum)ArtifactTypeId)
				{
					case ArtifactTypeEnum.Requirement:
						retStr = Requirement.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.TestCase:
						retStr = TestCase.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.Incident:
						retStr = Incident.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.Release:
						retStr = Release.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.TestRun:
						retStr = TestRun.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.Task:
						retStr = Task.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.TestStep:
						retStr = TestStep.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.TestSet:
						retStr = TestSet.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.AutomationHost:
						retStr = AutomationHost.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.RequirementStep:
						retStr = RequirementStep.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.Risk:
						retStr = Risk.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.RiskMitigation:
						retStr = RiskMitigation.ARTIFACT_PREFIX;
						break;
					case ArtifactTypeEnum.Document:
						retStr = Attachment.ARTIFACT_PREFIX;
						break;
				}

				return retStr;

			}
		}

		public override string ArtifactToken
		{
			get
			{
				return "[" + ArtifactPrefix + ":" + ArtifactId + "]";
			}
		}

		public override int ArtifactId
		{
			get
			{
				return ChangedArtifactId;
			}
		}
	}
}
