using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the OrderedGrid AJAX component for displaying/updating the list of test configurations in a test configuration set
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class TestConfigurationService : ListServiceBase, ITestConfigurationService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TestConfigurationService::";

        #region IOrderedList Methods

        /// <summary>
        /// Returns a list of test configurations in the test configuration set
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for TestConfigurationSetId</param>
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

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();

                //Create the array of data items
                OrderedData orderedData = new OrderedData();
                List<OrderedDataItem> dataItems = orderedData.Items;

                //The test configuration set needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException(Resources.Messages.TestConfigurationService_MissingTestConfigurationSetIdStdFilter);
                }
                if (!standardFilters.ContainsKey("TestConfigurationSetId"))
                {
                    throw new ArgumentException(Resources.Messages.TestConfigurationService_MissingTestConfigurationSetIdStdFilter);
                }
                int testConfigurationSetId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestConfigurationSetId"]);

                //Get the test set list of test configurations
                //Since the 'shape' of columns is dynamic (we have a crosstab) we need the data to create the shape
                List<TestConfigurationEntry> testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId, testConfigurationSetId);

                if (testConfigurationEntries != null)
                {
                    //Create the first 'shape' item, we can clone others from it later
                    OrderedDataItem shapeItem = new OrderedDataItem();
                    List<IGrouping<int, TestConfigurationEntry>> testConfigurationsGroupedByParameter = testConfigurationEntries.GroupBy(t => t.TestCaseParameterId).ToList();
                    List<IGrouping<int, TestConfigurationEntry>> testConfigurationsGroupedByConfiguration = testConfigurationEntries.GroupBy(t => t.TestCaseConfigurationId).ToList();
                    PopulateShape(projectId, userId, shapeItem, testConfigurationsGroupedByParameter);
                    dataItems.Add(shapeItem);

                    //Now get the pagination information and add to the shape item
                    ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_GENERAL);
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
                    int artifactCount = testConfigurationsGroupedByConfiguration.Count;
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
                        IGrouping<int, TestConfigurationEntry> testConfiguration = testConfigurationsGroupedByConfiguration[i];

                        //We clone the template/shape item as the basis of all the new items
                        OrderedDataItem dataItem = shapeItem.Clone();

                        //Now populate with the data
                        PopulateRow(projectId, dataItem, testConfiguration);
                        dataItems.Add(dataItem);
                    }

                    //Add the pagination information
                    orderedData.PaginationOptions = this.RetrievePaginationOptions(projectId);
                    orderedData.TotalCount = artifactCount;
                    orderedData.VisibleCount = dataItems.Count - 1;

                    Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created data items with " + dataItems.Count.ToString() + " rows");
                }

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
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_GENERAL);

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
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_GENERAL);
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
        /// Removes the specified test configurations from the specified test configuration set
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items being removed</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for TestConfigurationSetId</param>
        public void OrderedList_Delete(int projectId, JsonDictionaryOfStrings standardFilters, List<string> items)
        {
            const string METHOD_NAME = "OrderedList_Delete";

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
                //The test configuration set needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException(Resources.Messages.TestConfigurationService_MissingTestConfigurationSetIdStdFilter);
                }
                if (!standardFilters.ContainsKey("TestConfigurationSetId"))
                {
                    throw new ArgumentException(Resources.Messages.TestConfigurationService_MissingTestConfigurationSetIdStdFilter);
                }
                int testConfigurationSetId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestConfigurationSetId"]);

                //Iterate through all the items to be removed and convert to ints
                List<int> testConfigurationIds = new List<int>();
                foreach (string itemValue in items)
                {
                    //Get the test configuration ID
                    int testConfigurationId = Int32.Parse(itemValue);
                    testConfigurationIds.Add(testConfigurationId);
                }

                //Now do the removal
                if (testConfigurationIds.Count > 0)
                {
                    TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
                    testConfigurationManager.RemoveTestConfigurationsFromSet(projectId, testConfigurationSetId, testConfigurationIds);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Changes the position of test configurations in the test configuration set
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sourceItems">The items to move</param>
        /// <param name="destTestConfigurationId">The destination item's id (or null for no destination selected)</param>
        /// <param name="standardFilters">A standard filters collection that contains a value for TestConfigurationSetId</param>
        public void OrderedList_Move(int projectId, JsonDictionaryOfStrings standardFilters, List<string> sourceItems, int? destTestConfigurationId)
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
                //The test configuration set needs to be passed in as a standard filter
                if (standardFilters == null)
                {
                    throw new ArgumentException(Resources.Messages.TestConfigurationService_MissingTestConfigurationSetIdStdFilter);
                }
                if (!standardFilters.ContainsKey("TestConfigurationSetId"))
                {
                    throw new ArgumentException(Resources.Messages.TestConfigurationService_MissingTestConfigurationSetIdStdFilter);
                }
                int testConfigurationSetId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestConfigurationSetId"]);

                //Iterate through all the items to be moved and perform the operation
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
                foreach (string itemValue in sourceItems)
                {
                    //Get the source ID
                    int sourceTestConfigurationId = Int32.Parse(itemValue);
                    testConfigurationManager.MoveTestConfigurations(testConfigurationSetId, sourceTestConfigurationId, destTestConfigurationId);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }


        #endregion

        #region ITestConfigurationService Methods

        /// <summary>
        /// Populates a new set of test configurations from the provided test case parameters and custom lists
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testConfigurationSetId">The id of the test configuration set to populate</param>
        /// <param name="testParameters">The data to populate it with</param>
        public void TestConfiguration_Populate(int projectId, int testConfigurationSetId, JsonDictionaryOfStrings testParameters)
        {
            const string METHOD_NAME = "TestConfiguration_RetrieveParameters";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Make sure we have at least 1 parameter
                if (testParameters == null || testParameters.Count < 1)
                {
                    throw new InvalidOperationException(Resources.Messages.TestConfigurationService_NoParametersSpecified);
                }

                //Make sure they are not trying to add more than MAX_COUNT parameters
                const int MAX_COUNT = 5;
                if (testParameters.Count > MAX_COUNT)
                {
                    throw new InvalidOperationException(String.Format(Resources.Messages.TestConfigurationService_TooManyParametersSpecified, MAX_COUNT));
                }

                //Convert into a typed dictionary
                Dictionary<int,int> testParametersDic = new Dictionary<int,int>();
                foreach (KeyValuePair<string, string> testParameter in testParameters)
                {
                    int parameterId;
                    int customListId;
                    if (Int32.TryParse(testParameter.Key, out parameterId) && Int32.TryParse(testParameter.Value, out customListId))
                    {
                        testParametersDic.Add(parameterId, customListId);
                    }
                }

                //Do the population
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
                testConfigurationManager.PopulateTestConfigurations(projectId, testConfigurationSetId, testParametersDic);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of all test case parameters in the project, used in the test configurations page
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The list of uniquely named parameters</returns>
        /// <remarks>Ignore multiple parameters that share the same name</remarks>
        public JsonDictionaryOfStrings TestConfiguration_RetrieveParameters(int projectId)
        {
            const string METHOD_NAME = "TestConfiguration_RetrieveParameters";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Create the dictionary to store the parameter id and name
                JsonDictionaryOfStrings availableParameters = new JsonDictionaryOfStrings();

                //Get the list of parameters for all test cases in the project
                TestCaseManager testCaseManager = new TestCaseManager();

                //First get the list of test parameters
                List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveAllParameters(projectId);

                //Loop through the list and only add one with each name (since we match on name for test configurations)
                foreach (TestCaseParameter testCaseParameter in testCaseParameters)
                {
                    if (!availableParameters.ContainsValue(testCaseParameter.Name))
                    {
                        availableParameters.Add(testCaseParameter.TestCaseParameterId.ToString(), testCaseParameter.Name);
                    }
                }

                return availableParameters;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of all active custom lists in the specified project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The list of all active custom lists in the project</returns>
        public JsonDictionaryOfStrings TestConfiguration_RetrieveCustomLists(int projectId)
        {
            const string METHOD_NAME = "TestConfiguration_RetrieveCustomLists";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Create the dictionary to store the parameter id and name
                JsonDictionaryOfStrings availableCustomLists = new JsonDictionaryOfStrings();

                //Get the list of active custom lists in the project
                CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

                //First get the list of custom property lists
                List<CustomPropertyList> customPropertyLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(projectTemplateId).OrderBy(c => c.Name).ToList();

                //Loop through and populate the dictionary
                foreach (CustomPropertyList customPropertyList in customPropertyLists)
                {
                    availableCustomLists.Add(customPropertyList.CustomPropertyListId.ToString(), customPropertyList.Name);
                }

                return availableCustomLists;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the test cases as</param>
        /// <param name="groups">The test configurations grouped by parameter</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        protected void PopulateShape(int projectId, int userId, OrderedDataItem dataItem, IEnumerable<IGrouping<int, TestConfigurationEntry>> groups)
        {
            //First add the dynamic crosstab columns
            DataItemField dataItemField;
            foreach (IGrouping<int, TestConfigurationEntry> group in groups)
            {
                TestConfigurationEntry firstEntry = group.FirstOrDefault();
                if (firstEntry != null)
                {
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "entry_" + group.Key;
                    dataItemField.Caption = TestCaseManager.CreateParameterToken(firstEntry.ParameterName);
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                }
            }

            //Next add the ID column
            dataItemField = new DataItemField();
            dataItemField.FieldName = "TestConfigurationId";
            dataItemField.Caption = Resources.Fields.ID;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
        }

        /// <summary>
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="testConfiguration">The group containing the test configuration data</param>
        protected void PopulateRow(int projectId, OrderedDataItem dataItem, IGrouping<int, TestConfigurationEntry> testConfiguration)
        {
            //Set the primary key
            dataItem.PrimaryKey = testConfiguration.Key;

            //Specify its position
            DataItemField dataItemField;
            TestConfigurationEntry testConfigurationEntryFirst = testConfiguration.FirstOrDefault();
            if (testConfigurationEntryFirst != null)
            {
                dataItem.Position = testConfigurationEntryFirst.Position;

                //Iterate through all the parameters for this configuration and populate them
                foreach (TestConfigurationEntry testConfigurationEntry in testConfiguration)
                {
                    string fieldName = "entry_" + testConfigurationEntry.TestCaseParameterId;
                    dataItemField = dataItem.Fields[fieldName];
                    if (dataItemField != null)
                    {
                        dataItemField.TextValue = testConfigurationEntry.ParameterValue;
                    }
                }
            }

            //Set the ID
            dataItemField = dataItem.Fields["TestConfigurationId"];
            if (dataItemField != null)
            {
                dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_ID, testConfiguration.Key);
                dataItemField.IntValue = testConfiguration.Key;
            }
        }

        #endregion

        #region Not Implemented Methods

        public List<ValidationMessage> OrderedList_Update(int projectId, JsonDictionaryOfStrings standardFilters, List<OrderedDataItem> dataItems)
        {
            throw new NotImplementedException();
        }

        public int OrderedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? artifactId)
        {
            throw new NotImplementedException();
        }

        public void OrderedList_Copy(int projectId, JsonDictionaryOfStrings standardFilters, List<string> items)
        {
            throw new NotImplementedException();
        }

        public void ToggleColumnVisibility(int projectId, string fieldName)
        {
            throw new NotImplementedException();
        }

        public OrderedDataItem OrderedList_Refresh(int projectId, JsonDictionaryOfStrings standardFilters, int artifactId)
        {
            throw new NotImplementedException();
        }

        public string RetrieveNameDesc(int? projectId, int artifactId, int? displayTypeId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
