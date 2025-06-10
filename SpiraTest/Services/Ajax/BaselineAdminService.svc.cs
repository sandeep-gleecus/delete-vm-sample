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
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>Provides the web service used to interacting with the various client-side artifact association AJAX components</summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class BaselineAdminService : SortedListServiceBase, IBaselineAdminService
	{
		private const string CLASS_NAME = "Web.Services.Ajax.BaselineAdminService::";

		#region ISortedListService Methods
		/// <summary>Updates the current sort stored in the system (property and direction)</summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="productId">The project we're interested in</param>
		/// <param name="sortProperty">The artifact property we want to sort on</param>
		/// <param name="sortAscending">Are we sorting ascending or not</param>
		/// <returns>Any error messages</returns>
		public string SortedList_UpdateSort(int productId, string sortProperty, bool sortAscending, int? displayTypeId)
		{
			//Make sure we're authenticated
			int userId = checkUserLogged();

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(productId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			//Call the base method with the appropriate settings collection
			return UpdateSort(userId, productId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_BASELINEADMIN_SORT_EXPRESSION);
		}

		/// <summary>Returns the baselines for the specified release</summary>
		/// <param name="userId">The user we're viewing the data as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">Contains required ArtifactType and ArtifactId filters</param>
		/// <returns>Collection of dataitems</returns>
		public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = CLASS_NAME + "SortedList_Retrieve()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			int userId = checkUserLogged();

			//Make sure we're authorized
			bool isProjectAdmin = checkProjectAdmin(projectId);
			if (!isProjectAdmin)
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			//The return object. 
			SortedData sortedData = new SortedData();

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Now get the list of populated filters and the current sort
			Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_BASELINEADMIN_FILTERS_LIST);
			string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_BASELINEADMIN_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "BaselineId DESC");
			string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
			string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
			bool sortAscending = (sortDirectionString == "ASC");
			sortedData.FilterNames = GetFilterNames(filterList);

			//Create the filter item first - we can clone it later
			SortedDataItem filterItem = new SortedDataItem();
			PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
			sortedData.Items.Add(filterItem);

			//Now get the pagination information
			ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_BASELINEADMIN_PAGINATION);
			paginationSettings.Restore();
			//Default values
			int paginationSize = 15;
			int currentPage = 1;
			if (paginationSettings["NumberRowsPerPage"] != null)
				paginationSize = (int)paginationSettings["NumberRowsPerPage"];
			if (paginationSettings["CurrentPage"] != null)
				currentPage = (int)paginationSettings["CurrentPage"];

			//Get the count of items
			BaselineManager baselineManager = new BaselineManager();
			SortFilter sortFilters = new SortFilter
			{
				FilterList = filterList,
				PageSize = paginationSize,
				StartingPage = currentPage,
				SortAscending = sortAscending,
				SortProperty = sortProperty
			};
			sortedData.CurrPage = sortFilters.StartingPage;
			sortedData.StartRow = sortFilters.StartingRowNumber;
			sortedData.SortProperty = sortFilters.SortProperty;
			sortedData.SortAscending = sortFilters.SortAscending;
			sortedData.PaginationOptions = RetrievePaginationOptions(projectId);

			//Sorting checks..
			if (sortedData.SortProperty.Equals("CreatorUserName"))
				sortFilters.SortProperty = "Creator.Profile.FirstName";
            if (sortedData.SortProperty.Equals("ReleaseFullName"))
                sortFilters.SortProperty = "Release.VersionNumber";

            //Actually retrieve the list of baselines
            int totalPreFilter = 0;
			List<ProjectBaseline> baselines = baselineManager.Baseline_RetrieveFilteredForProductRelease(projectId, null, sortFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out totalPreFilter);

			foreach (ProjectBaseline baseline in baselines)
			{
				SortedDataItem dataItem = filterItem.Clone();

				//Now populate with the data
				PopulateRow(projectId, dataItem, baseline, isProjectAdmin);
				sortedData.Items.Add(dataItem);
			}
			sortedData.VisibleCount = baselines.Count();
			sortedData.TotalCount = baselineManager.Baseline_Count(projectId, null);
			sortedData.PageCount = (int)Math.Ceiling(totalPreFilter / (decimal)paginationSize);

			Logger.LogExitingEvent(METHOD_NAME);
			return sortedData;
		}

		/// <summary>Returns new information for the given Baseline ID.</summary>
		/// <param name="projectId">The project ID.</param>
		/// <param name="artifactId">The Baseline Id.</param>
		/// <returns>The item requested.</returns>
		public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
		{
			const string METHOD_NAME = CLASS_NAME + "SortedList_Refresh()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			int userId = checkUserLogged();

			//Make sure we're authorized (limited OK because we need it for the counts)
			bool isProjectAdmin = checkProjectAdmin(projectId);
			if (!isProjectAdmin)
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			//The return item.
			SortedDataItem dataItem = new SortedDataItem();

			//Get the templateid.
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;


			//Get our Baseline Data.
			bool canEditRow = false;
			var baseline = new BaselineManager().Baseline_RetrieveById(artifactId);

			//Populate the shape.
			PopulateShape(projectId, projectTemplateId, userId, dataItem, null);
			PopulateRow(projectId, dataItem, baseline, checkProjectAdmin(projectId), canEditRow);

			Logger.LogExitingEvent(METHOD_NAME);
			return dataItem;
		}
		#endregion ISortedListService Methods

		#region IListService Methods
		/// <summary>Returns text for a tooltip item. At this time, returns an empty string.</summary>
		/// <param name="projectId">The ProductId</param>
		/// <param name="artifactId">The ID of the Baseline.</param>
		/// <param name="displayTypeId">Not used.</param>
		/// <returns></returns>
		public string RetrieveNameDesc(int? projectId, int artifactId, int? displayTypeId)
		{
			return "";
		}

		/// <summary>
		/// Returns a list of pagination options that the user can choose from
		/// </summary>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		public JsonDictionaryOfStrings RetrievePaginationOptions(int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrievePaginationOptions()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			int userId = checkUserLogged();

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			//Delegate to the generic method in the base class - passing the correct collection name
			JsonDictionaryOfStrings paginationDictionary = RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_BASELINEADMIN_PAGINATION);

			Logger.LogExitingEvent(METHOD_NAME);
			return paginationDictionary;
		}

		/// <summary>
		/// Updates the size of pages returned and the currently selected page
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
		/// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
		public void UpdatePagination(int projectId, int pageSize, int currentPage)
		{
			const string METHOD_NAME = CLASS_NAME + "UpdatePagination()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			int userId = checkUserLogged();

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			//Get the pagination settings collection and update
			ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_BASELINEADMIN_PAGINATION);
			paginationSettings.Restore();

			if (pageSize != -1)
				paginationSettings["NumberRowsPerPage"] = pageSize;

			if (currentPage != -1)
				paginationSettings["CurrentPage"] = currentPage;

			paginationSettings.Save();

			Logger.LogExitingEvent(METHOD_NAME);
		}
		#endregion IListService Methods

		#region IFilteredListService Methods
		/// <summary>
		/// Updates the filters stored in the system
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="filters">The array of filters (name,value)</param>
		/// <returns>Any error messages</returns>
		public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
		{
			const string METHOD_NAME = CLASS_NAME + "UpdateFilters()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			int userId = checkUserLogged();

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

            //Get the current filters from session
            ProjectSettingsCollection savedFilters = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_BASELINEADMIN_FILTERS_LIST);
            int oldFilterCount = savedFilters.Count;
            savedFilters.Clear(); //Clear the filters

            //Iterate through the filters, updating the project collection
            foreach (KeyValuePair<string, string> filter in filters)
            {
                string filterName = filter.Key;
                //Now get the type of field that we have. Since baselines are not a true artifact,
                //these values have to be hardcoded, as they're not stored in the TST_ARTIFACT_FIELD table
                Artifact.ArtifactFieldTypeEnum artifactFieldType = Artifact.ArtifactFieldTypeEnum.Text;
                switch (filterName)
                {
                    case "BaselineId":
                        artifactFieldType = Artifact.ArtifactFieldTypeEnum.Identifier;
                        break;

                    case "ChangeSetId":
                        artifactFieldType = Artifact.ArtifactFieldTypeEnum.Integer;
                        break;

                    case "CreationDate":
                        artifactFieldType = Artifact.ArtifactFieldTypeEnum.DateTime;
                        break;

                    case "CreatorUserId":
                        artifactFieldType = Artifact.ArtifactFieldTypeEnum.Lookup;
                        break;

                    case "ReleaseId":
                        artifactFieldType = Artifact.ArtifactFieldTypeEnum.HierarchyLookup;
                        break;

                    case "IsAcive":
                        artifactFieldType = Artifact.ArtifactFieldTypeEnum.Flag;
                        break;
                }

                switch (artifactFieldType)
                {
                    case Artifact.ArtifactFieldTypeEnum.Lookup:
                        {
                            //Need to make sure that they are MultiValueFilter classes
                            MultiValueFilter multiValueFilter;
                            if (MultiValueFilter.TryParse(filter.Value, out multiValueFilter))
                            {
                                savedFilters.Add(filterName, multiValueFilter);
                            }
                        }
                        break;

                    case Artifact.ArtifactFieldTypeEnum.HierarchyLookup:
                        {
                            //Need to make sure that they are int values
                            Int32 intFilter;
                            if (Int32.TryParse(filter.Value, out intFilter))
                            {
                                savedFilters.Add(filterName, intFilter);
                            }
                        }
                        break;

                    case Artifact.ArtifactFieldTypeEnum.Identifier:
                        {
                            //All identifiers must be numeric
                            int filterValueInt = -1;
                            if (int.TryParse(filter.Value, out filterValueInt))
                            {
                                savedFilters.Add(filterName, filterValueInt);
                            }
                            else
                            {
                                return string.Format(Resources.Messages.Global_EnterValidIntegerValue, filterName);
                            }
                        }
                        break;

                    case Artifact.ArtifactFieldTypeEnum.DateTime:
                        {
                            //If we have date values, need to make sure that they are indeed dates
                            //Otherwise we need to throw back a friendly error message
                            DateRange dateRange;
                            if (!DateRange.TryParse(filter.Value, out dateRange))
                            {
                                return string.Format(Resources.Messages.Global_EnterValidDateRangeValue, filterName);
                            }
                            savedFilters.Add(filterName, dateRange);
                        }
                        break;

                    case Artifact.ArtifactFieldTypeEnum.Integer:
                        {
                            //If we have integer values, need to make sure that they are indeed integral
                            //or integer ranges
                            //This service only has an Int64 / Long column to worry about
                            LongRange longRange;
                            if (LongRange.TryParse(filter.Value, out longRange))
                            {
                                savedFilters.Add(filterName, longRange);
                            }
                            else
                            {
                                Int64 longValue;
                                if (!Int64.TryParse(filter.Value, out longValue))
                                {
                                    return String.Format(Resources.Messages.Global_EnterValidIntegerValue, filterName);
                                }
                                savedFilters.Add(filterName, longValue);
                            }
                        }
                        break;

                    case Artifact.ArtifactFieldTypeEnum.NameDescription:
                    case Artifact.ArtifactFieldTypeEnum.Text:
                        {
                            //For text, just save the value
                            savedFilters.Add(filterName, filter.Value);
                        }
                        break;
                }
            }

            //Save the Filter.
            savedFilters.Save();

            Logger.LogExitingEvent(METHOD_NAME);
            return "";  //Success
        }

		#endregion IFilteredListService Methods

		#region Private Methods
		/// <summary>Populates the 'shape' of the data item that will be used as a template for the retrieved data items</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="userId">The user we're viewing the artifact links as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		/// <param name="isProjectAdmin">If the user is a project admin, the name of the baseline is a clickable link</param>
		private void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList)
		{
			//We need to add the various artifact link fields to be displayed

			#region Name
			DataItemField dataFieldName = new DataItemField
			{
				FieldName = "Name",
				FieldType = Artifact.ArtifactFieldTypeEnum.NameDescription,
				Caption = Resources.Fields.Name,
				Required = true
			};
			dataItem.Fields.Add(dataFieldName.FieldName, dataFieldName);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataFieldName.FieldName) && filterList[dataFieldName.FieldName] is string)
				dataFieldName.TextValue = (string)filterList[dataFieldName.FieldName];
			#endregion

			#region Description
			DataItemField dataFieldDescription = new DataItemField
			{
				FieldName = "Description",
				FieldType = Artifact.ArtifactFieldTypeEnum.Text,
				Caption = Resources.Fields.Description
			};
			dataItem.Fields.Add(dataFieldDescription.FieldName, dataFieldDescription);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataFieldDescription.FieldName) && filterList[dataFieldDescription.FieldName] is string)
				dataFieldDescription.TextValue = (string)filterList[dataFieldDescription.FieldName];
			#endregion Description

			#region Release
			DataItemField dataFieldRelease = new DataItemField
			{
				FieldName = "ReleaseId",
				FieldType = Artifact.ArtifactFieldTypeEnum.HierarchyLookup,
				Caption = Resources.Fields.Release,
				LookupName = "ReleaseFullName",
			};
			dataItem.Fields.Add(dataFieldRelease.FieldName, dataFieldRelease);

			//Get the Lookups (the Releases)
			dataFieldRelease.Lookups = GetLookupValues(dataFieldRelease.FieldName, projectId, projectTemplateId);

			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataFieldRelease.FieldName) && filterList[dataFieldRelease.FieldName] is int)
				dataFieldRelease.IntValue = (int)filterList[dataFieldRelease.FieldName];
			#endregion Release

			#region Created By
			DataItemField dataFieldCreator = new DataItemField
			{
				FieldName = "CreatorUserId",
				Caption = Resources.Fields.CreatorId,
				FieldType = Artifact.ArtifactFieldTypeEnum.Lookup,
				LookupName = "CreatorUserName",
			};
			dataItem.Fields.Add(dataFieldCreator.FieldName, dataFieldCreator);
			//Set the list of possible lookup values
			dataFieldCreator.Lookups = GetLookupValues(dataFieldCreator.FieldName, projectId, projectTemplateId);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataFieldCreator.FieldName))
			{
				if (filterList[dataFieldCreator.FieldName] is int)
					dataFieldCreator.TextValue = ((int)filterList[dataFieldCreator.FieldName]).ToString();
				else if (filterList[dataFieldCreator.FieldName] is MultiValueFilter)
					dataFieldCreator.TextValue = ((MultiValueFilter)filterList[dataFieldCreator.FieldName]).ToString();
			}
			#endregion

			#region Creation Date
			DataItemField dataFieldDate = new DataItemField
			{
				FieldName = "CreationDate",
				FieldType = Artifact.ArtifactFieldTypeEnum.DateTime,
				Caption = Resources.Fields.Date,
			};
			dataItem.Fields.Add(dataFieldDate.FieldName, dataFieldDate);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataFieldDate.FieldName))
			{
				//Need to convert into the displayable date form
				DateRange dateRange = (DateRange)filterList[dataFieldDate.FieldName];
				string textValue = "";
				if (dateRange.StartDate.HasValue)
					textValue += string.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.StartDate.Value);
				textValue += "|";
				if (dateRange.EndDate.HasValue)
					textValue += string.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.EndDate.Value);
				dataFieldDate.TextValue = textValue;
			}
			#endregion

			#region Active Flag
			DataItemField dataFieldActive = new DataItemField
			{
				FieldName = "IsActive",
				Caption = Resources.Fields.IsActive,
				FieldType = Artifact.ArtifactFieldTypeEnum.Flag,
				Required = true
			};
			dataItem.Fields.Add(dataFieldActive.FieldName, dataFieldActive);
			//Set the list of possible flag values
			dataFieldActive.Lookups = new JsonDictionaryOfStrings(GlobalFunctions.YesNoList());
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataFieldActive.FieldName))
				dataFieldActive.TextValue = (string)filterList[dataFieldActive.FieldName];
			#endregion

			#region ChangeSet ID
			//Baseline ID
			DataItemField dataFieldChange = new DataItemField
			{
				FieldName = "ChangeSetId",
				Caption = Resources.Fields.ChangeSetId,
				FieldType = Artifact.ArtifactFieldTypeEnum.Integer,
			};
			dataItem.Fields.Add(dataFieldChange.FieldName, dataFieldChange);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataFieldChange.FieldName))
			{
				if (filterList[dataFieldChange.FieldName].GetType() == typeof(LongRange))
				{
					dataFieldChange.TextValue = ((LongRange)filterList[dataFieldChange.FieldName]).ToString();
				}
				else if (filterList[dataFieldChange.FieldName].GetType() == typeof(long))
				{
					dataFieldChange.TextValue = ((long)filterList[dataFieldChange.FieldName]).ToString();
				}
			}
			#endregion

			#region Baseline ID
			//Baseline ID
			DataItemField dataFieldID = new DataItemField
			{
				FieldName = "BaselineId",
				Caption = Resources.Fields.ID,
				FieldType = Artifact.ArtifactFieldTypeEnum.Identifier,
			};
			dataItem.Fields.Add(dataFieldID.FieldName, dataFieldID);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataFieldID.FieldName))
				dataFieldID.IntValue = (int)filterList[dataFieldID.FieldName];
			#endregion
		}

		/// <summary>Gets the list of lookup values and names for a specific lookup</summary>
		/// <param name="lookupName">The name of the lookup</param>
		/// <param name="projectId">The id of the project - needed for some lookups</param>
		/// <returns>The name/value pairs</returns>
		private JsonDictionaryOfStrings GetLookupValues(string lookupName, int projectId, int projectTemplateId)
		{
			const string METHOD_NAME = CLASS_NAME + "GetLookupValues()";
			Logger.LogEnteringEvent(METHOD_NAME);

			JsonDictionaryOfStrings lookupValues = null;
			UserManager user = new UserManager();

			if (lookupName == "CreatorUserId")
			{
				List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);
				lookupValues = ConvertLookupValues(users.OfType<Entity>().ToList(), "UserId", "FullName");
			}
			else if (lookupName == "ReleaseId")
			{
				List<ReleaseView> releases = new ReleaseManager().RetrieveByProjectId(projectId, false, true, false);
				lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return lookupValues;
		}

		/// <summary>Populates a data item from a dataset datarow</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="baseline">The datarow containing the data</param>
		/// <param name="isProjectAdmin">Are we a project admin</param>
		/// <param name="canEditRow">Can the user edit this row</param>
		private void PopulateRow(int projectId, SortedDataItem dataItem, ProjectBaseline baseline, bool isProjectAdmin, bool canEditRow = false)
		{
			//Set the primary key
			dataItem.PrimaryKey = baseline.BaselineId;

			//Baselines don't have an attachment flag
			dataItem.Attachment = false;

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				//First populate the data-item from the data-row
				PopulateFieldRow(dataItem, dataItemField, baseline, null, null, false, null);

				//Apply the conditional formatting to the active flag
				if (dataItemField.FieldName == "IsActive")
				{
					dataItemField.CssClass = GlobalFunctions.GetActiveFlagCssClass(baseline.IsActive);
				}
				////Add a link for the Release.
				//else if (dataItemField.FieldName == "ReleaseId")
				//{
				//	dataItem.CustomUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, projectId, baseline.ReleaseId.Value);
				//}
				////Add a link for the ChangeSet Id.
				//else if (dataItemField.FieldName == "ChangeSetId")
				//{
				//	dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/" + projectId + "/Administration/HistoryDetails/" + baseline.ChangeSetId.ToString() + ".aspx");
				//}
			}
		}

		/// <summary>Checks that the user is actually logged in. Returns the User's ID or throws an exception.</summary>
		/// <returns></returns>
		/// <throws>FaultException</throws>
		private int checkUserLogged()
		{
			//Make sure we're authenticated
			if (!CurrentUserId.HasValue || CurrentUserId.Value < 1)
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			return CurrentUserId.Value;
		}

		/// <summary>Verifies the user is an Admin of the project.</summary>
		/// <param name="projectId">The ProjectId to check</param>
		/// <returns></returns>
		private bool checkProjectAdmin(int projectId)
		{
			bool retVal = IsAuthorized(projectId, Project.PermissionEnum.ProjectAdmin, Artifact.ArtifactTypeEnum.None) == Project.AuthorizationState.Authorized;

			return retVal;
		}
		#endregion Private Methods

		#region Not Implemented Methods
		/// <summary>Saved the items listed.</summary>
		/// <param name="projectId">The project ID.</param>
		/// <param name="dataItems">The items to save.</param>
		/// <returns>Any errors.</returns>
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

		public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			throw new NotImplementedException();
		}
		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			throw new NotImplementedException();
		}
		public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			throw new NotImplementedException();
		}
		#endregion Not Implemented Methods
	}
}
