using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Linq;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Net;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the SortableGrid AJAX component for displaying/updating test run data
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class TestRunService : SortedListServiceBase, ITestRunService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TestRunService::";

        protected internal const string PROJECT_SETTINGS_PAGINATION = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_TEST_RUN_PAGINATION_SIZE;

        #region IFormService methods

        /// <summary>
        /// Deletes the current test run and returns the ID of the item to redirect to (if any)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //First we need to determine which test run to redirect the user to after the delete
                int? newTestRunId = null;
                //Look through the current dataset to see what is the next testRun in the list
                //If we are the last one on the list then we need to simply use the one before

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "TestRunId ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_TEST_RUN_PAGINATION_SIZE);
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
                //Get the number of testRuns in the project
                TestRunManager testRunManager = new TestRunManager();
                int artifactCount = testRunManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                //Get the testRuns list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }

                List<TestRunView> testRuns = testRunManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, 0);
                bool matchFound = false;
                int previousTestRunId = -1;
                foreach (TestRunView testRunRow in testRuns)
                {
                    int testTestRunId = testRunRow.TestRunId;
                    if (testTestRunId == artifactId)
                    {
                        matchFound = true;
                    }
                    else
                    {
                        //If we found a match on the previous iteration, then we want to this (next) testRun
                        if (matchFound)
                        {
                            newTestRunId = testTestRunId;
                            break;
                        }

                        //If this matches the current testRun, set flag
                        if (testTestRunId == artifactId)
                        {
                            matchFound = true;
                        }
                        if (!matchFound)
                        {
                            previousTestRunId = testTestRunId;
                        }
                    }
                }
                if (!newTestRunId.HasValue && previousTestRunId != -1)
                {
                    newTestRunId = previousTestRunId;
                }

                //Next we need to delete the current test run
                testRunManager.Delete(artifactId, projectId);

                return newTestRunId;
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

        /// <summary>Returns a single test run data record (all columns) for use by the FormManager control</summary>
        /// <param name="artifactId">The id of the current test run</param>
        /// <returns>A test run data item</returns>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business classes
                TestRunManager testRunManager = new TestRunManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, false);

                //Get the test run for the specific test run id
                TestRunView testRun = testRunManager.RetrieveById(artifactId.Value);

                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && testRun.TesterId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //See if we have any existing artifact custom properties for this row
                if (artifactCustomProperty == null)
                {
                    List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, true, false);
                    PopulateRow(dataItem, testRun, customProperties, true, (ArtifactCustomProperty)null);
                }
                else
                {
                    PopulateRow(dataItem, testRun, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty);
                }

                //Populate any other values are not part of the standard 'shape'
                if (artifactId.HasValue)
                {
                    //First the Test Case ID/Name
                    DataItemField testCaseField = new DataItemField();
                    testCaseField.FieldName = "TestCaseId";
                    testCaseField.FieldType = Artifact.ArtifactFieldTypeEnum.Lookup;
                    testCaseField.IntValue = testRun.TestCaseId;
                    testCaseField.TextValue = String.Format("{0} [{1}:{2}]", testRun.Name, GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE, testRun.TestCaseId);
                    dataItem.Fields.Add(testCaseField.FieldName, testCaseField);

                    //Automation Info
                    DataItemField automationField;
                    if (testRun.RunnerAssertCount.HasValue)
                    {
                        automationField = new DataItemField();
                        automationField.FieldName = "RunnerAssertCount";
                        automationField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
                        automationField.IntValue = testRun.RunnerAssertCount;
                        automationField.TextValue = testRun.RunnerAssertCount.Value.ToString();
                        dataItem.Fields.Add(automationField.FieldName, automationField);
                    }

                    automationField = new DataItemField();
                    automationField.FieldName = "RunnerMessage";
                    automationField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
                    automationField.TextValue = testRun.RunnerMessage;
                    dataItem.Fields.Add(automationField.FieldName, automationField);

                    automationField = new DataItemField();
                    automationField.FieldName = "RunnerTestName";
                    automationField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
                    automationField.TextValue = testRun.RunnerTestName;
                    dataItem.Fields.Add(automationField.FieldName, automationField);

                    if (testRun.TestRunFormatId.HasValue)
                    {
                        automationField = new DataItemField();
                        automationField.FieldName = "RunnerStackTrace";
                        automationField.FieldType = Artifact.ArtifactFieldTypeEnum.Html;
                        if (testRun.TestRunFormatId.Value == (int)TestRun.TestRunFormatEnum.HTML)
                        {
                            automationField.TextValue = testRun.RunnerStackTrace;
                        }
                        else
                        {
                            string htmlVersion = GlobalFunctions.TextRenderAsHtml(WebUtility.HtmlEncode(testRun.RunnerStackTrace));
                            automationField.TextValue = htmlVersion;
                        }
                        dataItem.Fields.Add(automationField.FieldName, automationField);
                    }

                    //The test set / test case instance id
                    if (testRun.TestSetTestCaseId.HasValue)
                    {
                        DataItemField testSetTestCaseField = new DataItemField();
                        testSetTestCaseField.FieldName = "TestSetTestCaseId";
                        testSetTestCaseField.FieldType = Artifact.ArtifactFieldTypeEnum.Identifier;
                        testSetTestCaseField.IntValue = testRun.TestSetTestCaseId.Value;
                        dataItem.Fields.Add(testSetTestCaseField.FieldName, testSetTestCaseField);
                    }

                    //Next Data Mapping Entries
                    DataMappingManager dataMappingManager = new DataMappingManager();
                    List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.TestRun, artifactId.Value);
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

        /// <summary>Saves a single test run data item</summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="dataItem">The test run to save</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Get the test run id
            int testRunId = dataItem.PrimaryKey;

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business classes
                TestRunManager testRunManager = new TestRunManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Load the custom property definitions (once, not per artifact)
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);

                //This service only supports updates, so we should get a test run id that is valid

                //Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
                TestRun testRun = testRunManager.RetrieveById2(testRunId);

                //Make sure the user is authorized for this item if they only have limited permissions
                if (authorizationState == Project.AuthorizationState.Limited && testRun.TesterId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Create a new artifact custom property row if one doesn't already exist
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, false, customProperties);
                if (artifactCustomProperty == null)
                {
                    artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestRun, testRunId, customProperties);
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
                        testRun.ConcurrencyDate = concurrencyDateTimeValue;
                        testRun.AcceptChanges();
                    }
                }

                //Now we can start tracking any changes
                testRun.StartTracking();

                //Update the field values, tracking changes
                List<string> fieldsToIgnore = new List<string>();
                fieldsToIgnore.Add("StartDate");
                fieldsToIgnore.Add("EndDate");
                fieldsToIgnore.Add("TestRunTypeId");
                fieldsToIgnore.Add("TestCaseId");

                //Need to handle any data-mapping fields (project-admin only)
                if (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)
                {
                    DataMappingManager dataMappingManager = new DataMappingManager();
                    List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.TestRun, testRunId);
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
                UpdateFields(validationMessages, dataItem, testRun, customProperties, artifactCustomProperty, projectId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, fieldsToIgnore);

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

                //If the test set id has changed, we need to find a matching test case for this test run
                //otherwise we reset the test set id back
                if (testRun.ChangeTracker.OriginalValues.ContainsKey("TestSetId") && testRun.TestSetId.HasValue)
                {
                    int newTestSetId = testRun.TestSetId.Value;
                    int? oldTestSetId = null;
                    if (testRun.ChangeTracker.OriginalValues["TestSetId"] != null)
                    {
                        oldTestSetId = (int)testRun.ChangeTracker.OriginalValues["TestSetId"];
                    }

                    bool matchFound = false;
                    List<TestSetTestCaseView> testSetTestCases = new TestSetManager().RetrieveTestCases(newTestSetId);
                    foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
                    {
                        if (testSetTestCase.TestCaseId == testRun.TestCaseId)
                        {
                            testRun.TestSetTestCaseId = testSetTestCase.TestSetTestCaseId;
                            matchFound = true;
                            break;
                        }
                    }

                    if (!matchFound)
                    {
                        //Revert it back
                        testRun.TestSetId = oldTestSetId; 
                    }
                }

                //Update the test run and any custom properties
                try
                {
                    testRunManager.Update(projectId, testRun, userId);
                }
                catch (TestRunInvalidTestSetException)
                {
                    //Display an error message
                    return CreateSimpleValidationMessage(Resources.Messages.TestRunDetails_TestSetIsFolderOrNotExists);
                }
                catch (OptimisticConcurrencyException)
                {
                    return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
                }
                catch (EntityForeignKeyException)
                {
                    return CreateSimpleValidationMessage(Resources.Messages.Global_DependentArtifactDeleted);
                }
                customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

                //Return back any messages.
                return validationMessages;
            }
            catch (ArtifactNotExistsException)
            {
                //Let the user know that the ticket no inter exists
                return CreateSimpleValidationMessage(String.Format(Resources.Messages.TestRunService_TestRunNotFound, testRunId));
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

        #region ISortedList Methods

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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.TestRun);
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
                TestRunManager testRunManager = new TestRunManager();

                //Load the custom property definitions (once, not per artifact)
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);
                foreach (SortedDataItem dataItem in dataItems)
                {
                    //Get the testRun id
                    int testRunId = dataItem.PrimaryKey;

                    //Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
                    TestRun testRun = testRunManager.RetrieveById2(testRunId);
                    ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, false, customProperties);

                    //Create a new artifact custom property row if one doesn't already exist
                    if (artifactCustomProperty == null)
                    {
                        artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestRun, testRunId, customProperties);
                    }
                    else
                    {
                        artifactCustomProperty.StartTracking();
                    }

                    if (testRun != null)
                    {
                        //Need to set the original date of this record to match the concurrency date
                        if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                        {
                            DateTime concurrencyDateTimeValue;
                            if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                            {
                                testRun.ConcurrencyDate = concurrencyDateTimeValue;
                                testRun.AcceptChanges();
                            }
                        }

                        //Now we can start tracking any changes
                        testRun.StartTracking();

                        //Update the field values
                        List<string> fieldsToIgnore = new List<string>();
                        fieldsToIgnore.Add("StartDate");
                        fieldsToIgnore.Add("EndDate");
                        fieldsToIgnore.Add("TestRunTypeId");
                        fieldsToIgnore.Add("TestCaseId");
                        UpdateFields(validationMessages, dataItem, testRun, customProperties, artifactCustomProperty, projectId, testRunId, 0, fieldsToIgnore);

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
                            //If the test set id has changed, we need to find a matching test case for this test run
                            //otherwise we reset the test set id back
                            if (testRun.ChangeTracker.OriginalValues.ContainsKey("TestSetId") && testRun.TestSetId.HasValue)
                            {
                                int newTestSetId = testRun.TestSetId.Value;
                                int? oldTestSetId = null;
                                if (testRun.ChangeTracker.OriginalValues["TestSetId"] != null)
                                {
                                    oldTestSetId = (int)testRun.ChangeTracker.OriginalValues["TestSetId"];
                                }

                                bool matchFound = false;
                                List<TestSetTestCaseView> testSetTestCases = new TestSetManager().RetrieveTestCases(newTestSetId);
                                foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
                                {
                                    if (testSetTestCase.TestCaseId == testRun.TestCaseId)
                                    {
                                        testRun.TestSetTestCaseId = testSetTestCase.TestSetTestCaseId;
                                        matchFound = true;
                                        break;
                                    }
                                }

                                if (!matchFound)
                                {
                                    //Revert it back
                                    testRun.TestSetId = oldTestSetId;
                                }
                            }

                            //Persist to database, catching any business exceptions and displaying them
                            try
                            {
                                testRunManager.Update(projectId, testRun, userId);
                            }
                            catch (TestRunInvalidTestSetException)
                            {
                                return CreateSimpleValidationMessage(Resources.Messages.TestRunService_TestSetNotValid);
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
        /// Returns a list of testRuns in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the testRuns as</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the testRun and custom property business objects
                TestRunManager testRunManager = new TestRunManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the array of data items (including the first filter item)
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;

                //Now get the list of populated filters and the current sort
                string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST;
                string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION;
                if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestCase_Runs)
                {
                    filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_DETAILS_TEST_RUNS_FILTERS;
                    sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_DETAILS_TEST_RUNS_GENERAL;
                }
                if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestSet_Runs)
                {
                    filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_DETAILS_TEST_RUNS_FILTERS;
                    sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_DETAILS_TEST_RUNS_GENERAL;
                }

                Hashtable filterList = GetProjectSettings(userId, projectId, filtersSettingsCollection);
                string sortCommand = GetProjectSetting(userId, projectId, sortSettingsCollection, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "EndDate DESC");
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
                sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestRun);

                //Create the filter item first - we can clone it later
                SortedDataItem filterItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
                dataItems.Add(filterItem);

                //Get the ProjectSettings collection
                ProjectSettings projectSettings = null;
                if (projectId > 0)
                {
                    projectSettings = new ProjectSettings(projectId);
                }

                //** WORX - Add the special WorX column if enabled
                if (projectSettings != null && projectSettings.Testing_WorXEnabled)
                {
                    DataItemField worxField = new DataItemField();
                    worxField.FieldName = "worx";
                    worxField.Caption = "Worx";
                    worxField.FieldType = Artifact.ArtifactFieldTypeEnum.Html;
                    worxField.AllowDragAndDrop = true;
                    filterItem.Fields.Add("worx", worxField);
                }

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
                //Get the number of testRuns in the project
                int artifactCount = testRunManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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
                List<TestRunView> testRuns = testRunManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //Display the pagination information
                sortedData.CurrPage = currentPage;
                sortedData.PageCount = pageCount;
                sortedData.StartRow = startRow;

                //Display the visible and total count of artifacts
                sortedData.VisibleCount = testRuns.Count;
                sortedData.TotalCount = artifactCount;

                //Display the sort information
                sortedData.SortProperty = sortProperty;
                sortedData.SortAscending = sortAscending;

                //Now get the list of custom property options and lookup values for this artifact type / project
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, true, false, true);

                //Iterate through all the testRuns and populate the dataitem
                foreach (TestRunView testRun in testRuns)
                {
                    //We clone the template item as the basis of all the new items
                    SortedDataItem dataItem = filterItem.Clone();

                    //Now populate with the data
                    PopulateRow(dataItem, testRun, customProperties, false, null);
                    dataItems.Add(dataItem);

                    //WorX - populate Worx links
                    if (projectSettings != null && projectSettings.Testing_WorXEnabled)
                    {
                        if (dataItem.Fields.ContainsKey("worx"))
                        {
                            DataItemField worxField = dataItem.Fields["worx"];
                            worxField.TextValue =
                                "<a href=\"worx:spira/open/pr" + projectId + "/tr" + dataItem.PrimaryKey + "\">Open</a> | " +
                                "<a href=\"worx:spira/review/pr" + projectId + "/tr" + dataItem.PrimaryKey + "\">Review</a>";
                        }
                    }
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
        /// Returns the latest information on a single testRun in the system
        /// </summary>
        /// <param name="userId">The user we're viewing the testRun as</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the testRun and custom property business objects
                TestRunManager testRunManager = new TestRunManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

                //Get the test run record for the specific testRun id
                TestRunView testRun = testRunManager.RetrieveById(artifactId);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && testRun.TesterId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);

                //Finally populate the dataitem from the dataset
                if (testRun != null)
                {
                    //See if we already have an artifact custom property row
                    if (artifactCustomProperty != null)
                    {
                        PopulateRow(dataItem, testRun, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty);
                    }
                    else
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, true, false);
                        PopulateRow(dataItem, testRun, customProperties, true, null);
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

            string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION;
            if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestCase_Runs)
            {
                sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_DETAILS_TEST_RUNS_GENERAL;
            }
            if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestSet_Runs)
            {
                sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_DETAILS_TEST_RUNS_GENERAL;
            }

            //Call the base method with the appropriate settings collection
            return base.UpdateSort(userId, projectId, sortProperty, sortAscending, sortSettingsCollection);
        }

        /// <summary>
        /// Deletes the selected test runs from the system
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="items">The list of test run ids</param>
        public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            const string METHOD_NAME = "Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Iterate through all the items to be deleted
                TestRunManager testRunManager = new TestRunManager();
                foreach (string itemValue in items)
                {
                    //Get the testRun ID
                    int testRunId = Int32.Parse(itemValue);
                    testRunManager.Delete(testRunId, projectId);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
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

        #endregion

        #region IListService Methods

        /// <summary>
        /// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
        /// </summary>
        /// <param name="testRunId">The id of the testRun to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        public string RetrieveNameDesc(int? projectId, int testRunId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the testRun business object
                TestRunManager testRunManager = new TestRunManager();

                //Now retrieve the specific testRun - handle quietly if it doesn't exist
                try
                {
                    TestRun testrun = testRunManager.RetrieveByIdWithSteps(testRunId);
                    //display the name and execution status
                    string tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testrun.Name) + " - " + testrun.ExecutionStatus.Name + "</u>";
                    if (!String.IsNullOrEmpty(testrun.Description))
                    {
                        //Add the description
                        tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testrun.Description);
                    }

                    //If automated, add the short message
                    if (testrun.TestRunTypeId == (int)TestRun.TestRunTypeEnum.Automated && testrun.RunnerAssertCount.HasValue && !String.IsNullOrEmpty(testrun.RunnerMessage))
                    {
                        tooltip += "<br />\n<i>" + Resources.Fields.AssertCount + ": " + testrun.RunnerAssertCount.Value + "<br />\n" +
                            Resources.Fields.RunnerMessage + ": " + Microsoft.Security.Application.Encoder.HtmlEncode(testrun.RunnerMessage) + "</i>\n";
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

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the testRun, but it has already been deleted on the server
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for testRun");
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
                    customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestRun, fieldName);
                }
                else
                {
                    //Toggle the status of the appropriate field name
                    ArtifactManager artifactManager = new ArtifactManager();
                    artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestRun, fieldName);
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
                artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestRun, fieldName, width);
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
        /// Changes the order of columns in the test run list
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
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //The field position may be different to the index because index is zero-based
                int newPosition = newIndex + 1;

                //Toggle the status of the appropriate artifact field or custom property
                ArtifactManager artifactManager = new ArtifactManager();
                artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestRun, fieldName, newPosition);
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

            string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST;
            if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestCase_Runs)
            {
                filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_DETAILS_TEST_RUNS_FILTERS;
            }
            if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestSet_Runs)
            {
                filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_DETAILS_TEST_RUNS_FILTERS;
            }

            //Call the base method with the appropriate settings collection
            return base.UpdateFilters(userId, projectId, filters, filtersSettingsCollection, DataModel.Artifact.ArtifactTypeEnum.TestRun);
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
            return base.RetrieveFilters(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, includeShared);
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

            return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.TestRun, GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST, GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION, isShared, existingSavedFilterId, includeColumns);
        }


        #endregion

        #region Internal Functions


        /// <summary>
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="testRun">The datarow containing the data</param>
        /// <param name="customProperties">The list of custom property definitions and values</param>
        /// <param name="editable">Does the data need to be in editable form?</param>
        /// <param name="artifactCustomProperty">The artifatc's custom property data (if not provided as part of dataitem) - pass null if not used</param>
        protected void PopulateRow(SortedDataItem dataItem, TestRunView testRun, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty)
        {
            //Set the primary key
            dataItem.PrimaryKey = testRun.TestRunId;

            //Specify if it has an attachment or not
            dataItem.Attachment = testRun.IsAttachments;

            //The date and some fields are not editable
            List<string> readOnlyFields = new List<string>() { "StartDate", "EndDate", "ExecutionStatusId", "RunnerName", "TestRunTypeId", "LastUpdateDate"  };

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (testRun.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, testRun, customProperties, artifactCustomProperty, editable, null, null, null, readOnlyFields);

                    //Apply the conditional formatting to the execution status column (if displayed)
                    if (dataItemField.FieldName == "ExecutionStatusId")
                    {
                        dataItemField.CssClass = GlobalFunctions.GetExecutionStatusCssClass(testRun.ExecutionStatusId);
                    }
                }
            }
        }

        /// <summary>Populates the 'shape' of the data item that will be used as a template for the retrieved data items</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="userId">The user we're viewing the testRuns as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        /// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
        protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
        {
            //We need to dynamically add the various columns from the field list
            LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
            AddDynamicColumns(Artifact.ArtifactTypeEnum.TestRun, getLookupValues, projectId, projectTemplateId, userId, dataItem, filterList, null, returnJustListFields);
			
			//Remove 'Not Run' from filter list
			if (dataItem.Fields.ContainsKey("ExecutionStatusId"))
			{
				DataItemField field = dataItem.Fields["ExecutionStatusId"];
				//Remove "'3' - Not Run" if it exists.
				if (field.Lookups != null && field.Lookups.ContainsKey("3"))
					field.Lookups.Remove("3");
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
                TestRunManager testRunManager = new TestRunManager();
                TestCaseManager testCaseManager = new TestCaseManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                if (lookupName == "TesterId")
                {
                    List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
                    lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
                }
                if (lookupName == "TestRunTypeId")
                {
                    List<TestRunType> testRunTypes = testRunManager.RetrieveTypes();
                    lookupValues = ConvertLookupValues(testRunTypes.OfType<Entity>().ToList(), "TestRunTypeId", "Name");
                }
                if (lookupName == "BuildId")
                {
                    List<BuildView> builds = new BuildManager().RetrieveForProject(projectId);
                    lookupValues = ConvertLookupValues(builds.OfType<Entity>().ToList(), "BuildId", "Name");
                }
                if (lookupName == "AutomationHostId")
                {
                    AutomationManager automationManager = new AutomationManager();
                    List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId);
                    lookupValues = ConvertLookupValues(automationHosts.OfType<Entity>().ToList(), "AutomationHostId", "Name");
                }
                if (lookupName == "ExecutionStatusId")
                {
                    List<ExecutionStatus> executionStati = testCaseManager.RetrieveExecutionStatuses();
                    lookupValues = ConvertLookupValues(executionStati.OfType<DataModel.Entity>().ToList(), "ExecutionStatusId", "Name");
                }
                if (lookupName == "ReleaseId")
                {
                    ReleaseManager releaseManager = new ReleaseManager();
                    List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false, true);
                    lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
                }
                if (lookupName == "TestSetId")
                {
                    TestSetManager testSetManager = new TestSetManager();
                    Dictionary<string, string> testSetLookup = testSetManager.RetrieveForLookups(projectId);
                    lookupValues = new JsonDictionaryOfStrings(testSetLookup);
                }

                //The custom property lookups
                int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
                if (customPropertyNumber.HasValue)
                {
                    CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, customPropertyNumber.Value, true);
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

        #endregion

        #region INavigationService Methods

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
        /// Returns a list of test runs for display in the navigation bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="indentLevel">Not used for test runs since not hierarchical</param>
        /// <returns>List of test runs</returns>
        /// <param name="containerId">The id of the parent artifact (if specified)</param>
        /// <param name="displayMode">
        /// The display mode of the navigation list:
        /// 1 = Filtered List
        /// 2 = All Items (no filters)
        /// 5 = Associated with the specified test case
        /// 6 = Associated with the specified test set
        /// 7 = Associated with the specified release
        /// </param>
        /// <param name="selectedItemId">The id of the currently selected test run</param>
        public List<HierarchicalDataItem> NavigationBar_RetrieveList(int projectId, string indentLevel, int displayMode, int? selectedItemId, int? containerId)
        {
            const string METHOD_NAME = "RetrieveNavigationList";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the testRun business object
                TestRunManager testRunManager = new TestRunManager();

                //Create the array of data items
                List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "EndDate DESC");
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
                //Get the number of testRuns in the project
                int artifactCount = testRunManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //**** Now we need to actually populate the rows of data to be returned ****

                //Get the testRuns list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }
                List<TestRunView> testRuns;
                string parentIndent = "";
                if (displayMode == 2)
                {
                    //All Items
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                    testRuns = testRunManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, 0);
                }
                else if (displayMode == 5 && containerId.HasValue)
                {
                    //For the Test Case
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                    int testCaseId = containerId.Value;
                    Hashtable filters = new Hashtable();
                    filters.Add("TestCaseId", testCaseId);
                    testRuns = testRunManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                    //We also need to add a special test case item
                    TestCaseView testCase = new TestCaseManager().RetrieveById(projectId, testCaseId);
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();

                    //Populate the necessary fields
                    parentIndent = "AAA";
                    dataItem.PrimaryKey = testCase.TestCaseId;
                    dataItem.Indent = parentIndent;
                    dataItem.Expanded = true;
                    dataItem.Summary = true;
                    dataItem.Alternate = false;
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, projectId, testCaseId, GlobalFunctions.PARAMETER_TAB_TESTRUN));

                    //Name/Desc
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = testCase.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Execution Status style
                    dataItemField.CssClass = GlobalFunctions.GetExecutionStatusCssClass(testCase.ExecutionStatusId);

                    //Add to the items collection
                    dataItems.Add(dataItem);
                }
                else if (displayMode == 7 && containerId.HasValue)
                {
                    //For the Release
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                    int releaseId = containerId.Value;
                    Hashtable filters = new Hashtable();
                    filters.Add("ReleaseId", releaseId);
                    testRuns = testRunManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                    //We also need to add a special release item
                    ReleaseView release = new ReleaseManager().RetrieveById2(projectId, releaseId);
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();

                    //Populate the necessary fields
                    parentIndent = "AAA";
                    dataItem.PrimaryKey = release.ReleaseId;
                    dataItem.Indent = parentIndent;
                    dataItem.Expanded = true;
                    dataItem.Summary = true;
                    dataItem.Alternate = release.IsIterationOrPhase;
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, projectId, releaseId, GlobalFunctions.PARAMETER_TAB_TESTRUN));

                    //Name/Desc
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = release.Name;
                    dataItem.Fields.Add("Name", dataItemField);

                    //Add to the items collection
                    dataItems.Add(dataItem);
                }
                else if (displayMode == 6 && containerId.HasValue)
                {
                    //For the test set
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                    int testSetId = containerId.Value;
                    Hashtable filters = new Hashtable();
                    filters.Add("TestSetId", testSetId);
                    testRuns = testRunManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                    //We also need to add a special test set item
                    TestSetView testSet = new TestSetManager().RetrieveById(projectId, testSetId);
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();

                    //Populate the necessary fields
                    parentIndent = "AAA";
                    dataItem.PrimaryKey = testSet.TestSetId;
                    dataItem.Indent = parentIndent;
                    dataItem.Expanded = true;
                    dataItem.Summary = true;
                    dataItem.Alternate = false;
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, projectId, testSetId, GlobalFunctions.PARAMETER_TAB_TESTRUN));

                    //Name/Desc
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = testSet.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Add to the items collection
                    dataItems.Add(dataItem);
                }
                else
                {
                    //Filtered List
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                    testRuns = testRunManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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

                //Iterate through all the testRuns and populate the dataitem (only some columns are needed)
                string testRunIndent = (parentIndent == "") ? "" : parentIndent + "AAA";
                foreach (TestRunView testRunView in testRuns)
                {
                    //Create the data-item
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();

                    //Populate the necessary fields
                    dataItem.PrimaryKey = testRunView.TestRunId;
                    dataItem.Indent = testRunIndent;
                    dataItem.Expanded = false;

                    //Name/Desc
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, testRunView.EndDate) + " (" + GlobalFunctions.ARTIFACT_PREFIX_TEST_RUN + String.Format(GlobalFunctions.FORMAT_ID, testRunView.TestRunId) + ")";
                    dataItem.Summary = false;
                    dataItem.Alternate = false;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Execution Status style
                    dataItemField.CssClass = GlobalFunctions.GetExecutionStatusCssClass(testRunView.ExecutionStatusId);

                    //Add to the items collection and increment indent if necessary
                    dataItems.Add(dataItem);
                    if (testRunIndent != "")
                    {
                        testRunIndent = HierarchicalList.IncrementIndentLevel(testRunIndent);
                    }
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
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION);
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

		#region ITestRunService methods

		/// <summary>
		/// Reassigns a test run to a new user
		/// </summary>
		/// <param name="testRunsPendingId">The ID of the test runs pending</param>
		/// <param name="newAssigneeId">The new assignee</param>
		/// <remarks>
		/// You either need to be a project admin or the person it is currently assigned to
		/// </remarks>
		public void TestRun_ReassignPending(int testRunsPendingId, int newAssigneeId)
		{
			const string METHOD_NAME = "Task_RetrieveBurndown";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			try
			{
				//Retrieve the test run pending first
				TestRunManager testRunManager = new TestRunManager();
				TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, false);

				//Get the project
				int projectId = testRunsPending.ProjectId;
				
				//Make sure we're authorized to view test runs
				Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestRun);
				if (authorizationState == Project.AuthorizationState.Prohibited)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Make sure we're either project admin or the current assignee
				authorizationState = IsAuthorized(projectId, Project.PermissionEnum.ProjectAdmin);
				if (testRunsPending.TesterId != userId && authorizationState != Project.AuthorizationState.Authorized)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Actually do the reassignment
				testRunManager.ReassignPending(testRunsPendingId, newAssigneeId, userId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (ArtifactNotExistsException)
			{
				//Just throw and let the page display the message
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns the data needed for the simple test run progress graph on the list page
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release (optional)</param>
		/// <returns></returns>
		public GraphData TestRun_RetrieveProgress(int projectId, int? releaseId)
        {
            const string METHOD_NAME = "TestRun_RetrieveProgress";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view test runs
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Create the graph data
                DataObjects.GraphData graphData = new DataObjects.GraphData();

                //We need to find the most recent test run for this release
                DateTime endDate = DateTime.UtcNow;
                Hashtable filters = new Hashtable();
                if (releaseId.HasValue)
                {
                    filters.Add("ReleaseId", releaseId.Value);
                }
                TestRunView lastTestRun = new TestRunManager().Retrieve(projectId, "EndDate", false, 1, 1, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset()).FirstOrDefault();
                if (lastTestRun != null && lastTestRun.EndDate.HasValue)
                {
                    endDate = lastTestRun.EndDate.Value;
                }

                //Specify the last 15 days by day
                DateRange dateRange = new DateRange();
                dateRange.ConsiderTimes = false;
                dateRange.StartDate = endDate.AddDays(-15);
                dateRange.EndDate = endDate;
                Graph.ReportingIntervalEnum interval = Graph.ReportingIntervalEnum.Daily;
                graphData.Interval = "1 day";

                //Specify that the data is stacked
                graphData.Options = "stacked";
                DataSet graphDataSet = new GraphManager().RetrieveTestRunCountByExecutionStatus(projectId, interval, dateRange, GlobalFunctions.GetCurrentTimezoneUtcOffset(), releaseId);

                if (graphDataSet == null)
                {
                    return null;
                }

                //Generate the number of different data-series
                DataTable dataTable = graphDataSet.Tables[0];
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    //We don't add the key columns as they are used for the axis
                    if (dataTable.PrimaryKey.Contains(dataColumn))
                    {
                        graphData.XAxisCaption = dataColumn.Caption;
                    }
                    else
                    {
                        DataObjects.DataSeries series = new DataObjects.DataSeries();
                        series.Name = dataColumn.ColumnName;
                        series.Caption = dataColumn.Caption;
                        //See if a color is included in the series
                        if (dataColumn.ExtendedProperties.ContainsKey("Color"))
                        {
                            series.Color = (string)dataColumn.ExtendedProperties["Color"];
                        }

                        graphData.Series.Add(series);
                    }
                }

                //Populate the data items
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    //First we need to add the actual data x-axis values
                    //Get the key column
                    DataColumn dataColumn = dataTable.PrimaryKey[0];
                    GraphAxisPosition axisPosition = new GraphAxisPosition();
                    axisPosition.Id = dataTable.Rows.IndexOf(dataRow);
                    axisPosition.StringValue = ((DateTime)dataRow[dataColumn]).ToShortDateString();
                    axisPosition.DateValue = ((DateTime)dataRow[dataColumn]);
                    graphData.XAxis.Add(axisPosition);

                    //Now add the data series
                    foreach (DataObjects.DataSeries series in graphData.Series)
                    {
                        if (dataRow[series.Name] != null)
                        {
                            object value = dataRow[series.Name];
                            if (value.GetType() == typeof(decimal))
                            {
                                series.Values.Add(axisPosition.Id.ToString(), (decimal)value);
                            }
                            else if (value.GetType() == typeof(int))
                            {
                                int intValue = (int)value;
                                series.Values.Add(axisPosition.Id.ToString(), (decimal)intValue);
                            }
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return graphData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Counts the number of test runs
        /// </summary>
        /// <param name="projectId">The project id</param>
        /// <param name="artifact">The artifact we want history for</param>
        /// <returns>The count</returns>
        public int TestRun_Count(int projectId, ArtifactReference artifact)
        {
            const string METHOD_NAME = "TestRun_Count";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view test runs
            //Limited OK because we need to display the 'has data' in tabs
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Depending on the artifact that these test runs are for (test case, release or test set)
                //we need to set the grid properties accordingly and also indicate if we have any data
                int testRunCount = 0;
                TestRunManager testRunManager = new TestRunManager();
                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestCase)
                {
                    //Get the count of test runs (filtered)
                    Hashtable testRunFilters = new Hashtable();
                    testRunFilters.Add("TestCaseId", artifact.ArtifactId);
                    testRunCount = testRunManager.Count(projectId, testRunFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }

                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestSet)
                {
                    //Get the count of test runs (filtered)
                    Hashtable testRunFilters = new Hashtable();
                    testRunFilters.Add("TestSetId", artifact.ArtifactId);
                    testRunCount = testRunManager.Count(projectId, testRunFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }

                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Release)
                {
                    //Get the count of test runs (filtered)
                    Hashtable testRunFilters = new Hashtable();
                    testRunFilters.Add("ReleaseId", artifact.ArtifactId);
                    testRunCount = testRunManager.Count(projectId, testRunFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }

                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.AutomationHost)
                {
                    //Get the count of test runs (filtered)
                    Hashtable testRunFilters = new Hashtable();
                    testRunFilters.Add("AutomationHostId", artifact.ArtifactId);
                    testRunCount = testRunManager.Count(projectId, testRunFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }

                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Build)
                {
                    //Get the count of test runs (filtered)
                    Hashtable testRunFilters = new Hashtable();
                    testRunFilters.Add("BuildId", artifact.ArtifactId);
                    testRunCount = testRunManager.Count(projectId, testRunFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }

                return testRunCount;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }



        /// <summary>
        /// Returns a list of test runs pending for a user and specific test case - to help show a user their existing pending runs
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project id - used for permission checks only</param>
        /// <param name="testCaseId">A specific test case id</param>
        /// <returns>List of test runs pending</returns>
        /// </param>
        public List<NameValue> RetrievePendingByUserIdAndTestCase(int projectId, int testCaseId)
        {
            const string METHOD_NAME = "RetrievePendingByUserIdAndTestCase";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the testRun business object
                TestRunManager testRunManager = new TestRunManager();
                int ENTRIES_TO_DISPLAY = 5;

                //Get any test runs pending and select the first X only
                IEnumerable<TestRunsPendingView> testRunsPending = testRunManager
                    .RetrievePendingByUserIdAndTestCase(userId, testCaseId)
                    .Take(ENTRIES_TO_DISPLAY);

                List<NameValue> nameValues = new List<NameValue>();
                //Create the data to return to client
                if (testRunsPending.Count() > 0)
                {
                    foreach(TestRunsPendingView testRun in testRunsPending)
                    {
                        // if the testrunspending is from a test set then we add that information to the Name so that the user can see the test set info in the ui
                        string testSetSuffix = " (" + (testRun.TestSetId.HasValue ? Resources.Fields.TestSet + ": " : "") + testRun.Name + ")";
                        NameValue nameValue = new NameValue()
                        {
                            Id = testRun.TestRunsPendingId,
                            Name = GlobalFunctions.LocalizeDate(testRun.LastUpdateDate).ToString() + testSetSuffix
                        };
                        nameValues.Add(nameValue);
                    }
                }

                return nameValues;
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
