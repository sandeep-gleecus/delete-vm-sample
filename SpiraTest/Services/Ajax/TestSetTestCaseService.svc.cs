using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the OrderedGrid AJAX component for displaying/updating the list of test cases in a test set
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class TestSetTestCaseService : ListServiceBase, ITestSetTestCaseService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TestSetTestCaseService::";

        /// <summary>
        /// Retrieves a list of parameters that are defined for a specific test case in the test set
        /// (either inherited or direct), including any values that are populated
        /// </summary>
        /// <param name="testSetTestCaseId">The id of the unique test case in the test set</param>
        /// <returns>The list of parameter data objects</returns>
        public List<DataItem> RetrieveParameters(int testSetTestCaseId)
        {
            const string METHOD_NAME = "RetrieveParameters";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Create the array of data items to store the parameter values
                List<DataItem> dataItems = new List<DataItem>();

                //Get the list of parameters that are either direct or inherited for the test case
                TestCaseManager testCaseManager = new TestCaseManager();
                TestSetManager testSetManager = new TestSetManager();
                TestSetTestCase testSetTestCase = testSetManager.RetrieveTestCaseById(testSetTestCaseId);
                int testCaseId = testSetTestCase.TestCaseId;
                List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(testCaseId, true, true);

                //Now get the list of values that have been already set on the test case in the test set
                List<TestSetTestCaseParameter> testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);

                //Populate the data items list
                foreach (TestCaseParameter testCaseParameter in testCaseParameters)
                {
                    //The data item itself
                    DataItem dataItem = new DataItem();
                    dataItem.PrimaryKey = testCaseParameter.TestCaseParameterId;
                    dataItems.Add(dataItem);

                    //The Name field
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = testCaseParameter.Name;
                    dataItem.Fields.Add("Name", dataItemField);

                    //The Value field from the test steps dataset
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Value";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    TestSetTestCaseParameter testSetTestCaseParameter = testSetTestCaseParameters.FirstOrDefault(t => t.TestCaseParameterId == testCaseParameter.TestCaseParameterId);
                    if (testSetTestCaseParameter == null)
                    {
                        dataItemField.TextValue = null;
                    }
                    else
                    {
                        dataItemField.TextValue = testSetTestCaseParameter.Value;
                    }
                    dataItem.Fields.Add("Value", dataItemField);
                }

                return dataItems;
            }
            catch (ArtifactNotExistsException)
            {
                //Just return null
                return null;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates the list of parameter values with those provided
        /// </summary>
        /// <param name="testSetTestCaseId">The id of the unique test case in the test set</param>
        /// <param name="testCaseParameterValues">The list of parameter value data objects</param>
        /// <remarks>
        /// Will insert/delete parameter values if necessary
        /// </remarks>
        public void UpdateParameters(int testSetTestCaseId, List<DataItem> testCaseParameterValues)
        {
            const string METHOD_NAME = "UpdateParameters";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Convert the parameters into their EF equivalent objects, the BO function determines if INSERT/UPDATE/DELETE
                List<TestSetTestCaseParameter> testSetTestCaseParameterValues = new List<TestSetTestCaseParameter>();
                foreach (DataItem parameterItem in testCaseParameterValues)
                {
                    if (parameterItem.Fields.ContainsKey("Value") && !String.IsNullOrWhiteSpace(parameterItem.Fields["Value"].TextValue))
                    {
                        TestSetTestCaseParameter testSetTestCaseParameterValue = new TestSetTestCaseParameter();
                        testSetTestCaseParameterValue.TestSetTestCaseId = testSetTestCaseId;
                        testSetTestCaseParameterValue.TestCaseParameterId = parameterItem.PrimaryKey;
                        testSetTestCaseParameterValue.Value = parameterItem.Fields["Value"].TextValue;
                        testSetTestCaseParameterValues.Add(testSetTestCaseParameterValue);
                    }
                }
                //Check to see if we are removing all/the last value remaining
                TestSetManager testSetManager = new TestSetManager();
                if (testSetTestCaseParameterValues.Count == 0)
                {
                    testSetManager.RemoveTestCaseParameterValues(testSetTestCaseId);
                }
                else
                {
                    //Commit the changes
                    testSetManager.SaveTestCaseParameterValues(testSetTestCaseParameterValues);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns a list of test cases in the test set
        /// </summary>
        /// <param name="userId">The user we're viewing the test cases as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for TestSetId</param>
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

            //Make sure we're authorized, limited is OK as long as we own the test set itself
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business objects
                TestSetManager testSetManager = new TestSetManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the array of data items
                OrderedData orderedData = new OrderedData();
                List<OrderedDataItem> dataItems = orderedData.Items;

                //Create the first 'shape' item, we can clone others from it later
                OrderedDataItem shapeItem = new OrderedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, shapeItem);
                dataItems.Add(shapeItem);

                //The test set needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException(Resources.Messages.TestSetTestCaseService_MissingTestSetIdStdFilter);
                }
                if (!standardFilters.ContainsKey("TestSetId"))
                {
                    throw new ArgumentException(Resources.Messages.TestSetTestCaseService_MissingTestSetIdStdFilter);
                }
                int testSetId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestSetId"]);

                //If we have limited view, make sure we own the test set
                if (authorizationState == Project.AuthorizationState.Limited)
                {
                    TestSetView testSet = testSetManager.RetrieveById(null, testSetId);
                    if (testSet.OwnerId != userId && testSet.CreatorId != userId)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                }

                //Now get the pagination information and add to the shape item
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_TEST_CASE_PAGINATION);
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

                //See if we are viewing for a specific release or not
                int? filterReleaseId = null;                
                int filterReleaseIdSetting = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                if (filterReleaseIdSetting > 0)
                {
                    filterReleaseId = filterReleaseIdSetting;
                }

                //**** Now we need to actually populate the rows of data to be returned ****

                //Now get the list of test case custom property options and lookup values
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, true, false, true);

                //Get the test set list of test cases
                List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases3(projectId, testSetId, filterReleaseId);
                int artifactCount = testSetTestCases.Count;
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

                //Iterate through all the test cases in the pagination range populate the dataitem
                for (int i = startRow - 1; i < startRow + paginationSize - 1 && i < artifactCount; i++)
                {
                    TestSetTestCaseView testSetTestCaseView = testSetTestCases[i];

                    //We clone the template/shape item as the basis of all the new items
                    OrderedDataItem dataItem = shapeItem.Clone();

                    //Now populate with the data
                    PopulateRow(projectId, dataItem, testSetTestCaseView, testSetTestCaseView.Position, customProperties, false, null);
                    dataItems.Add(dataItem);
                }
                
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
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the test cases as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        protected void PopulateShape(int projectId, int projectTemplateId, int userId, OrderedDataItem dataItem)
        {
            //First add the fixed columns
            //Test Case Name
            DataItemField dataItemField = new DataItemField();
            dataItemField.FieldName = "Name";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
            dataItemField.Caption = Resources.Fields.TestCaseName;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //Next add the dynamic columns
            AddDynamicColumns(projectId, projectTemplateId, userId, dataItem, GetLookupValues);

            //Test Case ID
            dataItemField = new DataItemField();
            dataItemField.FieldName = "TestCaseId";
            dataItemField.Caption = Resources.Fields.ID;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
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
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                if (lookupName == "OwnerId")
                {
                    UserManager userManager = new UserManager();
                    List<DataModel.User> users = userManager.RetrieveActiveByProjectId(projectId);
                    lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
                }

                //The custom property lookups
                int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
                if (customPropertyNumber.HasValue)
                {
                    CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, customPropertyNumber.Value, true);
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

                return lookupValues;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Adds the shape of the dynamic columns to the data item
        /// </summary>
        /// <param name="lookupRetrieval">The delegate used to get lookup values</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="dataItem">The data item</param>
        /// <param name="projectTemplateId">The d of the project template</param>
        /// <param name="dataItemField">The data item field</param>
        /// <remarks>We do not use the base class version because we are storing the column section in a different way</remarks>
        protected void AddDynamicColumns(int projectId, int projectTemplateId, int userId, DataItem dataItem, LookupRetrieval lookupRetrieval)
        {
            const string METHOD_NAME = "AddDynamicColumns";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                DataItemField dataItemField;
                TestSetManager testSetManager = new TestSetManager();
                List<ArtifactListFieldDisplay> artifactFields = testSetManager.RetrieveTestSetTestCaseFieldsForList(projectId, projectTemplateId, userId);
                int visibleColumnCount = 0;
                foreach (ArtifactListFieldDisplay artifactField in artifactFields)
                {
                    //Only show visible columns unless we have a user id = UserInternal
                    if (artifactField.IsVisible || userId == Business.UserManager.UserInternal)
                    {
                        visibleColumnCount++;
                        //We need to get the datatype of this field
                        string fieldName = artifactField.Name;
                        string lookupField = artifactField.LookupProperty;

                        //Create the template item field
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = fieldName;
                        dataItemField.AllowDragAndDrop = true;
                        dataItemField.FieldType = (DataModel.Artifact.ArtifactFieldTypeEnum)artifactField.ArtifactFieldTypeId;

                        //Populate the shape depending on the type of field
                        switch (dataItemField.FieldType)
                        {
                            case DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup:
                                dataItemField.LookupName = lookupField;

                                //Set the list of possible lookup values
                                if (lookupRetrieval != null)
                                {
                                    dataItemField.Lookups = lookupRetrieval(fieldName, projectId, projectTemplateId);
                                }
                                break;

                            case DataModel.Artifact.ArtifactFieldTypeEnum.Lookup:
                                {
                                    dataItemField.LookupName = lookupField;

                                    //Set the list of possible lookup values
                                    if (lookupRetrieval != null)
                                    {
                                        dataItemField.Lookups = lookupRetrieval(fieldName, projectId, projectTemplateId);
                                    }
                                }
                                break;

                            case DataModel.Artifact.ArtifactFieldTypeEnum.MultiList:
                                {
                                    //Set the list of possible lookup values
                                    if (lookupRetrieval != null)
                                    {
                                        dataItemField.Lookups = lookupRetrieval(fieldName, projectId, projectTemplateId);
                                    }
                                }
                                break;

                            case DataModel.Artifact.ArtifactFieldTypeEnum.CustomPropertyLookup:
                            case DataModel.Artifact.ArtifactFieldTypeEnum.CustomPropertyMultiList:
                                //Set the list of possible lookup values
                                if (lookupRetrieval != null)
                                {
                                    dataItemField.Lookups = lookupRetrieval(fieldName, projectId, projectTemplateId);
                                }
                                break;

                            case DataModel.Artifact.ArtifactFieldTypeEnum.Flag:
                                //The flag need to be a drop-down despite being text
                                if (lookupRetrieval != null)
                                {
                                    dataItemField.Lookups = lookupRetrieval(fieldName, projectId, projectTemplateId);
                                }
                                break;
                        }

                        //See if we have a localized caption, otherwise use the default
                        //For the primary key fields, need to always use the localized name for ID
                        string localizedName = Resources.Fields.ResourceManager.GetString(fieldName);
                        if (dataItemField.FieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Identifier)
                        {
                            localizedName = Resources.Fields.ID;
                        }
                        if (String.IsNullOrEmpty(localizedName))
                        {
                            dataItemField.Caption = artifactField.Caption;
                        }
                        else
                        {
                            dataItemField.Caption = localizedName;
                        }
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
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
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="testSetTestCase">The datarow containing the data</param>
        /// <param name="position">The position of the datarow</param>
        /// <param name="artifactCustomProperty">The test case artifact custom properties</param>
        /// <param name="customProperties">The test case custom property definitions</param>
        /// <param name="editable">Do we want the data in editable format</param>
        protected void PopulateRow(int projectId, OrderedDataItem dataItem, TestSetTestCaseView testSetTestCase, int position, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty)
        {
            //Set the primary key
            dataItem.PrimaryKey = testSetTestCase.TestSetTestCaseId;

            //Specify if it has an attachment or not
            dataItem.Attachment = testSetTestCase.IsAttachments;

            //Specify if it has test steps or not
            dataItem.Alternate = testSetTestCase.IsTestSteps;

            //Specify its position
            dataItem.Position = position;

            //Specify the custom url, using the TestCaseId not the TestSetTestCaseId (primary key)
            dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(HttpContext.Current.Request.ApplicationPath, UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, (int)testSetTestCase["ProjectId"], (int)testSetTestCase["TestCaseId"], GlobalFunctions.PARAMETER_TAB_TESTRUN) + "?" + GlobalFunctions.PARAMETER_REFERER_TEST_SET_DETAILS + "=" + (int)testSetTestCase["TestSetId"]);

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (testSetTestCase.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the entity
                    PopulateFieldRow(dataItem, dataItemField, testSetTestCase, customProperties, artifactCustomProperty, editable, null);

                    //Only the Owner field is editable
                    dataItemField.Editable = (fieldName == "OwnerId");
                }

                //Apply the conditional formatting to the priority column and execution status columns
                if (dataItemField.FieldName == "TestCasePriorityId" && testSetTestCase.TestCasePriorityId.HasValue)
                {
                    dataItemField.CssClass = "#" + testSetTestCase.TestCasePriorityColor;
                }
                if (dataItemField.FieldName == "ExecutionStatusId" && testSetTestCase.ExecutionStatusId.HasValue)
                {
                    dataItemField.CssClass = GlobalFunctions.GetExecutionStatusCssClass(testSetTestCase.ExecutionStatusId.Value);
                    //Also add code so that you can redirect to the appropriate test run (unless not run)
                    if (testSetTestCase.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.NotRun)
                    {
                        dataItemField.Tooltip = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSetTestCaseRuns, projectId, testSetTestCase.TestSetTestCaseId));
                    }
                }

                //Handle the parameters separately since it will be formatted HTML
                if (dataItemField.FieldName == "Parameters")
                {
                    //Now we need to check for parameters, and render as an HTML table
                    List<TestSetTestCaseParameter> testSetCaseParameterValues = new TestSetManager().RetrieveTestCaseParameterValues(testSetTestCase.TestSetTestCaseId);
                    if (testSetCaseParameterValues.Count > 0)
                    {
                        string markup = @"<table class=""parameter-table""><tbody>";
                        foreach (TestSetTestCaseParameter testSetCaseParameterValue in testSetCaseParameterValues)
                        {
                            markup += "<tr><td>" + testSetCaseParameterValue.Name + "</td><td>" + testSetCaseParameterValue.Value + "</td></tr>\n";
                        }

                        markup += @"</tbody></table>";
                        dataItemField.TextValue = markup;
                    }
                }
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
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_TEST_CASE_PAGINATION);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return paginationDictionary;
        }

        /// <summary>
        /// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
        /// </summary>
        /// <param name="testSetTestCaseId">The id of the test case record in the test set to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        /// <remarks>This also includes the folders as that makes it easier in the test set page</remarks>
        public string RetrieveNameDesc(int? projectId, int testSetTestCaseId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the test set and test case business objects
                TestSetManager testSetManager = new TestSetManager();
                TestCaseManager testCaseManager = new TestCaseManager();

                //Now retrieve the specific test case - handle quietly if it doesn't exist
                try
                {
                    //First we need to get the test set test case case record itself
                    TestSetTestCase testSetTestCase = testSetManager.RetrieveTestCaseById(testSetTestCaseId);

                    //Next we need to get the list of successive parent folders
                    TestCase testCase = testSetTestCase.TestCase;
                    string tooltip = "";
                    if (testCase.TestCaseFolderId.HasValue)
                    {
                        List<TestCaseFolderHierarchyView> parentTestCaseFolders = testCaseManager.TestCaseFolder_GetParents(testCase.ProjectId, testCase.TestCaseFolderId.Value, true);
                        foreach (TestCaseFolderHierarchyView testFolderRow in parentTestCaseFolders)
                        {
                            tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testFolderRow.Name) + "</u> &gt; ";
                        }
                    }

                    //Now we need to get the test case itself
                    tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testCase.Name) + "</u>";
                    if (!String.IsNullOrEmpty(testCase.Description))
                    {
                        tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testCase.Description);
                    }
                    
                    //See if this test case has any parameter values set
                    List<TestSetTestCaseParameter> testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);
                    if (testSetTestCaseParameters.Count > 0)
                    {
                        tooltip += "<i>";
                        for (int i = 0; i < testSetTestCaseParameters.Count; i++)
                        {
                            if (i == 0)
                            {
                                tooltip += "<br />" + Resources.Main.Global_With + " ";
                            }
                            else
                            {
                                tooltip += ", ";
                            }
                            tooltip += testSetTestCaseParameters[i].Name + "=" + testSetTestCaseParameters[i].Value;
                        }
                        tooltip += "</i>";
                    }

                    //Get the default parameters for this test case
                    List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(testCase.TestCaseId, true, true);
                    
                    //Display any that are not overriden by the test set
                    bool first = true;
                    if (testCaseParameters.Count > 0)
                    {
                        tooltip += "<i>";
                        for (int i = 0; i < testCaseParameters.Count; i++)
                        {
                            if (!String.IsNullOrEmpty(testCaseParameters[i].DefaultValue) && !testSetTestCaseParameters.Any(t => t.Name == testCaseParameters[i].Name))
                            {
                                if (first)
                                {
                                    tooltip += "<br />" + Resources.Fields.Parameters + ": ";
                                    first = false;
                                }
                                else
                                {
                                    tooltip += ", ";
                                }
                                tooltip += testCaseParameters[i].Name + "=[" + testCaseParameters[i].DefaultValue + "]";
                            }
                        }
                        tooltip += "</i>";
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the test case, but it has already been deleted on the server
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for test case");
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
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_TEST_CASE_PAGINATION);
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
        /// Removes the set of test cases from the specified test set
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items being removed</param>
        /// <param name="standardFilters">The id of the test set the test cases belong to passed in as a filter (key = 'TestSetId')</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The test set needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a TestSetId as a standard filter");
                }
                if (!standardFilters.ContainsKey("TestSetId"))
                {
                    throw new ArgumentException("You need to provide a TestSetId as a standard filter");
                }
                int testSetId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestSetId"]);

                //Iterate through all the items to be removed and convert to ints
                List<int> testSetTestCaseIds = new List<int>();
                foreach (string itemValue in items)
                {
                    //Get the test case ID
                    int testSetTestCaseId = Int32.Parse(itemValue);
                    testSetTestCaseIds.Add(testSetTestCaseId);
                }

                //Now do the removal
                if (testSetTestCaseIds.Count > 0)
                {
                    TestSetManager testSetManager = new TestSetManager();
                    testSetManager.RemoveTestCase(projectId, testSetId, testSetTestCaseIds);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Changes the position of a test case in the test set
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sourceItems">The items to move</param>
        /// <param name="destTestCaseId">The destination item's id (or null for no destination selected)</param>
        /// <param name="standardFilters">The id of the test set the test cases belong to passed in as a filter (key = 'TestSetId')</param>
        public void OrderedList_Move(int projectId, JsonDictionaryOfStrings standardFilters, List<string> sourceItems, int? destTestCaseId)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The test set needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException("You need to provide a TestSetId as a standard filter");
                }
                if (!standardFilters.ContainsKey("TestSetId"))
                {
                    throw new ArgumentException("You need to provide a TestSetId as a standard filter");
                }
                int testSetId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestSetId"]);

                //Iterate through all the items to be moved and perform the operation
                TestSetManager testSetManager = new TestSetManager();
                //Check to make sure we don't have any duplicates
                List<int> existingIds = new List<int>();
                foreach (string itemValue in sourceItems)
                {
                    //Get the source ID
                    int sourceTestCaseId = Int32.Parse(itemValue);
                    if (!existingIds.Contains(sourceTestCaseId))
                    {
                        testSetManager.MoveTestCase(testSetId, sourceTestCaseId, destTestCaseId);
                        existingIds.Add(sourceTestCaseId);
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
        /// Returns the latest information on a single test case in the test set
        /// </summary>
        /// <param name="testSetTestCaseId">The id of the particular test case in the test set we want to retrieve</param>
        /// <param name="userId">The user we're viewing the test cases as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for TestSetId</param>
        /// <returns>A single dataitem object</returns>
        public OrderedDataItem OrderedList_Refresh(int projectId, JsonDictionaryOfStrings standardFilters, int testSetTestCaseId)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                TestSetManager testSetManager = new TestSetManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //The test set needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException(Resources.Messages.TestSetTestCaseService_MissingTestSetIdStdFilter);
                }
                if (!standardFilters.ContainsKey("TestSetId"))
                {
                    throw new ArgumentException(Resources.Messages.TestSetTestCaseService_MissingTestSetIdStdFilter);
                }
                int testSetId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestSetId"]);

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Create the data item record
                OrderedDataItem dataItem = new OrderedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, dataItem);

                //Get the test case dataset record for the specific test set and test case entry
                TestSetTestCaseView testSetTestCase = testSetManager.RetrieveTestCaseById2(testSetTestCaseId);
                if (testSetTestCase == null)
                {
                    throw new ArtifactNotExistsException("Unable to locate test case with testSetTestCaseId=" + testSetTestCaseId + " in the test set. It no longer exists!");
                }

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && testSetTestCase.OwnerId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Get the custom property info
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, true, false);

                //Finally populate the dataitem from the dataset
                PopulateRow(projectId, dataItem, testSetTestCase, testSetTestCase.Position, customProperties, true, null);

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
        /// Inserts a list of test cases into the test set at a position before the existing selected item
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectId"></param>
        /// <param name="artifact">The type of artifact we're inserting (only TestCase supported)</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for TestSetId and the test cases to add</param>
        /// <param name="artifactId">The id of the existing test set's test case record we're inserting in front of (-1 for none)</param>
        /// <returns>Always -1 since we are adding multiple items</returns>
        public int OrderedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? artifactId)
        {
            const string METHOD_NAME = "Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                if (artifact == "TestCase")
                {
                    //The test set needs to be passed in as a standard filter
                    if (standardFilters == null)
                    {
                        throw new ArgumentException(Resources.Messages.TestSetTestCaseService_MissingTestSetIdStdFilter);
                    }
                    if (!standardFilters.ContainsKey("TestSetId"))
                    {
                        throw new ArgumentException(Resources.Messages.TestSetTestCaseService_MissingTestSetIdStdFilter);
                    }
                    int testSetId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestSetId"]);

                    //The test cases we're adding to needs to be passed in as an additional 'standard filter'
                    if (!standardFilters.ContainsKey("AddedTestCaseIds"))
                    {
                        throw new ArgumentException(Resources.Messages.TestSetTestCaseService_MissingAddedTestCasesStdFilter);
                    }
                    string addedTestCaseIds = (string)GlobalFunctions.DeSerializeValue(standardFilters["AddedTestCaseIds"]);

                    //Convert the added test case ids into an array of ids
                    string[] items = addedTestCaseIds.Split(',');
                    List<int> testCaseIds = items.Select(Int32.Parse).ToList();

                    //Check to see if we are inserting before an existing test case or simply adding at the end
                    TestSetManager testSetManager = new TestSetManager();

                    //Call the BO insert passing the appropriate existing item if it appropriate
                    testSetManager.AddTestCases(
                        projectId,
                        testSetId,
                        testCaseIds,
                        null,
                        artifactId
                        );

                    return -1;
                }
                else
                {
                    throw new ArgumentException("The artifact '" + artifact + "' is not supported by this service.");
                }
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
        /// <param name="standardFilters">A standard filters collection that contains a value for TestSetId</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestCase);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Used to store any validation messages
            List<ValidationMessage> validationMessages = new List<ValidationMessage>();

            try
            {
                //The test set needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException(Resources.Messages.TestSetTestCaseService_MissingTestSetIdStdFilter);
                }
                if (!standardFilters.ContainsKey("TestSetId"))
                {
                    throw new ArgumentException(Resources.Messages.TestSetTestCaseService_MissingTestSetIdStdFilter);
                }
                int testSetId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestSetId"]);

                //The only editable field is the 'Owner' dropdown list, so no validation needs to be performed

                //Iterate through each data item and make the updates
                TestSetManager testSetManager = new TestSetManager();

                //See if we are viewing for a specific release or not
                int filterReleaseId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST, "ReleaseId", -1);

                //Get the test case dataset record for the specific test set and test case entry
                List<TestSetTestCase> testSetTestCases = testSetManager.RetrieveTestCases2(testSetId);

                foreach (OrderedDataItem dataItem in dataItems)
                {
                    //Get the test set test case id
                    int testSetTestCaseId = dataItem.PrimaryKey;

                    //Locate the existing record - and make sure it still exists
                    TestSetTestCase testSetTestCase = testSetTestCases.FirstOrDefault(t => t.TestSetTestCaseId == testSetTestCaseId);
                    if (testSetTestCase == null)
                    {
                        throw new ArtifactNotExistsException("Unable to locate test case with testSetTestCaseId=" + testSetTestCaseId + " in the test set. It no longer exists!");
                    }

                    //Start Tracking Changes
                    testSetTestCase.StartTracking();

                    //Update the field values
                    UpdateFields(validationMessages, dataItem, testSetTestCase, null, null, projectId, testSetTestCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase);
                }

                //Persist the changes to the database unless we have validation messages
                if (validationMessages.Count == 0)
                {
                    testSetManager.UpdateTestCases(testSetTestCases, userId);
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
        /// Toggles which columns are hidden/visible for the current user/project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="fieldName">The field name to toggle show/hide</param>
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
                //Toggle the status of the appropriate field name
                TestSetManager testSetManager = new TestSetManager();
                testSetManager.ToggleColumnVisibility(userId, projectId, fieldName);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }


        public void OrderedList_Copy(int projectId, JsonDictionaryOfStrings standardFilters, List<string> items)
        {
            throw new NotImplementedException();
        }
    }
}
