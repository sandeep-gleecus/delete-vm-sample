using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;
using System.Net;
using System.Data;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the OrderedGrid AJAX component for displaying/updating the list of steps in a use case
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class RequirementStepService : ListServiceBase, IRequirementStepService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.RequirementStepService::";

        #region IOrderedList Interface

        /// <summary>
        /// Returns a list of steps in the use case
        /// </summary>
        /// <param name="userId">The user we're viewing the steps as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for RequirementId</param>
        /// <returns>Collection of JS serializable dataitems</returns>
        public OrderedData OrderedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters)
        {
            const string METHOD_NAME = "OrderedList_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited is OK)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                RequirementManager requirementManager = new RequirementManager();

                //Create the array of data items
                OrderedData orderedData = new OrderedData();
                List<OrderedDataItem> dataItems = orderedData.Items;

                //Create the first 'shape' item, we can clone others from it later
                OrderedDataItem shapeItem = new OrderedDataItem();
                PopulateShape(projectId, userId, shapeItem);
                dataItems.Add(shapeItem);

                //The requirement id needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RequirementId"))
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                int requirementId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RequirementId"]);

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

                //Get the list of steps in the use case (requirement)
                List<RequirementStep> requirementSteps = requirementManager.RetrieveSteps(requirementId);
                int artifactCount = requirementSteps.Count;

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);
                    if (requirement.OwnerId != userId && requirement.AuthorId != userId)
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

                //Iterate through all the requirement steps in the pagination range populate the dataitem
                int visibleCount = 0;
                for (int i = startRow - 1; i < startRow + paginationSize - 1 && i < artifactCount; i++)
                {
                    RequirementStep requirementStep = requirementSteps[i];

                    //We clone the template/shape item as the basis of all the new items
                    OrderedDataItem dataItem = shapeItem.Clone();

                    //The entity doesn't contain the position, but it's ordered by position, so can get it indirectly
                    int displayPosition = i + 1;

                    //Now populate with the data
                    PopulateRow(projectId, dataItem, requirementStep, false, displayPosition);
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
            dataItemField.Caption = Resources.Fields.Step;
            dataItemField.Editable = false;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Description
            dataItemField = new DataItemField();
            dataItemField.FieldName = "Description";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
            dataItemField.Caption = Resources.Fields.Description;
            dataItemField.Editable = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //There are no dynamic columns for requirement steps

            //Finally add the static columns (always present) that occur after the dynamic ones
            //Requirement Step Id ID
            dataItemField = new DataItemField();
            dataItemField.FieldName = "RequirementStepId";
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
        /// <param name="requirementStep">The entity containing the data</param>
        /// <param name="displayPosition">The display position of the datarow</param>
        /// <param name="projectId">The id of the current project</param>
        protected void PopulateRow(int projectId, OrderedDataItem dataItem, RequirementStep requirementStep, bool editable, int displayPosition)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = requirementStep.RequirementStepId;
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, requirementStep.ConcurrencyDate);
            dataItem.Alternate = true;  //Since steps don't have hyperlinks, mark all of them as 'alternate'

            //Specify its position
            dataItem.Position = requirementStep.Position;

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (requirementStep.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, requirementStep, null, null, editable, null);

                    //Specify which fields are editable or not
                    //Unless specified, all fields are editable
                    dataItemField.Editable = true;

                    //The position field is not editable
                    if (fieldName == "Position")
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
                        dataItemField.TextValue = Resources.Fields.Step + "\u00a0" + displayPosition;
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
        /// <param name="standardFilters">A standard filters collection that contains a value for RequirementId</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Used to store any validation messages
            List<ValidationMessage> validationMessages = new List<ValidationMessage>();

            try
            {
                //Instantiate the business objects
                RequirementManager requirementManager = new RequirementManager();

                //The requirement needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RequirementId"))
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                int requirementId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RequirementId"]);


                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);
                    if (requirement.OwnerId != userId && requirement.AuthorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Iterate through each data item and make the updates
                foreach (OrderedDataItem dataItem in dataItems)
                {
                    //Get the requirement step id
                    int requirementStepId = dataItem.PrimaryKey;

                    //Locate the existing record - and make sure it still exists
                    RequirementStep requirementStep = requirementManager.RetrieveStepById(requirementStepId);
                    if (requirementStep == null)
                    {
                        throw new ArtifactNotExistsException("Unable to locate test step " + requirementStepId + " in the project. It no longer exists!");
                    }

                    //Make sure this step belongs to the specified requirement (in case we have a spoofed requirement id)
                    if (requirementStep.RequirementId == requirementId)
                    {
                        //Need to set the original date of this record to match the concurrency date
                        if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                        {
                            DateTime concurrencyDateTimeValue;
                            if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                            {
                                requirementStep.ConcurrencyDate = concurrencyDateTimeValue;
                                requirementStep.AcceptChanges();
                            }
                        }

                        //Now we can start tracking any changes
                        requirementStep.StartTracking();

                        //Update the field values
                        List<string> fieldsToIgnore = new List<string>();
                        UpdateFields(validationMessages, dataItem, requirementStep, null, null, projectId, requirementStepId, DataModel.Artifact.ArtifactTypeEnum.None);

                        //Make sure we have no validation messages before updating
                        if (validationMessages.Count == 0)
                        {
                            //Persist to database
                            try
                            {
                                requirementManager.UpdateStep(projectId, requirementStep, userId);
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
        /// Inserts a new requirement step into the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="artifactId">The id of the existing requirement step we're inserting in front of (null for none)</param>
        /// <param name="artifact">The type of artifact we're inserting (Step, Link, etc.)</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for RequirementId</param>
        /// <returns>The id of the new requirement step</returns>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The requirement needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RequirementId"))
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                int requirementId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RequirementId"]);

                //Instantiate the business objects
                RequirementManager requirementManager = new RequirementManager();

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);
                    if (requirement.OwnerId != userId && requirement.AuthorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                int requirementStepId = requirementManager.InsertStep(projectId, requirementId, artifactId, null, userId);
                return requirementStepId;
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
        /// <param name="standardFilters">The id of the requirement the steps belong to passed in as a filter (key = 'RequirementId')</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                RequirementManager requirementManager = new RequirementManager();

                //The requirement needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RequirementId"))
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                int requirementId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RequirementId"]);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);
                    if (requirement.OwnerId != userId && requirement.AuthorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Iterate through all the items to be removed
                foreach (string itemValue in items)
                {
                    //Get the requirement step ID
                    int requirementStepId = Int32.Parse(itemValue);
                    try
                    {
                        requirementManager.DeleteStep(projectId, requirementStepId, userId);
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
        /// <param name="destRequirementStepId">The destination item's id (or null for no destination selected)</param>
        /// <param name="standardFilters">The id of the requirement the steps belong to passed in as a filter (key = 'RequirementId')</param>
        public void OrderedList_Move(int projectId, JsonDictionaryOfStrings standardFilters, List<string> sourceItems, int? destRequirementStepId)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                RequirementManager requirementManager = new RequirementManager();

                //The requirement needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RequirementId"))
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                int requirementId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RequirementId"]);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);
                    if (requirement.OwnerId != userId && requirement.AuthorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Iterate through all the items to be moved and perform the operation
                foreach (string itemValue in sourceItems)
                {
                    //Get the source ID
                    int sourceRequirementStepId = Int32.Parse(itemValue);
                    requirementManager.MoveStep(requirementId, sourceRequirementStepId, destRequirementStepId, userId);
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
        /// <param name="requirementStepId">The id of the particular use case step we want to retrieve</param>
        /// <param name="userId">The user we're viewing the test cases/steps as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for RequirementId</param>
        /// <returns>A single dataitem object</returns>
        public OrderedDataItem OrderedList_Refresh(int projectId, JsonDictionaryOfStrings standardFilters, int requirementStepId)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                RequirementManager requirementManager = new RequirementManager();

                //The requirement needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RequirementId"))
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                int requirementId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RequirementId"]);

                //Create the data item record
                OrderedDataItem dataItem = new OrderedDataItem();
                PopulateShape(projectId, userId, dataItem);

                //Get the use case step record for the specific test case and requirement step id
                RequirementStep requirementStep = requirementManager.RetrieveStepById(requirementStepId);
                if (requirementStep == null)
                {
                    throw new ArtifactNotExistsException("Unable to locate requirement step " + requirementStepId + " in the project. It no longer exists!");
                }

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementStep.RequirementId);
                    if (requirement.OwnerId != userId && requirement.AuthorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Finally populate the dataitem from the dataset
                PopulateRow(projectId, dataItem, requirementStep, true, requirementStep.Position);
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
        /// <param name="requirementStepId">The id of the requirement step to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        public string RetrieveNameDesc(int? projectId, int requirementStepId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the requirement business object
                RequirementManager requirementManager = new RequirementManager();

                //Now retrieve the specific test step - handle quietly if it doesn't exist
                RequirementStep requirementStep = requirementManager.RetrieveStepById(requirementStepId);
                if (requirementStep == null)
                {
                    return Resources.Messages.Global_UnableRetrieveTooltip;
                }

                string tooltip = GlobalFunctions.HtmlRenderAsPlainText(requirementStep.Description);

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
        /// Clones/copies a selection of requirement steps
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for RequirementId</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The requirement needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                if (!standardFilters.ContainsKey("RequirementId"))
                {
                    throw new ArgumentException("You need to provide a RequirementId as a standard filter");
                }
                int requirementId = (int)GlobalFunctions.DeSerializeValue(standardFilters["RequirementId"]);


                //Instantiate the business objects
                RequirementManager requirementManager = new RequirementManager();

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);
                    if (requirement.OwnerId != userId && requirement.AuthorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Iterate through all the items to be copied
                foreach (string itemValue in items)
                {
                    //Get the requirement step ID
                    int requirementStepId = Int32.Parse(itemValue);
                    try
                    {
                        requirementManager.CopyStep(userId, projectId, requirementStepId);
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore any errors due to the test set of test case no longer existing
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

        #region RequirementStep Methods

        /// <summary>
        /// Retrieves the use case diagram (UML) for a requirement that has steps as DOT notation
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="requirementId">The id of the requirement</param>
        /// <returns>The DOT notation form of the requirement steps</returns>
        /// <remarks>
        public string RequirementStep_RetrieveAsDotNotation(int projectId, int requirementId)
        {
            const string METHOD_NAME = "RequirementStep_RetrieveAsDotNotation";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited is OK)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                RequirementManager requirementManager = new RequirementManager();

                //Get the list of steps in the use case (requirement)
                List<RequirementStep> requirementSteps = requirementManager.RetrieveSteps(requirementId);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);
                    if (requirement.OwnerId != userId && requirement.AuthorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                /*

                digraph use_case_diagram{
                    rankdir=TB;
                    node [shape=box rx=5 ry=5 color=lightblue2 style=filled];
                
	                subgraph main_flow {
		                style=filled;
		                color=lightgrey;
		                node [style=filled,color=white];
		                a0 -> a1 -> a2 -> a3;
		                label = "process #1";
                    }

                    Start -> "RQ:4";
                    "RQ:4" -> "RQ:5";
                    "RQ:5" -> "RQ:6";
                    "RQ:6" -> End;

                	Start [shape=ellipse];
	                Start [shape=ellipse];
                }*/

                //Convert the requirement steps into the dot notation
                string clusterFlow = "";
                string dotNotation =
@"digraph use_case_diagram {
    rankdir=TB;
    node [rx=5, ry=5, style=filled];
";
                if (requirementSteps.Count > 0)
                {
                    //Loop through the steps
                    string previousArtifactToken = "";
                    foreach (RequirementStep requirementStep in requirementSteps)
                    {
                        //Add the label
                        //need to render the RQ step itself as plain text as the JS library does not support HTML
                        //need to change double quotes to single quotes as double quotes in the plain text name cause problems
                        string stepDescription = GlobalFunctions.HtmlRenderAsPlainText(requirementStep.Description).Replace("\"", "'");
                        //truncate the length of the description so that it fits nicely on desktop screens without side scrolling
                        int maxDescriptionLength = 144;
                        stepDescription = stepDescription.Length > maxDescriptionLength ? stepDescription.Substring(0, maxDescriptionLength) + "..." : stepDescription;

                        clusterFlow += String.Format("    \"{0}\" [label=\"<span>{1}</span>\", labelType = \"html\"];\n", requirementStep.ArtifactToken, stepDescription + " [" + requirementStep.ArtifactToken + "]", requirementStep.RequirementStepId);

                        //Add the relationship
                        if (!String.IsNullOrEmpty(previousArtifactToken))
                        {
                            clusterFlow += String.Format("    \"{0}\" -> \"{1}\";\n", previousArtifactToken, requirementStep.ArtifactToken);
                        }
                        previousArtifactToken = requirementStep.ArtifactToken;
                    }

                    dotNotation += @"
	subgraph main_flow {
		style=filled;
		color=lightgrey;
		" + clusterFlow + @"
    }
";
                    //Add the start node
                    dotNotation += String.Format("    \"{0}\" -> \"{1}\";\n", "Start", requirementSteps[0].ArtifactToken);

                    //Add the end node
                    dotNotation += String.Format("    \"{0}\" -> \"{1}\";\n", requirementSteps[requirementSteps.Count - 1].ArtifactToken, "End");

                    //Style/localize the start/end nodes
                    dotNotation += String.Format("    \"{0}\" [label=\"<span>{1}</span>\", labelType = \"html\" shape=ellipse];\n", "Start", Resources.Main.RequirementStep_Start);
                    dotNotation += String.Format("    \"{0}\" [label=\"<span>{1}</span>\", labelType = \"html\"shape=ellipse];\n", "End", Resources.Main.RequirementStep_End);
                }


                dotNotation += @"
}
";

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return dotNotation;
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
