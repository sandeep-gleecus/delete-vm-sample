using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// This service is used on the Administration 'Project History Changes', and shows 
	/// </summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class HistoryChangeSetService : SortedListServiceBase, IHistoryChangeSetService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.HistoryChangeSetService::";

		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			throw new NotImplementedException();
		}

		#region ISortedListService methods

		/// <summary>Updates the current sort stored in the system (property and direction)</summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The artifact property we want to sort on</param>
		/// <param name="sortAscending">Are we sorting ascending or not</param>
		/// <returns>Any error messages</returns>
		public string SortedList_UpdateSort(int projectId, string sortProperty, bool sortAscending, int? displayTypeId)
		{
			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//You cannot sort by signed status, so prevent
			if (sortProperty == "Signed")
			{
				return "";
			}

			//Call the base method with the appropriate settings collection
			return UpdateSort(userId, projectId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_HISTORY_ADMINSETSORT_EXPRESSION);
		}

		/// <summary>Returns the artifact history for the specific artifact</summary>
		/// <param name="userId">The user we're viewing the data as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">Contains required ArtifactType and ArtifactId filters</param>
		/// <returns>Collection of dataitems</returns>
		public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the history business object
				HistoryManager history = new HistoryManager();

				//Create the array of data items (including the first filter item)
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

				//Now get the list of populated filters and the current sort
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_HISTORY_ADMINFILTERS_LIST);
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_HISTORY_ADMINSETSORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "ChangeDate DESC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");
				sortedData.FilterNames = GetFilterNames(filterList);

				//Create the filter item first - we can clone it later
				SortedDataItem filterItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
				dataItems.Add(filterItem);

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_HISTORYSET_ADMINPAGINATION);
				paginationSettings.Restore();
				//Default values
				int paginationSize = 15;
				int currentPage = 1;
				if (paginationSettings["NumberRowsPerPage"] != null)
					paginationSize = (int)paginationSettings["NumberRowsPerPage"];
				if (paginationSettings["CurrentPage"] != null)
					currentPage = (int)paginationSettings["CurrentPage"];

				//Get the count of items
				int artifactCount = history.CountSet(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				int pageCount = (int)decimal.Ceiling(artifactCount / (decimal)paginationSize);
				if (pageCount < 1)
					pageCount = 1;
				//Make sure that the current page is not larger than the number of pages or less than 1
				if (currentPage > pageCount)
				{
					currentPage = pageCount;
					paginationSettings["CurrentPage"] = currentPage;
					paginationSettings.Save();
				}
				else if (currentPage < 1)
				{
					currentPage = 1;
					paginationSettings["CurrentPage"] = currentPage;
					paginationSettings.Save();
				}
				int startRow = ((currentPage - 1) * paginationSize) + 1;

				//Actually retrieve the list of history items
				List<HistoryChangeSetView> historyChangeSets = history.RetrieveSetsByProjectId(projectId, GlobalFunctions.GetCurrentTimezoneUtcOffset(), sortProperty, sortAscending, filterList, startRow, paginationSize);

				//Set count info..
				sortedData.TotalCount = new HistoryManager().CountSet(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				sortedData.VisibleCount = historyChangeSets.Count;

				//Display the pagination information
				sortedData.CurrPage = currentPage;
				sortedData.PageCount = pageCount;
				sortedData.StartRow = startRow;

				//Display the sort information
				sortedData.SortProperty = sortProperty;
				sortedData.SortAscending = sortAscending;

				//Get required baseline information.
				long? baselineChangeSetId = null;
				ProjectSettings projSet = new ProjectSettings(projectId);
				if (projSet.BaseliningEnabled && Common.Global.Feature_Baselines)
				{
					//baselineChangeSetId = new BaselineManager().Baseline_RetrieveForProduct(projectId)?.Select(b => b.BaselineId)?.Max();
					var baselines = new BaselineManager().Baseline_RetrieveForProduct(projectId);
					if (baselines != null && baselines.Count > 0)
						baselineChangeSetId = baselines.Select(b => b.ChangeSetId).Max();
				}

				//Iterate through all the artifact links in the pagination range and populate the dataitem
				foreach (HistoryChangeSetView historyRow in historyChangeSets)
				{
					SortedDataItem dataItem = filterItem.Clone();

					//Now populate with the data
					PopulateRow(projectId, dataItem, historyRow, baselineChangeSetId);
					dataItems.Add(dataItem);
				}

				//Also include the pagination info
				sortedData.PaginationOptions = RetrievePaginationOptions(projectId);

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
				if (!CurrentUserId.HasValue)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
				}
				int userId = CurrentUserId.Value;

				string tooltip = HistoryActivityService.RetrieveArtifactNameDesc(userId, changeSetId);

				Logger.LogExitingEvent(METHOD_NAME);
				return tooltip;
			}
			catch (ArtifactNotExistsException)
			{
				//Ignore artifact not found, it may have been just deleted
				return "";
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
			const string METHOD_NAME = "RetrievePaginationOptions";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Delegate to the generic method in the base class - passing the correct collection name
			JsonDictionaryOfStrings paginationDictionary = RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_HISTORYSET_ADMINPAGINATION);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return paginationDictionary;
		}

		/// <summary>Updates the size of pages returned and the currently selected page</summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
		/// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
		public void UpdatePagination(int projectId, int pageSize, int currentPage)
		{
			const string METHOD_NAME = "UpdatePagination";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the pagination settings collection and update
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_HISTORYSET_ADMINPAGINATION);
				paginationSettings.Restore();
				if (pageSize != -1)
					paginationSettings["NumberRowsPerPage"] = pageSize;
				if (currentPage != -1)
					paginationSettings["CurrentPage"] = currentPage;
				paginationSettings.Save();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
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
			const string METHOD_NAME = "UpdateFilters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the current filters from session
				ProjectSettingsCollection savedFilters = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_HISTORY_ADMINFILTERS_LIST);
				int oldFilterCount = savedFilters.Count;
				savedFilters.Clear(); //Clear the filters

				//Iterate through the filters, updating the project collection
				foreach (KeyValuePair<string, string> filter in filters)
				{
					string filterName = filter.Key;
					//Now get the type of field that we have. Since history items are not a true artifact,
					//these values have to be hardcoded, as they're not stored in the TST_ARTIFACT_FIELD table
					Artifact.ArtifactFieldTypeEnum artifactFieldType = ArtifactFieldTypeEnum.Text;
					switch (filterName)
					{
						case "ChangeSetId":
							artifactFieldType = ArtifactFieldTypeEnum.Identifier;
							break;

						case "ProjectId":
						case "ArtifactId":
							artifactFieldType = ArtifactFieldTypeEnum.Integer;
							break;

						case "ChangeDate":
							artifactFieldType = ArtifactFieldTypeEnum.DateTime;
							break;

						case "ArtifactTypeId":
						case "ChangeTypeId":
						case "UserId":
						case "Signed":
							artifactFieldType = ArtifactFieldTypeEnum.Lookup;
							break;
					}

					if (artifactFieldType == ArtifactFieldTypeEnum.Lookup)
					{
						//Need to make sure that they are MultiValueFilter classes
						MultiValueFilter multiValueFilter;
						if (MultiValueFilter.TryParse(filter.Value, out multiValueFilter))
						{
							savedFilters.Add(filterName, multiValueFilter);
						}
					}
					else if (artifactFieldType == ArtifactFieldTypeEnum.Identifier)
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
					else if (artifactFieldType == ArtifactFieldTypeEnum.DateTime)
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
					else if (artifactFieldType == ArtifactFieldTypeEnum.Integer)
					{
						//If we have integer values, need to make sure that they are indeed integral
						int intValue;
						if (!int.TryParse(filter.Value, out intValue))
						{
							return string.Format(Resources.Messages.Global_EnterValidIntegerValue, filterName);
						}
						savedFilters.Add(filterName, intValue);
					}
					else if (artifactFieldType == ArtifactFieldTypeEnum.Text || artifactFieldType == ArtifactFieldTypeEnum.NameDescription)
					{
						//For text, just save the value
						savedFilters.Add(filterName, filter.Value);
					}
				}
				savedFilters.Save();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return "";  //Success
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region Private methods

		/// <summary>Populates the 'shape' of the data item that will be used as a template for the retrieved data items</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="userId">The user we're viewing the artifact links as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList)
		{
			const string METHOD_NAME = CLASS_NAME + "PopulateShape(int,int,dataitem,hashtable)";

			//We need to add the various artifact link fields to be displayed
			#region ChangeSet ID
			DataItemField dataItemField = new DataItemField();
			dataItemField.FieldName = "ChangeSetId";
			dataItemField.FieldType = ArtifactFieldTypeEnum.NameDescription;
			dataItemField.Caption = Resources.Fields.ChangeSetId;
			dataItemField.AllowDragAndDrop = false;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				if (filterList[dataItemField.FieldName].GetType() == typeof(int))
				{
					dataItemField.IntValue = (int)filterList[dataItemField.FieldName];
					dataItemField.TextValue = dataItemField.IntValue.ToString();
				}
				else if (filterList[dataItemField.FieldName].GetType() == typeof(string))
				{
					try
					{
						dataItemField.IntValue = int.Parse((string)filterList[dataItemField.FieldName]);
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD_NAME, ex);
						dataItemField.IntValue = -1;
					}
					dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
				}
			}
			#endregion

			#region Change Date
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ChangeDate";
			dataItemField.FieldType = ArtifactFieldTypeEnum.DateTime;
			dataItemField.Caption = Resources.Fields.ChangeDate;
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
					textValue += string.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.StartDate.Value);
				}
				textValue += "|";
				if (dateRange.EndDate.HasValue)
				{
					textValue += string.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.EndDate.Value);
				}
				dataItemField.TextValue = textValue;
			}
			#endregion

			#region ChangedBy
			dataItemField = new DataItemField();
			dataItemField.FieldName = "UserId";
			dataItemField.Caption = Resources.Fields.ChangerId;
			dataItemField.FieldType = ArtifactFieldTypeEnum.Lookup;
			dataItemField.LookupName = "UserName";
			dataItemField.AllowDragAndDrop = true;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the list of possible lookup values
			dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
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

			#region ArtifactTypeId
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ArtifactTypeId";
			dataItemField.FieldType = ArtifactFieldTypeEnum.Lookup;
			dataItemField.LookupName = "ArtifactTypeName";
			dataItemField.Caption = Resources.Fields.ArtifactTypeId;
			dataItemField.AllowDragAndDrop = false;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the list of possible lookup values
			dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
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

			#region Artifact ID
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ArtifactId";
			dataItemField.FieldType = ArtifactFieldTypeEnum.Identifier;
			dataItemField.Caption = Resources.Fields.ArtifactId;
			dataItemField.AllowDragAndDrop = false;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
				if (filterList[dataItemField.FieldName] is int)
					dataItemField.IntValue = (int)filterList[dataItemField.FieldName];
				else
					dataItemField.IntValue = int.Parse((string)filterList[dataItemField.FieldName]);
			#endregion

			#region Artifact Name
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ArtifactDesc";
			dataItemField.FieldType = ArtifactFieldTypeEnum.NameDescription;
			dataItemField.Caption = Resources.Fields.ArtifactName;
			dataItemField.AllowDragAndDrop = false;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
				dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
			#endregion

			#region ChangeType ID (Name)
			dataItemField = new DataItemField();
			dataItemField.FieldName = "ChangeTypeId";
			dataItemField.FieldType = ArtifactFieldTypeEnum.Lookup;
			dataItemField.LookupName = "ChangeTypeName";
			dataItemField.Caption = Resources.Fields.ChangeType;
			dataItemField.AllowDragAndDrop = false;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the list of possible lookup values
			dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
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

			#region Signature

			dataItemField = new DataItemField();
			dataItemField.FieldName = "Signed";
			dataItemField.FieldType = ArtifactFieldTypeEnum.Lookup;
			dataItemField.LookupName = "SignedName";
			dataItemField.Caption = Resources.Fields.Signed;
			dataItemField.AllowDragAndDrop = false;
			dataItemField.NotSortable = true;
			//Set the list of possible lookup values
			dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
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
		}

		/// <summary>Populates a data item from a dataset datarow</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="historyChangeSet">The datarow containing the data</param>
		protected void PopulateRow(int projectId, SortedDataItem dataItem, HistoryChangeSetView historyChangeSet, long? baselineChangeSetId)
		{
			//Set the primary key
			dataItem.PrimaryKey = (int)historyChangeSet.ChangeSetId;

			//History items don't have an attachment flag
			dataItem.Attachment = false;

			//Depending on the changeset type, enable/disable the checkboxes.
			switch (historyChangeSet.ChangeTypeId)
			{
				case (int)HistoryManager.ChangeSetTypeEnum.Added:
				case (int)HistoryManager.ChangeSetTypeEnum.Rollback:
				case (int)HistoryManager.ChangeSetTypeEnum.Undelete:
				case (int)HistoryManager.ChangeSetTypeEnum.Purged:
				case (int)HistoryManager.ChangeSetTypeEnum.Imported:
				case (int)HistoryManager.ChangeSetTypeEnum.Exported:
				case (int)HistoryManager.ChangeSetTypeEnum.Association_Add:
				case (int)HistoryManager.ChangeSetTypeEnum.Association_Remove:
					dataItem.PrimaryKey *= -1;
					break;
			}

			//If it is a Project artifact type, we need to disable the checkbox. 
			if (historyChangeSet.ArtifactTypeId == (int)ArtifactTypeEnum.Project && dataItem.PrimaryKey > 0)
				dataItem.PrimaryKey *= -1;

			//Disable the checkboxes if the change is WITHIN our baseline, 
			if (baselineChangeSetId.HasValue && dataItem.PrimaryKey > 0 && historyChangeSet.ChangeSetId <= baselineChangeSetId.Value)
			{
				//If baselines are enabled ('baselineChangeSetId' != null) and we have not already negativised the ID,
				//  and this changeset is higher than the values in 'baselineChangeSetId', then we negativise the value.
				dataItem.PrimaryKey = dataItem.PrimaryKey * -1;
			}


			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (historyChangeSet.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the data-row
					if (dataItemField.FieldName == "ChangeSetId")
						dataItemField.TextValue = "#" + historyChangeSet.ChangeSetId.ToString();
					else if (dataItemField.FieldName == "ArtifactId")
					{
						//HACK: [TK:2682] When project, don't display Project #.
						if (historyChangeSet.ArtifactTypeId != (int)ArtifactTypeEnum.Project)
						{
							dataItemField.TextValue = historyChangeSet.ArtifactId.ToSafeString();
						}
					}
					else if (dataItemField.FieldName == "ArtifactTypeId" && historyChangeSet.ArtifactTypeId == (int)ArtifactTypeEnum.Project)
					{
						//HACK: [TK:2682] When project, display 'Product' instead of database value 'product'. DB not updated at this point.
						dataItemField.TextValue = Resources.Fields.Project;
					}
					else if (dataItemField.FieldName == "Signed")
					{
						if (historyChangeSet.Signed == (int)HistoryChangeSetView.SignedStatusEnum.Valid)
						{
							dataItemField.TextValue = Resources.Fields.Signature_Valid;
							dataItemField.CssClass = "ExecutionStatusPassed";
						}
						else if (historyChangeSet.Signed == (int)HistoryChangeSetView.SignedStatusEnum.Invalid)
						{
							dataItemField.TextValue = Resources.Fields.Signature_Invalid;
							dataItemField.CssClass = "ExecutionStatusFailed";
						}
						else
						{
							dataItemField.TextValue = Resources.Fields.Signature_NotSigned;
						}
					}
					else
						PopulateFieldRow(dataItem, dataItemField, historyChangeSet, null, null, false, null);

					//There are currently no editable fields
					dataItemField.Editable = false;
					dataItemField.Required = false;
				}
			}
		}

		/// <summary>Gets the list of lookup values and names for a specific lookup</summary>
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
				UserManager user = new UserManager();

				if (lookupName == "UserId")
				{
					List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);
					lookupValues = ConvertLookupValues(users.OfType<Entity>().ToList(), "UserId", "FullName");
				}
				else if (lookupName == "ChangeTypeId")
				{
					//Generate a list from the enumeration.
					SortedList<int, string> lstTypes = new SortedList<int, string>();
					foreach (HistoryManager.ChangeSetTypeEnum item in Enum.GetValues(typeof(HistoryManager.ChangeSetTypeEnum)))
						lstTypes.Add((int)item, item.ToString().Replace("_", " "));
					lookupValues = ConvertLookupValues(lstTypes);
				}
				else if (lookupName == "ArtifactTypeId")
				{
					ArtifactManager aMgr = new ArtifactManager();
					List<ArtifactType> artTypes = aMgr.ArtifactType_RetrieveAll();
					var listArt = artTypes.OfType<Entity>().ToList();

					//HACK: [TK:2682] Get the Project artifact, and force-rename it to 'program. Per [TK:2682]
					var projArt = aMgr.ArtifactType_RetrieveById(ArtifactTypeEnum.Project);
					projArt.Name = Resources.Fields.Project;
					listArt.Add(projArt);
					lookupValues = ConvertLookupValues(listArt, "ArtifactTypeId", "Name");
				}
				else if (lookupName == "Signed")
				{
					//Generate a list from the enumeration.
					SortedList<int, string> lstSignedStatus = new SortedList<int, string>();
					lstSignedStatus.Add((int)HistoryChangeSetView.SignedStatusEnum.NotSigned, Resources.Fields.Signature_NotSigned);
					lstSignedStatus.Add((int)HistoryChangeSetView.SignedStatusEnum.Valid, Resources.Fields.Signature_Valid);
					lstSignedStatus.Add((int)HistoryChangeSetView.SignedStatusEnum.Invalid, Resources.Fields.Signature_Invalid);
					lookupValues = ConvertLookupValues(lstSignedStatus);
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
