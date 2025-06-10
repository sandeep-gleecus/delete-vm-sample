using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Data;


using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;


namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the OrderedGrid AJAX component for displaying/updating the list of mitigations in a risk
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class RiskMitigationService : ListServiceBase, IRiskMitigationService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.RiskMitigationService::";

        #region IOrderedList Interface

        /// <summary>
        /// Returns a list of steps in the use case
        /// </summary>
        /// <param name="userId">The user we're viewing the steps as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for RiskId</param>
        /// <returns>Collection of JS serializable dataitems</returns>
        public OrderedData OrderedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters)
        {
            const string METHOD_NAME = "Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited is OK)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Risk);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                RiskManager riskManager = new RiskManager();

                //Create the array of data items
                OrderedData orderedData = new OrderedData();
                List<OrderedDataItem> dataItems = orderedData.Items;

                //Create the first 'shape' item, we can clone others from it later
                OrderedDataItem shapeItem = new OrderedDataItem();
                PopulateShape(projectId, userId, shapeItem);
                dataItems.Add(shapeItem);

                //The risk id needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RiskId"))
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                int riskId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RiskId"]);

                //Now get the pagination information and add to the shape item
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_STEP_GENERAL_SETTINGS);
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

                //**** Now we need to actually populate the rows of data to be returned ****

                //Get the list of steps in the use case (risk)
                List<RiskMitigation> riskMitigations = riskManager.RiskMitigation_Retrieve(riskId);
                int artifactCount = riskMitigations.Count;

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RiskView risk = riskManager.Risk_RetrieveById2(riskId);
                    if (risk.OwnerId != userId && risk.CreatorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }

                //Calculate the number of pages
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
                orderedData.CurrPage = currentPage;
                orderedData.PageCount = pageCount;

                //Iterate through all the risk steps in the pagination range populate the dataitem
                int visibleCount = 0;
                for (int i = startRow - 1; i < startRow + paginationSize - 1 && i < artifactCount; i++)
                {
                    RiskMitigation riskMitigation = riskMitigations[i];

                    //We clone the template/shape item as the basis of all the new items
                    OrderedDataItem dataItem = shapeItem.Clone();

                    //The entity doesn't contain the position, but it's ordered by position, so can get it indirectly
                    int displayPosition = i + 1;

                    //Now populate with the data
                    PopulateRow(projectId, dataItem, riskMitigation, false, displayPosition);
                    dataItems.Add(dataItem);
                    visibleCount++;
                }

                //Display the visible and total count of artifacts
                orderedData.VisibleCount = visibleCount;
                orderedData.TotalCount = artifactCount;

                //Add the pagination information
                orderedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created data items with " + dataItems.Count.ToString() + " rows");

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return orderedData;
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
        /// <param name="userId">The user we're viewing the test steps as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
        protected void PopulateShape(int projectId, int userId, OrderedDataItem dataItem, bool returnJustListFields = true)
        {
            //First add the static columns (always present) that occur before the dynamic ones
            DataItemField dataItemField = new DataItemField();
            dataItemField.FieldName = "Position";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
            dataItemField.Caption = Resources.Fields.Position;
            dataItemField.Editable = false;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //Description
            dataItemField = new DataItemField();
            dataItemField.FieldName = "Description";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
            dataItemField.Caption = Resources.Fields.Description;
            dataItemField.Editable = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //ReviewDate
            dataItemField = new DataItemField();
            dataItemField.FieldName = "ReviewDate";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
            dataItemField.Caption = Resources.Fields.ReviewDate;
            dataItemField.Editable = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //CreationDate
            dataItemField = new DataItemField();
            dataItemField.FieldName = "CreationDate";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
            dataItemField.Caption = Resources.Fields.CreationDate;
            dataItemField.Editable = false;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //LastUpdateDate
            dataItemField = new DataItemField();
            dataItemField.FieldName = "LastUpdateDate";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
            dataItemField.Caption = Resources.Fields.LastUpdateDate;
            dataItemField.Editable = false;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //There are no dynamic columns for risk steps

            //Finally add the static columns (always present) that occur after the dynamic ones
            //Risk Step Id ID
            dataItemField = new DataItemField();
            dataItemField.FieldName = "RiskMitigationId";
            dataItemField.Caption = Resources.Fields.ID;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
        }

        /// <summary>
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="editable">Does the data need to be in editable form?</param>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="riskMitigation">The entity containing the data</param>
        /// <param name="displayPosition">The display position of the datarow</param>
        /// <param name="projectId">The id of the current project</param>
        protected void PopulateRow(int projectId, OrderedDataItem dataItem, RiskMitigation riskMitigation, bool editable, int displayPosition)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = riskMitigation.RiskMitigationId;
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, riskMitigation.ConcurrencyDate);
            dataItem.Alternate = true;  //Since steps don't have hyperlinks, mark all of them as 'alternate'

            //Specify its position
            dataItem.Position = riskMitigation.Position;

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (riskMitigation.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, riskMitigation, null, null, editable, null);

                    //Specify which fields are editable or not
                    //Unless specified, all fields are editable
                    dataItemField.Editable = true;

                    //The position and several date fields field are not editable
                    if (fieldName == "Position" || fieldName == "CreationDate" || fieldName == "LastUpdateDate")
                    {
                        dataItemField.Editable = false;
                    }

                    //Certain fields do not allow null values
                    if (fieldName == "Description")
                    {
                        dataItemField.Required = true;
                    }

                    //The position needs to be specified as "Step X"
                    if (dataItemField.FieldName == "Position")
                    {
                        dataItemField.TextValue = displayPosition.ToString();
                    }

                    //Apply the conditional formatting to the Review Date column (if displayed)
                    if (dataItemField.FieldName == "ReviewDate" && riskMitigation.ReviewDate.HasValue && riskMitigation.ReviewDate.Value < DateTime.UtcNow)
                    {
                        dataItemField.CssClass = "Warning";
                    }
                }
            }
        }

        /// <summary>
        /// Updates records of data in the system
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="dataItems">The updated data records</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for RiskId</param>
        /// <returns>List of any validation messages</returns>
        public List<ValidationMessage> OrderedList_Update(int projectId, JsonDictionaryOfStrings standardFilters, List<OrderedDataItem> dataItems)
        {
            const string METHOD_NAME = "OrderedList_Update";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Risk);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Used to store any validation messages
            List<ValidationMessage> validationMessages = new List<ValidationMessage>();

            try
            {
                //Instantiate the business objects
                RiskManager riskManager = new RiskManager();

                //The risk needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RiskId"))
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                int riskId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RiskId"]);


                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RiskView risk = riskManager.Risk_RetrieveById2(riskId);
                    if (risk.OwnerId != userId && risk.CreatorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Iterate through each data item and make the updates
                foreach (OrderedDataItem dataItem in dataItems)
                {
                    //Get the risk step id
                    int riskMitigationId = dataItem.PrimaryKey;

                    //Locate the existing record - and make sure it still exists
                    RiskMitigation riskMitigation = riskManager.RiskMitigation_RetrieveById(riskMitigationId);
                    if (riskMitigation == null)
                    {
                        throw new ArtifactNotExistsException("Unable to locate risk mitigation " + riskMitigationId + " in the project. It no longer exists!");
                    }

                    //Make sure this step belongs to the specified risk (in case we have a spoofed risk id)
                    if (riskMitigation.RiskId == riskId)
                    {
                        //Need to set the original date of this record to match the concurrency date
                        if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                        {
                            DateTime concurrencyDateTimeValue;
                            if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                            {
                                riskMitigation.ConcurrencyDate = concurrencyDateTimeValue;
                                riskMitigation.AcceptChanges();
                            }
                        }

                        //Now we can start tracking any changes
                        riskMitigation.StartTracking();

                        //Update the field values
                        List<string> fieldsToIgnore = new List<string>();
                        UpdateFields(validationMessages, dataItem, riskMitigation, null, null, projectId, riskMitigationId, DataModel.Artifact.ArtifactTypeEnum.None);

                        //Make sure we have no validation messages before updating
                        if (validationMessages.Count == 0)
                        {
                            //Persist to database
                            try
                            {
                                riskManager.RiskMitigation_Update(projectId, riskMitigation, userId);
                            }
                            catch (OptimisticConcurrencyException)
                            {
                                return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
                            }
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
        /// Inserts a new risk step into the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="artifactId">The id of the existing risk step we're inserting in front of (null for none)</param>
        /// <param name="artifact">The type of artifact we're inserting (Step, Link, etc.)</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for RiskId</param>
        /// <returns>The id of the new risk step</returns>
        public int OrderedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? artifactId)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Risk);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The risk needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RiskId"))
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                int riskId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RiskId"]);

                //Instantiate the business objects
                RiskManager riskManager = new RiskManager();

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RiskView risk = riskManager.Risk_RetrieveById2(riskId);
                    if (risk.OwnerId != userId && risk.CreatorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                int riskMitigationId = riskManager.RiskMitigation_Insert(projectId, riskId, artifactId, null, userId);
                return riskMitigationId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Deletes the selected steps from the specified use case
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items being removed</param>
        /// <param name="standardFilters">The id of the risk the steps belong to passed in as a filter (key = 'RiskId')</param>
        public void OrderedList_Delete(int projectId, JsonDictionaryOfStrings standardFilters, List<string> items)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Risk);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                RiskManager riskManager = new RiskManager();

                //The risk needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RiskId"))
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                int riskId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RiskId"]);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RiskView risk = riskManager.Risk_RetrieveById2(riskId);
                    if (risk.OwnerId != userId && risk.CreatorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Iterate through all the items to be removed
                foreach (string itemValue in items)
                {
                    //Get the risk step ID
                    int riskMitigationId = Int32.Parse(itemValue);
                    try
                    {
                        riskManager.RiskMitigation_Delete(projectId, riskMitigationId, userId);
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore any errors due to the step no longer existing
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
        /// Changes the position of a step in the use case
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sourceItems">The items to move</param>
        /// <param name="destRiskMitigationId">The destination item's id (or null for no destination selected)</param>
        /// <param name="standardFilters">The id of the risk the steps belong to passed in as a filter (key = 'RiskId')</param>
        public void OrderedList_Move(int projectId, JsonDictionaryOfStrings standardFilters, List<string> sourceItems, int? destRiskMitigationId)
        {
            const string METHOD_NAME = "OrderedList_Move";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Risk);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                RiskManager riskManager = new RiskManager();

                //The risk needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RiskId"))
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                int riskId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RiskId"]);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RiskView risk = riskManager.Risk_RetrieveById2(riskId);
                    if (risk.OwnerId != userId && risk.CreatorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Iterate through all the items to be moved and perform the operation
                foreach (string itemValue in sourceItems)
                {
                    //Get the source ID
                    int sourceRiskMitigationId = Int32.Parse(itemValue);
                    riskManager.RiskMitigation_Move(riskId, sourceRiskMitigationId, destRiskMitigationId, userId);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns the latest information on a single use case step in the system
        /// </summary>
        /// <param name="riskMitigationId">The id of the particular use case step we want to retrieve</param>
        /// <param name="userId">The user we're viewing the test cases/steps as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for RiskId</param>
        /// <returns>A single dataitem object</returns>
        public OrderedDataItem OrderedList_Refresh(int projectId, JsonDictionaryOfStrings standardFilters, int riskMitigationId)
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
                //Instantiate the business objects
                RiskManager riskManager = new RiskManager();

                //The risk needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RiskId"))
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                int riskId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RiskId"]);

                //Create the data item record
                OrderedDataItem dataItem = new OrderedDataItem();
                PopulateShape(projectId, userId, dataItem);

                //Get the use case step record for the specific test case and risk step id
                RiskMitigation riskMitigation = riskManager.RiskMitigation_RetrieveById(riskMitigationId);
                if (riskMitigation == null)
                {
                    throw new ArtifactNotExistsException("Unable to locate risk step " + riskMitigationId + " in the project. It no longer exists!");
                }

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RiskView risk = riskManager.Risk_RetrieveById2(riskMitigation.RiskId);
                    if (risk.OwnerId != userId && risk.CreatorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Finally populate the dataitem from the dataset
                PopulateRow(projectId, dataItem, riskMitigation, true, riskMitigation.Position);
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
        /// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
        /// </summary>
        /// <param name="riskMitigationId">The id of the risk step to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        public string RetrieveNameDesc(int? projectId, int riskMitigationId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the risk business object
                RiskManager riskManager = new RiskManager();

                //Now retrieve the specific test step - handle quietly if it doesn't exist
                RiskMitigation riskMitigation = riskManager.RiskMitigation_RetrieveById(riskMitigationId);
                if (riskMitigation == null)
                {
                    return Resources.Messages.Global_UnableRetrieveTooltip;
                }

                string tooltip = GlobalFunctions.HtmlRenderAsPlainText(riskMitigation.Description);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return tooltip;
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
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_STEP_GENERAL_SETTINGS);

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
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_STEP_GENERAL_SETTINGS);
                paginationSettings.Restore();
                if (pageSize != -1)
                {
                    paginationSettings["PaginationOption"] = pageSize;
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
        /// Clones/copies a selection of risk steps
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for RiskId</param>
        /// <param name="items">The items being copied/cloned</param>
        public void OrderedList_Copy(int projectId, JsonDictionaryOfStrings standardFilters, List<string> items)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Risk);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The risk needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RiskId"))
                {
                    throw new ArgumentException("You need to provide a RiskId as a standard filter");
                }
                int riskId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RiskId"]);


                //Instantiate the business objects
                RiskManager riskManager = new RiskManager();

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RiskView risk = riskManager.Risk_RetrieveById2(riskId);
                    if (risk.OwnerId != userId && risk.CreatorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Iterate through all the items to be copied
                foreach (string itemValue in items)
                {
                    //Get the risk step ID
                    int riskMitigationId = Int32.Parse(itemValue);
                    try
                    {
                        riskManager.RiskMitigation_Copy(userId, projectId, riskMitigationId);
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore any errors due to the risk mitigation no longer existing
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region Not Implemented Methods

        public void ToggleColumnVisibility(int projectId, string fieldName)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
