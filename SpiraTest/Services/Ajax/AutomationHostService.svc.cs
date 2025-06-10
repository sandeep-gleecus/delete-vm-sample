using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Linq;
using System.Data;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the SortableGrid AJAX component for displaying/updating hosts
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class AutomationHostService : SortedListServiceBase, IAutomationHostService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.AutomationHostsService::";

        #region IFormService methods

        /// <summary>Returns a single automation host data record (all columns) for use by the FormManager control</summary>
        /// <param name="artifactId">The id of the current automation host</param>
        /// <returns>A automation host data item</returns>
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

            //Make sure we're authorized (full edit only)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.AutomationHost);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business classes
                AutomationManager automationManager = new AutomationManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, false);

                //Get the automation host for the specific automation host id
                AutomationHostView automationHost = automationManager.RetrieveHostById(artifactId.Value);

                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, true);

                //See if we have any existing artifact custom properties for this row
                if (artifactCustomProperty == null)
                {
                    List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, true, false);
                    PopulateRow(dataItem, automationHost, customProperties, true, (ArtifactCustomProperty)null);
                }
                else
                {
                    PopulateRow(dataItem, automationHost, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty);
                }

                //Populate any data mapping values are not part of the standard 'shape'
                if (artifactId.HasValue)
                {
                    DataMappingManager dataMappingManager = new DataMappingManager();
                    List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.AutomationHost, artifactId.Value);
                    foreach (DataSyncArtifactMapping artifactMappingRow in artifactMappings)
                    {
                        DataItemField dataItemField = new DataItemField();
                        dataItemField.FieldName = DataMappingManager.FIELD_PREPEND + artifactMappingRow.DataSyncSystemId;
                        dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
                        if (String.IsNullOrEmpty(artifactMappingRow.ExternalKey))
                        {
                            dataItemField.TextValue = "";
                        }
                        else
                        {
                            dataItemField.TextValue = artifactMappingRow.ExternalKey;
                        }
                        dataItemField.Editable = (SpiraContext.Current.IsProjectAdmin); //Read-only unless project admin
                        dataItemField.Hidden = false;   //Always visible
                        dataItem.Fields.Add(DataMappingManager.FIELD_PREPEND + artifactMappingRow.DataSyncSystemId, dataItemField);
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
        /// Deletes the current automation host and returns the ID of the item to redirect to (if any)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.AutomationHost);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //First we need to determine which host id to redirect the user to after the delete
                int? newAutomationHostId = null;

                //Look through the current dataset to see what is the next automation host in the list
                //If we are the last one on the list then we need to simply use the one before

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_FILTERS_LIST);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "AutomationHostId ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_PAGINATION_SIZE);
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
                //Get the number of automation hosts in the project
                AutomationManager automationManager = new AutomationManager();
                int artifactCount = automationManager.CountHosts(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                //Get the automation host list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }

                List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                bool matchFound = false;
                int previousAutomationHostId = -1;
                foreach (AutomationHostView automationHost in automationHosts)
                {
                    int testAutomationHostId = automationHost.AutomationHostId;
                    if (testAutomationHostId == artifactId)
                    {
                        matchFound = true;
                    }
                    else
                    {
                        //If we found a match on the previous iteration, then we want to this (next) task
                        if (matchFound)
                        {
                            newAutomationHostId = testAutomationHostId;
                            break;
                        }

                        //If this matches the current incident, set flag
                        if (testAutomationHostId == artifactId)
                        {
                            matchFound = true;
                        }
                        if (!matchFound)
                        {
                            previousAutomationHostId = testAutomationHostId;
                        }
                    }
                }
                if (!newAutomationHostId.HasValue && previousAutomationHostId != -1)
                {
                    newAutomationHostId = previousAutomationHostId;
                }

                //Next we need to delete the current automation host
                automationManager.MarkHostAsDeleted(projectId, artifactId, userId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return newAutomationHostId;
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

        /// <summary>Saves a single automation host data item</summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="dataItem">The automation host to save</param>
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

            //Make sure we're authorized (full only because automation hosts don't have owners)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.AutomationHost);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Get the automation host id
            int automationHostId = dataItem.PrimaryKey;

            try
            {
                //Instantiate the business classes
                AutomationManager automationManager = new AutomationManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Load the custom property definitions (once, not per artifact)
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, false);

                //This service only supports updates, so we should get a automation host id that is valid

                //Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
                AutomationHost automationHost = automationManager.RetrieveHostById2(automationHostId);

                //Create a new artifact custom property row if one doesn't already exist
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, automationHostId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, false, customProperties);
                if (artifactCustomProperty == null)
                {
                    artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.AutomationHost, automationHostId, customProperties);
                }
                else
                {
                    artifactCustomProperty.StartTracking();
                }

                //Need to set the original date of this record to match the concurrency date
                //The value is already in UTC so no need to convert
                if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                {
                    DateTime concurrencyDateTimeValue;
                    if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                    {
                        automationHost.ConcurrencyDate = concurrencyDateTimeValue;
                        automationHost.AcceptChanges();
                    }
                }

                //Now we can start tracking any changes
                automationHost.StartTracking();

                //Update the field values, tracking changes
                List<string> fieldsToIgnore = new List<string>();
                fieldsToIgnore.Add("CreationDate");
                fieldsToIgnore.Add("LastUpdateDate");

                //Need to handle any data-mapping fields (project-admin only)
                if (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)
                {
                    DataMappingManager dataMappingManager = new DataMappingManager();
                    List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.AutomationHost, automationHostId);
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
                UpdateFields(validationMessages, dataItem, automationHost, customProperties, artifactCustomProperty, projectId, automationHostId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, fieldsToIgnore);

                //Now verify the options for the custom properties to make sure all rules have been followed
                Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
                foreach (KeyValuePair<string, string> customPropOptionMessage in customPropOptionMessages)
                {
                    ValidationMessage newMsg = new ValidationMessage();
                    newMsg.FieldName = customPropOptionMessage.Key;
                    newMsg.Message = customPropOptionMessage.Value;
                    AddUniqueMessage(validationMessages, newMsg);
                }

                //If we have validation messages, stop now
                if (validationMessages.Count > 0)
                {
                    return validationMessages;
                }

                //Update the automation host and any custom properties
                try
                {
                    automationManager.UpdateHost(automationHost, userId);
                }
			    catch (EntityConstraintViolationException)
			    {
                    return CreateSimpleValidationMessage(Resources.Messages.AutomationHostService_AutomationHostTokenNotUnique);
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

                //If we're asked to save and create a new automation host, need to do the insert and send back the new id
                if (operation == "new")
                {
                    //Simply insert the new item into the automation host list
                    //Need to generate a new unique token each time
                    List<AutomationHostView> existingHosts = automationManager.RetrieveHosts(projectId, true);
                    int suffix = existingHosts.Count;
                    bool matchFound = false;
                    do
                    {
                        matchFound = false;
                        foreach (AutomationHostView hostRow in existingHosts)
                        {
                            if (hostRow.Token == Resources.Dialogs.Global_NewHost + " " + suffix)
                            {
                                matchFound = true;
                                suffix++;
                                break;
                            }
                        }
                    }
                    while (matchFound);

                    string newToken = Resources.Dialogs.Global_NewHost + " " + suffix;
                    int newAutomationHostId = automationManager.InsertHost(
                        projectId,
                        "",
                        newToken,
                        null,
                        true,
                        userId
                        );

                    //We need to populate any custom property default values
                    artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.AutomationHost, newAutomationHostId, customProperties);
                    artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                    customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

                    //We need to encode the new artifact id as a 'pseudo' validation message
                    ValidationMessage newMsg = new ValidationMessage();
                    newMsg.FieldName = "$NewArtifactId";
                    newMsg.Message = newAutomationHostId.ToString();
                    AddUniqueMessage(validationMessages, newMsg);
                }

                //Return back any messages. For success it should only contain a new artifact ID if we're inserting
                return validationMessages;
            }
            catch (ArtifactNotExistsException)
            {
                //Let the user know that the ticket no inter exists
                return CreateSimpleValidationMessage(String.Format(Resources.Messages.AutomationHostsService_AutomationHostNotFound, automationHostId));
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        public List<DataItemField> Form_RetrieveWorkflowFieldStates(int projectId, int typeId, int stepId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region SortedList Methods

        /// <summary>
        /// Updates records of data in the system
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="dataItems">The updated data records</param>
        /// <returns>Any validation messages</returns>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.AutomationHost);
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
                AutomationManager automationManager = new AutomationManager();

                //Load the custom property definitions (once, not per artifact)
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, false);

                foreach (SortedDataItem dataItem in dataItems)
                {
                    //Get the automationHost id
                    int automationHostId = dataItem.PrimaryKey;

                    //Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
                    AutomationHost automationHost = automationManager.RetrieveHostById2(automationHostId);
                    ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, automationHostId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, false, customProperties);

                    //Create a new artifact custom property row if one doesn't already exist
                    if (artifactCustomProperty == null)
                    {
                        artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.AutomationHost, automationHostId, customProperties);
                    }
                    else
                    {
                        artifactCustomProperty.StartTracking();
                    }

                    if (automationHost != null)
                    {
                        //Need to set the original date of this record to match the concurrency date
                        if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                        {
                            DateTime concurrencyDateTimeValue;
                            if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                            {
                                automationHost.ConcurrencyDate = concurrencyDateTimeValue;
                                automationHost.AcceptChanges();
                            }
                        }

                        //Update the field values
                        List<string> fieldsToIgnore = new List<string>();
                        fieldsToIgnore.Add("CreationDate");
                        UpdateFields(validationMessages, dataItem, automationHost, customProperties, artifactCustomProperty, projectId, automationHostId, 0, fieldsToIgnore);

                        //Now verify the options for the custom properties to make sure all rules have been followed
                        Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
                        foreach (KeyValuePair<string, string> customPropOptionMessage in customPropOptionMessages)
                        {
                            ValidationMessage newMsg = new ValidationMessage();
                            newMsg.FieldName = customPropOptionMessage.Key;
                            newMsg.Message = customPropOptionMessage.Value;
                            AddUniqueMessage(validationMessages, newMsg);
                        }

                        //Make sure we have no validation messages before updating
                        if (validationMessages.Count == 0)
                        {
                            //Persist to database, catching any business exceptions and displaying them
                            try
                            {
                                automationManager.UpdateHost(automationHost, userId);
                            }
                            catch (DataValidationException exception)
                            {
                                return CreateSimpleValidationMessage(exception.Message);
                            }
                            catch (OptimisticConcurrencyException)
                            {
                                return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
                            }
                            catch (EntityConstraintViolationException)
                            {
                                return CreateSimpleValidationMessage(Resources.Messages.AutomationHostService_AutomationHostTokenNotUnique);
                            }
                            customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
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
        /// Returns a list of automationHosts in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the automationHosts as</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.AutomationHost);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the automation and custom property business objects
                AutomationManager automationManager = new AutomationManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the array of data items (including the first filter item)
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;

                //Now get the list of populated filters and the current sort
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_FILTERS_LIST);
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "AutomationHostId ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");
                sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost);

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

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_PAGINATION_SIZE);
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
                //Get the number of automation hosts in the project
                int artifactCount = automationManager.CountHosts(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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
                List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //Display the pagination information
                sortedData.CurrPage = currentPage;
                sortedData.PageCount = pageCount;
                sortedData.StartRow = startRow;

                //Display the visible and total count of artifacts
                sortedData.VisibleCount = automationHosts.Count;
                sortedData.TotalCount = artifactCount;

                //Display the sort information
                sortedData.SortProperty = sortProperty;
                sortedData.SortAscending = sortAscending;

                //Now get the list of custom property options and lookup values for this artifact type / project
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, true, false, true);

                //Iterate through all the automation hosts and populate the dataitem
                foreach (AutomationHostView automationHost in automationHosts)
                {
                    //We clone the template item as the basis of all the new items
                    SortedDataItem dataItem = filterItem.Clone();

                    //Now populate with the data
                    PopulateRow(dataItem, automationHost, customProperties, false, null);
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
        /// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
        /// </summary>
        /// <param name="automationHostId">The id of the automationHost to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        public string RetrieveNameDesc(int? projectId, int automationHostId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the automation business object
                AutomationManager automationManager = new AutomationManager();

                //Now retrieve the specific automationHost - handle quietly if it doesn't exist
                try
                {
                    AutomationHostView automationHost = automationManager.RetrieveHostById(automationHostId);
                    string tooltip;
                    if (String.IsNullOrEmpty(automationHost.Description))
                    {
                        tooltip = GlobalFunctions.HtmlRenderAsPlainText(automationHost.Name);
                    }
                    else
                    {
                        tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(automationHost.Name) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(automationHost.Description);
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the automationHost, but it has already been deleted on the server
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for automation host");
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

            //Call the base method with the appropriate settings collection
            return base.UpdateSort(userId, projectId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION);
        }

        #endregion

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
            return base.UpdateFilters(userId, projectId, filters, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.AutomationHost);
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

            return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_FILTERS_LIST, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION, isShared, existingSavedFilterId, includeColumns);
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
            return base.RetrieveFilters(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, includeShared);
        }

        /// <summary>
        /// Returns the latest information on a single automationHost in the system
        /// </summary>
        /// <param name="userId">The user we're viewing the automationHost as</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.AutomationHost);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the automation and custom property business objects
                AutomationManager automationManager = new AutomationManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

                //Get the automationHost entity record for the specific automationHost id
                AutomationHostView automationHost = automationManager.RetrieveHostById(artifactId);

                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, true);

                //Finally populate the dataitem from the dataset
                if (automationHost != null)
                {
                    //See if we already have an artifact custom property row
                    if (artifactCustomProperty != null)
                    {
                        PopulateRow(dataItem, automationHost, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty);
                    }
                    else
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, true, false);
                        PopulateRow(dataItem, automationHost, customProperties, true, null);
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
                    customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, fieldName);
                }
                else
                {
                    //Toggle the status of the appropriate field name
                    ArtifactManager artifactManager = new ArtifactManager();
                    artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, fieldName);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

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
                artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, fieldName, width);
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
        /// Changes the order of columns in the automation host list
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
                artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, fieldName, newPosition);
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

        /// <summary>
        /// Inserts a new automationHost into the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="artifact">The type of artifact we're inserting</param>
        /// <param name="standardFilters">Any standard filters that are set by the page</param>
        /// <returns>The id of the new automationHost</returns>
        public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.AutomationHost);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business object(s)
                AutomationManager automationManager = new AutomationManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Simply insert the new item into the automation host list
                //Need to generate a new unique token each time
                List<AutomationHostView> existingHosts = automationManager.RetrieveHosts(projectId, true);
                int suffix = existingHosts.Count;
                bool matchFound = false;
                do
                {
                    matchFound = false;
                    foreach (AutomationHostView hostRow in existingHosts)
                    {
                        if (hostRow.Token == Resources.Dialogs.Global_Host + " " + suffix)
                        {
                            matchFound = true;
                            suffix++;
                            break;
                        }
                    }
                }
                while (matchFound);

                string newToken = Resources.Dialogs.Global_Host + " " + suffix;
                int automationHostId = automationManager.InsertHost(
                    projectId,
                    "",
                    newToken,
                    null,
                    true,
                    userId
                    );

                //We now need to populate the appropriate default custom properties
                AutomationHost automationHost = automationManager.RetrieveHostById2(automationHostId);
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, automationHostId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, true);
                if (automationHost != null)
                {
                    //If the artifact custom property row is null, create a new one and populate the defaults
                    if (artifactCustomProperty == null)
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, false);
                        artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.AutomationHost, automationHostId, customProperties);
                        artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                    }
                    else
                    {
                        artifactCustomProperty.StartTracking();
                    }

                    //If we have filters currently applied to the view, then we need to set this new automationHost to the same value
                    //(if possible) so that it will show up in the list
                    ProjectSettingsCollection filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_FILTERS_LIST);
                    if (filterList.Count > 0)
                    {
                        List<string> fieldsToIgnore = new List<string>() { "AutomationHostId", "CreationDate", "Token" };
                        UpdateToMatchFilters(projectId, filterList, automationHostId, automationHost, artifactCustomProperty, fieldsToIgnore);
                        automationManager.UpdateHost(automationHost, userId);
                    }

                    //Save the custom properties
                    customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
                }

                return automationHostId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Deletes a set of automationHosts
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.AutomationHost);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Iterate through all the items to be deleted
                AutomationManager automationManager = new AutomationManager();
                foreach (string itemValue in items)
                {
                    //Get the automation Host ID
                    int automationHostId = Int32.Parse(itemValue);
                    automationManager.MarkHostAsDeleted(projectId, automationHostId, userId);
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
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_PAGINATION_SIZE);

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
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_PAGINATION_SIZE);
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
        /// Populates a data item from an automation host entity
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="dataRow">The datarow containing the data</param>
        /// <param name="customProperties">The list of custom property definitions and values</param>
        /// <param name="editable">Does the data need to be in editable form?</param>
        /// <param name="artifactCustomProperty">The artifact's custom property data (if not provided as part of dataitem) - pass null if not used</param>
        protected void PopulateRow(SortedDataItem dataItem, AutomationHostView automationHost, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = automationHost.AutomationHostId;
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, automationHost.ConcurrencyDate);

            //Specify if it has an attachment or not
            dataItem.Attachment = automationHost.IsAttachments;

            //The date and some other fields are not editable
            List<string> readOnlyFields = new List<string>() { "CreationDate", "LastUpdateDate" };

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (automationHost.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, automationHost, customProperties, artifactCustomProperty, editable, null, null, null, readOnlyFields);
                }
            }
        }

        /// <summary>
        /// Gets the list of lookup values and names for a specific lookup
        /// </summary>
        /// <param name="lookupName">The name of the lookup</param>
        /// <param name="projectTemplateId">the id of the project template</param>
        /// <param name="projectId">The id of the project - needed for some lookups</param>
        /// <returns>The name/value pairs</returns>
        protected JsonDictionaryOfStrings GetLookupValues(string lookupName, int projectId, int projectTemplateId)
        {
            const string METHOD_NAME = "GetLookupValues";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                JsonDictionaryOfStrings lookupValues = null;

                if (lookupName == "IsActive")
                {
                    AutomationManager automationManager = new AutomationManager();
                    lookupValues = new JsonDictionaryOfStrings(automationManager.RetrieveFlagLookupDictionary());
                }

                //The custom property lookups
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
                if (customPropertyNumber.HasValue)
                {
                    CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, customPropertyNumber.Value, true);
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
        /// <param name="projectTemplateId">the id of the project template</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the automationHosts as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        /// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
        protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
        {
            //We need to dynamically add the various columns from the field list
            LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
            DataItemField dataItemField = new DataItemField();
            AddDynamicColumns(Artifact.ArtifactTypeEnum.AutomationHost, getLookupValues, projectId, projectTemplateId, userId, dataItem, filterList, null, returnJustListFields);
        }

        #region Not Implemented Methods

        public void SortedList_Export(int destProjectId, List<string> items)
        {
            throw new NotImplementedException();
        }
        public void SortedList_Copy(int projectId, List<string> items)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region INavigationService interface

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
        /// Returns a list of hosts for display in the navigation bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="indentLevel">Not used for hosts since not hierarchical</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.AutomationHost);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business object
                AutomationManager automationManager = new AutomationManager();

                //Create the array of data items
                List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_FILTERS_LIST);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "AutomationHostId ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_PAGINATION_SIZE);
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
                //Get the number of hosts in the project
                int artifactCount = automationManager.CountHosts(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //**** Now we need to actually populate the rows of data to be returned ****

                //Get the incidents list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }
                List<AutomationHostView> automationHosts;
                if (displayMode == 2)
                {
                    //All Items
                    automationHosts = automationManager.RetrieveHosts(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }
                else
                {
                    //Filtered List
                    automationHosts = automationManager.RetrieveHosts(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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

                //Iterate through all the hosts and populate the dataitem (only some columns are needed)
                foreach (AutomationHostView automationHostView in automationHosts)
                {
                    //Create the data-item
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();

                    //Populate the necessary fields
                    dataItem.PrimaryKey = automationHostView.AutomationHostId;
                    dataItem.Indent = "";
                    dataItem.Expanded = false;

                    //Name/Desc
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = automationHostView.Name;
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
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION);
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
    }
}
