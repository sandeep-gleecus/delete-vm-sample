using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the HierarchicalGrid AJAX component for non-summary requirements with tasks nested underneath
    /// </summary>
    /// <remarks>
    /// Unlike the RequirementsService and TasksService this one doesn't support user-configurable columns
    /// since we need to force-fit the requirements and task columns together
    /// </remarks>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class RequirementsTaskService : HierarchicalListServiceBase, IRequirementsTaskService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.RequirementsTaskService::";

        /// <summary>
        /// Constructor
        /// </summary>
        public RequirementsTaskService()
        {
        }

        #region IRequirementsTask Methods

        /// <summary>
        /// Counts the number of requirements/tasks associated with the release
        /// </summary>
        /// <param name="projectId">The project id</param>
        /// <param name="artifact">The art
        public int RequirementsTask_Count(int projectId, ArtifactReference artifact)
        {
            const string METHOD_NAME = "RequirementsTask_Count";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view requirements
            //Limited OK because we need to display the 'has data' in tabs
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Depending on the artifact that these incidents are for (e.g. release)
                //we need to set the grid properties accordingly and also indicate if we have any data
                int reqCount = 0;
                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Release)
                {
                    //Get the count of child requirements (filtered by release)
                    RequirementManager requirementManager = new RequirementManager();
                    Hashtable reqFilters2 = new Hashtable();
                    reqFilters2.Add("ReleaseId", artifact.ArtifactId);
                    reqCount = requirementManager.CountNonSummary(projectId, reqFilters2, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }

                return reqCount;
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
        /// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
        /// </summary>
        /// <param name="artifactId">The id of the requirement or task to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        /// <remarks>
        /// Since this service only returns non-summary requirements, we display the list of parent requirements
        /// in the tooltip
        /// </remarks>
        public string RetrieveNameDesc(int? projectId, int artifactId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Now retrieve the specific requirement or task - handle quietly if it doesn't exist
                try
                {
                    //See if we have a requirement or task
                    if (artifactId > 0)
                    {
                        int requirementId = artifactId;
                        //Instantiate the requirement business object
                        RequirementManager requirementManager = new RequirementManager();

                        //First we need to get the requirement itself
                        RequirementView requirement = requirementManager.RetrieveById2(null, requirementId);

                        //Next we need to get the list of successive parent folders
                        List<RequirementView> parentRequirements = requirementManager.RetrieveParents(Business.UserManager.UserInternal, requirement.ProjectId, requirement.IndentLevel);
                        string tooltip = "";
                        foreach (RequirementView parentRequirement in parentRequirements)
                        {
                            tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentRequirement.Name) + "</u> &gt; ";
                        }

                        //Now we need to get the requirement itself
                        tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(requirement.Name) + "</u>";
                        if (!String.IsNullOrEmpty(requirement.Description))
                        {
                            tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(requirement.Description);
                        }

                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return tooltip;
                    }
                    else
                    {
                        int taskId = -artifactId;
                        TaskManager taskManager = new TaskManager();
                        Task task = taskManager.RetrieveById(taskId);
                        string tooltip;
                        if (String.IsNullOrEmpty(task.Description))
                        {
                            tooltip = "<u>" + GlobalFunctions.HtmlRenderAsPlainText(task.Name) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(task.Description);
                        }
                        else
                        {
                            tooltip = GlobalFunctions.HtmlRenderAsPlainText(task.Name);
                        }

                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return tooltip;
                    }
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the requirement, but it has already been deleted on the server
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for requirement");
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return Resources.Messages.Global_UnableRetrieveTooltip;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Expands a requirements node
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="requirementId">The requirement we're expanding</param>
        public void Expand(int projectId, int requirementId)
        {
            const string METHOD_NAME = "Expand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Expand the requirement to display the tasks
                if (CurrentUserId.HasValue)
                {
                    ProjectSettingsCollection expandedRequirements = GetProjectSettings(CurrentUserId.Value, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_TASKS_EXPANDED);
                    if (expandedRequirements.ContainsKey(requirementId.ToString()))
                    {
                        expandedRequirements[requirementId.ToString()] = true;
                    }
                    else
                    {
                        expandedRequirements.Add(requirementId.ToString(), true);
                    }
                    expandedRequirements.Save();
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
        /// Collapses a requirements node
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="requirementId">The requirement we're collapsing</param>
        public void Collapse(int projectId, int requirementId)
        {
            const string METHOD_NAME = "Collapse";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Collapse the requirement to hide the tasks
                if (CurrentUserId.HasValue)
                {
                    ProjectSettingsCollection expandedRequirements = GetProjectSettings(CurrentUserId.Value, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_TASKS_EXPANDED);
                    if (expandedRequirements.ContainsKey(requirementId.ToString()))
                    {
                        expandedRequirements[requirementId.ToString()] = false;
                    }
                    else
                    {
                        expandedRequirements.Add(requirementId.ToString(), false);
                    }
                    expandedRequirements.Save();
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
        /// Inserts a new task under a requirement in the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="artifactId">The id of the existing artifact we're inserting in front of (-1 for none)</param>
        /// <param name="artifact">The type of artifact we're inserting ('Requirement')</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>The id of the new requirement</returns>
        public int HierarchicalList_Insert(int projectId, JsonDictionaryOfStrings standardFilters, int artifactId, string artifact)
        {
            const string METHOD_NAME = "Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Task);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                if (artifact != "Task")
                {
                    throw new NotImplementedException("This method only supports Task inserts");
                }

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Deserialize any standard filters
                int? filteredReleaseId = null;
                if (standardFilters != null && standardFilters.Count > 0)
                {
                    Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
                    filteredReleaseId = (int)deserializedFilters["ReleaseId"];
                }

                //See if we have a task or requirement id (task ids are negative)
                TaskManager taskManager = new TaskManager();
                int? requirementId = null;
                int? releaseId = null;
                if (artifactId < -1)
                {
                    //We have a selected task row
                    int existingTaskId = -artifactId;
                    //Retrieve the existing item
                    Task existingTask = taskManager.RetrieveById(existingTaskId);
                    if (!existingTask.RequirementId.HasValue)
                    {
                        //There is no requirement associated with the task, so just add to the release directly
                        releaseId = filteredReleaseId;
                    }
                    requirementId = existingTask.RequirementId;
                }
                else if (artifactId > 0)
                {
                    //We have a selected requirement row
                    requirementId = artifactId;
                }
                else
                {
                    //No rows are selected, so just add to release
                    releaseId = filteredReleaseId;
                }
                //Insert a new task, the release/iteration will be inherited from the requirement, so we can pass NULL
                //Note: The standard filters collection might have the parent release of an iteration, so safer to let
                //it be inherited from the requirement instead
                int taskId = taskManager.Insert(
                    projectId,
                    userId,
                    Task.TaskStatusEnum.NotStarted,
                    null,
                    null,
                    requirementId,
                    releaseId,
                    null,
                    null,
                    Resources.Dialogs.Global_NewTask,
                    "",
                    null,
                    null,
                    null,
                    null,
                    null,
                    userId
                    );

                //We now need to populate the appropriate default custom properties
                Task task = taskManager.RetrieveById(taskId);
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, true);
                if (task != null)
                {
                    //If the artifact custom property row is null, create a new one and populate the defaults
                    if (artifactCustomProperty == null)
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
                        artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, taskId, customProperties);
                        artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                    }
                    else
                    {
                        artifactCustomProperty.StartTracking();
                    }

                    //If we have filters currently applied to the view, then we need to set this new task to the same value
                    //(if possible) so that it will show up in the list
                    ProjectSettingsCollection filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENTS_TASKS_FILTERS_LIST);
                    if (filterList.Count > 0)
                    {
                        task.StartTracking();
                        //We need to tell it to ignore any filtering by the ID, creation date since we cannot set that on a new item
                        List<string> fieldsToIgnore = new List<string>() { "TaskId", "CreationDate" };
                        UpdateToMatchFilters(projectId, filterList, taskId, task, artifactCustomProperty, fieldsToIgnore);
                        taskManager.Update(task, userId);
                    }

                    //Save the custom properties
                    customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
                }

                //The grid expects the task ids to be negative (to avoid collisions with the requirement ids)
                return -taskId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Expands the list of requirements and tasks to a specific level
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="level">The number of levels to expand to (1 to expand all, 2 to collapse all)</param>
        /// <param name="standardFilters">Any standard filters</param>
        public void ExpandToLevel(int projectId, int level, JsonDictionaryOfStrings standardFilters)
        {
            const string METHOD_NAME = "ExpandToLevel";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //See whether we want to expand all or collapse all
                if (level == 1)
                {
                    //Add any standard filters to standard filters list
                    Hashtable filterList = new Hashtable();
                    if (standardFilters != null && standardFilters.Count > 0)
                    {
                        Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
                        foreach (KeyValuePair<string, object> filter in deserializedFilters)
                        {
                            filterList[filter.Key] = filter.Value;
                        }
                    }

                    //Expand all by setting expanded flag for all requirements
                    RequirementManager requirementManager = new RequirementManager();
                    List<RequirementView> requirements = requirementManager.RetrieveNonSummary(userId, projectId, 1, Int32.MaxValue, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                    if (CurrentUserId.HasValue)
                    {
                        ProjectSettingsCollection expandedRequirements = GetProjectSettings(CurrentUserId.Value, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_TASKS_EXPANDED);
                        foreach (RequirementView requirement in requirements)
                        {
                            if (expandedRequirements.ContainsKey(requirement.RequirementId.ToString()))
                            {
                                expandedRequirements[requirement.RequirementId.ToString()] = true;
                            }
                            else
                            {
                                expandedRequirements.Add(requirement.RequirementId.ToString(), true);
                            }
                        }
                        expandedRequirements.Save();
                    }
                }
                if (level == 2)
                {
                    //Collapse all
                    ProjectSettingsCollection expandedRequirements = GetProjectSettings(CurrentUserId.Value, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_TASKS_EXPANDED);
                    expandedRequirements.Clear();
                    expandedRequirements.Save();
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Moves a task between two requirements in the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sourceItems">The items to copy</param>
        /// <param name="destId">The destination item's id</param>
        /// <remarks>You cannot move requirements with this service</remarks>
        public void Move(int projectId, List<string> sourceItems, int? destId)
        {
            const string METHOD_NAME = "Move";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Task);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Only tasks are supported as destinations (have negative ids)
                if (!destId.HasValue || destId.Value > -1)
                {
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to use the task insert command with a requirement item as destination");
                    return;
                }
                int destTaskId = -destId.Value;
                //Retrieve the destination
                TaskManager taskManager = new TaskManager();
                Task destTask = taskManager.RetrieveById(destTaskId);
                if (!destTask.RequirementId.HasValue)
                {
                    //Need to have a destination requirement
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The destination task is no longer associated with a requirement");
                    return;
                }

                //Iterate through all the items to be moved and perform the operation
                foreach (string itemValue in sourceItems)
                {
                    //Get the source ID
                    int sourceId = Int32.Parse(itemValue);
                    //Only tasks can be moved (they have negative artifact ids)
                    if (sourceId < 0)
                    {
                        int sourceTaskId = -sourceId;
                        Task sourceTask = taskManager.RetrieveById(sourceTaskId);
                        if (sourceTask.RequirementId.HasValue && sourceTask.RequirementId.Value != destTask.RequirementId.Value)
                        {
                            sourceTask.RequirementId = sourceTask.RequirementId.Value;
                            taskManager.Update(sourceTask, userId);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
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
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENTS_TASKS_PAGINATION);
                paginationSettings.Restore();
                if (pageSize != -1)
                {
                    paginationSettings["PaginationOption"] = pageSize;
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
        /// Updates records of data in the system
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="dataItems">The updated data records</param>
        /// <returns>Validation messages</returns>
        public List<ValidationMessage> HierarchicalList_Update(int projectId, List<HierarchicalDataItem> dataItems)
        {
            const string METHOD_NAME = "HierarchicalList_Update";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Used to store any validation messages
            List<ValidationMessage> validationMessages = new List<ValidationMessage>();

            try
            {
                //Iterate through each data item and make the updates
                RequirementManager requirementManager = new RequirementManager();
                TaskManager taskManager = new TaskManager();
                foreach (HierarchicalDataItem dataItem in dataItems)
                {
                    //Get the requirement id or task id
                    if (dataItem.PrimaryKey > 0)
                    {
                        //Make sure we're authorized
                        Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.Requirement);
                        if (authorizationState == Project.AuthorizationState.Authorized)
                        {
                            int requirementId = dataItem.PrimaryKey;

                            //Retrieve the existing record - and make sure it still exists.
                            Requirement requirement = requirementManager.RetrieveById3(projectId, requirementId);
                            if (requirement != null)
                            {
                                //Need to set the original date of this record to match the concurrency date
                                if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                                {
                                    DateTime concurrencyDateTimeValue;
                                    if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                                    {
                                        requirement.ConcurrencyDate = concurrencyDateTimeValue;
                                        requirement.AcceptChanges();
                                    }
                                }

                                //Now we can start tracking any changes
                                requirement.StartTracking();

                                //Update the field values
                                List<string> fieldsToIgnore = new List<string>();
                                fieldsToIgnore.Add("CreationDate");
                                UpdateFields(validationMessages, dataItem, requirement, null, null, projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, fieldsToIgnore);

                                //Make sure we have no validation messages before updating
                                if (validationMessages.Count == 0)
                                {
                                    //Persist to database
                                    try
                                    {
                                        requirementManager.Update(userId, projectId, new List<Requirement>() { requirement });
                                    }
                                    catch (OptimisticConcurrencyException)
                                    {
                                        return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Make sure we're authorized
                        Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.Task);
                        if (authorizationState == Project.AuthorizationState.Authorized)
                        {
                            int taskId = -dataItem.PrimaryKey;

                            //Retrieve the existing record - and make sure it still exists
                            Task task = taskManager.RetrieveById(taskId);
                            if (task != null)
                            {
                                //Need to set the original date of this record to match the concurrency date
                                if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                                {
                                    DateTime concurrencyDateTimeValue;
                                    if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                                    {
                                        task.ConcurrencyDate = concurrencyDateTimeValue;
                                        task.AcceptChanges();
                                    }
                                }

                                //Update the field values - converting the requirement column names into task column names first
                                foreach (KeyValuePair<string, DataItemField> kvp in dataItem.Fields)
                                {
                                    ConvertReqFieldsToTask(kvp.Value);
                                }
                                List<string> fieldsToIgnore = new List<string>();
                                fieldsToIgnore.Add("CreationDate");
                                UpdateFields(validationMessages, dataItem, task, null, null, projectId, taskId, DataModel.Artifact.ArtifactTypeEnum.Task);

                                //Perform any business level validations on the datarow
                                Dictionary<string, string> businessMessages = taskManager.Validate(task);
                                foreach (KeyValuePair<string, string> businessMessage in businessMessages)
                                {
                                    ValidationMessage newMsg = new ValidationMessage();
                                    newMsg.FieldName = businessMessage.Key;
                                    newMsg.Message = businessMessage.Value;
                                    AddUniqueMessage(validationMessages, newMsg);
                                }

                                //Make sure we have no validation messages before updating
                                if (validationMessages.Count == 0)
                                {
                                    //Persist to database, catching any business exceptions and displaying them
                                    try
                                    {
                                        taskManager.Update(task, userId);
                                    }
                                    catch (OptimisticConcurrencyException)
                                    {
                                        return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
                                    }
                                    catch (TaskDateOutOfBoundsException)
                                    {
                                        return CreateSimpleValidationMessage(Resources.Messages.RequirementsTaskService_TaskOutsideDateRange);
                                    }
                                }
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
        /// Retrieves a list of saved filters for the current user/project
        /// </summary>
        /// <param name="includeShared">Should we include shared ones</param>
        /// <param name="projectId">The current project</param>
        /// <returns>Dictionary of saved filters</returns>
        public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
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

            //Delegate to the generic implementation
            return base.RetrieveFilters(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, includeShared);
        }

        /// <summary>
        /// Saves the current filters with the specified name
        /// </summary>
        /// <param name="includeColumns">Should we include the column selection</param>
        /// <param name="existingSavedFilterId">Populated if we're updating an existing saved filter</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="name">The name of the filter</param>
        /// <param name="isShared">Is this a shared filter</param>
        /// <returns>Validation/error message (or empty string if none)</returns>
        public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
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

            return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.Requirement, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENTS_TASKS_FILTERS_LIST, isShared, existingSavedFilterId, includeColumns);
        }

        /// <summary>
        /// Updates the filters stored in the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="filters">The array of filters (name,value)</param>
        /// <returns>Validation/error message (or empty string if none)</returns>
        public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
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

            //We need to change TaskCount to ProgressId if that's one of the filters
            if (filters.ContainsKey("TaskCount"))
            {
                string filterValue = filters["TaskCount"];
                filters.Remove("TaskCount");
                filters.Add("ProgressId", filterValue);
            }
            bool expandAll = false;
            string result = base.UpdateFilters(userId, projectId, filters, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENTS_TASKS_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.Requirement, out expandAll);
            return result;
        }

        /// <summary>
        /// Returns the latest information on a single requirement in the system as a lookup
        /// </summary>
        /// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <returns>A single dataitem object</returns>
        /// <remarks>This interface is used by the AjaxLookupManager control</remarks>
        public DataItem RetrieveLookup(int projectId, int artifactId)
        {
            const string METHOD_NAME = "RetrieveLookup";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //See if we have a requirement or task
                if (artifactId > 0)
                {
                    //Instantiate the requirement business object
                    RequirementManager requirementManager = new RequirementManager();

                    //See which requirements are expanded
                    ProjectSettingsCollection expandedRequirements = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_TASKS_EXPANDED);

                    //Create the data item record (no filter items)
                    //We use the Internal user ID so that we always get all columns
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();
                    PopulateShape(projectId, projectTemplateId, UserManager.UserInternal, dataItem, null);

                    //Get the requirement dataset record for the specific requirement id
                    RequirementView requirement = requirementManager.RetrieveById2(projectId, artifactId);

                    //Finally populate the dataitem from the dataset
                    if (requirement != null)
                    {
                        PopulateRequirementRow(dataItem, requirement, null, true, null, expandedRequirements);
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();

                    return dataItem;
                }
                else
                {
                    int taskId = -artifactId;
                    //Instantiate the task business object
                    TaskManager taskManager = new TaskManager();

                    //Create the data item record (no filter items)
                    //We use the Internal user ID so that we always get all columns
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();
                    PopulateShape(projectId, projectTemplateId, UserManager.UserInternal, dataItem, null);

                    //Get the task entity for the specific task id
                    TaskView task = taskManager.TaskView_RetrieveById(taskId);

                    //Finally populate the dataitem from the dataset
                    if (task != null)
                    {
                        PopulateTaskRow(projectId, projectTemplateId, dataItem, task, true, "");
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();

                    return dataItem;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns the latest information on a single requirement in the system
        /// </summary>
        /// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
        /// <param name="userId">The user we're viewing the requirements as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <returns>A single dataitem object</returns>
        public HierarchicalDataItem Refresh(int projectId, int artifactId)
        {
            const string METHOD_NAME = "Refresh";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //See if we have a requirement or task
                if (artifactId > 0)
                {
                    //Make sure we're authorized
                    Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
                    if (authorizationState == Project.AuthorizationState.Prohibited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }

                    //Instantiate the requirement business object
                    RequirementManager requirementManager = new RequirementManager();

                    //See which requirements are expanded
                    ProjectSettingsCollection expandedRequirements = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_TASKS_EXPANDED);

                    //Create the data item record (no filter items)
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();
                    PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

                    //Get the requirement dataset record for the specific requirement id
                    RequirementView requirement = requirementManager.RetrieveById2(projectId, artifactId);

                    //Make sure the user is authorized for this item
                    int ownerId = -1;
                    if (requirement.OwnerId.HasValue)
                    {
                        ownerId = requirement.OwnerId.Value;
                    }
                    if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && requirement.AuthorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }

                    //Finally populate the dataitem from the dataset
                    if (requirement != null)
                    {
                        PopulateRequirementRow(dataItem, requirement, null, true, null, expandedRequirements);
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();

                    return dataItem;
                }
                else
                {
                    //Make sure we're authorized
                    Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Task);
                    if (authorizationState == Project.AuthorizationState.Prohibited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }

                    int taskId = -artifactId;
                    //Instantiate the task business object
                    TaskManager taskManager = new TaskManager();

                    //Create the data item record (no filter items)
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();
                    PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

                    //Get the task entity for the specific task id
                    TaskView task = taskManager.TaskView_RetrieveById(taskId);

                    //Make sure the user is authorized for this item
                    int ownerId = -1;
                    if (task.OwnerId.HasValue)
                    {
                        ownerId = task.OwnerId.Value;
                    }
                    if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && task.CreatorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }

                    //Finally populate the dataitem from the entity
                    if (task != null)
                    {
                        PopulateTaskRow(projectId, projectTemplateId, dataItem, task, true, "");
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();

                    return dataItem;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Populates a data item from a requirement entity
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="requirementView">The requirement entity containing the data</param>
        /// <param name="customProperties">The list of custom property definitions and lookup values</param>
        /// <param name="editable">Does the data need to be in editable form?</param>
        /// <param name="artifactCustomProperty">The record of custom property data (if not provided as part of dataitem) - pass null if not used</param>
        protected void PopulateRequirementRow(HierarchicalDataItem dataItem, RequirementView requirementView, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty, ProjectSettingsCollection expandedRequirements)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = requirementView.RequirementId;
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, requirementView.ConcurrencyDate);

            //All requirements are considered summary for this service
            //since tasks are nested underneath
            dataItem.Summary = true;
            dataItem.Expanded = (expandedRequirements.ContainsKey(requirementView.RequirementId.ToString()) && (bool)expandedRequirements[requirementView.RequirementId.ToString()]);

            //Specify if it has an attachment or not
            dataItem.Attachment = requirementView.IsAttachments;

            //Specify the indent level
            dataItem.Indent = requirementView.IndentLevel;

            //Specify the TASK url for the row
            dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, requirementView.ProjectId, requirementView.RequirementId, GlobalFunctions.PARAMETER_TAB_TASK));

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (requirementView.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, requirementView, customProperties, artifactCustomProperty, editable, PopulateRequirementEqualizer);

                    //Specify which fields are editable or not
                    //Unless specified, all fields are editable
                    dataItemField.Editable = true;

                    //The task estimated/actual effort fields are not editable for requirements
                    if (fieldName == "TaskEstimatedEffort" || fieldName == "TaskActualEffort" ||
                        fieldName == "TaskRemainingEffort" || fieldName == "TaskProjectedEffort")
                    {
                        dataItemField.Editable = false;
                    }
                }

                //Apply the conditional formatting to the importance column (if displayed)
                if (dataItemField.FieldName == "ImportanceId" && requirementView.ImportanceId.HasValue)
                {
                    dataItemField.CssClass = "#" + requirementView.ImportanceColor;
                }

                //If we have no tasks, display the requirement's own estimated effort instead
                if (requirementView.TaskCount < 1 && dataItemField.FieldName == "TaskEstimatedEffort" && requirementView.EstimatedEffort.HasValue)
                {
                    dataItemField.IntValue = requirementView.EstimatedEffort.Value;
                    //Display as fractional hours
                    decimal fractionalHours = ((decimal)dataItemField.IntValue) / (decimal)60;
                    if (editable)
                    {
                        dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_TIME_INTERVAL_HOURS_EDITABLE, fractionalHours);
                    }
                    else
                    {
                        dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_TIME_INTERVAL_HOURS, fractionalHours);
                    }

                    int hours = EffortUtils.GetEffortHoursComponent(dataItemField.IntValue.Value);
                    int mins = EffortUtils.GetEffortMinutesComponent(dataItemField.IntValue.Value);
                    dataItemField.Tooltip = hours + " " + Resources.Fields.Hours + " " + mins + " " + Resources.Fields.Minutes;
                }
            }
        }

        /// <summary>
        /// Converts a requirement field name into the equivalent task field name
        /// </summary>
        /// <param name="dataItemField">The requirement field to be converted</param>
        /// <returns>The name of the corresponding Task column</returns>
        protected static string ConvertReqFieldsToTask(DataItemField dataItemField)
        {
            //Unless it's a specific known field, just return the requirement field name
            if (dataItemField.FieldName == "ImportanceId")
            {
                dataItemField.FieldName = "TaskPriorityId";
                dataItemField.LookupName = "TaskPriorityName";
                return "TaskPriorityId";
            }
            if (dataItemField.FieldName == "RequirementStatusId")
            {
                dataItemField.FieldName = "TaskStatusId";
                dataItemField.LookupName = "TaskStatusName";
                return "TaskStatusId";
            }
            if (dataItemField.FieldName == "TaskEstimatedEffort")
            {
                dataItemField.FieldName = "EstimatedEffort";
                return "EstimatedEffort";
            }
            if (dataItemField.FieldName == "TaskActualEffort")
            {
                dataItemField.FieldName = "ActualEffort";
                return "ActualEffort";
            }
            if (dataItemField.FieldName == "TaskProjectedEffort")
            {
                dataItemField.FieldName = "ProjectedEffort";
                return "ProjectedEffort";
            }
            if (dataItemField.FieldName == "TaskRemainingEffort")
            {
                dataItemField.FieldName = "RemainingEffort";
                return "ProjectedEffort";
            }
            if (dataItemField.FieldName == "TaskCount")
            {
                dataItemField.FieldName = "ProgressId";
                return "ProgressId";
            }

            return dataItemField.FieldName;
        }

        /// <summary>
        /// Populates a data item from a task dataset datarow
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="projectTemplateId">the id of the template</param>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="task">The entity containing the data</param>
        /// <param name="editable">Does the data need to be in editable form?</param>
        /// <param name="indentLevel">The indent level of the task</param>
        protected void PopulateTaskRow(int projectId, int projectTemplateId, HierarchicalDataItem dataItem, TaskView task, bool editable, string indentLevel)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = -task.TaskId;  //Use a negative number to avoid colliding with requirement ids
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, task.ConcurrencyDate);

            //All tasks are considered non-summary for this service
            //since tasks are nested underneath requirements
            dataItem.Summary = false;
            dataItem.Expanded = false;

            //Specify if it has an attachment or not
            dataItem.Attachment = task.IsAttachments;

            //Specify the indentation
            dataItem.Indent = indentLevel;

            //Specify the TASK url for the row
            dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, task.ProjectId, task.TaskId));

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                //Need to convert requirement field names to the corressponding task field name
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                fieldName = ConvertReqFieldsToTask(dataItemField);
                if (task.ContainsProperty(dataItemField.FieldName))
                {
                    //Override the lookups for the task status and priority
                    if (fieldName == "TaskStatusId" || fieldName == "TaskPriorityId")
                    {
                        dataItemField.Lookups = GetLookupValues(fieldName, projectId, projectTemplateId);
                    }

                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, task, null, (ArtifactCustomProperty)null, editable, PopulateTaskEqualizer);

                    //Specify which fields are editable or not
                    //Unless specified, all fields are editable
                    dataItemField.Editable = true;

                    //The project effort field and estimate points (RQ only field) are not editable
                    if (fieldName == "ProjectedEffort" || fieldName == "EstimatePoints")
                    {
                        dataItemField.Editable = false;
                    }
                }

                //Apply the conditional formatting to the importance column (if displayed)
                if (dataItemField.FieldName == "TaskPriorityId" && task.TaskPriorityId.HasValue)
                {
                    dataItemField.CssClass = "#" + task.TaskPriorityColor;
                }
            }
        }

        /// <summary>
        /// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the requirements as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        protected void PopulateShape(int projectId, int projectTemplateId, int userId, HierarchicalDataItem dataItem, Hashtable filterList)
        {
            RequirementManager requirement = new RequirementManager();

            //The columns in this service are always fixed, since we need to sync up the requirement and task records
            //Requirement Name
            DataItemField dataItemField = new DataItemField();
            dataItemField.FieldName = "Name";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
            dataItemField.Caption = Resources.Fields.RequirementTaskName;
            dataItemField.Editable = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }

            //Status
            dataItemField = new DataItemField();
            dataItemField.FieldName = "RequirementStatusId";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
            dataItemField.LookupName = "RequirementStatusName";
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
            dataItemField.Caption = Resources.Fields.Status;
            dataItemField.Editable = true;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is MultiValueFilter)
            {
                MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[dataItemField.FieldName];
                dataItemField.TextValue = multiValueFilter.ToString();
            }

            //Importance
            dataItemField = new DataItemField();
            dataItemField.FieldName = "ImportanceId";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
            dataItemField.LookupName = "ImportanceName";
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
            dataItemField.Caption = Resources.Fields.Importance;
            dataItemField.Editable = true;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is MultiValueFilter)
            {
                MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[dataItemField.FieldName];
                dataItemField.TextValue = multiValueFilter.ToString();
            }

            //Task Progress
            dataItemField = new DataItemField();
            dataItemField.FieldName = "ProgressId";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
            PopulateEqualizerShape(dataItemField.FieldName, dataItemField, filterList, projectId, projectTemplateId);
            dataItemField.Caption = Resources.Fields.Progress;
            dataItemField.Editable = false;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //Owner
            dataItemField = new DataItemField();
            dataItemField.FieldName = "OwnerId";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
            dataItemField.LookupName = "OwnerName";
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
            dataItemField.Caption = Resources.Fields.Owner;
            dataItemField.Editable = true;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is MultiValueFilter)
            {
                MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[dataItemField.FieldName];
                dataItemField.TextValue = multiValueFilter.ToString();
            }

            //See if we're planning by points or hours
            ProjectSettings projectSettings = new ProjectSettings(projectId);
            bool useHours = projectSettings.DisplayHoursOnPlanningBoard;
            if (useHours)
            {
                //Estimated Effort
                dataItemField = new DataItemField();
                dataItemField.FieldName = "TaskEstimatedEffort";
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                dataItemField.Caption = Resources.Fields.EstimatedEffort;
                dataItemField.Editable = true;
                dataItemField.AllowDragAndDrop = true;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is EffortRange)
                {
                    //Need to convert into the displayable range form
                    EffortRange effortRange = (EffortRange)filterList[dataItemField.FieldName];
                    dataItemField.TextValue = effortRange.ToString();
                }

                //Actual Effort
                dataItemField = new DataItemField();
                dataItemField.FieldName = "TaskActualEffort";
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                dataItemField.Caption = Resources.Fields.ActualEffort;
                dataItemField.Editable = true;
                dataItemField.AllowDragAndDrop = true;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is EffortRange)
                {
                    //Need to convert into the displayable range form
                    EffortRange effortRange = (EffortRange)filterList[dataItemField.FieldName];
                    dataItemField.TextValue = effortRange.ToString();
                }

                //Projected Effort
                dataItemField = new DataItemField();
                dataItemField.FieldName = "TaskProjectedEffort";
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                dataItemField.Caption = Resources.Fields.ProjectedEffort;
                dataItemField.Editable = true;
                dataItemField.AllowDragAndDrop = true;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is EffortRange)
                {
                    //Need to convert into the displayable range form
                    EffortRange effortRange = (EffortRange)filterList[dataItemField.FieldName];
                    dataItemField.TextValue = effortRange.ToString();
                }
            }
            else
            {
                //Estimated Points - Requirements only
                dataItemField = new DataItemField();
                dataItemField.FieldName = "EstimatePoints";
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Decimal;
                dataItemField.Caption = Resources.Fields.EstimateWithPoints;
                dataItemField.Editable = true;
                dataItemField.AllowDragAndDrop = true;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is DecimalRange)
                {
                    //Need to convert into the displayable range form
                    DecimalRange decimalRange = (DecimalRange)filterList[dataItemField.FieldName];
                    dataItemField.TextValue = decimalRange.ToString();
                }
            }
        }

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
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENTS_TASKS_PAGINATION);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return paginationDictionary;
        }

        /// <summary>
        /// Returns the current hierarchy configuration for the current page
        /// </summary>
        /// <param name="userId">The user we're viewing the artifacts as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <returns>a dictionary where key=artifactid, value=indentlevel</returns>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        public JsonDictionaryOfStrings RetrieveHierarchy(int projectId, JsonDictionaryOfStrings standardFilters)
        {
            //Auth is done by the main Retrieve function

            //Get the full list of items for the current page
            List<HierarchicalDataItem> dataItems = this.HierarchicalList_Retrieve(projectId, standardFilters, false).Items;

            //Populate a dictionary with just the artifact ids and indent levels
            //as this will consume less bandwidth when retrieved by the client
            JsonDictionaryOfStrings hierarchyLevels = new JsonDictionaryOfStrings();
            for (int i = 1; i < dataItems.Count; i++)
            {
                hierarchyLevels.Add(dataItems[i].PrimaryKey.ToString(), dataItems[i].Indent);
            }
            return hierarchyLevels;
        }

        /// <summary>
        /// Returns a subset of the list of artifacts in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the artifacts as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="itemCount">The number of items to retrieve (-1 for all)</param>
        /// <param name="startIndex">The starting point relative to the current page</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>Collection of dataitems</returns>
        /// <remarks>Used when we only need to refresh part of the hierarchy</remarks>
        public HierarchicalData HierarchicalList_RetrieveSelection(int projectId, JsonDictionaryOfStrings standardFilters, int startIndex, int itemCount)
        {
            //Auth is done in the main Retrieve function

            //Get the full list of items for the current page
            HierarchicalData data = this.HierarchicalList_Retrieve(projectId, standardFilters, false);
            List<HierarchicalDataItem> dataItems = data.Items;

            //Return just the first row (header) and the subset requested
            HierarchicalData dataSubset = new HierarchicalData();
            List<HierarchicalDataItem> dataItemsSubset = dataSubset.Items;
            dataSubset.PageCount = data.PageCount;
            dataSubset.CurrPage = data.CurrPage;
            dataSubset.VisibleCount = data.VisibleCount;
            dataSubset.TotalCount = data.TotalCount;

            //First add the header row
            dataItemsSubset.Add(dataItems[0]);
            int endIndex = startIndex + itemCount;
            if (endIndex > dataItems.Count - 1 || itemCount == -1)
            {
                endIndex = dataItems.Count - 1;
            }
            //Now the data rows - add 1 to the start index since we don't need to return the header row
            for (int i = startIndex + 1; i <= endIndex; i++)
            {
                dataItemsSubset.Add(dataItems[i]);
            }
            return dataSubset;
        }

        /// <summary>
        /// Returns a list of requirements in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the requirements as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="updatedRecordsOnly"> Do we want to only return recently updates records</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>Collection of dataitems</returns>
        public HierarchicalData HierarchicalList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, bool updatedRecordsOnly)
        {
            const string METHOD_NAME = "Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the requirement and task business objects
                RequirementManager requirementManager = new RequirementManager();
                TaskManager taskManager = new TaskManager();

                //Create the array of data items (including the first filter item)
                HierarchicalData hierarchicalData = new HierarchicalData();
                List<HierarchicalDataItem> dataItems = hierarchicalData.Items;

                //Now get the list of populated filters
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENTS_TASKS_FILTERS_LIST);

                //Add any standard filters
                if (standardFilters != null && standardFilters.Count > 0)
                {
                    Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
                    foreach (KeyValuePair<string, object> filter in deserializedFilters)
                    {
                        filterList[filter.Key] = filter.Value;
                    }
                }

                //Create the filter item first - we can clone it later
                HierarchicalDataItem filterItem = new HierarchicalDataItem();
                PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
                dataItems.Add(filterItem);

                //Display the filter names
                hierarchicalData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Requirement);

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

                //Now get the pagination information and add to the filter item
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENTS_TASKS_PAGINATION);
                paginationSettings.Restore();
                //Default values
                int paginationSize = 15;
                int currentPage = 1;
                if (paginationSettings["PaginationOption"] != null)
                {
                    paginationSize = (int)paginationSettings["PaginationOption"];
                }
                if (paginationSettings["CurrentPage"] != null)
                {
                    currentPage = (int)paginationSettings["CurrentPage"];
                }
                //Get the number of requirements in the project
                int requirementCount = requirementManager.CountNonSummary(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //Also count the number of tasks not linked to requirements in the project
                //The filters for tasks are a bit different, so we need to convert the requirement filters over
                Hashtable taskFilterList = new Hashtable();
                MultiValueFilter taskReqFilter = new MultiValueFilter();
                taskReqFilter.IsNone = true;
                taskFilterList.Add("RequirementId", taskReqFilter);
                if (filterList.ContainsKey("ReleaseId"))
                {
                    taskFilterList.Add("ReleaseId", filterList["ReleaseId"]);
                }
                if (filterList.ContainsKey("OwnerId"))
                {
                    taskFilterList.Add("OwnerId", filterList["OwnerId"]);
                }
                if (filterList.ContainsKey("ImportanceId"))
                {
                    taskFilterList.Add("TaskPriorityId", filterList["ImportanceId"]);
                }
                if (filterList.ContainsKey("RequirementStatusId") && filterList["RequirementStatusId"] is Int32)
                {
                    taskFilterList.Add("TaskStatusId", ConvertStatusFilter((int)filterList["RequirementStatusId"]));
                }
                if (filterList.ContainsKey("ProgressId"))
                {
                    taskFilterList.Add("ProgressId", filterList["ProgressId"]);
                }
                if (filterList.ContainsKey("Name"))
                {
                    taskFilterList.Add("Name", filterList["Name"]);
                }
                if (filterList.ContainsKey("TaskEstimatedEffort"))
                {
                    taskFilterList.Add("EstimatedEffort", filterList["TaskEstimatedEffort"]);
                }
                if (filterList.ContainsKey("TaskActualEffort"))
                {
                    taskFilterList.Add("ActualEffort", filterList["TaskActualEffort"]);
                }
                if (filterList.ContainsKey("TaskProjectedEffort"))
                {
                    taskFilterList.Add("ProjectedEffort", filterList["TaskProjectedEffort"]);
                }
                // there's no equivalent of rq points on a task so set its effort as -1 - ie always hide all tasks if filtering by this rq field
                if (filterList.ContainsKey("EstimatePoints"))
                {
                    if (taskFilterList.ContainsKey("EstimatedEffort"))
                    {
                        taskFilterList["EstimatedEffort"] = -1;
                    }
                    else
                    {
                        taskFilterList.Add("EstimatedEffort", -1);
                    }
                }
                int taskCount = taskManager.Count(projectId, taskFilterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                int artifactCount = requirementCount + taskCount;

                //**** Now we need to actually populate the rows of data to be returned ****

                //Get the requirements list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }
                List<RequirementView> requirements = requirementManager.RetrieveNonSummary(userId, projectId, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                int pageCount = (int)Decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);
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
                hierarchicalData.CurrPage = currentPage;
                hierarchicalData.PageCount = pageCount;

                //See which requirements are expanded
                ProjectSettingsCollection expandedRequirements = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_TASKS_EXPANDED);

                //Iterate through all the requirements and populate the dataitem
                List<TaskView> taskList;
                HierarchicalDataItem dataItem;
                int index;
                int visibleCount = 0;
                foreach (RequirementView requirement in requirements)
                {
                    //See if we're only asked to get updated items (we have a 5-min buffer
                    if (!updatedRecordsOnly || requirement.LastUpdateDate > DateTime.UtcNow.AddMinutes(UPDATE_TIME_BUFFER_MINUTES))
                    {
                        //We clone the template item as the basis of all the new items
                        dataItem = filterItem.Clone();

                        //Now populate with the data
                        PopulateRequirementRow(dataItem, requirement, null, false, null, expandedRequirements);
                        dataItems.Add(dataItem);
                        visibleCount++;

                        //See if we need to get any of the child tasks for this requirement
                        if (expandedRequirements.ContainsKey(requirement.RequirementId.ToString()) && (bool)expandedRequirements[requirement.RequirementId.ToString()])
                        {
                            taskReqFilter.IsNone = false;
                            taskReqFilter.Values.Clear();
                            taskReqFilter.Values.Add(requirement.RequirementId);
                            taskList = taskManager.Retrieve(projectId, "StartDate", true, 1, Int32.MaxValue, taskFilterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                            string indentLevel = requirement.IndentLevel;
                            index = 0;
                            foreach (TaskView task in taskList)
                            {
                                //We clone the template item as the basis of all the new items
                                dataItem = filterItem.Clone();

                                //Now populate with the data
                                PopulateTaskRow(projectId, projectTemplateId, dataItem, task, false, indentLevel + index.ToString("0000"));
                                dataItems.Add(dataItem);
                                index++;
                            }
                        }
                    }
                }

                //Finally get any tasks that are linked to the user or release specified in the standard filter
                //but are not included as children of any requirements
                //See if there is any space on the current page for tasks
                if (requirementCount - startRow <= paginationSize)
                {
                    int taskStartRow = startRow - requirementCount;
                    if (taskStartRow < 1)
                    {
                        taskStartRow = 1;
                    }
                    //Get all tasks regardless of requirement
                    taskReqFilter.IsNone = false;
                    taskReqFilter.Clear();
                    taskList = taskManager.Retrieve(projectId, "StartDate", true, taskStartRow, paginationSize, taskFilterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                    index = 0;
                    foreach (TaskView task in taskList)
                    {
                        //Make sure the task is not already under a requirement
                        if (!task.RequirementId.HasValue || !requirements.Any(r => r.RequirementId == task.RequirementId.Value))
                        {
                            //We clone the template item as the basis of all the new items
                            dataItem = filterItem.Clone();

                            //Now populate with the data
                            PopulateTaskRow(projectId, projectTemplateId, dataItem, task, false, index.ToString("000"));
                            dataItems.Add(dataItem);
                            index++;
                            visibleCount++;
                        }
                    }
                }

                //Display the visible and total count of requirements only since we cannot include tasks in the total
                //counts since we only know the count for those expanded
                hierarchicalData.VisibleCount = visibleCount;
                hierarchicalData.TotalCount = requirementCount;

                //If we're getting all items, also include the pagination info
                if (!updatedRecordsOnly)
                {
                    hierarchicalData.PaginationOptions = this.RetrievePaginationOptions(projectId);
                }

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created data items with " + dataItems.Count.ToString() + " rows");

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return hierarchicalData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Takes a requirement status and returns a matching task status
        /// </summary>
        /// <param name="requirementStatusId">The id of the requirement status</param>
        /// <returns>The matching task status or -1 if no match</returns>
        protected int ConvertStatusFilter(int requirementStatusId)
        {
            int taskStatusId = -1;
            switch ((Requirement.RequirementStatusEnum)requirementStatusId)
            {
                case Requirement.RequirementStatusEnum.Completed:
                case Requirement.RequirementStatusEnum.Developed:
                case Requirement.RequirementStatusEnum.Tested:
                case Requirement.RequirementStatusEnum.Released:
                case Requirement.RequirementStatusEnum.Documented:
                    {
                        taskStatusId = (int)Task.TaskStatusEnum.Completed;
                    }
                    break;

                case Requirement.RequirementStatusEnum.DesignInProcess:
                case Requirement.RequirementStatusEnum.InProgress:
                    {
                        taskStatusId = (int)Task.TaskStatusEnum.InProgress;
                    }
                    break;

                case Requirement.RequirementStatusEnum.Requested:
                case Requirement.RequirementStatusEnum.ReadyForReview:
                case Requirement.RequirementStatusEnum.Accepted:
                case Requirement.RequirementStatusEnum.DesignApproval:
                case Requirement.RequirementStatusEnum.Planned:
                    {
                        taskStatusId = (int)Task.TaskStatusEnum.NotStarted;
                    }
                    break;

                case Requirement.RequirementStatusEnum.Obsolete:
                    {
                        taskStatusId = (int)Task.TaskStatusEnum.Obsolete;
                    }
                    break;

                case Requirement.RequirementStatusEnum.Rejected:
                    {
                        taskStatusId = (int)Task.TaskStatusEnum.Rejected;
                    }
                    break;

                case Requirement.RequirementStatusEnum.UnderReview:
                    {
                        taskStatusId = (int)Task.TaskStatusEnum.UnderReview;
                    }
                    break;
            }

            return taskStatusId;
        }

        /// <summary>
        /// Used to populate the shape of the special compound fields used to display the information
        /// in the color-coded bar-chart 'equalizer' fields where different colors represent different values
        /// </summary>
        /// <param name="projectTemplateId">the id of the project template</param>
        /// <param name="dataItemField">The field whose shape we're populating</param>
        /// <param name="fieldName">The field name we're handling</param>
        /// <param name="filterList">The list of filters</param>
        /// <param name="projectId">The project we're interested in</param>
        protected void PopulateEqualizerShape(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectId, int projectTemplateId)
        {
            if (fieldName == "ProgressId")
            {
                dataItemField.FieldName = "TaskCount";
                string filterLookupName = fieldName;
                dataItemField.Lookups = GetLookupValues(filterLookupName, projectId, projectTemplateId);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(filterLookupName))
                {
                    dataItemField.IntValue = (int)filterList[filterLookupName];
                }
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
                RequirementManager requirementManager = new RequirementManager();
                TaskManager taskManager = new TaskManager();
                ReleaseManager releaseManager = new ReleaseManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                if (lookupName == "RequirementStatusId")
                {
                    List<RequirementStatus> statuses = requirementManager.RetrieveStatuses();
                    lookupValues = ConvertLookupValues(statuses.OfType<DataModel.Entity>().ToList(), "RequirementStatusId", "Name");
                }
                if (lookupName == "TaskStatusId")
                {
                    List<TaskStatus> statuses = taskManager.RetrieveStatuses();
                    lookupValues = ConvertLookupValues(statuses.OfType<DataModel.Entity>().ToList(), "TaskStatusId", "Name");
                }
                if (lookupName == "RequirementTypeId")
                {
                    List<RequirementType> types = requirementManager.RequirementType_Retrieve(projectTemplateId, false);
                    lookupValues = ConvertLookupValues(types.OfType<DataModel.Entity>().ToList(), "RequirementTypeId", "Name");
                }
                if (lookupName == "AuthorId" || lookupName == "OwnerId")
                {
                    List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
                    lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
                }
                if (lookupName == "ImportanceId")
                {
                    List<Importance> importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
                    lookupValues = ConvertLookupValues(importances.OfType<DataModel.Entity>().ToList(), "ImportanceId", "Name");
                }
                if (lookupName == "TaskPriorityId")
                {
                    List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
                    lookupValues = ConvertLookupValues(priorities.OfType<DataModel.Entity>().ToList(), "TaskPriorityId", "Name");
                }
                if (lookupName == "ReleaseId")
                {
                    List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false, true);
                    lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
                }
                if (lookupName == "CoverageId")
                {
                    lookupValues = new JsonDictionaryOfStrings(requirementManager.RetrieveCoverageFiltersLookup());
                }
                if (lookupName == "ProgressId")
                {
                    lookupValues = new JsonDictionaryOfStrings(taskManager.RetrieveProgressFiltersLookup());
                }

                //The custom property lookups
                int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
                if (customPropertyNumber.HasValue)
                {
                    CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, customPropertyNumber.Value, true);
                    if (customProperty != null)
                    {
                        //Handle the case of normal lists
                        if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List || customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
                        {
                            if (customProperty.List != null && customProperty.List.Values.Count > 0)
                            {
                                lookupValues = ConvertLookupValues(CustomPropertyManager.SortCustomListValuesForLookups(customProperty.List), "CustomPropertyValueId", "Name");
                            }
                        }

                        //Handle the case of user lists
                        if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.User)
                        {
                            List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
                            lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
                        }

                        //Handle the case of flags
                        if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Boolean)
                        {
                            lookupValues = new JsonDictionaryOfStrings(GlobalFunctions.YesNoList());
                        }
                    }
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
        /// Populates the equalizer type graph for tasks
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="artifact">The task entity</param>
        /// <param name="dataItem">The data item being populated</param>
        protected void PopulateTaskEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
        {
            //Explicitly recast the artifact to our specific entity type
            TaskView taskView = (TaskView)artifact;

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
        /// Populates the equalizer type graph for requirements
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="artifact">The requirement</param>
        /// <param name="dataItem">The data item being populated</param>
        protected void PopulateRequirementEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
        {
            //Explicitly recast the artifact to our specific entity type
            RequirementView requirement = (RequirementView)artifact;

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
        /// Deletes a set of tasks (not requirements)
        /// </summary>
        /// <param name="items">The items to delete</param>
        /// <param name="projectId">The id of the project (not used)</param>
        /// <param name="userId">The user we're viewing as</param>
        public void Delete(int projectId, List<string> items)
        {
            const string METHOD_NAME = "Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Task);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Iterate through all the items to be deleted
                //Only items that have negative IDs are tasks
                //and any requirements should not be deleted
                TaskManager taskManager = new TaskManager();
                foreach (string itemValue in items)
                {
                    //Get the task ID
                    int artifactId = Int32.Parse(itemValue);
                    if (artifactId < 0)
                    {
                        int taskId = -artifactId;
                        taskManager.MarkAsDeleted(projectId, taskId, userId);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #region Not Implemented Methods

        /// <summary>
        /// Adds/removes a column from the list of fields displayed in the current view
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="fieldName">The name of the column we displaying/hiding</param>
        /// <remarks>
        /// Since we have to sync up task and requirement columns, this option
        /// is not supported
        /// </remarks>
        public void ToggleColumnVisibility(int projectId, string fieldName)
        {
            throw new NotImplementedException();
        }

        public void Copy(int projectId, List<string> sourceItems, int destId)
        {
            throw new NotImplementedException();
        }

        public string Indent(int projectId, List<string> items)
        {
            throw new NotImplementedException();
        }

        public string Outdent(int projectId, List<string> items)
        {
            throw new NotImplementedException();
        }

        public void Export(int sourceProjectId, int destProjectId, List<string> items)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
