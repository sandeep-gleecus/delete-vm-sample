using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>Provides the web service used to interacting with the various client-side artifact association AJAX components</summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class HistoryActivityService : SortedListServiceBase, IHistoryActivityService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.HistoryActivityService::";
		private Dictionary<int, string> _projNames;

		#region ISortedListService methods

		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			throw new NotImplementedException();
		}

		/// <summary>Updates the current sort stored in the system (property and direction)</summary>
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

			//Call the base method with the appropriate settings collection
            if (projectId < 1)
            {
                //User Profile > Activity
                UserSettingsCollection sortSettings = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS);
                //Get the appropriate direction name ASC|DESC
                string sortDirection = (sortAscending) ? "ASC" : "DESC";
                sortSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION] = sortProperty + " " + sortDirection;
                sortSettings.Save();
                return "";  //Success
            }
            else
            {
                //Make sure we're authorized
                Project.AuthorizationState authorizationState = IsAuthorized(projectId);
                if (authorizationState == Project.AuthorizationState.Prohibited)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Project Resources > Activity
                return base.UpdateSort(userId, projectId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_ACTIONSORT_EXPRESSION);
            }
		}

		/// <summary>Returns the artifact history for the specific artifact</summary>
		/// <param name="userId">The user we're viewing the data as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">Contains required ArtifactType and ArtifactId filters</param>
		/// <returns>Collection of dataitems</returns>
		public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = CLASS_NAME + "Retrieve(int,int,JsonDict)";

			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//See if we have a user id passed through as a filter (for the case where it's a user's own activity)
			int? activityUserId = null;
            if (standardFilters != null && standardFilters.ContainsKey("UserId"))
			{
				activityUserId = (int)GlobalFunctions.DeSerializeValue(standardFilters["UserId"]);
			}

			//Make sure we're authorized (doesn't apply if this is the case where it's a user's own activity)
			//since a project may not be specified
			if (!(activityUserId.HasValue && activityUserId.Value == userId && projectId < 1))
			{
				Project.AuthorizationState authorizationState = IsAuthorized(projectId);
				if (authorizationState == Project.AuthorizationState.Prohibited)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}
			}

			try
			{
				//Instantiate the history business object
				HistoryManager history = new HistoryManager();

				//Create the array of data items (including the first filter item)
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

				//Now get the list of populated filters and the current sort and Pagination options.
				Hashtable filterList = new Hashtable();
				Hashtable pageSettings = new Hashtable();
				string sortCommand = "";
				if (projectId < 1 && activityUserId.HasValue)
				{
                    //The user profile history tab

					//Get the filters.
					UserSettingsCollection userSettings = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_USER_PROFILE_ACTIONFILTER);
					userSettings.Restore();
					filterList = userSettings;

					//Get the sort.
					sortCommand = "ChangeDate DESC";
					UserSettingsCollection userSettingsSort = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS);
					userSettingsSort.Restore();

                    if (userSettingsSort[GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION] != null)
                        sortCommand = (string)userSettingsSort[GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION];

					//Get the page settings..
					pageSettings = userSettingsSort;
				}
				else
				{
                    //The resource details history tab

					filterList = this.GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_ACTIONFILTERS_LIST);
					sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_ACTIONSORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "ChangeDate DESC");

					//Now get the pagination information
					ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS);
					paginationSettings.Restore();
					pageSettings = paginationSettings;
				}

				string sortProperty = sortCommand.Split(' ')[0];
				string sortDirectionString = sortCommand.Split(' ')[1];
				bool sortAscending = (sortDirectionString == "ASC");
				sortedData.FilterNames = GetFilterNames(filterList);

				//Create the filter item first - we can clone it later
                bool includeProjectField = false;
                bool includeUserField = false;
                if (activityUserId.HasValue)
                {
                    //Resource Details & User Profile > History
                    filterList.Add("UserId", activityUserId.Value);
                    includeProjectField = true;
                }
                else
                {
                    //Activity Stream List
                    includeUserField = true;

                    //Get a list of all artifacts that the user has view permissions for
                    if (!SpiraContext.Current.IsProjectAdmin)
                    {
                        if (!SpiraContext.Current.ProjectRoleId.HasValue)
                        {
                            //Force no results
                            filterList["ArtifactTypeId"] = "-1";
                        }
                        else
                        {
                            //Get the list of artifact types that the current user is allowed to view
                            ProjectManager projectManager = new ProjectManager();
                            ProjectRole projectRole = projectManager.RetrieveRolePermissions(SpiraContext.Current.ProjectRoleId.Value);

                            //If limited view user don't show anything
                            if (projectRole == null || projectRole.IsLimitedView)
                            {
                                //Force no results
                                filterList["ArtifactTypeId"] = "-1";
                            }
                            else
                            {
                                //Apply as a filter if no filter already specified, otherwise
                                //limit specified filter to those values
                                if (filterList["ArtifactTypeId"] != null && filterList["ArtifactTypeId"] is MultiValueFilter)
                                {
                                    MultiValueFilter oldMvf = (MultiValueFilter)filterList["ArtifactTypeId"];
                                    MultiValueFilter mvf = new MultiValueFilter();
                                    if (oldMvf.IsNone)
                                    {
                                        //Handle the special 'None' case
                                        mvf = oldMvf;
                                    }
                                    else
                                    {
                                        foreach (ProjectRolePermission projectRolePermission in projectRole.RolePermissions)
                                        {
                                            if (projectRolePermission.PermissionId == (int)Project.PermissionEnum.View && oldMvf.Values.Contains(projectRolePermission.ArtifactTypeId))
                                            {
                                                mvf.Values.Add(projectRolePermission.ArtifactTypeId);
                                            }
                                        }
                                    }
                                    filterList["ArtifactTypeId"] = mvf;
                                }
                                else
                                {
                                    MultiValueFilter mvf = new MultiValueFilter();
                                    foreach (ProjectRolePermission projectRolePermission in projectRole.RolePermissions)
                                    {
                                        if (projectRolePermission.PermissionId == (int)Project.PermissionEnum.View)
                                        {
                                            mvf.Values.Add(projectRolePermission.ArtifactTypeId);
                                        }
                                    }
                                    filterList["ArtifactTypeId"] = mvf;
                                }
                            }
                        }
                    }
                }

				SortedDataItem filterItem = new SortedDataItem();
                this.PopulateShape(projectId, userId, filterItem, filterList, includeUserField, includeProjectField);
				dataItems.Add(filterItem);

				//Default values
				int paginationSize = 15;
				int currentPage = 1;
				if (pageSettings["Activity.NumberRowsPerPage"] != null)
					paginationSize = (int)pageSettings["Activity.NumberRowsPerPage"];
				if (pageSettings["Activity.CurrentPage"] != null)
					currentPage = (int)pageSettings["Activity.CurrentPage"];

				//Get the count of items
				int artifactCount = history.CountSet((projectId > 0) ? (int?)projectId : null, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				int pageCount = (int)Decimal.Floor((decimal)artifactCount / (decimal)paginationSize) + 1;

				//Make sure that the current page is not larger than the number of pages or less than 1
				if ((currentPage > pageCount) || (currentPage < 1))
				{
					if (currentPage < 1)
						currentPage = 1;
					else
						currentPage = pageCount;

					//Save the new settings..
					if (projectId < 1)
					{
						UserSettingsCollection userSettings = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS);
						userSettings.Restore();
						userSettings["Actions.CurrentPage"] = currentPage;
						userSettings.Save();
					}
					else
					{
						ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS);
						paginationSettings.Restore();
						paginationSettings["Actions.CurrentPage"] = currentPage;
						paginationSettings.Save();
					}
				}

				int startRow = ((currentPage - 1) * paginationSize) + 1;

				//Actually retrieve the list of history change sets
				List<HistoryChangeSetView> historyChangeSets = history.RetrieveSetsByProjectId((projectId > 0) ? (int?)projectId : null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), sortProperty, sortAscending, filterList, startRow, paginationSize);

				//Set count info..
                sortedData.TotalCount = new HistoryManager().CountSet((projectId > 0) ? (int?)projectId : null, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                sortedData.VisibleCount = historyChangeSets.Count;

				//Display the pagination information
				sortedData.CurrPage = currentPage;
				sortedData.PageCount = pageCount;
				sortedData.StartRow = startRow;

				//Display the sort information
				sortedData.SortProperty = sortProperty;
				sortedData.SortAscending = sortAscending;

				//Iterate through all the artifact links in the pagination range and populate the dataitem
                foreach (HistoryChangeSetView historyRow in historyChangeSets)
				{
					try
					{
						SortedDataItem dataItem = filterItem.Clone();

						//Now populate with the data
						PopulateRow(projectId, dataItem, historyRow);
						dataItems.Add(dataItem);
					}
					catch (Exception ex2)
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex2, "Populating table row.");
					}
				}

				//Also include the pagination info
				sortedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

				Logger.LogExitingEvent(METHOD_NAME);
				return sortedData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region IListService Methods

        /// <summary>
        /// Returns the artifact tooltip for the changeSet in question
        /// </summary>
        /// <param name="changeSetId"></param>
        /// <returns></returns>
        public string RetrieveNameDesc(int? projectId, int changeSetId, int? displayTypeId)
        {
            const string METHOD_NAME = CLASS_NAME + "RetrieveNameDesc(int)";
            Logger.LogEnteringEvent(METHOD_NAME);

            try
            {
                //Make sure we're authenticated
                if (!this.CurrentUserId.HasValue)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
                }
                int userId = this.CurrentUserId.Value;

                string tooltip = HistoryActivityService.RetrieveArtifactNameDesc(userId, changeSetId);

                Logger.LogExitingEvent(METHOD_NAME);
                return tooltip;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                return "";
            }
        }

		/// <summary>Returns a list of pagination options that the user can choose from</summary>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		public JsonDictionaryOfStrings RetrievePaginationOptions(int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrievePaginationOptions(int,int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized unless projectId == -1
			if (projectId > 0)
			{
				Project.AuthorizationState authorizationState = IsAuthorized(projectId);
				if (authorizationState == Project.AuthorizationState.Prohibited)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}
			}

			//Delegate to the generic method in the base class - passing the correct collection name
			JsonDictionaryOfStrings paginationDictionary = new JsonDictionaryOfStrings();
			if (projectId == -1)
				paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS, "Activity");
			else
				paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, "Activity");

			Logger.LogExitingEvent(METHOD_NAME);
			return paginationDictionary;
		}

		/// <summary>Updates the size of pages returned and the currently selected page</summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
		/// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
		public void UpdatePagination(int projectId, int pageSize, int currentPage)
		{
			const string METHOD_NAME = CLASS_NAME + "UpdatePagination(int,int,int,int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			if (projectId > 0)
			{
				Project.AuthorizationState authorizationState = IsAuthorized(projectId);
				if (authorizationState == Project.AuthorizationState.Prohibited)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}
			}

			try
			{
				//Get the pagination settings collection and update
				//Need to decide whether it's project-specifiec (Resources page) or user-specific (MyPage).
				if (projectId == -1)
				{
					//Restore settings from database and put in session
					UserSettingsCollection userSettingsCollection = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS);
					userSettingsCollection.Restore();

					if (pageSize != -1)
						userSettingsCollection["Activity.NumberRowsPerPage"] = pageSize;
					if (currentPage != -1)
						userSettingsCollection["Activity.CurrentPage"] = currentPage;
					userSettingsCollection.Save();
				}
				else
				{
					ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS);
					paginationSettings.Restore();
					if (pageSize != -1)
						paginationSettings["Activity.NumberRowsPerPage"] = pageSize;
					if (currentPage != -1)
						paginationSettings["Activity.CurrentPage"] = currentPage;
					paginationSettings.Save();
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
			Logger.LogExitingEvent(METHOD_NAME);
		}

		#endregion

		#region IFilteredListService Methods

		/// <summary>Updates the filters stored in the system</summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="filters">The array of filters (name,value)</param>
		/// <returns>Any error messages</returns>
		public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
		{
			const string METHOD_NAME = CLASS_NAME + "UpdateFilters(int,int,JsonDict)";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			Hashtable filterList = new Hashtable();

			try
			{
				//Iterate through the filters, updating the filter hash.
				foreach (KeyValuePair<string, string> filter in filters)
				{
					string filterName = filter.Key;
					//Now get the type of field that we have. Since history items are not a true artifact,
					//these values have to be hardcoded, as they're not stored in the TST_ARTIFACT_FIELD table
					DataModel.Artifact.ArtifactFieldTypeEnum artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
					switch (filterName)
					{
						case "ChangeDate":
							artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
							break;

						case "ArtifactId":
							artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
							break;

						case "ArtifactDesc":
							artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
							break;

                        case "ProjectId":
                        case "UserId":
                        case "ArtifactTypeId":
						case "ChangeTypeId":
							artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
							break;
					}

					switch (artifactFieldType)
					{
						case DataModel.Artifact.ArtifactFieldTypeEnum.Lookup:
							//Need to make sure that they are MultiValueFilter classes
							MultiValueFilter multiValueFilter;
							if (MultiValueFilter.TryParse(filter.Value, out multiValueFilter))
							{
								filterList.Add(filterName, multiValueFilter);
							}
							break;

						case DataModel.Artifact.ArtifactFieldTypeEnum.Identifier:
							//All identifiers must be numeric
							int filterValueInt = -1;
							if (Int32.TryParse(filter.Value, out filterValueInt))
								filterList.Add(filterName, filterValueInt);
							else
								return String.Format(Resources.Messages.Global_EnterValidIntegerValue, filterName);
							break;

						case DataModel.Artifact.ArtifactFieldTypeEnum.DateTime:
							//If we have date values, need to make sure that they are indeed dates
							//Otherwise we need to throw back a friendly error message
							DateRange dateRange;
							if (!DateRange.TryParse(filter.Value, out dateRange))
							{
								return String.Format(Resources.Messages.Global_EnterValidDateRangeValue, filterName);
							}
							filterList.Add(filterName, dateRange);
							break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Integer:
                            {
                                //If we have integer values, need to make sure that they are indeed integral
                                //or integer ranges
                                IntRange intRange;
                                if (IntRange.TryParse(filter.Value, out intRange))
                                {
                                    filterList.Add(filterName, intRange);
                                }
                                else
                                {
                                    Int32 intValue;
                                    if (!Int32.TryParse(filter.Value, out intValue))
                                    {
                                        return String.Format(Resources.Messages.Global_EnterValidIntegerValue, filterName);
                                    }
                                    filterList.Add(filterName, intValue);
                                }
                            }
                            break;

						case DataModel.Artifact.ArtifactFieldTypeEnum.Text:
						case DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription:
							//For text, just save the value
							filterList.Add(filterName, filter.Value);
							break;

					}
				}

				//Now, save them..
				if (projectId < 1)
				{
					//Get the user setting..
					UserSettingsCollection userSettings = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_USER_PROFILE_ACTIONFILTER);
					userSettings.Clear();
					foreach (DictionaryEntry item in filterList)
						userSettings.Add(item.Key, item.Value);
					userSettings.Save();
				}
				else
				{
                    //Make sure we're authorized
                    Project.AuthorizationState authorizationState = IsAuthorized(projectId);
                    if (authorizationState == Project.AuthorizationState.Prohibited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }

					//Get the project setting...
					ProjectSettingsCollection projectFilters = this.GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RESOURCES_ACTIONFILTERS_LIST);
					projectFilters.Clear();
					foreach (DictionaryEntry item in filterList)
						projectFilters.Add(item.Key, item.Value);
					projectFilters.Save();
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return "";  //Success
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region Private methods

        /// <summary>
        /// Returns the tooltip for a changed artifact
        /// </summary>
        /// <param name="changeSetId">The id of the changeset</param>
        /// <param name="userId">The id of the current user</param>
        /// <returns>This static function is used by this service and the HistoryChangeSetService</returns>
        protected internal static string RetrieveArtifactNameDesc(int userId, int changeSetId)
        {
            string tooltip = "";

            //First get the changeset
            Business.HistoryManager historyManager = new HistoryManager();
            HistoryChangeSet historyChangeSet = historyManager.RetrieveChangeSetById(changeSetId, false);
            if (historyChangeSet != null)
            {
                //Make sure the user is authorized to see this item
                bool authorized = false;
                if (historyChangeSet.UserId == userId)
                {
                    authorized = true;
                }

                //Get the matching artifact
                int artifactId = historyChangeSet.ArtifactId;
                Artifact.ArtifactTypeEnum artifactType = (Artifact.ArtifactTypeEnum)historyChangeSet.ArtifactTypeId;
                if (!authorized)
                {
                    //See if the user has view permissions of this artifact
                    int projectId = historyChangeSet.ProjectId.Value;
                    Business.ProjectManager projectManager = new Business.ProjectManager();
                    ProjectUserView projectMembership = projectManager.RetrieveUserMembershipById(projectId, userId);
                    if (projectMembership != null)
                    {
                        int projectRoleId = projectMembership.ProjectRoleId;
                        ProjectRole projectRole = projectManager.RetrieveRolePermissions(projectRoleId);

                        //If limited view user don't show anything
                        if (projectRole != null && !projectRole.IsLimitedView)
                        {
                            if (projectRole.RolePermissions.Any(p => p.ProjectRoleId == projectRoleId && p.ArtifactTypeId == (int)artifactType && p.PermissionId == (int)Project.PermissionEnum.View))
                            {
                                authorized = true;
                            }
                        }
                    }
                }
                if (artifactId > 0 && authorized)
                {
                    switch (artifactType)
                    {
                        case Artifact.ArtifactTypeEnum.Requirement:
                            {
                                //Instantiate the requirement business object
                                RequirementManager requirementManager = new RequirementManager();

                                //Now retrieve the specific requirement
                                RequirementView requirement = requirementManager.RetrieveById2(null, artifactId);
                                if (String.IsNullOrEmpty(requirement.Description))
                                {
                                    tooltip = GlobalFunctions.HtmlRenderAsPlainText(requirement.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, artifactId, true);
                                }
                                else
                                {
                                    tooltip = "<u>" + GlobalFunctions.HtmlRenderAsPlainText(requirement.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, artifactId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(requirement.Description);
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
				                //Instantiate the test case business object
				                TestCaseManager testCaseManager = new TestCaseManager();

                                //First we need to get the test case itself
                                TestCaseView testCaseView = testCaseManager.RetrieveById(null, artifactId);

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
                                tooltip += "<u>" + GlobalFunctions.HtmlRenderAsPlainText(testCaseView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE, artifactId, true) + "</u>";
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

                        case Artifact.ArtifactTypeEnum.Release:
                            {
                                //Instantiate the release business object
                                ReleaseManager releaseManager = new ReleaseManager();

                                //Now retrieve the specific release - handle quietly if it doesn't exist
                                ReleaseView release = releaseManager.RetrieveById2(null, artifactId);
                                if (String.IsNullOrEmpty(release.Description))
                                {
                                    tooltip = GlobalFunctions.HtmlRenderAsPlainText(release.FullName);
                                }
                                else
                                {
                                    tooltip = "<u>" + GlobalFunctions.HtmlRenderAsPlainText(release.FullName) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(release.Description);
                                }
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.TestSet:
                            {
                                //Instantiate the test set business object
                                TestSetManager testSetManager = new TestSetManager();

                                //First we need to get the test set itself
                                TestSetView testSetView = testSetManager.RetrieveById(null, artifactId);

                                //Next we need to get the list of successive parent folders
                                if (testSetView.TestSetFolderId.HasValue)
                                {
                                    List<TestSetFolderHierarchyView> parentFolders = testSetManager.TestSetFolder_GetParents(testSetView.ProjectId, testSetView.TestSetFolderId.Value, true);
                                    foreach (TestSetFolderHierarchyView parentFolder in parentFolders)
                                    {
                                        tooltip += "<u>" + parentFolder.Name + "</u> &gt; ";
                                    }
                                }

                                //Now we need to get the test set itself
                                tooltip += "<u>" + GlobalFunctions.HtmlRenderAsPlainText(testSetView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TEST_SET, artifactId, true) + "</u>";
                                if (!String.IsNullOrEmpty(testSetView.Description))
                                {
                                    tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testSetView.Description);
                                }

                                //See if we have any comments to append
                                IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(artifactId, Artifact.ArtifactTypeEnum.TestSet, false);
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

                        case Artifact.ArtifactTypeEnum.TestRun:
                            {
                                //Instantiate the testRun business object
                                TestRunManager testRunManager = new TestRunManager();

                                TestRun testrun = testRunManager.RetrieveByIdWithSteps(artifactId);
                                //display the name and execution status
                                tooltip = "<u>" + GlobalFunctions.HtmlRenderAsPlainText(testrun.Name) + " - " + testrun.ExecutionStatus.Name + "</u>";
                                if (!String.IsNullOrEmpty(testrun.Description))
                                {
                                    //Add the description
                                    tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testrun.Description);
                                }

                                //If automated, add the short message
                                if (testrun.TestRunTypeId == (int)TestRun.TestRunTypeEnum.Automated && testrun.RunnerAssertCount.HasValue && !String.IsNullOrEmpty(testrun.RunnerMessage))
                                {
                                    tooltip += "<br />\n<i>" + Resources.Fields.AssertCount + ": " + testrun.RunnerAssertCount.Value + "<br />\n" +
                                        Resources.Fields.RunnerMessage + ": " + GlobalFunctions.HtmlRenderAsPlainText(testrun.RunnerMessage) + "</i>\n";
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

                        case Artifact.ArtifactTypeEnum.Task:
                            {
                                //Instantiate the task business object
                                TaskManager taskManager = new TaskManager();

                                //Now retrieve the specific task
                                TaskView taskView = taskManager.TaskView_RetrieveById(artifactId);
                                if (String.IsNullOrEmpty(taskView.Description))
                                {
                                    //See if we have a requirement it belongs to
                                    if (taskView.RequirementId.HasValue)
                                    {
                                        tooltip += GlobalFunctions.HtmlRenderAsPlainText(taskView.RequirementName) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, taskView.RequirementId.Value, true) + " &gt; ";
                                    }
                                    tooltip += GlobalFunctions.HtmlRenderAsPlainText(taskView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TASK, taskView.TaskId, true);
                                }
                                else
                                {
                                    //See if we have a requirement it belongs to
                                    if (taskView.RequirementId.HasValue)
                                    {
                                        tooltip += "<u>" + GlobalFunctions.HtmlRenderAsPlainText(taskView.RequirementName) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, taskView.RequirementId.Value, true) + "</u> &gt; ";
                                    }
                                    tooltip += "<u>" + GlobalFunctions.HtmlRenderAsPlainText(taskView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TASK, taskView.TaskId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(taskView.Description);
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
				                //Instantiate the incident business object
				                IncidentManager incidentManager = new IncidentManager();

				                //Now retrieve the specific incident - handle quietly if it doesn't exist
					            Incident incident = incidentManager.RetrieveById(artifactId, true);
					            tooltip = "<u>" + GlobalFunctions.HtmlRenderAsPlainText(incident.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_INCIDENT, incident.IncidentId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(incident.Description);

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

                        case Artifact.ArtifactTypeEnum.Document:
                            {
                                //Instantiate the attachment business object
                                AttachmentManager attachmentManager = new AttachmentManager();

                                //Now retrieve the specific document
                                Attachment attachment = attachmentManager.RetrieveById(artifactId);
                                if (String.IsNullOrEmpty(attachment.Description))
                                {
                                    tooltip += GlobalFunctions.HtmlRenderAsPlainText(attachment.Filename) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_DOCUMENT, attachment.AttachmentId, true);
                                }
                                else
                                {
                                    tooltip += "<u>" + GlobalFunctions.HtmlRenderAsPlainText(attachment.Filename) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_DOCUMENT, attachment.AttachmentId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(attachment.Description);
                                }

                                //Documents don't current have comments/discussions
                            }
                            break;
                    }
                }
            }
            return tooltip;
        }

		/// <summary>Populates the 'shape' of the data item that will be used as a template for the retrieved data items</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the artifact links as</param>
        /// <param name="includeProjectField">Should we include the project field</param>
        /// <param name="includeUserField">Should we include the user field</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		protected void PopulateShape(int projectId, int userId, SortedDataItem dataItem, Hashtable filterList, bool includeUserField, bool includeProjectField)
		{
			//Maintenance, clean up project name dictionary..
			this._projNames = new Dictionary<int, string>();

			DataItemField dataItemField = new DataItemField();

			//We need to add the various artifact link fields to be displayed
			#region ChangeDate
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ChangeDate";
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
			dataItemField.Caption = Resources.Fields.ChangeDate;
			dataItemField.AllowDragAndDrop = false;
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
			#endregion

			#region ProjectId

            if (includeProjectField)
            {
                dataItemField = new DataItemField();
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                dataItemField.Caption = Resources.Fields.Project;
                dataItemField.AllowDragAndDrop = false;
                dataItemField.FieldName = "ProjectId";
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the list of possible lookup values
                dataItemField.Lookups = this.GetLookupValues(dataItemField.FieldName, projectId, userId);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName))
                {
                    if (filterList[dataItemField.FieldName] is int)
                    {
                        dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
                    }
                    if (filterList[dataItemField.FieldName] is MultiValueFilter)
                    {
                        dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
                    }
                }
            }

			#endregion

			#region ArtifactTypeId
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ArtifactTypeId";
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
			dataItemField.LookupName = "ArtifactTypeName";
			dataItemField.Caption = Resources.Fields.ArtifactTypeId;
			dataItemField.AllowDragAndDrop = false;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the list of possible lookup values
            dataItemField.Lookups = this.GetLookupValues(dataItemField.FieldName, projectId, userId);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				if (filterList[dataItemField.FieldName] is int)
				{
					dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
				}
				if (filterList[dataItemField.FieldName] is MultiValueFilter)
				{
					dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
				}
			}
			#endregion

			#region ArtifactDesc
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ArtifactDesc";
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
            dataItemField.Caption = Resources.Fields.ArtifactDesc;
			dataItemField.AllowDragAndDrop = false;

			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
				dataItemField.TextValue = (string)filterList[dataItemField.FieldName];

			#endregion

			#region ArtifactId
			dataItemField = new DataItemField();
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
			dataItemField.Caption = Resources.Fields.ArtifactId;
			dataItemField.AllowDragAndDrop = false;
			dataItemField.FieldName = "ArtifactId";
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				if (filterList[dataItemField.FieldName] is int)
				{
					dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
				}
				else if (filterList[dataItemField.FieldName] is IntRange)
				{
                    dataItemField.TextValue = ((IntRange)filterList[dataItemField.FieldName]).ToString();
				}
			}
			#endregion

			#region ChangeTypeID
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ChangeTypeId";
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
			dataItemField.LookupName = "ChangeTypeName";
			dataItemField.Caption = Resources.Fields.ChangeType;
			dataItemField.AllowDragAndDrop = false;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the list of possible lookup values
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, userId);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				if (filterList[dataItemField.FieldName] is int)
				{
					dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
				}
				if (filterList[dataItemField.FieldName] is MultiValueFilter)
				{
					dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
				}
			}
			#endregion

            #region UserId

            if (includeUserField)
            {
                dataItemField = new DataItemField();
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                dataItemField.Caption = Resources.Fields.User;
                dataItemField.AllowDragAndDrop = false;
                dataItemField.FieldName = "UserId";
                dataItemField.LookupName = "UserName";
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the list of possible lookup values
                dataItemField.Lookups = this.GetLookupValues(dataItemField.FieldName, projectId, userId);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName))
                {
                    if (filterList[dataItemField.FieldName] is int)
                    {
                        dataItemField.TextValue = ((int)filterList[dataItemField.FieldName]).ToString();
                    }
                    if (filterList[dataItemField.FieldName] is MultiValueFilter)
                    {
                        dataItemField.TextValue = ((MultiValueFilter)filterList[dataItemField.FieldName]).ToString();
                    }
                }
            }

            #endregion
		}

		/// <summary>Populates a data item from a dataset datarow</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="historyChangeSet">The datarow containing the data</param>
        protected void PopulateRow(int projectId, SortedDataItem dataItem, HistoryChangeSetView historyChangeSet)
		{
			//Set the primary key
			dataItem.PrimaryKey = (int)historyChangeSet.ChangeSetId;

			dataItem.Attachment = false;

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				DataItemField dataItemField = dataItemFieldKVP.Value;

				//Set fixed field values..
				dataItemField.Editable = false;
				dataItemField.Required = false;
				dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)historyChangeSet["ArtifactTypeId"], (int)historyChangeSet["ProjectId"], (int)historyChangeSet["ArtifactId"]));

				string fieldName = dataItemFieldKVP.Key;
                if (historyChangeSet.ContainsProperty(dataItemField.FieldName))
				{
					switch (dataItemField.FieldName)
					{
						case "ArtifactDesc":
							string celText = "";

							//Now the artifact's name..
							if (!String.IsNullOrEmpty(historyChangeSet.ArtifactDesc))
								celText += (string)historyChangeSet["ArtifactDesc"];
							else
							{
								int artifactId = (int)historyChangeSet["ArtifactId"];
								string strName = Resources.Main.Global_Unknown;
								switch ((int)historyChangeSet["ArtifactTypeId"])
								{
									case (int)DataModel.Artifact.ArtifactTypeEnum.Incident:
										strName = new IncidentManager().RetrieveById(artifactId, true).Name;
										break;
									case (int)DataModel.Artifact.ArtifactTypeEnum.Project:
										strName = new ProjectManager().RetrieveById(artifactId).Name;
										break;
									case (int)DataModel.Artifact.ArtifactTypeEnum.Release:
										strName = new ReleaseManager().RetrieveById2(projectId, artifactId, true).FullName;
										break;
									case (int)DataModel.Artifact.ArtifactTypeEnum.Requirement:
										strName = new RequirementManager().RetrieveById(-1, projectId, artifactId, true).Name;
										break;
									case (int)DataModel.Artifact.ArtifactTypeEnum.Task:
										strName = new TaskManager().RetrieveById(artifactId, true).Name;
										break;
                                    case (int)DataModel.Artifact.ArtifactTypeEnum.Document:
                                        strName = new AttachmentManager().RetrieveById(artifactId).Filename;
                                        break;
									case (int)DataModel.Artifact.ArtifactTypeEnum.TestCase:
										strName = new TestCaseManager().RetrieveById(projectId, artifactId, true).Name;
										break;
									case (int)DataModel.Artifact.ArtifactTypeEnum.TestSet:
										strName = new TestSetManager().RetrieveById(projectId, artifactId, true).Name;
										break;
									case (int)DataModel.Artifact.ArtifactTypeEnum.TestStep:
                                        {
                                            TestStep testStep = new TestCaseManager().RetrieveStepById(projectId, artifactId, true);
                                            strName = testStep.TestCase.Name + " [" + Resources.Fields.Step + " " + testStep.ArtifactToken + "]";
                                        }
										break;
								}
								celText += strName;
							}
							dataItemField.TextValue = celText;
							dataItemField.Tooltip = GlobalFunctions.GetIconForArtifactType((DataModel.Artifact.ArtifactTypeEnum)historyChangeSet["ArtifactTypeId"]);
							break;

						case "ProjectId":
							//Get project variables..
							int projId = historyChangeSet.ProjectId.Value;
							string projName = "";
							//Get the project's name..
							if (this._projNames.ContainsKey(projId))
								projName = this._projNames[projId];
							else
							{
								//We need to look it up.
								try
								{
									Project project = new Business.ProjectManager().RetrieveById(projId);
									if (project != null)
									{
										projName = project.Name;

										//Add it ot the list.
										this._projNames.Add(projId, projName);
									}
								}
								catch (ArtifactNotExistsException)
								{
									//Project no longer exists, so just ignore
								}
							}
							dataItemField.IntValue = (int)historyChangeSet["ProjectId"];
							dataItemField.TextValue = projName + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.GetPrefixForArtifactType(DataModel.Artifact.ArtifactTypeEnum.Project), projId, true);
							dataItemField.Tooltip = dataItemField.TextValue;
							break;

						case "ArtifactId":
							dataItemField.IntValue = (int)historyChangeSet["ArtifactId"];
							dataItemField.TextValue = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.GetPrefixForArtifactType((int)historyChangeSet["ArtifactTypeId"]), (int)historyChangeSet["ArtifactId"]);
							break;

						case "ArtifactTypeId":
							dataItemField.IntValue = (int)historyChangeSet["ArtifactTypeId"];
							dataItemField.TextValue = (string)historyChangeSet["ArtifactTypeName"];
							break;

						default:
							this.PopulateFieldRow(dataItem, dataItemField, historyChangeSet, null, (DataModel.ArtifactCustomProperty)null, false, null);
							break;
					}
				}
			}
		}

		/// <summary>Gets the list of lookup values and names for a specific lookup</summary>
		/// <param name="lookupName">The name of the lookup</param>
		/// <param name="projectId">The id of the project - needed for some lookups</param>
        /// <param name="userId">The id of the current user - needed for some lookups</param>
		/// <returns>The name/value pairs</returns>
        protected JsonDictionaryOfStrings GetLookupValues(string lookupName, int projectId, int userId)
		{
			const string METHOD_NAME = "GetLookupValues";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				JsonDictionaryOfStrings lookupValues = null;
				Business.UserManager user = new Business.UserManager();

				if (lookupName == "ChangeTypeId")
				{
					//Generate a list from the enumeration.
					SortedList<int, string> lstTypes = new SortedList<int, string>();
					foreach (HistoryManager.ChangeSetTypeEnum item in Enum.GetValues(typeof(HistoryManager.ChangeSetTypeEnum)))
                        if (!item.ToString().Contains("_"))
                            lstTypes.Add((int)item, item.ToString());
					lookupValues = this.ConvertLookupValues(lstTypes);
				}
				else if (lookupName == "ArtifactTypeId")
				{
					List<ArtifactType> artTypes = new ArtifactManager().ArtifactType_RetrieveAll();
					lookupValues = this.ConvertLookupValues(artTypes.OfType<Entity>().ToList(), "ArtifactTypeId", "Name");
				}
                else if (lookupName == "ProjectId")
                {
                    List<ProjectForUserView> projects = new ProjectManager().RetrieveForUser(userId);
                    lookupValues = this.ConvertLookupValues(projects.OfType<Entity>().ToList(), "ProjectId", "Name");
                }
                else if (lookupName == "UserId")
                {
                    List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
                    lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
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

		#region NotImplemented methods

		public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			throw new NotImplementedException();
		}

		public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
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

		public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
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
