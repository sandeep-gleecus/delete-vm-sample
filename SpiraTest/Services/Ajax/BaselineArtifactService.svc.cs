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
	/// <summary>This service handles calls where </summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class BaselineArtifactService : SortedListServiceBase, IBaselineArtifactService
	{
		private const string CLASS_NAME = "Web.Services.Ajax.BaselineArtifactService::";

		#region ISortedListService Methods
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
			Hashtable filterList = GetProjectSettings(
				userId,
				projectId,
				GlobalFunctions.PROJECT_SETTINGS_BASELINEARTIFACT_FILTERS_LIST);
			string sortCommand = GetProjectSetting(
				userId,
				projectId,
				GlobalFunctions.PROJECT_SETTINGS_BASELINEARTIFACT_SORT_EXPRESSION,
				GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION,
                "ChangeDate DESC");
			string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
			string sortDirectionString = sortCommand
				.Substring(
					sortCommand.IndexOf(" "),
					sortCommand.Length - sortCommand.IndexOf(" "))
				.Trim();
			bool sortAscending = (sortDirectionString == "ASC");
			sortedData.FilterNames = GetFilterNames(filterList);

			//Create the filter item first - we can clone it later
			SortedDataItem filterItem = new SortedDataItem();
			PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
			sortedData.Items.Add(filterItem);

			//Now get the pagination information
			ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_BASELINEARTIFACT_PAGINATION);
			paginationSettings.Restore();
			//Default values
			int paginationSize = 15;
			int currentPage = 1;
			if (paginationSettings["NumberRowsPerPage"] != null)
				paginationSize = (int)paginationSettings["NumberRowsPerPage"];
			if (paginationSettings["CurrentPage"] != null)
				currentPage = (int)paginationSettings["CurrentPage"];

			//Pull the static filters out of the filter object.
			int latestBaseline = (int)GlobalFunctions.DeSerializeValue(standardFilters["baselineLatest"]);

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

            //Actually retrieve the list of baselines
            int count;
			List<HistoryChangeSetNetChangeSquashed> artifacts = baselineManager.Artifacts_ChangedBetweenChangesets(projectId, latestBaseline, sortFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out count);

			foreach (HistoryChangeSetNetChangeSquashed artifact in artifacts)
			{
				SortedDataItem dataItem = filterItem.Clone();

				//Now populate with the data, unless it is a project settings change (which we don't display)
				if (artifact.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.Project)
				{
					PopulateRow(projectId, dataItem, artifact, isProjectAdmin);
				}
				sortedData.Items.Add(dataItem);
			}
			sortedData.VisibleCount = artifacts.Count();
            sortedData.TotalCount = count;
            sortedData.PageCount = (int)Math.Ceiling((decimal)count / (decimal)paginationSize);

			Logger.LogExitingEvent(METHOD_NAME);
			return sortedData;
		}

		/// <summary>Saves the user's Sorting order.</summary>
		/// <param name="productId"></param>
		/// <param name="sortProperty"></param>
		/// <param name="sortAscending"></param>
		/// <param name="displayTypeId"></param>
		public string SortedList_UpdateSort(int productId, string sortProperty, bool sortAscending, int? displayTypeId)
		{
			//Make sure we're authenticated
			int userId = checkUserLogged();

			//Make sure we're authorized
			bool isProjectAdmin = checkProjectAdmin(productId);
			if (!isProjectAdmin)
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			//Call the base method with the appropriate settings collection
			return UpdateSort(userId, productId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_BASELINEARTIFACT_SORT_EXPRESSION);
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
			//TODO: Use the 'displayType' property for the artifact ID.
			// so we can return the artifact's *current* description.
			return "";
		}

		/// <summary>Returns a list of pagination options that the user can choose from</summary>
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
			JsonDictionaryOfStrings paginationDictionary = RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_BASELINEARTIFACT_PAGINATION);

			Logger.LogExitingEvent(METHOD_NAME);
			return paginationDictionary;
		}

		/// <summary>Updates the size of pages returned and the currently selected page</summary>
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
			ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_BASELINEARTIFACT_PAGINATION);
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
		/// <summary>Updates the filters stored in the system</summary>
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


			//Get the name..
			string settingsCollection = GlobalFunctions.PROJECT_SETTINGS_BASELINEARTIFACT_FILTERS_LIST;
			return UpdateFilters(userId, projectId, filters, settingsCollection, Artifact.ArtifactTypeEnum.None);

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
            //Artifact Name
            DataItemField fieldName = new DataItemField
            {
                FieldName = "ArtifactName",
                FieldType = Artifact.ArtifactFieldTypeEnum.NameDescription,
                Caption = Resources.Fields.Name
            };
            dataItem.Fields.Add(fieldName.FieldName, fieldName);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(fieldName.FieldName))
            {
                fieldName.TextValue = (string)filterList[fieldName.FieldName];
            }

            #endregion

            #region Artifact Type
            //Artifact type
            DataItemField fieldArtType = new DataItemField
			{
				FieldName = "ArtifactTypeId",
				Caption = Resources.Fields.ArtifactTypeId,
				FieldType = Artifact.ArtifactFieldTypeEnum.Lookup,
				LookupName = "ArtifactTypeName",
			};
			dataItem.Fields.Add(fieldArtType.FieldName, fieldArtType);
			//Set the list of possible lookup values
			fieldArtType.Lookups = GetLookupValues(fieldArtType.FieldName, projectId, projectTemplateId);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(fieldArtType.FieldName))
			{
				if (filterList[fieldArtType.FieldName] is int)
				{
					fieldArtType.TextValue = ((int)filterList[fieldArtType.FieldName]).ToString();
				}
				if (filterList[fieldArtType.FieldName] is MultiValueFilter)
				{
					fieldArtType.TextValue = ((MultiValueFilter)filterList[fieldArtType.FieldName]).ToString();
				}
			}
			#endregion Artifact Type

			#region Artifact ID
			DataItemField fieldArtId = new DataItemField
			{
				FieldName = "ChangedArtifactId",
				Caption = Resources.Fields.ArtifactId,
				FieldType = Artifact.ArtifactFieldTypeEnum.Identifier,
			};
			dataItem.Fields.Add(fieldArtId.FieldName, fieldArtId);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(fieldArtId.FieldName))
			{
				fieldArtId.IntValue = (int)filterList[fieldArtId.FieldName];
			}
			#endregion Artifact ID

			#region Last Changer
			DataItemField fieldWho = new DataItemField
			{
				FieldName = "UserId",
				Caption = Resources.Fields.UserName,
				FieldType = Artifact.ArtifactFieldTypeEnum.Lookup,
				LookupName = "UserFullName"
			};
			dataItem.Fields.Add(fieldWho.FieldName, fieldWho);
			//Set the list of possible lookup values
			fieldWho.Lookups = GetLookupValues(fieldWho.FieldName, projectId, projectTemplateId);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(fieldWho.FieldName))
			{
				if (filterList[fieldWho.FieldName] is int)
				{
					fieldWho.TextValue = ((int)filterList[fieldWho.FieldName]).ToString();
				}
				if (filterList[fieldWho.FieldName] is MultiValueFilter)
				{
					fieldWho.TextValue = ((MultiValueFilter)filterList[fieldWho.FieldName]).ToString();
				}
			}
			#endregion Last Changer

			#region Last Change Date
			DataItemField fieldDate = new DataItemField
			{
				FieldName = "ChangeDate",
				Caption = Resources.Fields.LastModified,
				FieldType = Artifact.ArtifactFieldTypeEnum.DateTime,
			};
			dataItem.Fields.Add(fieldDate.FieldName, fieldDate);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(fieldDate.FieldName))
			{
				//Need to convert into the displayable date form
				DateRange dateRange = (DateRange)filterList[fieldDate.FieldName];
				string textValue = "";
				if (dateRange.StartDate.HasValue)
				{
					textValue += string.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.StartDate.Value);
				}
				textValue += "|";
				if (dateRange.EndDate.HasValue)
				{
					textValue += string.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.EndDate.Value);
				}
				fieldDate.TextValue = textValue;
			}
			#endregion Last Change Date

			#region Last Change
			DataItemField fieldChngType = new DataItemField
			{
				FieldName = "ChangeTypes",
				Caption = Resources.Fields.ChangeType,
				FieldType = Artifact.ArtifactFieldTypeEnum.Text,
			};
			dataItem.Fields.Add(fieldChngType.FieldName, fieldChngType);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(fieldChngType.FieldName))
			{
				fieldChngType.TextValue = (string)filterList[fieldChngType.FieldName];
			}
			#endregion Last Change
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

			switch (lookupName)
			{
				case "ArtifactTypeId":
					{
						List<ArtifactType> artTypes = new ArtifactManager().ArtifactType_RetrieveAll();
						lookupValues = ConvertLookupValues(artTypes.OfType<Entity>().ToList(), "ArtifactTypeId", "Name");
					}
					break;

				case "UserId":
					{
						List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);
						lookupValues = ConvertLookupValues(users.OfType<Entity>().ToList(), "UserId", "FullName");
					}
					break;
			}

			//TODO: Rewrite lookups.
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
        /// <param name="historyChangeSetNetChange">The net changes for a specific artifact</param>
		private void PopulateRow(
			int projectId,
			SortedDataItem dataItem,
			HistoryChangeSetNetChangeSquashed historyChangeSetNetChange,
			bool isProjectAdmin,
			bool canEditRow = false)
		{
			//Set the primary key and other static items, first.
			dataItem.PrimaryKey = (historyChangeSetNetChange.ArtifactTypeId * 10000000) + historyChangeSetNetChange.ArtifactId; //TODO: This above may cause issues  for anyone that has over 10 million of a single artifact, install-wide.
			dataItem.Attachment = false;
			dataItem.ReadOnly = true;

			//Link to the BaselineArtifactDetails page, unless this is a project settings change
			dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveProjectAdminUrl(projectId, "BaselineDetails/" + historyChangeSetNetChange.BaselineId + "/" + historyChangeSetNetChange.ArtifactTypeId + "/" + historyChangeSetNetChange.ArtifactId));

            //Populate the row dataitems.
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;

				//First populate the data-item from the data-row
				PopulateFieldRow(
					dataItem,
					dataItemField,
					historyChangeSetNetChange,
					null,
					null,
					false,
					null);

				//Set the field's properties..
				dataItemField.Editable = false;
				dataItemField.Required = false;
				dataItemField.Hidden = false;
				dataItemField.NotSortable = false;

                //If we have the name/desc field then we need to set the image to the appropriate artifact type
                //which is passed in the tooltip field
                if (dataItemField.FieldName == "ArtifactName")
                {
                    dataItemField.Tooltip = GlobalFunctions.GetIconForArtifactType((DataModel.Artifact.ArtifactTypeEnum)historyChangeSetNetChange.ArtifactTypeId);
                }
                
                //If this is the ID field, make a hyperlink
                if (dataItemField.FieldName == "ChangedArtifactId")
                {
                    dataItemField.Tooltip = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)historyChangeSetNetChange.ArtifactTypeId, projectId, historyChangeSetNetChange.ArtifactId));
                }
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
		public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
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
