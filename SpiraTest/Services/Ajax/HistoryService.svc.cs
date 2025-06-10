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

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>Provides the web service used to interacting with the various client-side artifact association AJAX components</summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class HistoryService : SortedListServiceBase, IHistoryService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.HistoryService::";

		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			throw new NotImplementedException();
		}

		#region IHistoryService methods

		/// <summary>
		/// Counts the number of history items
		/// </summary>
		/// <param name="projectId">The project id</param>
		/// <param name="artifact">The artifact we want history for</param>
		/// <returns>The count</returns>
		public int History_Count(int projectId, ArtifactReference artifact)
		{
			const string METHOD_NAME = "History_Count";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited OK because we need it for the counts)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (Artifact.ArtifactTypeEnum)artifact.ArtifactTypeId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//See if we have any history items so that we know to display the 'has-data' flag
				HistoryManager history = new HistoryManager();
				int count = history.Count(projectId, artifact.ArtifactId, (Artifact.ArtifactTypeEnum)artifact.ArtifactTypeId, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				return count;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

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

			//Call the base method with the appropriate settings collection
			return base.UpdateSort(userId, projectId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_HISTORY_SORT_EXPRESSION);
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

			try
			{
				//See if we need to include the use case / test step / risk mitigation details..
				bool includeStepHistory = false;
				if (standardFilters.ContainsKey("IncludeSteps"))
				{
					includeStepHistory = (bool)GlobalFunctions.DeSerializeValue(standardFilters["IncludeSteps"]);
					standardFilters.Remove("IncludeSteps");
				}

				//Get the artifact type and id from the filters
				int changesetId = -1;
				int artifactTypeId = -1;
				int artifactId = -1;
				int baselineLatestId = -1;
				int? baselinePreviousId = null;
				bool isProjectAdmin = false;

				//See which display mode we have
				if (displayTypeId == (int)Artifact.DisplayTypeEnum.Baseline_ArtifactChanges)
				{
					artifactTypeId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ArtifactType"]);
					artifactId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ArtifactId"]);
					baselineLatestId = (int)GlobalFunctions.DeSerializeValue(standardFilters["BaselineLatestId"]);
					if (standardFilters.ContainsKey("BaselinePreviousId"))
					{
						baselinePreviousId = (int)GlobalFunctions.DeSerializeValue(standardFilters["BaselinePreviousId"]);
					}

					//Include step history if appropriate
					includeStepHistory = true;
				}
				else
				{
					if (standardFilters.ContainsKey("ChangeSetId"))
					{
						changesetId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ChangeSetId"]);

						//Make sure we're authorized for the project
						Project.AuthorizationState authorizationState = IsAuthorized(projectId);
						if (authorizationState == Project.AuthorizationState.Prohibited)
						{
							throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
						}
					}
					else
					{
						artifactTypeId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ArtifactType"]);
						artifactId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ArtifactId"]);
						if (standardFilters["IsProjectAdmin"] != null)
						{
							isProjectAdmin = (bool)GlobalFunctions.DeSerializeValue(standardFilters["IsProjectAdmin"]);
						}

						//Make sure that we're authorized for this specific artifact type
						Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (Artifact.ArtifactTypeEnum)artifactTypeId);
						if (authorizationState == Project.AuthorizationState.Prohibited)
						{
							throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
						}

						//If we don't have permission to view test steps and this is for a test case, hide the steps
						if (artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestCase && IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestStep) != Project.AuthorizationState.Authorized)
						{
							includeStepHistory = false;
						}
					}
				}

				//Instantiate the business objects
				HistoryManager historyManager = new HistoryManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Create the array of data items (including the first filter item)
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

				//Now get the list of populated filters and the current sort
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_HISTORY_FILTERS_LIST);
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_HISTORY_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "ChangeDate DESC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//If we are looking for any changes between two baselines, then we need to retrieve the baselines
				//and add those to the changeset range filter
				if (displayTypeId == (int)Artifact.DisplayTypeEnum.Baseline_ArtifactChanges)
				{
					LongRange changeSetFilter = new LongRange();
					BaselineManager baselineManager = new BaselineManager();
					ProjectBaseline currentBaseline = baselineManager.Baseline_RetrieveById(baselineLatestId);
					if (currentBaseline != null)
					{
						changeSetFilter.MaxValue = currentBaseline.ChangeSetId;
					}
					if (baselinePreviousId.HasValue)
					{
						ProjectBaseline previousBaseline = baselineManager.Baseline_RetrieveById(baselinePreviousId.Value);
						if (previousBaseline != null)
						{
							changeSetFilter.MinValue = previousBaseline.ChangeSetId;
						}
					}
					filterList["ChangeSetId"] = changeSetFilter;
				}
				sortedData.FilterNames = GetFilterNames(filterList);

				//Create the filter item first - we can clone it later
				SortedDataItem filterItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList, (changesetId < 1), isProjectAdmin);
				dataItems.Add(filterItem);

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_HISTORY_PAGINATION);
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

				//Get the count of items
				int artifactCount = historyManager.Count(projectId, artifactId, (Artifact.ArtifactTypeEnum)artifactTypeId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), includeStepHistory);
				int pageCount = (int)decimal.Ceiling(artifactCount / (decimal)paginationSize);
				if (pageCount == 0) pageCount = 1;
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
				int startRow = ((currentPage - 1) * paginationSize) + 1;

				//Actually retrieve the list of history items
				List<HistoryChangeView> historyChangeDetails = null;
				List<Component> components = null;
				if (changesetId > 0)
				{
					historyChangeDetails = historyManager.RetrieveByChangeSetId(projectId, changesetId, sortProperty, sortAscending, filterList, startRow, paginationSize, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				}
				else
				{
					//Note: We only care about Requirement/TestCase/Risk changes here.
					historyChangeDetails = historyManager.RetrieveByArtifactId(projectId, artifactId, (Artifact.ArtifactTypeEnum)artifactTypeId, sortProperty, sortAscending, filterList, startRow, paginationSize, GlobalFunctions.GetCurrentTimezoneUtcOffset(), includeStepHistory);

					//If we are retreiving Incident/TestCase history, need to also get the list of components since the display names are not part of the dataset
					if ((artifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident || artifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase) && historyChangeDetails.Count > 0)
					{
						components = new ComponentManager().Component_Retrieve(projectId);
					}
				}

				//If we have at least one row, we need to get the custom property lists to resolve any list custom property values
				List<CustomPropertyList> customLists = null;
				if (historyChangeDetails.Count > 0)
				{
					customLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(projectTemplateId, true);
				}

				//Display the pagination information
				sortedData.CurrPage = currentPage;
				sortedData.PageCount = pageCount;
				sortedData.StartRow = startRow;

				//Display the sort information
				sortedData.SortProperty = sortProperty;
				sortedData.SortAscending = sortAscending;

				//Display the visible and total count of artifacts
				sortedData.VisibleCount = historyChangeDetails.Count;
				sortedData.TotalCount = artifactCount;

				//Iterate through all the artifact links in the pagination range and populate the dataitem
				foreach (HistoryChangeView historyRow in historyChangeDetails)
				{
					SortedDataItem dataItem = filterItem.Clone();

					//Now populate with the data
					PopulateRow(projectId, dataItem, historyRow, isProjectAdmin, customLists, components, includeStepHistory);
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
		/// Returns a list of pagination options that the user can choose from
		/// </summary>
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
			JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_HISTORY_PAGINATION);

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
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_HISTORY_PAGINATION);
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

		#endregion

		#region IFilteredListService Methods

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
				ProjectSettingsCollection savedFilters = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_HISTORY_FILTERS_LIST);
				int oldFilterCount = savedFilters.Count;
				savedFilters.Clear(); //Clear the filters

				//Iterate through the filters, updating the project collection
				foreach (KeyValuePair<string, string> filter in filters)
				{
					string filterName = filter.Key;
					//Now get the type of field that we have. Since history items are not a true artifact,
					//these values have to be hardcoded, as they're not stored in the TST_ARTIFACT_FIELD table
					Artifact.ArtifactFieldTypeEnum artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
					switch (filterName)
					{
						case "ChangeDate":
							artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
							break;
						case "ChangerId":
							artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
							break;
						case "OldValue":
						case "NewValue":
						case "FieldName":
							artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
							break;
						case "ArtifactHistoryId":
							artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
							break;
						case "ChangeSetId":
							//Technically it is a long not int
							artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
							break;
					}

					if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Lookup)
					{
						//Need to make sure that they are MultiValueFilter classes
						MultiValueFilter multiValueFilter;
						if (MultiValueFilter.TryParse(filter.Value, out multiValueFilter))
						{
							savedFilters.Add(filterName, multiValueFilter);
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
							return String.Format(Resources.Messages.Global_EnterValidIntegerValue, filterName);
						}
					}
					if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.DateTime)
					{
						//If we have date values, need to make sure that they are indeed dates
						//Otherwise we need to throw back a friendly error message
						DateRange dateRange;
						if (!DateRange.TryParse(filter.Value, out dateRange))
						{
							return String.Format(Resources.Messages.Global_EnterValidDateRangeValue, filterName);
						}
						savedFilters.Add(filterName, dateRange);
					}
					if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Integer)
					{
						//If we have integer values, need to make sure that they are indeed integral
						LongRange longRange;
						if (!LongRange.TryParse(filter.Value, out longRange))
						{
							return String.Format(Resources.Messages.Global_EnterValidIntegerValue, filterName);
						}
						savedFilters.Add(filterName, longRange);
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

		#endregion

		#region Private methods

		/// <summary>Populates the 'shape' of the data item that will be used as a template for the retrieved data items</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="userId">The user we're viewing the artifact links as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		/// <param name="changeSetDisplay">Whether to display changeset information or not. Default: TRUE</param>
		/// <param name="changeSetIsLink">Should the changeset id be displayed as a link. Default: FALSE</param>
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool changeSetDisplay = true, bool changeSetIsLink = false)
		{
			//We need to add the various artifact link fields to be displayed
			DataItemField dataItemField = null;

			#region ChangeSet ID
			if (changeSetDisplay)
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ChangeSetId";
				dataItemField.FieldType = (changeSetIsLink) ? DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription : DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
				dataItemField.Caption = Resources.Fields.ChangeSetId;
				dataItemField.AllowDragAndDrop = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					if (filterList[dataItemField.FieldName] is String)
					{
						if (dataItemField.FieldType == Artifact.ArtifactFieldTypeEnum.NameDescription)
						{
							dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
						}
						if (dataItemField.FieldType == Artifact.ArtifactFieldTypeEnum.Integer)
						{
							dataItemField.IntValue = Int32.Parse((string)filterList[dataItemField.FieldName]);
						}
					}
					if (filterList[dataItemField.FieldName] is LongRange)
					{
						dataItemField.TextValue = ((LongRange)filterList[dataItemField.FieldName]).ToString();
					}
				}
			}
			#endregion

			#region Change date
			if (changeSetDisplay)
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ChangeDate";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
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
						textValue += String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.StartDate.Value);
					}
					textValue += "|";
					if (dateRange.EndDate.HasValue)
					{
						textValue += String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.EndDate.Value);
					}
					dataItemField.TextValue = textValue;
				}
			}
			#endregion

			#region Field Name
			dataItemField = new DataItemField();
			dataItemField.FieldName = "FieldName";
			dataItemField.Caption = Resources.Fields.FieldName;
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
			dataItemField.AllowDragAndDrop = true;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
			}
			#endregion

			#region Old Value
			dataItemField = new DataItemField();
			dataItemField.FieldName = "OldValue";
			dataItemField.Caption = Resources.Fields.OldValue;
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
			dataItemField.AllowDragAndDrop = true;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
			}
			#endregion

			#region New Value
			dataItemField = new DataItemField();
			dataItemField.FieldName = "NewValue";
			dataItemField.Caption = Resources.Fields.NewValue;
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
			dataItemField.AllowDragAndDrop = true;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
			}
			#endregion

			#region Changed By
			if (changeSetDisplay)
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ChangerId";
				dataItemField.Caption = Resources.Fields.ChangerId;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "ChangerName";
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
			}
			#endregion

			#region Change Type
			if (changeSetDisplay)
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ChangeSetTypeId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "ChangeSetTypeName";
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
						dataItemField.IntValue = ((int)filterList[dataItemField.FieldName]);
					}
					if (filterList[dataItemField.FieldName] is string)
					{
						MultiValueFilter mvf;
						if (MultiValueFilter.TryParse((string)filterList[dataItemField.FieldName], out mvf))
						{
							dataItemField.TextValue = mvf.ToString();
						}
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
		/// <param name="historyChangeDetail">The datarow containing the data</param>
		/// <param name="isProjectAdmin">Are we a project admin</param>
		/// <param name="customLists">Any custom property lists defined (used for displaying the names of list values)</param>
		protected void PopulateRow(int projectId, SortedDataItem dataItem, HistoryChangeView historyChangeDetail, bool isProjectAdmin, List<CustomPropertyList> customLists, List<Component> components, bool includeStepHistory)
		{
			//Set the primary key
			dataItem.PrimaryKey = (int)historyChangeDetail.ArtifactHistoryId;

			//History items don't have an attachment flag
			dataItem.Attachment = false;

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (historyChangeDetail[dataItemField.FieldName] != null)
				{
					//First populate the data-item from the data-row
					if (dataItemField.FieldName == "FieldName")
					{
						//The field name needs to be localized (if possible)
						string localizedFieldName = Resources.Fields.ResourceManager.GetString(historyChangeDetail.FieldName);
						if (String.IsNullOrWhiteSpace(localizedFieldName))
						{
							//We use the caption instead
							dataItemField.TextValue = historyChangeDetail.FieldCaption;
						}
						else
						{
							dataItemField.TextValue = localizedFieldName;
						}

						//Add the 'Step' prefix if we have to include step changes (test step or use case step)
						if (includeStepHistory && (historyChangeDetail.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestStep || historyChangeDetail.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.RequirementStep))
						{
							dataItemField.TextValue = "[" + Resources.Main.Global_Step + " " + GlobalFunctions.ARTIFACT_PREFIX_TEST_STEP + historyChangeDetail.ArtifactId + "] " + dataItemField.TextValue;
						}
						//Add the 'Mitigation' prefix if we have to include mitigation changes (risk)
						if (includeStepHistory && historyChangeDetail.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.RiskMitigation)
						{
							dataItemField.TextValue = "[" + Resources.ServerControls.TabControl_Mitigations + "] " + dataItemField.TextValue;
						}
					}
					else
					{
						PopulateFieldRow(dataItem, dataItemField, historyChangeDetail, null, null, false, null);
					}

					//Some of the old/new values need special handling
					if (dataItemField.FieldName == "OldValue")
					{
						//If we have a date field, neede to convert from UTC to local
						if (historyChangeDetail.OldValueDate.HasValue)
						{
							dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(historyChangeDetail.OldValueDate.Value));
						}
						else if (historyChangeDetail.ArtifactFieldTypeId == (int)Artifact.ArtifactFieldTypeEnum.Integer && historyChangeDetail.OldValueInt.HasValue)
						{
							dataItemField.TextValue = historyChangeDetail.OldValueInt.Value.ToString();
						}
						else if (historyChangeDetail.ArtifactFieldTypeId == (int)Artifact.ArtifactFieldTypeEnum.Decimal && !String.IsNullOrEmpty(historyChangeDetail.OldValue))
						{
							decimal decValue;
							if (Decimal.TryParse(historyChangeDetail.OldValue, out decValue))
							{
								//Display as non-padded string
								dataItemField.TextValue = decValue.ToString();
							}
						}
						else if (historyChangeDetail.ArtifactFieldTypeId == (int)Artifact.ArtifactFieldTypeEnum.TimeInterval && !String.IsNullOrEmpty(historyChangeDetail.OldValue))
						{
							decimal decValue;
							if (Decimal.TryParse(historyChangeDetail.OldValue, out decValue))
							{
								//Display as fractional hours
								decimal fractionalHours = decValue / 60M;
								dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_TIME_INTERVAL_HOURS, fractionalHours);
							}
						}
						else
						{
							//The old value need to be converted into plain text
							dataItemField.TextValue = Strings.StripHTML(dataItemField.TextValue);
						}
					}
					if (dataItemField.FieldName == "NewValue")
					{
						//If we have a date field, neede to convert from UTC to local
						if (historyChangeDetail.NewValueDate.HasValue)
						{
							dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(historyChangeDetail.NewValueDate.Value));
						}
						else if (historyChangeDetail.ArtifactFieldTypeId == (int)Artifact.ArtifactFieldTypeEnum.Integer && historyChangeDetail.NewValueInt.HasValue)
						{
							dataItemField.TextValue = historyChangeDetail.NewValueInt.Value.ToString();
						}
						else if (historyChangeDetail.ArtifactFieldTypeId == (int)Artifact.ArtifactFieldTypeEnum.Decimal && !String.IsNullOrEmpty(historyChangeDetail.NewValue))
						{
							decimal decValue;
							if (Decimal.TryParse(historyChangeDetail.NewValue, out decValue))
							{
								//Display as non-padded string
								dataItemField.TextValue = decValue.ToString();
							}
						}
						else if (historyChangeDetail.ArtifactFieldTypeId == (int)Artifact.ArtifactFieldTypeEnum.TimeInterval && !String.IsNullOrEmpty(historyChangeDetail.NewValue))
						{
							decimal decValue;
							if (Decimal.TryParse(historyChangeDetail.NewValue, out decValue))
							{
								//Display as fractional hours
								decimal fractionalHours = decValue / 60M;
								dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_TIME_INTERVAL_HOURS, fractionalHours);
							}
						}
						else
						{
							//The new value need to be converted into plain text
							dataItemField.TextValue = Strings.StripHTML(dataItemField.TextValue);
						}
					}

					//If we are displaying the ChangeSetId as a link we need to pass value through as a 'Custom Url'
					//because the link needs to use the ChangeSetId not the ArtifactHistoryId (the table primary key)
					if (isProjectAdmin && dataItemField.FieldName == "ChangeSetId")
					{
						//Add link through to history page
						dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/" + projectId + "/Administration/HistoryDetails/" + dataItemField.TextValue + ".aspx");
					}

					//There are currently no editable fields
					dataItemField.Editable = false;
					dataItemField.Required = false;
				}
			}

			//If we have the ComponentIds multiselect, need to display 'nice' display names rather than IDs
			if (historyChangeDetail.FieldName == "ComponentIds" && components != null)
			{
				//Old Value
				if (!String.IsNullOrEmpty(historyChangeDetail.OldValue))
				{
					string summary;
					string names;
					List<int> componentIds = historyChangeDetail.OldValue.FromDatabaseSerialization_List_Int32();
					ComponentManager.GetComponentNamesFromIds(componentIds, components, Resources.Main.Global_Multiple, out summary, out names);
					dataItem.Fields["OldValue"].TextValue = names;
				}

				//New Value
				if (!String.IsNullOrEmpty(historyChangeDetail.NewValue))
				{
					string summary;
					string names;
					List<int> componentIds = historyChangeDetail.NewValue.FromDatabaseSerialization_List_Int32();
					ComponentManager.GetComponentNamesFromIds(componentIds, components, Resources.Main.Global_Multiple, out summary, out names);
					dataItem.Fields["NewValue"].TextValue = names;
				}
			}

			//If we have a custom property, need to make sure we're deserializing the value to make it display 'nicely'
			if (historyChangeDetail.CustomPropertyId.HasValue)
			{
				int customPropertyId = historyChangeDetail.CustomPropertyId.Value;

				//Get the custom property definition
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveById(customPropertyId, false);
				if (customProperty != null)
				{
					//Certain types need to have the old and new values deserialized to display correctly
					switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
					{
						case CustomProperty.CustomPropertyTypeEnum.Boolean:
							{
								bool? oldValue = dataItem.Fields["OldValue"].TextValue.FromDatabaseSerialization_Boolean();
								bool? newValue = dataItem.Fields["NewValue"].TextValue.FromDatabaseSerialization_Boolean();
								if (oldValue.HasValue)
								{
									dataItem.Fields["OldValue"].TextValue = GlobalFunctions.DisplayYnFlag(oldValue.Value);
								}
								if (newValue.HasValue)
								{
									dataItem.Fields["NewValue"].TextValue = GlobalFunctions.DisplayYnFlag(newValue.Value);
								}
							}
							break;

						case CustomProperty.CustomPropertyTypeEnum.Date:
							{
								DateTime? oldValue = dataItem.Fields["OldValue"].TextValue.FromDatabaseSerialization_DateTime();
								DateTime? newValue = dataItem.Fields["NewValue"].TextValue.FromDatabaseSerialization_DateTime();
								if (oldValue.HasValue)
								{
									dataItem.Fields["OldValue"].TextValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(oldValue.Value));
								}
								if (newValue.HasValue)
								{
									dataItem.Fields["NewValue"].TextValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(newValue.Value));
								}
							}
							break;

						case CustomProperty.CustomPropertyTypeEnum.Integer:
							{
								int? oldValue = dataItem.Fields["OldValue"].TextValue.FromDatabaseSerialization_Int32();
								int? newValue = dataItem.Fields["NewValue"].TextValue.FromDatabaseSerialization_Int32();
								if (oldValue.HasValue)
								{
									dataItem.Fields["OldValue"].TextValue = oldValue.Value.ToString();
								}
								if (newValue.HasValue)
								{
									dataItem.Fields["NewValue"].TextValue = newValue.Value.ToString();
								}
							}
							break;

						case CustomProperty.CustomPropertyTypeEnum.Decimal:
							{
								decimal? oldValue = dataItem.Fields["OldValue"].TextValue.FromDatabaseSerialization_Decimal();
								decimal? newValue = dataItem.Fields["NewValue"].TextValue.FromDatabaseSerialization_Decimal();
								if (oldValue.HasValue)
								{
									dataItem.Fields["OldValue"].TextValue = oldValue.Value.ToString();
								}
								if (newValue.HasValue)
								{
									dataItem.Fields["NewValue"].TextValue = newValue.Value.ToString();
								}
							}
							break;

						case CustomProperty.CustomPropertyTypeEnum.List:
							{
								int? oldValue = dataItem.Fields["OldValue"].TextValue.FromDatabaseSerialization_Int32();
								int? newValue = dataItem.Fields["NewValue"].TextValue.FromDatabaseSerialization_Int32();

								//Find the matching custom property list values
								if (oldValue.HasValue && customProperty.CustomPropertyListId.HasValue)
								{
									CustomPropertyList cpl = customLists.Find(cl => cl.CustomPropertyListId == customProperty.CustomPropertyListId.Value);
									if (cpl != null)
									{
										CustomPropertyValue cpv = cpl.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == oldValue.Value);
										if (cpv != null)
										{
											dataItem.Fields["OldValue"].TextValue = cpv.Name;
										}
									}
								}
								if (newValue.HasValue && customProperty.CustomPropertyListId.HasValue)
								{
									CustomPropertyList cpl = customLists.Find(cl => cl.CustomPropertyListId == customProperty.CustomPropertyListId.Value);
									if (cpl != null)
									{
										CustomPropertyValue cpv = cpl.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == newValue.Value);
										if (cpv != null)
										{
											dataItem.Fields["NewValue"].TextValue = cpv.Name;
										}
									}
								}
							}
							break;

						case CustomProperty.CustomPropertyTypeEnum.MultiList:
							{
								List<int> oldValues = dataItem.Fields["OldValue"].TextValue.FromDatabaseSerialization_List_Int32();
								List<int> newValues = dataItem.Fields["NewValue"].TextValue.FromDatabaseSerialization_List_Int32();

								//Find the matching custom property list values
								if (oldValues.Count > 0 && customProperty.CustomPropertyListId.HasValue)
								{
									CustomPropertyList cpl = customLists.Find(cl => cl.CustomPropertyListId == customProperty.CustomPropertyListId.Value);
									if (cpl != null)
									{
										string valuesText = "";
										foreach (int oldValue in oldValues)
										{
											CustomPropertyValue cpv = cpl.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == oldValue);
											if (cpv != null)
											{
												if (valuesText == "")
												{
													valuesText = cpv.Name;
												}
												else
												{
													valuesText += ", " + cpv.Name;
												}
											}
											dataItem.Fields["OldValue"].TextValue = valuesText;
										}
									}
								}
								if (newValues.Count > 0 && customProperty.CustomPropertyListId.HasValue)
								{
									CustomPropertyList cpl = customLists.Find(cl => cl.CustomPropertyListId == customProperty.CustomPropertyListId.Value);
									if (cpl != null)
									{
										string valuesText = "";
										foreach (int newValue in newValues)
										{
											CustomPropertyValue cpv = cpl.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == newValue);
											if (cpv != null)
											{
												if (valuesText == "")
												{
													valuesText = cpv.Name;
												}
												else
												{
													valuesText += ", " + cpv.Name;
												}
											}
											dataItem.Fields["NewValue"].TextValue = valuesText;
										}

									}
								}
							}
							break;

						case CustomProperty.CustomPropertyTypeEnum.User:
							{
								int? oldValue = dataItem.Fields["OldValue"].TextValue.FromDatabaseSerialization_Int32();
								int? newValue = dataItem.Fields["NewValue"].TextValue.FromDatabaseSerialization_Int32();
								if (oldValue.HasValue)
								{
									//Get the user
									try
									{
										User user = new UserManager().GetUserById(oldValue.Value);
										dataItem.Fields["OldValue"].TextValue = user.FullName;
									}
									catch (ArtifactNotExistsException)
									{
										//Ignore
									}
								}
								if (newValue.HasValue)
								{
									//Get the user
									try
									{
										User user = new UserManager().GetUserById(newValue.Value);
										dataItem.Fields["NewValue"].TextValue = user.FullName;
									}
									catch (ArtifactNotExistsException)
									{
										//Ignore
									}
								}
							}
							break;

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
				UserManager user = new UserManager();

				if (lookupName == "ChangerId")
				{
					List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);
					lookupValues = ConvertLookupValues(users.OfType<Entity>().ToList(), "UserId", "FullName");
				}
				else if (lookupName == "ChangeSetTypeId")
				{
					//Generate a list from the enumeration.
					SortedList<int, string> lstTypes = new SortedList<int, string>();
					foreach (HistoryManager.ChangeSetTypeEnum item in Enum.GetValues(typeof(HistoryManager.ChangeSetTypeEnum)))
						lstTypes.Add((int)item, item.ToString());

					lookupValues = ConvertLookupValues(lstTypes);
				}

				return lookupValues;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Display an empty tooltip</summary>
		/// <param name="artifactId"></param>
		/// <returns></returns>
		public string RetrieveNameDesc(int? projectId, int artifactId, int? displayTypeId)
		{
			return null;
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
