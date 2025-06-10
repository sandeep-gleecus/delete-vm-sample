using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>Communicates with the SortableGrid AJAX component for displaying/updating releases data</summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class GroupReleasesService : HierarchicalListServiceBase, IGroupReleasesService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.GroupReleasesService::";

        #region Hierarchical List methods


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
            //Authentication/authorization is done in the main retrieve function

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
        /// Returns a list of release/iterations in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the release/iterations as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="updatedRecordsOnly"> Do we want to only return recently updates records</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>Collection of dataitems</returns>
        public HierarchicalData HierarchicalList_Retrieve(/*Project Group*/ int projectId, JsonDictionaryOfStrings standardFilters, bool updatedRecordsOnly)
        {
            const string METHOD_NAME = "HierarchicalList_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Store project group id in correctly named variable to make code easier to understand
            int projectGroupId = projectId;

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the release/iterations and custom property business objects
                ReleaseManager releaseManager = new ReleaseManager();

                //Get the template associated with the project
                int? projectTemplateId = new ProjectGroupManager().RetrieveById(projectGroupId).ProjectTemplateId;

                //Create the array of data items (including the first filter item)
                HierarchicalData hierarchicalData = new HierarchicalData();
                List<HierarchicalDataItem> dataItems = hierarchicalData.Items;

                //Now get the list of populated filters
                Hashtable filterList = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_FILTERS);
                hierarchicalData.FilterNames = GetFilterNames(filterList, projectGroupId, projectTemplateId, Artifact.ArtifactTypeEnum.Release);

                //Create the filter item first - we can clone it later
                HierarchicalDataItem filterItem = new HierarchicalDataItem();
                PopulateShape(projectGroupId, userId, filterItem, filterList);
                dataItems.Add(filterItem);

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

                //Now get the pagination information and add to the filter item
                UserSettingsCollection paginationSettings = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_GENERAL);
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

                //First get the list of projects in the group
                List<ProjectView> projects = new ProjectManager().Project_RetrieveByGroup(projectGroupId);

                //Get the number of releases in the project group
                int unfilteredCount = projects.Count;
                int artifactCount = projects.Count;
                int visibleCount = 0;
                foreach (ProjectView project in projects)
                {
                    unfilteredCount += releaseManager.Count(User.UserInternal, project.ProjectId, null, 0);
                    artifactCount += releaseManager.Count(userId, project.ProjectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }

                //**** Now we need to actually populate the rows of data to be returned ****

                //Get the release list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }

                //Get the list of expanded projects
                List<int> expandedProjectIds = GetUserSetting(userId, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS, "").FromDatabaseSerialization_List_Int32();

                //We need to keep track of how many rows to display
                int remainingRows = paginationSize;

                //Next get the releases in each project
                string runningIndent = "AAA";
                foreach (ProjectView project in projects)
                {
                    //See if this is expanded or collapsed
                    bool expanded = expandedProjectIds.Contains(project.ProjectId);

                    //Add a 'fake' release for the project
                    HierarchicalDataItem dataItem = filterItem.Clone();
                    PopulateRow(dataItem, project, runningIndent, expanded);
                    dataItems.Add(dataItem);

                    remainingRows -= 1;
                    if (remainingRows < 1)
                    {
                        remainingRows = 0;
                    }
                    visibleCount++;

                    //Also remove from the start row
                    if (startRow > 1)
                    {
                        startRow -= 1;
                        if (startRow < 1)
                        {
                            startRow = 1;
                        }
                    }

                    //Add the releases if expanded and rows remaining
                    if (expanded && remainingRows > 0)
                    {
                        List<ReleaseView> releases = releaseManager.RetrieveByProjectId(userId, project.ProjectId, startRow, remainingRows, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                        int count = releaseManager.Count(userId, project.ProjectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                        //Iterate through all the releases and populate the dataitem
                        foreach (ReleaseView release in releases)
                        {
                            //See if we're only asked to get updated items (we have a 5-min buffer
                            if (!updatedRecordsOnly || release.LastUpdateDate > DateTime.UtcNow.AddMinutes(UPDATE_TIME_BUFFER_MINUTES))
                            {
                                //We clone the template item as the basis of all the new items
                                dataItem = filterItem.Clone();

                                //Now populate with the data
                                PopulateRow(dataItem, release, false, runningIndent);
                                dataItems.Add(dataItem);
                            }
                        }

                        remainingRows -= releases.Count;
                        if (remainingRows < 1)
                        {
                            remainingRows = 0;
                        }
                        visibleCount += releases.Count;

                        //Also remove from the start row for the next project
                        if (startRow > 1)
                        {
                            startRow -= count;
                            if (startRow < 1)
                            {
                                startRow = 1;
                            }
                        }
                    }

                    //See if we have any more rows left
                    if (remainingRows <= 0)
                    {
                        break;
                    }

                    //Increment the running indent
                    runningIndent = HierarchicalList.IncrementIndentLevel(runningIndent);
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
                hierarchicalData.CurrPage = currentPage;
                hierarchicalData.PageCount = pageCount;

                //Display the visible and total count of artifacts
                hierarchicalData.VisibleCount = visibleCount;
                hierarchicalData.TotalCount = unfilteredCount;

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
        /// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
        /// </summary>
        /// <param name="releaseId">The id of the release to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        public string RetrieveNameDesc(int? projectId, int releaseId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            if (!projectId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId.Value;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //A negative release ID is used for the fake 'project' parent releases
                if (releaseId < 0)
                {
                    int parentProjectId = -releaseId;
                    string tooltip = "";

                    ProjectView project = new ProjectManager().RetrieveById2(parentProjectId);

                    //Make sure the project is in our group
                    if (project.ProjectGroupId != projectGroupId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }

                    if (String.IsNullOrEmpty(project.Description))
                    {
                        tooltip = Microsoft.Security.Application.Encoder.HtmlEncode(project.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_PROJECT, parentProjectId, true);
                    }
                    else
                    {
                        tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(project.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_PROJECT, parentProjectId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(project.Description);
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                else
                {
                    //Now retrieve the specific release - handle quietly if it doesn't exist
                    try
                    {
                        //Instantiate the release business object
                        ReleaseManager releaseManager = new ReleaseManager();

                        ReleaseView release = releaseManager.RetrieveById2(null, releaseId);
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
                        //This is the case where the client still displays the release, but it has already been deleted on the server
                        Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for release");
                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return Resources.Messages.Global_UnableRetrieveTooltip;
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
        /// Returns a list of pagination options that the user can choose from
        /// </summary>
        /// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
        public JsonDictionaryOfStrings RetrievePaginationOptions(int /*Project Group*/projectId)
        {
            const string METHOD_NAME = "RetrievePaginationOptions";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Delegate to the generic method in the base class - passing the correct collection name
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(-1, userId, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_GENERAL);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return paginationDictionary;
        }

        /// <summary>
        /// Adds/removes a column from the list of fields displayed in the current view
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project group we're interested in</param>
        /// <param name="fieldName">The name of the column we displaying/hiding</param>
        public void ToggleColumnVisibility(int /*ProjectGroupId*/projectId, string fieldName)
        {
            const string METHOD_NAME = "ToggleColumnVisibility";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Toggle the status of the appropriate field name
                ReleaseManager releaseManager = new ReleaseManager();
                releaseManager.ToggleProjectGroupColumnVisibility(userId, projectGroupId, fieldName, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_COLUMNS);
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
            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //We need to change CountPassed to CoverageId if that's one of the filters
            if (filters.ContainsKey("CountPassed"))
            {
                string filterValue = filters["CountPassed"];
                filters.Remove("CountPassed");
                filters.Add("CoverageId", filterValue);
            }
            //We need to change TaskCount to ProgressId if that's one of the filters
            if (filters.ContainsKey("TaskCount"))
            {
                string filterValue = filters["TaskCount"];
                filters.Remove("TaskCount");
                filters.Add("ProgressId", filterValue);
            }
            //We need to change PercentComplete to CompletionId if that's one of the filters
            if (filters.ContainsKey("PercentComplete"))
            {
                string filterValue = filters["PercentComplete"];
                filters.Remove("PercentComplete");
                filters.Add("CompletionId", filterValue);
            }

            //Call the base method with the appropriate settings collection (-1 = user settings instead of project settings)
            bool newFilter = false;
            return base.UpdateFilters(userId, -1, filters, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_FILTERS, DataModel.Artifact.ArtifactTypeEnum.Release, out newFilter);
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

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the pagination settings collection and update
                UserSettingsCollection paginationSettings = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_GENERAL);
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
        /// Collapses a release node
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project group we're interested in</param>
        /// <param name="artifactId">The release we're collapsing</param>
        public void Collapse(int projectId, int artifactId)
        {
            const string METHOD_NAME = "Collapse";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //If we have a negative release id, it is actually a parent project entry that needs to be expanded/collapsed
                if (artifactId < 0)
                {
                    //Collapse the parent project
                    int parentProjectId = -artifactId;
                    UserSettingsCollection userSettings = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_GENERAL);
                    string serializedValue = "";
                    if (userSettings.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS))
                    {
                        serializedValue = (string)userSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS];
                    }
                    List<int> expandedProjectIds = serializedValue.FromDatabaseSerialization_List_Int32();
                    if (expandedProjectIds.Contains(parentProjectId))
                    {
                        expandedProjectIds.Remove(parentProjectId);
                        userSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS] = expandedProjectIds.ToDatabaseSerialization();
                        userSettings.Save();
                    }
                }
                else
                {
                    //Collapse the release
                    ReleaseManager releaseManager = new ReleaseManager();
                    ReleaseView release = releaseManager.RetrieveById(User.UserInternal, null, artifactId);
                    releaseManager.Collapse(userId, release.ProjectId, artifactId);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Expands a release node
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project group we're interested in</param>
        /// <param name="artifactId">The release we're expanding</param>
        public void Expand(int projectId, int artifactId)
        {
            const string METHOD_NAME = "Expand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //If we have a negative release id, it is actually a parent project entry that needs to be expanded/collapsed
                if (artifactId < 0)
                {
                    //Collapse the parent project
                    int parentProjectId = -artifactId;
                    UserSettingsCollection userSettings = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_GENERAL);
                    string serializedValue = "";
                    if (userSettings.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS))
                    {
                        serializedValue = (string)userSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS];
                    }
                    List<int> expandedProjectIds = serializedValue.FromDatabaseSerialization_List_Int32();
                    if (!expandedProjectIds.Contains(parentProjectId))
                    {
                        expandedProjectIds.Add(parentProjectId);
                        userSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS] = expandedProjectIds.ToDatabaseSerialization();
                        userSettings.Save();
                    }
                }
                else
                {
                    //Expand the release
                    ReleaseManager releaseManager = new ReleaseManager();
                    ReleaseView release = releaseManager.RetrieveById(User.UserInternal, null, artifactId);
                    releaseManager.Expand(userId, release.ProjectId, artifactId);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Expands the list of releases to a specific level
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="level">The number of levels to expand to (pass -1 for all levels)</param>
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

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get all the projects in the specified group
                List<ProjectView> projects = new ProjectManager().Project_RetrieveByGroup(projectGroupId);
                foreach (ProjectView project in projects)
                {
                    //Change the indentation level of the entire release list
                    Business.ReleaseManager releaseManager = new Business.ReleaseManager();
                    if (level == -1)
                    {
                        //Handle the 'all-levels' case
                        releaseManager.ExpandToLevel(userId, project.ProjectId, null);
                    }
                    else
                    {
                        //We subtract 1 from the level, because the project name acts as the level in the group list
                        releaseManager.ExpandToLevel(userId, project.ProjectId, level - 1);
                    }
                }

                //Now we need to also adjust the project's expansion level
                UserSettingsCollection userSettings = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_GENERAL);
                List<int> expandedProjectIds;
                if (level > 1 || level == -1 /* All Levels*/)
                {
                    //Expand the projects
                    expandedProjectIds = projects.Select(p => p.ProjectId).ToList();
                }
                else
                {
                    expandedProjectIds = new List<int>();   //Collapse the projects
                }
                userSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS] = expandedProjectIds.ToDatabaseSerialization();
                userSettings.Save();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
        #endregion

        #region Internal methods

        /// <summary>
        /// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
        /// </summary>
        /// <param name="projectGroupId">The project group we're interested in</param>
        /// <param name="userId">The user we're viewing the releases as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        /// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
        protected void PopulateShape(int projectGroupId, int userId, HierarchicalDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
        {
            //Get the list of fields to display for the release group list (vs. the project list)
            ReleaseManager releaseManager = new ReleaseManager();
            List<ArtifactListFieldDisplay> artifactFields = releaseManager.RetrieveFieldsForProjectGroupLists(projectGroupId, userId, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_COLUMNS);

            int visibleColumnCount = 0;
            DataItemField dataItemField;
            foreach (ArtifactListFieldDisplay artifactField in artifactFields)
            {
                //Only show visible columns
                if (artifactField.IsVisible)
                {
                    visibleColumnCount++;
                    //We need to get the datatype of this field
                    string fieldName = artifactField.Name;
                    string lookupField = artifactField.LookupProperty;

                    //Create the template item field
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = fieldName;
                    dataItemField.AllowDragAndDrop = true;
                    dataItemField.FieldType = (DataModel.Artifact.ArtifactFieldTypeEnum)artifactField.ArtifactFieldTypeId;
                    dataItemField.Width = artifactField.Width;

                    //Populate the shape depending on the type of field
                    switch (dataItemField.FieldType)
                    {
                        case DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup:
                            dataItemField.LookupName = lookupField;

                            //Set the list of possible lookup values
                            dataItemField.Lookups = GetLookupValues(fieldName, projectGroupId);

                            //Set the filter value (if one is set)
                            if (filterList != null && filterList.Contains(fieldName))
                            {
                                if (filterList[fieldName].GetType() == typeof(int))
                                {
                                    dataItemField.IntValue = (int)filterList[fieldName];
                                }
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Lookup:
                            {
                                dataItemField.LookupName = lookupField;

                                //Set the list of possible lookup values
                                dataItemField.Lookups = GetLookupValues(fieldName, projectGroupId);

                                //Set the filter value (if one is set)
                                if (filterList != null && filterList.Contains(fieldName))
                                {
                                    //handle single-value and multi-value cases correctly
                                    if (filterList[fieldName] is MultiValueFilter)
                                    {
                                        MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[fieldName];
                                        dataItemField.TextValue = multiValueFilter.ToString();
                                    }
                                    if (filterList[fieldName] is Int32)
                                    {
                                        int singleValueFilter = (int)filterList[fieldName];
                                        dataItemField.TextValue = singleValueFilter.ToString();
                                    }
                                }
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.MultiList:
                            {
                                //Set the list of possible lookup values
                                dataItemField.Lookups = GetLookupValues(fieldName, projectGroupId);

                                //Set the filter value (if one is set)
                                if (filterList != null && filterList.Contains(fieldName))
                                {
                                    //handle single-value and multi-value cases correctly
                                    if (filterList[fieldName] is MultiValueFilter)
                                    {
                                        MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[fieldName];
                                        dataItemField.TextValue = multiValueFilter.ToString();
                                    }
                                    if (filterList[fieldName] is Int32)
                                    {
                                        int singleValueFilter = (int)filterList[fieldName];
                                        dataItemField.TextValue = singleValueFilter.ToString();
                                    }
                                }
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer:
                            {
                                PopulateEqualizerShape(fieldName, dataItemField, filterList, projectGroupId);
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Flag:
                            //Set the list of possible lookup values
                            dataItemField.Lookups = GetLookupValues(fieldName, projectGroupId);


                            //Set the filter value (if one is set)
                            if (filterList != null && filterList.Contains(fieldName))
                            {
                                dataItemField.TextValue = (string)filterList[fieldName];
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Text:
                        case DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription:
                            //Set the filter value (if one is set)
                            if (filterList != null && filterList.Contains(fieldName))
                            {
                                dataItemField.TextValue = (string)filterList[fieldName];
                            }
                            break;
                        case DataModel.Artifact.ArtifactFieldTypeEnum.DateTime:
                            //Set the filter value (if one is set)
                            if (filterList != null && filterList.Contains(fieldName))
                            {
                                //Need to convert into the displayable date form
                                DateRange dateRange = (DateRange)filterList[fieldName];
                                string textValue = null;
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
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval:
                            //Set the filter value (if one is set)
                            if (filterList != null && filterList.Contains(fieldName) && filterList[fieldName] is EffortRange)
                            {
                                //Need to convert into the displayable range form
                                EffortRange effortRange = (EffortRange)filterList[fieldName];
                                dataItemField.TextValue = effortRange.ToString();
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Decimal:
                            //Set the filter value (if one is set)
                            if (filterList != null && filterList.Contains(fieldName) && filterList[fieldName] is DecimalRange)
                            {
                                //Need to convert into the displayable date form
                                DecimalRange decimalRange = (DecimalRange)filterList[fieldName];
                                dataItemField.TextValue = decimalRange.ToString();
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Integer:
                            //Set the filter value (if one is set)
                            if (filterList != null && filterList.Contains(fieldName) && filterList[fieldName] is IntRange)
                            {
                                //Need to convert into the displayable date form
                                IntRange intRange = (IntRange)filterList[fieldName];
                                dataItemField.TextValue = intRange.ToString();
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Identifier:
                            //Set the filter value
                            if (filterList != null && filterList.Contains(fieldName))
                            {
                                dataItemField.IntValue = (int)filterList[fieldName];
                                dataItemField.TextValue = dataItemField.IntValue.ToString();
                            }
                            break;
                    }

                    //See if we have a localized caption, otherwise use the default
                    //For the primary key fields, need to always use the localized name for ID
                    string localizedName = Resources.Fields.ResourceManager.GetString(fieldName);
                    if (dataItemField.FieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Identifier)
                    {
                        localizedName = Resources.Fields.ID;
                    }
                    if (String.IsNullOrEmpty(localizedName))
                    {
                        dataItemField.Caption = artifactField.Caption;
                    }
                    else
                    {
                        dataItemField.Caption = localizedName;
                    }
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                }
            }
        }

        /// <summary>
        /// Populates a pseudo-release from a project name. Used on the release list page to show the parent projects as
        /// as pseudo-releases
        /// </summary>
        /// <param name="isExpanded">Should we display as expanded</param>
        /// <param name="dataItem">The data item</param>
        /// <param name="projectArtifactSharing">The project sharing record</param>
        protected void PopulateRow(HierarchicalDataItem dataItem, ProjectView project, string runningIndent, bool isExpanded)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = -project.ProjectId;
            dataItem.Fields["Name"].TextValue = project.Name;

            //Specify if this is a summary row and whether expanded or not
            dataItem.Summary = true;
            dataItem.Expanded = isExpanded;
            dataItem.Alternate = false;
            dataItem.Attachment = false;
            dataItem.ReadOnly = true;
            dataItem.NotSelectable = true;

            //Add a custom URL to the release list page for that project
            dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, project.ProjectId));

            //Specify its indent position
            dataItem.Indent = runningIndent;
        }

        /// <summary>
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="projectIndentLevel">The parent project's indent level</param>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="releaseView">The datarow containing the data</param>
        /// <param name="editable">Does the data need to be in editable form?</param>
        protected void PopulateRow(HierarchicalDataItem dataItem, ReleaseView releaseView, bool editable, string projectIndentLevel)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = releaseView.ReleaseId;
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, releaseView.ConcurrencyDate);

            //Set the custom URL (needed since each project may be different)
            dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, releaseView.ProjectId, releaseView.ReleaseId));

            //Specify if this is a summary row and whether expanded or not
            dataItem.Summary = releaseView.IsSummary;
            dataItem.Expanded = releaseView.IsExpanded;

            //Specify if it has an attachment or not
            dataItem.Attachment = releaseView.IsAttachments;

            //Specify if it is an iteration or not
            if (!dataItem.Summary)
            {
                dataItem.Alternate = (releaseView.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration || releaseView.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Phase);
            }

            //Specify its indent position (add on to the project indent)
            dataItem.Indent = projectIndentLevel + releaseView.IndentLevel;

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (releaseView.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, releaseView, null, null, editable, PopulateEqualizer);

                    //Apply the conditional formatting to the Start Date column (if displayed)
                    if (dataItemField.FieldName == "StartDate" && releaseView.StartDate < DateTime.UtcNow && releaseView.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned)
                    {
                        dataItemField.CssClass = "Warning";
                    }

                    //Apply the conditional formatting to the End Date column (if displayed)
                    if (dataItemField.FieldName == "EndDate" && releaseView.EndDate < DateTime.UtcNow && (releaseView.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned || releaseView.ReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress))
                    {
                        dataItemField.CssClass = "Warning";
                    }
                }
            }
        }

        /// <summary>
        /// Populates the equalizer type graph for the release test  status field
        /// </summary>
        /// <param name="dataItem">The data item being populated</param>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="artifact">The data row</param>
        protected void PopulateEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
        {
            //Recast to the specific artifact entity
            ReleaseView releaseRow = (ReleaseView)artifact;

            //See which equalizer we have
            if (dataItemField.FieldName == "CountPassed")
            {
                //Configure the equalizer bars for test status
                int passedCount = releaseRow.CountPassed;
                int failureCount = releaseRow.CountFailed;
                int cautionCount = releaseRow.CountCaution;
                int blockedCount = releaseRow.CountBlocked;
                int notRunCount = releaseRow.CountNotRun;
                int notApplicableCount = releaseRow.CountNotApplicable;

                //Calculate the percentages, handling rounding correctly
                //We don't include N/A ones in the total as they are either inactive or folders
                int totalCount = passedCount + failureCount + cautionCount + blockedCount + notRunCount;
                int percentPassed = 0;
                int percentFailed = 0;
                int percentCaution = 0;
                int percentBlocked = 0;
                int percentNotRun = 0;
                int percentNotApplicable = 0;
                if (totalCount == 0)
                {
                    dataItemField.TextValue = Resources.Fields.NoTests;
                    dataItemField.CssClass = "NotCovered";
                    dataItemField.Tooltip = Resources.Dialogs.ReleasesService_NoTestsMapped;
                }
                else
                {
                    //Need check to handle divide by zero case
                    percentPassed = (int)Decimal.Round(((decimal)passedCount * (decimal)100) / (decimal)totalCount, 0);
                    percentFailed = (int)Decimal.Round(((decimal)failureCount * (decimal)100) / (decimal)totalCount, 0);
                    percentCaution = (int)Decimal.Round(((decimal)cautionCount * (decimal)100) / (decimal)totalCount, 0);
                    percentBlocked = (int)Decimal.Round(((decimal)blockedCount * (decimal)100) / (decimal)totalCount, 0);
                    percentNotRun = (int)Decimal.Round(((decimal)notRunCount * (decimal)100) / (decimal)totalCount, 0);
                    percentNotApplicable = (int)Decimal.Round(((decimal)notApplicableCount * (decimal)100) / (decimal)totalCount, 0);

                    //Specify the tooltip to be displayed
                    string tooltipText = "# " + Resources.Fields.Passed + "=" + passedCount.ToString() + ", # " + Resources.Fields.Failed + "=" + failureCount.ToString() + ", # " + Resources.Fields.Caution + "=" + cautionCount.ToString() + ", # " + Resources.Fields.Blocked + "=" + blockedCount.ToString() + ", # " + Resources.Fields.NotRun + "=" + notRunCount.ToString();
                    dataItemField.Tooltip = tooltipText;

                    //Now populate the equalizer graph
                    dataItemField.EqualizerGreen = percentPassed;
                    dataItemField.EqualizerRed = percentFailed;
                    dataItemField.EqualizerOrange = percentCaution;
                    dataItemField.EqualizerYellow = percentBlocked;
                    dataItemField.EqualizerGray = percentNotRun;
                }
            }
            if (dataItemField.FieldName == "TaskCount")
            {
                //First see how many tasks we have
                int taskCount = releaseRow.TaskCount;

                //Handle the no tasks case first
                if (taskCount == 0)
                {
                    dataItemField.Tooltip = ReleaseManager.GenerateTaskProgressTooltip(releaseRow);
                    dataItemField.TextValue = ReleaseManager.GenerateTaskProgressTooltip(releaseRow);
                    dataItemField.CssClass = "NotCovered";
                }
                else
                {
                    //Populate the percentages                    
                    dataItemField.EqualizerGreen = (releaseRow.TaskPercentOnTime < 0) ? 0 : releaseRow.TaskPercentOnTime;
                    dataItemField.EqualizerRed = (releaseRow.TaskPercentLateFinish < 0) ? 0 : releaseRow.TaskPercentLateFinish;
                    dataItemField.EqualizerYellow = (releaseRow.TaskPercentLateStart < 0) ? 0 : releaseRow.TaskPercentLateStart;
                    dataItemField.EqualizerGray = (releaseRow.TaskPercentNotStart < 0) ? 0 : releaseRow.TaskPercentNotStart;

                    //Populate Tooltip
                    dataItemField.TextValue = "";
                    dataItemField.Tooltip = ReleaseManager.GenerateTaskProgressTooltip(releaseRow);
                }
            }

            if (dataItemField.FieldName == "PercentComplete")
            {
                //First see how many requirements we have
                int requirementCount = releaseRow.RequirementCount;

                //Handle the no requirements case first
                if (requirementCount == 0)
                {
                    dataItemField.Tooltip = ReleaseManager.GenerateReqCompletionTooltip(releaseRow);
                    dataItemField.TextValue = ReleaseManager.GenerateReqCompletionTooltip(releaseRow);
                    dataItemField.CssClass = "NotCovered";
                }
                else
                {
                    //Populate the percentages
                    //It's always shown as green
                    dataItemField.EqualizerGreen = (releaseRow.PercentComplete < 0) ? 0 : releaseRow.PercentComplete;
                    dataItemField.EqualizerRed = 0;
                    dataItemField.EqualizerGray = (releaseRow.PercentComplete < 0) ? 100 : (100 - releaseRow.PercentComplete);

                    //Populate Tooltip
                    dataItemField.TextValue = "";
                    dataItemField.Tooltip = ReleaseManager.GenerateReqCompletionTooltip(releaseRow);
                }
            }
        }


        /// <summary>
        /// Gets the list of lookup values and names for a specific lookup
        /// </summary>
        /// <param name="lookupName">The name of the lookup</param>
        /// <param name="projectGroupId">The id of the project group - needed for some lookups</param>
        /// <returns>The name/value pairs</returns>
        protected JsonDictionaryOfStrings GetLookupValues(string lookupName, int projectGroupId)
        {
            const string METHOD_NAME = "GetLookupValues";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                JsonDictionaryOfStrings lookupValues = null;
                ReleaseManager release = new ReleaseManager();
                Business.UserManager user = new Business.UserManager();
                TaskManager task = new TaskManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                if (lookupName == "CreatorId" || lookupName == "OwnerId")
                {
                    List<ProjectResourceView> resources = new ProjectGroupManager().RetrieveResourcesForGroup(projectGroupId);
                    lookupValues = ConvertLookupValues(resources.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
                }
                if (lookupName == "ReleaseStatusId")
                {
                    List<ReleaseStatus> releaseStati = new ReleaseManager().RetrieveStatuses();
                    lookupValues = ConvertLookupValues(releaseStati.OfType<DataModel.Entity>().ToList(), "ReleaseStatusId", "Name");
                }
                if (lookupName == "ReleaseTypeId")
                {
                    List<ReleaseType> releaseTypes = new ReleaseManager().RetrieveTypes();
                    lookupValues = ConvertLookupValues(releaseTypes.OfType<DataModel.Entity>().ToList(), "ReleaseTypeId", "Name");
                }
                if (lookupName == "CoverageId")
                {
                    lookupValues = new JsonDictionaryOfStrings(release.RetrieveCoverageFiltersLookup());
                }
                if (lookupName == "ProgressId")
                {
                    lookupValues = new JsonDictionaryOfStrings(task.RetrieveProgressFiltersLookup());
                }
                if (lookupName == "CompletionId")
                {
                    lookupValues = new JsonDictionaryOfStrings(new RequirementManager().RetrieveCompletionFiltersLookup());
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
        /// Used to populate the shape of the special compound fields used to display the information
        /// in the color-coded bar-chart 'equalizer' fields where different colors represent different values
        /// </summary>
        /// <param name="dataItemField">The field whose shape we're populating</param>
        /// <param name="fieldName">The field name we're handling</param>
        /// <param name="filterList">The list of filters</param>
        /// <param name="projectId">The project group we're interested in</param>
        protected void PopulateEqualizerShape(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectGroupId)
        {
            //Check to see if this is a field we can handle
            if (fieldName == "CoverageId")
            {
                dataItemField.FieldName = "CountPassed";
                string filterLookupName = fieldName;
                dataItemField.Lookups = GetLookupValues(filterLookupName, projectGroupId);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(filterLookupName))
                {
                    dataItemField.IntValue = (int)filterList[filterLookupName];
                }
            }
            if (fieldName == "ProgressId")
            {
                dataItemField.FieldName = "TaskCount";
                string filterLookupName = fieldName;
                dataItemField.Lookups = GetLookupValues(filterLookupName, projectGroupId);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(filterLookupName))
                {
                    dataItemField.IntValue = (int)filterList[filterLookupName];
                }
            }
            if (fieldName == "CompletionId")
            {
                dataItemField.FieldName = "PercentComplete";
                string filterLookupName = fieldName;
                dataItemField.Lookups = GetLookupValues(filterLookupName, projectGroupId);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(filterLookupName))
                {
                    dataItemField.IntValue = (int)filterList[filterLookupName];
                }
            }
        }

        #endregion

        #region NotImplemented methods

        public void Copy(int projectId, List<string> sourceItems, int destId)
        {
            throw new NotImplementedException();
        }

        public void Delete(int projectId, List<string> items)
        {
            throw new NotImplementedException();
        }


        public void Export(int sourceProjectId, int destProjectId, List<string> items)
        {
            throw new NotImplementedException();
        }

        public int HierarchicalList_Insert(int projectId, JsonDictionaryOfStrings standardFilters, int artifactId, string artifact)
        {
            throw new NotImplementedException();
        }

        public List<ValidationMessage> HierarchicalList_Update(int projectId, List<HierarchicalDataItem> dataItems)
        {
            throw new NotImplementedException();
        }

        public string Indent(int projectId, List<string> items)
        {
            throw new NotImplementedException();
        }

        public void Move(int projectId, List<string> sourceItems, int? destId)
        {
            throw new NotImplementedException();
        }

        public string Outdent(int projectId, List<string> items)
        {
            throw new NotImplementedException();
        }

        public HierarchicalDataItem Refresh(int projectId, int artifactId)
        {
            throw new NotImplementedException();
        }

        public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
        {
            throw new NotImplementedException();
        }

        public JsonDictionaryOfStrings RetrieveHierarchy(int projectId, JsonDictionaryOfStrings standardFilters)
        {
            throw new NotImplementedException();
        }

        public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
