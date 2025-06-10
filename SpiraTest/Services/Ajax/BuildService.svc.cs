using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using System.Collections;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the SortableGrid AJAX component for displaying/updating build data
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class BuildService : SortedListServiceBase, IBuildService, INavigationService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.BuildService::";
		private static int MAX_CHARS_DESCRIPTION = 2000000;


		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
        {
            //Not used since editing is not allowed
            throw new NotImplementedException();
        }

        #region IBuildService methods

        /// <summary>
        /// Gets a list of build ids/names for a specific release
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="releaseId">The current release</param>
        /// <returns>The list of builds</returns>
        public JsonDictionaryOfStrings GetBuildsForRelease(int projectId, int releaseId)
        {
            const string METHOD_NAME = "Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Get the list of builds
                BuildManager buildManager = new BuildManager();
                List<BuildView> builds = buildManager.RetrieveForRelease(projectId, releaseId, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                if (builds == null)
                {
                    return null;
                }
                JsonDictionaryOfStrings lookup = ConvertLookupValues(builds.OfType<Entity>().ToList(), "BuildId", "Name");

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return lookup;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region IFormService Methods

        /// <summary>Returns a single release data record (all columns) for use by the FormManager control</summary>
        /// <param name="artifactId">The id of the current release</param>
        /// <returns>A release data item</returns>
        public DataItem Form_Retrieve(int projectId, int? artifactId)
        {
            const string METHOD_NAME = CLASS_NAME + "Form_Retrieve";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited edit or full view of releases)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business classes
                ReleaseManager releaseManager = new ReleaseManager();
                BuildManager buildManager = new BuildManager();

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, true);

                //Get the build and the release (for permission checking)
                Build build = buildManager.RetrieveById(artifactId.Value);
                ReleaseView releaseView = releaseManager.RetrieveById2(projectId, build.ReleaseId);

                //Make sure the user is authorized for the release that the build is associated with
                int ownerId = -1;
                if (releaseView.OwnerId.HasValue)
                {
                    ownerId = releaseView.OwnerId.Value;
                }
                if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && releaseView.CreatorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Populate the row
                PopulateRow(dataItem, build, false);

                //Also need to return back a special field to denote if the user is the owner or creator of the artifact
                bool isArtifactCreatorOrOwner = (ownerId == userId || releaseView.CreatorId == userId);
                dataItem.Fields.Add("_IsArtifactCreatorOrOwner", new DataItemField() { FieldName = "_IsArtifactCreatorOrOwner", TextValue = isArtifactCreatorOrOwner.ToDatabaseSerialization() });

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return dataItem;
            }
            catch (ArtifactNotExistsException)
            {
                //Just return no data back
                return null;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region ISortedListService Methods

        /// <summary>
        /// Returns a list of builds in the system for the specific user/project/release
        /// </summary>
        /// <param name="userId">The user we're viewing the builds as</param>
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

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the BuildManager class
                BuildManager buildManager = new BuildManager();

                //Create the array of data items (including the first filter item)
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;

                //Now get the list of populated filters and the current sort
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_BUILD_FILTERS_LIST);
                string sortExpression = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "CreationDate DESC");
                sortedData.FilterNames = GetFilterNames(filterList);

                //Make sure that releaseId was passed in as a 'standard filter'
                int releaseId = -1;
                if (standardFilters != null && standardFilters.ContainsKey("ReleaseId"))
                {
                    releaseId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ReleaseId"]);
                }
                if (releaseId == -1)
                {
                    throw new InvalidOperationException("You need to specify a ReleaseId as a standard filter to use the BuildService");
                }
                
                //Create the filter item first - we can clone it later
                SortedDataItem filterItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
                dataItems.Add(filterItem);

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS);
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

                //**** Now retrieve the builds in the pagination range ****
                int startIndex = ((currentPage - 1) * paginationSize);
                int artifactCount;
                List<BuildView> builds = buildManager.RetrieveForRelease(projectId, releaseId, sortExpression, startIndex, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out artifactCount);
                int unfilteredArtifactCount = buildManager.CountForRelease(projectId, releaseId);

                //Calculate the pagination informatio
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

                //Display the pagination information
                startIndex = ((currentPage - 1) * paginationSize);
                sortedData.CurrPage = currentPage;
                sortedData.PageCount = pageCount;
                sortedData.StartRow = startIndex + 1;
                sortedData.VisibleCount = builds.Count;
                sortedData.TotalCount = unfilteredArtifactCount;

                //Display the sort information
                sortedData.SortProperty = sortExpression.Substring(0, sortExpression.IndexOf(" "));
                string sortDirectionString = sortExpression.Substring(sortExpression.IndexOf(" "), sortExpression.Length - sortExpression.IndexOf(" ")).Trim();
                sortedData.SortAscending = (sortDirectionString == "ASC");

                //Iterate through all the builds and populate the dataitem
                foreach (BuildView build in builds)
                {
                    //We clone the template item as the basis of all the new items
                    SortedDataItem dataItem = filterItem.Clone();

                    //Now populate with the data
                    PopulateRow(dataItem, build, false);
                    dataItems.Add(dataItem);
                }

                //Also include the pagination info
                sortedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created data items with " + dataItems.Count.ToString() + " rows");

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
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS);

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
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS);
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
            return base.UpdateSort(userId, projectId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS);
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

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Call the base method with the appropriate settings collection
            return base.UpdateFilters(userId, projectId, filters, GlobalFunctions.PROJECT_SETTINGS_BUILD_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.None);
        }

        /// <summary>
        /// Displays the name and description of the build
        /// </summary>
        /// <param name="artifactId">The id of the build</param>
        /// <returns>The tooltip</returns>
        public string RetrieveNameDesc(int? projectId, int artifactId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the Build manager object
                BuildManager buildManager = new BuildManager();

                //Now retrieve the specific build - handle quietly if it doesn't exist
                try
                {
                    Build build = buildManager.RetrieveById(artifactId);
                    string tooltip;
                    //display the name and execution status
                    if (String.IsNullOrEmpty(build.Description))
                    {
                        tooltip = Microsoft.Security.Application.Encoder.HtmlEncode(build.Name) + " - " + build.BuildStatusName;
                    }
                    else
                    {
                        //Add the description, only the start and end since it is *HUGE*
                        string description = GlobalFunctions.HtmlRenderAsPlainText(build.Description);
                        if (description.Length > 1000)
                        {
                            description = description.Substring(0, 500) + "<br />...<br />" + description.Substring(description.Length - 500);
                        }
                        tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(build.Name) + " - " + build.BuildStatusName + "</u><br />\n" + description;
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the testRun, but it has already been deleted on the server
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for build");
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

        #region INavigationService Methods

        /// <summary>
        /// Returns a list of builds for the current release, to display in the navigation bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="indentLevel">Not used since we always return the whole list on load</param>
        /// <returns>List of builds, potentially including parent release/iteration</returns>
        /// <param name="displayMode">
        /// The display mode of the navigation list:
        /// 1 = Filtered List - Shows parent release/iteration, uses current filter and sort
        /// 2 = All Items - Shows parent release/iteration, no filter, but uses current sort
        /// </param>
        /// <param name="selectedItemId">The id of the currently selected item</param>
        /// <param name="containerId">The of the release that the builds are for</param>
        /// <remarks>
        /// Returns just the child items of the passed-in requirement indent-level
        /// </remarks>
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

			//Make sure we're authorized to view releases
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the business objects
				BuildManager buildManager = new BuildManager();
				ReleaseManager releaseManager = new ReleaseManager();

				//Create the array of data items
				List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();
				int releaseId = containerId.Value;

				//Now get the list of populated filters if appropriate
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_BUILD_FILTERS_LIST);

				//Get the sort information
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "CreationDate DESC");

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS);
				paginationSettings.Restore();
				//Default values
				int paginationSize = 500;
				int currentPage = 0;
				if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE] != null)
				{
					paginationSize = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE];
				}
				if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] != null)
				{
					currentPage = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE];
				}

				//**** Now we need to actually populate the rows of data to be returned ****

				//Get the requirements list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
                int artifactCount = 0;
				List<BuildView> builds = new List<BuildView>(); //Default to empty list
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.FilteredList)
				{
					//Filtered List
					if (authorizationState == Project.AuthorizationState.Limited)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}
                    builds = buildManager.RetrieveForRelease(projectId, releaseId, sortCommand, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out artifactCount);
				}
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.AllItems)
				{
					//All Items
					if (authorizationState == Project.AuthorizationState.Limited)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}
                    builds = buildManager.RetrieveForRelease(projectId, releaseId, sortCommand, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out artifactCount);
				}

				int pageCount = (int)Decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);
				//Make sure that the current page is not larger than the number of pages or less than 0
				if (currentPage > pageCount)
				{
					currentPage = pageCount;
					paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] = currentPage;
					paginationSettings.Save();
				}
				if (currentPage < 0)
				{
					currentPage = 0;
					paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] = currentPage;
					paginationSettings.Save();
				}

				//Retrieve the release and populate as the first item
				try
				{
					ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId);

					//Create the data-item
					HierarchicalDataItem dataItem = new HierarchicalDataItem();

					//Populate the necessary fields
					dataItem.PrimaryKey = 0;    //So we don't try and live-load it
					dataItem.Indent = "AAA";
					dataItem.Expanded = true;

					//We need to pass through an override URL since the releases have a different URL
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, projectId, release.ReleaseId, GlobalFunctions.PARAMETER_TAB_BUILD));

					//Name/Desc
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.TextValue = release.Name;
					dataItemField.Tooltip = (release.IsIterationOrPhase) ? "Images/artifact-Iteration.svg" : "Images/artifact-Release.svg";
					dataItem.Summary = true;    //All releases are listed as summary items
					dataItem.Fields.Add("Name", dataItemField);

					//Add to the items collection
					dataItems.Add(dataItem);

					//Now populate the builds underneath
					PopulateBuilds(builds, dataItems, "AAA");
				}
				catch (ArtifactNotExistsException)
				{
                    //Just get the basic list of builds
                    PopulateBuilds(builds, dataItems, "");
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
        /// Populates the list of builds
        /// </summary>
        /// <param name="builds">The build list</param>
        /// <param name="dataItems">The list of nav-bar items</param>
        /// <param name="indentLevel">The release indent level (if there is one)</param>
        /// <returns>The last used indent level</returns>
        protected string PopulateBuilds(List<BuildView> builds, List<HierarchicalDataItem> dataItems, string indentLevel)
        {
            //Iterate through all the builds and populate the dataitem (only some columns are needed)
            string buildIndentLevel = indentLevel + "AAA"; //Add on to the req indent level
            foreach (BuildView build in builds)
            {
                //Create the data-item
                HierarchicalDataItem dataItem = new HierarchicalDataItem();

                //Populate the necessary fields
                dataItem.PrimaryKey = build.BuildId;
                dataItem.Indent = buildIndentLevel;

                //Name/Desc
                DataItemField dataItemField = new DataItemField();
                dataItemField.FieldName = "Name";
                dataItemField.TextValue = build.Name;
                dataItem.Summary = false;
                dataItem.Alternate = false;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                //Execution Status style
                dataItemField.CssClass = GlobalFunctions.GetBuildStatusCssClass((Build.BuildStatusEnum)build.BuildStatusId);

                //Add to the items collection
                dataItems.Add(dataItem);

                //Increment the indent level
                buildIndentLevel = HierarchicalList.IncrementIndentLevel(buildIndentLevel);
            }

            return buildIndentLevel;
        }

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
        /// Updates the display settings used by the Navigation Bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="displayMode">Not used for this service</param>
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
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS);
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

        #region Internal Functions

        /// <summary>
        /// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the builds as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        /// <param name="allFields">Do we want to populate all fields (used on details page)</param>
        protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool allFields = false)
        {
            //First add the static columns
            //Build Name
            DataItemField dataItemField = new DataItemField();
            dataItemField.FieldName = "Name";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
            dataItemField.Caption = Resources.Fields.BuildName;
            dataItemField.Editable = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }

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
                Common.DateRange dateRange = (Common.DateRange)filterList[dataItemField.FieldName];
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

            //Status
            dataItemField = new DataItemField();
            dataItemField.FieldName = "BuildStatusId";
            dataItemField.Caption = Resources.Fields.BuildStatusId;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
            dataItemField.LookupName = "BuildStatusName";
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

            //Last Update Date
            dataItemField = new DataItemField();
            dataItemField.FieldName = "LastUpdateDate";
            dataItemField.Caption = Resources.Fields.LastUpdated;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                //Need to convert into the displayable date form
                Common.DateRange dateRange = (Common.DateRange)filterList[dataItemField.FieldName];
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

            //Build ID
            dataItemField = new DataItemField();
            dataItemField.FieldName = "BuildId";
            dataItemField.Caption = Resources.Fields.ID;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.IntValue = (int)filterList[dataItemField.FieldName];
            }

            if (allFields)
            {
                //Description - no filtering, since only on details page
                dataItemField = new DataItemField();
                dataItemField.FieldName = "Description";
                dataItemField.Caption = Resources.Fields.Description;
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            }
        }

        /// <summary>
        /// Populates a data item from an entity item
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="build">The entity record containing the data</param>
        /// <param name="editable">Does the data need to be in editable form?</param>
        protected void PopulateRow(SortedDataItem dataItem, BuildView build, bool editable)
        {
            //Set the primary key
            dataItem.PrimaryKey = build.BuildId;

            //Builds do not support attachments (currently)
            dataItem.Attachment = false;

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (build[dataItemField.FieldName] != null)
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, build, null, null, editable, null);

                    //Specify which fields are editable or not
                    dataItemField.Editable = false;
                    //Certain fields do not allow null values
                    dataItemField.Required = false;

                    //Apply the conditional formatting to the build status column (if displayed)
                    if (dataItemField.FieldName == "BuildStatusId")
                    {
                        dataItemField.CssClass = GlobalFunctions.GetBuildStatusCssClass((Build.BuildStatusEnum)build.BuildStatusId);
                    }
                }
            }
        }

        /// <summary>
        /// Populates a data item from an entity item
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="build">The entity record containing the data</param>
        /// <param name="editable">Does the data need to be in editable form?</param>
        protected void PopulateRow(SortedDataItem dataItem, Build build, bool editable)
        {
            //Set the primary key
            dataItem.PrimaryKey = build.BuildId;

            //Builds do not support attachments (currently)
            dataItem.Attachment = false;

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (build[dataItemField.FieldName] != null)
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, build, null, null, editable, null);

                    //Specify which fields are editable or not
                    dataItemField.Editable = false;
                    //Certain fields do not allow null values
                    dataItemField.Required = false;

                    //Apply the conditional formatting to the build status column (if displayed)
                    if (dataItemField.FieldName == "BuildStatusId")
                    {
                        dataItemField.CssClass = GlobalFunctions.GetBuildStatusCssClass((Build.BuildStatusEnum)build.BuildStatusId);
                    }

					//Apply the conditional truncation to the build description (log) field
					if (dataItemField.FieldName == "Description")
					{
						if (dataItemField.TextValue.Length > MAX_CHARS_DESCRIPTION)
						{
							int halfMaxLength = MAX_CHARS_DESCRIPTION / 2; //assume that the max_chars returns an int when divided by 2
							//Add the start of the field, then a clear break, then the end of the field
							string truncatedDescription = "================================================================\n" + Resources.Messages.BuildDetails_DescriptionTruncated + "\n================================================================\n\n";
							truncatedDescription += Strings.SafeSubstring(dataItemField.TextValue, 0, halfMaxLength);
							truncatedDescription += "...\n\n...\n\n...";
							truncatedDescription += Strings.SafeSubstring(dataItemField.TextValue, dataItemField.TextValue.Length - halfMaxLength, halfMaxLength);
							dataItemField.TextValue = truncatedDescription;
						}
					}
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
                BuildManager buildManager = new BuildManager();
                if (lookupName == "BuildStatusId")
                {
                    List<BuildStatus> statuses = buildManager.RetrieveStatuses();
                    lookupValues = ConvertLookupValues(statuses.OfType<Entity>().ToList(), "BuildStatusId", "Name");
                }

                return lookupValues;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region Not Implemented Methods

        public List<ValidationMessage> Form_Save(int projectId, DataItem dataItem, string operation, Signature signature)
        {
            throw new NotImplementedException();
        }

        public List<DataItemField> Form_RetrieveWorkflowFieldStates(int projectId, int typeId, int stepId)
        {
            throw new NotImplementedException();
        }

        public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
        {
            throw new NotImplementedException();
        }

        public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
        {
            throw new NotImplementedException();
        }

        public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            throw new NotImplementedException();
        }

        public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
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

        public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
        {
            throw new NotImplementedException();
        }

        public void ToggleColumnVisibility(int projectId, string fieldName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
