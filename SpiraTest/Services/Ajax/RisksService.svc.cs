using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>Communicates with the SortableGrid AJAX component for displaying/updating risks</summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class RisksService : SortedListServiceBase, IRisksService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.RisksService::";

		protected const string PROJECT_SETTINGS_PAGINATION = GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL;

		#region IList interface methods

		/// <summary>
		/// Changes the width of a column in a grid. Needs to be overidden by the subclass
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="fieldName">The name of the column being moved</param>
		/// <param name="width">The new width of the column (in pixels)</param>
		public override void List_ChangeColumnWidth(int projectId, string fieldName, int width)
		{
			const string METHOD_NAME = "List_ChangeColumnWidth";

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
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Change the width of the appropriate artifact field or custom property
				ArtifactManager artifactManager = new ArtifactManager();
				artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Risk, fieldName, width);
			}
			catch (InvalidOperationException)
			{
				//The field cannot be found, so fail quietly
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Changes the order of columns in the risk list
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="fieldName">The name of the column being moved</param>
		/// <param name="newIndex">The new index of the column's position</param>
		public override void List_ChangeColumnPosition(int projectId, string fieldName, int newIndex)
		{
			const string METHOD_NAME = "List_ChangeColumnPosition";

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
				//The field position may be different to the index because index is zero-based
				int newPosition = newIndex + 1;

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Toggle the status of the appropriate artifact field or custom property
				ArtifactManager artifactManager = new ArtifactManager();
				artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Risk, fieldName, newPosition);
			}
			catch (InvalidOperationException)
			{
				//The field cannot be found, so fail quietly
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region Internal Functions

		/// <summary>Handles service-specific functionality that can be performed on a selected number of items in the sorted grid</summary>
		/// <param name="operation">The name of the operation</param>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="destId">The destination id (if appropriate)</param>
		/// <param name="items">The items to peform the operation on</param>
		/// <returns></returns>
		public override string CustomListOperation(string operation, int projectId, int destId, List<string> items)
		{
			const string METHOD_NAME = "CustomListOperation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			try
			{
				//See which operation we have
				throw new NotImplementedException("Operation '" + operation + "' is not currently supported");

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

		#region Sorted List interface methods

		/// <summary>Returns a list of risks in the system for the specific user/project</summary>
		/// <param name="userId">The user we're viewing the risks as</param>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the risk and custom property business objects
				RiskManager riskManager = new RiskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the list of components (cannot use a lookup field since multilist). Include inactive, but not deleted
				List<Component> components = new ComponentManager().Component_Retrieve(projectId, false, false);

				//Create the array of data items (including the first filter item)
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now get the list of populated filters and the current sort
				string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_FILTERS;
				string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL;

				//Now get the list of populated filters and the current sort
				Hashtable filterList = GetProjectSettings(userId, projectId, filtersSettingsCollection);
				string sortCommand = GetProjectSetting(userId, projectId, sortSettingsCollection, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "RiskId ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

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
				SortedDataItem filterItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
				dataItems.Add(filterItem);
				sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Risk);

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
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
				//Get the number of risks in the project
				int artifactCount = riskManager.Risk_Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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
				List<RiskView> risks = riskManager.Risk_Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

				//Display the pagination information
				sortedData.CurrPage = currentPage;
				sortedData.PageCount = pageCount;
				sortedData.StartRow = startRow;

				//Display the visible and total count of artifacts
				sortedData.VisibleCount = risks.Count;
				sortedData.TotalCount = artifactCount;

				//Display the sort information
				sortedData.SortProperty = sortProperty;
				sortedData.SortAscending = sortAscending;

				//Now get the list of custom property options and lookup values for this artifact type / project
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false, true);

				//Iterate through all the risks and populate the dataitem
				foreach (RiskView risk in risks)
				{
					//We clone the template item as the basis of all the new items
					SortedDataItem dataItem = filterItem.Clone();

					//Now populate with the data
					PopulateRow(dataItem, risk, customProperties, false, null, components);
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

		/// <summary>Returns a plain-text version of the artifact name/description typically used in dynamic tooltips</summary>
		/// <param name="riskId">The id of the risk to get the data for</param>
		/// <returns>The name and description converted to plain-text</returns>
		/// <remarks>For risks also includes the most recent resolution</remarks>
		public string RetrieveNameDesc(int? projectId, int riskId, int? displayTypeId)
		{
			const string METHOD_NAME = "RetrieveNameDesc";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the risk business object
				RiskManager riskManager = new RiskManager();

				//Now retrieve the specific risk - handle quietly if it doesn't exist
				try
				{
					Risk risk = riskManager.Risk_RetrieveById(riskId, true, true);
					string tooltip;
					tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(risk.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_RISK, risk.RiskId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(risk.Description);

					//See if we have any comments to append
					if (risk.Discussions.Count > 0)
					{
						RiskDiscussion comment = risk.Discussions.OrderByDescending(r => r.CreationDate).First();

						tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
							GlobalFunctions.LocalizeDate(comment.CreationDate).ToShortDateString(),
							GlobalFunctions.HtmlRenderAsPlainText(comment.Text),
							Microsoft.Security.Application.Encoder.HtmlEncode(comment.Creator.FullName)
							);
					}

					//See if we have any mitigations to append
					if (risk.Mitigations.Count > 0)
					{
						tooltip += "<ul>";
						foreach (RiskMitigation mitigation in risk.Mitigations)
						{
							if (mitigation.ReviewDate.HasValue)
							{
								tooltip += String.Format("<li><i>{0} - {1} ({2})</i></li>",
									GlobalFunctions.LocalizeDate(mitigation.CreationDate).ToShortDateString(),
									GlobalFunctions.HtmlRenderAsPlainText(mitigation.Description),
									GlobalFunctions.LocalizeDate(mitigation.ReviewDate).Value.ToShortDateString()
									);
							}
							else
							{
								tooltip += String.Format("<li><i>{0} - {1}</i></li>",
									GlobalFunctions.LocalizeDate(mitigation.CreationDate).ToShortDateString(),
									GlobalFunctions.HtmlRenderAsPlainText(mitigation.Description)
									);
							}
						}
						tooltip += "</ul>";
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return tooltip;
				}
				catch (ArtifactNotExistsException)
				{
					//This is the case where the client still displays the risk, but it has already been deleted on the server
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Global_UnableRetrieveTooltip);
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

			string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL;

			//Call the base method with the appropriate settings collection
			return base.UpdateSort(userId, projectId, sortProperty, sortAscending, sortSettingsCollection);
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

			string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_FILTERS;

			//Call the base method with the appropriate settings collection
			return base.UpdateFilters(userId, projectId, filters, filtersSettingsCollection, DataModel.Artifact.ArtifactTypeEnum.Risk);
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

			return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.Risk, GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_FILTERS, GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL, isShared, existingSavedFilterId, includeColumns);
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
			return base.RetrieveFilters(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.Risk, includeShared);
		}

		/// <summary>
		/// Returns the latest information on a single risk in the system
		/// </summary>
		/// <param name="userId">The user we're viewing the risk as</param>
		/// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>A single dataitem object</returns>
		public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
		{
			const string METHOD_NAME = "Refresh";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}
			try
			{
				//Instantiate the risk and custom property business objects
				RiskManager riskManager = new RiskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Create the data item record (no filter items)
				SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

				//Get the risk dataset record for the specific risk id
				RiskView risk = riskManager.Risk_RetrieveById2(artifactId);

				//Make sure the user is authorized for this item
				int ownerId = -1;
				if (risk.OwnerId.HasValue)
				{
					ownerId = risk.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && risk.CreatorId != userId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}
				//The main dataset does not have the custom properties, they need to be retrieved separately
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, DataModel.Artifact.ArtifactTypeEnum.Risk, true);

				//Finally populate the dataitem from the dataset
				if (risk != null)
				{
					//See if we already have an artifact custom property row
					if (artifactCustomProperty != null)
					{
						PopulateRow(dataItem, risk, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty, null);
					}
					else
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false);
						PopulateRow(dataItem, risk, customProperties, true, null, null);
					}

					//See if we are allowed to bulk edit status (template setting)
					ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(projectTemplateId);
					if (!templateSettings.Workflow_BulkEditCanChangeStatus && dataItem.Fields.ContainsKey("RiskStatusId"))
					{
						dataItem.Fields["RiskStatusId"].Editable = false;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItem;
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
		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			const string METHOD_NAME = "SortedList_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Used to store any validation messages
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Iterate through each data item and make the updates
				RiskManager riskManager = new RiskManager();
				//Load the custom property definitions (once, not per artifact)
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, false);

				foreach (SortedDataItem dataItem in dataItems)
				{
					//Get the risk id
					int riskId = dataItem.PrimaryKey;

					//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
					Risk risk = riskManager.Risk_RetrieveById(riskId, false);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, riskId, DataModel.Artifact.ArtifactTypeEnum.Risk, false, customProperties);

					//Create a new artifact custom property row if one doesn't already exist
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Risk, riskId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

					if (risk != null)
					{
						//Need to set the original date of this record to match the concurrency date
						if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
						{
							DateTime concurrencyDateTimeValue;
							if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
							{
								risk.ConcurrencyDate = concurrencyDateTimeValue;
								risk.AcceptChanges();
							}
						}

						//Update the field values
						List<string> fieldsToIgnore = new List<string>();
						fieldsToIgnore.Add("Comments");
						fieldsToIgnore.Add("CreationDate");
						UpdateFields(validationMessages, dataItem, risk, customProperties, artifactCustomProperty, projectId, riskId, 0, fieldsToIgnore);

						//Now verify the options for the custom properties to make sure all rules have been followed
						Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
						foreach (KeyValuePair<string, string> customPropOptionMessage in customPropOptionMessages)
						{
							ValidationMessage newMsg = new ValidationMessage();
							newMsg.FieldName = customPropOptionMessage.Key;
							newMsg.Message = customPropOptionMessage.Value;
							AddUniqueMessage(validationMessages, newMsg);
						}

						//Perform any business level validations on the datarow
						Dictionary<string, string> businessMessages = riskManager.Validate(risk);
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
							//Get copies of everything..
							Risk notificationArt = risk.Clone();
							ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

							//Persist to database
							try
							{
								riskManager.Risk_Update(risk, userId);
							}
							catch (DataValidationException exception)
							{
								return CreateSimpleValidationMessage(exception.Message);
							}
							catch (OptimisticConcurrencyException)
							{
								return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
							}
							customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

							//Call notifications..
							try
							{
								new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
							}
							catch (Exception ex)
							{
								Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Risk " + risk.ArtifactToken + ".");
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return validationMessages;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Inserts a new risk into the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="artifact">The type of artifact we're inserting</param>
		/// <returns>Not implemented for risks since they use the details screen</returns>
		public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			throw new NotImplementedException("This operation is not currently implemented");
		}

		/// <summary>
		/// Adds/removes a column from the list of fields displayed in the current view
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="fieldName">The name of the column we displaying/hiding</param>
		public void ToggleColumnVisibility(int projectId, string fieldName)
		{
			const string METHOD_NAME = "ToggleColumnVisibility";

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
				//See if we have a custom property (they need to be handled differently)
				if (CustomPropertyManager.IsFieldCustomProperty(fieldName).HasValue)
				{
					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Toggle the status of the appropriate custom property
					Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
					customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Risk, fieldName);
				}
				else
				{
					//Toggle the status of the appropriate field name
					ArtifactManager artifactManager = new ArtifactManager();
					artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Risk, fieldName);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Copies a set of risks
		/// </summary>
		/// <param name="userId">The ID of the user making the copy</param>
		/// <param name="items">The items to copy</param>
		public void SortedList_Copy(int projectId, List<string> items)
		{
			const string METHOD_NAME = "Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Iterate through all the items to be copied
				RiskManager riskManager = new RiskManager();
				foreach (string itemValue in items)
				{
					//Get the risk ID
					int riskId = Int32.Parse(itemValue);
					riskManager.Risk_Copy(userId, riskId);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Exports a set of risks to another project</summary>
		/// <param name="items">The items to export</param>
		/// <param name="destProjectId">The project to export them to</param>
		public void SortedList_Export(int destProjectId, List<string> items)
		{

			const string METHOD_NAME = "Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(destProjectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Iterate through all the items to be exported
				RiskManager riskManager = new RiskManager();
				foreach (string itemValue in items)
				{
					//Get the risk ID
					int riskId = Int32.Parse(itemValue);
					riskManager.Risk_Export(riskId, destProjectId, userId);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Deletes a set of risks
		/// </summary>
		/// <param name="items">The items to delete</param>
		/// <param name="projectId">The id of the project (not used)</param>
		public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Iterate through all the items to be deleted
				RiskManager riskManager = new RiskManager();
				foreach (string itemValue in items)
				{
					//Get the risk ID
					int riskId = Int32.Parse(itemValue);
					riskManager.Risk_MarkAsDeleted(projectId, riskId, userId);
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
			JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, PROJECT_SETTINGS_PAGINATION);

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
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
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

		#region Internal Functions

		/// <summary>
		/// Populates a data item from a dataset datarow
		/// </summary>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="riskView">The entity containing the data</param>
		/// <param name="customProperties">The list of custom property definitions and values</param>
		/// <param name="editable">Does the data need to be in editable form?</param>
		/// <param name="workflowCustomProps">The custom properties workflow states</param>
		/// <param name="workflowFields">The standard fields workflow states</param>
		/// <param name="components">The list of components in the project (or null)</param>
		/// <param name="artifactCustomProperty">The artifatc's custom property data (if not provided as part of dataitem) - pass null if not used</param>
		protected void PopulateRow(SortedDataItem dataItem, RiskView riskView, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty, List<Component> components, List<RiskWorkflowField> workflowFields = null, List<RiskWorkflowCustomProperty> workflowCustomProps = null)
		{
			//Set the primary key and concurrency value
			dataItem.PrimaryKey = riskView.RiskId;
			dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, riskView.ConcurrencyDate);

			//Specify if it has an attachment or not
			dataItem.Attachment = riskView.IsAttachments;

			//Convert the workflow lists into the type expected by the ListServiceBase function
			List<WorkflowField> workflowFields2 = RiskWorkflowManager.ConvertFields(workflowFields);
			List<WorkflowCustomProperty> workflowCustomProps2 = RiskWorkflowManager.ConvertFields(workflowCustomProps);

			//The date and some effort fields are not editable
			List<string> readOnlyFields = new List<string>() { "CreationDate", "LastUpdateDate", "RiskExposure" };

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (riskView.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the data-row
					PopulateFieldRow(dataItem, dataItemField, riskView, customProperties, artifactCustomProperty, editable, null, workflowFields2, workflowCustomProps2, readOnlyFields);

					//Apply the conditional formatting to the priority and severity columns (if displayed)
					if (dataItemField.FieldName == "RiskProbabilityId" && riskView.RiskProbabilityId.HasValue)
					{
						//Despite the name, cssClass can store either color or CSS class for SortedDataItem's
						dataItemField.CssClass = "#" + riskView.RiskProbabilityColor;
					}
					if (dataItemField.FieldName == "RiskImpactId" && riskView.RiskImpactId.HasValue)
					{
						//Despite the name, cssClass can store either color or CSS class for SortedDataItem's
						dataItemField.CssClass = "#" + riskView.RiskImpactColor;
					}
					if (dataItemField.FieldName == "RiskExposure" && !String.IsNullOrEmpty(riskView.RiskProbabilityColor) && !String.IsNullOrEmpty(riskView.RiskImpactColor))
					{
						//Despite the name, cssClass can store either color or CSS class for SortedDataItem's
						//We interpolate between the impact and probability colors
						System.Drawing.Color backColor = GlobalFunctions.InterpolateColor2(riskView.RiskProbabilityScore.HasValue ? riskView.RiskProbabilityScore.Value : 1, riskView.RiskImpactScore.HasValue ? riskView.RiskImpactScore.Value : 1);
						string exposureColor = System.Drawing.ColorTranslator.ToHtml(backColor);
						dataItemField.CssClass = exposureColor;
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
				ReleaseManager release = new ReleaseManager();
				Business.UserManager user = new Business.UserManager();
				RiskManager riskManager = new RiskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				//
				if (lookupName == "CreatorId" || lookupName == "OwnerId")
				{
					List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
					lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
				}
				if (lookupName == "ComponentId")
				{
					List<DataModel.Component> components = new ComponentManager().Component_Retrieve(projectId);
					lookupValues = ConvertLookupValues(components.OfType<DataModel.Entity>().ToList(), "ComponentId", "Name");
				}
				if (lookupName == "RiskProbabilityId")
				{
					List<RiskProbability> riskProbabilities = riskManager.RiskProbability_Retrieve(projectTemplateId, true);
					lookupValues = ConvertLookupValues(riskProbabilities.OfType<Entity>().ToList(), "RiskProbabilityId", "Name");
				}
				if (lookupName == "RiskImpactId")
				{
					List<RiskImpact> riskImpacts = riskManager.RiskImpact_Retrieve(projectTemplateId, true);
					lookupValues = ConvertLookupValues(riskImpacts.OfType<Entity>().ToList(), "RiskImpactId", "Name");
				}
				if (lookupName == "RiskStatusId")
				{
					List<RiskStatus> riskStati = riskManager.RiskStatus_Retrieve(projectTemplateId, true);
					lookupValues = new JsonDictionaryOfStrings();
					//Add the composite (All Open) and (All Closed) items to the risk status filter
					lookupValues.Add(RiskManager.RiskStatusId_AllOpen.ToString(), Resources.Fields.IncidentStatus_AllOpen);
					lookupValues.Add(RiskManager.RiskStatusId_AllClosed.ToString(), Resources.Fields.IncidentStatus_AllClosed);

					//Now add the real lookup values
					AddLookupValues(lookupValues, riskStati.OfType<Entity>().ToList(), "RiskStatusId", "Name");
				}
				if (lookupName == "RiskTypeId")
				{
					List<RiskType> riskTypes = riskManager.RiskType_Retrieve(projectTemplateId, true);
					lookupValues = ConvertLookupValues(riskTypes.OfType<Entity>().ToList(), "RiskTypeId", "Name");
				}
				if (lookupName == "ReleaseId")
				{
					//The release includes just active releases for risks
					List<ReleaseView> releases = release.RetrieveByProjectId(projectId, false, true);
					lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
				}

				//The custom property lookups
				int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
				if (customPropertyNumber.HasValue)
				{
					CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, customPropertyNumber.Value, true);
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
		/// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the risks as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		/// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
		{
			//There are no static columns to add

			//We need to dynamically add the various columns from the field list
			LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
			AddDynamicColumns(Artifact.ArtifactTypeEnum.Risk, getLookupValues, projectId, projectTemplateId, userId, dataItem, filterList, null, returnJustListFields);
		}

		/// <summary>
		/// Verifies the digital signature on a workflow status change if it is required
		/// </summary>
		/// <param name="workflowId">The id of the workflow</param>
		/// <param name="originalStatusId">The original status</param>
		/// <param name="currentStatusId">The new status</param>
		/// <param name="signature">The digital signature</param>
		/// <param name="detectorId">The detector of the risk</param>
		/// <param name="ownerId">The owner of the risk</param>
		/// <returns>True for a valid signature, Null if no signature required and False if invalid signature</returns>
		protected bool? VerifyDigitalSignature(int workflowId, int originalStatusId, int currentStatusId, Signature signature, int detectorId, int? ownerId)
		{
			const string METHOD_NAME = "VerifyDigitalSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RiskWorkflowManager workflowManager = new RiskWorkflowManager();
				RiskWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, originalStatusId, currentStatusId);
				if (workflowTransition == null)
				{
					//No transition possible, so return failure
					return false;
				}
				if (!workflowTransition.IsSignature)
				{
					//No signature required, so return null
					return null;
				}

                //Make sure the login/password was valid
                string lowerUser = signature.Login.ToLower(System.Globalization.CultureInfo.InvariantCulture);
                bool isValidUser = Membership.ValidateUser(lowerUser, signature.Password);

                //If the password check does not return, lets see if the password is a GUID and test it against RSS/API Key
                Guid passGu;
                if (!isValidUser && Guid.TryParse(signature.Password, out passGu))
                {
                    SpiraMembershipProvider prov = (SpiraMembershipProvider)Membership.Provider;
                    if (prov != null)
                    {
                        isValidUser = prov.ValidateUserByRssToken(lowerUser, signature.Password, true, true);
                    }
                }

                if (!isValidUser)
                {
                    //User's login/password does not match
                    return false;
                }

                //Make sure the login is for the current user
                MembershipUser user = Membership.GetUser();
				if (user == null)
				{
					//Not authenticated (should't ever hit this point)
					return false;
				}
				if (user.UserName != signature.Login)
				{
					//Signed login does not match current user
					return false;
				}
				int userId = (int)user.ProviderUserKey;
				int? projectRoleId = SpiraContext.Current.ProjectRoleId;

				//Make sure the user can execute this transition
				bool isAllowed = false;
				workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransition.WorkflowTransitionId);
				if (workflowTransition.IsExecuteByCreator && detectorId == userId)
				{
					isAllowed = true;
				}
				else if (workflowTransition.IsExecuteByOwner && ownerId.HasValue && ownerId.Value == userId)
				{
					isAllowed = true;
				}
				else if (projectRoleId.HasValue && workflowTransition.TransitionRoles.Any(r => r.ProjectRoleId == projectRoleId.Value))
				{
					isAllowed = true;
				}
				return isAllowed;
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
		/// Returns a list of risks for display in the navigation bar
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="indentLevel">Not used for risks since not hierarchical</param>
		/// <returns>List of risks</returns>
		/// <param name="displayMode">
		/// The display mode of the navigation list:
		/// 1 = Filtered List
		/// 2 = All Items (no filters)
		/// 3 = Assigned to the Current User
		/// 4 = Detected by the Current User
		/// </param>
		/// <param name="selectedItemId">The id of the currently selected item</param>
		/// <remarks>Returns just the child items of the passed-in indent-level</remarks>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the risk business object
				RiskManager riskManager = new RiskManager();

				//Create the array of data items
				List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

				//Now get the list of populated filters if appropriate
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_FILTERS);

				//Get the sort information
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "RiskId ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
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
				//Get the number of risks in the project
				int artifactCount = riskManager.Risk_Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

				//**** Now we need to actually populate the rows of data to be returned ****

				//Get the risks list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
				}
				List<RiskView> risks = null;
				if (displayMode == 2)
				{
					//Make sure authorized for all items
					if (authorizationState != Project.AuthorizationState.Authorized)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//All Items
					risks = riskManager.Risk_Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, 0);
				}
				else if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Assigned)
				{
					//Assigned to User
					risks = riskManager.Risk_RetrieveOpenByOwnerId(userId, projectId, null);
				}
				else if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Detected)
				{
					//Detected by User
					risks = riskManager.Risk_RetrieveOpenByCreatorId(userId, projectId);
				}
				else
				{
					//Make sure authorized for all items
					if (authorizationState != Project.AuthorizationState.Authorized)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Filtered List
					risks = riskManager.Risk_Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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

				//Iterate through all the risks and populate the dataitem (only some columns are needed)
				foreach (RiskView risk in risks)
				{
					//Create the data-item
					HierarchicalDataItem dataItem = new HierarchicalDataItem();

					//Populate the necessary fields
					dataItem.PrimaryKey = risk.RiskId;
					dataItem.Indent = "";
					dataItem.Expanded = false;

					//Name/Desc (include the IN #)
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.TextValue = risk.Name + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_RISK, risk.RiskId, true);
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
		/// Returns a list of pagination options that the user can choose from
		/// </summary>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		public JsonDictionaryOfStrings NavigationBar_RetrievePaginationOptions(int projectId)
		{
			//Same implementation as the list service
			return RetrievePaginationOptions(projectId);
		}

		/// <summary>
		/// Updates the display settings used by the Navigation Bar
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="displayMode">The current display mode</param>
		/// <param name="displayWidth">The display width</param>
		/// <param name="minimized">Is the navigation bar minimized or visible</param>
		public void NavigationBar_UpdateSettings(int projectId, Nullable<int> displayMode, Nullable<int> displayWidth, Nullable<bool> minimized)
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
				ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL);
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

		#region WorkflowOperations Methods

		/// <summary>
		/// Retrieves the list of workflow operations for the current risk
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="typeId">The risk type</param>
		/// <param name="artifactId">The id of the risk</param>
		/// <returns>The list of available workflow operations</returns>
		/// <remarks>Pass a specific type id if the user has changed the type of the risk, but not saved it yet.</remarks>
		public List<DataItem> WorkflowOperations_Retrieve(int projectId, int artifactId, int? typeId)
		{
			const string METHOD_NAME = "WorkflowOperations_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Create the array of data items to store the workflow operations
				List<DataItem> dataItems = new List<DataItem>();

				//Get the list of available transitions for the current step in the workflow
				RiskManager riskManager = new RiskManager();
				RiskWorkflowManager workflowManager = new RiskWorkflowManager();
				Risk risk = riskManager.Risk_RetrieveById(artifactId, false);
				int workflowId;
				if (typeId.HasValue)
				{
					workflowId = workflowManager.Workflow_GetForRiskType(typeId.Value);
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForRiskType(risk.RiskTypeId);
				}

				//Get the current user's role
				int projectRoleId = (SpiraContext.Current.ProjectRoleId.HasValue) ? SpiraContext.Current.ProjectRoleId.Value : -1;

				//Determine if the current user is the detected or owner of the risk
				bool isDetector = false;
				if (risk.CreatorId == CurrentUserId.Value)
				{
					isDetector = true;
				}
				bool isOwner = false;
				if (risk.OwnerId.HasValue && risk.OwnerId.Value == CurrentUserId.Value)
				{
					isOwner = true;
				}
				int statusId = risk.RiskStatusId;
				List<RiskWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusId, projectRoleId, isDetector, isOwner);

				//Populate the data items list
				foreach (RiskWorkflowTransition workflowTransition in workflowTransitions)
				{
					//The data item itself
					DataItem dataItem = new DataItem();
					dataItem.PrimaryKey = (int)workflowTransition.WorkflowTransitionId;
					dataItems.Add(dataItem);

					//The WorkflowId field
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "WorkflowId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.RiskWorkflowId;
					dataItem.Fields.Add("WorkflowId", dataItemField);

					//The Name field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
					dataItemField.TextValue = workflowTransition.Name;
					dataItem.Fields.Add("Name", dataItemField);

					//The InputStatusId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "InputStatusId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.InputRiskStatusId;
					dataItemField.TextValue = workflowTransition.InputStatus.Name;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The OutputStatusId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OutputStatusId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.OutputRiskStatusId;
					dataItemField.TextValue = workflowTransition.OutputStatus.Name;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The OutputStatusOpenYn field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OutputStatusOpenYn";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
					dataItemField.TextValue = (workflowTransition.OutputStatus.IsOpen) ? "Y" : "N";
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The SignatureYn field (does it need a signature)
					dataItemField = new DataItemField();
					dataItemField.FieldName = "SignatureYn";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
					dataItemField.TextValue = (workflowTransition.IsSignature) ? "Y" : "N";
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				}

				return dataItems;
			}
			catch (ArtifactNotExistsException)
			{
				//Just return nothing back
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region IFormService methods

		/// <summary>Returns a single risk data record (all columns) for use by the FormManager control</summary>
		/// <param name="artifactId">The id of the current risk - null means new risk</param>
		/// <returns>An risk data item</returns>
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

			//Make sure we're authorized (limited edit or full edit)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the business classes
				RiskManager riskManager = new RiskManager();
				RiskWorkflowManager workflowManager = new RiskWorkflowManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Create the data item record (no filter items)
				SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, false);

				//Need to add the empty column to capture any new comments added
				if (!dataItem.Fields.ContainsKey("NewComment"))
				{
					dataItem.Fields.Add("NewComment", new DataItemField() { FieldName = "NewComment", Required = false, Editable = true, Hidden = false });
				}

				//Get the risk for the specific risk id or just create a new temporary one
				RiskView riskView = null;
				ArtifactCustomProperty artifactCustomProperty = null;
				if (artifactId.HasValue)
				{
					riskView = riskManager.Risk_RetrieveById2(artifactId.Value);
					//The main dataset does not have the custom properties, they need to be retrieved separately
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, DataModel.Artifact.ArtifactTypeEnum.Risk, true);

					//Make sure the user is authorized for this item
					int ownerId = -1;
					if (riskView.OwnerId.HasValue)
					{
						ownerId = riskView.OwnerId.Value;
					}
					if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && riskView.CreatorId != userId)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Also need to add the read-only title field since we display 'New Risk' for new risks
					dataItem.Fields.Add("Title", new DataItemField() { FieldName = "Title", TextValue = riskView.Name });

					//Also need to return back a special field to denote if the user is the owner or creator of the risk
					bool isArtifactCreatorOrOwner = (ownerId == userId || riskView.CreatorId == userId);
					dataItem.Fields.Add("_IsArtifactCreatorOrOwner", new DataItemField() { FieldName = "_IsArtifactCreatorOrOwner", TextValue = isArtifactCreatorOrOwner.ToDatabaseSerialization() });
				}
				else
				{
					//Insert Case, need to create the new risk
					riskView = riskManager.Risk_New(projectId, userId);

					//Also need to add a PlaceholderId field that's used to capture the placeholder id used for storing attachments and associations
					//prior to final risk submission
					if (!dataItem.Fields.ContainsKey("PlaceholderId"))
					{
						dataItem.Fields.Add("PlaceholderId", new DataItemField() { FieldName = "PlaceholderId" });
					}

					//Also need to add the read-only title field since we display 'New Risk' for new risks
					if (!dataItem.Fields.ContainsKey("Title"))
					{
						dataItem.Fields.Add("Title", new DataItemField() { FieldName = "Title", TextValue = Resources.Dialogs.Global_NewRisk });
					}

					//Also we need to populate any default Artifact Custom Properties
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false);
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Risk, -1, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
				}

				//Get the list of workflow fields and custom properties
				int workflowId;
				int statusId;
				if (artifactId.HasValue)
				{
					workflowId = workflowManager.Workflow_GetForRiskType(riskView.RiskTypeId);
					statusId = riskView.RiskStatusId;
				}
				else
				{
					//Get the default status and default workflow
					int riskTypeId = riskManager.RiskType_RetrieveDefault(projectTemplateId).RiskTypeId;
					workflowId = workflowManager.Workflow_GetForRiskType(riskTypeId);
					statusId = riskManager.RiskStatus_RetrieveDefault(projectTemplateId).RiskStatusId;
					riskView.RiskStatusId = statusId; //Needed so that the form manager knows the default status
				}
				List<RiskWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, statusId);
				List<RiskWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, statusId);

				//Finally populate the dataitem from the dataset
				if (riskView != null)
				{
					//See if we have any existing artifact custom properties for this row
					if (artifactCustomProperty == null)
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false);
						PopulateRow(dataItem, riskView, customProperties, true, (ArtifactCustomProperty)null, null, workflowFields, workflowCustomProps);
					}
					else
					{
						PopulateRow(dataItem, riskView, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty, null, workflowFields, workflowCustomProps);
					}

					//The Resolution (comments) field is not part of the entity so needs to be handled separately for workflow
					if (dataItem.Fields.ContainsKey("Comments"))
					{
						DataItemField resolutionField = dataItem.Fields["Comments"];
						if (workflowFields.Any(f => f.ArtifactField.Name == "Comments" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
						{
							resolutionField.Required = true;
						}
						if (workflowFields.Any(f => f.ArtifactField.Name == "Comments" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
						{
							resolutionField.Hidden = true;
						}
						if (!workflowFields.Any(f => f.ArtifactField.Name == "Comments" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
						{
							resolutionField.Editable = true;
						}
					}

					//Populate any data mapping values are not part of the standard 'shape'
					if (artifactId.HasValue)
					{
						DataMappingManager dataMappingManager = new DataMappingManager();
						List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.Risk, artifactId.Value);
						foreach (DataSyncArtifactMapping artifactMapping in artifactMappings)
						{
							DataItemField dataItemField = new DataItemField();
							dataItemField.FieldName = DataMappingManager.FIELD_PREPEND + artifactMapping.DataSyncSystemId;
							dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
							if (String.IsNullOrEmpty(artifactMapping.ExternalKey))
							{
								dataItemField.TextValue = "";
							}
							else
							{
								dataItemField.TextValue = artifactMapping.ExternalKey;
							}
							dataItemField.Editable = (SpiraContext.Current.IsProjectAdmin); //Read-only unless project admin
							dataItemField.Hidden = false;   //Always visible
							dataItem.Fields.Add(DataMappingManager.FIELD_PREPEND + artifactMapping.DataSyncSystemId, dataItemField);
						}
					}
				}

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

		/// <summary>
		/// Creates a new risk id and returns it to the form
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The id of the new artifact or null if one is not created</returns>
		/// <remarks>
		/// Risks don't create a new record when created, so we just need to return null for risks
		/// </remarks>
		public override int? Form_New(int projectId, int artifactId)
		{
			return null;    //Tells the form manager to simply set the mode to 'New'
		}

		/// Clones the current requirement and returns the ID of the item to redirect to
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <returns>The id to redirect to</returns>
		public override int? Form_Clone(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Form_Clone";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized to create the item
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Now we need to make a copy of it and then redirect to the copy
				RiskManager riskManager = new RiskManager();
				int newRiskId = riskManager.Risk_Copy(userId, artifactId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newRiskId;
			}
			catch (ArtifactNotExistsException)
			{
				//The item does not exist, so return null
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Deletes the current requirement and returns the ID of the item to redirect to (if any)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <returns>The id to redirect to</returns>
		public override int? Form_Delete(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Form_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized to delete the item
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Task);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//First we need to determine which risk to redirect the user to after the delete
				int? newRiskId = null;
				//Look through the current dataset to see what is the next risk in the list
				//If we are the last one on the list then we need to simply use the one before

				//Now get the list of populated filters if appropriate
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_FILTERS);

				//Get the sort information
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "RiskId ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL);
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
				//Get the number of risks in the project
				RiskManager riskManager = new RiskManager();
				int artifactCount = riskManager.Risk_Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				//Get the risks list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
				}

				List<RiskView> riskNavigationList = riskManager.Risk_Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				bool matchFound = false;
				int previousRiskId = -1;
				foreach (RiskView risk in riskNavigationList)
				{
					int testRiskId = risk.RiskId;
					if (testRiskId == artifactId)
					{
						matchFound = true;
					}
					else
					{
						//If we found a match on the previous iteration, then we want to this (next) task
						if (matchFound)
						{
							newRiskId = testRiskId;
							break;
						}

						//If this matches the current risk, set flag
						if (testRiskId == artifactId)
						{
							matchFound = true;
						}
						if (!matchFound)
						{
							previousRiskId = testRiskId;
						}
					}
				}
				if (!newRiskId.HasValue && previousRiskId != -1)
				{
					newRiskId = previousRiskId;
				}

				//Next we need to delete the current risk
				riskManager.Risk_MarkAsDeleted(projectId, artifactId, userId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return newRiskId;
			}
			catch (ArtifactNotExistsException)
			{
				//The item does not exist, so return null
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Saves a single risk data item</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The risk to save</param>
		/// <param name="operation">The type of save operation ('new', 'close', '', etc.)</param>
		/// <returns>Any error message or null if successful</returns>
		/// <param name="signature">Any digital signature</param>
		public List<ValidationMessage> Form_Save(int projectId, DataItem dataItem, string operation, Signature signature)
		{
			const string METHOD_NAME = CLASS_NAME + "Form_Save";
			Logger.LogEnteringEvent(METHOD_NAME);

			//The return list..
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (limited is OK, we check that later)
			//For new risks need create vs. modify permissions
			Project.PermissionEnum requiredPermission = (dataItem.PrimaryKey > 0) ? Project.PermissionEnum.Modify : Project.PermissionEnum.Create;

			Project.AuthorizationState authorizationState = IsAuthorized(projectId, requiredPermission, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Get the risk id
			int riskId = dataItem.PrimaryKey;

			try
			{
				//Instantiate the business classes
				RiskManager riskManager = new RiskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				RiskWorkflowManager workflowManager = new RiskWorkflowManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Load the custom property definitions (once, not per artifact)
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, false);

				//If we have a zero/negative primary key it means that it's actually a new item being inserted
				Risk risk;
				ArtifactCustomProperty artifactCustomProperty;
				if (riskId < 1)
				{
					//Insert Case, need to use the Risk_New() method since we need the mandatory fields populated
					RiskView riskView = riskManager.Risk_New(projectId, userId);
					risk = riskView.ConvertTo<RiskView, Risk>();

					//Also we need to populate any default Artifact Custom Properties
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Risk, -1, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
				}
				else
				{
					//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
					risk = riskManager.Risk_RetrieveById(riskId, false);

					//Make sure the user is authorized for this item if they only have limited permissions
					int ownerId = -1;
					if (risk.OwnerId.HasValue)
					{
						ownerId = risk.OwnerId.Value;
					}
					if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && risk.CreatorId != userId)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Create a new artifact custom property row if one doesn't already exist
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, riskId, DataModel.Artifact.ArtifactTypeEnum.Risk, false, customProperties);
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Risk, riskId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}
				}

				//For saving, need to use the current status and type of the dataItem which may be different to the one retrieved
				int currentStatusId = (dataItem.Fields["RiskStatusId"].IntValue.HasValue) ? dataItem.Fields["RiskStatusId"].IntValue.Value : -1;
				int originalStatusId = risk.RiskStatusId;
				int riskTypeId = (dataItem.Fields["RiskTypeId"].IntValue.HasValue) ? dataItem.Fields["RiskTypeId"].IntValue.Value : -1;

				//Get the list of workflow fields and custom properties
				int workflowId;
				if (riskTypeId < 1)
				{
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).RiskWorkflowId;
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForRiskType(riskTypeId);
				}
				List<RiskWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, currentStatusId);
				List<RiskWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, currentStatusId);

				//Convert the workflow lists into the type expected by the ListServiceBase function
				List<WorkflowField> workflowFields2 = RiskWorkflowManager.ConvertFields(workflowFields);
				List<WorkflowCustomProperty> workflowCustomProps2 = RiskWorkflowManager.ConvertFields(workflowCustomProps);

				//If the workflow status changed, check to see if we need a digital signature and if it was provided and is valid
				if (currentStatusId != originalStatusId)
				{
					//Only attempt to verify signature requirements if we have no concurrency date or if the client side concurrency matches that from the DB
					bool shouldVerifyDigitalSignature = true;
					if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
					{
						DateTime concurrencyDateTimeValue;
						if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
						{
							shouldVerifyDigitalSignature = risk.ConcurrencyDate == concurrencyDateTimeValue;
						}
					}

					if (shouldVerifyDigitalSignature)
					{
						bool? valid = VerifyDigitalSignature(workflowId, originalStatusId, currentStatusId, signature, risk.CreatorId, risk.OwnerId);
						if (valid.HasValue)
						{
							if (valid.Value)
							{
								//Add the meaning to the artifact so that it can be recorded
								risk.SignatureMeaning = signature.Meaning;
							}
							else
							{
								//Let the user know that the digital signature is not valid
								return CreateSimpleValidationMessage(Resources.Messages.Services_DigitalSignatureNotValid);
							}
						}
					}
				}

				//Need to set the original date of this record to match the concurrency date
				//The value is already in UTC so no need to convert
				if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
				{
					DateTime concurrencyDateTimeValue;
					if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
					{
						risk.ConcurrencyDate = concurrencyDateTimeValue;
						risk.AcceptChanges();
					}
				}

				//Now we can start tracking any changes
				risk.StartTracking();

				//Update the field values, tracking changes
				List<string> fieldsToIgnore = new List<string>();
				if (riskId < 1)
				{
					fieldsToIgnore.Add("PlaceholderId");
				}
				fieldsToIgnore.Add("NewComment");
				fieldsToIgnore.Add("Comments");
				fieldsToIgnore.Add("CreationDate");
				fieldsToIgnore.Add("LastUpdateDate");   //Breaks concurrency otherwise

				//Need to handle any data-mapping fields (project-admin only)
				if (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)
				{
					DataMappingManager dataMappingManager = new DataMappingManager();
					List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.Risk, riskId);
					foreach (KeyValuePair<string, DataItemField> kvp in dataItem.Fields)
					{
						DataItemField dataItemField = kvp.Value;
						if (dataItemField.FieldName.SafeSubstring(0, DataMappingManager.FIELD_PREPEND.Length) == DataMappingManager.FIELD_PREPEND)
						{
							//See if we have a matching row
							foreach (DataSyncArtifactMapping artifactMapping in artifactMappings)
							{
								if (DataMappingManager.FIELD_PREPEND + artifactMapping.DataSyncSystemId == dataItemField.FieldName)
								{
									artifactMapping.StartTracking();
									if (String.IsNullOrWhiteSpace(dataItemField.TextValue))
									{
										artifactMapping.ExternalKey = null;
									}
									else
									{
										artifactMapping.ExternalKey = dataItemField.TextValue;
									}
								}
							}
						}
					}

					//Now save the data
					dataMappingManager.SaveDataSyncArtifactMappings(artifactMappings);
				}

				//Update the field values
				UpdateFields(validationMessages, dataItem, risk, customProperties, artifactCustomProperty, projectId, riskId, 0, fieldsToIgnore, workflowFields2, workflowCustomProps2);

				//Check to see if a comment was required and if so, verify it was provided. It's not handled as part of 'UpdateFields'
				//because there is no Comments field on the Risk entity
				//Only prompt if the status changed to avoid endless requests for a comment
				if (workflowFields != null && workflowFields.Any(w => w.ArtifactField.Name == "Comments" && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) && currentStatusId != originalStatusId)
				{
					//Comment is required, so check that it's present
					if (String.IsNullOrWhiteSpace(dataItem.Fields["NewComment"].TextValue))
					{
						AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = "NewComment", Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, Resources.Fields.Comment) });
					}
				}

				//Now verify the options for the custom properties to make sure all rules have been followed
				Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
				foreach (KeyValuePair<string, string> customPropOptionMessage in customPropOptionMessages)
				{
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = customPropOptionMessage.Key;
					newMsg.Message = customPropOptionMessage.Value;
					AddUniqueMessage(validationMessages, newMsg);
				}

				//Perform any business level validations on the datarow
				Dictionary<string, string> businessMessages = riskManager.Validate(risk);
				foreach (KeyValuePair<string, string> businessMessage in businessMessages)
				{
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = businessMessage.Key;
					newMsg.Message = businessMessage.Value;
					AddUniqueMessage(validationMessages, newMsg);
				}

				//If we have validation messages, stop now
				if (validationMessages.Count > 0)
				{
					return validationMessages;
				}

				//Get copies of everything..
				Artifact notificationArt = risk.Clone();
				ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

				//Either insert or update the risk
				if (riskId < 1)
				{
					//Submit the new risk
					DateTime creationDate = DateTime.UtcNow;
					riskId = riskManager.Risk_Insert(
						projectId,
						risk.RiskStatusId,
						risk.RiskTypeId,
						risk.RiskProbabilityId,
						risk.RiskImpactId,
						risk.CreatorId,
						risk.OwnerId,
						risk.Name,
						risk.Description,
						risk.ReleaseId,
						risk.ComponentId,
						creationDate,
						risk.ReviewDate,
						risk.ClosedDate
						);

					//Now save the custom properties
					artifactCustomProperty.ArtifactId = riskId;
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//Now move any attachments from the placeholder to the actual risk
					if (dataItem.Fields.ContainsKey("PlaceholderId") && dataItem.Fields["PlaceholderId"].IntValue.HasValue)
					{
						int placeholderId = dataItem.Fields["PlaceholderId"].IntValue.Value;
						AttachmentManager attachment = new AttachmentManager();
						attachment.Attachment_Move(userId, projectId, placeholderId, Artifact.ArtifactTypeEnum.Placeholder, riskId, Artifact.ArtifactTypeEnum.Risk);
					}

					//We need to encode the new artifact id as a 'pseudo' validation message
					//We don't do this in the 'Save and New' case since it needs to load a new blank form
					if (operation != "new")
					{
						ValidationMessage newMsg = new ValidationMessage();
						newMsg.FieldName = "$NewArtifactId";
						newMsg.Message = riskId.ToString();
						AddUniqueMessage(validationMessages, newMsg);
					}

					//Add the ID for notifications to work
					((Risk)notificationArt).MarkAsAdded();
					((Risk)notificationArt).RiskId = riskId;
				}
				else
				{
					try
					{
						riskManager.Risk_Update(risk, userId, sendNotification: false);
					}
					catch (EntityForeignKeyException)
					{
						return CreateSimpleValidationMessage(Resources.Messages.Global_DependentArtifactDeleted);
					}
					catch (OptimisticConcurrencyException)
					{
						return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
					}
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
				}

				//See if we have a new comment encoded in the list of fields
				string notificationComment = null;
				if (dataItem.Fields.ContainsKey("NewComment") && riskId > 0)
				{
					string newComment = dataItem.Fields["NewComment"].TextValue;

					if (!String.IsNullOrWhiteSpace(newComment))
					{
						DiscussionManager discussionManager = new DiscussionManager();
						discussionManager.Insert(CurrentUserId.Value, riskId, Artifact.ArtifactTypeEnum.Risk, newComment, DateTime.UtcNow, projectId, false, false);
						notificationComment = newComment;
					}
				}

				//Send to Notification to see if we need to send anything out.
				if (notificationArt != null)
				{
					try
					{
						new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, notificationComment);
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Risk.");
					}
				}

				//Return back any messages. For success it should only contain a new artifact ID if we're inserting
				return validationMessages;
			}
			catch (ArtifactNotExistsException)
			{
				//Let the user know that the ticket no inter exists
				return CreateSimpleValidationMessage(String.Format(Resources.Messages.RisksService_RiskNotFound, riskId));
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns the list of workflow field states separate from the main retrieve (used when changing workflow only)
		/// </summary>
		/// <param name="typeId">The id of the current risk type</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="stepId">The id of the current step/status</param>
		/// <returns>The list of workflow states only</returns>
		public List<DataItemField> Form_RetrieveWorkflowFieldStates(int projectId, int typeId, int stepId)
		{
			const string METHOD_NAME = "Form_RetrieveWorkflowFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				List<DataItemField> dataItemFields = new List<DataItemField>();

				//Get the list of artifact fields and custom properties
				ArtifactManager artifactManager = new ArtifactManager();
				List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Risk);
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, false);

				//Get the list of workflow fields and custom properties for the specified type and step
				RiskWorkflowManager workflowManager = new RiskWorkflowManager();
				int workflowId = workflowManager.Workflow_GetForRiskType(typeId);
				List<RiskWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, stepId);
				List<RiskWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, stepId);

				//First the standard fields
				foreach (ArtifactField artifactField in artifactFields)
				{
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = artifactField.Name;
					dataItemFields.Add(dataItemField);

					//Set the workflow state
					//Specify which fields are editable or required
					dataItemField.Editable = true;
					dataItemField.Required = false;
					dataItemField.Hidden = false;
					if (workflowFields != null)
					{
						if (workflowFields.Any(w => w.ArtifactField.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
						{
							dataItemField.Editable = false;
						}
						if (workflowFields.Any(w => w.ArtifactField.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
						{
							dataItemField.Required = true;
						}
						if (workflowFields.Any(w => w.ArtifactField.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
						{
							dataItemField.Hidden = true;
						}
					}
				}

				//Now the custom properties
				foreach (CustomProperty customProperty in customProperties)
				{
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = customProperty.CustomPropertyFieldName;
					dataItemFields.Add(dataItemField);

					//Set the workflow state
					//Specify which fields are editable or required
					dataItemField.Editable = true;
					dataItemField.Required = false;
					dataItemField.Hidden = false;

					//First see if the custom property is required due to its definition
					if (customProperty.Options != null)
					{
						CustomPropertyOptionValue customPropOptionValue = customProperty.Options.FirstOrDefault(co => co.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty);
						if (customPropOptionValue != null)
						{
							bool? allowEmpty = customPropOptionValue.Value.FromDatabaseSerialization_Boolean();
							if (allowEmpty.HasValue)
							{
								dataItemField.Required = !allowEmpty.Value;
							}
						}
					}

					//Now check the workflow states
					if (workflowCustomProps != null)
					{
						if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyId == customProperty.CustomPropertyId && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
						{
							dataItemField.Editable = false;
						}
						if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyId == customProperty.CustomPropertyId && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
						{
							dataItemField.Required = true;
						}
						if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyId == customProperty.CustomPropertyId && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
						{
							dataItemField.Hidden = true;
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItemFields;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region ICommentService Methods

		/// <summary>
		/// Retrieves the list of comments associated with a risk
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the risk</param>
		/// <returns>The list of comments</returns>
		public List<CommentItem> Comment_Retrieve(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Comment_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Create the new list of comments
				List<CommentItem> commentItems = new List<CommentItem>();

				//Get the risk (to verify permissions) and also the comments
				RiskManager riskManager = new RiskManager();
				UserManager userManager = new UserManager();
				DiscussionManager discussion = new DiscussionManager();
				Risk risk = riskManager.Risk_RetrieveById(artifactId);
				List<IDiscussion> comments = discussion.Retrieve(artifactId, Artifact.ArtifactTypeEnum.Risk).ToList();

				//Make sure the user is either the owner or author if limited permissions
				int ownerId = -1;
				if (risk.OwnerId.HasValue)
				{
					ownerId = risk.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && risk.CreatorId != userId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//See if we're sorting ascending or descending
				SortDirection sortDirection = (SortDirection)GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)SortDirection.Descending);

				int startIndex;
				int increment;
				if (sortDirection == SortDirection.Ascending)
				{
					startIndex = 0;
					increment = 1;
				}
				else
				{
					startIndex = comments.Count - 1;
					increment = -1;
				}
				for (var i = startIndex; (increment == 1 && i < comments.Count) || (increment == -1 && i >= 0); i += increment)
				{
					IDiscussion discussionRow = comments[i];
					//Add a new comment
					CommentItem commentItem = new CommentItem();
					commentItem.primaryKey = discussionRow.DiscussionId;
					commentItem.text = discussionRow.Text;
					commentItem.creatorId = discussionRow.CreatorId;
					commentItem.creatorName = discussionRow.CreatorName;
					commentItem.creationDate = GlobalFunctions.LocalizeDate(discussionRow.CreationDate);
					commentItem.creationDateText = GlobalFunctions.LocalizeDate(discussionRow.CreationDate).ToNiceString(GlobalFunctions.LocalizeDate(DateTime.UtcNow));
					commentItem.sortDirection = (int)sortDirection;

					//Specify if the user can delete the item
					if (!discussionRow.IsPermanent && (discussionRow.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)))
					{
						commentItem.deleteable = true;
					}
					else
					{
						commentItem.deleteable = false;
					}

					commentItems.Add(commentItem);
				}

				//Return the comments
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return commentItems;
			}
			catch (ArtifactNotExistsException)
			{
				//The incident doesn't exist, so just return null
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Updates the sort direction of the comments list
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="sortDirectionId">The new direction for the sort</param>
		public void Comment_UpdateSortDirection(int projectId, int sortDirectionId)
		{
			const string METHOD_NAME = "Comment_UpdateSortDirection";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Update the setting
				SortDirection sortDirection = (SortDirection)sortDirectionId;
				SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)sortDirectionId);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Deletes a specific comment in the comment list
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="commentId">The id of the comment</param>
		/// <param name="artifactId">The id of the risk</param>
		public void Comment_Delete(int projectId, int artifactId, int commentId)
		{
			const string METHOD_NAME = "Comment_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Delete the comment, making sure we have permissions
				DiscussionManager discussion = new DiscussionManager();
				IDiscussion comment = discussion.RetrieveById(commentId, Artifact.ArtifactTypeEnum.Risk);
				//If the comment no longer exists do nothing
				if (comment != null && !comment.IsPermanent)
				{
					if (comment.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin))
					{
						discussion.DeleteDiscussionId(commentId, Artifact.ArtifactTypeEnum.Risk);
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
		/// Adds a comment to an artifact
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="comment">The comment being added</param>
		/// <returns>The id of the newly added comment</returns>
		public int Comment_Add(int projectId, int artifactId, string comment)
		{
			const string METHOD_NAME = "Comment_Add";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized to view the item (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Make sure we're allowed to add comments
			if (IsAuthorizedToAddComments(projectId) == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Add the comment
				string cleanedComment = GlobalFunctions.HtmlScrubInput(comment);
				DiscussionManager discussion = new DiscussionManager();
				int commentId = discussion.Insert(userId, artifactId, Artifact.ArtifactTypeEnum.Risk, cleanedComment, projectId, false, true);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return commentId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region IRiskService Methods

		/// <summary>
		/// Checks to see if an risk id exists and the cureent user has access to it (any project, or specific project)
		/// </summary>
		/// <param name="projectId">The current project (if specified)</param>
		/// <param name="riskId">The id of the risk</param>
		/// <returns>True if it exists</returns>
		public bool Risk_CheckExists(int? projectId, int riskId)
		{
			const string METHOD_NAME = "Risk_CheckExists";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			try
			{
				//Retrieve
				RiskManager riskManager = new RiskManager();
				RiskView riskView = riskManager.Risk_RetrieveById2(riskId);
				if (riskView == null)
				{
					return false;
				}

				//See if we have a project specified
				if (projectId.HasValue && projectId != riskView.ProjectId)
				{
					return false;
				}

				//Make sure we're authorized
				Project.AuthorizationState authorizationState = IsAuthorized(riskView.ProjectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
				if (authorizationState == Project.AuthorizationState.Prohibited)
				{
					return false;
				}
				return true;
			}
			catch (ArtifactNotExistsException)
			{
				return false;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Displays a graph of the count of risks by exposure
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The count by exposure</returns>
		public GraphData Risk_RetrieveCountByExposure(int projectId)
		{
			const string METHOD_NAME = "Risk_RetrieveCountByExposure";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (full view needed since it's showing the count of all risks)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				GraphData graphData = new GraphData();

				//Get the list of open risks for the project
				RiskManager riskManager = new RiskManager();
				Hashtable filters = new Hashtable();
				IntRange rangeFilter = new IntRange();
				rangeFilter.MinValue = 1;   //Want exposure > 0
				filters.Add("RiskStatusId", RiskManager.RiskStatusId_AllOpen);
				filters.Add("RiskExposure", rangeFilter);
				List<RiskView> openRisks = riskManager.Risk_Retrieve(projectId, "RiskExposure", true, 1, Int32.MaxValue, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());

				//Group by exposure
				IEnumerable<IGrouping<int?, RiskView>> exposures = openRisks.GroupBy(r => r.RiskExposure);

				foreach (IGrouping<int?, RiskView> exposure in exposures)
				{
					if (exposure.Key.HasValue)
					{
						RiskView risk = exposure.FirstOrDefault();
						if (risk != null && !String.IsNullOrEmpty(risk.RiskProbabilityColor) && !String.IsNullOrEmpty(risk.RiskImpactColor))
						{
							DataSeries dataSeries = new DataSeries();
							dataSeries.Value = exposure.Count();
							dataSeries.Caption = exposure.Key.Value.ToString();
							System.Drawing.Color backColor = GlobalFunctions.InterpolateColor2(risk.RiskProbabilityScore.HasValue ? risk.RiskProbabilityScore.Value : 1, risk.RiskImpactScore.HasValue ? risk.RiskImpactScore.Value : 1);
							string exposureColor = System.Drawing.ColorTranslator.ToHtml(backColor);
							dataSeries.Color = exposureColor; //GlobalFunctions.InterpolateColorAsHtml(risk.RiskImpactColor, risk.RiskProbabilityColor);

							graphData.Series.Add(dataSeries);
						}
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
				Logger.Flush();

				return graphData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		#endregion
	}
}
