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
    /// Communicates with the PlanningBoard AJAX component for the Task Board
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class TaskBoardService : AjaxWebServiceBase, ITaskBoardService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TaskBoardService::";

        protected const string EXPANDED_KEY_UNASSIGNED = "Unassigned";
        protected const int CONTAINER_ID_CURRENT_RELEASE = -2;
        protected const int CONTAINER_ID_CURRENT_ITERATION = -3;

        //Cached workflow states
        static Dictionary<string, List<TaskWorkflowField>> taskWorkflowStates = new Dictionary<string, List<TaskWorkflowField>>();

        #region Other Public Methods

        /// <summary>
        /// Clears the cached task workflow field states (called by admin when workflows change)
        /// </summary>
        public static void ClearTaskWorkflowCache()
        {
            taskWorkflowStates = new Dictionary<string, List<TaskWorkflowField>>();
        }

        #endregion

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
                ProjectSettingsCollection planningBoardSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS);
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
                ProjectSettingsCollection planningBoardSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS);
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
                ProjectSettingsCollection planningBoardSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS);
                switch (option)
                {
                    case "IncludeDetails":
                        planningBoardSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_DETAILS] = optionValue;
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

                //Get the list of expanded items
                ProjectSettingsCollection expandedItems = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_EXPANDED_GROUPS);

                //Get the specified release or iteration
                ReleaseManager releaseManager = new ReleaseManager();
                ReleaseView releaseOrIteration = releaseManager.RetrieveById2(projectId, releaseId);
                planningData = PopulateReleaseAndIterations(projectId, new List<ReleaseView>() { releaseOrIteration });
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
                        PlanningData parentItemPlanningData = PopulateReleaseAndIterations(projectId, parentReleases);
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

            //Now retrieve the specific task - handle quietly if it doesn't exist
            try
            {
                //See if we have a task
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

            //Make sure we're authorized to view tasks
            Artifact.ArtifactTypeEnum authorizedArtifact = Artifact.ArtifactTypeEnum.Task;
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, authorizedArtifact);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The data being returned
                PlanningData planningData = null;

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//See what we're grouping by
				TaskManager taskManager = new TaskManager();
                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPriority)
                {
                    //Pass null for release Id if we're considering 'all releases'
                    List<TaskView> tasks = taskManager.Task_RetrieveByPriorityId(projectId, (releaseId == -2) ? null : (int?)releaseId, containerId);

                    //Populate the data objects
                    planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "TaskPriorityId");
                }

                //The next set are all for the case of 'all releases' (i.e. releaseId == -2)

                if (releaseId == -2)
                {
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByRelease)
                    {
                        //Get the list of tasks by release (including child iterations)
                        List<TaskView> tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, containerId);

                        //Populate the data objects
                        planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "ReleaseId");
                    }

                    //By status for all releases
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus && containerId.HasValue)
                    {
                        //Get the list of tasks by status for all releases
                        List<TaskView> tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, -2, containerId.Value);

                        //Populate the data objects
                        planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "TaskStatusId");
                    }

                    //Gets the items for all releases in the project, organized by person (or unassigned)
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPerson)
                    {
                        //Get the list of tasks by resource/user for all releases in the project
                        List<TaskView> tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, -2, containerId, true);

                        //Populate the data objects
                        planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "OwnerId");
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

                        //Get the list of tasks by release/iteration
                        List<TaskView> tasks = taskManager.RetrieveByReleaseId(projectId, containerId, false);

                        //Populate the data objects
                        planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "ReleaseId");
                    }

                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByRequirement && containerId.HasValue && containerId > 0)
                    {
                        //We need to retrieve the tasks that belong the specified requirement
                        List<TaskView> tasks = taskManager.RetrieveByRequirementId(containerId.Value);

                        //Populate the data objects
                        planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "RequirementId");
                    }

                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                    {
                        //If we're asked to get the ones associated with the release, retrieve just those for that release only
                        //We will be given the current iteration in this case, so need to get the parent
                        List<TaskView> tasks;
                        if (containerId == CONTAINER_ID_CURRENT_RELEASE && isIteration)
                        {
                            //Get the list of tasks by the parent release of the current iteration
                            int? parentReleaseId = new ReleaseManager().GetParentReleaseId(releaseId);
                            if (parentReleaseId.HasValue)
                            {
                                tasks = taskManager.RetrieveByReleaseId(projectId, parentReleaseId.Value);
                            }
                            else
                            {
                                //Otherwise just return an empty list
                                tasks = new List<TaskView>();
                            }
                        }
                        else
                        {
                            //Get the list of tasks by tasks status
                            tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, releaseId, containerId);
                        }
                        //Populate the data objects
                        planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "TaskStatusId");
                    }

                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPerson)
                    {
                        //If we're asked to get the ones associated with the release, retrieve just those for that release only
                        if (containerId == CONTAINER_ID_CURRENT_RELEASE)
                        {
                            //If we're given the current iteration, need to get the parent release
                            if (isIteration)
                            {
                                //Get the list of tasks by the parent release of the current iteration that has no person assigned
                                int? parentReleaseId = new ReleaseManager().GetParentReleaseId(releaseId);
                                if (parentReleaseId.HasValue)
                                {
                                    List<TaskView> tasks = new TaskManager().Task_RetrieveBacklogByUserId(projectId, parentReleaseId.Value, null, false);

                                    //Populate the data objects
                                    planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "OwnerId");
                                }
                                else
                                {
                                    //Just return an empty data object
                                    planningData = new PlanningData();
                                }
                            }
                            else
                            {
                                //Get the tasks for the current release that have no person assigned
                                List<TaskView> tasks = new TaskManager().Task_RetrieveBacklogByUserId(projectId, releaseId, null, true);

                                //Populate the data objects
                                planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "OwnerId");
                            }
                        }
                        else if (containerId == CONTAINER_ID_CURRENT_ITERATION)
                        {
                            //Get the tasks for the current iteration that have no person assigned
                            List<TaskView> tasks = new TaskManager().Task_RetrieveBacklogByUserId(projectId, releaseId, null, false);

                            //Populate the data objects
                            planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "OwnerId");
                        }
                        else if (!containerId.HasValue)
                        {
                            //Get the list of tasks where they are not assigned to a release or person
                            List<TaskView> tasks = new TaskManager().Task_RetrieveBacklogByUserId(projectId, null, null, false);

                            //Populate the data objects
                            planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "OwnerId");
                        }
                        else
                        {
                            //Get the list of tasks by resource/user for this release/iteration
                            List<TaskView> tasks = new TaskManager().Task_RetrieveBacklogByUserId(projectId, releaseId, containerId, true);

                            //Populate the data objects
                            planningData = PopulateTaskItems(projectId, projectTemplateId, tasks, includeDetails, "OwnerId");
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

            //Make sure we're authorized to view tasks
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Task);
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

                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPriority)
                {
                    //Get the list of task priorities
                    TaskManager taskManager = new TaskManager();
                    List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);

                    //Populate the data objects
                    planningData = PopulatePriorities(projectId, priorities);
                }

                if (releaseId == -2 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByRelease)
                {
                    //Get the list of releases (not iterations)
                    ReleaseManager releaseManager = new ReleaseManager();
                    List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, true, false);

                    //Populate the data objects
                    planningData = PopulateReleases(projectId, releases);
                }

                if (releaseId == -2 && groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                {
                    //Get the list of task stati
                    TaskManager taskManager = new TaskManager();
                    List<TaskStatus> taskStati = taskManager.RetrieveStatusesInUse(projectTemplateId);

                    //Populate the data objects
                    planningData = PopulateStati(projectId, taskStati);
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
                        planningData = PopulateReleaseAndIterations(projectId, iterations);
                    }

                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPerson)
                    {
                        //Get the list of people in the project
                        ProjectManager projectManager = new ProjectManager();
                        List<ProjectResourceView> projectResources = projectManager.RetrieveResources(projectId, releaseId);

                        //Populate the data objects
                        planningData = PopulateResources(projectId, projectResources);
                    }

                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                    {
                        //Get the list of task stati
                        TaskManager taskManager = new TaskManager();
                        List<TaskStatus> taskStati = taskManager.RetrieveStatusesInUse(projectTemplateId);

                        //Populate the data objects
                        planningData = PopulateStati(projectId, taskStati);
                    }

                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByRequirement)
                    {
                        //Get the list of requirements for this release/iteration
                        RequirementManager requirementManager = new RequirementManager();
                        List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, releaseId, true);

                        //Populate the data objects
                        planningData = PopulateRequirements(projectId, requirements);
                    }
                }

                //See which ones are expanded or not (only used for horizontal style board)
                if (planningData != null && planningData.Items.Count > 0)
                {
                    ProjectSettingsCollection expandedGroups = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_EXPANDED_GROUPS);
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

            //Make sure we're authorized for this project (since it's just updating a user setting, just check they can view tasks)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Task);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the list of expanded items
                ProjectSettingsCollection expandedItems = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_EXPANDED_GROUPS);

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

            //Make sure we're authorized to modify tasks
            Artifact.ArtifactTypeEnum authorizedArtifact = Artifact.ArtifactTypeEnum.Task;
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, authorizedArtifact);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //See what we're grouping by
                TaskManager taskManager = new TaskManager();

                //By Priority
                if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPriority)
                {
                    if (containerId == -1)
                    {
                        containerId = null;
                    }

                    //Extract the task ids
                    List<int> taskIds = ExtractTaskIds(items);

                    //Update the tasks
                    foreach (int taskId in taskIds)
                    {
                        try
                        {
                            Task task = taskManager.RetrieveById(taskId);
                            task.StartTracking();
                            task.TaskPriorityId = containerId;
                            task.LastUpdateDate = DateTime.UtcNow;
                            task.ConcurrencyDate = DateTime.UtcNow;
                            taskManager.Update(task, userId);
                        }
                        catch (ArtifactNotExistsException)
                        {
                            //Ignore, just don't update the item
                        }
                    }
                }

                //For the next options, we handle the All-Releases (releaseId == -2) case separately
                if (releaseId == -2)
                {
                    //By Release
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByRelease)
                    {
                        //Extract the task ids
                        List<int> taskIds = ExtractTaskIds(items);

                        if (containerId.HasValue && containerId > 0)
                        {
                            //Update the tasks
                            taskManager.AssociateToIteration(taskIds, containerId.Value, userId);
                        }
                        else
                        {
                            //Update the tasks
                            taskManager.RemoveReleaseAssociation(taskIds, userId);
                        }
                    }

                    //By Status
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus && containerId.HasValue && containerId != -1)
                    {
                        //Extract the task ids
                        List<int> taskIds = ExtractTaskIds(items);

                        //Update the tasks
                        foreach (int taskId in taskIds)
                        {
                            try
                            {
                                Task task = taskManager.RetrieveById(taskId);
                                task.StartTracking();
                                task.LastUpdateDate = DateTime.UtcNow;
                                task.ConcurrencyDate = DateTime.UtcNow;
                                task.TaskStatusId = containerId.Value;
                                taskManager.Update(task, userId);
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
                        //Extract the task ids
                        List<int> taskIds = ExtractTaskIds(items);

                        //Update the tasks
                        int? ownerId = containerId;
                        if (containerId == -1)
                        {
                            //Unassign
                            ownerId = null;
                        }
                        foreach (int taskId in taskIds)
                        {
                            try
                            {
                                taskManager.AssignToUser(taskId, ownerId, userId);
                            }
                            catch (ArtifactNotExistsException)
                            {
                                //Ignore, just don't update the item
                            }
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

                        //Extract the task ids
                        List<int> taskIds = ExtractTaskIds(items);

                        if (containerId.HasValue)
                        {
                            //Update the tasks
                            taskManager.AssociateToIteration(taskIds, containerId.Value, userId);
                        }
                        else
                        {
                            //Update the tasks
                            taskManager.RemoveReleaseAssociation(taskIds, userId);
                        }
                    }

                    //By Person
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByPerson)
                    {
                        //Extract the task ids
                        List<int> taskIds = ExtractTaskIds(items);

                        //If we are viewing by Person, the user may drop the item on a release or iteration container
                        //this is the special case
                        int? releaseOrIterationId = null;
                        int? ownerId = containerId;
                        if (!containerId.HasValue || containerId == -1)
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
                                //Get the list of tasks by the parent release of the current iteration that has no person assigned
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

                        //Update the tasks
                        foreach (int taskId in taskIds)
                        {
                            try
                            {
                                taskManager.AssignToUser(taskId, ownerId, userId);
                            }
                            catch (ArtifactNotExistsException)
                            {
                                //Ignore, just don't update the item
                            }
                        }

                        //Change the release/iteration if necessary. The called functions will check to see if a change
                        //actually needs to happen
                        if (releaseOrIterationId.HasValue)
                        {
                            //Update the tasks
                            taskManager.AssociateToIteration(taskIds, releaseOrIterationId.Value, userId);
                        }
                        else
                        {
                            //Update the tasks
                            taskManager.RemoveReleaseAssociation(taskIds, userId);
                        }
                    }

                    //By Requirement
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByRequirement && containerId.HasValue && containerId > 0)
                    {
                        //Extract the task ids
                        List<int> taskIds = ExtractTaskIds(items);

                        //Change the requirement field
                        foreach (int taskId in taskIds)
                        {
                            taskManager.Task_AssociateWithRequirement(taskId, containerId.Value, userId);
                        }
                    }

                    //By Status
                    if (groupById == (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus)
                    {
                        if (containerId == -1)
                        {
                            containerId = null;
                        }

                        //Extract the task ids
                        List<int> taskIds = ExtractTaskIds(items);

                        //If we are viewing an Iteration (not release) and they drag the item to the parent
                        //release, leave the status unchangedm but change the release to the parent
                        int? parentReleaseId = null;
                        if (containerId == CONTAINER_ID_CURRENT_RELEASE && isIteration)
                        {
                            //Get the list of tasks by the parent release of the current iteration
                            parentReleaseId = new ReleaseManager().GetParentReleaseId(releaseId);
                        }

                        //Update the tasks
                        foreach (int taskId in taskIds)
                        {
                            try
                            {
                                Task task = taskManager.RetrieveById(taskId);
                                task.StartTracking();
                                task.LastUpdateDate = DateTime.UtcNow;
                                task.ConcurrencyDate = DateTime.UtcNow;
                                if (containerId == CONTAINER_ID_CURRENT_RELEASE && isIteration)
                                {
                                    if (parentReleaseId.HasValue)
                                    {
                                        task.ReleaseId = parentReleaseId;
                                    }
                                }
                                else if (!containerId.HasValue)
                                {
                                    //If we have no status specified, it means they are dragging the item back to the product backlog
                                    //in which case, set status to 'Not Started' and unset the release
                                    task.TaskStatusId = (int)Task.TaskStatusEnum.NotStarted;
                                    task.ReleaseId = null;
                                }
                                else
                                {
                                    if (!task.ReleaseId.HasValue)
                                    {
                                        task.ReleaseId = releaseId;
                                    }
                                    task.TaskStatusId = containerId.Value;
                                }

                                taskManager.Update(task, userId);
                            }
                            catch (ArtifactNotExistsException)
                            {
                                //Ignore, just don't update the item
                            }
                        }
                    }
                }

                //Tasks don't currently have a Rank option

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
        /// Populates the list of requirements as planning objects
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="requirements">The list of requirements</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulateRequirements(int projectId, List<RequirementView> requirements)
        {
            const string METHOD_NAME = "PopulateRequirements";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();
                planningData.ArtifactImage = "artifact-Requirement.svg";

                //Populate the items
                List<PlanningDataItem> dataItems = planningData.Items;
                foreach (RequirementView requirement in requirements)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = requirement.RequirementId;
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, requirement.ProjectId, requirement.RequirementId, GlobalFunctions.PARAMETER_TAB_TASK));

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = requirement.Name;
                    if (String.IsNullOrWhiteSpace(requirement.Description))
                    {
                        dataItemField.Tooltip = "<u>" + requirement.Name + " [" + requirement.ArtifactToken + "]</u>";
                    }
                    else
                    {
                        dataItemField.Tooltip = "<u>" + requirement.Name + " [" + requirement.ArtifactToken + "]</u><br />" + requirement.Description.StripHTML();
                    }
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //For the tasks by requirement view we don't want to show the indent level, keep it flat
                    /*
                    //IndentLevel
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "IndentLevel";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = requirement.IndentLevel;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    */

                    //Task Progress
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Progress";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
                    PopulateRequirementEqualizer(dataItemField, requirement);
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
        /// Populates the list of priorities as planning objects
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="priorities">The list of importances</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulatePriorities(int projectId, List<TaskPriority> priorities)
        {
            const string METHOD_NAME = "PopulatePriorities";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();

                //Populate the items     
                List<PlanningDataItem> dataItems = planningData.Items;
                foreach (TaskPriority priority in priorities)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = priority.TaskPriorityId;

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = priority.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Color
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "CssClass";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = priority.Color;
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
        /// Populates the list of task stati as planning objects
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="taskStati">The list of task stati</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulateStati(int projectId, List<TaskStatus> taskStati)
        {
            const string METHOD_NAME = "PopulateStati";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();

                //Populate the items     
                List<PlanningDataItem> dataItems = planningData.Items;
                foreach (TaskStatus taskStatus in taskStati)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = taskStatus.TaskStatusId;
                    dataItem.Expanded = true;

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = taskStatus.Name;
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
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "PlannedEffort";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                    if (resource.ResourceEffort.HasValue)
                    {
                        dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(resource.ResourceEffort.Value) + "h";
                    }
                    else
                    {
                        dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(0) + "h";
                    }
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Utilized Effort
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "UtilizedEffort";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                    dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(resource.TotalEffort) + "h";
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Available Effort
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "AvailableEffort";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                    if (resource.RemainingEffort.HasValue)
                    {
                        dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(resource.RemainingEffort.Value) + "h";
                    }
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

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
        /// <returns>The planning data</returns>
        protected PlanningData PopulateReleases(int projectId, List<ReleaseView> releases)
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
        /// <param name="projectId">The id of the current project</param>
        /// <param name="releaseIterations">The list of release and iterations</param>
        /// <returns>The planning data</returns>
        protected PlanningData PopulateReleaseAndIterations(int projectId, List<ReleaseView> releaseIterations)
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
        /// Populates the list of tasks as story card items
        /// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="tasks">The list of tasks</param>
        /// <param name="includeDetails">Do we need to include the 'detailed view' data</param>
        /// <param name="fieldName">The field that we need to check workflow for</param>
        protected PlanningData PopulateTaskItems(int projectId, int projectTemplateId, List<TaskView> tasks, bool includeDetails, string fieldName)
        {
            const string METHOD_NAME = "PopulateTaskItems";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the new data object
                PlanningData planningData = new PlanningData();
				UserManager userManager = new UserManager();

				bool canChangeStatus = true;
				if (fieldName == "TaskStatusId")
				{
					//See if we are allowed to bulk edit status (template setting)
					ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(projectTemplateId);
					canChangeStatus = templateSettings.Workflow_BulkEditCanChangeStatus;
				}

				//Populate the items
				List<PlanningDataItem> dataItems = planningData.Items;
                foreach (TaskView task in tasks)
                {
                    //Now populate with the data
                    PlanningDataItem dataItem = new PlanningDataItem();
                    dataItem.PrimaryKey = task.TaskId;
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, task.ProjectId, task.TaskId));

                    //ArtifactTypeId
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "ArtifactTypeId";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
                    dataItemField.IntValue = (int)task.ArtifactType;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Token (prefix + id)
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Token";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = task.ArtifactToken;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Name
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                    dataItemField.TextValue = task.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Image (specified per-item)
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Image";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
					dataItemField.TextValue = (task.IsPullRequest) ? "artifact-PullRequest.svg" : "artifact-Task.svg";
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Priority
                    if (task.TaskPriorityId.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "ImportanceId";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                        dataItemField.IntValue = task.TaskPriorityId;
                        dataItemField.TextValue = task.TaskPriorityName;
                        dataItemField.CssClass = "#" + task.TaskPriorityColor;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Estimate (Task = ProjectedEffort)
                    if (task.ProjectedEffort.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "Estimate";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Decimal;
                        dataItemField.TextValue = GlobalFunctions.GetEffortInFractionalHours(task.ProjectedEffort.Value) + "h";
                        dataItemField.Tooltip = GetEstimateToolTip(task);
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }

                    //Owner
                    if (task.OwnerId.HasValue)
                    {
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "OwnerId";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                        dataItemField.IntValue = task.OwnerId.Value;
                        dataItemField.TextValue = task.OwnerName;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//Owner has icon
						User owner = userManager.GetUserById(task.OwnerId.Value);
						if (string.IsNullOrEmpty(owner.Profile.AvatarImage))
						{
							dataItem.OwnerIconInitials = owner.Profile.FirstName.Substring(0, 1).ToUpper() + owner.Profile.LastName.Substring(0, 1).ToUpper();
						}
					}

					//PlannableYn (whether the item can be moved by the user)
					bool isPlannable;
					if (fieldName == "TaskStatusId")
					{
						isPlannable = canChangeStatus;
					}
					else
					{
						isPlannable = IsTaskFieldEnabled(projectId, task.TaskTypeId, task.TaskStatusId, fieldName);
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
                        if (!String.IsNullOrEmpty(task.Description))
                        {
                            //We send the description as plain-text
                            dataItemField = new DataItemField();
                            dataItemField.FieldName = "Description";
                            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                            dataItemField.TextValue = task.Description.StripHTML(false, true);
                            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        }

                        //Task Progress
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "Progress";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
                        PopulateTaskEqualizer(dataItemField, task);
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
            dataItemField.EqualizerGreen = (release.TaskPercentOnTime < 0) ? 0 : release.TaskPercentOnTime;
            dataItemField.EqualizerRed = (release.TaskPercentLateFinish < 0) ? 0 : release.TaskPercentLateFinish;
            dataItemField.EqualizerYellow = (release.TaskPercentLateStart < 0) ? 0 : release.TaskPercentLateStart;
            dataItemField.EqualizerGray = (release.TaskPercentNotStart < 0) ? 0 : release.TaskPercentNotStart;

            //Populate Tooltip
            dataItemField.TextValue = "";
            dataItemField.Tooltip = ReleaseManager.GenerateTaskProgressTooltip(release);
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
        /// Is the task field enabled for the specified type, status and field
        /// </summary>
        /// <param name="projectId">The project</param>
        /// <param name="taskTypeId">The task type</param>
        /// <param name="taskStatusId">The task status</param>
        /// <param name="fieldName">The field to check</param>
        /// <returns>True if the field is enabled</returns>
        public bool IsTaskFieldEnabled(int projectId, int taskTypeId, int taskStatusId, string fieldName)
        {
            //If no field specific, return true
            if (String.IsNullOrEmpty(fieldName))
            {
                return true;
            }

            //See if we have it in our cache
            List<TaskWorkflowField> fields;
            if (taskWorkflowStates.ContainsKey(projectId + "_" + taskTypeId + "_" + taskStatusId))
            {
                fields = taskWorkflowStates[projectId + "_" + taskTypeId + "_" + taskStatusId];
            }
            else
            {
                TaskWorkflowManager taskWorkflowManager = new TaskWorkflowManager();
                int workflowId = taskWorkflowManager.Workflow_GetForTaskType(taskTypeId);
                fields = taskWorkflowManager.Workflow_RetrieveFieldStates(workflowId, taskStatusId);
                taskWorkflowStates.Add(projectId + "_" + taskTypeId + "_" + taskStatusId, fields);
            }
            return (!fields.Any(f => f.ArtifactField.Name == fieldName && (f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive || f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden)));
        }

        #endregion
    }
}
