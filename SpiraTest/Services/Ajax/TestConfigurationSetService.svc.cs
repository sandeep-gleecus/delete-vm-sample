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
    public class TestConfigurationSetService : SortedListServiceBase, ITestConfigurationSetService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TestConfigurationSetService::";

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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Used to store any validation messages
            List<ValidationMessage> validationMessages = new List<ValidationMessage>();

            try
            {
                //Iterate through each data item and make the updates
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();

                foreach (SortedDataItem dataItem in dataItems)
                {
                    //Get the testConfigurationSet id
                    int testConfigurationSetId = dataItem.PrimaryKey;

                    //Retrieve the existing record - and make sure it still exists
                    TestConfigurationSet testConfigurationSet = testConfigurationManager.RetrieveSetById(testConfigurationSetId);

                    if (testConfigurationSet != null)
                    {
                        //Need to set the original date of this record to match the concurrency date
                        if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                        {
                            DateTime concurrencyDateTimeValue;
                            if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                            {
                                testConfigurationSet.ConcurrencyDate = concurrencyDateTimeValue;
                                testConfigurationSet.AcceptChanges();
                            }
                        }

                        //Update the field values
                        List<string> fieldsToIgnore = new List<string>();
                        fieldsToIgnore.Add("CreationDate");
                        UpdateFields(validationMessages, dataItem, testConfigurationSet, null, null, projectId, testConfigurationSetId, 0, fieldsToIgnore);

                        //Make sure we have no validation messages before updating
                        if (validationMessages.Count == 0)
                        {
                            //Persist to database, catching any business exceptions and displaying them
                            try
                            {
                                testConfigurationManager.UpdateSet(testConfigurationSet, userId);
                            }
                            catch (DataValidationException exception)
                            {
                                return CreateSimpleValidationMessage(exception.Message);
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
        /// Returns a list of testConfigurationSets in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the testConfigurationSets as</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business objects
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();

                //Create the array of data items (including the first filter item)
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;

                //Now get the list of populated filters and the current sort
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_FILTERS);
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "TestConfigurationSetId ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");
                sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestConfigurationSet);

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
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL);
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
                int artifactCount = testConfigurationManager.CountSets(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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
                List<TestConfigurationSet> testConfigurationSets = testConfigurationManager.RetrieveSets(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //Display the pagination information
                sortedData.CurrPage = currentPage;
                sortedData.PageCount = pageCount;
                sortedData.StartRow = startRow;

                //Display the visible and total count of artifacts
                sortedData.VisibleCount = testConfigurationSets.Count;
                sortedData.TotalCount = artifactCount;

                //Display the sort information
                sortedData.SortProperty = sortProperty;
                sortedData.SortAscending = sortAscending;

                //Iterate through all the automation hosts and populate the dataitem
                foreach (TestConfigurationSet testConfigurationSet in testConfigurationSets)
                {
                    //We clone the template item as the basis of all the new items
                    SortedDataItem dataItem = filterItem.Clone();

                    //Now populate with the data
                    PopulateRow(dataItem, testConfigurationSet, false);
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
        /// <param name="testConfigurationSetId">The id of the testConfigurationSet to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        public string RetrieveNameDesc(int? projectId, int testConfigurationSetId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the automation business object
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();

                //Now retrieve the specific testConfigurationSet - handle quietly if it doesn't exist
                try
                {
                    TestConfigurationSet testConfigurationSet = testConfigurationManager.RetrieveSetById(testConfigurationSetId);
                    string tooltip;
                    if (String.IsNullOrEmpty(testConfigurationSet.Description))
                    {
                        tooltip = GlobalFunctions.HtmlRenderAsPlainText(testConfigurationSet.Name);
                    }
                    else
                    {
                        tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testConfigurationSet.Name) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testConfigurationSet.Description);
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the testConfigurationSet, but it has already been deleted on the server
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
            return base.UpdateSort(userId, projectId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL);
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

            //Call the base method with the appropriate settings collection
            return base.UpdateFilters(userId, projectId, filters, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_FILTERS, DataModel.Artifact.ArtifactTypeEnum.TestConfigurationSet);
        }

        /// <summary>
        /// Returns the latest information on a single testConfigurationSet in the system
        /// </summary>
        /// <param name="userId">The user we're viewing the testConfigurationSet as</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business objects
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

                //Get the testConfigurationSet entity record for the specific testConfigurationSet id
                TestConfigurationSet testConfigurationSet = testConfigurationManager.RetrieveSetById(artifactId);

                //Finally populate the dataitem from the dataset
                if (testConfigurationSet != null)
                {
                    PopulateRow(dataItem, testConfigurationSet, true);
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
        /// Deletes a set of testConfigurationSets
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Iterate through all the items to be deleted
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
                foreach (string itemValue in items)
                {
                    //Get the automation Host ID
                    int testConfigurationSetId = Int32.Parse(itemValue);
                    testConfigurationManager.MarkSetAsDeleted(projectId, testConfigurationSetId, userId);
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
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL);

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
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL);
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
        /// Inserts a new testConfigurationSet into the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="artifact">The type of artifact we're inserting</param>
        /// <param name="standardFilters">Any standard filters that are set by the page</param>
        /// <returns>The id of the new testConfigurationSet</returns>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business object(s)
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();

                //Simply insert the new item into the list
                int testConfigurationSetId = testConfigurationManager.InsertSet(
                    projectId,
                    "",
                    null,
                    true,
                    userId
                    );

                TestConfigurationSet testConfigurationSet = testConfigurationManager.RetrieveSetById(testConfigurationSetId);
                if (testConfigurationSet != null)
                {

                    //If we have filters currently applied to the view, then we need to set this new testConfigurationSet to the same value
                    //(if possible) so that it will show up in the list
                    ProjectSettingsCollection filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_FILTERS);
                    if (filterList.Count > 0)
                    {
                        List<string> fieldsToIgnore = new List<string>() { "TestConfigurationSetId", "CreationDate", "Name" };
                        UpdateToMatchFilters(projectId, filterList, testConfigurationSetId, testConfigurationSet, null, fieldsToIgnore);
                        testConfigurationManager.UpdateSet(testConfigurationSet, userId);
                    }
                }

                return testConfigurationSetId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
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
        /// Returns a list of configuration sets for display in the navigation bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="indentLevel">Not used for configuration sets since not hierarchical</param>
        /// <returns>List of test configuration sets</returns>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business object
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();

                //Create the array of data items
                List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_FILTERS);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "AutomationHostId ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL);
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
                //Get the number of configuration sets in the project
                int artifactCount = testConfigurationManager.CountSets(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //**** Now we need to actually populate the rows of data to be returned ****

                //Get the incidents list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }
                List<TestConfigurationSet> testConfigurationSets;
                if (displayMode == 2)
                {
                    //All Items
                    testConfigurationSets = testConfigurationManager.RetrieveSets(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }
                else
                {
                    //Filtered List
                    testConfigurationSets = testConfigurationManager.RetrieveSets(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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

                //Iterate through all the configuration sets and populate the dataitem (only some columns are needed)
                foreach (TestConfigurationSet automationHostView in testConfigurationSets)
                {
                    //Create the data-item
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();

                    //Populate the necessary fields
                    dataItem.PrimaryKey = automationHostView.TestConfigurationSetId;
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
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL);
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

        #region IFormService methods

        /// <summary>
        /// Deletes a single test configuration and returns the ID of the next one to redirect to (or null if none left)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactId">The id of the artifact to delete</param>
        /// <returns>The id of the artifact to redirect to</returns>
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

            //Make sure we're authorized to delete the item (test set since we don't have permissions for configurations)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //First we need to determine which artifact id to redirect the user to after the delete
                int? newTestConfigurationSetId = null;

                //Look through the current dataset to see what is the next automation host in the list
                //If we are the last one on the list then we need to simply use the one before

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_FILTERS);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "TestConfigurationSetId ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL);
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
                //Get the number of test configuration sets in the project
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
                int artifactCount = testConfigurationManager.CountSets(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                //Get the automation host list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }

                List<TestConfigurationSet> testConfigurationSets = testConfigurationManager.RetrieveSets(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                bool matchFound = false;
                int previousTestConfigurationSetId = -1;
                foreach (TestConfigurationSet testConfigurationSet in testConfigurationSets)
                {
                    int testConfigurationSetId = testConfigurationSet.TestConfigurationSetId;
                    if (testConfigurationSetId == artifactId)
                    {
                        matchFound = true;
                    }
                    else
                    {
                        //If we found a match on the previous iteration, then we want to this (next) task
                        if (matchFound)
                        {
                            newTestConfigurationSetId = testConfigurationSetId;
                            break;
                        }

                        //If this matches the current incident, set flag
                        if (testConfigurationSetId == artifactId)
                        {
                            matchFound = true;
                        }
                        if (!matchFound)
                        {
                            previousTestConfigurationSetId = testConfigurationSetId;
                        }
                    }
                }
                if (!newTestConfigurationSetId.HasValue && previousTestConfigurationSetId != -1)
                {
                    newTestConfigurationSetId = previousTestConfigurationSetId;
                }

                //Next we need to delete the current configuration set
                testConfigurationManager.MarkSetAsDeleted(projectId, artifactId, userId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return newTestConfigurationSetId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>Returns a single test configuration set data record (all columns) for use by the FormManager control</summary>
        /// <param name="artifactId">The id of the current test configuration set</param>
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

            //Make sure we're authorized (full edit only, since no owner/opener available)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business classes
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, false);

                //Get the specific test configuration set by its artifact id
                TestConfigurationSet testConfigurationSet = testConfigurationManager.RetrieveSetById(artifactId.Value);
                if (testConfigurationSet == null)
                {
                    //Just return no data back
                    return null;
                }

                //There are no custom properties or data mapping values to worry about for test configuration sets
                PopulateRow(dataItem, testConfigurationSet, true);

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return dataItem;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>Saves a single test configuration set</summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="dataItem">The item to save</param>
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

            //Make sure we're authorized (full only because test configuration sets don't have owners)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Get the test configuration set id
            int testConfigurationSetId = dataItem.PrimaryKey;

            try
            {
                //Instantiate the business classes
                TestConfigurationManager testConfigurationManager = new TestConfigurationManager();

                //No custom properties or data mapping to worry about

                //This service only supports updates, so we should get a automation host id that is valid

                //Retrieve the existing record - and make sure it still exists
                TestConfigurationSet testConfigurationSet = testConfigurationManager.RetrieveSetById(testConfigurationSetId);
                if (testConfigurationSet == null)
                {
                    //Let the user know that the ticket no inter exists
                    return CreateSimpleValidationMessage(String.Format(Resources.Messages.AutomationHostsService_AutomationHostNotFound, testConfigurationSetId));
                }

                //Need to set the original date of this record to match the concurrency date
                //The value is already in UTC so no need to convert
                if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                {
                    DateTime concurrencyDateTimeValue;
                    if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                    {
                        testConfigurationSet.ConcurrencyDate = concurrencyDateTimeValue;
                        testConfigurationSet.AcceptChanges();
                    }
                }

                //Now we can start tracking any changes
                testConfigurationSet.StartTracking();

                //Update the field values, tracking changes
                List<string> fieldsToIgnore = new List<string>();
                fieldsToIgnore.Add("CreationDate");
                fieldsToIgnore.Add("LastUpdatedDate");

                //Update the field values
                UpdateFields(validationMessages, dataItem, testConfigurationSet, null, null, projectId, testConfigurationSetId, DataModel.Artifact.ArtifactTypeEnum.TestConfigurationSet, fieldsToIgnore);

                //If we have validation messages, stop now
                if (validationMessages.Count > 0)
                {
                    return validationMessages;
                }

                //Update the test configuration set
                try
                {
                    testConfigurationManager.UpdateSet(testConfigurationSet, userId);
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

                //If we're asked to save and create a new test configuration set, need to do the insert and send back the new id
                if (operation == "new")
                {
                    //Simply insert the new item into the test configuration set list
                    int newTestConfigurationSetId = testConfigurationManager.InsertSet(
                        projectId,
                        "",
                        null,
                        true,
                        userId
                        );

                    //We need to encode the new artifact id as a 'pseudo' validation message
                    ValidationMessage newMsg = new ValidationMessage();
                    newMsg.FieldName = "$NewArtifactId";
                    newMsg.Message = newTestConfigurationSetId.ToString();
                    AddUniqueMessage(validationMessages, newMsg);
                }

                //Return back any messages. For success it should only contain a new artifact ID if we're inserting
                return validationMessages;
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


        #region Not Implemented Methods

        public void SortedList_Export(int destProjectId, List<string> items)
        {
            throw new NotImplementedException();
        }
        public void SortedList_Copy(int projectId, List<string> items)
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

        #region Internal Functions

        /// <summary>
        /// Populates a data item from an test configuration set entity
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="dataRow">The datarow containing the data</param>
        /// <param name="editable">Does the data need to be in editable form?</param>
        protected void PopulateRow(SortedDataItem dataItem, TestConfigurationSet testConfigurationSet, bool editable)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = testConfigurationSet.TestConfigurationSetId;
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, testConfigurationSet.ConcurrencyDate);

            //Specify if it has an attachment or not
            dataItem.Attachment = false;

            //The date and some other fields are not editable
            List<string> readOnlyFields = new List<string>() { "CreationDate", "LastUpdatedDate" };

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (testConfigurationSet.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, testConfigurationSet, null, null, editable, null, null, null, readOnlyFields);
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

                if (lookupName == "IsActive")
                {
                    TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
                    lookupValues = new JsonDictionaryOfStrings(testConfigurationManager.RetrieveFlagLookupDictionary());
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
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the testConfigurationSets as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        /// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
        protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
        {
            //First add the static columns
            //Name
            DataItemField dataItemField = new DataItemField();
            dataItemField.FieldName = "Name";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
            dataItemField.Caption = Resources.Fields.Name;
            dataItemField.Editable = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }

            //Creation Date
            dataItemField = new DataItemField();
            dataItemField.FieldName = "CreationDate";
            dataItemField.Caption = Resources.Fields.CreationDate;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                //Need to convert into the displayable date form
                Common.DateRange dateRange = (Common.DateRange)filterList[dataItemField.FieldName];
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

            //IsActive
            dataItemField = new DataItemField();
            dataItemField.FieldName = "IsActive";
            dataItemField.Caption = Resources.Fields.IsActive;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Flag;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the list of possible lookup values
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }

            //Last Update Date
            dataItemField = new DataItemField();
            dataItemField.FieldName = "LastUpdatedDate";
            dataItemField.Caption = Resources.Fields.LastUpdated;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                //Need to convert into the displayable date form
                Common.DateRange dateRange = (Common.DateRange)filterList[dataItemField.FieldName];
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

            //TestConfigurationSetId
            dataItemField = new DataItemField();
            dataItemField.FieldName = "TestConfigurationSetId";
            dataItemField.Caption = Resources.Fields.ID;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.IntValue = (int)filterList[dataItemField.FieldName];
            }

            //Populate the description
            if (!returnJustListFields)
            {
                //Name
                dataItemField = new DataItemField();
                dataItemField.FieldName = "Description";
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
                dataItemField.Caption = Resources.Fields.Description;
                dataItemField.Editable = true;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            }
        }

        #endregion
    }
}
