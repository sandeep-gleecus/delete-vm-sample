using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Linq;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to interacting with the various client-side user management AJAX components
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class UserService : SortedListServiceBase, IUserService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.UserService::";

        protected const string EXPANDED_KEY_UNASSIGNED = "Unassigned";

        #region ISortedListService methods

        public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
        {
            //Not used since editing is not allowed
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a list of resources in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the user resource list as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>Collection of dataitems</returns>
        public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            const string METHOD_NAME = "Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this project
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the project business object
                ProjectManager projectManager = new ProjectManager();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Create the array of data items (including the first filter item)
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;

                //Now get the list of populated filters and the current sort
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_FILTERS_LIST);
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "FullName ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");
                sortedData.FilterNames = GetFilterNames(filterList);

                //Create the filter item first - we can clone it later
                SortedDataItem filterItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
                dataItems.Add(filterItem);

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS);
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

                //See if we have a release selection to apply (or not)
                int releaseId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

                //Actually retrieve the resource data

                //If we have a value of -2, it actually means retrieve all the resource information for the current project group
                List<ProjectResourceView> projectResources;
                if (releaseId == -2)
                {
                    Project project = projectManager.RetrieveById(projectId);
                    int projectGroupId = project.ProjectGroupId;
                    projectResources = new ProjectGroupManager().RetrieveResourcesForGroup(projectGroupId, sortProperty, sortAscending, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }
                else
                {
                    projectResources = projectManager.RetrieveResources(projectId, (releaseId < 1) ? null : (int?)releaseId, sortProperty, sortAscending, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }

                //Get the number of user resources in the project
                int artifactCount = projectResources.Count;
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

                //**** Now we need to actually populate the rows of data to be returned ****
                int startRow = ((currentPage - 1) * paginationSize) + 1;

				//Display the pagination information
                sortedData.CurrPage = currentPage;
                sortedData.PageCount = pageCount;
                sortedData.StartRow = startRow;

				//Display the visible and total count of artifacts
                sortedData.VisibleCount = projectResources.Count;
                sortedData.TotalCount = artifactCount;

				//Display the sort information
                sortedData.SortProperty = sortProperty;
                sortedData.SortAscending = sortAscending;


                //Iterate through all the user resource records and populate if within the pagination range
                int startIndex = (currentPage - 1) * paginationSize;
                int endIndex = currentPage * paginationSize;
                if (endIndex > projectResources.Count)
                {
                    endIndex = projectResources.Count;
                }
                for (int i = startIndex; i < endIndex; i++)
                {
                    //We clone the template item as the basis of all the new items
                    SortedDataItem dataItem = filterItem.Clone();

                    //Now populate with the data
                    PopulateRow(dataItem, projectResources[i]);
                    dataItems.Add(dataItem);
                }

                //Also include the pagination info
                sortedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

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

            //Call the base method with the appropriate settings collection
            return base.UpdateSort(userId, projectId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS);
        }

        public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            throw new NotImplementedException();
        }
        public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            throw new NotImplementedException();
        }
        public void SortedList_Export(int destProjectId, List<string> items)
        {
            throw new NotImplementedException();
        }
        public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
        {
            //Not used since editing is not allowed
            throw new NotImplementedException();
        }
        public void SortedList_Copy(int projectId, List<string> items)
        {
            //Not used since copying is not allowed
            throw new NotImplementedException();
        }

        /* IListService methods */

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
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS);
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
                //Get the current filters from session
                ProjectSettingsCollection savedFilters = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_FILTERS_LIST);
                int oldFilterCount = savedFilters.Count;
                savedFilters.Clear(); //Clear the filters

                //Iterate through the filters, updating the project collection
                foreach (KeyValuePair<string, string> filter in filters)
                {
                    string filterName = filter.Key;
                    //Now get the type of field that we have. Since user resources are not a true artifact,
                    //these values have to be hardcoded, as they're not stored in the TST_ARTIFACT_FIELD table
                    DataModel.Artifact.ArtifactFieldTypeEnum artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    switch (filterName)
                    {
                        case "FullName":
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                            break;
                        case "ProjectRoleId":
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                            break;
                        case "AllocationIndicator":
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                            break;
                        case "ResourceEffort":
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            break;
                        case "ReqTaskEffort":
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            break;
                        case "IncidentEffort":
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            break;
                        case "TotalEffort":
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            break;
                        case "RemainingEffort":
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                            break;
                        case "UserId":
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
                            break;
                    }

                    if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Lookup)
                    {
                        //All list custom properties are numeric ids or multivalues
                        int filterValueInt = -1;
                        MultiValueFilter filterMultiValue;
                        if (Int32.TryParse(filter.Value, out filterValueInt))
                        {
                            savedFilters.Add(filterName, filterValueInt);
                        }
                        else if (MultiValueFilter.TryParse(filter.Value, out filterMultiValue))
                        {
                            savedFilters.Add(filterName, filterMultiValue);
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
                            return String.Format(Resources.Messages.ListServiceBase_EnterValidInteger, filterName);
                        }
                    }
                    if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval)
                    {
                        //If we have time interval values, need to make sure that they are indeed effort ranges
                        EffortRange effortRange;
                        if (!EffortRange.TryParse(filter.Value, out effortRange))
                        {
                            return String.Format(Resources.Messages.ListServiceBase_EnterValidEffortRange, filterName);
                        }
                        savedFilters.Add(filterName, effortRange);
                    }
                    if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Text || artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription)
                    {
                        //For text, just save the value
                        savedFilters.Add(filterName, filter.Value);
                    }
                }
                savedFilters.Save();

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
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return paginationDictionary;
        }

        /// <summary>
        /// Handles custom operations that are artifact/page-specific (buttons, drop-downs, etc.)
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="operation">The name of the operation</param>
        /// <param name="value">The parameter value being passed to the operation</param>
        /// <returns>Any error messages</returns>
        public override string CustomOperation(int projectId, string operation, string value)
        {
            const string METHOD_NAME = "CustomOperation";

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
                //See which operation we have and handle accordingly
                if (operation == "SelectRelease")
                {
                    //The value contains the id of the release we want to select
                    //We need to capture the release and put it in the project setting
                    if (value == "")
                    {
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                    }
                    else
                    {
                        int releaseId = Int32.Parse(value);
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);
                    }
                }
                //See which operation we have and handle accordingly
                if (operation == "SelectPlanningRelease")
                {
                    //The value contains the id of the release we want to select
                    //We need to capture the release and put it in the project setting
                    if (value == "")
                    {
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                    }
                    else
                    {
                        int releaseId = Int32.Parse(value);
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);
                    }
                    //We also need to reset the iteration index back to zero
                    SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_STARTING_INDEX, 0);
                }
                if (operation == "IncludeTasks")
                {
                    //Do we want to include tasks
                    bool includeTasks;
                    if (Boolean.TryParse(value, out includeTasks))
                    {
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_TASKS, includeTasks);
                    }
                }
                if (operation == "IncludeIncidents")
                {
                    //Do we want to include incidents
                    bool includeIncidents;
                    if (Boolean.TryParse(value, out includeIncidents))
                    {
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_INCIDENTS, includeIncidents);
                    }
                }
                return "";
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns the tooltip for the user
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>A popup tooltip</returns>
        public string RetrieveNameDesc(int? projectId, int userId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the user business object
                UserManager userManager = new UserManager();

                //Now retrieve the specific user - handle quietly if it doesn't exist
                try
                {
                    DataModel.User user = userManager.GetUserById(userId);
                    string department = "-";
                    if (!String.IsNullOrEmpty(user.Profile.Department))
                    {
                        department = Microsoft.Security.Application.Encoder.HtmlEncode(user.Profile.Department);
                    }
                    string tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(user.FullName) + "</u><br />\n" +
                        "Email: " + Microsoft.Security.Application.Encoder.HtmlEncode(user.EmailAddress) + "<br />\n" +
                        "Department: " + department;

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the task, but it has already been deleted on the server
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for task");
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

        #endregion

        public JsonDictionaryOfStrings RetrieveLookup(int projectId, string operation)
        {
            throw new NotImplementedException();
        }
        public string CustomListOperation(string operation, int userId, int projectId, int destId, List<string> items)
        {
            throw new NotImplementedException();
        }
        public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
        {
            throw new NotImplementedException();
        }
        public void ToggleColumnVisibility(int projectId, string fieldName)
        {
            throw new NotImplementedException();
        }
        public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
        {
            throw new NotImplementedException();
        }

        #region Internal Functions

        /// <summary>
        /// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the user resource list as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList)
        {
            //We need to add the various user resource fields to be displayed
            //User Name
            DataItemField dataItemField = new DataItemField();
            dataItemField.FieldName = "FullName";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
            dataItemField.Caption = Resources.Fields.ResourceName;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }

            //Project Role
            dataItemField = new DataItemField();
            dataItemField.FieldName = "ProjectRoleId";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
            dataItemField.Caption = Resources.Fields.Role;
            dataItemField.LookupName = "ProjectRoleName";
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the list of possible lookup values
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                if (filterList[dataItemField.FieldName] is Int32)
                {
                    dataItemField.IntValue = (int)filterList[dataItemField.FieldName];
                    dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
                }
                if (filterList[dataItemField.FieldName] is MultiValueFilter)
                {
                    dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
                }
            }

            //Allocation Bar - Need to be able to view releases
            Project.AuthorizationState releaseAuthorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Release);
            if (releaseAuthorizationState != Project.AuthorizationState.Prohibited)
            {
                dataItemField = new DataItemField();
                dataItemField.FieldName = "AllocationIndicator";
                dataItemField.Caption = Resources.Fields.Allocation;
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;

                //Set the list of possible lookup values
                dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName))
                {
                    if (filterList[dataItemField.FieldName] is Int32)
                    {
                        dataItemField.IntValue = (int)filterList[dataItemField.FieldName];
                        dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
                    }
                    if (filterList[dataItemField.FieldName] is MultiValueFilter)
                    {
                        dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
                    }
                }
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                //Resource (available) Effort
                dataItemField = new DataItemField();
                dataItemField.FieldName = "ResourceEffort";
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                dataItemField.Caption = Resources.Fields.AvailableEffort;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is EffortRange)
                {
                    //Need to convert into the displayable range form
                    EffortRange effortRange = (EffortRange)filterList[dataItemField.FieldName];
                    dataItemField.TextValue = effortRange.ToString();
                }
            }

            //Req/Task Effort, check permissions
            Project.AuthorizationState taskAuthorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
            if (taskAuthorizationState != Project.AuthorizationState.Prohibited)
            {
                dataItemField = new DataItemField();
                dataItemField.FieldName = "ReqTaskEffort";
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                dataItemField.Caption = Resources.Fields.ReqTaskEffort;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is EffortRange)
                {
                    //Need to convert into the displayable range form
                    EffortRange effortRange = (EffortRange)filterList[dataItemField.FieldName];
                    dataItemField.TextValue = effortRange.ToString();
                }
            }

            //Incident Effort, check permissions
            Project.AuthorizationState incidentAuthorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Incident);
            if (incidentAuthorizationState != Project.AuthorizationState.Prohibited)
            {
                dataItemField = new DataItemField();
                dataItemField.FieldName = "IncidentEffort";
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                dataItemField.Caption = Resources.Fields.IncidentEffort;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is EffortRange)
                {
                    //Need to convert into the displayable range form
                    EffortRange effortRange = (EffortRange)filterList[dataItemField.FieldName];
                    dataItemField.TextValue = effortRange.ToString();
                }
            }

            //Total Effort
            if (releaseAuthorizationState != Project.AuthorizationState.Prohibited)
            {
                dataItemField = new DataItemField();
                dataItemField.FieldName = "TotalEffort";
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                dataItemField.Caption = Resources.Fields.TotalEffort;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is EffortRange)
                {
                    //Need to convert into the displayable range form
                    EffortRange effortRange = (EffortRange)filterList[dataItemField.FieldName];
                    dataItemField.TextValue = effortRange.ToString();
                }

                //Remaining Effort
                dataItemField = new DataItemField();
                dataItemField.FieldName = "RemainingEffort";
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                dataItemField.Caption = Resources.Fields.RemainingEffort;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is EffortRange)
                {
                    //Need to convert into the displayable range form
                    EffortRange effortRange = (EffortRange)filterList[dataItemField.FieldName];
                    dataItemField.TextValue = effortRange.ToString();
                }
            }

            //User ID
            dataItemField = new DataItemField();
            dataItemField.FieldName = "UserId";
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
        /// Used to populate the shape of the special compound fields used to display the information
        /// in the color-coded bar-chart 'equalizer' fields where different colors represent different values
        /// </summary>
        /// <param name="dataItemField">The field whose shape we're populating</param>
        /// <param name="fieldName">The field name we're handling</param>
        /// <param name="filterList">The list of filters</param>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="projectId">The project we're interested in</param>
        protected void PopulateEqualizerShape(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectId, int projectTemplateId)
        {
            //Check to see if this is a field we can handle
            if (fieldName == "AllocationIndicator")
            {
                dataItemField.FieldName = fieldName;
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
                ProjectManager projectManager = new ProjectManager();

                if (lookupName == "ProjectRoleId")
                {
                    List<ProjectRole> projectRoles = projectManager.RetrieveProjectRoles(true);
                    lookupValues = ConvertLookupValues(projectRoles.OfType<DataModel.Entity>().ToList(), "ProjectRoleId", "Name");
                }
                if (lookupName == "AllocationIndicator")
                {
                    SortedList<int,string> filters = ProjectManager.RetrieveAllocationFilters();
                    lookupValues = ConvertLookupValues(filters);
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
        /// Populates the equalizer type graph for the resource progress
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="dataRow">The data row</param>
        protected void PopulateEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
        {
            //Explicitly recast the artifact to the type we're expecting
            ProjectResourceView projectResource = (ProjectResourceView)artifact;

            //Calculate the information to display
            int percentGreen;
            int percentRed;
            int percentYellow;
            int percentGray;
            string tooltipText = ProjectManager.CalculateResourceProgress(projectResource, out percentGreen, out percentRed, out percentYellow, out percentGray);

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
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="projectResource">The entity containing the data</param>
        protected void PopulateRow(DataItem dataItem, ProjectResourceView projectResource)
        {
            //Set the primary key
            dataItem.PrimaryKey = projectResource.UserId;

            //project resources don't have an attachment flag
            dataItem.Attachment = false;

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (projectResource.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, projectResource, null, null, false, PopulateEqualizer);

                    //All fields are not editable for user resources
                    dataItemField.Editable = false;
                    dataItemField.Required = false;
                }

                //Apply the conditional formatting to the remaining effort column
                //If the total effort exceeds the available effort in that release/iteration for one person
                //then we need to mark the row as such
                if (dataItemField.FieldName == "RemainingEffort" && projectResource.IsOverAllocated && projectResource.TotalOpenEffort > 0)
                {
                    dataItemField.CssClass = "PriorityHigh";
                }
            }
        }

        #endregion

        #region INavigationService Members

        /// <summary>
        /// Returns a list of pagination options that the user can choose from
        /// </summary>
        /// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
        public JsonDictionaryOfStrings NavigationBar_RetrievePaginationOptions(int projectId)
        {
            //Same implementation as the list service
            return RetrievePaginationOptions(projectId);
        }

        /// <summary>
        /// Updates the size of pages returned and the currently selected page
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
        /// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
        public void NavigationBar_UpdatePagination(int projectId, int pageSize, int currentPage)
        {
            //Same implementation as the list service
            this.UpdatePagination(projectId, pageSize, currentPage);
        }

        /// <summary>
        /// Returns a list of resources for display in the navigation bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="indentLevel">Not used for resources since not hierarchical</param>
        /// <returns>List of hosts</returns>
        /// <param name="displayMode">
        /// The display mode of the navigation list:
        /// 1 = Filtered List
        /// 2 = All Items (no filters)
        /// </param>
        /// <param name="selectedItemId">The id of the currently selected item</param>
        public List<HierarchicalDataItem> NavigationBar_RetrieveList(int projectId, string indentLevel, int displayMode, int? selectedItemId, int? containerId)
        {
            const string METHOD_NAME = "NavigationBar_RetrieveList";

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
                //Instantiate the business object
                ProjectManager projectManager = new ProjectManager();

                //Create the array of data items
                List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_FILTERS_LIST);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "UserId ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //See if we have a release selection to apply (or not)
                int releaseId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS);
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

                //**** Now we need to actually populate the rows of data to be returned ****

                //If we have a value of -2, it actually means retrieve all the resource information for the current project group
                List<ProjectResourceView> projectResources;
                if (releaseId == -2)
                {
                    Project project = projectManager.RetrieveById(projectId);
                    int projectGroupId = project.ProjectGroupId;
                    projectResources = new ProjectGroupManager().RetrieveResourcesForGroup(projectGroupId);
                }
                else
                {
                    projectResources = projectManager.RetrieveResources(projectId, (releaseId < 1) ? null : (int?)releaseId);
                }

                //Sort the list
                //dataSet.ProjectResources.DefaultView.Sort = sortProperty + " " + sortDirectionString;

                if (displayMode == 1)
                {
                    /*
                    //Need to get a filtered list
                    string filterClause = "";
                    if (filterList != null)
                    {
                        IDictionaryEnumerator enumerator = filterList.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            string filterProperty = enumerator.Key.ToString();
                            object filterValue = enumerator.Value;
                            string filterElement = "";
                            if (dataSet.ProjectResources.Columns[filterProperty].GetType() == typeof(string))
                            {
                                filterElement = filterProperty + " LIKE '*" + filterValue.ToString() + "*'";
                            }
                            else
                            {
                                filterElement = filterProperty + "='" + filterValue.ToString() + "'";
                            }
                            if (filterClause == "")
                            {
                                filterClause = filterElement;
                            }
                            else
                            {
                                filterClause += " AND " + filterElement;
                            }
                        }
                    }
                    dataSet.ProjectResources.DefaultView.RowFilter = filterClause;*/
                }

                //Get the number of user resources in the project
                int artifactCount = projectResources.Count;

                //Get the incidents list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }

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

                //Iterate through all the user resource records and populate if within the pagination range
                int startIndex = (currentPage - 1) * paginationSize;
                int endIndex = currentPage * paginationSize;
                if (endIndex > projectResources.Count)
                {
                    endIndex = projectResources.Count;
                }
                for (int i = startIndex; i < endIndex; i++)
                {
                    //Create the data-item
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();

                    //Populate the necessary fields
                    ProjectResourceView projectResource = projectResources[i];
                    dataItem.PrimaryKey = projectResource.UserId;
                    dataItem.Indent = "";
                    dataItem.Expanded = false;

                    //Name/Desc
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = projectResource.FullName;
                    dataItem.Summary = false;
                    dataItem.Alternate = false;
                    dataItem.Fields.Add("Name", dataItemField);

                    //Add to the items collection
                    dataItems.Add(dataItem);
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
        /// Updates the display settings used by the Navigation Bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="displayMode">The current display mode</param>
        /// <param name="displayWidth">The display width</param>
        /// <param name="minimized">Is the navigation bar minimized or visible</param>
        public void NavigationBar_UpdateSettings(int projectId, int? displayMode, int? displayWidth, bool? minimized)
        {
            const string METHOD_NAME = "NavigationBar_UpdateSettings";

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
                //Update the user's project settings
                bool changed = false;
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS);
                if (displayMode.HasValue)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = displayMode.Value;
                    changed = true;
                }
                if (minimized.HasValue)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED] = minimized.Value;
                    changed = true;
                }
                if (displayWidth.HasValue)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH] = displayWidth.Value;
                    changed = true;
                }
                if (changed)
                {
                    settings.Save();
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

        #region IUserService Members

        /// <summary>
        /// Attempts to link the specific user to an LDAP server
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="ldapDn">The LDAP DN to link them to</param>
        /// <returns>Either success or a message</returns>
        /// <remarks>
        /// You need to be a system administrator to perform this action
        /// </remarks>
        public string User_LinkUserToLdapDn(int userId, string ldapDn)
        {
            const string METHOD_NAME = "User_LinkUserToLdapDn";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int currentUserId = this.CurrentUserId.Value;

            //Make sure we're a system administrator
            if (!UserIsAdmin)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //First retrieve the user and make sure they are approved and not an OAUTH managed user
                SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;
                User user = provider.GetProviderUser(userId);
                if (user != null)
                {
                    //Make sure approved
                    if (!user.IsApproved)
                    {
                        return Resources.Messages.Admin_UserDetails_CannotLinkUnapprovedUserToLdap;
                    }

                    //Make sure not OAUTH Managed
                    if (user.OAuthProviderId.HasValue)
                    {
                        return Resources.Messages.Admin_UserDetails_CannotLinkOAuthUserToLdap;
                    }

                    //Make sure that we have an LDAP server configured
                    if (String.IsNullOrWhiteSpace(ConfigurationSettings.Default.Ldap_Host))
                    {
                        return Resources.Messages.UserLdapImport_UnableToAccessLdapServer;
                    }

                    //Actually make the change
                    user.StartTracking();
                    user.LdapDn = ldapDn.Trim();
                    provider.UpdateProviderUser(user);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return ""; //Success
            }
            catch (ArtifactNotExistsException exception)
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

        /// <summary>
        /// Retrieves a list of all the users in a project that are active (used in user dropdown lists)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The list of users as a simple dictionary (key=UserId, value=FullName)</returns>
        public JsonDictionaryOfStrings User_RetrieveActiveForProject(int projectId)
        {
            const string METHOD_NAME = "RetrievePaginationOptions";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (i.e. a member of the project, is sufficient for this function)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Get the list of assignable users
            List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);

            //Convert to API object
            JsonDictionaryOfStrings usersDictionary = new JsonDictionaryOfStrings();
            foreach (User user in users)
            {
                string key = user.UserId + "_" + user.IsActive.ToDatabaseSerialization();
                if (!usersDictionary.ContainsKey(key))
                {
                    usersDictionary.Add(key, user.FullName);
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return usersDictionary;
        }

        #endregion
    }
}
