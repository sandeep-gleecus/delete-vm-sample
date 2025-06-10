using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections;
using System.Data;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the PlanningBoard AJAX component
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class PlanningBoardService : AjaxWebServiceBase, IPlanningBoardService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.PlanningBoardService::";

        protected const string EXPANDED_KEY_UNASSIGNED = "Unassigned";
        protected const int CONTAINER_ID_CURRENT_RELEASE = -2;
        protected const int CONTAINER_ID_CURRENT_ITERATION = -3;

        //Cached workflow states
        static Dictionary<string, List<WorkflowField>> incidentWorkflowStates = new Dictionary<string, List<WorkflowField>>();
        static Dictionary<string, List<RequirementWorkflowField>> requirementWorkflowStates = new Dictionary<string, List<RequirementWorkflowField>>();

        #region IPlanningBoardService Methods

        /// <summary>
        /// Updates the release selection for the planning board
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="releaseId">The release selection (-1 = backlog, -2 = all releases)</param>
        public void PlanningBoard_UpdateRelease(int projectId, int releaseId)
        {
            const string METHOD_NAME = "PlanningBoard_UpdateRelease";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Update the setting
                ProjectSettingsCollection planningBoardSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE);
                planningBoardSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID] = releaseId;
                planningBoardSettings.Save();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates the group by selection for the planning board
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="groupById">The current group by id value</param>
        public void PlanningBoard_UpdateGroupBy(int projectId, int groupById)
        {
            const string METHOD_NAME = "PlanningBoard_UpdateGroupBy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Update the setting
                ProjectSettingsCollection planningBoardSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE);
                planningBoardSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_GROUP_BY_OPTION] = groupById;
                planningBoardSettings.Save();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates the other planning board options
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="option">The option we're setting</param>
        /// <param name="optionValue">The option value</param>
        public void PlanningBoard_UpdateOptions(int projectId, string option, bool optionValue)
        {
            const string METHOD_NAME = "PlanningBoard_UpdateOptions";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Update the settings
                ProjectSettingsCollection planningBoardSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE);
                switch (option)
                {
                    case "IncludeDetails":
                        planningBoardSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_DETAILS] = optionValue;
                        break;

                    case "IncludeIncidents":
                        planningBoardSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_INCIDENTS] = optionValue;
                        break;

                    case "IncludeTasks":
                        planningBoardSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_TASKS] = optionValue;
                        break;

                    case "IncludeTestCases":
                        planningBoardSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_TEST_CASES] = optionValue;
                        break;
                }
                planningBoardSettings.Save();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the information on either the current release or the current iteration and its parent release
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="releaseId">The id of the release or iteration in question</param>
        /// <param name="groupById">What we're grouping by</param>
        /// <returns>The release/iteration info</returns>
        public PlanningData PlanningBoard_RetrieveReleaseIterationInfo(int projectId, int releaseId, int groupById)
        {
            const string METHOD_NAME = "PlanningBoard_RetrieveReleaseIterationInfo";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view releases
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The data being returned
                PlanningData planningData = null;

                //See if we're planning by points or hours
                ProjectSettings projectSettings = new ProjectSettings(projectId);
                bool usePoints = !projectSettings.DisplayHoursOnPlanningBoard;

                //Get the list of expanded items
                ProjectSettingsCollection expandedItems = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_PLANNING_BOARD_EXPANDED_GROUPS);

                //Get the specified release or iteration
                ReleaseManager releaseManager = new ReleaseManager();
                ReleaseView releaseOrIteration = releaseManager.RetrieveById2(projectId, releaseId);
                planningData = PopulateReleaseAndIterations(projectId, new List<ReleaseView>() { releaseOrIteration }, usePoints);
                if (releaseOrIteration.IsIteration)
                {
                    planningData.Items[0].PrimaryKey = CONTAINER_ID_CURRENT_ITERATION; //Signifies that it's the Iteration record when grouping by something else
                    if (expandedItems[groupById + "_" + CONTAINER_ID_CURRENT_ITERATION] != null)
                    {
                        planningData.Items[0].Expanded = (bool)expandedItems[groupById + "_" + CONTAINER_ID_CURRENT_ITERATION];
                    }

                    //If we're grouping by status, disable this item
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                    {
                        DataItemField field = new DataItemField();
                        field.FieldName = "Disabled";
                        planningData.Items[0].Fields.Add(field.FieldName, field);
                    }

                    //Get the parent release first
                    List<ReleaseView> parentReleases = releaseManager.RetrieveParents(projectId, releaseOrIteration.IndentLevel, true);
                    if (parentReleases.Count > 0)
                    {
                        PlanningData parentItemPlanningData = PopulateReleaseAndIterations(projectId, parentReleases, usePoints);
                        planningData.Items.Insert(0, parentItemPlanningData.Items[0]);
                        parentItemPlanningData.Items[0].PrimaryKey = CONTAINER_ID_CURRENT_RELEASE; //Signifies that it's the Release record when grouping by something else
                        if (expandedItems[groupById + "_" + CONTAINER_ID_CURRENT_RELEASE] != null)
                        {
                            parentItemPlanningData.Items[0].Expanded = (bool)expandedItems[groupById + "_" + CONTAINER_ID_CURRENT_RELEASE];
                        }
                    }
                }
                else
                {
                    planningData.Items[0].PrimaryKey = CONTAINER_ID_CURRENT_RELEASE; //Signifies that it's the Release record when grouping by something else
                    if (expandedItems[groupById + "_" + CONTAINER_ID_CURRENT_RELEASE] != null)
                    {
                        planningData.Items[0].Expanded = (bool)expandedItems[groupById + "_" + CONTAINER_ID_CURRENT_RELEASE];
                    }

                    //If we're grouping by status, disable this item
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                    {
                        DataItemField field = new DataItemField();
                        field.FieldName = "Disabled";
                        planningData.Items[0].Fields.Add(field.FieldName, field);
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the tooltip for a specific item (requirement, incident, etc.)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <param name="artifactTypeId">The id of the type of artifact</param>
        /// <returns></returns>
        public string PlanningBoard_RetrieveItemTooltip(int projectId, int artifactId, int artifactTypeId)
        {
            const string METHOD_NAME = "PlanningBoard_RetrieveItemTooltip";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authorized to view the type of item in question
            Artifact.ArtifactTypeEnum authorizedArtifact = (Artifact.ArtifactTypeEnum)artifactTypeId;
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, authorizedArtifact);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Now retrieve the specific requirement or incident - handle quietly if it doesn't exist
            try
            {
                //See if we have a requirement or task
                if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement)
                {
                    int requirementId = artifactId;
                    //Instantiate the requirement business object
                    RequirementManager requirementManager = new RequirementManager();

                    //First we need to get the requirement itself
                    RequirementView requirement = requirementManager.RetrieveById2(null, requirementId);

                    //Next we need to get the list of successive parent folders
                    List<RequirementView> parentRequirements = requirementManager.RetrieveParents(Business.UserManager.UserInternal, requirement.ProjectId, requirement.IndentLevel);
                    string tooltip = "";
					int parentIndex = 0;
					int parentIndexLast = parentRequirements.Count - 1;
					foreach (RequirementView parentRequirement in parentRequirements)
                    {
                        tooltip += "<b>" + parentRequirement.Name + "</b>";
						if (parentIndex != parentIndexLast)
						{
							tooltip += " &gt; ";
						}
						parentIndex++;
					}

                    //Now we need to get the requirement itself 
                    tooltip += "<br />\n<u>" + requirement.ArtifactToken + ": " 
						+ GlobalFunctions.HtmlRenderAsPlainText(requirement.Name) + "</u>"
						//then add the type and status
						+ "<br />\n<b>(" + requirement.RequirementTypeName + " | " + requirement.RequirementStatusName + ")</b>";

					if (!String.IsNullOrEmpty(requirement.Description))
                    {
                        tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(requirement.Description);
                    }

                    //See if we have any comments to append
                    IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(requirementId, Artifact.ArtifactTypeEnum.Requirement, false);
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

                if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident)
                {
                    int incidentId = artifactId;
				    //Instantiate the incident business object
				    IncidentManager incidentManager = new IncidentManager();

				    //Now retrieve the specific incident - handle quietly if it doesn't exist
                    Incident incident = incidentManager.RetrieveById(incidentId, true);
					IncidentType incidentType = incidentManager.RetrieveIncidentTypeById(incident.IncidentTypeId);
					IncidentStatus incidentStatus = incidentManager.IncidentStatus_RetrieveById(incident.IncidentStatusId);
					string tooltip;
                    tooltip = "<u>" + incident.ArtifactToken + ": " + GlobalFunctions.HtmlRenderAsPlainText(incident.Name)
						+ " " + "</u><br />\n<b>(" + incidentType.Name + " | " + incidentStatus.Name + ")</b><br />\n"
						+ GlobalFunctions.HtmlRenderAsPlainText(incident.Description);

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

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return tooltip;
                }

                if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Task)
                {
                    int taskId = artifactId;
                    TaskView taskView = new TaskManager().TaskView_RetrieveById(taskId);
                    string tooltip = "";

					//See if we have a requirement it belongs to
					if (taskView.RequirementId.HasValue)
					{
						tooltip += "<b>" + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, taskView.RequirementId.Value, true) + " " + GlobalFunctions.HtmlRenderAsPlainText(taskView.RequirementName) + "</b><br />\n";

					}
					tooltip += "<u>" + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TASK, taskView.TaskId, true)
						+ " " + GlobalFunctions.HtmlRenderAsPlainText(taskView.Name) + "</u>"
						+ "<br />\n<b>(" + taskView.TaskTypeName + " | " + taskView.TaskStatusName + ")</b>";

					//Add the description if we have one
					if (!String.IsNullOrEmpty(taskView.Description))
					{
						tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(taskView.Description);
					}

                    //See if we have any comments to append
                    IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(taskId, Artifact.ArtifactTypeEnum.Task, false);
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

                return Resources.Messages.Global_UnableRetrieveTooltip;
            }
            catch (ArtifactNotExistsException)
            {
                //This is the case where the client still displays the requirement, but it has already been deleted on the server
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for artifact");
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return Resources.Messages.Global_UnableRetrieveTooltip;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of planning board items to display in a specific container
        /// </summary>
        /// <param name="projectId">The currently selected project</param>
        /// <param name="releaseId">The currently selected release</param>
        /// <param name="groupById">The data that we're grouping by</param>
        /// <param name="containerId">The id of the container we're retrieving items for (null = unassigned)</param>
        /// <param name="includeDetails">Do we include the full details</param>
        /// <param name="includeIncidents">Do we include incident items</param>
        /// <param name="includeTasks">Do we include the linked tasks</param>
        /// <param name="includeTestCases">Do we include the linked test cases</param>
        /// <param name="isIteration">Is the current releaseId an iteration</param>
        /// <returns>The requested items</returns>
        public PlanningData PlanningBoard_RetrieveItems(int projectId, int releaseId, bool isIteration, int groupById, int? containerId, bool includeDetails, bool includeIncidents, bool includeTasks, bool includeTestCases)
        {
            const string METHOD_NAME = "PlanningBoard_RetrieveItems";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view requirements
            Artifact.ArtifactTypeEnum authorizedArtifact = Artifact.ArtifactTypeEnum.Requirement;
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, authorizedArtifact);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//The data being returned
				PlanningData planningData = null;

                //See what we're grouping by
                RequirementManager requirementManager = new RequirementManager();
                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByComponent)
                {
                    //Get the list of requirements by component, see if we are looking for backlog or all releases
                    if (releaseId == -1)
                    {
                        //Backlog
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByComponentId(projectId, containerId);

                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "ComponentId");

                        //Retrieve and populate the incidents if that's specified
                        if (includeIncidents)
                        {
                            List<IncidentView> incidents = new IncidentManager().Incident_RetrieveBacklogByComponentId(projectId, containerId);
                            PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "ComponentId");

                            //Resort the combined list by rank
                            planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                        }
                    }
                    else if (releaseId == -2)
                    {
                        //All Releases
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveAllReleasesByComponentId(projectId, containerId);

                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "ComponentId");

                        //Retrieve and populate the incidents if that's specified
                        if (includeIncidents)
                        {
                            List<IncidentView> incidents = new IncidentManager().Incident_RetrieveAllReleasesByComponentId(projectId, containerId);
                            PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "ComponentId");

                            //Resort the combined list by rank
                            planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                        }
                    }
                    else if (releaseId > 0)
                    {
                        //Specific Release/Sprint
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveForReleaseByComponentId(projectId, containerId, releaseId);

                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "ComponentId");

                        //Retrieve and populate the incidents if that's specified
                        if (includeIncidents)
                        {
                            List<IncidentView> incidents = new IncidentManager().Incident_RetrieveForReleaseByComponentId(projectId, containerId, releaseId);
                            PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "ComponentId");

                            //Resort the combined list by rank
                            planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                        }
                    }
                }

                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPackage)
                {
                    //Get the list of requirements by package/parent, see if we are looking for backlog or all releases
                    if (releaseId == -1)
                    {
                        //Backlog
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByPackageRequirementId(projectId, containerId);

                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, null);

                        //Retrieve and populate the incidents if that's specified
                        if (includeIncidents)
                        {
                            List<IncidentView> incidents = new IncidentManager().Incident_RetrieveBacklogByRequirementId(projectId, containerId);
                            PopulateIncidentItems(projectId, planningData, incidents, includeDetails, null, false);

                            //Resort the combined list by rank
                            planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                        }
                    }
                    else if (releaseId == -2)
                    {
                        //All Releases
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveAllReleasesByPackageRequirementId(projectId, containerId, releaseId);

                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, null);

                        //Retrieve and populate the incidents if that's specified
                        if (includeIncidents)
                        {
                            List<IncidentView> incidents = new IncidentManager().Incident_RetrieveAllReleasesByRequirementId(projectId, containerId);
                            PopulateIncidentItems(projectId, planningData, incidents, includeDetails, null, false);

                            //Resort the combined list by rank
                            planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                        }
                    }
                    else if (releaseId > 0)
                    {
                        //Specific Release/Sprint
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveForReleaseByPackageRequirementId(projectId, containerId, releaseId);

                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, null);

                        //Retrieve and populate the incidents if that's specified
                        if (includeIncidents)
                        {
                            List<IncidentView> incidents = new IncidentManager().Incident_RetrieveForReleaseByRequirementId(projectId, containerId, releaseId);
                            PopulateIncidentItems(projectId, planningData, incidents, includeDetails, null, false);

                            //Resort the combined list by rank
                            planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                        }
                    }
                }

                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPriority)
                {
                    //Get the list of requirements by priority, see if we are looking for backlog or all releases
                    if (releaseId == -1)
                    {
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByImportanceId(projectId, containerId);

                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "ImportanceId");

                        //Since we only show the requirement importances, we don't include incidents currently
                    }
                    else if (releaseId == -2)
                    {
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveAllReleasesByImportanceId(projectId, containerId);

                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "ImportanceId");

                        //Since we only show the requirement importances, we don't include incidents currently
                    }
                    else if (releaseId > 0)
                    {
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, containerId, releaseId);

                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "ImportanceId");

                        //Since we only show the requirement importances, we don't include incidents currently
                    }
                }

                if (releaseId == -2 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByRelease)
                {
                    //Get the list of requirements by release
                    List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, containerId, true);

                    //Populate the data objects
                    planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "ReleaseId");

                    //Retrieve and populate the incidents if that's specified
                    if (includeIncidents)
                    {
                        List<IncidentView> incidents = new IncidentManager().Incident_RetrieveBacklogByReleaseId(projectId, containerId, true);
                        PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "ResolvedReleaseId");

                        //Resort the combined list by rank
                        planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                    }
                }

                if (releaseId == -1 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                {
                    //Get the list of requirements by status that have no release
                    List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, null, containerId);

                    //Populate the data objects
                    planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "RequirementStatusId");
                }

                if (releaseId == -2 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus && containerId.HasValue)
                {
                    //Get the list of requirements by status that have a release set
                    List<RequirementView> requirements = requirementManager.Requirement_RetrieveAllReleasesByStatusId(projectId, containerId.Value);

                    //Populate the data objects
                    planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "RequirementStatusId");
                }

                //Gets the items for all releases in the project, organized by person (or unassigned)
                if (releaseId == -2 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPerson)
                {
                    //Get the list of requirements by resource/user for all releases in the project
                    List<RequirementView> requirements = requirementManager.Requirement_RetrieveAllReleasesByUserId(projectId, containerId);

                    //Populate the data objects
                    planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "OwnerId");

                    //Retrieve and populate the incidents if that's specified
                    if (includeIncidents)
                    {
                        List<IncidentView> incidents = new IncidentManager().Incident_RetrieveAllReleasesByUserId(projectId, containerId);
                        PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "OwnerId");

                        //Resort the combined list by rank
                        planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                    }
                }

                //These options require that a valid release or iteration be provided
                if (releaseId > 0)
                {
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByIteration)
                    {
                        //If the container requested is the current release, use that
                        if (containerId == CONTAINER_ID_CURRENT_RELEASE)
                        {
                            containerId = releaseId;
                        }

                        //Get the list of requirements by release/iteration
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, containerId, false);

                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "ReleaseId");

                        //Retrieve and populate the incidents if that's specified
                        if (includeIncidents)
                        {
                            List<IncidentView> incidents = new IncidentManager().Incident_RetrieveBacklogByReleaseId(projectId, containerId, false);
                            PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "ResolvedReleaseId");

                            //Resort the combined list by rank
                            planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                        }
                    }

                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                    {
                        //If we're asked to get the ones associated with the release, retrieve just those for that release only
                        //We will be given the current iteration in this case, so need to get the parent
                        List<RequirementView> requirements;
                        if (containerId == CONTAINER_ID_CURRENT_RELEASE && isIteration)
                        {
                            //Get the list of requirements by the parent release of the current iteration
                            int? parentReleaseId = new ReleaseManager().GetParentReleaseId(releaseId);
                            if (parentReleaseId.HasValue)
                            {
                                requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, parentReleaseId.Value, false);
                            }
                            else
                            {
                                //Otherwise just return an empty list
                                requirements = new List<RequirementView>();
                            }
                        }
                        else
                        {
                            //Get the list of requirements by requirements status
                            requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, releaseId, containerId);
                        }
                        //Populate the data objects
                        planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "RequirementStatusId");

                        /*
                        //Retrieve and populate the incidents if that's specified
                        if (includeIncidents)
                        {
                            List<IncidentView> incidents = new IncidentManager().Incident_RetrieveBacklogByReleaseId(projectId, containerId);
                            PopulateIncidentItems(projectId, planningData, incidents, includeDetails);

                            //Resort the combined list by rank
                            planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                        }*/
                    }

                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPerson)
                    {
                        //If we're asked to get the ones associated with the release, retrieve just those for that release only
                        if (containerId == CONTAINER_ID_CURRENT_RELEASE)
                        {
                            //If we're given the current iteration, need to get the parent release
                            if (isIteration)
                            {
                                //Get the list of requirements by the parent release of the current iteration that has no person assigned
                                int? parentReleaseId = new ReleaseManager().GetParentReleaseId(releaseId);
                                if (parentReleaseId.HasValue)
                                {
                                    List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, parentReleaseId.Value, null, false);

                                    //Populate the data objects
                                    planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "OwnerId");

                                    //Retrieve and populate the incidents if that's specified
                                    if (includeIncidents)
                                    {
                                        List<IncidentView> incidents = new IncidentManager().Incident_RetrieveBacklogByUserId(projectId, parentReleaseId.Value, null, false);
                                        PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "OwnerId");

                                        //Resort the combined list by rank
                                        planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                                    }
                                }
                                else
                                {
                                    //Just return an empty data object
                                    planningData = new PlanningData();
                                }
                            }
                            else
                            {
                                //Get the items for the current release that have no person assigned
                                List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, releaseId, null, true);

                                //Populate the data objects
                                planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "OwnerId");

                                //Retrieve and populate the incidents if that's specified
                                if (includeIncidents)
                                {
                                    List<IncidentView> incidents = new IncidentManager().Incident_RetrieveBacklogByUserId(projectId, releaseId, null, true);
                                    PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "OwnerId");

                                    //Resort the combined list by rank
                                    planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                                }

                            }
                        }
                        else if (containerId == CONTAINER_ID_CURRENT_ITERATION)
                        {
                            //Get the items for the current iteration that have no person assigned
                            List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, releaseId, null, false);

                            //Populate the data objects
                            planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "OwnerId");

                            //Retrieve and populate the incidents if that's specified
                            if (includeIncidents)
                            {
                                List<IncidentView> incidents = new IncidentManager().Incident_RetrieveBacklogByUserId(projectId, releaseId, null, false);
                                PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "OwnerId");

                                //Resort the combined list by rank
                                planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                            }
                        }
                        else if (!containerId.HasValue)
                        {
                            //Get the list of requirements where they are not assigned to a release or person
                            List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, null, null, false);

                            //Populate the data objects
                            planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "OwnerId");

                            //Retrieve and populate the incidents if that's specified
                            if (includeIncidents)
                            {
                                List<IncidentView> incidents = new IncidentManager().Incident_RetrieveBacklogByUserId(projectId, null, null, false);
                                PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "OwnerId");

                                //Resort the combined list by rank
                                planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                            }
                        }
                        else
                        {
                            //Get the list of requirements by resource/user for this release/iteration
                            List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, releaseId, containerId, true);

                            //Populate the data objects
                            planningData = PopulateRequirementItems(projectId, projectTemplateId, requirements, includeDetails, includeTasks, includeTestCases, "OwnerId");

                            //Retrieve and populate the incidents if that's specified
                            if (includeIncidents)
                            {
                                List<IncidentView> incidents = new IncidentManager().Incident_RetrieveBacklogByUserId(projectId, releaseId, containerId, true);
                                PopulateIncidentItems(projectId, planningData, incidents, includeDetails, "OwnerId");

                                //Resort the combined list by rank
                                planningData.Items = planningData.Items.OrderByDescending(i => i.Rank).ToList();
                            }
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of grouping items for the planning board
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="releaseId">The id of the selected release (-1 = backlog, -2 = all releases)</param>
        /// <param name="groupById">The id of the grouping type</param>
        /// <returns>The list of items</returns>
        public PlanningData PlanningBoard_RetrieveGroupByContainers(int projectId, int releaseId, int groupById)
        {
            const string METHOD_NAME = "PlanningBoard_RetrieveGroupByContainers";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view requirements
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //See if we're planning by points or hours
                ProjectSettings projectSettings = new ProjectSettings(projectId);
                bool usePoints = !projectSettings.DisplayHoursOnPlanningBoard;

                //See if we have any WIP limits defined
                string statusPercentages = projectSettings.KanbanWip_StatusPercentages;

                //The data being returned
                PlanningData planningData = null;

                //See what we're grouping by
                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByComponent)
                {
                    //Get the list of active components
                    ComponentManager componentManager = new ComponentManager();
                    List<Component> components = componentManager.Component_Retrieve(projectId);

                    //Populate the data objects
                    planningData = PopulateComponents(projectId, components);
                }

                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPackage)
                {
                    //Get the list of summary requirements (packages) for the project
                    RequirementManager requirementManager = new RequirementManager();
                    List<RequirementView> packages = requirementManager.Requirement_RetrieveSummaryBacklog(projectId);

                    //Populate the data objects
                    planningData = PopulatePackages(projectId, packages);
                }

                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPriority)
                {
                    //Get the list of requirements importances
                    RequirementManager requirementManager = new RequirementManager();
                    List<Importance> importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);

                    //Populate the data objects
                    planningData = PopulateImportances(projectId, importances);
                }

                if (releaseId == -2 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByRelease)
                {
                    //Get the list of releases (not iterations)
                    ReleaseManager releaseManager = new ReleaseManager();
                    List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, true, false);

                    //Populate the data objects
                    planningData = PopulateReleases(projectId, releases, usePoints);
                }

                if (releaseId == -1 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                {
                    //Get the list of requirement stati - only statuses before Planned get included
                    RequirementManager requirementManager = new RequirementManager();
                    List<RequirementStatus> requirementStati = requirementManager.RetrieveStatusesInUse(projectTemplateId, true, false, false);

                    //Populate the data objects
                    planningData = PopulateStati(projectId, requirementStati);
                }

                if (releaseId == -2 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                {
                    //Get the list of requirement stati - Only statuses that are Planned or after get included, and exclude Obsolete
                    RequirementManager requirementManager = new RequirementManager();
                    List<RequirementStatus> requirementStati = requirementManager.RetrieveStatusesInUse(projectTemplateId, false, true, false);

                    //Populate the data objects
                    planningData = PopulateStati(projectId, requirementStati);
                }

                if (releaseId == -2 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPerson)
                {
                    //Get the list of people in the project
                    ProjectManager projectManager = new ProjectManager();
                    List<ProjectResourceView> projectResources = projectManager.RetrieveResources(projectId, null);

                    //Populate the data objects
                    planningData = PopulateResources(projectId, projectResources);
                }

                //These options require that a valid release or iteration be provided
                if (releaseId > 0)
                {
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByIteration)
                    {
                        //Get the specified release's child iterations
                        ReleaseManager releaseManager = new ReleaseManager();
                        List<ReleaseView> iterations = releaseManager.RetrieveIterations(projectId, releaseId, true);

                        //Populate the data objects
                        planningData = PopulateReleaseAndIterations(projectId, iterations, usePoints);
                    }

                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                    {
                        //Get the list of requirement stati - Only statuses that are Planned or after get included, and exclude Obsolete
                        RequirementManager requirementManager = new RequirementManager();
                        List<RequirementStatus> requirementStati = requirementManager.RetrieveStatusesInUse(projectTemplateId, false, true, false);

                        Dictionary<int, int> wipLimitsByStatus = new Dictionary<int, int>();
                        if (!String.IsNullOrEmpty(statusPercentages))
                        {
                            //If we have WIP limits, we need to get the release/sprint information to calculate the actual WIP limit
                            ReleaseView release = new ReleaseManager().RetrieveById2(projectId, releaseId);
                            decimal wipMultiplier = (release.IsIteration) ? projectSettings.KanbanWip_IterationWipMultiplier : projectSettings.KanbanWip_ReleaseWipMultiplier;
                            decimal maxNumberOfItems = wipMultiplier * release.ResourceCount;

                            //Loop through each status
                            string[] statuses = statusPercentages.Split('|');
                            foreach (string statusEntry in statuses)
                            {
                                string[] parts = statusEntry.Split(',');
                                if (parts.Length == 3)
                                {
                                    int requirementStatusId;
                                    if (Int32.TryParse(parts[0], out requirementStatusId))
                                    {
                                        string percentageString = (release.IsIteration) ? parts[2] : parts[1];
                                        decimal percentage;
                                        if (!String.IsNullOrEmpty(percentageString) && Decimal.TryParse(percentageString, out percentage))
                                        {
                                            int itemLimit = (int)Math.Truncate(percentage * maxNumberOfItems / 100M);
                                            if (!wipLimitsByStatus.ContainsKey(requirementStatusId))
                                            {
                                                wipLimitsByStatus.Add(requirementStatusId, itemLimit);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Populate the data objects
                        planningData = PopulateStati(projectId, requirementStati, wipLimitsByStatus);
                    }

                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPerson)
                    {
                        //Get the list of people in the project
                        ProjectManager projectManager = new ProjectManager();
                        List<ProjectResourceView> projectResources = projectManager.RetrieveResources(projectId, releaseId);

                        //Populate the data objects
                        planningData = PopulateResources(projectId, projectResources);
                    }
                }

                //See which ones are expanded or not (only used for horizontal style board)
                if (planningData != null && planningData.Items.Count > 0)
                {
                    ProjectSettingsCollection expandedGroups = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_PLANNING_BOARD_EXPANDED_GROUPS);
                    foreach (PlanningDataItem dataItem in planningData.Items)
                    {
                        string key = groupById + "_" + dataItem.PrimaryKey;
                        if (expandedGroups[key] != null)
                        {
                            dataItem.Expanded = (bool)expandedGroups[key];
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates the expanded/collapsed state of a group on the planning board
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="expanded">Whether it's expanded or not</param>
        /// <param name="groupById">The data that we're grouping by</param>
        /// <param name="containerId">The id of the container we're retrieving items for (null = unassigned)</param>
        public void PlanningBoard_UpdateExpandCollapsed(int projectId, int groupById, int? containerId, bool expanded)
        {
            const string METHOD_NAME = "PlanningBoard_UpdateExpandCollapsed";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this project (since it's just updating a user setting, just check they can view reqs)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the list of expanded items
                ProjectSettingsCollection expandedItems = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_PLANNING_BOARD_EXPANDED_GROUPS);

                //Add/update an entry for this artifact
                if (containerId.HasValue)
                {
                    expandedItems[groupById + "_" + containerId.Value] = expanded;
                }
                else
                {
                    expandedItems[groupById + "_" + EXPANDED_KEY_UNASSIGNED] = expanded;
                }
                expandedItems.Save();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Moves an item from one container to another, and optionally specifies an existing item in that container to move
        /// in front of (otherwise it's located at the end)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="groupById">The id of the type of container we're grouping by</param>
        /// <param name="containerId">The id of the grouping container</param>
        /// <param name="items">The list of items to be moved</param>
        /// <param name="existingArtifactTypeId">The id of the type of artifact we're moving in front of</param>
        /// <param name="existingArtifactId">The id of the artifact that we're moving in front of</param>
        /// <param name="releaseId">The currently selected release</param>
        /// <param name="isIteration">Is the current releaseId an iteration</param>
        public void PlanningBoard_MoveItems(int projectId, int releaseId, bool isIteration, int groupById, int? containerId, List<ArtifactReference> items, int? existingArtifactTypeId, int? existingArtifactId)
        {
            const string METHOD_NAME = "PlanningBoard_MoveItems";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to modify requirements
            Artifact.ArtifactTypeEnum authorizedArtifact = Artifact.ArtifactTypeEnum.Requirement;
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, authorizedArtifact);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //See what we're grouping by
                RequirementManager requirementManager = new RequirementManager();
                IncidentManager incidentManager = new IncidentManager();

                //By Component
                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByComponent)
                {
                    if (containerId == -1)
                    {
                        containerId = null;
                    }

                    //Extract the requirement and incident ids
                    List<int> requirementIds = ExtractRequirementIds(items);
                    List<int> incidentIds = ExtractIncidentIds(items);

                    //Update the requirements
                    foreach (int requirementId in requirementIds)
                    {
                        try
                        {
                            Requirement requirement = requirementManager.RetrieveById3(projectId, requirementId);
                            requirement.StartTracking();
                            requirement.ComponentId = containerId;
                            requirement.LastUpdateDate = DateTime.UtcNow;
                            requirement.ConcurrencyDate = DateTime.UtcNow;
                            requirementManager.Update(userId, projectId, new List<Requirement> { requirement });
                        }
                        catch (ArtifactNotExistsException)
                        {
                            //Ignore, just don't update the item
                        }
                    }

                    //Update the incidents
                    foreach (int incidentId in incidentIds)
                    {
                        try
                        {
                            Incident incident = incidentManager.RetrieveById(incidentId, false);
                            incident.StartTracking();
                            incident.ComponentIds = containerId.ToString();
                            incident.LastUpdateDate = DateTime.UtcNow;
                            incident.ConcurrencyDate = DateTime.UtcNow;
                            incidentManager.Update(incident, userId);
                        }
                        catch (ArtifactNotExistsException)
                        {
                            //Ignore, just don't update the item
                        }
                    }
                }

                //By Package
                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPackage)
                {
                    if (containerId == -1)
                    {
                        containerId = null;
                    }

                    //Extract the requirement ids
                    List<int> requirementIds = ExtractRequirementIds(items);

                    //Update the requirements
                    requirementManager.Requirement_UpdateBacklogPackageRequirementId(projectId, requirementIds, containerId, userId);

                    //Incidents cannot be moved in this view (since they are really associations)
                }

                //By Importance
                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPriority)
                {
                    if (containerId == -1)
                    {
                        containerId = null;
                    }

                    //Extract the requirement ids
                    List<int> requirementIds = ExtractRequirementIds(items);

                    //Update the requirements
                    foreach (int requirementId in requirementIds)
                    {
                        try
                        {
                            Requirement requirement = requirementManager.RetrieveById3(projectId, requirementId);
                            requirement.StartTracking();
                            requirement.ImportanceId = containerId;
                            requirement.LastUpdateDate = DateTime.UtcNow;
                            requirement.ConcurrencyDate = DateTime.UtcNow;
                            requirementManager.Update(userId, projectId, new List<Requirement> { requirement });
                        }
                        catch (ArtifactNotExistsException)
                        {
                            //Ignore, just don't update the item
                        }
                    }
                }

                //By Release
                if (releaseId == -2 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByRelease)
                {
                    //Extract the requirement and incident ids
                    List<int> requirementIds = ExtractRequirementIds(items);
                    List<int> incidentIds = ExtractIncidentIds(items);

                    if (containerId.HasValue && containerId > 0)
                    {
                        //Update the requirements
                        requirementManager.AssociateToIteration(requirementIds, containerId.Value, userId);

                        //Update the incidents
                        incidentManager.AssociateToIteration(incidentIds, containerId.Value, userId);
                    }
                    else
                    {
                        //Update the requirements
                        requirementManager.RemoveReleaseAssociation(userId, requirementIds);

                        //Update the incidents
                        incidentManager.RemoveReleaseAssociation(incidentIds, userId);
                    }
                }

                //By Status (product backlog or all projects)
                if ((releaseId == -1 || releaseId == -2) && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus && containerId.HasValue)
                {
                    //Extract the requirement ids
                    List<int> requirementIds = ExtractRequirementIds(items);

                    //Update the requirements
                    foreach (int requirementId in requirementIds)
                    {
                        try
                        {
                            Requirement requirement = requirementManager.RetrieveById3(projectId, requirementId);
                            requirement.StartTracking();
                            requirement.RequirementStatusId = containerId.Value;
                            requirement.LastUpdateDate = DateTime.UtcNow;
                            requirement.ConcurrencyDate = DateTime.UtcNow;
                            requirementManager.Update(userId, projectId, new List<Requirement> { requirement });
                        }
                        catch (ArtifactNotExistsException)
                        {
                            //Ignore, just don't update the item
                        }
                    }
                }

                //By Person
                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPerson)
                {
                    if (containerId == -1)
                    {
                        containerId = null;
                    }

                    //Extract the requirement and incident ids
                    List<int> requirementIds = ExtractRequirementIds(items);
                    List<int> incidentIds = ExtractIncidentIds(items);

                    //If we are viewing by Person, the user may drop the item on a release or iteration container
                    //this is the special case
                    int? releaseOrIterationId = null;
                    int? ownerId = containerId;
                    if (!containerId.HasValue)
                    {
                        //We want to unassign the user and the release/iteration
                        releaseOrIterationId = null;
                        ownerId = null;
                    }
                    else if (containerId == CONTAINER_ID_CURRENT_RELEASE)
                    {
                        //If we're given the current iteration, need to get the parent release
                        if (isIteration)
                        {
                            //Get the list of requirements by the parent release of the current iteration that has no person assigned
                            releaseOrIterationId = new ReleaseManager().GetParentReleaseId(releaseId);
                        }
                        else
                        {
                            //Otherwise we already have the current release
                            releaseOrIterationId = releaseId;
                        }
                        ownerId = null;
                    }
                    else if (containerId == CONTAINER_ID_CURRENT_ITERATION)
                    {
                        //The current iteration
                        releaseOrIterationId = releaseId;
                        ownerId = null;
                    }
                    else
                    {
                        //Set the release
                        //Leave the owner
                        releaseOrIterationId = releaseId;
                    }

                    //Update the requirements
                    foreach (int requirementId in requirementIds)
                    {
                        try
                        {
                            requirementManager.AssignToUser(requirementId, ownerId, userId);
                        }
                        catch (ArtifactNotExistsException)
                        {
                            //Ignore, just don't update the item
                        }
                    }

                    //Update the incidents
                    foreach (int incidentId in incidentIds)
                    {
                        try
                        {
                            incidentManager.AssignToUser(incidentId, ownerId, userId);
                        }
                        catch (ArtifactNotExistsException)
                        {
                            //Ignore, just don't update the item
                        }
                    }

                    //Change the release/iteration if necessary. The called functions will check to see if a change
                    //actually needs to happen
                    //We don't do this in the 'all-releases' view
                    if (releaseId != -2)
                    {
                        if (releaseOrIterationId.HasValue)
                        {
                            //Update the requirements
                            requirementManager.AssociateToIteration(requirementIds, releaseOrIterationId.Value, userId);

                            //Update the incidents
                            incidentManager.AssociateToIteration(incidentIds, releaseOrIterationId.Value, userId);
                        }
                        else
                        {
                            //Update the requirements
                            requirementManager.RemoveReleaseAssociation(userId, requirementIds);

                            //Update the incidents
                            incidentManager.RemoveReleaseAssociation(incidentIds, userId);
                        }
                    }
                }

                //These options require that a valid release or iteration be provided
                if (releaseId > 0)
                {
                    //By Iteration
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByIteration)
                    {
                        if (containerId == -1)
                        {
                            containerId = null;
                        }
                        //If the container requested is the current release, use that
                        if (containerId == CONTAINER_ID_CURRENT_RELEASE)
                        {
                            containerId = releaseId;
                        }

                        //Extract the requirement and incident ids
                        List<int> requirementIds = ExtractRequirementIds(items);
                        List<int> incidentIds = ExtractIncidentIds(items);

                        if (containerId.HasValue)
                        {
                            //Update the requirements
                            requirementManager.AssociateToIteration(requirementIds, containerId.Value, userId);

                            //Update the incidents
                            incidentManager.AssociateToIteration(incidentIds, containerId.Value, userId);
                        }
                        else
                        {
                            //Update the requirements
                            requirementManager.RemoveReleaseAssociation(userId, requirementIds);

                            //Update the incidents
                            incidentManager.RemoveReleaseAssociation(incidentIds, userId);
                        }
                    }

                    //By Status
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                    {
                        if (containerId == -1)
                        {
                            containerId = null;
                        }

                        //Extract the requirement ids
                        List<int> requirementIds = ExtractRequirementIds(items);

                        //If we have no status specified, it means they are dragging the item back to the product backlog
                        //in which case, set status to 'Accepted' and unset the release
                        if (!containerId.HasValue)
                        {
                            containerId = (int)Requirement.RequirementStatusEnum.Accepted;
                        }

                        //If we are viewing an Iteration (not release) and they drag the item to the parent
                        //release, leave the status unchangedm but change the release to the parent
                        int? parentReleaseId = null;
                        if (containerId == CONTAINER_ID_CURRENT_RELEASE && isIteration)
                        {
                            //Get the list of requirements by the parent release of the current iteration
                            parentReleaseId = new ReleaseManager().GetParentReleaseId(releaseId);
                        }

                        //Update the requirements
                        foreach (int requirementId in requirementIds)
                        {
                            try
                            {
                                Requirement requirement = requirementManager.RetrieveById3(projectId, requirementId);
                                requirement.StartTracking();
                                requirement.LastUpdateDate = DateTime.UtcNow;
                                requirement.ConcurrencyDate = DateTime.UtcNow;
                                if (containerId == CONTAINER_ID_CURRENT_RELEASE && isIteration)
                                {
                                    if (parentReleaseId.HasValue)
                                    {
                                        requirement.ReleaseId = parentReleaseId;
                                    }
                                }
                                else
                                {
                                    requirement.RequirementStatusId = containerId.Value;
                                    if (containerId.Value == (int)Requirement.RequirementStatusEnum.Accepted)
                                    {
                                        if (requirement.ReleaseId.HasValue)
                                        {
                                            //Unset the release
                                            requirement.ReleaseId = null;
                                        }
                                    }
                                    else
                                    {
                                        if (!requirement.ReleaseId.HasValue || requirement.ReleaseId != releaseId)
                                        {
                                            //Set the release to the current one
                                            requirement.ReleaseId = releaseId;
                                        }
                                    }
                                }
                                requirementManager.Update(userId, projectId, new List<Requirement> { requirement });
                            }
                            catch (ArtifactNotExistsException)
                            {
                                //Ignore, just don't update the item
                            }
                        }
                    }
                }

                //If we have an existing item, then also update the Rank (incidents/requirements only, not tasks)
                if (existingArtifactTypeId.HasValue && existingArtifactId.HasValue && (existingArtifactTypeId.Value == (int)Artifact.ArtifactTypeEnum.Requirement || existingArtifactTypeId.Value == (int)Artifact.ArtifactTypeEnum.Incident))
                {
                    try
                    {
                        //Get the existing rank
                        int? existingRank = null;
                        if (existingArtifactTypeId.Value == (int)Artifact.ArtifactTypeEnum.Requirement)
                        {
                            RequirementView requirementView = requirementManager.RetrieveById2(projectId, existingArtifactId.Value);
                            if (requirementView != null && requirementView.Rank.HasValue)
                            {
                                existingRank = requirementView.Rank.Value;
                            }
                        }
                        if (existingArtifactTypeId.Value == (int)Artifact.ArtifactTypeEnum.Incident)
                        {
                            Incident incident = incidentManager.RetrieveById(existingArtifactId.Value, false);
                            if (incident != null && incident.Rank.HasValue)
                            {
                                existingRank = incident.Rank.Value;
                            }
                        }

                        //Extract the requirement and incident ids
                        List<int> requirementIds = ExtractRequirementIds(items);
                        List<int> incidentIds = ExtractIncidentIds(items);

                        //Update the requirement ranks
                        if (requirementIds.Count > 0)
                        {
                            requirementManager.Requirement_UpdateRanks(projectId, requirementIds, existingRank);
                        }

                        //Update the incident ranks
                        if (incidentIds.Count > 0)
                        {
                            incidentManager.Incident_UpdateRanks(projectId, incidentIds, existingRank);
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Existing items is deleted, just ignore
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }


        #endregion

        #region Internal Functions

        /// <summary>
        /// Gets the list of requirement ids (only) from a generic item list
        /// </summary>
        /// <param name="items">The list of items</param>
        /// <returns>The list of requirement ids</returns>
        /// <remarks>Returns empty list if there are no requirement items</remarks>
        protected List<int> ExtractRequirementIds(List<ArtifactReference> items)
        {
            if (items == null || items.Count < 1)
            {
                return new List<int>();
            }
            return items.Where(i => i.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement).Select(i => i.ArtifactId).ToList();
        }

        /// <summary>
        /// Gets the list of incident ids (only) from a generic item list
        /// </summary>
        /// <param name="items">The list of items</param>
        /// <returns>The list of incident ids</returns>
        /// <remarks>Returns empty list if there are no incident items</remarks>
        protected List<int> ExtractIncidentIds(List<ArtifactReference> items)
        {
            if (items == null || items.Count < 1)
            {
                return new List<int>();
            }
            return items.Where(i => i.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident).Select(i => i.ArtifactId).ToList();
        }

        /// <summary>
        /// Gets the list of task ids (only) from a generic item list
        /// </summary>
        /// <param name="items">The list of items</param>
        /// <returns>The list of task ids</returns>
        /// <remarks>Returns empty list if there are no task items</remarks>
        protected List<int> ExtractTaskIds(List<ArtifactReference> items)
        {
            if (items == null || items.Count < 1)
            {
                return new List<int>();
            }
            return items.Where(i => i.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task).Select(i => i.ArtifactId).ToList();
        }



        /// <summary>
        /// Populates the list of components as planning objects
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="components">The list of components</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulateComponents(int projectId, List<Component> components)
        {
            const string METHOD_NAME = "PopulateComponents";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();
                planningData.ArtifactImage = "org-Component.svg";

                //Populate the items                
                List<PlanningDataItem> dataItems = planningData.Items;
                foreach (Component component in components)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = component.ComponentId;

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = component.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Add the item
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Populates the list of importances as planning objects
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="importances">The list of importances</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulateImportances(int projectId, List<Importance> importances)
        {
            const string METHOD_NAME = "PopulateImportances";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();

                //Populate the items     
                List<PlanningDataItem> dataItems = planningData.Items;
                foreach (Importance importance in importances)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = importance.ImportanceId;

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = importance.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Color
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "CssClass";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = importance.Color;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Add the item
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Populates the list of requirement stati as planning objects
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="requirementStati">The list of requirement stati</param>
        /// <param name="wipLimitsByStatus">list of WIP limits mapped to status</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulateStati(int projectId, List<RequirementStatus> requirementStati, Dictionary<int, int> wipLimitsByStatus = null)
        {
            const string METHOD_NAME = "PopulateStati";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();

                //Populate the items     
                List<PlanningDataItem> dataItems = planningData.Items;
                foreach (RequirementStatus requirementStatus in requirementStati)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = requirementStatus.RequirementStatusId;
                    dataItem.Expanded = true;

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = requirementStatus.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Add the WIP if specified
                    if (wipLimitsByStatus != null && wipLimitsByStatus.ContainsKey(requirementStatus.RequirementStatusId))
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "WipLimit";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
                        dataItemField.IntValue = wipLimitsByStatus[requirementStatus.RequirementStatusId];
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Add the item
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Populates the list of packages as planning objects
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="packages">The list of packages</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulatePackages(int projectId, List<RequirementView> packages)
        {
            const string METHOD_NAME = "PopulatePackages";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();
                planningData.ArtifactImage = "artifact-RequirementSummary.svg";

                //Populate the items
                List<PlanningDataItem> dataItems = planningData.Items;
                foreach (RequirementView package in packages)
                {
					//Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = package.RequirementId;
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, package.ProjectId, package.RequirementId, GlobalFunctions.PARAMETER_TAB_TASK));

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = package.Name;
                    if (!String.IsNullOrWhiteSpace(package.Description))
                    {
                        dataItemField.Tooltip = "<u>" + package.Name + "</u><br />" + package.Description.StripHTML();
                    }
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //IndentLevel
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "IndentLevel";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = package.IndentLevel;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Task Progress
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Progress";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
                    PopulateRequirementEqualizer(dataItemField, package);
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Add the item
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Populates the list of resources as planning objects
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="projectResources">The list of resources</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulateResources(int projectId, List<ProjectResourceView> projectResources)
        {
            const string METHOD_NAME = "PopulateResources";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();

                //Populate the items
                List<PlanningDataItem> dataItems = planningData.Items;
                foreach (ProjectResourceView resource in projectResources)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = resource.UserId;
                    dataItem.Expanded = true;
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Resources, resource.ProjectId, resource.UserId, GlobalFunctions.PARAMETER_TAB_REQUIREMENT));

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = resource.FullName;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Planned Effort
                    if (resource.ResourceEffort.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "PlannedEffort";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                        dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(resource.ResourceEffort.Value) + "h";
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Utilized Effort
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "UtilizedEffort";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                    dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(resource.TotalEffort) + "h";
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Available/Remaining Effort
                    if (resource.RemainingEffort.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "AvailableEffort";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                        if (resource.RemainingEffort.HasValue)
                        {
                            dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(resource.RemainingEffort.Value) + "h";
                        }
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Progress
                    if (resource.ResourceEffort.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "Progress";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
                        PopulateResourceEqualizer(dataItemField, resource);
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Add the item
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Populates the list of releases as planning objects
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="releases">The list of releases</param>
        /// <param name="usePoints">Should we use points (otherwise use hours)</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulateReleases(int projectId, List<ReleaseView> releases, bool usePoints)
        {
            const string METHOD_NAME = "PopulateReleases";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();
                planningData.ArtifactImage = "artifact-Release.svg";

                //Populate the items
                List<PlanningDataItem> dataItems = planningData.Items;
                foreach (ReleaseView release in releases)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = release.ReleaseId;
                    dataItem.Expanded = true;
                    dataItem.CustomUrl = "spira://release"; //Choose a release link

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = release.FullName;
                    dataItemField.Tooltip = "<u>" + release.FullName + "</u>";
                    if (!String.IsNullOrEmpty(release.Description))
                    {
                        dataItemField.Tooltip += "<br />" + release.Description.StripHTML();
                    }
                    dataItemField.Tooltip += "<br/><i>" + release.StartDate.ToShortDateString();
                    dataItemField.Tooltip += " - " + release.EndDate.ToShortDateString() + "</i>";
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    if (usePoints)
                    {
                        //PlannedPoints => PlannedEffort field
                        if (release.PlannedPoints.HasValue)
                        {
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "PlannedEffort";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_POINTS, release.PlannedPoints.Value) + " " + Resources.Main.Global_Points;
                            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        }

                        //RequirementPoints => UtilizedEffort field
                        if (release.RequirementPoints.HasValue)
                        {
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "UtilizedEffort";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_POINTS, release.RequirementPoints.Value) + " " + Resources.Main.Global_Points;
                            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        }

                        //Available Effort
                        if (release.PlannedPoints.HasValue && release.RequirementPoints.HasValue)
                        {
                            decimal pointsRemaining = release.PlannedPoints.Value - release.RequirementPoints.Value;
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "AvailableEffort";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_POINTS, pointsRemaining) + " " + Resources.Main.Global_Points;
                            //Basically we only need to know if positive or negative to determine if we have exceeded the # allowed
                            dataItemField.IntValue = (int)(pointsRemaining * 10M);
                            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        }
                    }
                    else
                    {
                        //Planned Effort
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "PlannedEffort";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                        dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(release.PlannedEffort) + "h";
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //Utilized Effort (i.e. Projected Utilized not Estimated Utilized)
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "UtilizedEffort";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                        if (release.TaskProjectedEffort.HasValue)
                        {
                            dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(release.TaskProjectedEffort.Value) + "h";
                        }
                        else
                        {
                            dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(0) + "h";
                        }
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //Available Effort
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "AvailableEffort";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                        dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(release.AvailableEffort) + "h";
                        dataItemField.IntValue = release.AvailableEffort;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Progress
                    if (release.TaskCount > 0)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "Progress";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
                        PopulateReleaseEqualizer(dataItemField, release);
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //ReleaseId - used when the user clicks on it
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "ReleaseId";
                    dataItemField.IntValue = release.ReleaseId;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Add the item
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Populates the list of releases as planning objects
        /// </summary>
        /// <param name="usePoints">Should we use points (otherwise use hours)</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="releaseIterations">The list of release and iterations</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulateReleaseAndIterations(int projectId, List<ReleaseView> releaseIterations, bool usePoints)
        {
            const string METHOD_NAME = "PopulateReleaseAndIterations";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();
                planningData.ArtifactImage = "artifact-Release.svg";
                planningData.AlternateImage = "artifact-Iteration.svg";


                //Populate the items
                List<PlanningDataItem> dataItems = planningData.Items;
                foreach (ReleaseView releaseOrIteration in releaseIterations)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.Expanded = true;
                    dataItem.PrimaryKey = releaseOrIteration.ReleaseId;
                    dataItem.Alternate = releaseOrIteration.IsIteration;
                    dataItem.CustomUrl = "spira://release"; //Choose a release/iteration link

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = releaseOrIteration.FullName;
                    dataItemField.Tooltip = "<u>" + releaseOrIteration.FullName + "</u>";
                    if (!String.IsNullOrEmpty(releaseOrIteration.Description))
                    {
                        dataItemField.Tooltip += "<br />" + releaseOrIteration.Description.StripHTML();
                    }
                    dataItemField.Tooltip += "<br/><i>" + releaseOrIteration.StartDate.ToShortDateString();
                    dataItemField.Tooltip += " - " + releaseOrIteration.EndDate.ToShortDateString() + "</i>";
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    if (usePoints)
                    {
                        //PlannedPoints => PlannedEffort field
                        if (releaseOrIteration.PlannedPoints.HasValue)
                        {
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "PlannedEffort";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_POINTS, releaseOrIteration.PlannedPoints.Value) + " " + Resources.Main.Global_Points;
                            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        }

                        //RequirementPoints => UtilizedEffort field
                        if (releaseOrIteration.RequirementPoints.HasValue)
                        {
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "UtilizedEffort";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_POINTS, releaseOrIteration.RequirementPoints.Value) + " " + Resources.Main.Global_Points;
                            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        }

                        //Available Effort
                        if (releaseOrIteration.PlannedPoints.HasValue && releaseOrIteration.RequirementPoints.HasValue)
                        {
                            decimal pointsRemaining = releaseOrIteration.PlannedPoints.Value - releaseOrIteration.RequirementPoints.Value;
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "AvailableEffort";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_POINTS, pointsRemaining) + " " + Resources.Main.Global_Points;
                            //Basically we only need to know if positive or negative to determine if we have exceeded the # allowed
                            dataItemField.IntValue = (int)(pointsRemaining * 10M);
                            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        }
                    }
                    else
                    {
                        //Planned Effort
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "PlannedEffort";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                        dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(releaseOrIteration.PlannedEffort) + "h";
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //Utilized Effort (ie. the Projected Utilized not the Estimated Utilized)
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "UtilizedEffort";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                        if (releaseOrIteration.TaskProjectedEffort.HasValue)
                        {
                            dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(releaseOrIteration.TaskProjectedEffort.Value) + "h";
                        }
                        else
                        {
                            dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(0) + "h";
                        }
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //Available Effort
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "AvailableEffort";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                        dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(releaseOrIteration.AvailableEffort) + "h";
                        dataItemField.IntValue = releaseOrIteration.AvailableEffort;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Progress
                    if (releaseOrIteration.TaskCount > 0)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "Progress";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
                        PopulateReleaseEqualizer(dataItemField, releaseOrIteration);
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //ReleaseId - used when the user clicks on it
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "ReleaseId";
                    dataItemField.IntValue = releaseOrIteration.ReleaseId;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Add the item
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Populates the list of incidents as story card items
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="isPlannable">Set if you want to force the plannable flag to Y/N</param>
        /// <param name="incidents">The list of incidents</param>
        /// <param name="planningData">The requirements planning data to add to</param>
        /// <param name="includeDetails">Do we need to include the 'detailed view' data</param>
        /// <param name="fieldName">The field that we need to check workflow for</param>
        protected void PopulateIncidentItems(int projectId, PlanningData planningData, List<IncidentView> incidents, bool includeDetails, string fieldName, bool? isPlannable = null)
        {
            const string METHOD_NAME = "PopulateRequirementItems";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Populate the items
                List<PlanningDataItem> dataItems = planningData.Items;
				UserManager userManager = new UserManager();

				foreach (IncidentView incident in incidents)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = incident.IncidentId;
                    dataItem.Rank = incident.Rank;
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, incident.ProjectId, incident.IncidentId));

                    //ArtifactTypeId
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "ArtifactTypeId";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
                    dataItemField.IntValue = (int)incident.ArtifactType;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Token (prefix + id)
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Token";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = incident.ArtifactToken;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Name
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = incident.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Image (specified per-item)
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Image";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = "artifact-Incident.svg";
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Importance
                    if (incident.PriorityId.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "PriorityId";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                        dataItemField.IntValue = incident.PriorityId;
                        dataItemField.TextValue = incident.PriorityName;
                        dataItemField.CssClass = "#" + incident.PriorityColor;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Estimate (Incident = ProjectedEffort)
                    if (incident.ProjectedEffort.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "Estimate";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Decimal;
                        dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(incident.ProjectedEffort.Value) + "h";
                        dataItemField.Tooltip = GetEstimateToolTip(incident);
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Owner
                    if (incident.OwnerId.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "OwnerId";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                        dataItemField.IntValue = incident.OwnerId.Value;
                        dataItemField.TextValue = incident.OwnerName;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Owner has icon
						User owner = userManager.GetUserById(incident.OwnerId.Value);
						if (string.IsNullOrEmpty(owner.Profile.AvatarImage))
						{
							dataItem.OwnerIconInitials = owner.Profile.FirstName.Substring(0, 1).ToUpper() + owner.Profile.LastName.Substring(0, 1).ToUpper();
						}
					}

                    //PlannableYn (whether the item can be moved by the user)
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "PlannableYn";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Flag;
                    if (isPlannable.HasValue)
                    {
                        dataItemField.TextValue = (isPlannable.Value) ? "Y" : "N";
                    }
                    else
                    {
                        bool isWorkflowPlannable = IsIncidentFieldEnabled(incident.IncidentTypeId, incident.IncidentStatusId, fieldName);
                        dataItemField.TextValue = (isWorkflowPlannable) ? "Y" : "N";
                    }
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Include the data needed for the detailed view
                    if (includeDetails)
                    {
                        //Description
                        if (!String.IsNullOrEmpty(incident.Description))
                        {
                            //We send the description as plain-text
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "Description";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                            dataItemField.TextValue = incident.Description.StripHTML(false, true);
                            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        }

                        //Incident Progress
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "Progress";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
                        PopulateIncidentEqualizer(dataItemField, incident);
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    }

                    //Add the item
                    dataItems.Add(dataItem);
                }

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
        /// Returns the estimate bar tooltip for a requirement
        /// </summary>
        /// <param name="requirement">The requirement</param>
        /// <returns>The tooltip text</returns>
        protected string GetEstimateToolTip(RequirementView requirement)
        {
            string tooltip = "";

            //Estimate (in points)
            if (requirement.EstimatePoints.HasValue)
            {
                tooltip += Resources.Fields.Estimate + ": " + String.Format(GlobalFunctions.FORMAT_POINTS, requirement.EstimatePoints.Value) + " " + Resources.Main.Global_Points + "<br />\n";
            }

            //Estimated Effort
            if (requirement.EstimatedEffort.HasValue)
            {
                tooltip += Resources.Fields.EstimatedEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(requirement.EstimatedEffort.Value) + "h<br />\n";
            }

            //Task Estimated Effort
            if (requirement.TaskEstimatedEffort.HasValue)
            {
                tooltip += Resources.Fields.TaskEstimatedEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(requirement.TaskEstimatedEffort.Value) + "h<br />\n";
            }

            //Task Actual Effort
            if (requirement.TaskActualEffort.HasValue)
            {
                tooltip += Resources.Fields.TaskActualEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(requirement.TaskActualEffort.Value) + "h<br />\n";
            }

            //Task Remaining Effort
            if (requirement.TaskRemainingEffort.HasValue)
            {
                tooltip += Resources.Fields.TaskRemainingEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(requirement.TaskRemainingEffort.Value) + "h<br />\n";
            }

            //Task Projected Effort
            if (requirement.TaskProjectedEffort.HasValue)
            {
                tooltip += Resources.Fields.TaskProjectedEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(requirement.TaskProjectedEffort.Value) + "h<br />\n";
            }

            return tooltip;
        }

        /// <summary>
        /// Returns the estimate bar tooltip for an incident
        /// </summary>
        /// <param name="incident">The incident</param>
        /// <returns>The tooltip text</returns>
        protected string GetEstimateToolTip(IncidentView incident)
        {
            string tooltip = "";

            //Estimated Effort
            if (incident.EstimatedEffort.HasValue)
            {
                tooltip += Resources.Fields.EstimatedEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(incident.EstimatedEffort.Value) + "h<br />\n";
            }

            //Actual Effort
            if (incident.ActualEffort.HasValue)
            {
                tooltip += Resources.Fields.ActualEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(incident.ActualEffort.Value) + "h<br />\n";
            }

            //Remaining Effort
            if (incident.RemainingEffort.HasValue)
            {
                tooltip += Resources.Fields.RemainingEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(incident.RemainingEffort.Value) + "h<br />\n";
            }

            //Projected Effort
            if (incident.ProjectedEffort.HasValue)
            {
                tooltip += Resources.Fields.ProjectedEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(incident.ProjectedEffort.Value) + "h<br />\n";
            }

            return tooltip;
        }

        /// <summary>
        /// Returns the estimate bar tooltip for a task
        /// </summary>
        /// <param name="task">The task</param>
        /// <returns>The tooltip text</returns>
        protected string GetEstimateToolTip(TaskView task)
        {
            string tooltip = "";

            //Estimated Effort
            if (task.EstimatedEffort.HasValue)
            {
                tooltip += Resources.Fields.EstimatedEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(task.EstimatedEffort.Value) + "h<br />\n";
            }

            //Actual Effort
            if (task.ActualEffort.HasValue)
            {
                tooltip += Resources.Fields.ActualEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(task.ActualEffort.Value) + "h<br />\n";
            }

            //Remaining Effort
            if (task.RemainingEffort.HasValue)
            {
                tooltip += Resources.Fields.RemainingEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(task.RemainingEffort.Value) + "h<br />\n";
            }

            //Projected Effort
            if (task.ProjectedEffort.HasValue)
            {
                tooltip += Resources.Fields.ProjectedEffort + ": " + GlobalFunctions.GetEffortInFractionalHours(task.ProjectedEffort.Value) + "h<br />\n";
            }

            return tooltip;
        }

        /// <summary>
        /// Populates the list of requirements as story card items
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
		/// <param name="projectTemplateId">The id of the current template</param>
        /// <param name="requirements">The list of requirements</param>
        /// <param name="includeDetails">Do we need to include the 'detailed view' data</param>
        /// <param name="includeTasks">Do we include associated tasks</param>
        /// <param name="includeTestCases">Do we include associated test cases</param>
        /// <returns>The planning data</returns>
        /// <param name="fieldName">The field that we need to check workflow for</param>
        protected PlanningData PopulateRequirementItems(int projectId, int projectTemplateId, List<RequirementView> requirements, bool includeDetails, bool includeTasks, bool includeTestCases, string fieldName)
        {
            const string METHOD_NAME = "PopulateRequirementItems";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();
				UserManager userManager = new UserManager();

				TaskManager taskManager = null;
                if (includeTasks)
                {
                    taskManager = new TaskManager();
                }

                TestCaseManager testCaseManager = null;
                if (includeTestCases)
                {
                    testCaseManager = new TestCaseManager();
                }

				bool canChangeStatus = true;
				if (fieldName == "RequirementStatusId")
				{
					//See if we are allowed to bulk edit status (template setting)
					ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(projectTemplateId);
					canChangeStatus = templateSettings.Workflow_BulkEditCanChangeStatus;
				}

				//Populate the items
				List<PlanningDataItem> dataItems = planningData.Items;
                foreach (RequirementView requirement in requirements)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = requirement.RequirementId;
                    dataItem.Rank = requirement.Rank;
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, requirement.ProjectId, requirement.RequirementId, GlobalFunctions.PARAMETER_TAB_TASK));

                    //ArtifactTypeId
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "ArtifactTypeId";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
                    dataItemField.IntValue = (int)requirement.ArtifactType;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Token (prefix + id)
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Token";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = requirement.ArtifactToken;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Name
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = requirement.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Image (specified per-item)
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Image";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = (requirement.RequirementTypeIsSteps) ? "artifact-UseCase.svg" : "artifact-Requirement.svg";
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Importance
                    if (requirement.ImportanceId.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "ImportanceId";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                        dataItemField.IntValue = requirement.ImportanceId;
                        dataItemField.TextValue = requirement.ImportanceName;
                        dataItemField.CssClass = "#" + requirement.ImportanceColor;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Estimate
                    if (requirement.EstimatePoints.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "Estimate";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Decimal;
                        dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_POINTS, requirement.EstimatePoints.Value);
                        dataItemField.Tooltip = GetEstimateToolTip(requirement);
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Owner
                    if (requirement.OwnerId.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "OwnerId";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                        dataItemField.IntValue = requirement.OwnerId.Value;
                        dataItemField.TextValue = requirement.OwnerName;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Owner has icon
						User owner = userManager.GetUserById(requirement.OwnerId.Value);
						if (string.IsNullOrEmpty(owner.Profile.AvatarImage))
						{
							dataItem.OwnerIconInitials = owner.Profile.FirstName.Substring(0, 1).ToUpper() + owner.Profile.LastName.Substring(0, 1).ToUpper();
						}
					}

					//PlannableYn (whether the item can be moved by the user)
					bool isPlannable;
					if (fieldName == "RequirementStatusId")
					{
						isPlannable = canChangeStatus;
					}
					else
					{
						isPlannable = IsRequirementFieldEnabled(projectId, requirement.RequirementTypeId, requirement.RequirementStatusId, fieldName);
					}
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "PlannableYn";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Flag;
                    dataItemField.TextValue = (isPlannable) ? "Y" : "N";
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Include the data needed for the detailed view
                    if (includeDetails)
                    {
                        //Description
                        if (!String.IsNullOrEmpty(requirement.Description))
                        {
                            //We send the description as plain-text
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "Description";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                            dataItemField.TextValue = requirement.Description.StripHTML(false, true);
                            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        }

                        //Task Progress
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "Progress";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
                        PopulateRequirementEqualizer(dataItemField, requirement);
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //Task Count
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "TaskCount";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
                        dataItemField.IntValue = requirement.TaskCount;
                        dataItemField.Tooltip = requirement.TaskCount + " " + Resources.Fields.Tasks;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //Test Case Count
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "TestCaseCount";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
                        dataItemField.IntValue = requirement.CoverageCountTotal;
                        dataItemField.Tooltip = requirement.CoverageCountTotal + " " + Resources.Main.SiteMap_TestCases;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //See if we need to add associated tasks
                    if (includeTasks)
                    {
                        List<TaskView> tasks = taskManager.RetrieveByRequirementId(requirement.RequirementId);
                        dataItem.ChildTasks = new List<DataItem>();

                        //Populate
                        foreach (TaskView task in tasks)
                        {
                            DataItem childTask = new DataItem();
                            childTask.PrimaryKey = task.TaskId;
                            childTask.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, task.ProjectId, task.TaskId));
                            //Token (prefix + id)
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "Token";
                            dataItemField.TextValue = task.ArtifactToken;
                            if (String.IsNullOrWhiteSpace(task.Description))
                            {
                                dataItemField.Tooltip = task.ArtifactToken + ": " + task.Name;
                            }
                            else
                            {
                                dataItemField.Tooltip = "<u>" + task.ArtifactToken + ": " + task.Name + "</u><br />" + task.Description.StripHTML();
                            }
                            childTask.Fields.Add(dataItemField.FieldName, dataItemField);

                            //Progress
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "Progress";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
                            PopulateTaskEqualizer(dataItemField, task);
                            childTask.Fields.Add(dataItemField.FieldName, dataItemField);

                            //Add child task to collection
                            dataItem.ChildTasks.Add(childTask);
                        }
                    }

                    //See if we need to add associated test cases
                    if (includeTestCases)
                    {
                        List<TestCase> testCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirement.RequirementId);
                        dataItem.ChildTestCases = new List<DataItem>();

                        //Populate
                        foreach (TestCase testCase in testCases)
                        {
                            DataItem childTestCase = new DataItem();
                            childTestCase.PrimaryKey = testCase.TestCaseId;
                            childTestCase.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, testCase.ProjectId, testCase.TestCaseId));
                            //Token (prefix + id)
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "Token";
                            dataItemField.TextValue = GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE + "-" + testCase.TestCaseId;
                            if (String.IsNullOrEmpty(testCase.Description))
                            {
                                dataItemField.Tooltip = dataItemField.TextValue + ": " + testCase.Name;
                            }
                            else
                            {
                                dataItemField.Tooltip = "<u>" + dataItemField.TextValue + ": " + testCase.Name + "</u><br />" + testCase.Description.StripHTML();
                            }
                            childTestCase.Fields.Add(dataItemField.FieldName, dataItemField);

                            //Execution Status
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "ExecutionStatus";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
                            PopulateTestCaseEqualizer(dataItemField, testCase);
                            childTestCase.Fields.Add(dataItemField.FieldName, dataItemField);

                            //Add child test case to collection
                            dataItem.ChildTestCases.Add(childTestCase);
                        }
                    }

                    //Add the item
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return planningData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Populates the equalizer type graph for the incident progress
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="incidentView">The incident</param>
        protected void PopulateIncidentEqualizer(DataItemField dataItemField, IncidentView incidentView)
        {
            const string METHOD_NAME = "PopulateIncidentEqualizer";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Calculate the information to display
            int percentGreen;
            int percentRed;
            int percentYellow;
            int percentGray;
            Incident incident = incidentView.ConvertTo<IncidentView, Incident>();
            string tooltipText = IncidentManager.CalculateProgress(incident, out percentGreen, out percentRed, out percentYellow, out percentGray);

            //Now populate the equalizer graph
            dataItemField.EqualizerGreen = percentGreen;
            dataItemField.EqualizerRed = percentRed;
            dataItemField.EqualizerYellow = percentYellow;
            dataItemField.EqualizerGray = percentGray;

            //Populate Tooltip
            dataItemField.TextValue = "";
            dataItemField.Tooltip = tooltipText;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>
        /// Populates the equalizer type graph for the test case status
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="testCase">The test case row</param>
        protected void PopulateTestCaseEqualizer(DataItemField dataItemField, TestCase testCase)
        {
            //Now populate the equalizer graph (always shows just one color
            if (testCase.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.NotApplicable)
            {
                dataItemField.EqualizerGreen = (testCase.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Passed) ? 100 : 0;
                dataItemField.EqualizerRed = (testCase.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Failed) ? 100 : 0; ;
                dataItemField.EqualizerYellow = (testCase.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Blocked) ? 100 : 0; ;
                dataItemField.EqualizerGray = (testCase.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.NotRun) ? 100 : 0;
                dataItemField.EqualizerOrange = (testCase.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Caution) ? 100 : 0;
            }

            //Populate Tooltip
            dataItemField.TextValue = "";
            if (!String.IsNullOrEmpty(testCase.ExecutionStatusName))
            {
                dataItemField.Tooltip = testCase.ExecutionStatusName;
            }
        }

        /// <summary>
        /// Populates the equalizer type graph for the task progress
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="taskView">The task row</param>
        protected void PopulateTaskEqualizer(DataItemField dataItemField, TaskView taskView)
        {
            //Calculate the information to display
            int percentGreen;
            int percentRed;
            int percentYellow;
            int percentGray;
            Task task = taskView.ConvertTo<TaskView, Task>();
            string tooltipText = TaskManager.CalculateProgress(task, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out percentGreen, out percentRed, out percentYellow, out percentGray);

            //Now populate the equalizer graph
            dataItemField.EqualizerGreen = percentGreen;
            dataItemField.EqualizerRed = percentRed;
            dataItemField.EqualizerYellow = percentYellow;
            dataItemField.EqualizerGray = percentGray;

            //Populate Tooltip
            dataItemField.TextValue = "";
            dataItemField.Tooltip = tooltipText;
        }

        /// <summary>
        /// Populates the equalizer type graph for releases
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="release">The release</param>
        protected void PopulateReleaseEqualizer(DataItemField dataItemField, ReleaseView release)
        {
            //Populate the percentages                    

            dataItemField.EqualizerGreen = (release.PercentComplete < 0) ? 0 : release.PercentComplete;
            dataItemField.EqualizerGray = (release.PercentComplete < 0) ? 100 : (100 - release.PercentComplete);

            /* These were used when we used to use the Task Progress equalizer instead of the new Requirements % Complete
            dataItemField.EqualizerGreen = (release.TaskPercentOnTime < 0) ? 0 : release.TaskPercentOnTime;
            dataItemField.EqualizerRed = (release.TaskPercentLateFinish < 0) ? 0 : release.TaskPercentLateFinish;
            dataItemField.EqualizerYellow = (release.TaskPercentLateStart < 0) ? 0 : release.TaskPercentLateStart;
            dataItemField.EqualizerGray = (release.TaskPercentNotStart < 0) ? 0 : release.TaskPercentNotStart;
            */

            //Populate Tooltip
            dataItemField.TextValue = "";
            dataItemField.Tooltip = ReleaseManager.GenerateReqCompletionTooltip(release);
        }

        /// <summary>
        /// Populates the equalizer type graph for resources
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="resource">The resource</param>
        protected void PopulateResourceEqualizer(DataItemField dataItemField, ProjectResourceView resource)
        {
            //Calculate the information to display
            int percentGreen;
            int percentRed;
            int percentYellow;
            int percentGray;
            string tooltipText = ProjectManager.CalculateResourceProgress(resource, out percentGreen, out percentRed, out percentYellow, out percentGray);

            //Now populate the equalizer graph
            dataItemField.EqualizerGreen = percentGreen;
            dataItemField.EqualizerRed = percentRed;
            dataItemField.EqualizerYellow = percentYellow;
            dataItemField.EqualizerGray = percentGray;

            //Populate Tooltip
            dataItemField.TextValue = "";
            dataItemField.Tooltip = tooltipText;
        }

        /// <summary>
        /// Populates the equalizer type graph for requirements
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="requirement">The requirement</param>
        protected void PopulateRequirementEqualizer(DataItemField dataItemField, RequirementView requirement)
        {
            //First see how many tasks we have
            int taskCount = requirement.TaskCount;

            //Handle the no tasks case first
            if (taskCount == 0)
            {
                dataItemField.Tooltip = RequirementManager.GenerateTaskProgressTooltip(requirement);
                dataItemField.TextValue = RequirementManager.GenerateTaskProgressTooltip(requirement);
                dataItemField.CssClass = "NotCovered";
            }
            else
            {
                //Populate the percentages
                dataItemField.EqualizerGreen = (requirement.TaskPercentOnTime < 0) ? 0 : requirement.TaskPercentOnTime;
                dataItemField.EqualizerRed = (requirement.TaskPercentLateFinish < 0) ? 0 : requirement.TaskPercentLateFinish;
                dataItemField.EqualizerYellow = (requirement.TaskPercentLateStart < 0) ? 0 : requirement.TaskPercentLateStart;
                dataItemField.EqualizerGray = (requirement.TaskPercentNotStart < 0) ? 0 : requirement.TaskPercentNotStart;

                //Populate Tooltip
                dataItemField.TextValue = "";
                dataItemField.Tooltip = RequirementManager.GenerateTaskProgressTooltip(requirement);
            }
        }

        /// <summary>
        /// Is the requirement field enabled for the specified type, status and field
        /// </summary>
        /// <param name="projectId">The project</param>
        /// <param name="requirementTypeId">The requirement type</param>
        /// <param name="requirementStatusId">The requirement status</param>
        /// <param name="fieldName">The field to check</param>
        /// <returns>True if the field is enabled</returns>
        public bool IsRequirementFieldEnabled(int projectId, int requirementTypeId, int requirementStatusId, string fieldName)
        {
            //If no field specified or requirement type is unset, return true
            if (String.IsNullOrEmpty(fieldName) || requirementTypeId < 1)
            {
                return true;
            }

            //See if we have it in our cache
            List<RequirementWorkflowField> fields;
            if (requirementWorkflowStates.ContainsKey(projectId + "_" + requirementTypeId + "_" + requirementStatusId))
            {
                fields = requirementWorkflowStates[projectId + "_" + requirementTypeId + "_" + requirementStatusId];
            }
            else
            {
                RequirementWorkflowManager requirementWorkflowManager = new RequirementWorkflowManager();
                int workflowId = requirementWorkflowManager.Workflow_GetForRequirementType(requirementTypeId);
                fields = requirementWorkflowManager.Workflow_RetrieveFieldStates(workflowId, requirementStatusId);
                requirementWorkflowStates.Add(projectId + "_" + requirementTypeId + "_" + requirementStatusId, fields);
            }
            return (!fields.Any(f => f.ArtifactField.Name == fieldName && (f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive || f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden)));
        }

        /// <summary>
        /// Is the incident field enabled for the specified type, status and field
        /// </summary>
        /// <param name="incidentTypeId">The requirement type</param>
        /// <param name="incidentStatusId">The requirement status</param>
        /// <param name="fieldName">The field to check</param>
        /// <returns>True if the field is enabled</returns>
        public bool IsIncidentFieldEnabled(int incidentTypeId, int incidentStatusId, string fieldName)
        {
            //If no field specific, return true
            if (String.IsNullOrEmpty(fieldName))
            {
                return true;
            }

            //See if we have it in our cache
            List<WorkflowField> fields;
            if (incidentWorkflowStates.ContainsKey(incidentTypeId + "_" + incidentStatusId))
            {
                fields = incidentWorkflowStates[incidentTypeId + "_" + incidentStatusId];
            }
            else
            {
                WorkflowManager incidentWorkflowManager = new WorkflowManager();
                int workflowId = incidentWorkflowManager.Workflow_GetForIncidentType(incidentTypeId);
                fields = incidentWorkflowManager.Workflow_RetrieveFieldStates(workflowId, incidentStatusId);
                incidentWorkflowStates.Add(incidentTypeId + "_" + incidentStatusId, fields);
            }
            return (!fields.Any(f => f.Field.Name == fieldName && (f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive || f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden)));
        }

        #endregion

        #region Other Public Methods

        /// <summary>
        /// Clears the cached requirement workflow field states (called by admin when workflows change)
        /// </summary>
        public static void ClearRequirementWorkflowCache()
        {
            requirementWorkflowStates = new Dictionary<string, List<RequirementWorkflowField>>();
        }

        /// <summary>
        /// Clears the cached incident workflow field states (called by admin when workflows change)
        /// </summary>
        public static void ClearIncidentWorkflowCache()
        {
            incidentWorkflowStates = new Dictionary<string, List<WorkflowField>>();
        }

        #endregion
    }
}
