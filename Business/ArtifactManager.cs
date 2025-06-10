using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// reading and writing pan-Artifact generic data in the system
	/// </summary>
	public class ArtifactManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.ArtifactManager::";

		//Static cached copy of artifact meta-data that doesn't change dynamically ever
		private static Dictionary<int, List<ArtifactField>> _workflowConfigurableArtifactFields = new Dictionary<int, List<ArtifactField>>();
		private static Dictionary<int, List<ArtifactField>> _reportableArtifactFields = new Dictionary<int, List<ArtifactField>>();
		public static List<ArtifactField> _staticArtifactFields;
		public static List<ArtifactType> _staticArtifactTypes;

		#region Static Properties

		/// <summary>
		/// The static list of artifact fields with associated type information
		/// </summary>
		public static List<ArtifactField> ArtifactFields
		{
			get
			{
				if (_staticArtifactFields == null)
				{
					_staticArtifactFields = RetrieveArtifactFields();
				}
				return _staticArtifactFields;
			}
		}

		/// <summary>
		/// The static list of artifact fields with associated type information
		/// </summary>
		public static List<ArtifactType> ArtifactTypes
		{
			get
			{
				if (_staticArtifactTypes == null)
				{
					_staticArtifactTypes = RetrieveArtifactTypes();
				}
				return _staticArtifactTypes;
			}
		}

		#endregion

		#region Private Functions

		/// <summary>
		/// Loads in the static list of artifact fields
		/// </summary>
		private static List<ArtifactField> RetrieveArtifactFields()
		{
			const string METHOD_NAME = "RetrieveArtifactFields";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			List<ArtifactField> artifactFields;

			try
			{
				//Build the base query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from af in context.ArtifactFields.Include("Type")
								orderby af.ArtifactFieldId
								select af;

					artifactFields = query.ToList();
				}

				return artifactFields;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Loads in the static list of artifact types
		/// </summary>
		private static List<ArtifactType> RetrieveArtifactTypes()
		{
			const string METHOD_NAME = "RetrieveArtifactTypes";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			List<ArtifactType> artifactTypes;

			try
			{
				//Build the base query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from at in context.ArtifactTypes
								orderby at.ArtifactTypeId
								select at;

					artifactTypes = query.ToList();
				}

				return artifactTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		/// <summary>
		/// Represents a project-artifact type composite filter
		/// </summary>
		public class ProjectArtifactTypeFilter
		{
			/// <summary>
			/// The id of the project
			/// </summary>
			public int ProjectId { get; set; }

			/// <summary>
			/// The id of the artifact type
			/// </summary>
			public int ArtifactTypeId { get; set; }
		}

		/// <summary>
		///	Constructor method for class.
		/// </summary>
		public ArtifactManager()
		{
		}

		#region Other Functions

		/// <summary>
		/// Gets the string name of a lookup field for a particular artifact
		/// </summary>
		/// <param name="artifactType">The artifact type</param>
		/// <param name="fieldName">The name of the field</param>
		/// <param name="projectId">the id of the project</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="lookupId">The id of the value we're retrieving</param>
		/// <returns>The lookup string</returns>
		public string GetLookupValue(Artifact.ArtifactTypeEnum artifactType, int projectId, int projectTemplateId, string fieldName, int lookupId)
		{
			if (fieldName == "OpenerId")
			{
				User user = new UserManager().GetUserById(lookupId, false);
				if (user != null)
				{
					return user.FullName;
				}
				else
				{
					return "Default User";
				}
			}
			if (fieldName == "ArtifactLinkTypeId")
			{
				ArtifactLinkType artifactLink = new ArtifactLinkManager().RetrieveByLinkId(lookupId);
				if (artifactLink != null)
				{
					return artifactLink.Name;
				}
				else
				{
					return "Default ArtifactLinkType";
				}
			}
			if (fieldName == "ProjectTemplateId")
			{
				ProjectTemplate template = new TemplateManager().RetrieveById(lookupId);
				if (template != null)
				{
					return template.Name;
				}
				else
				{
					return "Default Program";
				}
			}

			if (fieldName == "PortfolioId")
			{
				Portfolio portfolio = new PortfolioManager().RetrieveById(lookupId);
				if (portfolio != null)
				{
					return portfolio.Name;
				}
				else
				{
					return "Default Program";
				}
			}

			if (fieldName == "ProjectGroupId")
			{
				ProjectGroup group = new ProjectGroupManager().RetrieveById(lookupId);
				if (group != null)
				{
					return group.Name;
				}
				else
				{
					return "Default Program";
				}
			}
			#region Requirement fields

			if (fieldName == "RequirementStatusId")
			{
				RequirementStatus status = new RequirementManager().RetrieveStatuses().FirstOrDefault(r => r.RequirementStatusId == lookupId);
				if (status != null)
				{
					return status.Name;
				}
			}
			if (fieldName == "RequirementTypeId")
			{
				RequirementType type = new RequirementManager().RequirementType_Retrieve(projectTemplateId, true).FirstOrDefault(r => r.RequirementTypeId == lookupId);
				if (type != null)
				{
					return type.Name;
				}
			}

			if (fieldName == "ImportanceId")
			{
				Importance importance = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(r => r.ImportanceId == lookupId);
				if (importance != null)
				{
					return importance.Name;
				}
			}

			#endregion

			#region Global fields

			if (fieldName == "ReleaseId" || fieldName == "DetectedReleaseId" || fieldName == "ResolvedReleaseId" || fieldName == "VerifiedReleaseId")
			{
				try
				{
					ReleaseView release = new ReleaseManager().RetrieveById2(projectId, lookupId, true);
					return release.FullName;
				}
				catch (ArtifactNotExistsException)
				{
					//Just fail quietly
				}
			}

			if (fieldName == "ComponentId")
			{
				DataModel.Component component = new ComponentManager().Component_RetrieveById(lookupId, true);
				if (component != null)
				{
					return component.Name;
				}
			}
			if (fieldName == "AuthorId" || fieldName == "OwnerId" || fieldName == "CreatorId" || fieldName == "EditorId")
			{
				try
				{
					DataModel.User user = new UserManager().GetUserById(lookupId, false);
					return user.FullName;
				}
				catch (ArtifactNotExistsException)
				{
					//Just fail quietly
				}
			}

			#endregion

			#region Task fields

			if (fieldName == "TaskStatusId")
			{
				TaskStatus status = new TaskManager().RetrieveStatuses().FirstOrDefault(r => r.TaskStatusId == lookupId);
				if (status != null)
				{
					return status.Name;
				}
			}
			if (fieldName == "TaskTypeId")
			{
				TaskType type = new TaskManager().TaskType_Retrieve(projectTemplateId).FirstOrDefault(r => r.TaskTypeId == lookupId);
				if (type != null)
				{
					return type.Name;
				}
			}
			if (fieldName == "TaskPriorityId")
			{
				TaskPriority type = new TaskManager().TaskPriority_Retrieve(projectTemplateId).FirstOrDefault(r => r.TaskPriorityId == lookupId);
				if (type != null)
				{
					return type.Name;
				}
			}

			#endregion

			#region Release fields

			if (fieldName == "ReleaseStatusId")
			{
				ReleaseStatus status = new ReleaseManager().RetrieveStatuses().FirstOrDefault(r => r.ReleaseStatusId == lookupId);
				if (status != null)
				{
					return status.Name;
				}
			}
			if (fieldName == "ReleaseTypeId")
			{
				ReleaseType type = new ReleaseManager().RetrieveTypes().FirstOrDefault(r => r.ReleaseTypeId == lookupId);
				if (type != null)
				{
					return type.Name;
				}
			}

			if (fieldName == "RequirementId")
			{
				try
				{
					RequirementView requirementView = new RequirementManager().RetrieveById2(projectId, lookupId, true);
					return requirementView.Name;
				}
				catch (ArtifactNotExistsException)
				{
					//Just fail quietly
				}
			}

			#endregion

			if (fieldName == "PeriodicReviewAlertId")
			{
				PeriodicReviewAlertType status = new TaskManager().PeriodicReviewAlertType_Retrieve(lookupId, true);
				if (status != null)
				{
					return status.Name;
				}
			}

			#region Incident fields

			if (fieldName == "IncidentStatusId")
			{
				IncidentStatus status = new IncidentManager().IncidentStatus_RetrieveById(lookupId);
				if (status != null)
				{
					return status.Name;
				}
			}
			if (fieldName == "IncidentTypeId")
			{
				IncidentType type = new IncidentManager().RetrieveIncidentTypeById(lookupId);
				if (type != null)
				{
					return type.Name;
				}
			}
			if (fieldName == "PriorityId")
			{
				IncidentPriority priority = new IncidentManager().RetrieveIncidentPriorityById(lookupId);
				if (priority != null)
				{
					return priority.Name;
				}
			}
			if (fieldName == "SeverityId")
			{
				IncidentSeverity severity = new IncidentManager().RetrieveIncidentSeverityById(lookupId);
				if (severity != null)
				{
					return severity.Name;
				}
			}

			#endregion

			#region Document fields

			if (fieldName == "DocumentTypeId")
			{
				DocumentType documentType = new AttachmentManager().RetrieveDocumentTypeById(lookupId);
				if (documentType != null)
				{
					return documentType.Name;
				}
			}

			if (fieldName == "DocumentStatusId")
			{
				DocumentStatus documentStatus = new AttachmentManager().DocumentStatus_RetrieveById(lookupId);
				if (documentStatus != null)
				{
					return documentStatus.Name;
				}
			}

			#endregion

			#region Test Case fields

			if (fieldName == "TestCaseStatusId")
			{
				TestCaseStatus status = new TestCaseManager().RetrieveStatusById(lookupId);
				if (status != null)
				{
					return status.Name;
				}
			}
			if (fieldName == "TestCaseTypeId")
			{
				TestCaseType type = new TestCaseManager().TestCaseType_RetrieveById(lookupId);
				if (type != null)
				{
					return type.Name;
				}
			}
			if (fieldName == "TestCasePriorityId")
			{
				TestCasePriority priority = new TestCaseManager().TestCasePriority_RetrieveById(lookupId);
				if (priority != null)
				{
					return priority.Name;
				}
			}

			if (fieldName == "TestCasePreparationStatusId")
			{
				TestCasePreparationStatus testCasePreparation = new TestCaseManager().TestCasePreparation_RetrieveById(lookupId);
				if (testCasePreparation != null)
				{
					return testCasePreparation.Name;
				}
			}

			if (fieldName == "ExecutionStatusId")
			{
				ExecutionStatus executionStatus = new TestCaseManager().RetrieveExecutionStatusById(lookupId);
				if (executionStatus != null)
				{
					return executionStatus.Name;
				}
			}

			if (fieldName == "AutomationEngineId")
			{
				try
				{
					AutomationEngine engine = new AutomationManager().RetrieveEngineById(lookupId);
					if (engine != null)
					{
						return engine.Name;
					}
				}
				catch (ArtifactNotExistsException)
				{
					//Ignore
				}
			}

			#endregion

			#region Test Set fields

			if (fieldName == "TestSetStatusId")
			{
				TestSetStatus status = new TestSetManager().RetrieveStatusById(lookupId);
				if (status != null)
				{
					return status.Name;
				}
			}

			if (fieldName == "RecurrenceId")
			{
				Recurrence recurrence = new TestSetManager().RetrieveRecurrenceById(lookupId);
				if (recurrence != null)
				{
					return recurrence.Name;
				}
			}

			if (fieldName == "TestConfigurationSetId")
			{
				TestConfigurationSet testConfigurationSet = new TestConfigurationManager().RetrieveSetById(lookupId);
				if (testConfigurationSet != null)
				{
					return testConfigurationSet.Name;
				}
			}

			if (fieldName == "AutomationHostId")
			{
				try
				{
					AutomationHostView host = new AutomationManager().RetrieveHostById(lookupId);
					if (host != null)
					{
						return host.Name;
					}
				}
				catch (ArtifactNotExistsException)
				{
					//Ignore
				}
			}

			#endregion

			#region Test Run fields

			if (fieldName == "TestRunTypeId")
			{
				TestRunType type = new TestRunManager().RetrieveTypeById(lookupId);
				if (type != null)
				{
					return type.Name;
				}
			}

			if (fieldName == "TestSetId")
			{
				try
				{
					TestSetView testSet = new TestSetManager().RetrieveById(null, lookupId);
					if (testSet != null)
					{
						return testSet.Name;
					}
				}
				catch (ArtifactNotExistsException)
				{
					//Ignore
				}
			}

			#endregion

			#region Risk fields

			if (fieldName == "RiskStatusId")
			{
				RiskStatus status = new RiskManager().RiskStatus_RetrieveById(lookupId);
				if (status != null)
				{
					return status.Name;
				}
			}
			if (fieldName == "RiskTypeId")
			{
				RiskType type = new RiskManager().RiskType_RetrieveById(lookupId);
				if (type != null)
				{
					return type.Name;
				}
			}
			if (fieldName == "RiskProbabilityId")
			{
				RiskProbability probability = new RiskManager().RiskProbability_RetrieveById(lookupId);
				if (probability != null)
				{
					return probability.Name;
				}
			}
			if (fieldName == "RiskImpactId")
			{
				RiskImpact impact = new RiskManager().RiskImpact_RetrieveById(lookupId);
				if (impact != null)
				{
					return impact.Name;
				}
			}

			#endregion

			//No match
			return "";
		}

		/// <summary>
		/// Gets the ID value of a lookup field for a particular artifact
		/// </summary>
		/// <param name="artifactType">The artifact type</param>
		/// <param name="fieldName">The name of the field</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="lookupValue">The display name of the list value</param>
		/// <returns>The lookup ID value or nul</returns>
		/// <remarks>
		/// This function is used by the template manager to change a template, so only fields connected to a template are returned
		/// </remarks>
		public int? GetLookupId(Artifact.ArtifactTypeEnum artifactType, int projectTemplateId, string fieldName, string lookupValue)
		{
			#region Requirement fields

			if (fieldName == "RequirementTypeId")
			{
				RequirementType type = new RequirementManager().RequirementType_Retrieve(projectTemplateId, true).FirstOrDefault(r => r.Name == lookupValue);
				if (type != null)
				{
					return (int)type[fieldName];
				}
			}

			if (fieldName == "ImportanceId")
			{
				Importance importance = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(r => r.Name == lookupValue);
				if (importance != null)
				{
					return (int)importance[fieldName];
				}
			}

			#endregion

			#region Task fields

			if (fieldName == "TaskTypeId")
			{
				TaskType type = new TaskManager().TaskType_Retrieve(projectTemplateId).FirstOrDefault(r => r.Name == lookupValue);
				if (type != null)
				{
					return (int)type[fieldName];
				}
			}
			if (fieldName == "TaskPriorityId")
			{
				TaskPriority type = new TaskManager().TaskPriority_Retrieve(projectTemplateId).FirstOrDefault(r => r.Name == lookupValue);
				if (type != null)
				{
					return (int)type[fieldName];
				}
			}

			#endregion

			#region Incident fields

			if (fieldName == "IncidentStatusId")
			{
				IncidentStatus status = new IncidentManager().IncidentStatus_Retrieve(projectTemplateId, false).FirstOrDefault(p => p.Name == lookupValue);
				if (status != null)
				{
					return (int)status[fieldName];
				}
			}
			if (fieldName == "IncidentTypeId")
			{
				IncidentType type = new IncidentManager().RetrieveIncidentTypes(projectTemplateId, false).FirstOrDefault(p => p.Name == lookupValue);
				if (type != null)
				{
					return (int)type[fieldName];
				}
			}
			if (fieldName == "PriorityId")
			{
				IncidentPriority priority = new IncidentManager().RetrieveIncidentPriorities(projectTemplateId, false).FirstOrDefault(p => p.Name == lookupValue);
				if (priority != null)
				{
					return (int)priority[fieldName];
				}
			}
			if (fieldName == "SeverityId")
			{
				IncidentSeverity severity = new IncidentManager().RetrieveIncidentSeverities(projectTemplateId, false).FirstOrDefault(p => p.Name == lookupValue);
				if (severity != null)
				{
					return (int)severity[fieldName];
				}
			}

			#endregion

			#region Document fields

			if (fieldName == "DocumentTypeId")
			{
				DocumentType documentType = new AttachmentManager().RetrieveDocumentTypes(projectTemplateId, false).FirstOrDefault(d => d.Name == lookupValue);
				if (documentType != null)
				{
					return (int)documentType[fieldName];
				}
			}

			if (fieldName == "DocumentStatusId")
			{
				DocumentStatus documentStatus = new AttachmentManager().DocumentStatus_Retrieve(projectTemplateId, false).FirstOrDefault(d => d.Name == lookupValue);
				if (documentStatus != null)
				{
					return (int)documentStatus[fieldName];
				}
			}

			#endregion

			#region Test Case fields

			if (fieldName == "TestCaseTypeId")
			{
				TestCaseType type = new TestCaseManager().TestCaseType_Retrieve(projectTemplateId, false).FirstOrDefault(p => p.Name == lookupValue);
				if (type != null)
				{
					return (int)type[fieldName];
				}
			}
			if (fieldName == "TestCasePriorityId")
			{
				TestCasePriority priority = new TestCaseManager().TestCasePriority_Retrieve(projectTemplateId, false).FirstOrDefault(p => p.Name == lookupValue);
				if (priority != null)
				{
					return (int)priority[fieldName];
				}
			}

			#endregion

			#region Risk fields

			if (fieldName == "RiskStatusId")
			{
				RiskStatus status = new RiskManager().RiskStatus_Retrieve(projectTemplateId, false).FirstOrDefault(p => p.Name == lookupValue);
				if (status != null)
				{
					return (int)status[fieldName];
				}
			}
			if (fieldName == "RiskTypeId")
			{
				RiskType type = new RiskManager().RiskType_Retrieve(projectTemplateId, false).FirstOrDefault(p => p.Name == lookupValue);
				if (type != null)
				{
					return (int)type[fieldName];
				}
			}
			if (fieldName == "RiskProbabilityId")
			{
				RiskProbability probability = new RiskManager().RiskProbability_Retrieve(projectTemplateId, false).FirstOrDefault(p => p.Name == lookupValue);
				if (probability != null)
				{
					return (int)probability[fieldName];
				}
			}
			if (fieldName == "RiskImpactId")
			{
				RiskImpact impact = new RiskManager().RiskImpact_Retrieve(projectTemplateId, false).FirstOrDefault(p => p.Name == lookupValue);
				if (impact != null)
				{
					return (int)impact[fieldName];
				}
			}

			#endregion

			//No match
			return null;
		}

		/// <summary>
		/// Determines if an artifact is supported by the currently loaded license
		/// </summary>
		/// <returns>True if supported</returns>
		public static bool IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			bool supported = true;
			//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.

			return supported;
		}

		/// <summary>
		/// Verify an artifact exists in any project and if so, return the project id
		/// </summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <returns>null if it doesn't exist, otherwise the project</returns>
		public int? GetProjectForArtifact(DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId)
		{
			int? projectId = null;

			try
			{
				switch (artifactType)
				{
					case DataModel.Artifact.ArtifactTypeEnum.Requirement:
						RequirementManager requirement = new RequirementManager();
						projectId = requirement.RetrieveById2(null, artifactId).ProjectId;
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestCase:
						{
							TestCaseManager testCaseManager = new TestCaseManager();
							projectId = testCaseManager.RetrieveById(null, artifactId).ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.Incident:
						IncidentManager incidentManager = new IncidentManager();
						projectId = incidentManager.RetrieveById(artifactId, false).ProjectId;
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Release:
						ReleaseManager releaseManager = new ReleaseManager();
						projectId = releaseManager.RetrieveById(UserManager.UserInternal, null, artifactId).ProjectId;
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Task:
						TaskManager taskManager = new TaskManager();
						projectId = taskManager.RetrieveById(artifactId).ProjectId;
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Risk:
						RiskManager riskManager = new RiskManager();
						projectId = riskManager.Risk_RetrieveById(artifactId).ProjectId;
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Document:
						AttachmentManager attachmentManager = new AttachmentManager();
						projectId = attachmentManager.RetrieveById(artifactId).ProjectId;
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestRun:
						TestRunManager testRunManager = new TestRunManager();
						projectId = testRunManager.RetrieveByIdWithSteps(artifactId).ProjectId;
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestSet:
						TestSetManager testSetManager = new TestSetManager();
						projectId = testSetManager.RetrieveById(null, artifactId).ProjectId;
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestStep:
						{
							TestCaseManager testCaseManager = new TestCaseManager();
							projectId = testCaseManager.RetrieveStepById(null, artifactId).TestCase.ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
						AutomationManager automationManager = new AutomationManager();
						projectId = automationManager.RetrieveHostById(artifactId).ProjectId;
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Placeholder:
						PlaceholderManager placeholderManager = new PlaceholderManager();
						projectId = placeholderManager.Placeholder_RetrieveById(artifactId).ProjectId;
						break;

					default:
						return null;
				}
			}
			catch (ArtifactNotExistsException)
			{
				return null;
			}

			return projectId;
		}

		/// <summary>
		/// Verifies if an artifact exists or not in a specific project
		/// </summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="projectId">The id of the project</param>
		public bool VerifyArtifactExists(DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, int projectId)
		{
			try
			{
				switch (artifactType)
				{
					case DataModel.Artifact.ArtifactTypeEnum.Requirement:
						RequirementManager requirement = new RequirementManager();
						requirement.RetrieveById2(projectId, artifactId);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestCase:
						{
							TestCaseManager testCaseManager = new TestCaseManager();
							testCaseManager.RetrieveById(projectId, artifactId);
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.Incident:
						IncidentManager incidentManager = new IncidentManager();
						incidentManager.RetrieveById(artifactId, false);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Release:
						ReleaseManager releaseManager = new ReleaseManager();
						releaseManager.RetrieveById(UserManager.UserInternal, projectId, artifactId);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Task:
						TaskManager taskManager = new TaskManager();
						taskManager.RetrieveById(artifactId);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Document:
						AttachmentManager attachmentManager = new AttachmentManager();
						attachmentManager.RetrieveById(artifactId);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestRun:
						TestRunManager testRunManager = new TestRunManager();
						testRunManager.RetrieveByIdWithSteps(artifactId);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestSet:
						TestSetManager testSetManager = new TestSetManager();
						testSetManager.RetrieveById(projectId, artifactId);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestStep:
						{
							TestCaseManager testCaseManager = new TestCaseManager();
							TestStep testStep = testCaseManager.RetrieveStepById(projectId, artifactId);
							if (testStep == null)
							{
								return false;
							}
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
						AutomationManager automationManager = new AutomationManager();
						automationManager.RetrieveHostById(artifactId);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Risk:
						RiskManager riskManager = new RiskManager();
						riskManager.Risk_RetrieveById(artifactId);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Placeholder:
						PlaceholderManager placeholderManager = new PlaceholderManager();
						placeholderManager.Placeholder_RetrieveById(artifactId);
						break;

					default:
						return false;
				}
			}
			catch (ArtifactNotExistsException)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Retrieves basic artifact info for any artifact in any project
		/// </summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>A list of info</returns>
		public ArtifactInfo RetrieveArtifactInfo(DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, int? projectId)
		{
			try
			{
				ArtifactInfo artifactInfo = null;

				switch (artifactType)
				{
					case DataModel.Artifact.ArtifactTypeEnum.Requirement:
						{
							RequirementManager requirementManager = new RequirementManager();
							RequirementView requirement = requirementManager.RetrieveById2(projectId, artifactId);
							artifactInfo = new ArtifactInfo();
							artifactInfo.ArtifactId = requirement.RequirementId;
							artifactInfo.ArtifactToken = requirement.ArtifactToken;
							artifactInfo.Name = requirement.Name;
							artifactInfo.Description = requirement.Description;
							artifactInfo.CreatorId = requirement.AuthorId;
							artifactInfo.OwnerId = requirement.OwnerId;
							artifactInfo.ProjectId = requirement.ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.TestCase:
						{
							TestCaseManager testCaseManager = new TestCaseManager();
							TestCase testCase = testCaseManager.RetrieveById2(projectId, artifactId);
							artifactInfo = new ArtifactInfo();
							artifactInfo.ArtifactId = testCase.TestCaseId;
							artifactInfo.ArtifactToken = testCase.ArtifactToken;
							artifactInfo.Name = testCase.Name;
							artifactInfo.Description = testCase.Description;
							artifactInfo.CreatorId = testCase.AuthorId;
							artifactInfo.OwnerId = testCase.OwnerId;
							artifactInfo.ProjectId = testCase.ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.Incident:
						{
							IncidentManager incidentManager = new IncidentManager();
							Incident incident = incidentManager.RetrieveById(artifactId, false);
							artifactInfo = new ArtifactInfo();
							artifactInfo.ArtifactId = incident.IncidentId;
							artifactInfo.ArtifactToken = incident.ArtifactToken;
							artifactInfo.Name = incident.Name;
							artifactInfo.Description = incident.Description;
							artifactInfo.CreatorId = incident.OpenerId;
							artifactInfo.OwnerId = incident.OwnerId;
							artifactInfo.ProjectId = incident.ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.Release:
						{
							ReleaseManager releaseManager = new ReleaseManager();
							ReleaseView release = releaseManager.RetrieveById(UserManager.UserInternal, projectId, artifactId);
							artifactInfo = new ArtifactInfo();
							artifactInfo.ArtifactId = release.ReleaseId;
							artifactInfo.ArtifactToken = release.ArtifactToken;
							artifactInfo.Name = release.Name;
							artifactInfo.Description = release.Description;
							artifactInfo.CreatorId = release.CreatorId;
							artifactInfo.OwnerId = release.OwnerId;
							artifactInfo.ProjectId = release.ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.Task:
						{
							TaskManager taskManager = new TaskManager();
							Task task = taskManager.RetrieveById(artifactId);
							artifactInfo = new ArtifactInfo();
							artifactInfo.ArtifactId = task.TaskId;
							artifactInfo.ArtifactToken = task.ArtifactToken;
							artifactInfo.Name = task.Name;
							artifactInfo.Description = task.Description;
							artifactInfo.CreatorId = task.CreatorId;
							artifactInfo.OwnerId = task.OwnerId;
							artifactInfo.ProjectId = task.ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.Document:
						{
							AttachmentManager attachmentManager = new AttachmentManager();
							Attachment attachment = attachmentManager.RetrieveById(artifactId);
							artifactInfo = new ArtifactInfo();
							artifactInfo.ArtifactId = attachment.AttachmentId;
							artifactInfo.ArtifactToken = attachment.ArtifactToken;
							artifactInfo.Name = attachment.Filename;
							artifactInfo.Description = attachment.Description;
							artifactInfo.CreatorId = attachment.AuthorId;
							artifactInfo.OwnerId = attachment.EditorId;
							artifactInfo.ProjectId = attachment.ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.TestRun:
						{
							TestRunManager testRunManager = new TestRunManager();
							TestRun testRun = testRunManager.RetrieveByIdWithSteps(artifactId);
							artifactInfo = new ArtifactInfo();
							artifactInfo.ArtifactId = testRun.TestRunId;
							artifactInfo.ArtifactToken = testRun.ArtifactToken;
							artifactInfo.Name = testRun.Name;
							artifactInfo.Description = testRun.Description;
							artifactInfo.CreatorId = testRun.TesterId;
							artifactInfo.ProjectId = testRun.ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.TestSet:
						{
							TestSetManager testSetManager = new TestSetManager();
							TestSetView testSet = testSetManager.RetrieveById(projectId, artifactId);
							artifactInfo = new ArtifactInfo();
							artifactInfo.ArtifactId = testSet.TestSetId;
							artifactInfo.ArtifactToken = testSet.ArtifactToken;
							artifactInfo.Name = testSet.Name;
							artifactInfo.Description = testSet.Description;
							artifactInfo.CreatorId = testSet.CreatorId;
							artifactInfo.OwnerId = testSet.OwnerId;
							artifactInfo.ProjectId = testSet.ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.TestStep:
						{
							TestCaseManager testCaseManager = new TestCaseManager();
							TestStep testStep = testCaseManager.RetrieveStepById(projectId, artifactId);
							if (testStep != null)
							{
								artifactInfo = new ArtifactInfo();
								artifactInfo.ArtifactId = testStep.TestStepId;
								artifactInfo.ArtifactToken = testStep.ArtifactToken;
								artifactInfo.Name = testStep.Name;
								artifactInfo.Description = testStep.Description;
								artifactInfo.ProjectId = testStep.ProjectId;
							}
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
						{
							AutomationManager automationManager = new AutomationManager();
							AutomationHostView host = automationManager.RetrieveHostById(artifactId);
							artifactInfo = new ArtifactInfo();
							artifactInfo.ArtifactId = host.AutomationHostId;
							artifactInfo.ArtifactToken = host.ArtifactToken;
							artifactInfo.Name = host.Name;
							artifactInfo.Description = host.Description;
							artifactInfo.ProjectId = host.ProjectId;
							break;
						}

					case DataModel.Artifact.ArtifactTypeEnum.Risk:
						{
							RiskManager riskManager = new RiskManager();
							Risk risk = riskManager.Risk_RetrieveById(artifactId);
							artifactInfo = new ArtifactInfo();
							artifactInfo.ArtifactId = risk.RiskId;
							artifactInfo.ArtifactToken = risk.ArtifactToken;
							artifactInfo.Name = risk.Name;
							artifactInfo.Description = risk.Description;
							artifactInfo.CreatorId = risk.CreatorId;
							artifactInfo.OwnerId = risk.OwnerId;
							artifactInfo.ProjectId = risk.ProjectId;
							break;
						}
				}

				return artifactInfo;
			}
			catch (ArtifactNotExistsException)
			{
				return null;
			}
		}

		/// <summary>
		/// Searches all the artifacts in the system (by name and description) for a specific keyword or artifact token
		/// </summary>
		/// <param name="keywords">The keyword(s) we're searching for</param>
		/// <returns>The list of matching artifacts</returns>
		/// <param name="pageIndex">The start index</param>
		/// <param name="pageSize">The pagination size</param>
		/// <param name="count">The total number of search results</param>
		/// <param name="projectArtifactList">A list of project-artifact entries to filter by</param>
		/// <remarks>You can have a single keyword with spaces by using double quotes</remarks>
		public List<ArtifactView> SearchByKeyword(string keywords, int pageIndex, int pageSize, out int count, List<ProjectArtifactTypeFilter> projectArtifactList = null)
		{
			const string METHOD_NAME = "SearchByKeyword";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ArtifactView> artifacts;

				//First see if we have an artifact id search (e.g. IN:45 or IN45)
				Regex regex = new Regex(Common.Global.REGEX_ARTIFACT_TOKEN_MATCHER, RegexOptions.IgnoreCase);
				Match artifactMatch = regex.Match(keywords);
				if (artifactMatch.Success && artifactMatch.Groups.Count == 3)
				{
					int artifactId = -1;    //Forces no match
					int artifactTypeId = -1;    //Forces no match
					int intValue;
					if (Int32.TryParse(artifactMatch.Groups[2].Value, out intValue))
					{
						artifactId = intValue;
					}
					string artifactPrefix = artifactMatch.Groups[1].Value.Trim();

					//Get the artifact id from the prefix
					ArtifactType artifactType = this.ArtifactType_RetrieveByPrefix(artifactPrefix);
					if (artifactType != null)
					{
						artifactTypeId = artifactType.ArtifactTypeId;
					}

					//Simply query the view directly for the specific artifact ID and artifact type
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Query the view for artifacts that match
						var query = from a in context.ArtifactsView
									where !a.IsDeleted.Value &&
											a.ArtifactId == artifactId &&
											a.ArtifactTypeId == artifactTypeId
									orderby a.LastUpdateDate
									select a;

						//Now execute the query to get the count and paginated results
						count = query.Count();
						artifacts = query.ToList();
					}

					//Finally make sure the user has permissions for this item
					if (projectArtifactList != null && artifacts.Count > 0)
					{
						if (!projectArtifactList.Any(p => p.ArtifactTypeId == artifacts[0].ArtifactTypeId && p.ProjectId == artifacts[0].ProjectId))
						{
							//Clear the returned data
							count = 0;
							artifacts = new List<ArtifactView>();
						}
					}
				}
				else
				{

					//Get the project/artifact type filters (if provided)
					string projectArtifactListString = "";
					if (projectArtifactList != null)
					{
						foreach (ProjectArtifactTypeFilter filter in projectArtifactList)
						{
							if (projectArtifactListString == "")
							{
								projectArtifactListString = filter.ProjectId + ":" + filter.ArtifactTypeId;
							}
							else
							{
								projectArtifactListString += "," + filter.ProjectId + ":" + filter.ArtifactTypeId;
							}
						}
					}

					//See if we have freetext indexing enabled as we have two separate search algorithms depending on
					//which is being used
					if (ConfigurationSettings.Default.Database_UseFreeTextCatalogs)
					{
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							//Call the two stored procedures to get the total count and the paginated range
							count = context.Artifact_CountByKeywordFreetext(keywords, projectArtifactListString).FirstOrDefault().Value;

							//Make pagination is in range
							if (pageIndex < 0)
							{
								pageIndex = 0;
							}

							//If we have no results, return empty list
							if (count > pageIndex)
							{
								artifacts = context.Artifact_RetrieveByKeywordFreetext(keywords, projectArtifactListString, pageIndex + 1, pageSize).ToList();
							}
							else
							{
								return new List<ArtifactView>();
							}
						}

					}
					else
					{
						//Split up the keyword string into separate keywords
						MatchCollection keywordMatches = Regex.Matches(keywords, Common.Global.REGEX_KEYWORD_MATCHER);

						//Create the keyword string (matches separated by a simple comma)
						string keywordString = "";
						foreach (Match keywordMatch in keywordMatches)
						{
							if (keywordString == "")
							{
								keywordString += keywordMatch.Value.Replace("\"", "");
							}
							else
							{
								keywordString += "," + keywordMatch.Value.Replace("\"", "");
							}
						}

						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							//Call the two stored procedures to get the total count and the paginated range
							count = context.Artifact_CountByKeyword(keywordString, projectArtifactListString).FirstOrDefault().Value;

							//Make pagination is in range
							if (pageIndex < 0)
							{
								pageIndex = 0;
							}

							//If we have no results, return empty list
							if (count > pageIndex)
							{
								artifacts = context.Artifact_RetrieveByKeyword(keywordString, projectArtifactListString, pageIndex + 1, pageSize).ToList();
							}
							else
							{
								return new List<ArtifactView>();
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifacts;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region ArtifactType Functions

		///<summary>Retrieves a list of all Artifacts configured in the system.</summary>
		///<param name="excludeNotifyDisabled">Optional. Specifies whether or not to filter out items disabled for Notifications. Default FALSE</param>
		///<param name="includeInactive">Optional. Specifies whether or not to filter out Inactive Artifact types. Default FALSE</param>
		///<param name="excludeDataSyncDisabled">Optional. Specifies whether or not to filter out items disabled for DataSync.</param>
		///<param name="onlyThoseThatSupportAttachments">Only return those that support attachments</param>
		///<param name="onlyThoseThatSupportCustomProperties">Only return those that support custom properties</param>
		///<returns>An ArtifactData dataset</returns>
		public List<ArtifactType> ArtifactType_RetrieveAll(bool excludeNotifyDisabled = false, bool includeInactive = false, bool excludeDataSyncDisabled = false, bool onlyThoseThatSupportAttachments = false, bool onlyThoseThatSupportCustomProperties = false)
		{
			const string METHOD_NAME = "ArtifactType_RetrieveAll";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			List<ArtifactType> artifactTypes;

			try
			{
				//Build the base query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from at in context.ArtifactTypes
								select at;

					//Add on the where clause items
					if (!includeInactive)
					{
						query = query.Where(at => at.IsActive);
					}
					if (excludeNotifyDisabled)
					{
						query = query.Where(at => at.IsNotify);
					}
					if (excludeDataSyncDisabled)
					{
						query = query.Where(at => at.IsDataSync);
					}
					if (onlyThoseThatSupportAttachments)
					{
						query = query.Where(at => at.IsAttachments);
					}
					if (onlyThoseThatSupportCustomProperties)
					{
						query = query.Where(at => at.IsCustomProperties);
					}

					//Add on the sorting
					query = query.OrderBy(at => at.Name).ThenBy(at => at.ArtifactTypeId);

					artifactTypes = query.ToList();
				}

				return artifactTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a specific artifact and all available fields for it.</summary>
		/// <param name="artifactTypeEnum">Type of the Artifact Type to pull.</param>
		/// <returns>Artifact and Field collection</returns>
		public ArtifactType ArtifactType_RetrieveById(DataModel.Artifact.ArtifactTypeEnum artifactTypeEnum)
		{
			return ArtifactType_RetrieveById((int)artifactTypeEnum);
		}

		/// <summary>Retrieves a specific artifact and all available fields for it.</summary>
		/// <param name="artifactTypeId">ID of the Artifact Type to pull.</param>
		/// <returns>Artifact and Field collection</returns>
		public ArtifactType ArtifactType_RetrieveById(int artifactTypeId)
		{
			const string METHOD_NAME = "ArtifactType_RetrieveById";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Set props.
			try
			{
				ArtifactType artifactType;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from at in context.ArtifactTypes.Include("Fields")
								where at.ArtifactTypeId == artifactTypeId
								select at;

					artifactType = query.FirstOrDefault();
				}

				return artifactType;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a specific artifact by its prefix</summary>
		/// <param name="prefix">Prefix of the Artifact Type to pull (e.g. IN) - case insensitive</param>
		/// <returns>Artifact and Field collection</returns>
		public ArtifactType ArtifactType_RetrieveByPrefix(string prefix)
		{
			const string METHOD_NAME = "ArtifactType_RetrieveByPrefix";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Set props.
			try
			{
				ArtifactType artifactType;

				string prefixUpper = prefix.ToUpperInvariant();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from at in context.ArtifactTypes
								where at.Prefix == prefixUpper
								select at;

					artifactType = query.FirstOrDefault();
				}

				return artifactType;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region ArtifactField Functions

		/// <summary>
		/// Changes the column width of an artifact field / custom property on a list page
		/// </summary>
		/// <param name="width">The new width of the field</param>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="userId">The user whose preferences we're using</param>
		/// <param name="fieldName">The name of the field we want to change the display position of</param>
		/// <remarks>This should be used for built-in AND custom properties</remarks>
		public void ArtifactField_ChangeColumnWidth(int projectId, int projectTemplateId, int userId, DataModel.Artifact.ArtifactTypeEnum artifactType, string fieldName, int width)
		{
			const string METHOD_NAME = "ArtifactField_ChangeColumnWidth";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Instantiate custom property manager (for custom fields)
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Change the width (handle custom property and standard fields separately
				int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(fieldName);
				if (customPropertyNumber.HasValue)
				{
					customPropertyManager.CustomProperty_UpdateColumnListWidth(projectId, projectTemplateId, userId, artifactType, customPropertyNumber.Value, width);
				}
				else
				{
					ArtifactField_UpdateColumnListWidth(projectId, userId, artifactType, fieldName, width);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Changes the list order of a particular artifact field / custom property on a list page
		/// </summary>
		/// <param name="newPosition">The new position of the field</param>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user whose preferences we're using</param>
		/// <param name="fieldName">The name of the field we want to change the display position of</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <remarks>This should be used for built-in AND custom properties</remarks>
		public void ArtifactField_ChangeListPosition(int projectId, int projectTemplateId, int userId, DataModel.Artifact.ArtifactTypeEnum artifactType, string fieldName, int newPosition)
		{
			const string METHOD_NAME = "ArtifactField_ChangeListPosition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Instantiate custom property manager (for custom fields)
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//First we need to get the current list of fields/properties so that we know the current positions/indexes
				List<ArtifactListFieldDisplay> artifactListFields = this.ArtifactField_RetrieveForLists(projectId, userId, artifactType);

				//Locate the current field, if we cannot find it throw an exception
				ArtifactListFieldDisplay movedArtifactListField = artifactListFields.FirstOrDefault(af => af.Name == fieldName);
				if (movedArtifactListField == null)
				{
					throw new InvalidOperationException("Unable to find a field with name '" + fieldName + "'");
				}
				int oldPosition = movedArtifactListField.ListPosition;
				//Now we need to loop through all the list fields and either add a record or update one to have the modified position
				foreach (ArtifactListFieldDisplay artifactListField in artifactListFields)
				{
					//See if we've moved to the left or right
					if (newPosition > oldPosition)
					{
						//Moved to the right

						//If we are after the old position, need to decrement
						if (artifactListField.ListPosition > oldPosition && artifactListField.ListPosition <= newPosition)
						{
							//Decrement the position (handle custom property and standard fields separately
							int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(artifactListField.Name);
							if (customPropertyNumber.HasValue)
							{
								customPropertyManager.CustomProperty_UpdateListPosition(projectId, projectTemplateId, userId, artifactType, customPropertyNumber.Value, artifactListField.ListPosition - 1, artifactListField.IsVisible);
							}
							else
							{
								ArtifactField_UpdateListPosition(projectId, userId, artifactType, artifactListField.Name, artifactListField.ListPosition - 1, artifactListField.IsVisible);
							}
						}
						else if (artifactListField.ListPosition == oldPosition)
						{
							//Update the position (handle custom property and standard fields separately
							int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(artifactListField.Name);
							if (customPropertyNumber.HasValue)
							{
								customPropertyManager.CustomProperty_UpdateListPosition(projectId, projectTemplateId, userId, artifactType, customPropertyNumber.Value, newPosition, artifactListField.IsVisible);
							}
							else
							{
								ArtifactField_UpdateListPosition(projectId, userId, artifactType, artifactListField.Name, newPosition, artifactListField.IsVisible);
							}
						}
					}
					else if (newPosition < oldPosition)
					{
						//Moved to the left

						//If we are before the old position and after the new position, need to increment
						if (artifactListField.ListPosition < oldPosition && artifactListField.ListPosition >= newPosition)
						{
							//Decrement the position (handle custom property and standard fields separately
							int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(artifactListField.Name);
							if (customPropertyNumber.HasValue)
							{
								customPropertyManager.CustomProperty_UpdateListPosition(projectId, projectTemplateId, userId, artifactType, customPropertyNumber.Value, artifactListField.ListPosition + 1, artifactListField.IsVisible);
							}
							else
							{
								ArtifactField_UpdateListPosition(projectId, userId, artifactType, artifactListField.Name, artifactListField.ListPosition + 1, artifactListField.IsVisible);
							}
						}
						else if (artifactListField.ListPosition == oldPosition)
						{
							//Update the position (handle custom property and standard fields separately
							int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(artifactListField.Name);
							if (customPropertyNumber.HasValue)
							{
								customPropertyManager.CustomProperty_UpdateListPosition(projectId, projectTemplateId, userId, artifactType, customPropertyNumber.Value, newPosition, artifactListField.IsVisible);
							}
							else
							{
								ArtifactField_UpdateListPosition(projectId, userId, artifactType, artifactListField.Name, newPosition, artifactListField.IsVisible);
							}
						}
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Updates the width of a standard field (not a custom property)
		/// </summary>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user whose preferences we're using</param>
		/// <param name="fieldName">The name of the field we want to change the display position of</param>
		/// <param name="newWidth">The new width</param>
		protected void ArtifactField_UpdateColumnListWidth(int projectId, int userId, DataModel.Artifact.ArtifactTypeEnum artifactType, string fieldName, int newWidth)
		{
			const string METHOD_NAME = "ArtifactField_UpdateColumnListWidth";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to retrieve the user artifact field record if there is one
					var query = from uf in context.UserArtifactFields.Include("ArtifactField")
								where uf.UserId == userId && uf.ProjectId == projectId &&
										uf.ArtifactField.ArtifactTypeId == (int)artifactType &&
										uf.ArtifactField.Name == fieldName &&
										uf.ArtifactField.IsActive
								select uf;

					UserArtifactField userArtifactField = query.FirstOrDefault();
					if (userArtifactField == null)
					{
						//We need to create a new record entry. First we need to get the artifact field id for this field name
						//if there isn't one we can just ignore the operation and fail quietly
						var query2 = from af in context.ArtifactFields
									 where af.ArtifactTypeId == (int)artifactType && af.IsActive &&
										 af.Name == fieldName
									 select af;
						ArtifactField artifactField = query2.FirstOrDefault();
						if (artifactField != null)
						{
							userArtifactField = new UserArtifactField();
							context.UserArtifactFields.AddObject(userArtifactField);
							userArtifactField.UserId = userId;
							userArtifactField.ProjectId = projectId;
							userArtifactField.ArtifactFieldId = artifactField.ArtifactFieldId;
							userArtifactField.ListPosition = null;
							userArtifactField.IsVisible = true;
							userArtifactField.Width = newWidth;
							context.SaveChanges();
						}
					}
					else
					{
						//We just need to update the position
						userArtifactField.StartTracking();
						userArtifactField.Width = newWidth;
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Updates the position record of a standard field (not a custom property)
		/// </summary>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user whose preferences we're using</param>
		/// <param name="fieldName">The name of the field we want to change the display position of</param>
		/// <param name="newPosition">The new position</param>
		/// <param name="visible">Is the field current visible</param>
		protected void ArtifactField_UpdateListPosition(int projectId, int userId, DataModel.Artifact.ArtifactTypeEnum artifactType, string fieldName, int newPosition, bool visible)
		{
			const string METHOD_NAME = "ArtifactField_UpdateListPosition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to retrieve the user artifact field record if there is one
					var query = from uf in context.UserArtifactFields.Include("ArtifactField")
								where uf.UserId == userId && uf.ProjectId == projectId &&
										uf.ArtifactField.ArtifactTypeId == (int)artifactType &&
										uf.ArtifactField.Name == fieldName &&
										uf.ArtifactField.IsActive
								select uf;

					UserArtifactField userArtifactField = query.FirstOrDefault();
					if (userArtifactField == null)
					{
						//We need to create a new record entry. First we need to get the artifact field id for this field name
						//if there isn't one we can just ignore the operation and fail quietly
						var query2 = from af in context.ArtifactFields
									 where af.ArtifactTypeId == (int)artifactType && af.IsActive &&
										 af.Name == fieldName
									 select af;
						ArtifactField artifactField = query2.FirstOrDefault();
						if (artifactField != null)
						{
							userArtifactField = new UserArtifactField();
							context.UserArtifactFields.AddObject(userArtifactField);
							userArtifactField.UserId = userId;
							userArtifactField.ProjectId = projectId;
							userArtifactField.ArtifactFieldId = artifactField.ArtifactFieldId;
							userArtifactField.ListPosition = newPosition;
							userArtifactField.IsVisible = visible;
							context.SaveChanges();
						}
					}
					else
					{
						//We just need to update the position
						userArtifactField.StartTracking();
						userArtifactField.ListPosition = newPosition;
						userArtifactField.IsVisible = visible;
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Changes the visibility status of a particular artifact field
		/// </summary>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user whose preferences we're using</param>
		/// <param name="fieldName">The name of the field we want to toggle the visibility for</param>
		/// <remarks>This only should be used for built-in fields, there is an equivalent method for custom properties</remarks>
		public void ArtifactField_ToggleListVisibility(int projectId, int userId, DataModel.Artifact.ArtifactTypeEnum artifactType, string fieldName)
		{
			const string METHOD_NAME = "ArtifactField_ToggleListVisibility";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to retrieve the user artifact field record if there is one
					var query = from uf in context.UserArtifactFields.Include("ArtifactField")
								where uf.UserId == userId && uf.ProjectId == projectId &&
										uf.ArtifactField.ArtifactTypeId == (int)artifactType &&
										uf.ArtifactField.Name == fieldName &&
										uf.ArtifactField.IsActive
								select uf;

					UserArtifactField userArtifactField = query.FirstOrDefault();
					if (userArtifactField == null)
					{
						//We need to create a new record entry. First we need to get the artifact field id for this field name
						//if there isn't one we can just ignore the operation and fail quietly
						var query2 = from af in context.ArtifactFields
									 where af.ArtifactTypeId == (int)artifactType && af.IsActive &&
										 af.Name == fieldName
									 select af;
						ArtifactField artifactField = query2.FirstOrDefault();
						if (artifactField != null)
						{
							userArtifactField = new UserArtifactField();
							context.UserArtifactFields.AddObject(userArtifactField);
							userArtifactField.UserId = userId;
							userArtifactField.ProjectId = projectId;
							userArtifactField.ArtifactFieldId = artifactField.ArtifactFieldId;
							userArtifactField.IsVisible = (!artifactField.IsListDefault);
							context.SaveChanges();
						}
					}
					else
					{
						//We just need to flip the visible flag
						userArtifactField.StartTracking();
						userArtifactField.IsVisible = !userArtifactField.IsVisible;
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Retrieves the list of artifact fields that are workflow configurable
		/// </summary>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <returns>List of artifact fields</returns>
		public List<ArtifactField> ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "ArtifactField_RetrieveWorkflowConfigurable";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				if (!_workflowConfigurableArtifactFields.ContainsKey((int)artifactType))
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from af in context.ArtifactFields
									where af.IsActive &&
										  af.IsWorkflowConfig &&
										  af.ArtifactTypeId == (int)artifactType
									orderby af.Name, af.ArtifactFieldId
									select af;

						_workflowConfigurableArtifactFields[(int)artifactType] = query.ToList();
					}

				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return _workflowConfigurableArtifactFields[(int)artifactType];
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of fields to display in the artifact grid for a specific artifact type and project.
		/// </summary>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>Complete list of artifact fields and custom properties that should be displayed in the artifact details page for this project</returns>
		/// <remarks>Also includes any project custom properties configured</remarks>
		public List<ArtifactFieldDisplay> ArtifactField_RetrieveAll(int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "ArtifactField_RetrieveAll";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ArtifactFieldDisplay> artifactFields;

				//We use the imported function to get the list of fields for the current user, project and type
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					artifactFields = context.Artifact_RetrieveAllFields(projectId, (int)artifactType, CustomProperty.FIELD_PREPEND).ToList();
				}

				//If we have certain product licenses installed, then we need to disable certain fields
				if (!IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.TestCase))
				{
					//Remove requirements and release test coverage columns
					for (int i = 0; i < artifactFields.Count; i++)
					{
						ArtifactFieldDisplay artifactField = artifactFields[i];
						if (artifactField.Name == "CoverageId")
						{
							artifactFields.Remove(artifactField);
						}
					}
				}
				if (!IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Task))
				{
					//Remove requirements and release task progress columns
					for (int i = 0; i < artifactFields.Count; i++)
					{
						ArtifactFieldDisplay artifactField = artifactFields[i];
						if (artifactField.Name == "ProgressId")
						{
							artifactFields.Remove(artifactField);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return (artifactFields);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of active artifact fields - used for the report filters
		/// </summary>
		/// <param name="artifactType">The artifact type we're interested in</param>
		/// <returns>List of artifact fields</returns>
		/// <remarks>Only returns fields of type = lookup</remarks>
		public List<ArtifactField> ArtifactField_RetrieveForReporting(DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "ArtifactField_RetrieveForReporting";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				if (!_reportableArtifactFields.ContainsKey((int)artifactType))
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from af in context.ArtifactFields
									where af.IsActive &&
										  af.IsReport &&
										  af.ArtifactTypeId == (int)artifactType
									orderby af.ArtifactFieldTypeId, af.Caption, af.ArtifactFieldId
									select af;

						_reportableArtifactFields[(int)artifactType] = query.ToList();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return _reportableArtifactFields[(int)artifactType];
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of fields to display in the artifact grid for a specific artifact type, project and user. It
		/// includes both visible and hidden fields so you need to check the IsVisible property of each one
		/// </summary>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user whose preferences we're using</param>
		/// <returns>list of artifact fields and custom properties that should be displayed in the artifact list page for this user/project</returns>
		/// <remarks>Also includes any project custom properties in the list</remarks>
		public List<ArtifactListFieldDisplay> ArtifactField_RetrieveForLists(int projectId, int userId, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "ArtifactField_RetrieveForLists";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ArtifactListFieldDisplay> artifactFields;

				//We use the imported function to get the list of fields for the current user, project and type
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					artifactFields = context.Artifact_RetrieveListFields(projectId, userId, (int)artifactType, CustomProperty.FIELD_PREPEND).ToList();
				}

				//If we have certain product licenses installed, then we need to disable certain fields
				if (!IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.TestCase))
				{
					//Remove requirements and release test coverage columns
					for (int i = 0; i < artifactFields.Count; i++)
					{
						ArtifactListFieldDisplay artifactField = artifactFields[i];
						if (artifactField.Name == "CoverageId")
						{
							artifactFields.Remove(artifactField);
						}
					}
				}
				if (!IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Task))
				{
					//Remove requirements and release task progress columns
					for (int i = 0; i < artifactFields.Count; i++)
					{
						ArtifactListFieldDisplay artifactField = artifactFields[i];
						if (artifactField.Name == "ProgressId")
						{
							artifactFields.Remove(artifactField);
						}
					}
				}
				if (IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Release))
				{
					//Remove requirements and release task progress columns
					for (int i = 0; i < artifactFields.Count; i++)
					{
						ArtifactListFieldDisplay artifactField = artifactFields[i];
						if (artifactField.Name == "PeriodicReviewAlertId")
						{
							artifactFields.Remove(artifactField);
						}
					}
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return (artifactFields);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of fields to display in the artifact grid for a specific artifact type. It does not include the visible
		/// flag unlike the other overload, it is just the list of possible fields, and does not include custom properties
		/// </summary>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <returns>list of artifact fields for this artifact type</returns>
		public List<ArtifactField> ArtifactField_RetrieveForLists(DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "ArtifactField_RetrieveForLists";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ArtifactField> artifactFields;

				//We use the imported function to get the list of fields for the current user, project and type
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the list configurable fields in general
					var query = from af in context.ArtifactFields
								where af.ArtifactTypeId == (int)artifactType && af.IsActive && af.IsListConfig
								orderby af.ListDefaultPosition, af.ArtifactFieldId
								select af;

					artifactFields = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactFields;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Gets all fields for a speficied artifact type.</summary>
		/// <param name="ArtifactTypeId">The artifact type ID.</param>
		/// <param name="excludeDisabledNotify">Optional. Exclude non-notification fields. (Default: true)</param>
		/// <param name="excludeInactive">Optional. Exclude inactive fields. (Default: true)</param>
		/// <returns>ArtifactDataSet</returns>
		public List<ArtifactField> ArtifactField_RetrieveAll(int artifactTypeId, bool excludeDisabledNotify = true, bool excludeInactive = true)
		{
			const string METHOD_NAME = "ArtifactField_RetrieveAll";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			List<ArtifactField> artifactFields;

			try
			{
				//Build the base query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from af in context.ArtifactFields
								where af.ArtifactTypeId == artifactTypeId
								select af;

					//Add on the where clause items
					if (excludeDisabledNotify)
					{
						query = query.Where(af => af.IsNotify);
					}
					if (excludeInactive)
					{
						query = query.Where(af => af.IsActive);
					}

					//Add on the sorting
					query = query.OrderBy(af => af.Name).ThenBy(af => af.ArtifactFieldId);

					artifactFields = query.ToList();
				}

				return artifactFields;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the Field ID of the given artifact type and field's name.</summary>
		/// <param name="artifactType">The artifact type we're interested in</param>
		/// <param name="fieldName">The name of the field.</param>
		/// <returns>Integer, the number of the Field ID, or null if not found or error.</returns>
		public int? RetrieveArtifactFieldIdByName(DataModel.Artifact.ArtifactTypeEnum artifactType, string fieldName)
		{
			ArtifactField artifactField = ArtifactField_RetrieveByName(artifactType, fieldName);
			if (artifactField == null)
			{
				return null;
			}
			else
			{
				return artifactField.ArtifactFieldId;
			}
		}

		/// <summary>Retrieves the Field of the given artifact type and field's name.</summary>
		/// <param name="artifactType">The artifact type we're interested in</param>
		/// <param name="fieldName">The name of the field.</param>
		/// <returns>The field entity</returns>
		public ArtifactField ArtifactField_RetrieveByName(DataModel.Artifact.ArtifactTypeEnum artifactType, string fieldName)
		{
			const string METHOD_NAME = "ArtifactField_RetrieveByName()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Set props.
			try
			{
				ArtifactField artifactField;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from af in context.ArtifactFields
								where af.ArtifactTypeId == (int)artifactType &&
										af.IsActive &&
										af.Name == fieldName
								select af;

					artifactField = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactField;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Retrieves a list of active artifact fields that can be used for lookups/reporting
		/// </summary>
		/// <param name="artifactType">The artifact type we're interested in</param>
		/// <returns>List of artifact fields</returns>
		/// <remarks>Only returns fields of type = lookup/hierarchy</remarks>
		public List<ArtifactField> ArtifactField_RetrieveForLookups(DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "ArtifactField_RetrieveForLookups";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ArtifactField> artifactFields;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from af in context.ArtifactFields
								where af.IsActive &&
										af.ArtifactTypeId == (int)artifactType &&
										(af.ArtifactFieldTypeId == (int)DataModel.Artifact.ArtifactFieldTypeEnum.Lookup || af.ArtifactFieldTypeId == (int)DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup)
								orderby af.Caption, af.ArtifactFieldId
								select af;

					artifactFields = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactFields;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Will pull the specified field's data, or null if none found.</summary>
		/// <param name="artifactFieldId">The ID of the field to pull.</param>
		/// <returns>ArtifactFieldRow containing data, or NULL.</returns>
		public ArtifactField ArtifactField_RetrieveById(int artifactFieldId)
		{
			const string METHOD_NAME = "ArtifactField_RetrieveById";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Set props.
			try
			{
				ArtifactField artifactField;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from af in context.ArtifactFields
								where af.ArtifactFieldId == artifactFieldId
								select af;

					artifactField = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactField;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public ArtifactField ArtifactField_RetrieveByArtifactId(int artifactTypeId, string name)
		{
			const string METHOD_NAME = "ArtifactField_RetrieveById";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Set props.
			try
			{
				ArtifactField artifactField;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from af in context.ArtifactFields
								where af.ArtifactTypeId == artifactTypeId && af.Name == name
								select af;

					artifactField = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactField;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion
	}

	/// <summary>
	/// General info on an artifact, used in tooltips
	/// </summary>
	public class ArtifactInfo
	{
		public int ArtifactId { get; set; }

		public string ArtifactToken { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public int? OwnerId { get; set; }

		public int? CreatorId { get; set; }

		public int? ProjectId { get; set; }
	}
}
