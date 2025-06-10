using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Provides the web service used to interacting with the various client-side artifact association AJAX components
	/// </summary>
	[
	AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
	]
	public class AssociationService : SortedListServiceBase, IAssociationService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.AssociationService::";

		protected const int MAX_ROWS = 500; //Return no more than 500 rows

		#region IAssociationService methods

		/// <summary>
		/// Returns a list of the projects that share the provided artifacts with this project
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="artifactTypeIds">The id of the artifact types we're interested in</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="displayTypeId">The type of association panel</param>
		/// <returns></returns>
		public List<NameValue> Association_RetrieveForDestProjectAndArtifact(int projectId, List<int> artifactTypeIds, int? displayTypeId)
		{
			const string METHOD_NAME = "Association_RetrieveForDestProjectAndArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized for this project, we don't check specific permissions since we only return a project id/name list
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				List<NameValue> dataItems = new List<NameValue>();

				//See if we have a display mode specified, and if so, don't return other projects
				//for association panels that NEVER support cross-project linking at any time
				if (displayTypeId.HasValue)
				{
					DataModel.Artifact.DisplayTypeEnum displayType = (DataModel.Artifact.DisplayTypeEnum)displayTypeId.Value;
					if (displayType == Artifact.DisplayTypeEnum.Build_Associations || displayType == Artifact.DisplayTypeEnum.Release_TestCases
						|| displayType == Artifact.DisplayTypeEnum.Requirement_Tasks || displayType == Artifact.DisplayTypeEnum.Build_Incidents
						|| displayType == Artifact.DisplayTypeEnum.Attachments || displayType == Artifact.DisplayTypeEnum.TestCase_Incidents
						|| displayType == Artifact.DisplayTypeEnum.SourceCodeRevision_Associations || displayType == Artifact.DisplayTypeEnum.SourceCodeFile_Associations
						|| displayType == Artifact.DisplayTypeEnum.TestCase_Releases || displayType == Artifact.DisplayTypeEnum.TestCase_Runs
						|| displayType == Artifact.DisplayTypeEnum.TestCase_TestSets || displayType == Artifact.DisplayTypeEnum.TestSet_Incidents
						|| displayType == Artifact.DisplayTypeEnum.TestSet_TestCases || displayType == Artifact.DisplayTypeEnum.TestStep_Requirements)
					{
						return dataItems;
					}
				}

				//Get the list of projects that can share these artifacts
				ProjectManager projectManager = new ProjectManager();
				List<ProjectArtifactSharing> projectSharing = projectManager.ProjectAssociation_RetrieveForDestProjectAndArtifacts(projectId, artifactTypeIds);
				IEnumerable<IGrouping<Project, ProjectArtifactSharing>> groupedProjects = projectSharing.GroupBy(p => p.SourceProject);
				foreach (IGrouping<Project, ProjectArtifactSharing> projectGroup in groupedProjects)
				{
					NameValue nameValue = new NameValue();
					nameValue.Id = projectGroup.Key.ProjectId;
					nameValue.Name = projectGroup.Key.Name;
					nameValue.ChildIds = projectGroup.Select(p => p.ArtifactTypeId).ToList();
					dataItems.Add(nameValue);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItems;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of folders (hierarchical) for artifact types that have them
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactTypeId">The id of the artifact</param>
		/// <returns></returns>
		public List<HierarchicalItem> Association_RetrieveArtifactFolders(int projectId, int artifactTypeId)
		{
			const string METHOD_NAME = "Association_RetrieveForDestProjectAndArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized for this project, we need to be able to view the artifact type (not limited)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (Artifact.ArtifactTypeEnum)artifactTypeId);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the folders for that type
				List<HierarchicalItem> dataItems = null;
				switch ((Artifact.ArtifactTypeEnum)artifactTypeId)
				{
					case Artifact.ArtifactTypeEnum.Requirement:
						{
							//Get the list of summary requirements (packages) for the project
							RequirementManager requirementManager = new RequirementManager();
							List<RequirementView> packages = requirementManager.Requirement_RetrieveSummaryBacklog(projectId);

							//Populate the data objects
							dataItems = PopulateFolders(packages, "RequirementId", null);
						}
						break;

					case Artifact.ArtifactTypeEnum.TestCase:
					case Artifact.ArtifactTypeEnum.TestStep:
						{
							//Get the list of test case folders
							TestCaseManager testCaseManager = new TestCaseManager();
							List<TestCaseFolderHierarchyView> folders = testCaseManager.TestCaseFolder_GetList(projectId);

							//Populate the data objects
							dataItems = PopulateFolders(folders, "TestCaseFolderId", "ParentTestCaseFolderId");
						}
						break;

					case Artifact.ArtifactTypeEnum.TestSet:
						{
							//Get the list of test set folders
							TestSetManager testSetManager = new TestSetManager();
							List<TestSetFolderHierarchyView> folders = testSetManager.TestSetFolder_GetList(projectId);

							//Populate the data objects
							dataItems = PopulateFolders(folders, "TestSetFolderId", "ParentTestSetFolderId");
						}
						break;

					case Artifact.ArtifactTypeEnum.Task:
						{
							//Get the list of task folders
							TaskManager taskManager = new TaskManager();
							List<TaskFolderHierarchyView> folders = taskManager.TaskFolder_GetList(projectId);

							//Populate the data objects
							dataItems = PopulateFolders(folders, "TaskFolderId", "ParentTaskFolderId");
						}
						break;

					case Artifact.ArtifactTypeEnum.Document:
						{
							//Get the list of document folders
							AttachmentManager attachmentManager = new AttachmentManager();
							List<ProjectAttachmentFolderHierarchy> folders = attachmentManager.RetrieveFoldersByProjectId(projectId);

							//Populate the data objects
							dataItems = PopulateFolders(folders, "ProjectAttachmentFolderId", "ParentProjectAttachmentFolderId");
						}
						break;
				}

				return dataItems;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// populates the folders list
		/// </summary>
		/// <typeparam name="T">The type</typeparam>
		/// <param name="projectId">The id of the project</param>
		/// <param name="folders">The list of folders</param>
		protected List<HierarchicalItem> PopulateFolders<T>(List<T> folders, string idFieldName, string parentIdFieldName)
			where T : Entity
		{
			List<HierarchicalItem> dataItems = new List<HierarchicalItem>();

			//Loop through the folders
			foreach (T folder in folders)
			{
				HierarchicalItem dataItem = new HierarchicalItem();
				dataItem.Id = (int)folder[idFieldName];
				dataItem.Name = (string)folder["Name"];
				dataItem.IndentLevel = (string)folder["IndentLevel"];
				if (!String.IsNullOrEmpty(parentIdFieldName))
				{
					dataItem.ParentId = (int?)folder[parentIdFieldName];
				}
				dataItems.Add(dataItem);
			}

			return dataItems;
		}

		/// <summary>
		/// Retrieves the tooltip for a specific artifact
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactTypeId">The id of the artifact type</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <returns>The tooltip</returns>
		public string Association_RetrieveTooltip(int projectId, int artifactTypeId, int artifactId)
		{
			const string METHOD_NAME = "Association_RetrieveTooltip";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized for this project, we need to be able to view the artifact type (not limited)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (Artifact.ArtifactTypeEnum)artifactTypeId);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//See which type of artifact we have
				string tooltip = "";
				switch ((Artifact.ArtifactTypeEnum)artifactTypeId)
				{
					case Artifact.ArtifactTypeEnum.Requirement:
						{
							RequirementManager requirementManager = new RequirementManager();
							RequirementView requirement = requirementManager.RetrieveById2(null, artifactId);
							if (String.IsNullOrEmpty(requirement.Description))
							{
								tooltip = Microsoft.Security.Application.Encoder.HtmlEncode(requirement.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, artifactId, true);
							}
							else
							{
								tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(requirement.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, artifactId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(requirement.Description);
							}

							//See if we have any comments to append
							IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(artifactId, Artifact.ArtifactTypeEnum.Requirement, false);
							if (comments.Count() > 0)
							{
								IDiscussion lastComment = comments.Last();
								tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
									GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
									GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
									Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
									);
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestCase:
						{
							TestCaseManager testCaseManager = new TestCaseManager();
							//First we need to get the test case itself
							TestCaseView testCaseView = testCaseManager.RetrieveById(null, artifactId);

							//Next we need to get the list of successive parent folders
							if (testCaseView.TestCaseFolderId.HasValue)
							{
								List<TestCaseFolderHierarchyView> parentFolders = testCaseManager.TestCaseFolder_GetParents(testCaseView.ProjectId, testCaseView.TestCaseFolderId.Value, true);
								foreach (TestCaseFolderHierarchyView parentFolder in parentFolders)
								{
									tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
								}
							}

							//Now we need to get the test case itself
							tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testCaseView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE, artifactId, true) + "</u>";
							if (!String.IsNullOrEmpty(testCaseView.Description))
							{
								tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testCaseView.Description);
							}

							//See if we have any comments to append
							IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(artifactId, Artifact.ArtifactTypeEnum.TestCase, false);
							if (comments.Count() > 0)
							{
								IDiscussion lastComment = comments.Last();
								tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
									GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
									GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
									Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
									);
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Task:
						{
							TaskManager taskManager = new TaskManager();
							TaskView taskView = taskManager.TaskView_RetrieveById(artifactId);
							if (String.IsNullOrEmpty(taskView.Description))
							{
								//See if we have a requirement or folder it belongs to
								if (taskView.RequirementId.HasValue)
								{
									tooltip += Microsoft.Security.Application.Encoder.HtmlEncode(taskView.RequirementName) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, taskView.RequirementId.Value, true) + " &gt; ";
								}
								else if (taskView.TaskFolderId.HasValue)
								{
									List<TaskFolderHierarchyView> parentFolders = taskManager.TaskFolder_GetParents(taskView.ProjectId, taskView.TaskFolderId.Value, true);
									foreach (TaskFolderHierarchyView parentFolder in parentFolders)
									{
										tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
									}
								}
								tooltip += Microsoft.Security.Application.Encoder.HtmlEncode(taskView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TASK, taskView.TaskId, true);
							}
							else
							{
								//See if we have a requirement or folder it belongs to
								if (taskView.RequirementId.HasValue)
								{
									tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(taskView.RequirementName) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, taskView.RequirementId.Value, true) + "</u> &gt; ";
								}
								else if (taskView.TaskFolderId.HasValue)
								{
									List<TaskFolderHierarchyView> parentFolders = taskManager.TaskFolder_GetParents(taskView.ProjectId, taskView.TaskFolderId.Value, true);
									foreach (TaskFolderHierarchyView parentFolder in parentFolders)
									{
										tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
									}
								}

								tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(taskView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TASK, taskView.TaskId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(taskView.Description);
							}

							//See if we have any comments to append
							IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(artifactId, Artifact.ArtifactTypeEnum.Task, false);
							if (comments.Count() > 0)
							{
								IDiscussion lastComment = comments.Last();
								tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
									GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
									GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
									Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
									);
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Incident:
						{
							IncidentManager incidentManager = new IncidentManager();
							Incident incident = incidentManager.RetrieveById(artifactId, true);
							tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(incident.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_INCIDENT, incident.IncidentId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(incident.Description);

							//See if we have any comments to append
							if (incident.Resolutions.Count > 0)
							{
								IncidentResolution resolution = incident.Resolutions.OrderByDescending(r => r.CreationDate).First();

								tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
									GlobalFunctions.LocalizeDate(resolution.CreationDate).ToShortDateString(),
									GlobalFunctions.HtmlRenderAsPlainText(resolution.Resolution),
									resolution.Creator.FullName
									);
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestStep:
						{
							TestCaseManager testCaseManager = new TestCaseManager();
							TestStepView testStep = testCaseManager.RetrieveStepById2(null, artifactId);
							if (testStep == null)
							{
								Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for test step " + artifactId);
								Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
								Logger.Flush();
								return Resources.Messages.Global_UnableRetrieveTooltip;
							}
							if (String.IsNullOrEmpty(testStep.ExpectedResult))
							{
								//See if we have a real test step or a linked test case
								if (!testStep.LinkedTestCaseId.HasValue)
								{
									tooltip = GlobalFunctions.HtmlRenderAsPlainText(testStep.Description);
								}
								else
								{
									//First concatenate the linked test case name and id
									tooltip = "<u>" + Resources.Main.TestStepService_Call + " '" + testStep.LinkedTestCaseName + "' (" + GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE + testStep.LinkedTestCaseId.Value + ")</u>";

									//Now we need to check for parameters, and add on the parameter list to the name of the linked test case
									List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(testStep.TestStepId);
									if (testStepParameterValues.Count > 0)
									{
										tooltip += "<i>";
										for (int i = 0; i < testStepParameterValues.Count; i++)
										{
											if (i == 0)
											{
												tooltip += "<br />" + Resources.Main.TestStepService_With + " ";
											}
											else
											{
												tooltip += ", ";
											}
											tooltip += testStepParameterValues[i].Name + "=" + testStepParameterValues[i].Value;
										}
										tooltip += "</i>";
									}
								}
							}
							else
							{
								tooltip = "<u>" + GlobalFunctions.HtmlRenderAsPlainText(testStep.Description) + "</u><br />\n<i>" + GlobalFunctions.HtmlRenderAsPlainText(testStep.ExpectedResult) + "</i>";
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Release:
						{
							ReleaseManager releaseManager = new ReleaseManager();
							ReleaseView release = releaseManager.RetrieveById2(null, artifactId);
							if (String.IsNullOrEmpty(release.Description))
							{
								tooltip = Microsoft.Security.Application.Encoder.HtmlEncode(release.FullName);
							}
							else
							{
								tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(release.FullName) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(release.Description);
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Document:
						{
							AttachmentManager attachmentManager = new AttachmentManager();
							Attachment attachment = attachmentManager.RetrieveById(artifactId);
							if (String.IsNullOrWhiteSpace(attachment.Description))
							{
								tooltip = Microsoft.Security.Application.Encoder.HtmlEncode(attachment.Filename);
							}
							else
							{
								tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(attachment.Filename) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(attachment.Description);
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestRun:
						{
							TestRunManager testRunManager = new TestRunManager();
							TestRun testrun = testRunManager.RetrieveByIdWithSteps(artifactId);
							//display the name and execution status
							tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testrun.Name) + " - " + testrun.ExecutionStatus.Name + "</u>";
							if (!String.IsNullOrEmpty(testrun.Description))
							{
								//Add the description
								tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testrun.Description);
							}

							//If automated, add the short message
							if (testrun.TestRunTypeId == (int)TestRun.TestRunTypeEnum.Automated && testrun.RunnerAssertCount.HasValue && !String.IsNullOrEmpty(testrun.RunnerMessage))
							{
								tooltip += "<br />\n<i>" + Resources.Fields.AssertCount + ": " + testrun.RunnerAssertCount.Value + "<br />\n" +
									Resources.Fields.RunnerMessage + ": " + Microsoft.Security.Application.Encoder.HtmlEncode(testrun.RunnerMessage) + "</i>\n";
							}
							else
							{
								//If manual, get all the steps with actual results
								foreach (TestRunStep testRunStep in testrun.TestRunSteps)
								{
									if (!String.IsNullOrEmpty(testRunStep.ActualResult))
									{
										tooltip += "<br /><i>- " + GlobalFunctions.HtmlRenderAsPlainText(testRunStep.ActualResult) + "</i>\n";
									}
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Risk:
						{
							RiskManager riskManager = new RiskManager();
							RiskView risk = riskManager.Risk_RetrieveById2(artifactId);
							if (String.IsNullOrEmpty(risk.Description))
							{
								tooltip = Microsoft.Security.Application.Encoder.HtmlEncode(risk.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_RISK, artifactId, true);
							}
							else
							{
								tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(risk.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_RISK, artifactId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(risk.Description);
							}

							//See if we have any comments to append
							IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(artifactId, Artifact.ArtifactTypeEnum.Risk, false);
							if (comments.Count() > 0)
							{
								IDiscussion lastComment = comments.Last();
								tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
									GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
									GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
									Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
									);
							}
						}
						break;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return tooltip;
			}
			catch (ArtifactNotExistsException)
			{
				//The tooltip is not available
				return Resources.Messages.Global_TooltipNotAvailable;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of artifacts that match the search
		/// </summary>
		/// <param name="projectId">The id of the project the current artifact is in</param>
		/// <param name="artifactTypeId">The type of the artifact we're adding associations to</param>
		/// <param name="artifactId">The id of the artifact we're adding associations to</param>
		/// <param name="searchArtifactTypeId">The type of artifact we're searching for</param>
		/// <param name="searchFolderId">The id of the folder the artifact is in (optional)</param>
		/// <param name="searchProjectId">The id of the project we're searching in</param>
		/// <param name="searchTerm">Any freetext name searching</param>
		/// <remarks>
		/// 1) It will attempt to filter out duplicates
		/// 2) It currently only handles:
		///     Requirements > Test Coverage
		///     Requirements > Associations (Requirements, Incidents)
		///     Incidents > Associations (Requirements, Incidents, Tasks and Test Steps)
		///     Test Sets > Test Cases
		/// </remarks>
		public List<HierarchicalItem> Association_SearchByProjectAndArtifact(int projectId, int artifactTypeId, int artifactId, int searchArtifactTypeId, int? searchFolderId, int searchProjectId, string searchTerm)
		{
			const string METHOD_NAME = "Association_SearchByProjectAndArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//If the artifact type is Test Run Step, check permissions for Test Run instead
			Artifact.ArtifactTypeEnum authorizedArtifactType = (Artifact.ArtifactTypeEnum)artifactTypeId;
			if (authorizedArtifactType == Artifact.ArtifactTypeEnum.TestRunStep)
			{
				authorizedArtifactType = Artifact.ArtifactTypeEnum.TestRun;
			}

			//If this is a source code revision, the authorization depends on what we're linking TO (destination)
			//so allow all roles to make the call, but filter the results
			if (authorizedArtifactType == Artifact.ArtifactTypeEnum.SourceCodeRevision || authorizedArtifactType == Artifact.ArtifactTypeEnum.SourceCodeFile)
			{
				authorizedArtifactType = Artifact.ArtifactTypeEnum.None;
			}

			//Make sure we're authorized for this project, we need to be able to view the artifact type (not limited)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, authorizedArtifactType);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//If we have certain cases, we cannot let them search cross-project, regardless of settings
				//(e.g. Test Set > Test Cases)
				if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.TestSet && searchArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase && projectId != searchProjectId)
				{
					searchProjectId = projectId;
				}
				if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Document && projectId != searchProjectId)
				{
					searchProjectId = projectId;
				}
				if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.SourceCodeRevision && projectId != searchProjectId)
				{
					searchProjectId = projectId;
				}
				if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.SourceCodeFile && projectId != searchProjectId)
				{
					searchProjectId = projectId;
				}
				//Test Execution > Link Existing
				if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.TestRunStep && searchArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident && projectId != searchProjectId)
				{
					searchProjectId = projectId;
				}
				//Incident > Test Step
				if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident && searchArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestStep && projectId != searchProjectId)
				{
					searchProjectId = projectId;
				}

				//See if we have an artifact token search
				int? searchArtifactId = null;
				if (!String.IsNullOrWhiteSpace(searchTerm))
				{
					Regex regex = new Regex(Common.Global.REGEX_ARTIFACT_TOKEN_MATCHER, RegexOptions.IgnoreCase);
					Match artifactMatch = regex.Match(searchTerm);
					if (artifactMatch.Success && artifactMatch.Groups.Count == 3)
					{
						int intValue;
						if (Int32.TryParse(artifactMatch.Groups[2].Value, out intValue))
						{
							searchArtifactId = intValue;
						}
						string artifactPrefix = artifactMatch.Groups[1].Value.Trim();

						//Get the artifact id from the prefix
						ArtifactType artifactType = new ArtifactManager().ArtifactType_RetrieveByPrefix(artifactPrefix);
						if (artifactType != null)
						{
							searchArtifactTypeId = artifactType.ArtifactTypeId;

							//Make sure we're authorized for this project, we need to be able to view the artifact type (not limited)
							Project.AuthorizationState authorizationState2 = IsAuthorized(projectId, Project.PermissionEnum.View, (Artifact.ArtifactTypeEnum)searchArtifactTypeId);
							if (authorizationState2 != Project.AuthorizationState.Authorized)
							{
								//Hide the result
								searchArtifactId = null;
							}
						}
					}
				}

				//See what type of artifact we're searching for
				List<HierarchicalItem> dataItems = new List<HierarchicalItem>();
				switch ((Artifact.ArtifactTypeEnum)searchArtifactTypeId)
				{
					case Artifact.ArtifactTypeEnum.Requirement:
						{
							//See if we're given a specific ID
							List<RequirementView> requirements;
							RequirementManager requirementManager = new RequirementManager();
							if (searchArtifactId.HasValue)
							{
								RequirementView requirement = requirementManager.RetrieveById2(null, searchArtifactId.Value);
								requirements = new List<RequirementView>() { requirement };
								searchProjectId = requirement.ProjectId;
							}
							else
							{
								//Find the list of requirements that match the search term
								//Use the InternalUser since we want to search in items collapsed for the current user
								Hashtable filters = new Hashtable();
								if (!String.IsNullOrEmpty(searchTerm))
								{
									filters.Add("Name", searchTerm);
								}
								if (searchFolderId.HasValue)
								{
									//Add the requirement ID as the filter (gets the children of it)
									filters.Add("RequirementId", searchFolderId.Value);
								}
								requirements = requirementManager.Retrieve(User.UserInternal, searchProjectId, 1, MAX_ROWS, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
							}

							//Return the list
							foreach (RequirementView requirement in requirements)
							{
								//If we're linking FROM the same artifact type, make sure we don't try and self-link
								if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.Requirement || requirement.RequirementId != artifactId)
								{
									HierarchicalItem dataItem = new HierarchicalItem
									{
										ProjectId = searchProjectId,
										ArtifactTypeId = (int)requirement.ArtifactType,
										Id = requirement.RequirementId,
										Name = requirement.Name,
										IndentLevel = requirement.IndentLevel,
										ProjectName = requirement.ProjectName,
										ArtifactSubType = requirement.RequirementTypeIsSteps ? "useCase" : requirement.IsSummary ? "summary" : ""
									};
									dataItems.Add(dataItem);
								}
							}

							//See what we're linking from
							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement || artifactTypeId == (int)Artifact.ArtifactTypeEnum.Risk)
							{
								//See if we have any of these in the existing association list
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								Hashtable filters2 = new Hashtable();
								filters2.Add("ArtifactTypeId", (int)Artifact.ArtifactTypeEnum.Requirement);    //We only want requirements back
								List<ArtifactLinkView> linkedArtifacts = artifactLinkManager.RetrieveByArtifactId((Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, "", true, filters2);
								foreach (ArtifactLinkView linkedArtifact in linkedArtifacts)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == linkedArtifact.ArtifactId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}

							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase)
							{
								//See if we have any of these in the existing requirements coverage
								List<RequirementView> coveringRequirements = requirementManager.RetrieveCoveredByTestCaseId(User.UserInternal, projectId, artifactId);
								foreach (RequirementView coveringRequirement in coveringRequirements)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == coveringRequirement.RequirementId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestCase:
						{
							//See if we're given a specific ID
							List<TestCaseView> testCases;
							TestCaseManager testCaseManager = new TestCaseManager();
							if (searchArtifactId.HasValue)
							{
								TestCaseView testCase = testCaseManager.RetrieveById(null, searchArtifactId.Value);
								searchProjectId = testCase.ProjectId;
								testCases = new List<TestCaseView>() { testCase };

								//If we have certain cases, we cannot let them search cross-project, regardless of settings
								//(e.g. Test Set > Test Cases)
								if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.TestSet && projectId != testCase.ProjectId)
								{
									//Hide the result
									testCases = new List<TestCaseView>();
								}
							}
							else
							{
								//Find the list of test cases that match the search term
								Hashtable filters = new Hashtable();
								if (!String.IsNullOrEmpty(searchTerm))
								{
									filters.Add("Name", searchTerm);
								}
								testCases = testCaseManager.Retrieve(searchProjectId, "Name", true, 1, MAX_ROWS, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset(), searchFolderId);
							}

							//Return the list
							foreach (TestCaseView testCase in testCases)
							{
								//If we're linking FROM the same artifact type, make sure we don't try and self-link
								if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.TestCase || testCase.TestCaseId != artifactId)
								{
									HierarchicalItem dataItem = new HierarchicalItem();
									dataItem.ProjectId = searchProjectId;
									dataItem.ArtifactTypeId = (int)testCase.ArtifactType;
									dataItem.Id = testCase.TestCaseId;
									dataItem.Name = testCase.Name;
									dataItem.ProjectName = testCase.ProjectName;
									dataItems.Add(dataItem);
								}
							}

							//See what we're linking from - requirements
							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement)
							{
								//See if we have any of these in the existing test coverage
								List<TestCase> coveringTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, artifactId);
								foreach (TestCase coveringTestCase in coveringTestCases)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == coveringTestCase.TestCaseId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}

							//See what we're linking from - releases
							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Release)
							{
								//See if we have any of these in the existing test coverage
								List<TestCase> mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, artifactId);
								foreach (TestCase mappedTestCase in mappedTestCases)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == mappedTestCase.TestCaseId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}

							//See what we're linking from - general assocations
							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Risk)
							{
								//See if we have any of these in the existing association list
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								Hashtable filters2 = new Hashtable();
								filters2.Add("ArtifactTypeId", (int)Artifact.ArtifactTypeEnum.TestCase);    //We only want test cases back
								List<ArtifactLinkView> linkedArtifacts = artifactLinkManager.RetrieveByArtifactId((Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, "", true, filters2);
								foreach (ArtifactLinkView linkedArtifact in linkedArtifacts)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == linkedArtifact.ArtifactId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}

							//For test sets you can add the same item multiple times, so no need to edit out
						}
						break;

					case Artifact.ArtifactTypeEnum.TestSet:
						{
							//See if we're given a specific ID
							List<TestSetView> testSets;
							TestSetManager testSetManager = new TestSetManager();
							if (searchArtifactId.HasValue)
							{
								TestSetView testSet = testSetManager.RetrieveById(null, searchArtifactId.Value);
								searchProjectId = testSet.ProjectId;
								testSets = new List<TestSetView>() { testSet };

								//If we have certain cases, we cannot let them search cross-project, regardless of settings
								//(e.g. Test Set > Test Cases)
								if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.TestSet && projectId != testSet.ProjectId)
								{
									//Hide the result
									testSets = new List<TestSetView>();
								}
							}
							else
							{
								//Find the list of test cases that match the search term
								Hashtable filters = new Hashtable();
								if (!String.IsNullOrEmpty(searchTerm))
								{
									filters.Add("Name", searchTerm);
								}
								testSets = testSetManager.Retrieve(searchProjectId, "Name", true, 1, MAX_ROWS, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset(), searchFolderId);
							}

							//Return the list
							foreach (TestSetView testSet in testSets)
							{
								//If we're linking FROM the same artifact type, make sure we don't try and self-link
								if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.TestSet || testSet.TestSetId != artifactId)
								{
									HierarchicalItem dataItem = new HierarchicalItem();
									dataItem.ProjectId = searchProjectId;
									dataItem.ArtifactTypeId = (int)testSet.ArtifactType;
									dataItem.Id = testSet.TestSetId;
									dataItem.Name = testSet.Name;
									dataItem.ProjectName = testSet.ProjectName;
									dataItems.Add(dataItem);
								}
							}

							//For test cases you can add the same item multiple times, so no need to edit out
							//For documents, it's handled after the case statement
						}
						break;

					case Artifact.ArtifactTypeEnum.Task:
						{
							//See if we're given a specific ID
							List<TaskView> tasks;
							TaskManager taskManager = new TaskManager();
							if (searchArtifactId.HasValue)
							{
								TaskView task = taskManager.TaskView_RetrieveById(searchArtifactId.Value);
								searchProjectId = task.ProjectId;
								tasks = new List<TaskView>() { task };
							}
							else
							{
								//Find the list of tasks that match the search term
								Hashtable filters = new Hashtable();
								if (!String.IsNullOrEmpty(searchTerm))
								{
									filters.Add("Name", searchTerm);
								}
								tasks = taskManager.Retrieve(searchProjectId, "Name", true, 1, MAX_ROWS, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset(), searchFolderId);
							}

							//Return the list
							foreach (TaskView task in tasks)
							{
								//If we're linking FROM the same artifact type, make sure we don't try and self-link
								if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.Task || task.TaskId != artifactId)
								{
									HierarchicalItem dataItem = new HierarchicalItem();
									dataItem.ProjectId = searchProjectId;
									dataItem.ArtifactTypeId = (int)task.ArtifactType;
									dataItem.Id = task.TaskId;
									dataItem.Name = task.Name;
									dataItem.ProjectName = task.ProjectName;
									dataItems.Add(dataItem);
								}
							}

							//See what we're linking from
							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident || artifactTypeId == (int)Artifact.ArtifactTypeEnum.Task)
							{
								//See if we have any of these in the existing association list
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								Hashtable filters2 = new Hashtable();
								filters2.Add("ArtifactTypeId", (int)Artifact.ArtifactTypeEnum.Task);    //We only want incidents back
								List<ArtifactLinkView> linkedArtifacts = artifactLinkManager.RetrieveByArtifactId((Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, "", true, filters2);
								foreach (ArtifactLinkView linkedArtifact in linkedArtifacts)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == linkedArtifact.ArtifactId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Incident:
						{
							//See if we're given a specific ID
							List<IncidentView> incidents;
							IncidentManager incidentManager = new IncidentManager();
							if (searchArtifactId.HasValue)
							{
								IncidentView incident = incidentManager.RetrieveById2(searchArtifactId.Value, false);

								//Make sure we're not linking FROM a test run step in another project
								if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.TestRunStep && projectId != incident.ProjectId)
								{
									incidents = new List<IncidentView>();
								}
								else
								{
									incidents = new List<IncidentView>() { incident };
									searchProjectId = incident.ProjectId;
								}
							}
							else
							{
								//Find the list of incidents that match the search term
								Hashtable filters = new Hashtable();
								if (!String.IsNullOrEmpty(searchTerm))
								{
									filters.Add("Name", searchTerm);
								}
								incidents = incidentManager.Retrieve(searchProjectId, "Name", true, 1, MAX_ROWS, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
							}

							//Return the list
							foreach (IncidentView incident in incidents)
							{
								//If we're linking FROM the same artifact type, make sure we don't try and self-link
								if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.Incident || incident.IncidentId != artifactId)
								{
									HierarchicalItem dataItem = new HierarchicalItem();
									dataItem.ProjectId = searchProjectId;
									dataItem.ArtifactTypeId = (int)incident.ArtifactType;
									dataItem.Id = incident.IncidentId;
									dataItem.Name = incident.Name;
									dataItem.ProjectName = incident.ProjectName;
									dataItems.Add(dataItem);
								}
							}

							//See what we're linking from
							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement ||
								artifactTypeId == (int)Artifact.ArtifactTypeEnum.Risk
								)
							{
								//See if we have any of these in the existing association list
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								Hashtable filters2 = new Hashtable();
								filters2.Add("ArtifactTypeId", (int)Artifact.ArtifactTypeEnum.Incident);    //We only want incidents back
								List<ArtifactLinkView> linkedArtifacts = artifactLinkManager.RetrieveByArtifactId((Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, "", true, filters2);
								foreach (ArtifactLinkView linkedArtifact in linkedArtifacts)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == linkedArtifact.ArtifactId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}
							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.TestStep)
							{
								//See if we have any of these in the existing association list
								List<IncidentView> linkedIncidents = incidentManager.RetrieveByTestStepId(artifactId);
								foreach (IncidentView linkedIncident in linkedIncidents)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == linkedIncident.IncidentId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.AutomationHost:
						{
							//See if we're given a specific ID
							List<AutomationHostView> automationHosts;
							AutomationManager automationManager = new AutomationManager();
							if (searchArtifactId.HasValue)
							{
								AutomationHostView automationHost = automationManager.RetrieveHostById(searchArtifactId.Value);
								automationHosts = new List<AutomationHostView>() { automationHost };
								searchProjectId = automationHost.ProjectId;
							}
							else
							{
								//Find the list of incidents that match the search term
								Hashtable filters = new Hashtable();
								if (!String.IsNullOrEmpty(searchTerm))
								{
									filters.Add("Name", searchTerm);
								}
								automationHosts = automationManager.RetrieveHosts(searchProjectId, "Name", true, 1, MAX_ROWS, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
							}

							//Return the list
							foreach (AutomationHostView automationHost in automationHosts)
							{
								//If we're linking FROM the same artifact type, make sure we don't try and self-link
								if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.AutomationHost || automationHost.AutomationHostId != artifactId)
								{
									HierarchicalItem dataItem = new HierarchicalItem();
									dataItem.ProjectId = searchProjectId;
									dataItem.ArtifactTypeId = (int)automationHost.ArtifactType;
									dataItem.Id = automationHost.AutomationHostId;
									dataItem.Name = automationHost.Name;
									dataItem.ProjectName = Project.ARTIFACT_PREFIX + automationHost.ProjectId;
									dataItems.Add(dataItem);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestRun:
						{
							//See if we're given a specific ID
							List<TestRunView> testRuns;
							TestRunManager testRunManager = new TestRunManager();
							if (searchArtifactId.HasValue)
							{
								TestRunView testRun = testRunManager.RetrieveById(searchArtifactId.Value);
								testRuns = new List<TestRunView>() { testRun };
								searchProjectId = testRun.ProjectId;
							}
							else
							{
								//Find the list of incidents that match the search term
								Hashtable filters = new Hashtable();
								if (!String.IsNullOrEmpty(searchTerm))
								{
									filters.Add("Name", searchTerm);
								}
								testRuns = testRunManager.Retrieve(searchProjectId, "EndDate", false, 1, MAX_ROWS, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
							}

							//Return the list
							foreach (TestRunView testRun in testRuns)
							{
								//If we're linking FROM the same artifact type, make sure we don't try and self-link
								if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.TestRun || testRun.TestRunId != artifactId)
								{
									HierarchicalItem dataItem = new HierarchicalItem();
									dataItem.ProjectId = searchProjectId;
									dataItem.ArtifactTypeId = (int)testRun.ArtifactType;
									dataItem.Id = testRun.TestRunId;
									dataItem.Name = testRun.Name + " - " + String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(testRun.EndDate));
									dataItem.ProjectName = Project.ARTIFACT_PREFIX + testRun.ProjectId;
									dataItems.Add(dataItem);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Release:
						{
							//See if we're given a specific ID
							List<ReleaseView> releases;
							ReleaseManager releaseManager = new ReleaseManager();
							if (searchArtifactId.HasValue)
							{
								ReleaseView release = releaseManager.RetrieveById2(null, searchArtifactId.Value);
								releases = new List<ReleaseView>() { release };
								searchProjectId = release.ProjectId;
							}
							else
							{
								//Find the list of incidents that match the search term
								Hashtable filters = new Hashtable();
								if (!String.IsNullOrEmpty(searchTerm))
								{
									filters.Add("FullName", searchTerm);
								}
								releases = releaseManager.RetrieveByProjectId(User.UserInternal, searchProjectId, 1, MAX_ROWS, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
							}

							//Return the list
							foreach (ReleaseView release in releases)
							{
								HierarchicalItem dataItem = new HierarchicalItem
								{
									ProjectId = searchProjectId,
									ArtifactTypeId = (int)release.ArtifactType,
									ArtifactSubType = release.IsIterationOrPhase ? "iterationOrPhase" : "",
									Id = release.ReleaseId,
									Name = release.FullName,
									IndentLevel = release.IndentLevel,
									ProjectName = Project.ARTIFACT_PREFIX + release.ProjectId
								};
								dataItems.Add(dataItem);
							}

							//See what we're linking from
							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase)
							{
								//See if we have any of these in the existing test case mapping
								List<ReleaseView> mappedReleases = releaseManager.RetrieveMappedByTestCaseId(User.UserInternal, projectId, artifactId);
								foreach (ReleaseView mappedRelease in mappedReleases)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == mappedRelease.ReleaseId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestStep:
						{
							//See if we're given a specific ID
							TestCaseManager testCaseManager = new TestCaseManager();
							if (searchArtifactId.HasValue)
							{
								TestStep testStep = testCaseManager.RetrieveStepById(null, searchArtifactId.Value);
								if (testStep == null || testStep.TestCase.ProjectId != searchProjectId)
								{
									//The token search didn't match or the projects don't match (not allowed cross-project links), return nothing
									return new List<HierarchicalItem>();
								}
								searchProjectId = testStep.TestCase.ProjectId;
								Project project = new ProjectManager().RetrieveById(searchProjectId);
								string testStepDescription = GlobalFunctions.HtmlRenderAsPlainText(testStep.Description);

								//First we need to add the test case
								HierarchicalItem dataItem = new HierarchicalItem();
								dataItem.ProjectId = searchProjectId;
								dataItem.ArtifactTypeId = (int)testStep.TestCase.ArtifactType;
								dataItem.IndentLevel = "AAA";
								//Negative IDs denote that it is a test case not a test step
								dataItem.Id = -testStep.TestCase.TestCaseId;
								dataItem.Name = testStep.TestCase.Name;
								dataItem.ProjectName = project.Name;
								dataItems.Add(dataItem);

								//Next add the test step
								dataItem = new HierarchicalItem();
								dataItem.ProjectId = searchProjectId;
								dataItem.ArtifactTypeId = (int)testStep.ArtifactType;
								dataItem.IndentLevel = "AAAAAA";
								dataItem.Id = testStep.TestStepId;
								dataItem.Name = Resources.Fields.Step + " " + testStep.Position + ": " + testStepDescription.Substring(0, Math.Min(testStepDescription.Length, 64));
								dataItem.ProjectName = project.Name;
								dataItems.Add(dataItem);
							}
							else
							{
								//Find the list of test cases that match the search term
								//We will then show those plus their test steps
								Hashtable filters = new Hashtable();
								if (!String.IsNullOrEmpty(searchTerm))
								{
									filters.Add("Name", searchTerm);
								}
								List<TestCase> testCasesWithSteps = new List<TestCase>();
								List<TestCaseView> testCases = testCaseManager.Retrieve(searchProjectId, "Name", true, 1, MAX_ROWS, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset(), searchFolderId);
								foreach (TestCaseView testCaseView in testCases)
								{
									TestCase testCase = testCaseManager.RetrieveByIdWithSteps(null, testCaseView.TestCaseId);
									if (testCase != null && testCase.TestSteps.Count > 0)
									{
										testCasesWithSteps.Add(testCase);
									}
								}
								Project project = new ProjectManager().RetrieveById(searchProjectId);

								//Return the list
								string tcIndentLevel = "AAA";
								foreach (TestCase testCase in testCasesWithSteps)
								{
									HierarchicalItem dataItem = new HierarchicalItem();
									dataItem.ProjectId = searchProjectId;
									dataItem.ArtifactTypeId = (int)testCase.ArtifactType;
									dataItem.IndentLevel = tcIndentLevel;
									//Negative IDs denote that it is a test case not a test step
									dataItem.Id = -testCase.TestCaseId;
									dataItem.Name = testCase.Name;
									dataItem.ProjectName = project.Name;
									dataItems.Add(dataItem);
									tcIndentLevel = HierarchicalList.IncrementIndentLevel(tcIndentLevel);

									//Now the test steps
									string tsIndentLevel = tcIndentLevel + "AAA";
									foreach (TestStep testStep in testCase.TestSteps)
									{
										//If we're linking FROM the same artifact type, make sure we don't try and self-link
										if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.TestCase || testCase.TestCaseId != artifactId)
										{
											dataItem = new HierarchicalItem();
											dataItem.ProjectId = searchProjectId;
											dataItem.ArtifactTypeId = (int)testStep.ArtifactType;
											dataItem.IndentLevel = tsIndentLevel;
											dataItem.Id = testStep.TestStepId;
											if (testStep.LinkedTestCaseId.HasValue)
											{
												//test Link
												dataItem.Name = Resources.Fields.Step + " " + testStep.Position + ": " + testStep.Description + " " + testStep.LinkedTestCase.Name;
											}
											else
											{
												//Test Step
												dataItem.Name = Resources.Fields.Step + " " + testStep.Position + ": " + GlobalFunctions.HtmlRenderAsPlainText(testStep.Description);
											}
											dataItem.ProjectName = project.Name;
											dataItems.Add(dataItem);
											tsIndentLevel = HierarchicalList.IncrementIndentLevel(tsIndentLevel);
										}
									}
								}
							}

							//See what we're linking from
							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement)
							{
								//See if we have any of these in the existing test coverage
								List<TestCase> coveringTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, artifactId);
								foreach (TestCase coveringTestCase in coveringTestCases)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == coveringTestCase.TestCaseId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Risk:
						{
							//See if we're given a specific ID
							List<RiskView> risks;
							RiskManager riskManager = new RiskManager();
							if (searchArtifactId.HasValue)
							{
								RiskView risk = riskManager.Risk_RetrieveById2(searchArtifactId.Value);
								searchProjectId = risk.ProjectId;
								risks = new List<RiskView>() { risk };
							}
							else
							{
								//Find the list of tasks that match the search term
								Hashtable filters = new Hashtable();
								if (!String.IsNullOrEmpty(searchTerm))
								{
									filters.Add("Name", searchTerm);
								}
								risks = riskManager.Risk_Retrieve(searchProjectId, "Name", true, 1, MAX_ROWS, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
							}

							//Return the list
							foreach (RiskView risk in risks)
							{
								//If we're linking FROM the same artifact type, make sure we don't try and self-link
								if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.Risk || risk.RiskId != artifactId)
								{
									HierarchicalItem dataItem = new HierarchicalItem();
									dataItem.ProjectId = searchProjectId;
									dataItem.ArtifactTypeId = (int)risk.ArtifactType;
									dataItem.Id = risk.RiskId;
									dataItem.Name = risk.Name;
									dataItem.ProjectName = risk.ProjectName;
									dataItems.Add(dataItem);
								}
							}

							//See what we're linking from
							if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident ||
								artifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase ||
								artifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement ||
								artifactTypeId == (int)Artifact.ArtifactTypeEnum.Risk
								)
							{
								//See if we have any of these in the existing association list
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								Hashtable filters2 = new Hashtable();
								filters2.Add("ArtifactTypeId", (int)Artifact.ArtifactTypeEnum.Risk);    //We only want risks back
								List<ArtifactLinkView> linkedArtifacts = artifactLinkManager.RetrieveByArtifactId((Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, "", true, filters2);
								foreach (ArtifactLinkView linkedArtifact in linkedArtifacts)
								{
									HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == linkedArtifact.ArtifactId);
									if (matchedItem != null)
									{
										dataItems.Remove(matchedItem);
									}
								}
							}
						}
						break;
				}

				//For all source artifact types. if we're linking FROM a document, remove any existing associations
				if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Document)
				{
					//See if we have any of these in the existing association list
					ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
					List<ArtifactAttachmentView> linkedArtifacts = artifactLinkManager.RetrieveByAttachmentId(projectId, artifactId);
					foreach (ArtifactAttachmentView linkedArtifact in linkedArtifacts)
					{
						HierarchicalItem matchedItem = dataItems.FirstOrDefault(d => d.Id == linkedArtifact.ArtifactId && d.ArtifactTypeId == linkedArtifact.ArtifactTypeId);
						if (matchedItem != null)
						{
							dataItems.Remove(matchedItem);
						}
					}
				}

				//Make sure the searched project allows us to return these artifacts
				ProjectManager projectManager = new ProjectManager();
				bool canShare = projectManager.ProjectAssociation_CanProjectShare(projectId, searchProjectId, (Artifact.ArtifactTypeEnum)searchArtifactTypeId);
				if (!canShare)
				{
					Logger.LogFailureAuditEvent(CLASS_NAME + METHOD_NAME, "Attempting to access a project that has not been shared (" + searchProjectId + ")");
					throw new DataValidationException(Resources.Messages.AssociationService_UnableToFindArtifactInSharedProjects);
				}

				return dataItems;
			}
			catch (ArtifactNotExistsException)
			{
				//The token search didn't match, return nothing
				return new List<HierarchicalItem>();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Counts the number of associations
		/// </summary>
		/// <param name="projectId">The project id</param>
		/// <param name="artifact">The artifact we want the associations for</param>
		/// <param name="displayTypeId">The type of association panel being used</param>
		/// <returns>The count</returns>
		public int Association_Count(int projectId, ArtifactReference artifact, int displayTypeId)
		{
			const string METHOD_NAME = "Association_Count";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (builds don't have their own permssion, they use release)
			//Limited OK because we need to display the 'has data' in tabs
			Artifact.ArtifactTypeEnum authorizedArtifact = (Artifact.ArtifactTypeEnum)artifact.ArtifactTypeId;
			if (authorizedArtifact == Artifact.ArtifactTypeEnum.Build)
			{
				authorizedArtifact = Artifact.ArtifactTypeEnum.Release;
			}
			//If this is a source code revision or file, the authorization depends on what we're linking TO (destination)
			//so allow all roles to make the call, but filter the results
			if (authorizedArtifact == Artifact.ArtifactTypeEnum.SourceCodeRevision || authorizedArtifact == Artifact.ArtifactTypeEnum.SourceCodeFile)
			{
				authorizedArtifact = Artifact.ArtifactTypeEnum.None;
			}
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, authorizedArtifact);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the correct association type so that we can get the count for the 'has data flag'
				int count = 0;
				Artifact.DisplayTypeEnum displayType = (Artifact.DisplayTypeEnum)displayTypeId;
				Artifact.ArtifactTypeEnum artifactType = (Artifact.ArtifactTypeEnum)artifact.ArtifactTypeId;
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Requirement && displayType == Artifact.DisplayTypeEnum.Requirement_TestCases)
				{
					//Set the has-data property if appropriate - once set true, don't unset
					TestCaseManager testCaseManager = new TestCaseManager();
					count = testCaseManager.CountCoveredByRequirementId(projectId, artifact.ArtifactId);
				}
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Release && displayType == Artifact.DisplayTypeEnum.Release_TestCases)
				{
					//Set the has-data property if appropriate - once set true, don't unset
					TestCaseManager testCaseManager = new TestCaseManager();
					count = testCaseManager.CountByRelease(projectId, artifact.ArtifactId, null, 0, null, false, true);
				}
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestStep && displayType == Artifact.DisplayTypeEnum.TestStep_Requirements)
				{
					//Set the has-data property if appropriate - once set true, don't unset
					RequirementManager requirementManager = new RequirementManager();
					List<RequirementView> coveredRequirements = requirementManager.RequirementTestStep_RetrieveByTestStepId(User.UserInternal, projectId, artifact.ArtifactId);
					count = coveredRequirements.Count;
				}
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestCase && displayType == Artifact.DisplayTypeEnum.TestCase_Requirements)
				{
					//Set the has-data property if appropriate - once set true, don't unset
					RequirementManager requirementManager = new RequirementManager();
					List<RequirementView> coveredRequirements = requirementManager.RetrieveCoveredByTestCaseId(Business.UserManager.UserInternal, projectId, artifact.ArtifactId);
					count = coveredRequirements.Count;
				}
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestCase && displayType == Artifact.DisplayTypeEnum.TestCase_Releases)
				{
					//Set the has-data property if appropriate - once set true, don't unset
					ReleaseManager releaseManager = new ReleaseManager();
					List<ReleaseView> mappedReleases = releaseManager.RetrieveMappedByTestCaseId(Business.UserManager.UserInternal, projectId, artifact.ArtifactId);
					count = mappedReleases.Count;
				}

				if (displayType == Artifact.DisplayTypeEnum.Attachments)
				{
					ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
					count = artifactLinkManager.CountByAttachmentId(projectId, artifact.ArtifactId);
				}

				if (displayType == Artifact.DisplayTypeEnum.Build_Associations && artifact.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Build)
				{
					//See if we have any artifact associations for the build
					try
					{
						SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
						List<ArtifactLinkView> artifactLinks = sourceCodeManager.RetrieveAssociationsForBuild(projectId, artifact.ArtifactId);
						return artifactLinks.Count;
					}
					catch (SourceCodeProviderLoadingException exception)
					{
						//Display a friendly message
						throw new DataValidationException(exception.Message);
					}
					catch (SourceCodeProviderArtifactPermissionDeniedException)
					{
						//If the user doesn't have permission to view, just ignore since data won't be loaded
					}
					catch (SourceCodeProviderGeneralException exception)
					{
						//Display a friendly message
						throw new DataValidationException(exception.Message);
					}
				}

				if (displayType == Artifact.DisplayTypeEnum.SourceCodeRevision_Associations && artifact.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.SourceCodeRevision)
				{
					//See if we have any artifact associations for the revision
					try
					{
						SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
						SourceCodeCommit sourceCodeCommit = sourceCodeManager.RetrieveRevisionById(artifact.ArtifactId);
						List<ArtifactLinkView> artifactLinks = sourceCodeManager.RetrieveAssociationsForRevision(sourceCodeCommit.Revisionkey);
						return artifactLinks.Count;
					}
					catch (SourceCodeProviderLoadingException exception)
					{
						//Display a friendly message
						throw new DataValidationException(exception.Message);
					}
					catch (SourceCodeProviderArtifactPermissionDeniedException)
					{
						//If the user doesn't have permission to view, just ignore since data won't be loaded
					}
					catch (SourceCodeProviderGeneralException exception)
					{
						//Display a friendly message
						throw new DataValidationException(exception.Message);
					}
				}

				if (displayType == Artifact.DisplayTypeEnum.SourceCodeFile_Associations && artifact.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.SourceCodeFile)
				{
					//See if we have any artifact associations for the file
					try
					{
						string branchKey;
						SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
						SourceCodeFile sourceCodeFile = sourceCodeManager.RetrieveFileById(artifact.ArtifactId, out branchKey);
						List<ArtifactLinkView> artifactLinks = sourceCodeManager.RetrieveAssociationsForFile(sourceCodeFile.FileKey, branchKey);
						return artifactLinks.Count;
					}
					catch (SourceCodeProviderLoadingException exception)
					{
						//Display a friendly message
						throw new DataValidationException(exception.Message);
					}
					catch (SourceCodeProviderArtifactPermissionDeniedException)
					{
						//If the user doesn't have permission to view, just ignore since data won't be loaded
					}
					catch (SourceCodeProviderGeneralException exception)
					{
						//Display a friendly message
						throw new DataValidationException(exception.Message);
					}
				}

				if (displayType == Artifact.DisplayTypeEnum.ArtifactLink)
				{
					//See if we have any artifact links so that we know to display the 'has-data' flag
					ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
					List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(artifactType, artifact.ArtifactId);
					count = artifactLinks.Count;

					//Check that we are to include source code revisions
					if (Common.Global.SourceCode_IncludeInAssociationsAndDocuments)
					{
						//See if we have any source code links
						try
						{
							SourceCodeManager sourceCodeManager = new SourceCodeManager();
							if (sourceCodeManager.RetrieveProjectSettings(projectId).Count > 0)
							{
								//Reload with the project specified, loads the provider
								sourceCodeManager = new SourceCodeManager(projectId);
								List<SourceCodeCommit> sourceCodeRevisions = sourceCodeManager.RetrieveRevisionsForArtifact(artifactType, artifact.ArtifactId);
								count += sourceCodeRevisions.Count;
							}
						}
						catch (SourceCodeProviderLoadingException exception)
						{
							//Log but don't throw/display
							Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, exception.Message);
						}
						catch (Exception exception)
						{
							//Log quietly
							Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						}
					}
				}

				return count;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds the new association relationship
		/// </summary>
		/// <param name="projectId">The id of the current project (not the one linked to)</param>
		/// <param name="artifactTypeId">The type of the artifact we're currently on</param>
		/// <param name="artifactId">The id of the artifact we're currently on</param>
		/// <param name="displayType">The type of panel we're using</param>
		/// <param name="artifactLinkTypeId">The type of link (related-to, dependent-on, etc.)</param>
		/// <param name="comment">The association comment</param>
		/// <param name="selectionAssociations">The list of artifacts/projects we're linking to</param>
		/// <param name="existingItemId">The id of an existing item we want to insert before (or null if none specified)</param>
		public void Association_Add(int projectId, int artifactTypeId, int artifactId, int displayType, int artifactLinkTypeId, string comment, List<ArtifactReference> selectionAssociations, int? existingItemId)
		{
			const string METHOD_NAME = "Association_Add";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized for this project, we need to be able to modify the artifact type
			//Limited view is OK as long as we own the 'source' artifact
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, (Artifact.ArtifactTypeEnum)artifactTypeId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				//If this is a source code revision, the authorization depends on what we're linking TO (destination)
				//so allow all roles to make the call, but filter the results
				if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.SourceCodeRevision && artifactTypeId != (int)Artifact.ArtifactTypeEnum.SourceCodeFile)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}
			}

			//Make sure we have at least one association
			if (selectionAssociations == null || selectionAssociations.Count < 1)
			{
				return;
			}

			try
			{
				//Make sure we own/created it if we have Limited Modify permissions
				if (authorizationState == Project.AuthorizationState.Limited)
				{
					ArtifactInfo artifactInfo = new ArtifactManager().RetrieveArtifactInfo((Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, projectId);
					if (artifactInfo == null || (artifactInfo.OwnerId != userId && artifactInfo.CreatorId != userId))
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}
				}

				//See what type of artifact we're associated FROM
				List<HierarchicalItem> dataItems = new List<HierarchicalItem>();
				switch ((Artifact.ArtifactTypeEnum)artifactTypeId)
				{
					case Artifact.ArtifactTypeEnum.Requirement:
						{
							//Select the test coverage ones separately
							List<int> testCaseIds = selectionAssociations.Where(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase).Select(p => p.ArtifactId).ToList();
							List<ArtifactReference> associations = selectionAssociations.Where(p =>
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement ||
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident ||
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Risk
							).ToList();

							//Add the test coverage items
							if (testCaseIds.Count > 0)
							{
								//Requirements > Test Coverage
								TestCaseManager testCaseManager = new TestCaseManager();
								testCaseManager.AddToRequirement(projectId, artifactId, testCaseIds, userId);
							}

							//Loop through the different associations and add the different types
							if (associations.Count > 0)
							{
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								foreach (ArtifactReference association in associations)
								{
									//Requirements > Requirements associations
									//Requirements > Incident associations
									artifactLinkManager.Insert(projectId, (Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, (Artifact.ArtifactTypeEnum)association.ArtifactTypeId, association.ArtifactId, userId, comment, DateTime.UtcNow, (ArtifactLink.ArtifactLinkTypeEnum)artifactLinkTypeId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Release:
						{
							//Select the test mapping ones separately
							List<int> testCaseIds = selectionAssociations.Where(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase).Select(p => p.ArtifactId).ToList();

							//Add the test coverage items
							if (testCaseIds.Count > 0)
							{
								//Release > Test Mapping
								TestCaseManager testCaseManager = new TestCaseManager();
								testCaseManager.AddToRelease(projectId, artifactId, testCaseIds, userId);
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestCase:
						{
							//Select the requirements and release coverage ones separately
							List<int> requirementIds = selectionAssociations.Where(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement).Select(p => p.ArtifactId).ToList();
							List<int> releaseIds = selectionAssociations.Where(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Release).Select(p => p.ArtifactId).ToList();
							List<ArtifactReference> associations = selectionAssociations.Where(p =>
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task ||
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Risk
							).ToList();

							if (requirementIds.Count > 0)
							{
								//Test Case > Requirements Coverage
								RequirementManager requirementManager = new RequirementManager();

								//Add the mappings
								requirementManager.AddToTestCase(projectId, artifactId, requirementIds, userId);
							}

							if (releaseIds.Count > 0)
							{
								//Test Case > Release Mapping
								ReleaseManager releaseManager = new ReleaseManager();

								//Add the mappings
								releaseManager.AddToTestCase(projectId, artifactId, releaseIds, userId);
							}

							if (associations.Count > 0)
							{
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								foreach (ArtifactReference association in associations)
								{
									//Test case > general associations
									artifactLinkManager.Insert(projectId, (Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, (Artifact.ArtifactTypeEnum)association.ArtifactTypeId, association.ArtifactId, userId, comment, DateTime.UtcNow, (ArtifactLink.ArtifactLinkTypeEnum)artifactLinkTypeId);
								}
							}

						}
						break;

					case Artifact.ArtifactTypeEnum.TestStep:
						{
							//Select the requirements and incident ones separately
							List<int> requirementIds = selectionAssociations.Where(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement).Select(p => p.ArtifactId).ToList();
							List<int> incidentIds = selectionAssociations.Where(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident).Select(p => p.ArtifactId).ToList();

							//Add the requirements coverage items
							if (requirementIds.Count > 0)
							{
								//Test Step > Requirements Coverage
								RequirementManager requirementManager = new RequirementManager();
								requirementManager.RequirementTestStep_AddToTestStep(projectId, userId, artifactId, requirementIds, false);
							}

							//Add the incident-test step links
							if (incidentIds.Count > 0)
							{
								//Test Step > Incident links
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								foreach (int incidentId in incidentIds)
								{
									artifactLinkManager.Insert(projectId, Artifact.ArtifactTypeEnum.TestStep, artifactId, Artifact.ArtifactTypeEnum.Incident, incidentId, userId, comment, DateTime.UtcNow);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestSet:
						{
							//Select the test case coverage ones separately
							List<int> testCaseIds = selectionAssociations.Where(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase).Select(p => p.ArtifactId).ToList();

							//Add the test cases to the set
							if (testCaseIds.Count > 0)
							{
								//Test Set > Test Cases
								TestSetManager testSetManager = new TestSetManager();
								testSetManager.AddTestCases(projectId, artifactId, testCaseIds, null, existingItemId);
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Task:
						{
							//We can link to either tasks or incidents, so select those ones
							List<ArtifactReference> associations = selectionAssociations.Where(p =>
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task ||
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident
							).ToList();

							//Loop through the different associations and add the different types
							if (associations.Count > 0)
							{
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								foreach (ArtifactReference association in associations)
								{
									artifactLinkManager.Insert(projectId, (Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, (Artifact.ArtifactTypeEnum)association.ArtifactTypeId, association.ArtifactId, userId, comment, DateTime.UtcNow, (ArtifactLink.ArtifactLinkTypeEnum)artifactLinkTypeId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Incident:
						{
							//Select the test step ones separately
							List<ArtifactReference> testStepAssociations = selectionAssociations.Where(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestStep).ToList();
							List<ArtifactReference> associations = selectionAssociations.Where(p =>
								p.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.TestStep ||
								p.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.Risk
							).ToList();

							//If we are adding a link for Test Step > Incident, the test step must always
							//be the source artifact as it's not a truly bidirectional link (unlike the others)
							if (testStepAssociations.Count > 0)
							{
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								foreach (ArtifactReference association in testStepAssociations)
								{
									//Incident > Test Step associations
									artifactLinkManager.Insert(projectId, (Artifact.ArtifactTypeEnum)association.ArtifactTypeId, association.ArtifactId, (Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, userId, comment, DateTime.UtcNow, (ArtifactLink.ArtifactLinkTypeEnum)artifactLinkTypeId);
								}
							}

							//Loop through the different associations and add the different types
							if (associations.Count > 0)
							{
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								foreach (ArtifactReference association in associations)
								{
									//Incident > Non test step associations
									artifactLinkManager.Insert(projectId, (Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, (Artifact.ArtifactTypeEnum)association.ArtifactTypeId, association.ArtifactId, userId, comment, DateTime.UtcNow, (ArtifactLink.ArtifactLinkTypeEnum)artifactLinkTypeId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Document:
						{
							//Get the attachment id
							int attachmentId = artifactId;

							//Add the attachment associations
							AttachmentManager attachmentManager = new AttachmentManager();
							foreach (ArtifactReference artifactReference in selectionAssociations)
							{
								try
								{
									attachmentManager.InsertArtifactAssociation(projectId, attachmentId, artifactReference.ArtifactId, (DataModel.Artifact.ArtifactTypeEnum)artifactReference.ArtifactTypeId);
								}
								catch (ArtifactLinkDestNotFoundException)
								{
									throw new DataValidationException(Resources.Messages.DocumentDetails_ArtifactNotExists);
								}
								catch (EntityConstraintViolationException)
								{
									throw new DataValidationException(Resources.Messages.DocumentDetails_ArtifactAlreadyAttached);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Risk:
						{
							//We can link to requirements, risks, incidents, test cases, so select those ones
							List<ArtifactReference> associations = selectionAssociations.Where(p =>
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Risk ||
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident ||
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement ||
								p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase
							).ToList();

							//Loop through the different associations and add the different types
							if (associations.Count > 0)
							{
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								foreach (ArtifactReference association in associations)
								{
									artifactLinkManager.Insert(projectId, (Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, (Artifact.ArtifactTypeEnum)association.ArtifactTypeId, association.ArtifactId, userId, comment, DateTime.UtcNow, (ArtifactLink.ArtifactLinkTypeEnum)artifactLinkTypeId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.SourceCodeRevision:
						{
							//Get the source code revision id
							int revisionId = artifactId;

							//Add the revision associations
							try
							{
								SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
								SourceCodeCommit sourceCodeCommit = sourceCodeManager.RetrieveRevisionById(revisionId);
								if (sourceCodeCommit != null)
								{
									foreach (ArtifactReference artifactReference in selectionAssociations)
									{
										try
										{
											sourceCodeManager.AddRevisionAssociation(projectId, sourceCodeCommit.Revisionkey, (DataModel.Artifact.ArtifactTypeEnum)artifactReference.ArtifactTypeId, artifactReference.ArtifactId, DateTime.UtcNow, comment);
										}
										catch (ArtifactLinkDestNotFoundException)
										{
											throw new DataValidationException(Resources.Messages.DocumentDetails_ArtifactNotExists);
										}
										catch (EntityConstraintViolationException)
										{
											throw new DataValidationException(Resources.Messages.DocumentDetails_ArtifactAlreadyAttached);
										}
									}
								}
							}
							catch (SourceCodeProviderLoadingException exception)
							{
								Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
								throw;
							}
							catch (SourceCodeProviderGeneralException exception)
							{
								Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
								throw;
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.SourceCodeFile:
						{
							//Get the source code file id
							int fileId = artifactId;

							//Add the file associations
							try
							{
								SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
								string branchKey;
								SourceCodeFile sourceCodeFile = sourceCodeManager.RetrieveFileById(fileId, out branchKey);
								if (sourceCodeFile != null)
								{
									foreach (ArtifactReference artifactReference in selectionAssociations)
									{
										try
										{
											sourceCodeManager.AddFileAssociation(projectId, sourceCodeFile.FileKey, (DataModel.Artifact.ArtifactTypeEnum)artifactReference.ArtifactTypeId, artifactReference.ArtifactId, DateTime.UtcNow, branchKey, comment);
										}
										catch (ArtifactLinkDestNotFoundException)
										{
											throw new DataValidationException(Resources.Messages.DocumentDetails_ArtifactNotExists);
										}
										catch (EntityConstraintViolationException)
										{
											throw new DataValidationException(Resources.Messages.DocumentDetails_ArtifactAlreadyAttached);
										}
									}
								}
							}
							catch (SourceCodeProviderLoadingException exception)
							{
								Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
								throw;
							}
							catch (SourceCodeProviderGeneralException exception)
							{
								Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
								throw;
							}
						}
						break;
				}
			}
			catch (ArtifactLinkDestNotFoundException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (EntityConstraintViolationException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region ISortedListService methods

		/// <summary>
		/// Updates the comments for the artifact links in the system
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="dataItems">The updated data records</param>
		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			if (!displayTypeId.HasValue)
			{
				throw new ArgumentException("You need to specify a display type");
			}
			DataModel.Artifact.DisplayTypeEnum displayType = (DataModel.Artifact.DisplayTypeEnum)displayTypeId.Value;

			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			try
			{
				//Iterate through each data item and make the updates
				ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
				foreach (SortedDataItem dataItem in dataItems)
				{
					//Get the artifact link id
					int artifactLinkId = dataItem.PrimaryKey;

					//See what type of association this is
					if (displayType == Artifact.DisplayTypeEnum.SourceCodeRevision_Associations)
					{
						SourceCodeManager sourceCodeManager = new SourceCodeManager();
						ArtifactLink artifactLink = sourceCodeManager.RetrieveRevisionAssociation(projectId, artifactLinkId);
						if (artifactLink != null && dataItem.Fields.ContainsKey("Comment"))
						{
							//Update the comment
							string comment = dataItem.Fields["Comment"].TextValue;
							sourceCodeManager.UpdateRevisionAssociation(projectId, artifactLinkId, comment);
						}
					}
					else if (displayType == Artifact.DisplayTypeEnum.SourceCodeFile_Associations)
					{
						SourceCodeManager sourceCodeManager = new SourceCodeManager();
						ArtifactLink artifactLink = sourceCodeManager.RetrieveFileAssociation(projectId, artifactLinkId);
						if (artifactLink != null && dataItem.Fields.ContainsKey("Comment"))
						{
							//Update the comment
							string comment = dataItem.Fields["Comment"].TextValue;
							sourceCodeManager.UpdateFileAssociation(projectId, artifactLinkId, comment);
						}
					}
					else
					{
						//Retrieve the existing record - and make sure it still exists
						ArtifactLink artifactLink = artifactLinkManager.RetrieveById(artifactLinkId);
						if (artifactLink != null)
						{
							//Update the field values
							artifactLink.StartTracking();
							UpdateFields(validationMessages, dataItem, artifactLink, null, null, projectId, artifactLinkId, DataModel.Artifact.ArtifactTypeEnum.None);

							//Check to see if we have any validation messages
							if (validationMessages.Count == 0)
							{
								//Persist to database, catching any business exceptions and displaying them
								artifactLinkManager.Update(artifactLink, userId);
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return validationMessages;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of artifact links in the system for the specific artifact
		/// </summary>
		/// <param name="userId">The user we're viewing the documents as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">
		/// Contains required ArtifactType and ArtifactId filters
		/// </param>
		/// <returns>Collection of dataitems</returns>
		public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that they've specified the artifact id and type in the standard filters
			if (standardFilters == null)
			{
				throw new ArgumentException("You need to specify an artifact id and artifact type");
			}
			if (!standardFilters.ContainsKey("ArtifactId"))
			{
				throw new ArgumentException("You need to specify an artifact id");
			}
			if (!standardFilters.ContainsKey("ArtifactType"))
			{
				throw new ArgumentException("You need to specify an artifact type");
			}
			if (!displayTypeId.HasValue)
			{
				throw new ArgumentException("You need to specify a display type");
			}

			//Get the artifact type, id, and association panel type from the filters
			int artifactTypeId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ArtifactType"]);
			DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;
			int artifactId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ArtifactId"]);
			DataModel.Artifact.DisplayTypeEnum displayType = (DataModel.Artifact.DisplayTypeEnum)displayTypeId.Value;

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (builds don't have their own permssion, they use release)
			Artifact.ArtifactTypeEnum authorizedArtifact = artifactType;
			if (authorizedArtifact == Artifact.ArtifactTypeEnum.Build)
			{
				authorizedArtifact = Artifact.ArtifactTypeEnum.Release;
			}
			//If this is a source code revision, the authorization depends on what we're linking TO (destination)
			//so allow all roles to make the call, but filter the results
			if (authorizedArtifact == Artifact.ArtifactTypeEnum.SourceCodeRevision || authorizedArtifact == Artifact.ArtifactTypeEnum.SourceCodeFile)
			{
				authorizedArtifact = Artifact.ArtifactTypeEnum.None;
			}
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, authorizedArtifact);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Create the array of data items (including the first filter item)
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

				//Now get the list of populated filters and the current sort
				Hashtable filterList = new Hashtable();
				string sortCommand = null;

				//set the filterList and sortCommand based on the display type we are on
				string projectSetting_Filters;
				string projectSetting_Sort;
				string defaultSortExpression;
				GetSettingsCollections(displayType, out projectSetting_Filters, out projectSetting_Sort, out defaultSortExpression);
				filterList = GetProjectSettings(userId, projectId, projectSetting_Filters);
				sortCommand = GetProjectSetting(userId, projectId, projectSetting_Sort, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, defaultSortExpression);

				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");
				sortedData.FilterNames = GetFilterNames(filterList);

				//Create the filter item first - we can clone it later
				SortedDataItem filterItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, displayType, filterItem, filterList);
				dataItems.Add(filterItem);

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_ARTIFACT_LINKS_PAGINATION);
				paginationSettings.Restore();
				//Default values
				int paginationSize = 15;
				int currentPage = 1;
				if (paginationSettings["NumberRowsPerPage"] != null)
				{
					paginationSize = (int)paginationSettings["NumberRowsPerPage"];
				}
				if (paginationSettings["CurrentPage"] != null)
				{
					currentPage = (int)paginationSettings["CurrentPage"];
				}

				//Instantiate the relevant business object
				switch (displayType)
				{
					case Artifact.DisplayTypeEnum.None:
					case Artifact.DisplayTypeEnum.ArtifactLink:
						sortedData = RetrieveArtifactLinks(projectId, userId, artifactType, artifactId, sortedData, dataItems, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
						break;

					case Artifact.DisplayTypeEnum.Requirement_TestCases:
					case Artifact.DisplayTypeEnum.Release_TestCases:
						sortedData = RetrieveTestCaseCoverage(projectId, artifactType, artifactId, sortedData, dataItems, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
						break;

					case Artifact.DisplayTypeEnum.TestCase_Requirements:
					case Artifact.DisplayTypeEnum.TestStep_Requirements:
						sortedData = RetrieveRequirementCoverage(projectId, artifactType, artifactId, sortedData, dataItems, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
						break;

					case Artifact.DisplayTypeEnum.TestCase_Releases:
						sortedData = RetrieveReleaseMapping(projectId, artifactType, artifactId, sortedData, dataItems, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
						break;

					case Artifact.DisplayTypeEnum.Attachments:
						sortedData = RetrieveAttachmentArtifacts(projectId, artifactId, sortedData, dataItems, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
						break;

					case Artifact.DisplayTypeEnum.Build_Associations:
						sortedData = RetrieveBuildAssociations(userId, projectId, artifactId, sortedData, dataItems, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
						break;

					case Artifact.DisplayTypeEnum.SourceCodeRevision_Associations:
						sortedData = RetrieveSourceCodeRevisionAssociations(userId, projectId, artifactId, sortedData, dataItems, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
						break;

					case Artifact.DisplayTypeEnum.SourceCodeFile_Associations:
						sortedData = RetrieveSourceCodeFileAssociations(userId, projectId, artifactId, sortedData, dataItems, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
						break;

					default:
						break;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return sortedData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of build artifact associations
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="userId">The current user</param>
		/// <param name="buildId">The id of the current build</param>
		/// <param name="sortedData">Partially processed SortedData </param>
		/// <param name="dataItems">Partially processed dataItems list - used to populate the sortedData </param>
		/// <param name="sortProperty">Sorting of the data </param>
		/// <param name="sortAscending">Whether data should be sorted in ascending order </param>
		/// <param name="paginationSettings">Pagination settings </param>
		/// <param name="paginationSize">How many items should be shown on a page </param>
		/// <param name="currentPage">The current page to collect data for </param>
		/// <param name="filterList">Information about filters</param>
		/// <param name="filterItem">Filter item </param>
		/// <returns>SortData, fully processed, of linked artifacts</returns> 
		protected SortedData RetrieveBuildAssociations(int userId, int projectId, int buildId, SortedData sortedData, List<SortedDataItem> dataItems, string sortProperty, bool sortAscending, ProjectSettingsCollection paginationSettings, int paginationSize, int currentPage, Hashtable filterList, SortedDataItem filterItem)
		{
			//See if we have any artifact associations for the build
			try
			{
				SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
				List<ArtifactLinkView> buildAssociations = sourceCodeManager.RetrieveAssociationsForBuild(projectId, buildId);

				//We have to use 'in-memory' sorting and filtering
				if (buildAssociations != null)
				{
					//Add the dynamic filters
					if (filterList != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						double utcOffset = GlobalFunctions.GetCurrentTimezoneUtcOffset();
						Expression<Func<ArtifactLinkView, bool>> filterClause = ArtifactLinkManager.CreateFilterExpression<ArtifactLinkView>(projectId, null, Artifact.ArtifactTypeEnum.None, filterList, utcOffset, null, null, false);
						if (filterClause != null)
						{
							buildAssociations = buildAssociations.Where(filterClause.Compile()).ToList();
						}
					}

					//Set the counts, filters, and pagination
					int itemCount = buildAssociations.Count;
					sortedData = SetSortAndFilter(projectId, sortedData, itemCount, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
				}

				//Add the dynamic sorts
				//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
				if (!String.IsNullOrEmpty(sortProperty))
				{
					//Convert from the shape fields to the ones in the test case entity
					string fieldName = sortProperty;
					buildAssociations = new GenericSorter<ArtifactLinkView>().Sort(buildAssociations, fieldName, sortAscending).ToList();
				}

				//Iterate through all the artifact links in the pagination range and populate the dataitem
				List<Project> projects = new List<Project>();
				for (int i = sortedData.StartRow - 1; i < buildAssociations.Count && i < (paginationSize + sortedData.StartRow - 1); i++)
				{
					ArtifactLinkView artifactLink = buildAssociations[i];
					SortedDataItem dataItem = filterItem.Clone();

					//Now populate with the data
					PopulateArtifactLinkRow(projectId, dataItem, artifactLink, projects);
					dataItems.Add(dataItem);
				}

				return sortedData;
			}
			catch (SourceCodeProviderLoadingException exception)
			{
				//Display a friendly message
				throw new DataValidationException(exception.Message);
			}
			catch (SourceCodeProviderArtifactPermissionDeniedException)
			{
				//If the user doesn't have permission to view, send back auth exception
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}
			catch (SourceCodeProviderGeneralException exception)
			{
				//Display a friendly message
				throw new DataValidationException(exception.Message);
			}
		}

		/// <summary>
		/// Returns a list of source code revision artifact associations
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="userId">The current user</param>
		/// <param name="revisionId">The id of the current revision</param>
		/// <param name="sortedData">Partially processed SortedData </param>
		/// <param name="dataItems">Partially processed dataItems list - used to populate the sortedData </param>
		/// <param name="sortProperty">Sorting of the data </param>
		/// <param name="sortAscending">Whether data should be sorted in ascending order </param>
		/// <param name="paginationSettings">Pagination settings </param>
		/// <param name="paginationSize">How many items should be shown on a page </param>
		/// <param name="currentPage">The current page to collect data for </param>
		/// <param name="filterList">Information about filters</param>
		/// <param name="filterItem">Filter item </param>
		/// <returns>SortData, fully processed, of linked artifacts</returns> 
		protected SortedData RetrieveSourceCodeRevisionAssociations(int userId, int projectId, int revisionId, SortedData sortedData, List<SortedDataItem> dataItems, string sortProperty, bool sortAscending, ProjectSettingsCollection paginationSettings, int paginationSize, int currentPage, Hashtable filterList, SortedDataItem filterItem)
		{
			//See if we have any artifact associations for the source code revision
			try
			{
				SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
				SourceCodeCommit sourceCodeCommit = sourceCodeManager.RetrieveRevisionById(revisionId);
				List<ArtifactLinkView> sourceCodeAssociations = sourceCodeManager.RetrieveAssociationsForRevision(sourceCodeCommit.Revisionkey);

				//We have to use 'in-memory' sorting and filtering
				if (sourceCodeAssociations != null)
				{
					//Add the dynamic filters
					if (filterList != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						double utcOffset = GlobalFunctions.GetCurrentTimezoneUtcOffset();
						Expression<Func<ArtifactLinkView, bool>> filterClause = ArtifactLinkManager.CreateFilterExpression<ArtifactLinkView>(projectId, null, Artifact.ArtifactTypeEnum.None, filterList, utcOffset, null, null, false);
						if (filterClause != null)
						{
							sourceCodeAssociations = sourceCodeAssociations.Where(filterClause.Compile()).ToList();
						}
					}

					//Set the counts, filters, and pagination
					int itemCount = sourceCodeAssociations.Count;
					sortedData = SetSortAndFilter(projectId, sortedData, itemCount, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
				}

				//Add the dynamic sorts
				//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
				if (!String.IsNullOrEmpty(sortProperty))
				{
					//Convert from the shape fields to the ones in the test case entity
					string fieldName = sortProperty;
					sourceCodeAssociations = new GenericSorter<ArtifactLinkView>().Sort(sourceCodeAssociations, fieldName, sortAscending).ToList();
				}

				//Iterate through all the artifact links in the pagination range and populate the dataitem
				List<Project> projects = new List<Project>();
				for (int i = sortedData.StartRow - 1; i < sourceCodeAssociations.Count && i < (paginationSize + sortedData.StartRow - 1); i++)
				{
					ArtifactLinkView artifactLink = sourceCodeAssociations[i];

					//Make sure the user is allowed to see this artifact
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (Artifact.ArtifactTypeEnum)artifactLink.ArtifactTypeId);
					if (authorizationState == Project.AuthorizationState.Authorized)
					{
						SortedDataItem dataItem = filterItem.Clone();

						//Now populate with the data
						PopulateArtifactLinkRow(projectId, dataItem, artifactLink, projects);
						dataItems.Add(dataItem);
					}
				}

				return sortedData;
			}
			catch (SourceCodeProviderLoadingException exception)
			{
				//Display a friendly message
				throw new DataValidationException(exception.Message);
			}
			catch (SourceCodeProviderArtifactPermissionDeniedException)
			{
				//If the user doesn't have permission to view, send back auth exception
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}
			catch (SourceCodeProviderGeneralException exception)
			{
				//Display a friendly message
				throw new DataValidationException(exception.Message);
			}
		}

		/// <summary>
		/// Returns a list of source code file artifact associations
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="userId">The current user</param>
		/// <param name="fileId">The id of the current file</param>
		/// <param name="sortedData">Partially processed SortedData </param>
		/// <param name="dataItems">Partially processed dataItems list - used to populate the sortedData </param>
		/// <param name="sortProperty">Sorting of the data </param>
		/// <param name="sortAscending">Whether data should be sorted in ascending order </param>
		/// <param name="paginationSettings">Pagination settings </param>
		/// <param name="paginationSize">How many items should be shown on a page </param>
		/// <param name="currentPage">The current page to collect data for </param>
		/// <param name="filterList">Information about filters</param>
		/// <param name="filterItem">Filter item </param>
		/// <returns>SortData, fully processed, of linked artifacts</returns> 
		protected SortedData RetrieveSourceCodeFileAssociations(int userId, int projectId, int fileId, SortedData sortedData, List<SortedDataItem> dataItems, string sortProperty, bool sortAscending, ProjectSettingsCollection paginationSettings, int paginationSize, int currentPage, Hashtable filterList, SortedDataItem filterItem)
		{
			//See if we have any artifact associations for the source code revision
			try
			{
				string branchKey;
				SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
				SourceCodeFile sourceCodeFile = sourceCodeManager.RetrieveFileById(fileId, out branchKey);
				List<ArtifactLinkView> sourceCodeAssociations = sourceCodeManager.RetrieveAssociationsForFile(sourceCodeFile.FileKey, branchKey);

				//We have to use 'in-memory' sorting and filtering
				if (sourceCodeAssociations != null)
				{
					//Add the dynamic filters
					if (filterList != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						double utcOffset = GlobalFunctions.GetCurrentTimezoneUtcOffset();
						Expression<Func<ArtifactLinkView, bool>> filterClause = ArtifactLinkManager.CreateFilterExpression<ArtifactLinkView>(projectId, null, Artifact.ArtifactTypeEnum.None, filterList, utcOffset, null, null, false);
						if (filterClause != null)
						{
							sourceCodeAssociations = sourceCodeAssociations.Where(filterClause.Compile()).ToList();
						}
					}

					//Set the counts, filters, and pagination
					int itemCount = sourceCodeAssociations.Count;
					sortedData = SetSortAndFilter(projectId, sortedData, itemCount, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);
				}

				//Add the dynamic sorts
				//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
				if (!String.IsNullOrEmpty(sortProperty))
				{
					//Convert from the shape fields to the ones in the test case entity
					string fieldName = sortProperty;
					sourceCodeAssociations = new GenericSorter<ArtifactLinkView>().Sort(sourceCodeAssociations, fieldName, sortAscending).ToList();
				}

				//Iterate through all the artifact links in the pagination range and populate the dataitem
				List<Project> projects = new List<Project>();
				for (int i = sortedData.StartRow - 1; i < sourceCodeAssociations.Count && i < (paginationSize + sortedData.StartRow - 1); i++)
				{
					ArtifactLinkView artifactLink = sourceCodeAssociations[i];

					//Make sure the user is allowed to see this artifact
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (Artifact.ArtifactTypeEnum)artifactLink.ArtifactTypeId);
					if (authorizationState == Project.AuthorizationState.Authorized)
					{

						SortedDataItem dataItem = filterItem.Clone();

						//Now populate with the data
						PopulateArtifactLinkRow(projectId, dataItem, artifactLink, projects);
						dataItems.Add(dataItem);
					}
				}

				return sortedData;
			}
			catch (SourceCodeProviderLoadingException exception)
			{
				//Display a friendly message
				throw new DataValidationException(exception.Message);
			}
			catch (SourceCodeProviderArtifactPermissionDeniedException)
			{
				//If the user doesn't have permission to view, send back auth exception
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}
			catch (SourceCodeProviderGeneralException exception)
			{
				//Display a friendly message
				throw new DataValidationException(exception.Message);
			}
		}

		/// <summary>
		/// Returns a list of attachment associations
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="userId">The current user</param>
		/// <param name="attachmentId">The id of the current attachment</param>
		/// <param name="sortedData">Partially processed SortedData </param>
		/// <param name="dataItems">Partially processed dataItems list - used to populate the sortedData </param>
		/// <param name="sortProperty">Sorting of the data </param>
		/// <param name="sortAscending">Whether data should be sorted in ascending order </param>
		/// <param name="paginationSettings">Pagination settings </param>
		/// <param name="paginationSize">How many items should be shown on a page </param>
		/// <param name="currentPage">The current page to collect data for </param>
		/// <param name="filterList">Information about filters</param>
		/// <param name="filterItem">Filter item </param>
		/// <returns>SortData, fully processed, of linked artifacts</returns> 
		private SortedData RetrieveAttachmentArtifacts(int projectId, int attachmentId, SortedData sortedData, List<SortedDataItem> dataItems, string sortProperty, bool sortAscending, ProjectSettingsCollection paginationSettings, int paginationSize, int currentPage, Hashtable filterList, SortedDataItem filterItem)
		{
			const string METHOD_NAME = "RetrieveArtifactLinks";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Actually retrieve the list of artifact links. We use in-memory pagination since the lists are generally small
			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
			List<ArtifactAttachmentView> attachmentAssociations = artifactLinkManager.RetrieveByAttachmentId(projectId, attachmentId);

			//Set the counts, filters, and pagination
			int itemCount = attachmentAssociations.Count;
			sortedData = SetSortAndFilter(projectId, sortedData, itemCount, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);

			//We have to use 'in-memory' sorting and filtering
			if (attachmentAssociations != null)
			{
				//Add the dynamic filters
				if (filterList != null)
				{
					//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
					double utcOffset = GlobalFunctions.GetCurrentTimezoneUtcOffset();
					Expression<Func<ArtifactAttachmentView, bool>> filterClause = ArtifactLinkManager.CreateFilterExpression<ArtifactAttachmentView>(projectId, null, Artifact.ArtifactTypeEnum.Requirement, filterList, utcOffset, null, null, false);
					if (filterClause != null)
					{
						attachmentAssociations = attachmentAssociations.Where(filterClause.Compile()).ToList();
					}
				}

				//Add the dynamic sorts
				//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
				if (!String.IsNullOrEmpty(sortProperty))
				{
					//Convert from the shape fields to the ones in the test case entity
					string fieldName = sortProperty;
					attachmentAssociations = new GenericSorter<ArtifactAttachmentView>().Sort(attachmentAssociations, fieldName, sortAscending).ToList();
				}
			}

			//Attachments can only be linked to items in the same project as the artifact

			//Iterate through all the artifact links in the pagination range and populate the dataitem
			for (int i = sortedData.StartRow - 1; i < attachmentAssociations.Count && i < (paginationSize + sortedData.StartRow - 1); i++)
			{
				ArtifactAttachmentView artifactLink = attachmentAssociations[i];
				SortedDataItem dataItem = filterItem.Clone();

				//Now populate with the data
				PopulateAttachmentArtifactRow(projectId, dataItem, artifactLink);
				dataItems.Add(dataItem);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();

			return sortedData;
		}

		/// <summary>
		/// Returns a list of linked artifacts
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="userId">The current user</param>
		/// <param name="artifactType">The artifact type asking for links</param>
		/// <param name="artifactId">The id of the current artifact</param>
		/// <param name="sortedData">Partially processed SortedData </param>
		/// <param name="dataItems">Partially processed dataItems list - used to populate the sortedData </param>
		/// <param name="sortProperty">Sorting of the data </param>
		/// <param name="sortAscending">Whether data should be sorted in ascending order </param>
		/// <param name="paginationSettings">Pagination settings </param>
		/// <param name="paginationSize">How many items should be shown on a page </param>
		/// <param name="currentPage">The current page to collect data for </param>
		/// <param name="filterList">Information about filters</param>
		/// <param name="filterItem">Filter item </param>
		/// <returns>SortData, fully processed, of linked artifacts</returns> 
		private SortedData RetrieveArtifactLinks(int projectId, int userId, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, SortedData sortedData, List<SortedDataItem> dataItems, string sortProperty, bool sortAscending, ProjectSettingsCollection paginationSettings, int paginationSize, int currentPage, Hashtable filterList, SortedDataItem filterItem)
		{
			const string METHOD_NAME = "RetrieveArtifactLinks";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();

			//Actually retrieve the list of artifact links. We use in-memory pagination since the lists are generally small
			List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(artifactType, artifactId, sortProperty, sortAscending, filterList);

			//If we are running SpiraPlan/SpiraTeam also include source code revisions if enabled for project
			//If the filter excludes source code revisions, don't bother
			bool sourceCodeIncluded = true;
			bool implicitLinksIncluded = true;
			if (filterList.ContainsKey("ArtifactLinkTypeId"))
			{
				if (filterList["ArtifactLinkTypeId"] is MultiValueFilter)
				{
					MultiValueFilter mvf = (MultiValueFilter)filterList["ArtifactLinkTypeId"];
					if (mvf.IsNone)
					{
						sourceCodeIncluded = false;
						implicitLinksIncluded = false;
					}
					if (mvf.Values != null && mvf.Values.Count > 0 && !mvf.Values.Contains((int)ArtifactLink.ArtifactLinkTypeEnum.SourceCodeCommit))
					{
						sourceCodeIncluded = false;
					}
					if (mvf.Values != null && mvf.Values.Count > 0 && !mvf.Values.Contains((int)ArtifactLink.ArtifactLinkTypeEnum.Implicit))
					{
						implicitLinksIncluded = false;
					}
				}
			}

			//PCS
			if ((Common.License.LicenseProductName == LicenseProductNameEnum.ValidationMaster) && sourceCodeIncluded && Common.Global.SourceCode_IncludeInAssociationsAndDocuments)
			{
				try
				{
					SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
					List<SourceCodeCommit> sourceCodeRevisions = sourceCodeManager.RetrieveRevisionsForArtifact(artifactType, artifactId);
					foreach (SourceCodeCommit sourceCodeRevision in sourceCodeRevisions)
					{
						//Add the source code revision row
						ArtifactLinkView artifactLinkRow = new ArtifactLinkView();
						artifactLinkRow.ArtifactId = sourceCodeRevision.RevisionId;
						artifactLinkRow.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.SourceCodeRevision;
						artifactLinkRow.ArtifactTypeName = SourceCodeManager.REVISION_ARTIFACT_TYPE_NAME;
						artifactLinkRow.Comment = sourceCodeRevision.Message;
						artifactLinkRow.CreatorId = Business.UserManager.UserInternal;
						artifactLinkRow.CreatorName = sourceCodeRevision.AuthorName;
						artifactLinkRow.CreationDate = sourceCodeRevision.UpdateDate;
						artifactLinkRow.ArtifactName = sourceCodeRevision.Name;
						artifactLinkRow.ArtifactLinkTypeId = (int)ArtifactLink.ArtifactLinkTypeEnum.SourceCodeCommit;
						artifactLinkRow.ArtifactLinkTypeName = Resources.Fields.SourceCode;
						artifactLinkRow.ArtifactStatusName = Resources.Main.Global_NotApplicable;
						artifactLinks.Add(artifactLinkRow);

						//See if this revision has builds associated
						List<Build> builds = new BuildManager().RetrieveForRevision(projectId, sourceCodeRevision.Revisionkey);
						if (builds != null && builds.Count > 0)
						{
							foreach (Build build in builds)
							{
								//Add the build row
								artifactLinkRow = new ArtifactLinkView();
								artifactLinkRow.ArtifactId = build.BuildId;
								artifactLinkRow.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Build;
								artifactLinkRow.ArtifactTypeName = Resources.Fields.Build;
								artifactLinkRow.Comment = sourceCodeRevision.Message;
								artifactLinkRow.CreatorId = Business.UserManager.UserInternal;
								artifactLinkRow.CreatorName = sourceCodeRevision.AuthorName;
								artifactLinkRow.CreationDate = build.CreationDate;
								artifactLinkRow.ArtifactName = build.Name;
								artifactLinkRow.ArtifactLinkTypeId = (int)ArtifactLink.ArtifactLinkTypeEnum.SourceCodeCommit;
								artifactLinkRow.ArtifactLinkTypeName = Resources.Fields.SourceCode;
								artifactLinkRow.ArtifactStatusName = build.Status.Name;

								//Only add th build if it is not already in the list of artifacts - this happens when a build contains 2+ commits that are included above
								if (!artifactLinks.Any(row => row.ArtifactId == artifactLinkRow.ArtifactId && row.ArtifactTypeId == artifactLinkRow.ArtifactTypeId))
								{
									artifactLinks.Add(artifactLinkRow);
								}
							}
						}
					}
				}
				catch (SourceCodeProviderLoadingException)
				{
					//It's not enabled so don't even log
				}
				catch (Exception exception)
				{
					//Log and continue
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				}
			}

			//Set the counts, filters, and pagination
			int itemCount = artifactLinks.Count;
			sortedData = SetSortAndFilter(projectId, sortedData, itemCount, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);

			//Populate the list of linked projects for the project field
			//Since links can be in both directions, need links from this project as well
			List<Project> projects = new ProjectManager().ProjectAssociation_RetrieveForDestProjectIncludingSelf(projectId);
			List<Project> destProjects = new ProjectManager().ProjectAssociation_RetrieveForSourceProject(projectId);

			//Iterate through all the artifact links in the pagination range and populate the dataitem
			for (int i = sortedData.StartRow - 1; i < artifactLinks.Count && i < (paginationSize + sortedData.StartRow - 1); i++)
			{
				ArtifactLinkView artifactLink = artifactLinks[i];

				//If we are filtering out implicit links, remove them
				if (implicitLinksIncluded || artifactLink.ArtifactLinkTypeId != (int)ArtifactLink.ArtifactLinkTypeEnum.Implicit)
				{
					SortedDataItem dataItem = filterItem.Clone();

					//Now populate with the data
					PopulateArtifactLinkRow(projectId, dataItem, artifactLink, projects, destProjects);
					dataItems.Add(dataItem);
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();

			return sortedData;
		}

		/// <summary>
		/// Returns a list of test cases covering the associated artifact (requirement or release)
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="artifactType">The artifact type asking for links (ie requirement or release)</param>
		/// <param name="artifactId">The id of the current artifact</param>
		/// <param name="sortedData">Partially processed SortedData </param>
		/// <param name="dataItems">Partially processed dataItems list - used to populate the sortedData </param>
		/// <param name="sortProperty">Sorting of the data </param>
		/// <param name="sortAscending">Whether data should be sorted in ascending order </param>
		/// <param name="paginationSettings">Pagination settings </param>
		/// <param name="paginationSize">How many items should be shown on a page </param>
		/// <param name="currentPage">The current page to collect data for </param>
		/// <param name="filterList">Information about filters</param>
		/// <param name="filterItem">Filter item </param>
		/// <returns>SortData, fully processed, of test cases</returns>     
		private SortedData RetrieveTestCaseCoverage(int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, SortedData sortedData, List<SortedDataItem> dataItems, string sortProperty, bool sortAscending, ProjectSettingsCollection paginationSettings, int paginationSize, int currentPage, Hashtable filterList, SortedDataItem filterItem)
		{
			const string METHOD_NAME = "RetrieveTestCaseCoverage";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int itemCount = 0;
			try
			{
				//Instantiate the test case business object
				TestCaseManager testCaseManager = new TestCaseManager();

				//Populate the list of linked projects for the project field
				List<Project> projects = new ProjectManager().ProjectAssociation_RetrieveForDestProjectIncludingSelf(projectId);

				//Get the list of mapped test cases
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Requirement)
				{
					List<TestCase> mappedTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, artifactId);

					//Add the dynamic filters
					if (filterList != null)
					{
						//Handle the Name filter separately
						if (filterList.ContainsKey("ArtifactName"))
						{
							string keyword = (string)filterList["ArtifactName"];
							if (!String.IsNullOrEmpty(keyword))
							{
								mappedTestCases = mappedTestCases.Where(r => r.Name != null && r.Name.ToLower().Contains(keyword)).ToList();
							}
						}

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						double utcOffset = GlobalFunctions.GetCurrentTimezoneUtcOffset();
						Expression<Func<TestCase, bool>> filterClause = TestCaseManager.CreateFilterExpression<TestCase>(projectId, null, Artifact.ArtifactTypeEnum.TestCase, filterList, utcOffset, null, null, false);
						if (filterClause != null)
						{
							mappedTestCases = mappedTestCases.Where(filterClause.Compile()).ToList();
						}
					}

					//Add the dynamic sorts
					//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
					if (!String.IsNullOrEmpty(sortProperty))
					{
						//Convert from the shape fields to the ones in the test case entity
						string fieldName = sortProperty;
						switch (sortProperty)
						{
							case "ArtifactName":
								fieldName = "Name";
								break;

							case "ArtifactId":
								fieldName = "TestCaseId";
								break;
						}

						mappedTestCases = new GenericSorter<TestCase>().Sort(mappedTestCases, fieldName, sortAscending).ToList();
					}

					// set the count
					itemCount = mappedTestCases.Count;

					//Set the counts, filters, pagination
					sortedData = SetSortAndFilter(projectId, sortedData, itemCount, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);

					//Iterate through all the test cases in the pagination range and populate the dataitem (only some columns are needed)
					int visibleCount = 0;
					for (int i = sortedData.StartRow - 1; i < mappedTestCases.Count && i < (paginationSize + sortedData.StartRow - 1); i++)
					{
						TestCase testCase = mappedTestCases[i];

						//Create the data-item
						SortedDataItem dataItem = new SortedDataItem();

						//Populate the necessary fields
						dataItem.PrimaryKey = testCase.TestCaseId;

						//Test Case Id
						string artifactPrefix = GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum.TestCase));

						DataItemField dataItemField = new DataItemField();
						dataItemField.FieldName = "ArtifactId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.ArtifactTypeId;
						dataItemField.IntValue = testCase.TestCaseId;
						dataItemField.TextValue = GlobalFunctions.GetTokenForArtifact(artifactPrefix, testCase.TestCaseId, true);
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Name/Desc
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ArtifactName";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
						dataItemField.Caption = Resources.Fields.ArtifactName;
						dataItemField.TextValue = testCase.Name;
						dataItemField.Tooltip = GlobalFunctions.GetIconForArtifactType(DataModel.Artifact.ArtifactTypeEnum.TestCase);
						dataItem.Alternate = testCase.IsTestSteps;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Execution Status
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ExecutionStatusId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.ExecutionStatus;
						dataItemField.IntValue = testCase.ExecutionStatusId;
						dataItemField.TextValue = testCase.ExecutionStatusName;
						dataItemField.CssClass = GlobalFunctions.GetExecutionStatusCssClass(testCase.ExecutionStatusId);
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Last Executed date
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ExecutionDate";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
						dataItemField.Caption = Resources.Fields.LastExecuted;
						dataItemField.DateValue = testCase.ExecutionDate;
						dataItemField.Tooltip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(testCase.ExecutionDate));
						dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(testCase.ExecutionDate));
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Test Case Status
						dataItemField = new DataItemField();
						dataItemField.FieldName = "TestCaseStatusId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Status;
						dataItemField.IntValue = testCase.TestCaseStatusId;
						dataItemField.TextValue = testCase.Status.Name;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Priority
						dataItemField = new DataItemField();
						dataItemField.FieldName = "TestCasePriorityId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Priority;
						dataItemField.IntValue = testCase.TestCasePriorityId;
						dataItemField.TextValue = (testCase.Priority != null) ? testCase.Priority.Name : "";
						dataItemField.CssClass = (testCase.Priority != null) ? "#" + testCase.Priority.Color : "";
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Type
						dataItemField = new DataItemField();
						dataItemField.FieldName = "TestCaseTypeId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Type;
						dataItemField.IntValue = testCase.TestCaseTypeId;
						dataItemField.TextValue = (testCase.Type != null) ? testCase.Type.Name : "";
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Project Id/Name
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ProjectId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.ProjectName;
						dataItemField.IntValue = testCase.ProjectId;
						Project project = projects.FirstOrDefault(p => p.ProjectId == testCase.ProjectId);
						if (project != null)
						{
							dataItemField.TextValue = project.Name;
						}
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Add to the items collection
						dataItems.Add(dataItem);
						visibleCount++;
					}

					//Update the visible count
					sortedData.VisibleCount = visibleCount;
				}
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Release)
				{
					List<TestCaseReleaseView> mappedTestCases = testCaseManager.RetrieveMappedByReleaseId2(projectId, artifactId);

					//Add the dynamic filters
					if (filterList != null)
					{
						//Handle the Name filter separately
						if (filterList.ContainsKey("ArtifactName"))
						{
							string keyword = (string)filterList["ArtifactName"];
							mappedTestCases = mappedTestCases.Where(r => r.Name != null && r.Name.ToLower().Contains(keyword)).ToList();
						}

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						double utcOffset = GlobalFunctions.GetCurrentTimezoneUtcOffset();
						Expression<Func<TestCaseReleaseView, bool>> filterClause = TestCaseManager.CreateFilterExpression<TestCaseReleaseView>(projectId, null, Artifact.ArtifactTypeEnum.TestCase, filterList, utcOffset, null, null, false);
						if (filterClause != null)
						{
							mappedTestCases = mappedTestCases.Where(filterClause.Compile()).ToList();
						}
					}

					//Add the dynamic sorts
					//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
					if (!String.IsNullOrEmpty(sortProperty))
					{
						//Convert from the shape fields to the ones in the test case entity
						string fieldName = sortProperty;
						switch (sortProperty)
						{
							case "ArtifactName":
								fieldName = "Name";
								break;

							case "ArtifactId":
								fieldName = "TestCaseId";
								break;
						}

						mappedTestCases = new GenericSorter<TestCaseReleaseView>().Sort(mappedTestCases, fieldName, sortAscending).ToList();
					}

					// set the count
					itemCount = mappedTestCases.Count;

					//Set the counts, filters, pagination
					sortedData = SetSortAndFilter(projectId, sortedData, itemCount, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);

					//Iterate through all the test cases in the pagination range and populate the dataitem (only some columns are needed)
					int visibleCount = 0;
					for (int i = sortedData.StartRow - 1; i < mappedTestCases.Count && i < (paginationSize + sortedData.StartRow - 1); i++)
					{
						TestCaseReleaseView testCase = mappedTestCases[i];
						//Create the data-item
						SortedDataItem dataItem = new SortedDataItem();

						//Populate the necessary fields
						dataItem.PrimaryKey = testCase.TestCaseId;

						//Test Case Id
						string artifactPrefix = GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum.TestCase));

						DataItemField dataItemField = new DataItemField();
						dataItemField.FieldName = "ArtifactId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.ArtifactTypeId;
						dataItemField.IntValue = testCase.TestCaseId;
						dataItemField.TextValue = GlobalFunctions.GetTokenForArtifact(artifactPrefix, testCase.TestCaseId, true);
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Name/Desc
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ArtifactName";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
						dataItemField.Caption = Resources.Fields.ArtifactName;
						dataItemField.TextValue = testCase.Name;
						dataItemField.Tooltip = GlobalFunctions.GetIconForArtifactType(DataModel.Artifact.ArtifactTypeEnum.TestCase);
						dataItem.Alternate = testCase.IsTestSteps;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Execution Status
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ExecutionStatusId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.ExecutionStatus;
						dataItemField.IntValue = testCase.ExecutionStatusId;
						dataItemField.TextValue = testCase.ExecutionStatusName;
						dataItemField.CssClass = GlobalFunctions.GetExecutionStatusCssClass(testCase.ExecutionStatusId);
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Last Executed date
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ExecutionDate";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
						dataItemField.Caption = Resources.Fields.LastExecuted;
						dataItemField.DateValue = testCase.ExecutionDate;
						dataItemField.Tooltip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(testCase.ExecutionDate));
						dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(testCase.ExecutionDate));
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Test Case Status
						dataItemField = new DataItemField();
						dataItemField.FieldName = "TestCaseStatusId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Status;
						dataItemField.IntValue = testCase.TestCaseStatusId;
						dataItemField.TextValue = testCase.TestCaseStatusName;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Priority
						dataItemField = new DataItemField();
						dataItemField.FieldName = "TestCasePriorityId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Priority;
						dataItemField.IntValue = testCase.TestCasePriorityId;
						dataItemField.TextValue = (testCase.TestCasePriorityName != null) ? testCase.TestCasePriorityName : "";
						dataItemField.CssClass = (testCase.TestCasePriorityName != null) ? "#" + testCase.TestCasePriorityColor : "";
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Status
						dataItemField = new DataItemField();
						dataItemField.FieldName = "TestCaseTypeId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Type;
						dataItemField.IntValue = testCase.TestCaseTypeId;
						dataItemField.TextValue = (testCase.TestCaseTypeName != null) ? testCase.TestCaseTypeName : "";
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Project Id/Name
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ProjectId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.ProjectName;
						dataItemField.IntValue = testCase.ProjectId;
						Project project = projects.FirstOrDefault(p => p.ProjectId == testCase.ProjectId);
						if (project != null)
						{
							dataItemField.TextValue = project.Name;
						}
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Add to the items collection
						dataItems.Add(dataItem);
						visibleCount++;
					}

					//Update the visible count
					sortedData.VisibleCount = visibleCount;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return sortedData;

			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of requirements covering the associated artifact (test case or test step)
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="artifactType">The artifact type asking for links (ie test case or test step)</param>
		/// <param name="artifactId">The id of the current artifact</param>
		/// <param name="sortedData">Partially processed SortedData </param>
		/// <param name="dataItems">Partially processed dataItems list - used to populate the sortedData </param>
		/// <param name="sortProperty">Sorting of the data </param>
		/// <param name="sortAscending">Whether data should be sorted in ascending order </param>
		/// <param name="paginationSettings">Pagination settings </param>
		/// <param name="paginationSize">How many items should be shown on a page </param>
		/// <param name="currentPage">The current page to collect data for </param>
		/// <param name="filterList">Information about filters</param>
		/// <param name="filterItem">Filter item </param>
		/// <returns>SortData, fully processed, of requirements</returns>     
		private SortedData RetrieveRequirementCoverage(int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, SortedData sortedData, List<SortedDataItem> dataItems, string sortProperty, bool sortAscending, ProjectSettingsCollection paginationSettings, int paginationSize, int currentPage, Hashtable filterList, SortedDataItem filterItem)
		{
			const string METHOD_NAME = "RetrieveRequirementCoverage";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int itemCount = 0;
			try
			{
				//Instantiate the requirement business object
				RequirementManager requirementManager = new RequirementManager();

				//Populate the list of linked projects for the project field
				List<Project> projects = new ProjectManager().ProjectAssociation_RetrieveForDestProjectIncludingSelf(projectId);

				//Get the list of mapped requirements
				List<RequirementView> mappedRequirements = null;
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestCase)
				{
					mappedRequirements = requirementManager.RetrieveCoveredByTestCaseId(User.UserInternal, null, artifactId);
				}
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestStep)
				{
					mappedRequirements = requirementManager.RequirementTestStep_RetrieveByTestStepId(User.UserInternal, projectId, artifactId);
				}

				if (mappedRequirements != null)
				{
					//Add the dynamic filters
					if (filterList != null)
					{
						//Handle the Name filter separately
						if (filterList.ContainsKey("ArtifactName"))
						{
							string keyword = (string)filterList["ArtifactName"];
							mappedRequirements = mappedRequirements.Where(r => r.Name != null && r.Name.ToLower().Contains(keyword)).ToList();
						}

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						double utcOffset = GlobalFunctions.GetCurrentTimezoneUtcOffset();
						Expression<Func<RequirementView, bool>> filterClause = RequirementManager.CreateFilterExpression<RequirementView>(projectId, null, Artifact.ArtifactTypeEnum.Requirement, filterList, utcOffset, null, null, false);
						if (filterClause != null)
						{
							mappedRequirements = mappedRequirements.Where(filterClause.Compile()).ToList();
						}
					}

					//Add the dynamic sorts
					//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
					if (!String.IsNullOrEmpty(sortProperty))
					{
						//Convert from the shape fields to the ones in the test case entity
						string fieldName = sortProperty;
						switch (sortProperty)
						{
							case "ArtifactName":
								fieldName = "Name";
								break;

							case "ArtifactId":
								fieldName = "RequirementId";
								break;
						}

						mappedRequirements = new GenericSorter<RequirementView>().Sort(mappedRequirements, fieldName, sortAscending).ToList();
					}

					//Iterate through all the test cases and populate the dataitem (only some columns are needed)
					foreach (RequirementView requirement in mappedRequirements)
					{
						//Create the data-item
						SortedDataItem dataItem = new SortedDataItem();

						//Populate the necessary fields
						dataItem.PrimaryKey = requirement.RequirementId;

						//Requirement Id
						string artifactPrefix = GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum.Requirement));

						DataItemField dataItemField = new DataItemField();
						dataItemField.FieldName = "ArtifactId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.ArtifactTypeId;
						dataItemField.IntValue = requirement.RequirementId;
						dataItemField.TextValue = GlobalFunctions.GetTokenForArtifact(artifactPrefix, requirement.RequirementId, true);
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Name/Desc
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ArtifactName";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
						dataItemField.Caption = Resources.Fields.ArtifactName;
						dataItemField.TextValue = requirement.Name;
						dataItem.Alternate = requirement.RequirementTypeIsSteps;
						dataItemField.Tooltip = GlobalFunctions.GetIconForArtifactType(DataModel.Artifact.ArtifactTypeEnum.Requirement, dataItem.Alternate);
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Requirement Status
						dataItemField = new DataItemField();
						dataItemField.FieldName = "RequirementStatusId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Status;
						dataItemField.IntValue = requirement.RequirementStatusId;
						dataItemField.TextValue = requirement.RequirementStatusName;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Importance
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ImportanceId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Importance;
						dataItemField.IntValue = requirement.ImportanceId;
						dataItemField.TextValue = requirement.ImportanceName;
						dataItemField.CssClass = (requirement.ImportanceColor == null) ? "" : "#" + requirement.ImportanceColor;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Type
						dataItemField = new DataItemField();
						dataItemField.FieldName = "RequirementTypeId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Type;
						dataItemField.IntValue = requirement.RequirementTypeId;
						dataItemField.TextValue = requirement.RequirementTypeName;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Project Id/Name
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ProjectId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.ProjectName;
						dataItemField.IntValue = requirement.ProjectId;
						Project project = projects.FirstOrDefault(p => p.ProjectId == requirement.ProjectId);
						if (project != null)
						{
							dataItemField.TextValue = project.Name;
						}
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Add to the items collection
						dataItems.Add(dataItem);
					}
					// set the count
					itemCount = mappedRequirements.Count;
				}

				//Set the counts, filters, pagination
				sortedData = SetSortAndFilter(projectId, sortedData, itemCount, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return sortedData;

			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of releases mapped to the associated artifact (test case only currently)
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="artifactType">The artifact type asking for links (ie test case)</param>
		/// <param name="artifactId">The id of the current artifact</param>
		/// <param name="sortedData">Partially processed SortedData </param>
		/// <param name="dataItems">Partially processed dataItems list - used to populate the sortedData </param>
		/// <param name="sortProperty">Sorting of the data </param>
		/// <param name="sortAscending">Whether data should be sorted in ascending order </param>
		/// <param name="paginationSettings">Pagination settings </param>
		/// <param name="paginationSize">How many items should be shown on a page </param>
		/// <param name="currentPage">The current page to collect data for </param>
		/// <param name="filterList">Information about filters</param>
		/// <param name="filterItem">Filter item </param>
		/// <returns>SortData, fully processed, of releases</returns>     
		private SortedData RetrieveReleaseMapping(int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, SortedData sortedData, List<SortedDataItem> dataItems, string sortProperty, bool sortAscending, ProjectSettingsCollection paginationSettings, int paginationSize, int currentPage, Hashtable filterList, SortedDataItem filterItem)
		{
			const string METHOD_NAME = "RetrieveReleaseMapping";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int itemCount = 0;
			try
			{
				//Instantiate the release business object
				ReleaseManager releaseManager = new ReleaseManager();

				//Populate the list of linked projects for the project field
				List<Project> projects = new ProjectManager().ProjectAssociation_RetrieveForDestProjectIncludingSelf(projectId);

				//Get the list of mapped releases
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestCase)
				{
					List<ReleaseView> mappedReleases = releaseManager.RetrieveMappedByTestCaseId(User.UserInternal, projectId, artifactId);

					//Add the dynamic filters
					if (filterList != null)
					{
						//Handle the Name filter separately
						if (filterList.ContainsKey("ArtifactName"))
						{
							string keyword = (string)filterList["ArtifactName"];
							mappedReleases = mappedReleases.Where(r => r.Name != null && r.Name.ToLower().Contains(keyword)).ToList();
						}

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						double utcOffset = GlobalFunctions.GetCurrentTimezoneUtcOffset();
						Expression<Func<ReleaseView, bool>> filterClause = ReleaseManager.CreateFilterExpression<ReleaseView>(projectId, null, Artifact.ArtifactTypeEnum.Release, filterList, utcOffset, null, null, false);
						if (filterClause != null)
						{
							mappedReleases = mappedReleases.Where(filterClause.Compile()).ToList();
						}
					}

					//Add the dynamic sorts
					//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
					if (!String.IsNullOrEmpty(sortProperty))
					{
						//Convert from the shape fields to the ones in the test case entity
						string fieldName = sortProperty;
						switch (sortProperty)
						{
							case "ArtifactName":
								fieldName = "Name";
								break;

							case "ArtifactId":
								fieldName = "ReleaseId";
								break;
						}

						mappedReleases = new GenericSorter<ReleaseView>().Sort(mappedReleases, fieldName, sortAscending).ToList();
					}

					//Iterate through all the test cases and populate the dataitem (only some columns are needed)
					foreach (ReleaseView release in mappedReleases)
					{
						//Create the data-item
						SortedDataItem dataItem = new SortedDataItem();

						//Populate the necessary fields
						dataItem.PrimaryKey = release.ReleaseId;

						//Release Id
						string artifactPrefix = GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum.Release));

						DataItemField dataItemField = new DataItemField();
						dataItemField.FieldName = "ArtifactId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.ArtifactTypeId;
						dataItemField.IntValue = release.ReleaseId;
						dataItemField.TextValue = GlobalFunctions.GetTokenForArtifact(artifactPrefix, release.ReleaseId, true);
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Name/Desc
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ArtifactName";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
						dataItemField.Caption = Resources.Fields.ArtifactName;
						dataItemField.TextValue = release.Name;
						dataItemField.Tooltip = GlobalFunctions.GetIconForArtifactType(DataModel.Artifact.ArtifactTypeEnum.Release, release.IsIterationOrPhase);
						dataItem.Alternate = release.IsIterationOrPhase;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Version Number
						dataItemField = new DataItemField();
						dataItemField.FieldName = "VersionNumber";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
						dataItemField.Caption = Resources.Fields.VersionNumber;
						dataItemField.TextValue = release.VersionNumber;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Release Status
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ReleaseStatusId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Status;
						dataItemField.IntValue = release.ReleaseStatusId;
						dataItemField.TextValue = release.ReleaseStatusName;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Type
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ReleaseTypeId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.Type;
						dataItemField.IntValue = release.ReleaseTypeId;
						dataItemField.TextValue = release.ReleaseTypeName;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Project Id/Name
						dataItemField = new DataItemField();
						dataItemField.FieldName = "ProjectId";
						dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
						dataItemField.Caption = Resources.Fields.ProjectName;
						dataItemField.IntValue = release.ProjectId;
						Project project = projects.FirstOrDefault(p => p.ProjectId == release.ProjectId);
						if (project != null)
						{
							dataItemField.TextValue = project.Name;
						}
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Add to the items collection
						dataItems.Add(dataItem);
					}
					// set the count
					itemCount = mappedReleases.Count;
				}

				//Set the counts, filters, pagination
				sortedData = SetSortAndFilter(projectId, sortedData, itemCount, sortProperty, sortAscending, paginationSettings, paginationSize, currentPage, filterList, filterItem);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return sortedData;

			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Sets the sorts, filter , and pagination options for the sorteddata view
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="sortedData">Partially processed SortedData </param>
		/// <param name="itemCount">The number of items retrieved </param>
		/// <param name="sortProperty">Sorting of the data </param>
		/// <param name="sortAscending">Whether data should be sorted in ascending order </param>
		/// <param name="paginationSettings">Pagination settings </param>
		/// <param name="paginationSize">How many items should be shown on a page </param>
		/// <param name="currentPage">The current page to collect data for </param>
		/// <param name="filterList">Information about filters</param>
		/// <param name="filterItem">Filter item </param>
		/// <returns>SortData, fully processed, of linked artifacts</returns> 
		private SortedData SetSortAndFilter(int projectId, SortedData sortedData, int itemCount, string sortProperty, bool sortAscending, ProjectSettingsCollection paginationSettings, int paginationSize, int currentPage, Hashtable filterList, SortedDataItem filterItem)
		{
			const string METHOD_NAME = "SetSortAndFilter";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);


			//Get the counts
			int pageCount = (int)Decimal.Ceiling((decimal)itemCount / (decimal)paginationSize);

			//Make sure that the current page is not larger than the number of pages or less than 1
			if (currentPage > pageCount)
			{
				currentPage = pageCount;
				paginationSettings["CurrentPage"] = currentPage;
				paginationSettings.Save();
			}
			if (currentPage < 1)
			{
				currentPage = 1;
				paginationSettings["CurrentPage"] = currentPage;
				paginationSettings.Save();
			}
			int startRow = ((currentPage - 1) * paginationSize) + 1;

			//Display the pagination information
			sortedData.CurrPage = currentPage;
			sortedData.PageCount = pageCount;
			sortedData.StartRow = startRow;

			//Display the sort information
			sortedData.SortProperty = sortProperty;
			sortedData.SortAscending = sortAscending;

			//Display the visible and total count of artifacts
			sortedData.VisibleCount = itemCount;
			sortedData.TotalCount = itemCount;

			//Also include the pagination info
			sortedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();

			return sortedData;
		}


		/// <summary>
		/// Updates the current sort stored in the system (property and direction)
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The artifact property we want to sort on</param>
		/// <param name="sortAscending">Are we sorting ascending or not</param>
		/// <returns>Any error messages</returns>
		public string SortedList_UpdateSort(int projectId, string sortProperty, bool sortAscending, int? displayTypeId)
		{
			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Make sure we have a display mode specified
			if (displayTypeId.HasValue)
			{
				DataModel.Artifact.DisplayTypeEnum displayType = (DataModel.Artifact.DisplayTypeEnum)displayTypeId.Value;

				string projectSetting_Filters;
				string projectSetting_Sort;
				string defaultSortExpression;
				GetSettingsCollections(displayType, out projectSetting_Filters, out projectSetting_Sort, out defaultSortExpression);

				//Call the base method with the appropriate settings collection
				return base.UpdateSort(userId, projectId, sortProperty, sortAscending, projectSetting_Sort);
			}
			return "";
		}

		/// <summary>
		/// Returns the latest information on a single artifact link in the system
		/// </summary>
		/// <param name="userId">The user we're viewing the artifact link as</param>
		/// <param name="artifactLinkId">The id of the particular artifact link we want to retrieve</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>A single dataitem object</returns>
		public SortedDataItem SortedList_Refresh(int projectId, int artifactLinkId, int? displayTypeId)
		{
			const string METHOD_NAME = "Refresh";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			if (!displayTypeId.HasValue)
			{
				throw new ArgumentException("You need to specify a display type");
			}
			DataModel.Artifact.DisplayTypeEnum displayType = (DataModel.Artifact.DisplayTypeEnum)displayTypeId.Value;

			if (artifactLinkId < 0)
			{
				//Real associations are positive
				throw new ArgumentException("You need to specify a non-negative artifactLinkId");
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Create the data item record (no filter items)
				//Right now only Association panel (Artifact Link display mode) is editable
				SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, DataModel.Artifact.DisplayTypeEnum.ArtifactLink, dataItem, null);

				//See if we have an Artifact Link or a Source Code Association
				ArtifactLink artifactLink;
				if (displayType == Artifact.DisplayTypeEnum.SourceCodeRevision_Associations)
				{
					//Get the artifact link view record for the specific artifact link id
					SourceCodeManager sourceCodeManager = new SourceCodeManager();
					artifactLink = sourceCodeManager.RetrieveRevisionAssociation(projectId, artifactLinkId);
				}
				else if (displayType == Artifact.DisplayTypeEnum.SourceCodeFile_Associations)
				{
					//Get the artifact link view record for the specific artifact link id
					SourceCodeManager sourceCodeManager = new SourceCodeManager();
					artifactLink = sourceCodeManager.RetrieveFileAssociation(projectId, artifactLinkId);
				}
				else
				{
					//Get the artifact link view record for the specific artifact link id
					ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
					artifactLink = artifactLinkManager.RetrieveById(artifactLinkId);
				}

				//Populate the list of linked projects for the project field
				List<Project> projects = new ProjectManager().ProjectAssociation_RetrieveForDestProjectIncludingSelf(projectId);

				//Finally populate the dataitem from the dataset
				if (artifactLink != null)
				{
					PopulateArtifactLinkRow(projectId, dataItem, artifactLink, projects);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItem;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Deletes a set of artifact links (not the artifact itself)
		/// </summary>
		/// <param name="displayTypeId">The display type/mode</param>
		/// <param name="standardFilters">Any standard filters</param>
		/// <param name="items">The items to delete</param>
		/// <param name="projectId">The current project</param>
		public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that they've specified the artifact id and type in the standard filters
			if (standardFilters == null)
			{
				throw new ArgumentException("You need to specify an artifact id and artifact type");
			}
			if (!standardFilters.ContainsKey("ArtifactId"))
			{
				throw new ArgumentException("You need to specify an artifact id");
			}
			if (!standardFilters.ContainsKey("ArtifactType"))
			{
				throw new ArgumentException("You need to specify an artifact type");
			}
			if (!displayTypeId.HasValue)
			{
				throw new ArgumentException("You need to specify a display type");
			}

			//Get the artifact type, id, and association panel type from the filters
			int artifactTypeId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ArtifactType"]);
			DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;
			int artifactId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ArtifactId"]);
			DataModel.Artifact.DisplayTypeEnum displayType = (DataModel.Artifact.DisplayTypeEnum)displayTypeId.Value;

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized for this project, we need to be able to modify the artifact type
			//Limited view is OK as long as we own the 'source' artifact
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, (Artifact.ArtifactTypeEnum)artifactTypeId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				//If this is a source code revision, the authorization depends on what we're linking TO (destination)
				//so allow all roles to make the call, but check what is removed
				if (artifactTypeId != (int)Artifact.ArtifactTypeEnum.SourceCodeRevision && artifactTypeId != (int)Artifact.ArtifactTypeEnum.SourceCodeFile)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}
			}

			try
			{
				//Make sure we own/created it if we have Limited Modify permissions
				if (authorizationState == Project.AuthorizationState.Limited)
				{
					ArtifactInfo artifactInfo = new ArtifactManager().RetrieveArtifactInfo((Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, projectId);
					if (artifactInfo == null || (artifactInfo.OwnerId != userId && artifactInfo.CreatorId != userId))
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}
				}

				//See what type of artifact we're associated FROM
				List<HierarchicalItem> dataItems = new List<HierarchicalItem>();
				switch ((Artifact.ArtifactTypeEnum)artifactTypeId)
				{
					case Artifact.ArtifactTypeEnum.Requirement:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.ArtifactLink)
							{
								ArtifactLinkManager artifactLink = new ArtifactLinkManager();
								foreach (string itemValue in items)
								{
									//Get the artifact link id
									int artifactLinkId = Int32.Parse(itemValue);
									artifactLink.Delete(artifactLinkId);
								}
							}
							if (displayType == Artifact.DisplayTypeEnum.Requirement_TestCases)
							{
								List<int> testCaseIds = new List<int>();
								foreach (string item in items)
								{
									int testCaseId;
									if (Int32.TryParse(item, out testCaseId))
									{
										testCaseIds.Add(testCaseId);
									}
								}
								if (testCaseIds.Count > 0)
								{
									TestCaseManager testCaseManager = new TestCaseManager();
									testCaseManager.RemoveFromRequirement(projectId, artifactId, testCaseIds, userId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Release:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.Release_TestCases)
							{
								List<int> testCaseIds = new List<int>();
								foreach (string item in items)
								{
									int testCaseId;
									if (Int32.TryParse(item, out testCaseId))
									{
										testCaseIds.Add(testCaseId);
									}
								}
								if (testCaseIds.Count > 0)
								{
									TestCaseManager testCaseManager = new TestCaseManager();
									testCaseManager.RemoveFromRelease(projectId, artifactId, testCaseIds, userId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestCase:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.TestCase_Requirements)
							{
								List<int> requirementIds = new List<int>();
								foreach (string item in items)
								{
									int requirementId;
									if (Int32.TryParse(item, out requirementId))
									{
										requirementIds.Add(requirementId);
									}
								}

								if (requirementIds.Count > 0)
								{
									//Instantiate the requirement business object
									RequirementManager requirementManager = new RequirementManager();

									//Remove the mappings
									requirementManager.RemoveFromTestCase(projectId, artifactId, requirementIds, userId);
								}
							}

							if (displayType == Artifact.DisplayTypeEnum.TestCase_Releases)
							{
								List<int> releaseIds = new List<int>();
								foreach (string item in items)
								{
									int releaseId;
									if (Int32.TryParse(item, out releaseId))
									{
										releaseIds.Add(releaseId);
									}
								}

								if (releaseIds.Count > 0)
								{
									//Instantiate the requirement business object
									ReleaseManager releaseManager = new ReleaseManager();

									//Remove the mappings
									releaseManager.RemoveFromTestCase(projectId, artifactId, releaseIds, userId);
								}
							}

							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.ArtifactLink)
							{
								ArtifactLinkManager artifactLink = new ArtifactLinkManager();
								foreach (string itemValue in items)
								{
									//Get the artifact link id
									int artifactLinkId = Int32.Parse(itemValue);
									artifactLink.Delete(artifactLinkId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestStep:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.TestStep_Requirements)
							{
								List<int> requirementIds = new List<int>();
								foreach (string item in items)
								{
									int requirementId;
									if (Int32.TryParse(item, out requirementId))
									{
										requirementIds.Add(requirementId);
									}
								}
								if (requirementIds.Count > 0)
								{
									RequirementManager requirementManager = new RequirementManager();
									requirementManager.RequirementTestStep_RemoveFromTestStep(projectId, artifactId, requirementIds);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Incident:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.ArtifactLink)
							{
								ArtifactLinkManager artifactLink = new ArtifactLinkManager();
								foreach (string itemValue in items)
								{
									//Get the artifact link id
									int artifactLinkId = Int32.Parse(itemValue);
									artifactLink.Delete(artifactLinkId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Task:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.ArtifactLink)
							{
								ArtifactLinkManager artifactLink = new ArtifactLinkManager();
								foreach (string itemValue in items)
								{
									//Get the artifact link id
									int artifactLinkId = Int32.Parse(itemValue);
									artifactLink.Delete(artifactLinkId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.TestRun:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.ArtifactLink)
							{
								ArtifactLinkManager artifactLink = new ArtifactLinkManager();
								foreach (string itemValue in items)
								{
									//Get the artifact link id
									int artifactLinkId = Int32.Parse(itemValue);
									artifactLink.Delete(artifactLinkId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Document:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.Attachments)
							{
								ArtifactLinkManager artifactLink = new ArtifactLinkManager();
								foreach (string itemValue in items)
								{
									//Get the artifact link id
									int artifactLinkId = Int32.Parse(itemValue);

									//Iterate through all the items to be deleted, we can't delete based on the artifact link id
									//as that's actually a synthetic id created from a composite of the real keys
									ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
									AttachmentManager attachmentManager = new AttachmentManager();
									List<ArtifactAttachmentView> artifactAttachments = artifactLinkManager.RetrieveByAttachmentId(projectId, artifactId);

									//See if we have a row with that synthetic artifact link id
									ArtifactAttachmentView linkRow = artifactAttachments.FirstOrDefault(a => a.ArtifactLinkId == artifactLinkId);
									if (linkRow != null)
									{
										//Delete the association between the document and the artifact
										attachmentManager.Delete(projectId, artifactId, linkRow.ArtifactId, (DataModel.Artifact.ArtifactTypeEnum)linkRow.ArtifactTypeId);
									}
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.Risk:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.ArtifactLink)
							{
								ArtifactLinkManager artifactLink = new ArtifactLinkManager();
								foreach (string itemValue in items)
								{
									//Get the artifact link id
									int artifactLinkId = Int32.Parse(itemValue);
									artifactLink.Delete(artifactLinkId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.SourceCodeRevision:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.SourceCodeRevision_Associations)
							{
								SourceCodeManager sourceCodeManager = new SourceCodeManager();
								foreach (string itemValue in items)
								{
									//Get the artifact source code ID
									int artifactSourceCodeId = Int32.Parse(itemValue);
									sourceCodeManager.RemoveRevisionAssociation(artifactSourceCodeId);
								}
							}
						}
						break;

					case Artifact.ArtifactTypeEnum.SourceCodeFile:
						{
							//See what mode we have
							if (displayType == Artifact.DisplayTypeEnum.SourceCodeFile_Associations)
							{
								SourceCodeManager sourceCodeManager = new SourceCodeManager();
								foreach (string itemValue in items)
								{
									//Get the artifact source code ID
									int artifactSourceCodeId = Int32.Parse(itemValue);
									sourceCodeManager.RemoveFileAssociation(artifactSourceCodeId);
								}
							}
						}
						break;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			throw new NotImplementedException();
		}
		public void SortedList_Copy(int projectId, List<string> items)
		{
			throw new NotImplementedException();
		}
		public void SortedList_Export(int destProjectId, List<string> items)
		{
			throw new NotImplementedException();
		}

		/* IFilteredListService methods */

		/// <summary>
		/// Get the appropriate settings name for the sort/filters depending on which type of association we're using
		/// </summary>
		/// <param name="displayType">What type of association we're displaying</param>
		/// <param name="projectSetting_Filters">The filters collection to use</param>
		/// <param name="projectSetting_Sort">The sort collection to use</param>
		/// <param name="defaultSortExpression">The default sort expression to use, if none found in settings</param>
		protected void GetSettingsCollections(Artifact.DisplayTypeEnum displayType, out string projectSetting_Filters, out string projectSetting_Sort, out string defaultSortExpression)
		{
			projectSetting_Filters = "";
			projectSetting_Sort = "";
			defaultSortExpression = "";

			switch (displayType)
			{
				case Artifact.DisplayTypeEnum.None:
				case Artifact.DisplayTypeEnum.ArtifactLink:
				case Artifact.DisplayTypeEnum.Build_Associations:
				case Artifact.DisplayTypeEnum.SourceCodeRevision_Associations:
				case Artifact.DisplayTypeEnum.SourceCodeFile_Associations:
					projectSetting_Filters = GlobalFunctions.PROJECT_SETTINGS_ASSOCIATIONS_ARTIFACT_FILTERS_LIST;
					projectSetting_Sort = GlobalFunctions.PROJECT_SETTINGS_ASSOCIATIONS_ARTIFACT_SORT_EXPRESSION;
					defaultSortExpression = "CreationDate DESC";
					break;

				case Artifact.DisplayTypeEnum.Requirement_TestCases:
					projectSetting_Filters = GlobalFunctions.PROJECT_SETTINGS_ASSOCIATIONS_TEST_CASE_COVERAGE_FILTERS_LIST;
					projectSetting_Sort = GlobalFunctions.PROJECT_SETTINGS_ASSOCIATIONS_TEST_CASE_COVERAGE_SORT_EXPRESSION;
					defaultSortExpression = "TestCaseId ASC";
					break;

				case Artifact.DisplayTypeEnum.TestCase_Requirements:
					projectSetting_Filters = GlobalFunctions.PROJECT_SETTINGS_ASSOCIATIONS_REQUIREMENT_COVERAGE_FILTERS_LIST;
					projectSetting_Sort = GlobalFunctions.PROJECT_SETTINGS_ASSOCIATIONS_REQUIREMENT_COVERAGE_SORT_EXPRESSION;
					defaultSortExpression = "RequirementId DESC";
					break;

				case Artifact.DisplayTypeEnum.TestStep_Requirements:
					projectSetting_Filters = GlobalFunctions.PROJECT_SETTINGS_ASSOCIATIONS_REQUIREMENT_COVERAGE_FILTERS_LIST;
					projectSetting_Sort = GlobalFunctions.PROJECT_SETTINGS_ASSOCIATIONS_REQUIREMENT_COVERAGE_SORT_EXPRESSION;
					defaultSortExpression = "RequirementId DESC";
					break;

				case Artifact.DisplayTypeEnum.TestCase_Releases:
					projectSetting_Filters = GlobalFunctions.PROJECT_SETTINGS_ASSOCIATIONS_RELEASE_COVERAGE_FILTERS_LIST;
					projectSetting_Sort = GlobalFunctions.PROJECT_SETTINGS_ASSOCIATIONS_RELEASE_COVERAGE_SORT_EXPRESSION;
					defaultSortExpression = "VersionNumber ASC";
					break;

				case Artifact.DisplayTypeEnum.Release_TestCases:
					projectSetting_Filters = GlobalFunctions.PROJECT_SETTINGS_RELEASE_DETAILS_TEST_CASES_FILTERS_LIST;
					projectSetting_Sort = GlobalFunctions.PROJECT_SETTINGS_RELEASE_DETAILS_TEST_CASES_GENERAL;
					defaultSortExpression = "TestCaseId ASC";
					break;

				case Artifact.DisplayTypeEnum.Attachments:
					projectSetting_Filters = GlobalFunctions.PROJECT_SETTINGS_ATTACHMENT_ASSOCIATIONS_FILTERS_LIST;
					projectSetting_Sort = GlobalFunctions.PROJECT_SETTINGS_ATTACHMENT_ASSOCIATIONS_GENERAL;
					defaultSortExpression = "CreationDate DESC";
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Updates the filters stored in the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="filters">The array of filters (name,value)</param>
		/// <returns>Any error messages</returns>
		public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
		{
			const string METHOD_NAME = "UpdateFilters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Make sure we have a display mode specified
				if (displayTypeId.HasValue)
				{
					DataModel.Artifact.DisplayTypeEnum displayType = (DataModel.Artifact.DisplayTypeEnum)displayTypeId.Value;

					string projectSetting_Filters;
					string projectSetting_Sort;
					string defaultSortExpression;
					GetSettingsCollections(displayType, out projectSetting_Filters, out projectSetting_Sort, out defaultSortExpression);

					//Get the current filters from settings
					ProjectSettingsCollection savedFilters = GetProjectSettings(userId, projectId, projectSetting_Filters);
					int oldFilterCount = savedFilters.Count;
					savedFilters.Clear(); //Clear the filters

					//Iterate through the filters, updating the project collection
					foreach (KeyValuePair<string, string> filter in filters)
					{
						string filterName = filter.Key;
						//Now get the type of field that we have. Since artifact links are not a true artifact,
						//these values have to be hardcoded, as they're not stored in the TST_ARTIFACT_FIELD table
						DataModel.Artifact.ArtifactFieldTypeEnum artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
						switch (filterName)
						{
							case "ProjectName":
							case "ArtifactName":
								artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
								break;

							case "CreatorId":
							case "ArtifactTypeId":
							case "ArtifactLinkTypeId":
							case "ExecutionStatusId":
							case "TestCaseStatusId":
							case "TestCaseTypeId":
							case "TestCasePriorityId":
							case "RequirementTypeId":
							case "RequirementStatusId":
							case "ImportanceId":
								artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
								break;

							case "CreationDate":
							case "ExecutionDate":
								artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
								break;

							case "Comment":
							case "ArtifactStatusName":
								artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
								break;

							case "ArtifactId":
								artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
								break;
						}

						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Lookup)
						{
							//Need to make sure that they are MultiValueFilter classes
							MultiValueFilter multiValueFilter;
							if (MultiValueFilter.TryParse(filter.Value, out multiValueFilter))
							{
								savedFilters.Add(filterName, multiValueFilter);
							}
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Identifier)
						{
							//All identifiers must be numeric
							int filterValueInt = -1;
							if (Int32.TryParse(filter.Value, out filterValueInt))
							{
								savedFilters.Add(filterName, filterValueInt);
							}
							else
							{
								return "You need to enter a valid integer value for '" + filterName + "'";
							}
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.DateTime)
						{
							//If we have date values, need to make sure that they are indeed dates
							//Otherwise we need to throw back a friendly error message
							DateRange dateRange;
							if (!DateRange.TryParse(filter.Value, out dateRange))
							{
								return "You need to enter a valid date-range value for '" + filterName + "'";
							}
							savedFilters.Add(filterName, dateRange);
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Integer)
						{
							//If we have integer values, need to make sure that they are indeed integral
							Int32 intValue;
							if (!Int32.TryParse(filter.Value, out intValue))
							{
								return "You need to enter a valid integer value for '" + filterName + "'";
							}
							savedFilters.Add(filterName, intValue);
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Text || artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription)
						{
							//For text, just save the value
							savedFilters.Add(filterName, filter.Value);
						}
					}
					savedFilters.Save();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return "";  //Success
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
		{
			throw new NotImplementedException();
		}
		public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
		{
			throw new NotImplementedException();
		}

		/* IListService methods */

		/// <summary>
		/// Returns a list of pagination options that the user can choose from
		/// </summary>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		public JsonDictionaryOfStrings RetrievePaginationOptions(int projectId)
		{
			const string METHOD_NAME = "RetrievePaginationOptions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Delegate to the generic method in the base class - passing the correct collection name
			JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_ARTIFACT_LINKS_PAGINATION);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return paginationDictionary;
		}

		/// <summary>
		/// Updates the size of pages returned and the currently selected page
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
		/// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
		public void UpdatePagination(int projectId, int pageSize, int currentPage)
		{
			const string METHOD_NAME = "UpdatePagination";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the pagination settings collection and update
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_ARTIFACT_LINKS_PAGINATION);
				paginationSettings.Restore();
				if (pageSize != -1)
				{
					paginationSettings["NumberRowsPerPage"] = pageSize;
				}
				if (currentPage != -1)
				{
					paginationSettings["CurrentPage"] = currentPage;
				}
				paginationSettings.Save();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Displays the name and description (if available) of the item
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="displayTypeId">The display type</param>
		/// <returns>The tooltip</returns>
		public string RetrieveNameDesc(int? projectId, int artifactId, int? displayTypeId)
		{
			const string METHOD_NAME = "RetrieveNameDesc";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//See the display mode
				if (displayTypeId.HasValue)
				{
					if (displayTypeId == (int)DataModel.Artifact.DisplayTypeEnum.TestCase_Releases)
					{
						try
						{
							ReleaseManager releaseManager = new ReleaseManager();
							ReleaseView release = releaseManager.RetrieveById2(projectId, artifactId);
							string tooltip;
							if (String.IsNullOrEmpty(release.Description))
							{
								tooltip = Microsoft.Security.Application.Encoder.HtmlEncode(release.FullName);
							}
							else
							{
								tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(release.FullName) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(release.Description);
							}
							Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
							Logger.Flush();
							return tooltip;
						}
						catch (ArtifactNotExistsException)
						{
							//This is the case where the client still displays the test case, but it has already been deleted on the server
							Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Global_UnableRetrieveTooltip);
							Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
							Logger.Flush();
							return Resources.Messages.Global_UnableRetrieveTooltip;
						}
					}
					if (displayTypeId == (int)DataModel.Artifact.DisplayTypeEnum.TestCase_Requirements || displayTypeId == (int)DataModel.Artifact.DisplayTypeEnum.TestStep_Requirements)
					{
						try
						{
							RequirementManager requirementManager = new RequirementManager();
							string tooltip = "";
							RequirementView requirement = requirementManager.RetrieveById2(null, artifactId);
							if (String.IsNullOrEmpty(requirement.Description))
							{
								tooltip = Microsoft.Security.Application.Encoder.HtmlEncode(requirement.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, artifactId, true);
							}
							else
							{
								tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(requirement.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, artifactId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(requirement.Description);
							}

							//See if we have any comments to append
							IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(artifactId, Artifact.ArtifactTypeEnum.Requirement, false);
							if (comments.Count() > 0)
							{
								IDiscussion lastComment = comments.Last();
								tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
									GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
									GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
									Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
									);
							}

							Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
							Logger.Flush();
							return tooltip;
						}
						catch (ArtifactNotExistsException)
						{
							//This is the case where the client still displays the test case, but it has already been deleted on the server
							Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Global_UnableRetrieveTooltip);
							Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
							Logger.Flush();
							return Resources.Messages.Global_UnableRetrieveTooltip;
						}
					}

					if (displayTypeId == (int)DataModel.Artifact.DisplayTypeEnum.Requirement_TestCases || displayTypeId == (int)DataModel.Artifact.DisplayTypeEnum.Release_TestCases)
					{
						//Instantiate the test case business object
						TestCaseManager testCaseManager = new TestCaseManager();

						//Now retrieve the specific test case/folder - handle quietly if it doesn't exist
						try
						{
							//The artifact Id is the test case
							int testCaseId = artifactId;
							string tooltip = "";
							//First we need to get the test case itself
							TestCaseView testCaseView = testCaseManager.RetrieveById(null, testCaseId);

							//Next we need to get the list of successive parent folders
							if (testCaseView.TestCaseFolderId.HasValue)
							{
								List<TestCaseFolderHierarchyView> parentFolders = testCaseManager.TestCaseFolder_GetParents(testCaseView.ProjectId, testCaseView.TestCaseFolderId.Value, true);
								foreach (TestCaseFolderHierarchyView parentFolder in parentFolders)
								{
									tooltip += "<u>" + parentFolder.Name + "</u> &gt; ";
								}
							}

							//Now we need to get the test case itself
							tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testCaseView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE, testCaseId, true) + "</u>";
							if (!String.IsNullOrEmpty(testCaseView.Description))
							{
								tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testCaseView.Description);
							}

							//See if we have any comments to append
							IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(testCaseId, Artifact.ArtifactTypeEnum.TestCase, false);
							if (comments.Count() > 0)
							{
								IDiscussion lastComment = comments.Last();
								tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
									GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
									GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
									Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
									);
							}

							Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
							Logger.Flush();
							return tooltip;
						}
						catch (ArtifactNotExistsException)
						{
							//This is the case where the client still displays the test case, but it has already been deleted on the server
							Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for test case");
							Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
							Logger.Flush();
							return Resources.Messages.Global_UnableRetrieveTooltip;
						}
					}

					if (displayTypeId == (int)DataModel.Artifact.DisplayTypeEnum.Attachments)
					{
						//We don't have a way to display the tooltips since we don't know the attachment id
						return null;
					}

					if (displayTypeId == (int)DataModel.Artifact.DisplayTypeEnum.Build_Associations)
					{
						//We don't have a way to display the tooltips since there is no actual primary key and we don't know the build id
						return null;
					}

					if (displayTypeId == (int)Artifact.DisplayTypeEnum.SourceCodeFile_Associations)
					{
						//Get the artifact link view record for the specific artifact link id
						SourceCodeManager sourceCodeManager = new SourceCodeManager();
						ArtifactLink artifactLink = sourceCodeManager.RetrieveFileAssociation(projectId.Value, artifactId);

						//Get the artifact name
						ArtifactManager artifactManager = new ArtifactManager();
						ArtifactInfo artifactInfo = artifactManager.RetrieveArtifactInfo((Artifact.ArtifactTypeEnum)artifactLink.DestArtifactTypeId, artifactLink.DestArtifactId, projectId);
						if (artifactInfo != null)
						{
							return "<u>" + artifactInfo.Name + " [" + artifactInfo.ArtifactToken + "]</u><br />" + GlobalFunctions.HtmlRenderAsPlainText(artifactInfo.Description) + "<br /><i>" + artifactLink.Comment + "</i>";
						}
					}

					if (displayTypeId == (int)Artifact.DisplayTypeEnum.SourceCodeRevision_Associations)
					{
						//We can only retrieve tooltips for the ones that are added inside Spira, not via. artifact tokens
						if (artifactId > 0)
						{
							//Get the artifact link view record for the specific artifact link id
							SourceCodeManager sourceCodeManager = new SourceCodeManager();
							ArtifactLink artifactLink = sourceCodeManager.RetrieveRevisionAssociation(projectId.Value, artifactId);

							//Get the artifact name
							ArtifactManager artifactManager = new ArtifactManager();
							ArtifactInfo artifactInfo = artifactManager.RetrieveArtifactInfo((Artifact.ArtifactTypeEnum)artifactLink.DestArtifactTypeId, artifactLink.DestArtifactId, projectId);
							if (artifactInfo != null)
							{
								return "<u>" + artifactInfo.Name + " [" + artifactInfo.ArtifactToken + "]</u><br />" + GlobalFunctions.HtmlRenderAsPlainText(artifactInfo.Description) + "<br /><i>" + artifactLink.Comment + "</i>";
							}
						}
					}

					if (displayTypeId == (int)DataModel.Artifact.DisplayTypeEnum.ArtifactLink)
					{
						//Make sure the artifact id is positve, otherwise it's source code
						string tooltip = "";
						if (artifactId > 0)
						{
							try
							{
								int artifactLinkId = artifactId;
								ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
								ArtifactLink artifactLink = artifactLinkManager.RetrieveById(artifactLinkId);
								ArtifactManager artifactManager = new ArtifactManager();
								ArtifactInfo sourceArtifactInfo = artifactManager.RetrieveArtifactInfo((Artifact.ArtifactTypeEnum)artifactLink.SourceArtifactTypeId, artifactLink.SourceArtifactId, null);
								ArtifactInfo destArtifactInfo = artifactManager.RetrieveArtifactInfo((Artifact.ArtifactTypeEnum)artifactLink.DestArtifactTypeId, artifactLink.DestArtifactId, null);

								if (sourceArtifactInfo != null && destArtifactInfo != null)
								{
									tooltip = "<u>" + sourceArtifactInfo.Name + " [" + sourceArtifactInfo.ArtifactToken + "]</u>\n";
									if (String.IsNullOrEmpty(sourceArtifactInfo.Description))
									{
										tooltip += "<br />";
									}
									else
									{
										tooltip += "<p>" + GlobalFunctions.HtmlRenderAsPlainText(sourceArtifactInfo.Description) + "</p>\n";
									}
									tooltip += "<u><span class=\"fas fa-arrow-right\"></span> " + destArtifactInfo.Name + " [" + destArtifactInfo.ArtifactToken + "]</u>\n";
									if (!String.IsNullOrEmpty(destArtifactInfo.Description))
									{
										tooltip += "<p>" + GlobalFunctions.HtmlRenderAsPlainText(destArtifactInfo.Description) + "</p>\n";
									}
								}
							}
							catch (ArtifactNotExistsException)
							{
								tooltip = Resources.Messages.Global_UnableRetrieveTooltip;
							}
						}

						return tooltip;
					}
				}

				return Resources.Messages.Global_UnableRetrieveTooltip;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public void ToggleColumnVisibility(int projectId, string fieldName)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Internal Functions

		/// <summary>
		/// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the artifact links as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		/// <param name="displayType">The type of association panel we are looking to populate - affecting what elements to create in the shape here</param>
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, DataModel.Artifact.DisplayTypeEnum displayType, SortedDataItem dataItem, Hashtable filterList)
		{
			//We need to add the various artifact link fields to be displayed
			DataItemField dataItemField;
			if (displayType == DataModel.Artifact.DisplayTypeEnum.ArtifactLink)
			{
				//Link Type (needs to come before artifact type because otherwise was confusing to read and know which was dependent on which)
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ArtifactLinkTypeId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "ArtifactLinkTypeName";
				dataItemField.Caption = Resources.Fields.Type;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}

			if (displayType == DataModel.Artifact.DisplayTypeEnum.ArtifactLink || displayType == Artifact.DisplayTypeEnum.Attachments ||
				displayType == Artifact.DisplayTypeEnum.Build_Associations || displayType == Artifact.DisplayTypeEnum.SourceCodeRevision_Associations ||
				displayType == Artifact.DisplayTypeEnum.SourceCodeFile_Associations)
			{
				//Artifact type
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ArtifactTypeId";
				dataItemField.Caption = Resources.Fields.ArtifactTypeId;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "ArtifactTypeName";
				dataItemField.AllowDragAndDrop = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}

			if (displayType == DataModel.Artifact.DisplayTypeEnum.Requirement_TestCases || displayType == DataModel.Artifact.DisplayTypeEnum.Release_TestCases)
			{
				//Test Case Type
				dataItemField = new DataItemField();
				dataItemField.FieldName = "TestCaseTypeId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.Caption = Resources.Fields.Type;
				dataItemField.LookupName = "TestCaseTypeName";
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}

			if (displayType == DataModel.Artifact.DisplayTypeEnum.TestStep_Requirements || displayType == DataModel.Artifact.DisplayTypeEnum.TestCase_Requirements)
			{
				//Requirement Type
				dataItemField = new DataItemField();
				dataItemField.FieldName = "RequirementTypeId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.Caption = Resources.Fields.Type;
				dataItemField.LookupName = "RequirementTypeName";
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}

			//Artifact Name
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ArtifactName";
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
			dataItemField.Caption = Resources.Fields.Name;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
			}

			if (displayType == DataModel.Artifact.DisplayTypeEnum.ArtifactLink || displayType == Artifact.DisplayTypeEnum.Attachments)
			{
				//Status Name
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ArtifactStatusName";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
				dataItemField.Caption = Resources.Fields.Status;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
				}
			}

			if (displayType == DataModel.Artifact.DisplayTypeEnum.ArtifactLink || displayType == Artifact.DisplayTypeEnum.Attachments ||
				displayType == Artifact.DisplayTypeEnum.Build_Associations || displayType == Artifact.DisplayTypeEnum.SourceCodeRevision_Associations ||
				displayType == Artifact.DisplayTypeEnum.SourceCodeFile_Associations)
			{
				//Creation Date
				dataItemField = new DataItemField();
				dataItemField.FieldName = "CreationDate";
				dataItemField.Caption = Resources.Fields.CreationDate;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
				dataItemField.AllowDragAndDrop = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					//Need to convert into the displayable date form
					DateRange dateRange = (DateRange)filterList[dataItemField.FieldName];
					string textValue = "";
					if (dateRange.StartDate.HasValue)
					{
						textValue += String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.StartDate.Value);
					}
					textValue += "|";
					if (dateRange.EndDate.HasValue)
					{
						textValue += String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.EndDate.Value);
					}
					dataItemField.TextValue = textValue;
				}

				//Author / Uploaded By
				dataItemField = new DataItemField();
				dataItemField.FieldName = "CreatorId";
				dataItemField.Caption = Resources.Fields.CreatorId;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "CreatorName";
				dataItemField.AllowDragAndDrop = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}

			if (displayType == DataModel.Artifact.DisplayTypeEnum.ArtifactLink || displayType == Artifact.DisplayTypeEnum.Build_Associations ||
				displayType == Artifact.DisplayTypeEnum.SourceCodeRevision_Associations || displayType == Artifact.DisplayTypeEnum.SourceCodeFile_Associations)
			{
				//Comment
				dataItemField = new DataItemField();
				dataItemField.FieldName = "Comment";
				dataItemField.Caption = Resources.Fields.Comment;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
				dataItemField.AllowDragAndDrop = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
				}
			}

			if (displayType == DataModel.Artifact.DisplayTypeEnum.TestCase_Releases)
			{
				//Release Type
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ReleaseTypeId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.Caption = Resources.Fields.Type;
				dataItemField.LookupName = "ReleaseTypeName";
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}

			if (displayType == DataModel.Artifact.DisplayTypeEnum.Requirement_TestCases || displayType == DataModel.Artifact.DisplayTypeEnum.Release_TestCases)
			{
				//Test Case  Status
				dataItemField = new DataItemField();
				dataItemField.FieldName = "TestCaseStatusId";
				dataItemField.Caption = Resources.Fields.Status;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "TestCaseStatusName";
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				dataItemField.AllowDragAndDrop = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}

				//Test Case Execution Status
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ExecutionStatusId";
				dataItemField.Caption = Resources.Fields.ExecutionStatus;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "ExecutionStatusName";
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				dataItemField.AllowDragAndDrop = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}

				//Last Executed Date
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ExecutionDate";
				dataItemField.Caption = Resources.Fields.LastExecuted;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
				dataItemField.AllowDragAndDrop = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					//Need to convert into the displayable date form
					DateRange dateRange = (DateRange)filterList[dataItemField.FieldName];
					string textValue = "";
					if (dateRange.StartDate.HasValue)
					{
						textValue += String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.StartDate.Value);
					}
					textValue += "|";
					if (dateRange.EndDate.HasValue)
					{
						textValue += String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.EndDate.Value);
					}
					dataItemField.TextValue = textValue;
				}

				//Priority
				dataItemField = new DataItemField();
				dataItemField.FieldName = "TestCasePriorityId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.Caption = Resources.Fields.Priority;
				dataItemField.LookupName = "TestCasePriorityName";
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}

			if (displayType == DataModel.Artifact.DisplayTypeEnum.TestStep_Requirements || displayType == DataModel.Artifact.DisplayTypeEnum.TestCase_Requirements)
			{
				//Requirement  Status
				dataItemField = new DataItemField();
				dataItemField.FieldName = "RequirementStatusId";
				dataItemField.Caption = Resources.Fields.Status;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "RequirementStatusName";
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				dataItemField.AllowDragAndDrop = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}

				//Importance
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ImportanceId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.Caption = Resources.Fields.Importance;
				dataItemField.LookupName = "ImportanceName";
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}

			if (displayType == DataModel.Artifact.DisplayTypeEnum.TestCase_Releases)
			{
				//Version Number
				dataItemField = new DataItemField();
				dataItemField.FieldName = "VersionNumber";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
				dataItemField.Caption = Resources.Fields.VersionNumber;
				//Set the list of possible lookup values
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is String)
					{
						dataItemField.TextValue = ((String)filterList[dataItemField.FieldName]);
					}
				}

				//Release  Status
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ReleaseStatusId";
				dataItemField.Caption = Resources.Fields.Status;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "ReleaseStatusName";
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				dataItemField.AllowDragAndDrop = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}

			if (displayType != Artifact.DisplayTypeEnum.Attachments && displayType != Artifact.DisplayTypeEnum.Build_Associations && displayType != Artifact.DisplayTypeEnum.SourceCodeRevision_Associations && displayType != Artifact.DisplayTypeEnum.SourceCodeFile_Associations)
			{
				//Artifact Project
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ProjectId";  //We don't send a LookupName because we'll populate separately
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.Caption = Resources.Fields.ProjectName;
				//Set the list of possible lookup values
				dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					int intValue;
					if (filterList[dataItemField.FieldName] is String && Int32.TryParse((string)filterList[dataItemField.FieldName], out intValue))
					{
						dataItemField.TextValue = intValue.ToString();
					}
					if (filterList[dataItemField.FieldName] is Int32)
					{
						dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
					}
					if (filterList[dataItemField.FieldName] is MultiValueFilter)
					{
						dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}

			//Artifact ID
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ArtifactId";
			dataItemField.Caption = Resources.Fields.ID;
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
			dataItemField.AllowDragAndDrop = true;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				dataItemField.IntValue = (int)filterList[dataItemField.FieldName];
			}
		}

		/// <summary>
		/// Gets the list of lookup values and names for a specific lookup
		/// </summary>
		/// <param name="lookupName">The name of the lookup</param>
		/// <param name="projectId">The id of the project - needed for some lookups</param>
		/// <returns>The name/value pairs</returns>
		protected JsonDictionaryOfStrings GetLookupValues(string lookupName, int projectId, int projectTemplateId)
		{
			const string METHOD_NAME = "GetLookupValues";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				JsonDictionaryOfStrings lookupValues = null;
				if (lookupName == "CreatorId")
				{
					List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
					lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
				}
				if (lookupName == "ArtifactTypeId")
				{
					List<ArtifactType> artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll();
					lookupValues = ConvertLookupValues(artifactTypes.OfType<DataModel.Entity>().ToList(), "ArtifactTypeId", "Name");
				}
				if (lookupName == "ArtifactLinkTypeId")
				{
					//Include the inactive types so that we can filter by source code/implicit
					List<ArtifactLinkType> artifactLinkTypes = new ArtifactLinkManager().RetrieveLinkTypes(true);
					lookupValues = ConvertLookupValues(artifactLinkTypes.OfType<DataModel.Entity>().ToList(), "ArtifactLinkTypeId", "Name");
				}
				if (lookupName == "TestCaseStatusId")
				{
					List<TestCaseStatus> testCaseStatus = new TestCaseManager().RetrieveStatuses();
					lookupValues = ConvertLookupValues(testCaseStatus.OfType<DataModel.Entity>().ToList(), "TestCaseStatusId", "Name");
				}
				if (lookupName == "ExecutionStatusId")
				{
					List<ExecutionStatus> executionStatus = new TestCaseManager().RetrieveExecutionStatuses();
					lookupValues = ConvertLookupValues(executionStatus.OfType<DataModel.Entity>().ToList(), "ExecutionStatusId", "Name");
				}
				if (lookupName == "TestCasePriorityId")
				{
					List<TestCasePriority> testCasePriority = new TestCaseManager().TestCasePriority_Retrieve(projectTemplateId);
					lookupValues = ConvertLookupValues(testCasePriority.OfType<DataModel.Entity>().ToList(), "TestCasePriorityId", "Name");
				}
				if (lookupName == "TestCaseTypeId")
				{
					List<TestCaseType> testCaseType = new TestCaseManager().TestCaseType_Retrieve(projectTemplateId);
					lookupValues = ConvertLookupValues(testCaseType.OfType<DataModel.Entity>().ToList(), "TestCaseTypeId", "Name");
				}
				if (lookupName == "ProjectId")
				{
					List<Project> projects = new ProjectManager().ProjectAssociation_RetrieveForDestProjectIncludingSelf(projectId);
					lookupValues = ConvertLookupValues(projects.OfType<DataModel.Entity>().ToList(), "ProjectId", "Name");
				}
				if (lookupName == "RequirementStatusId")
				{
					List<RequirementStatus> requirementStatus = new RequirementManager().RetrieveStatuses();
					lookupValues = ConvertLookupValues(requirementStatus.OfType<DataModel.Entity>().ToList(), "RequirementStatusId", "Name");
				}
				if (lookupName == "ReleaseStatusId")
				{
					List<ReleaseStatus> releaseStati = new ReleaseManager().RetrieveStatuses();
					lookupValues = ConvertLookupValues(releaseStati.OfType<DataModel.Entity>().ToList(), "ReleaseStatusId", "Name");
				}
				if (lookupName == "ImportanceId")
				{
					List<Importance> importances = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId);
					lookupValues = ConvertLookupValues(importances.OfType<DataModel.Entity>().ToList(), "ImportanceId", "Name");
				}
				if (lookupName == "RequirementTypeId")
				{
					List<RequirementType> requirementTypes = new RequirementManager().RequirementType_Retrieve(projectTemplateId, true);
					lookupValues = ConvertLookupValues(requirementTypes.OfType<DataModel.Entity>().ToList(), "RequirementTypeId", "Name");
				}
				if (lookupName == "ReleaseTypeId")
				{
					List<ReleaseType> releaseTypes = new ReleaseManager().RetrieveTypes();
					lookupValues = ConvertLookupValues(releaseTypes.OfType<DataModel.Entity>().ToList(), "ReleaseTypeId", "Name");
				}

				return lookupValues;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Populates a data item from a dataset datarow
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="artifactLink">The datarow containing the data</param>
		/// <param name="projects">The list of linked projects</param>
		/// <param name="sourceProjects">The list of projects being linked from</param>
		protected void PopulateArtifactLinkRow(int projectId, SortedDataItem dataItem, ArtifactLinkView artifactLink, List<Project> projects, List<Project> sourceProjects = null)
		{
			//Set the primary key and the URL which varies by artifact unlike most grids
			UrlRoots.NavigationLinkEnum artType = (UrlRoots.NavigationLinkEnum)artifactLink.ArtifactTypeId;
			int artifactId = artifactLink.ArtifactId;

			dataItem.PrimaryKey = artifactLink.ArtifactLinkId;
			//Need to handle the special case of revisions which are not real artifacts
			if (artifactLink.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.SourceCodeRevision)
			{
				dataItem.PrimaryKey = -1;
				dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeRedirect.aspx?" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_SESSION_ID + "=" + artifactId);
			}
			else
			{
				if (artType == UrlRoots.NavigationLinkEnum.Builds)
				{
					//Build links are not editable so set primary key to - 1
					dataItem.PrimaryKey = -1;
				}
				dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(artType, projectId, artifactId, GlobalFunctions.PARAMETER_TAB_ASSOCIATION));
			}

			//Artifact Links don't have an attachment flag
			dataItem.Attachment = false;

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (artifactLink.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the data-row
					string artifactPrefix = GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum)artifactLink.ArtifactTypeId);
					PopulateFieldRow(dataItem, dataItemField, artifactLink, null, (DataModel.ArtifactCustomProperty)null, false, null);

					//The only editable fields are comment and type
					dataItemField.Editable = (dataItemField.FieldName == "Comment" || dataItemField.FieldName == "ArtifactLinkTypeId");
					dataItemField.Required = (dataItemField.FieldName == "ArtifactLinkTypeId");

					//If this is the ID field, need to populate manually because ArtifactLinkViews do not inherit from Artifact
					if (dataItemField.FieldName == "ArtifactId")
					{
						dataItemField.TextValue = GlobalFunctions.GetTokenForArtifact(artifactPrefix, artifactLink.ArtifactId, true);

						if (artifactLink.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.SourceCodeRevision)
						{
							//The revision is a special type of 'pseudo artifact'
							dataItemField.TextValue = "-";
						}
					}

					//For the project id field, populate the display name from the project list retrieved separately
					//We try the destination project list before trying the optional source project list
					if (dataItemField.FieldName == "ProjectId")
					{
						Project project = projects.FirstOrDefault(p => p.ProjectId == artifactLink.ProjectId);
						if (project != null)
						{
							dataItemField.TextValue = project.Name;
						}
						else if (sourceProjects != null)
						{
							project = sourceProjects.FirstOrDefault(p => p.ProjectId == artifactLink.ProjectId);
							if (project != null)
							{
								dataItemField.TextValue = project.Name;
							}
						}
					}

					//If we have the name/desc field then we need to set the image to the appropriate artifact type
					//which is passed in the tooltip field
					if (dataItemField.FieldName == "ArtifactName")
					{
						//The revision is a special type of 'pseudo artifact'
						if (artifactLink.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.SourceCodeRevision)
						{
							dataItemField.Tooltip = "artifact-Revision.svg";
						}
						else
						{
							dataItemField.Tooltip = GlobalFunctions.GetIconForArtifactType((DataModel.Artifact.ArtifactTypeEnum)artifactLink.ArtifactTypeId);
						}
					}
				}
			}
		}

		/// <summary>
		/// Populates a data item from a dataset datarow
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="artifactAttachment">The datarow containing the data</param>
		protected void PopulateAttachmentArtifactRow(int projectId, SortedDataItem dataItem, ArtifactAttachmentView artifactAttachment)
		{
			//Set the primary key and the URL which varies by artifact unlike most grids
			UrlRoots.NavigationLinkEnum artType = (UrlRoots.NavigationLinkEnum)artifactAttachment.ArtifactTypeId;
			int artifactId = artifactAttachment.ArtifactId;

			dataItem.PrimaryKey = artifactAttachment.ArtifactLinkId.Value;
			dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(artType, projectId, artifactId, GlobalFunctions.PARAMETER_TAB_ASSOCIATION));

			//Artifact Links don't have an attachment flag
			//dataItem.Attachment = false;

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (artifactAttachment.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the data-row
					string artifactPrefix = GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum)artifactAttachment.ArtifactTypeId);
					PopulateFieldRow(dataItem, dataItemField, artifactAttachment, null, (DataModel.ArtifactCustomProperty)null, false, null);

					//If this is the ID field, need to populate manually because ArtifactLinkViews do not inherit from Artifact
					if (dataItemField.FieldName == "ArtifactId")
					{
						dataItemField.TextValue = GlobalFunctions.GetTokenForArtifact(artifactPrefix, artifactAttachment.ArtifactId, true);
					}

					//If we have the name/desc field then we need to set the image to the appropriate artifact type
					//which is passed in the tooltip field
					if (dataItemField.FieldName == "ArtifactName")
					{
						dataItemField.Tooltip = GlobalFunctions.GetIconForArtifactType((DataModel.Artifact.ArtifactTypeEnum)artifactAttachment.ArtifactTypeId);
					}
				}
			}
		}

		/// <summary>
		/// Populates a data item from a dataset datarow
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="artifactLink">The datarow containing the data</param>
		/// <param name="projects">The list of linked projects</param>
		protected void PopulateArtifactLinkRow(int projectId, SortedDataItem dataItem, ArtifactLink artifactLink, List<Project> projects)
		{
			//Set the primary key and the URL which varies by artifact unlike most grids
			UrlRoots.NavigationLinkEnum artType = (UrlRoots.NavigationLinkEnum)artifactLink.DestArtifactTypeId;
			int artifactId = artifactLink.ArtifactId;

			dataItem.PrimaryKey = artifactLink.ArtifactLinkId;
			//Need to handle the special case of revisions which are not real artifacts
			if (artifactLink.DestArtifactTypeId == (int)Artifact.ArtifactTypeEnum.SourceCodeRevision)
			{
				dataItem.PrimaryKey = -1;
				dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeRedirect.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_SESSION_ID + "=" + artifactId);
			}
			else
			{
				dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(artType, projectId, artifactId, GlobalFunctions.PARAMETER_TAB_ASSOCIATION));
			}

			//Artifact Links don't have an attachment flag
			dataItem.Attachment = false;

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (artifactLink.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the data-row
					string artifactPrefix = GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum)artifactLink.DestArtifactTypeId);
					PopulateFieldRow(dataItem, dataItemField, artifactLink, null, (DataModel.ArtifactCustomProperty)null, false, null);

					//The only editable fields are comment and type
					dataItemField.Editable = (dataItemField.FieldName == "Comment" || dataItemField.FieldName == "ArtifactLinkTypeId");
					dataItemField.Required = (dataItemField.FieldName == "ArtifactLinkTypeId");

					//If this is the ID field, need to populate manually because ArtifactLinkViews do not inherit from Artifact
					if (dataItemField.FieldName == "ArtifactId")
					{
						dataItemField.TextValue = GlobalFunctions.GetTokenForArtifact(artifactPrefix, artifactLink.ArtifactId, true);

						if (artifactLink.DestArtifactTypeId == (int)Artifact.ArtifactTypeEnum.SourceCodeRevision)
						{
							//The revision is a special type of 'pseudo artifact'
							dataItemField.TextValue = "-";
						}
					}

					//If we have the name/desc field then we need to set the image to the appropriate artifact type
					//which is passed in the tooltip field
					if (dataItemField.FieldName == "ArtifactName")
					{
						//The revision is a special type of 'pseudo artifact'
						if (artifactLink.DestArtifactTypeId == (int)Artifact.ArtifactTypeEnum.SourceCodeRevision)
						{
							dataItemField.Tooltip = "artifact-Revision.svg";
						}
						else
						{
							dataItemField.Tooltip = GlobalFunctions.GetIconForArtifactType((DataModel.Artifact.ArtifactTypeEnum)artifactLink.DestArtifactTypeId);
						}
					}
				}
			}
		}

		#endregion
	}
}
