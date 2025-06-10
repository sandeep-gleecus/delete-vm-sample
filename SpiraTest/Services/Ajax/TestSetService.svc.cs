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
using System.Threading;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the HierarchicalGrid AJAX component for displaying/updating test set data
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class TestSetService : SortedListServiceBase, ITestSetService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TestSetService::";

        protected const string PROJECT_SETTINGS_PAGINATION = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_TEST_SET_PAGINATION_SIZE;

        /// <summary>
        /// Constructor
        /// </summary>
        public TestSetService()
        {
        }

        #region IItemSelector Methods

        /// <summary>
        /// Returns a list of test sets (just the basic name/id fields) for using in popup item selection dialog boxes
        /// </summary>
        /// <remarks>
        /// Does not return test set folders
        /// </remarks>
        /// <param name="projectId">The current project</param>
        /// <param name="standardFilters">Any standard filters (e.g. the folder)</param>
        public ItemSelectorData ItemSelector_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters)
        {
            const string METHOD_NAME = "ItemSelector_Retrieve";

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
                //Instantiate the test set business object
                TestSetManager testSetManager = new TestSetManager();

                //Create the array of data items
                ItemSelectorData itemSelectorData = new ItemSelectorData();
                List<DataItem> dataItems = itemSelectorData.Items;

                //See if we have a folder selected (otherwise root folder)
                int? folderId = null;
                if (standardFilters != null && standardFilters.Count > 0)
                {
                    Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
                    if (deserializedFilters.ContainsKey("TestSetFolderId") && deserializedFilters["TestSetFolderId"] is Int32)
                    {
                        folderId = (int)deserializedFilters["TestSetFolderId"];
                    }
                }

                //Get the test set list for the folder
                List<TestSetView> testSets = testSetManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);

                //Iterate through all the test sets and populate the dataitem (only some columns are needed)
                foreach (TestSetView testSet in testSets)
                {
                    //Create the data-item
                    DataItem dataItem = new DataItem();

                    //Populate the necessary fields
                    dataItem.PrimaryKey = testSet.TestSetId;

                    //Test Case Id
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Id";
                    dataItemField.IntValue = testSet.TestSetId;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Name/Desc
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = testSet.Name;
                    dataItem.Alternate = false;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Status
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "StatusId";
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Lookup;
                    dataItemField.IntValue = testSet.TestSetStatusId;
                    dataItemField.TextValue = testSet.TestSetStatusName;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Add to the items collection
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return itemSelectorData;
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
        /// Retrieves the list of comments associated with a test set
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactId">The id of the test set</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Create the new list of comments
                List<CommentItem> commentItems = new List<CommentItem>();

                //Get the test set (to verify permissions) and also the comments
                TestSetManager testSetManager = new TestSetManager();
                UserManager userManager = new UserManager();
                DiscussionManager discussion = new DiscussionManager();
                TestSetView testSet = testSetManager.RetrieveById(projectId, artifactId);
                List<IDiscussion> comments = discussion.Retrieve(artifactId, Artifact.ArtifactTypeEnum.TestSet).ToList();

                //Make sure the user is either the owner or author if limited permissions
                int ownerId = -1;
                if (testSet.OwnerId.HasValue)
                {
                    ownerId = testSet.OwnerId.Value;
                }
                if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && testSet.CreatorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //See if we're sorting ascending or descending
                SortDirection sortDirection = (SortDirection)GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)SortDirection.Descending);

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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Update the setting
                SortDirection sortDirection = (SortDirection)sortDirectionId;
                SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)sortDirectionId);
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
        /// <param name="artifactId">The id of the test set</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Delete the comment, making sure we have permissions
                DiscussionManager discussion = new DiscussionManager();
                IDiscussion comment = discussion.RetrieveById(commentId, Artifact.ArtifactTypeEnum.TestSet);
                //If the comment no longer exists do nothing
                if (comment != null && !comment.IsPermanent)
                {
                    if (comment.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin))
                    {
                        discussion.DeleteDiscussionId(commentId, Artifact.ArtifactTypeEnum.TestSet);
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
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
                int commentId = discussion.Insert(userId, artifactId, Artifact.ArtifactTypeEnum.TestSet, cleanedComment, projectId, false, true);

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

        #region IFormService methods

        /// <summary>Returns a single test set data record (all columns) for use by the FormManager control</summary>
        /// <param name="artifactId">The id of the current test set</param>
        /// <returns>A test set data item</returns>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business classes
                TestSetManager testSetManager = new TestSetManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, false);

                //Need to add the empty column to capture any new comments added
                if (!dataItem.Fields.ContainsKey("NewComment"))
                {
                    dataItem.Fields.Add("NewComment", new DataItemField() { FieldName = "NewComment", Required = false, Editable = true, Hidden = false });
                }

                //Get the test set for the specific test set id
                TestSetView testSetView = testSetManager.RetrieveById(projectId, artifactId.Value);

                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);

                //Make sure the user is authorized for this item
                int ownerId = -1;
                if (testSetView.OwnerId.HasValue)
                {
                    ownerId = testSetView.OwnerId.Value;
                }
                if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && testSetView.CreatorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //See if we are viewing for a specific release or not
                int? filterReleaseId = null;
                int filterReleaseIdSetting = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                if (filterReleaseIdSetting > 0)
                {
                    filterReleaseId = filterReleaseIdSetting;
                }

                //See if we have any existing artifact custom properties for this row
                if (artifactCustomProperty == null)
                {
                    List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, true, false);
                    PopulateRow(dataItem, testSetView, customProperties, true, (ArtifactCustomProperty)null, filterReleaseId);
                }
                else
                {
                    PopulateRow(dataItem, testSetView, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty, filterReleaseId);
                }

                //If we are filtering by release, add that as a field
                if (filterReleaseId.HasValue)
                {
                    try
                    {
                        Release displayRelease = new ReleaseManager().RetrieveById3(projectId, filterReleaseId.Value);
                        dataItem.Fields.Add("DisplayReleaseId", new DataItemField() { FieldName = "DisplayReleaseId", IntValue = displayRelease.ReleaseId, TextValue = displayRelease.VersionNumber + " - " + displayRelease.Name });
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //If release no longer exists, just do nothing
                    }
                }

                //Also need to return back a special field to denote if the user is the owner or creator of the artifact
                bool isArtifactCreatorOrOwner = (ownerId == userId || testSetView.CreatorId == userId);
                dataItem.Fields.Add("_IsArtifactCreatorOrOwner", new DataItemField() { FieldName = "_IsArtifactCreatorOrOwner", TextValue = isArtifactCreatorOrOwner.ToDatabaseSerialization() });

                //Populate any data mapping values are not part of the standard 'shape'
                if (artifactId.HasValue)
                {
                    DataMappingManager dataMappingManager = new DataMappingManager();
                    List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.TestSet, artifactId.Value);
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

                    //Populate the folder path as a special field
                    if (testSetView.TestSetFolderId.HasValue)
                    {
                        List<TestSetFolderHierarchyView> parentFolders = testSetManager.TestSetFolder_GetParents(testSetView.ProjectId, testSetView.TestSetFolderId.Value, true);
                        string pathArray = "[";
                        bool isFirst = true;
                        foreach (TestSetFolderHierarchyView parentFolder in parentFolders)
                        {
                            if (isFirst)
                            {
                                isFirst = false;
                            }
                            else
                            {
                                pathArray += ",";
                            }
                            pathArray += "{ \"name\": \"" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "\", \"id\": " + parentFolder.TestSetFolderId + " }";
                        }
                        pathArray += "]";
                        dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath", TextValue = pathArray });
                    }
                    else
                    {
                        //send a blank folder path object back so client knows this artifact has folders
                        dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath" });
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
        /// Deletes the current test set and returns the ID of the item to redirect to (if any)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Look through the current dataset to see what is the next test set in the list
                //If we are the last one on the list then we need to simply use the one before
                int? newTestSetId = null;

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Get the current folder
                int? folderId = null;
                int nodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (nodeId > 0)
                {
                    folderId = nodeId;
                }

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_TEST_SET_PAGINATION_SIZE);
                paginationSettings.Restore();
                //Default values
                int paginationSize = 500;
                int currentPage = 1;
                if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE] != null)
                {
                    paginationSize = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE];
                }
                if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] != null)
                {
                    currentPage = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE];

                }
                //Get the number of testSets in the project
                TestSetManager testSetManager = new TestSetManager();
                int artifactCount = testSetManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
                //Get the testSets list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }

                List<TestSetView> testSetNavigationList = testSetManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
                bool matchFound = false;
                int previousTestSetId = -1;
                foreach (TestSetView testSet in testSetNavigationList)
                {
                    int testTestSetId = testSet.TestSetId;
                    if (testTestSetId == artifactId)
                    {
                        matchFound = true;
                    }
                    else
                    {
                        //If we found a match on the previous iteration, then we want to this (next) testSet
                        if (matchFound)
                        {
                            newTestSetId = testTestSetId;
                            break;
                        }

                        //If this matches the current testSet, set flag
                        if (testTestSetId == artifactId)
                        {
                            matchFound = true;
                        }
                        if (!matchFound)
                        {
                            previousTestSetId = testTestSetId;
                        }
                    }
                }
                if (!newTestSetId.HasValue && previousTestSetId != -1)
                {
                    newTestSetId = previousTestSetId;
                }

                //Next we need to delete the current test-set
                testSetManager.MarkAsDeleted(userId, projectId, artifactId);
                return newTestSetId;
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
        /// Creates a new testSet and returns it to the form
        /// </summary>
        /// <param name="artifactId">the id of the existing test set we were on</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The id of the new testSet</returns>
        public override int? Form_New(int projectId, int artifactId)
        {
            const string METHOD_NAME = "Form_New";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to create the item
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Get the existing artifact and get its folder to insert in
                TestSetManager testSetManager = new TestSetManager();
                TestSet testSet;
                int? folderId = null;
                try
                {
                    testSet = testSetManager.RetrieveById2(projectId, artifactId);
                    folderId = testSet.TestSetFolderId;
                }
                catch (ArtifactNotExistsException)
                {
                    //Ignore, leave indent level as null;
                }

                //Now we need to create the testSet and then navigate to it
                int testSetId = testSetManager.Insert(
                        userId,
                        projectId,
                        folderId,
                        null,
                        userId,
                        null,
                        TestSet.TestSetStatusEnum.NotStarted,
                        "",
                        null,
                        null,
                        TestRun.TestRunTypeEnum.Manual,
                        null,
                        null
                        );
                testSet = testSetManager.RetrieveById2(projectId, testSetId);

                //We now need to populate the appropriate default custom properties
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);
                if (testSet != null)
                {
                    //If the artifact custom property row is null, create a new one and populate the defaults
                    if (artifactCustomProperty == null)
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, false);
                        artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestSet, testSetId, customProperties);
                        artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                    }
                    else
                    {
                        artifactCustomProperty.StartTracking();
                    }

                    //Save the custom properties
                    customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Clones the current test set and returns the ID of the item to redirect to
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Now we need to copy the test set into the current folder and then navigate to it
                TestSetManager testSetManager = new TestSetManager();
                TestSet testSet = testSetManager.RetrieveById2(projectId, artifactId);
                int newTestSetId = testSetManager.TestSet_Copy(userId, projectId, testSet.TestSetId, testSet.TestSetFolderId);

                return newTestSetId;
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

        /// <summary>Saves a single test set data item</summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="dataItem">The test set to save</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Get the test set id
            int testSetId = dataItem.PrimaryKey;

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business classes
                TestSetManager testSetManager = new TestSetManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Load the custom property definitions (once, not per artifact)
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, false);

                //This service only supports updates, so we should get a test set id that is valid

                //Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
                TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId);

                //Make sure the user is authorized for this item if they only have limited permissions
                int? ownerId = -1;
                if (testSet.OwnerId.HasValue)
                {
                    ownerId = testSet.OwnerId.Value;
                }
                if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && testSet.CreatorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Create a new artifact custom property row if one doesn't already exist
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, false, customProperties);
                if (artifactCustomProperty == null)
                {
                    artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestSet, testSetId, customProperties);
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
                        testSet.ConcurrencyDate = concurrencyDateTimeValue;
                        testSet.AcceptChanges();
                    }
                }

                //Now we can start tracking any changes
                testSet.StartTracking();

                //Update the field values, tracking changes
                List<string> fieldsToIgnore = new List<string>();
                fieldsToIgnore.Add("NewComment");
                fieldsToIgnore.Add("Comments");
                fieldsToIgnore.Add("CreationDate");
                fieldsToIgnore.Add("LastUpdateDate");
                fieldsToIgnore.Add("ExecutionDate");

                //Need to handle any data-mapping fields (project-admin only)
                if (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)
                {
                    DataMappingManager dataMappingManager = new DataMappingManager();
                    List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.TestSet, testSetId);
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
                UpdateFields(validationMessages, dataItem, testSet, customProperties, artifactCustomProperty, projectId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, fieldsToIgnore);

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

                //Clone for use in notifications
                Artifact notificationArt = testSet.Clone();
                ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

                //Update the test set and any custom properties
                try
                {
                    testSetManager.Update(testSet, userId);
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

                //See if we have a new comment encoded in the list of fields
                string newComment = "";
                if (dataItem.Fields.ContainsKey("NewComment"))
                {
                    newComment = dataItem.Fields["NewComment"].TextValue;

                    if (!String.IsNullOrWhiteSpace(newComment))
                    {
                        new DiscussionManager().Insert(userId, testSetId, Artifact.ArtifactTypeEnum.TestSet, newComment, DateTime.UtcNow, projectId, false, false);
                    }
                }

                //Call notifications..
                try
                {
                    new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, newComment);
                }
                catch (Exception ex)
                {
                    Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for " + testSet.ArtifactToken);
                }

                //If we're asked to save and create a new test set, need to do the insert and send back the new id
                if (operation == "new")
                {
                    //Get the values from the existing test set that we want to set on the new one (not status)
                    //Set the ownerId to null if it is still unset - done here to avoid any side effects
                    if (ownerId == -1)
                    {
                        ownerId = null;
                    }
                    //Now we need to create a new test set in the list and then navigate to it
                    int newTestSetId = testSetManager.Insert(
                        userId,
                        projectId,
                        testSet.TestSetFolderId,
                        testSet.ReleaseId,
                        userId,
                        ownerId,
                        TestSet.TestSetStatusEnum.NotStarted,
                        "",
                        null,
                        null,
                        TestRun.TestRunTypeEnum.Manual,
                        null,
                        null
                        );

                    //We need to populate any custom property default values
                    artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestSet, newTestSetId, customProperties);
                    artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                    customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

                    //We need to encode the new artifact id as a 'pseudo' validation message
                    ValidationMessage newMsg = new ValidationMessage();
                    newMsg.FieldName = "$NewArtifactId";
                    newMsg.Message = newTestSetId.ToString();
                    AddUniqueMessage(validationMessages, newMsg);
                }

                //Return back any messages. For success it should only contain a new artifact ID if we're inserting
                return validationMessages;
            }
            catch (ArtifactNotExistsException)
            {
                //Let the user know that the ticket no inter exists
                return CreateSimpleValidationMessage(String.Format(Resources.Messages.TestSetService_TestSetNotFound, testSetId));
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

        #region ITestSetService Methods

        /// <summary>
        /// Counts the number of test sets
        /// </summary>
        /// <param name="projectId">The project id</param>
        /// <param name="artifact">The artifact we want history for</param>
        /// <returns>The count</returns>
        public int TestSet_Count(int projectId, ArtifactReference artifact)
        {
            const string METHOD_NAME = "TestSet_Count";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view test sets
            //Limited OK because we need to display the 'has data' in tabs
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Depending on the artifact that these test sets are for (test case, test configuration)
                //we need to set the grid properties accordingly and also indicate if we have any data
                int testSetCount = 0;
                TestSetManager testSetManager = new TestSetManager();
                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestCase)
                {
                    //Get the count of test sets (filtered)
                    testSetCount = new TestSetManager().CountByTestCase(projectId, artifact.ArtifactId);
                }

                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestConfigurationSet)
                {
                    //Get the count of test sets (filtered)
                    testSetCount = new TestSetManager().CountByTestConfigurationSet(projectId, artifact.ArtifactId);
                }

                return testSetCount;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Returns the data for the test set execution status graph
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="releaseId">The id of the release (optional)</param>
        /// <returns></returns>
        public List<GraphEntry> TestSet_RetrieveExecutionSummary(int projectId, int? releaseId)
        {
            const string METHOD_NAME = "TestSet_RetrieveExecutionSummary";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view test sets
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Now get the execution status list
                List<GraphEntry> graphEntries = new List<GraphEntry>();
                List<TestSet_ExecutionStatusSummary> executionStatiSummary = new TestSetManager().RetrieveExecutionStatusSummary(projectId, releaseId);
                if (executionStatiSummary != null)
                {
                    foreach (TestSet_ExecutionStatusSummary entry in executionStatiSummary)
                    {
                        if (entry.StatusCount.HasValue)
                        {
                            GraphEntry graphEntry = new GraphEntry();
                            graphEntry.Name = entry.ExecutionStatusId.ToString();
                            graphEntry.Caption = entry.ExecutionStatusName;
                            graphEntry.Count = entry.StatusCount.Value;
                            graphEntry.Color = TestCaseManager.GetExecutionStatusColor(entry.ExecutionStatusId);
                            graphEntries.Add(graphEntry);
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return graphEntries;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns the count of overdue, planned, and unscheduled test sets
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="releaseId">The id of the release (optional)</param>
        /// <returns></returns>
        public List<GraphEntry> TestSet_RetrieveScheduleSummary(int projectId, int? releaseId)
        {
            const string METHOD_NAME = "TestSet_RetrieveScheduleSummary";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view test sets
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Now get the execution status list
                List<GraphEntry> graphEntries = new List<GraphEntry>();
                Dictionary<string, int> scheduleSummary = new TestSetManager().RetrieveScheduleSummary(projectId, releaseId);
                if (scheduleSummary != null)
                {
                    foreach (KeyValuePair<string, int> entry in scheduleSummary)
                    {
                        GraphEntry graphEntry = new GraphEntry();
                        graphEntry.Name = entry.Key;
                        graphEntry.Caption = entry.Key;
                        graphEntry.Count = entry.Value;
                        graphEntries.Add(graphEntry);
                    }

                    //Add the colors
                    graphEntries[0].Color = "f47457";   //Late
                    graphEntries[1].Color = "e0e0e0";   //Future
                    graphEntries[2].Color = "d0d0d0";   //Unscheduled
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return graphEntries;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Deletes a test set parameter value
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testSetId">The id of the test set</param>
        /// <param name="testCaseParameterId">The id of the test case parameter</param>
        /// <param name="newValue">The new value for the parameter</param>
        public void DeleteParameterValue(int projectId, int testSetId, int testCaseParameterId)
        {
            const string METHOD_NAME = "DeleteParameterValue";

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
                //First get the test set itself
                TestSetManager testSetManager = new TestSetManager();
                TestSetView testSet = testSetManager.RetrieveById(projectId, testSetId);

                //Make sure we're authorized if limited modify user
                if (authorizationState == Project.AuthorizationState.Limited && testSet.CreatorId != userId && testSet.OwnerId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Now delete the parameter value
                testSetManager.DeleteTestSetParameter(testSetId, testCaseParameterId);
            }
            catch (ArtifactNotExistsException)
            {
                //Do nothing
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Adds a new test set parameter value
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testSetId">The id of the test set</param>
        /// <param name="testCaseParameterId">The id of the test case parameter</param>
        /// <param name="newValue">The new value for the parameter</param>
        public void AddParameterValue(int projectId, int testSetId, int testCaseParameterId, string newValue)
        {
            const string METHOD_NAME = "DeleteParameterValue";

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
                //First get the test set itself
                TestSetManager testSetManager = new TestSetManager();
                TestSetView testSet = testSetManager.RetrieveById(projectId, testSetId);

                //Make sure we're authorized if limited modify user
                if (authorizationState == Project.AuthorizationState.Limited && testSet.CreatorId != userId && testSet.OwnerId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Now update the parameter value
                testSetManager.AddTestSetParameter(testSetId, testCaseParameterId, newValue);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates a test set parameter value
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testSetId">The id of the test set</param>
        /// <param name="testCaseParameterId">The id of the test case parameter</param>
        /// <param name="newValue">The new value for the parameter</param>
        public void UpdateParameterValue(int projectId, int testSetId, int testCaseParameterId, string newValue)
        {
            const string METHOD_NAME = "DeleteParameterValue";

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
                //First get the test set itself
                TestSetManager testSetManager = new TestSetManager();
                TestSetView testSet = testSetManager.RetrieveById(projectId, testSetId);

                //Make sure we're authorized if limited modify user
                if (authorizationState == Project.AuthorizationState.Limited && testSet.CreatorId != userId && testSet.OwnerId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Now update the parameter value
                testSetManager.UpdateTestSetParameter(testSetId, testCaseParameterId, newValue);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of parameters that have their values set at the test set level
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testSetId">The id of the test set</param>
        /// <returns>List of parameter values currently set by the test set itself (not the individual test cases)</returns>
        public List<DataItem> RetrieveParameterValues(int projectId, int testSetId)
        {
            const string METHOD_NAME = "RetrieveParameterValues";

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
                //Create the array of data items to store the parameter values
                List<DataItem> dataItems = new List<DataItem>();

                //Get the list of parameters and values specified currently at the test set level
                TestSetManager testSetManager = new TestSetManager();
                List<TestSetParameter> testSetParameterValues = testSetManager.RetrieveParameterValues(testSetId);

                //Populate the data items list
                foreach (TestSetParameter testSetParameterValue in testSetParameterValues)
                {
                    //The data item itself
                    DataItem dataItem = new DataItem();
                    dataItem.PrimaryKey = testSetParameterValue.TestCaseParameterId;
                    dataItems.Add(dataItem);

                    //The Name field
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = testSetParameterValue.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The Value specified by the test set
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Value";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItemField.TextValue = testSetParameterValue.Value;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The Default specified by the test case (if any)
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "DefaultValue";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    if (testSetParameterValue.TestCaseParameter != null && !String.IsNullOrEmpty(testSetParameterValue.TestCaseParameter.DefaultValue))
                    {
                        dataItemField.TextValue = testSetParameterValue.TestCaseParameter.DefaultValue;
                    }
                }

                return dataItems;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
        
        /// <summary>
        /// Retrieves a list of parameters available for the test set
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testSetId">The id of the test set</param>
        /// <returns>List of unique parameters for all of the test cases in the set</returns>
        /// <remarks>Excludes any that are already set on the test set</remarks>
        public JsonDictionaryOfStrings RetrieveParameters(int projectId, int testSetId)
        {
            const string METHOD_NAME = "RetrieveParameters";

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

                //Get the list of parameters that are either direct or inherited for the test case
                TestCaseManager testCaseManager = new TestCaseManager();
                TestSetManager testSetManager = new TestSetManager();

                //First get the list of test cases in the test set
                List<TestSetTestCase> testSetTestCases = testSetManager.RetrieveTestCases2(testSetId);

                //Get the list already set
                List<TestSetParameter> existingValues = testSetManager.RetrieveParameterValues(testSetId);

                //Loop through each test case and the list of available parameters
                foreach (TestSetTestCase testSetTestCase in testSetTestCases)
                {
                    List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(testSetTestCase.TestCaseId, true, true);

                    //Populate the data items list
                    foreach (TestCaseParameter testCaseParameter in testCaseParameters)
                    {
                        if (!availableParameters.ContainsKey(testCaseParameter.TestCaseParameterId.ToString()) && !existingValues.Any(p => p.TestCaseParameterId == testCaseParameter.TestCaseParameterId))
                        {
                            availableParameters.Add(testCaseParameter.TestCaseParameterId.ToString(), testCaseParameter.Name);
                        }
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

        #endregion

        #region ISortedList Methods

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
        /// Returns the current hierarchy configuration for the current page
        /// </summary>
        /// <param name="userId">The user we're viewing the artifacts as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>a dictionary where key=artifactid, value=indentlevel</returns>
        public JsonDictionaryOfStrings RetrieveHierarchy(int projectId, JsonDictionaryOfStrings standardFilters)
        {
            //Auth is handled by the Retrieve method

            //Get the full list of items for the current page
            List<SortedDataItem> dataItems = this.SortedList_Retrieve(projectId, standardFilters, null).Items;

            //Populate a dictionary with just the artifact ids and indent levels
            //as this will consume less bandwidth when retrieved by the client
            JsonDictionaryOfStrings hierarchyLevels = new JsonDictionaryOfStrings();
            for (int i = 1; i < dataItems.Count; i++)
            {
                //hierarchyLevels.Add(dataItems[i].PrimaryKey.ToString(), dataItems[i].Indent);
            }
            return hierarchyLevels;
        }

        /// <summary>
        /// Returns a list of test sets in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the test sets as</param>
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

                //Instantiate the test set and custom property business objects
                TestSetManager testSetManager = new TestSetManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the array of data items (including the first filter item)
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;

                //Now get the list of populated filters and the current sort
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST);
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //The following is used in the special case of filtering by test case id
                int? filterTestCaseId = null;

                //See if we're filtering by test configurations (special case)
                bool filteringByTestConfigurations = false;

                //Add any standard filters
                int? folderId = null;
                if (standardFilters != null && standardFilters.Count > 0)
                {
                    Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
                    foreach (KeyValuePair<string, object> filter in deserializedFilters)
                    {
                        //Handle the special test case
                        if (filter.Key == "TestCaseId" && filter.Value is Int32)
                        {
                            filterTestCaseId = (int)filter.Value;
                        }
                        else if (filter.Key == GlobalFunctions.SPECIAL_FILTER_FOLDER_ID && filter.Value is Int32)
                        {
                            int intValue = (int)(filter.Value);
                            if (intValue > 0 && testSetManager.TestSetFolder_Exists(projectId, intValue))
                            {
                                folderId = intValue;
                            }
                        }
                        else
                        {
                            filterList[filter.Key] = filter.Value;
                        }

                        if (filter.Key == "TestConfigurationSetId")
                        {
                            filteringByTestConfigurations = true;
                        }
                    }
                }
                else
                {
                    //See if we have a folder to filter on, not applied if we have a standard filter
                    //because those screens don't display the folders on the left-hand side

                    //-1 = no filter
                    //0 = root folder
                    int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                    if (selectedNodeId >= 1)
                    {
                        if (testSetManager.TestSetFolder_Exists(projectId, selectedNodeId))
                        {
                            //Filter by specific Folder
                            folderId = selectedNodeId;
                        }
                        else
                        {
                            //Set to the Root Folder (i.e. no folder items only) and update the projectsetting
                            SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                        }
                    }
                }

                //Create the filter item first - we can clone it later
                SortedDataItem filterItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
                dataItems.Add(filterItem);
                sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet);

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

                //Now get the pagination information and add to the filter item
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
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

                //See if we are viewing execution data for one release or all releases
                int releaseId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

                //Make sure the release actually exists
                if (releaseId > 0)
                {
                    try
                    {
                        Release release = new ReleaseManager().RetrieveById3(projectId, releaseId);
                        if (release == null)
                        {
                            SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                    }
                }

                //Get the number of test sets in the project or release
                int artifactCount = 0;
                if (filterTestCaseId.HasValue)
                {
                    artifactCount = testSetManager.CountByTestCase(projectId, filterTestCaseId.Value, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }
                else
                {
                    if (releaseId > 0 && !filteringByTestConfigurations)
                    {
                        artifactCount = testSetManager.CountByRelease(projectId, releaseId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
                    }
                    else
                    {
                        artifactCount = testSetManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
                    }
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
                sortedData.CurrPage = currentPage;
                sortedData.PageCount = pageCount;


                //**** Now we need to actually populate the rows of data to be returned ****

                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }
                sortedData.StartRow = startRow;

                //Now get the list of custom property options and lookup values for this artifact type / project
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, true, false, true);

                //The queries depend on whether we are filtering by release, test case, project or not
                if (filterTestCaseId.HasValue)
                {
                    //When filtering by test case, we do not show the folders

                    //Get the test set list for the project or release
                    List<TestSetView> testSets = testSetManager.RetrieveByTestCaseId(projectId, filterTestCaseId.Value, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                    //Get the visible count from the data rows returned
                    sortedData.VisibleCount = testSets.Count;

                    //Iterate through all the test sets and populate the dataitems
                    foreach (TestSetView testSetView in testSets)
                    {
                        //We clone the template item as the basis of all the new items
                        SortedDataItem dataItem = filterItem.Clone();

                        //Now populate with the data
                        PopulateRow(dataItem, testSetView, customProperties, false, null);
                        dataItems.Add(dataItem);
                    }

                    //For test sets we need to also provide the visible count and total count of
                    //actual test sets (not folders) with no filters for all folders
                    sortedData.TotalCount = testSetManager.CountByTestCase(projectId, filterTestCaseId.Value, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }
                else
                {
                    if (releaseId > 0 && !filteringByTestConfigurations)
                    {
                        //Get the test set list for the project or release
                        List<TestSetReleaseView> testSets = testSetManager.RetrieveByReleaseId(projectId, releaseId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);

                        //We also need to get and sub-folders in this folder
                        List<TestSetFolderReleaseView> testFolders = testSetManager.TestSetFolder_GetByParentIdForRelease(projectId, folderId, releaseId, sortProperty, sortAscending, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                        //Get the visible count from the data rows returned
                        sortedData.VisibleCount = testSets.Count;

                        //Iterate through all the test folders and populate the data items
                        foreach (TestSetFolderReleaseView testFolder in testFolders)
                        {
                            //We clone the template item as the basis of all the new items
                            SortedDataItem dataItem = filterItem.Clone();

                            //Now populate with the data
                            PopulateRow(dataItem, testFolder.ConvertTo<TestSetFolderReleaseView, TestSetFolder>());
                            dataItems.Add(dataItem);
                        }

                        //Iterate through all the test sets and populate the dataitems
                        foreach (TestSetReleaseView testSetView in testSets)
                        {
                            //We clone the template item as the basis of all the new items
                            SortedDataItem dataItem = filterItem.Clone();

                            //Now populate with the data
                            PopulateRow(dataItem, testSetView.ConvertTo<TestSetReleaseView, TestSetView>(), customProperties, false, null);
                            dataItems.Add(dataItem);
                        }

                        //For test sets we need to also provide the visible count and total count of
                        //actual test sets (not folders) with no filters for all folders
                        sortedData.TotalCount = testSetManager.CountByRelease(projectId, releaseId, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), null, false, true);
                    }
                    else
                    {
                        //Get the test set list for the project or release
                        if (filteringByTestConfigurations)
                        {
                            //Always show all folders
                            folderId = null;
                        }
                        List<TestSetView> testSets = testSetManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);

                        //We also need to get any sub-folders in this folder
                        List<TestSetFolder> testFolders = testSetManager.TestSetFolder_GetByParentId(projectId, folderId, sortProperty, sortAscending, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                        //Get the visible count from the data rows returned
                        sortedData.VisibleCount = testSets.Count;

                        //Iterate through all the test folders and populate the data items
                        //unless we're filtering by test configurations
                        if (!filteringByTestConfigurations)
                        {
                            foreach (TestSetFolder testFolder in testFolders)
                            {
                                //We clone the template item as the basis of all the new items
                                SortedDataItem dataItem = filterItem.Clone();

                                //Now populate with the data
                                PopulateRow(dataItem, testFolder);
                                dataItems.Add(dataItem);
                            }
                        }

                        //Iterate through all the test sets and populate the dataitems
                        foreach (TestSetView testSetView in testSets)
                        {
                            //We clone the template item as the basis of all the new items
                            SortedDataItem dataItem = filterItem.Clone();

                            //Now populate with the data
                            PopulateRow(dataItem, testSetView, customProperties, false, null);
                            dataItems.Add(dataItem);
                        }

                        //For test sets we need to also provide the visible count and total count of
                        //actual test sets (not folders) with no filters for all folders
                        sortedData.TotalCount = testSetManager.Count(projectId, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), null, false, true);
                    }
                }

                //Include the pagination info
                sortedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

                //Display the sort information
                sortedData.SortProperty = sortProperty;
                sortedData.SortAscending = sortAscending;

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
        /// Returns the latest information on a single test set in the system
        /// </summary>
        /// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <returns>A single dataitem object</returns>
        public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
        {
            const string METHOD_NAME = "SortedList_Refresh";

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

                //Instantiate the test set and custom property business objects
                TestSetManager testSetManager = new TestSetManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

                //Get the test set dataset record for the specific test set id
                TestSetView testSet = testSetManager.RetrieveById(projectId, artifactId);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && testSet.OwnerId != userId && testSet.CreatorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);

                //Finally populate the dataitem from the dataset
                if (testSet != null)
                {
                    //See if we already have an artifact custom property row
                    if (artifactCustomProperty != null)
                    {
                        PopulateRow(dataItem, testSet, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty);
                    }
                    else
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, true, false);
                        PopulateRow(dataItem, testSet, customProperties, true, null);
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
        /// Handles custom operations that are artifact/page-specific (buttons, drop-downs, etc.)
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="operation">The name of the operation</param>
        /// <param name="value">The parameter value being passed to the operation</param>
        /// <returns>Any error messages</returns>
        public override string CustomOperation(int projectId, string operation, string value)
        {
            const string METHOD_NAME = "CustomOperation";

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
                //See which operation we have and handle accordingly
                if (operation == "SelectRelease")
                {
                    //The value contains the id of the release we want to select
                    //We need to capture the release and put it in the project setting
                    if (value == "")
                    {
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                    }
                    else
                    {
                        int releaseId = Int32.Parse(value);
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);
                    }
                }
                return "";
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
            return base.UpdateSort(userId, projectId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS);
        }

        /// <summary>
        /// Updates records of data in the system
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="dataItems">The updated data records</param>
        /// <returns>Validation messages</returns>
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
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Iterate through each data item and make sure all validation rules have been followed

                //Iterate through each data item and make the updates
                TestSetManager testSetManager = new TestSetManager();
                //Load the custom property definitions (once, not per artifact)
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, false);

                foreach (SortedDataItem dataItem in dataItems)
                {
                    //Get the test set id
                    int testSetId = dataItem.PrimaryKey;

                    //Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
                    TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId);
                    ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, false, customProperties);

                    //Create a new artifact custom property row if one doesn't already exist
                    if (artifactCustomProperty == null)
                    {
                        artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestSet, testSetId, customProperties);
                    }
                    else
                    {
                        artifactCustomProperty.StartTracking();
                    }

                    if (testSet != null)
                    {
                        //Need to set the original date of this record to match the concurrency date
                        if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                        {
                            DateTime concurrencyDateTimeValue;
                            if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                            {
                                testSet.ConcurrencyDate = concurrencyDateTimeValue;
                                testSet.AcceptChanges();
                            }
                        }

                        //Update the field values
                        List<string> fieldsToIgnore = new List<string>();
                        fieldsToIgnore.Add("CreationDate");
                        fieldsToIgnore.Add("ConcurrencyDate");   //Breaks concurrency otherwise
                        UpdateFields(validationMessages, dataItem, testSet, customProperties, artifactCustomProperty, projectId, testSetId, 0, fieldsToIgnore);

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
                            //Clone for use in notifications
                            Artifact notificationArt = testSet.Clone();
                            ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

                            //Persist to database
                            try
                            {
                                testSetManager.Update(testSet, userId);
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
                                Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for " + testSet.ArtifactToken);
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
        /// Inserts a new test set / folder into the system
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="artifact">The type of artifact we're inserting (TestSet, Folder)</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>The id of the new test set / folder</returns>
        public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            const string METHOD_NAME = "SortedList_Insert";

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
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                TestSetManager testSetManager = new TestSetManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();


                //Get the current folder that we need to insert into
                //First, check if a folder was passed in via the filters
                int? passedInFolderId = null;
                if (standardFilters != null && standardFilters.Count > 0)
                {
                    Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
                    foreach (KeyValuePair<string, object> filter in deserializedFilters)
                    {
                        //See if we have the folder id passed through as a filter
                        if (filter.Key == GlobalFunctions.SPECIAL_FILTER_FOLDER_ID && filter.Value is Int32)
                        {
                            passedInFolderId = (int)(filter.Value);
                        }
                    }
                }

                //See if we have a folder to filter by
                //-1 = root folder
                int? folderId = null;
                if (passedInFolderId.HasValue && passedInFolderId.Value > 0)
                {
                    // set the folder id and update the selected node (set to root folder if the folder does not exist)
                    int intValue = (int)(passedInFolderId.Value);
                    folderId = testSetManager.TestSetFolder_Exists(projectId, intValue) ? intValue : ManagerBase.NoneFilterValue;
                    this.TreeView_SetSelectedNode(projectId, folderId.ToString());
                }
                else
                {
                    int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                    if (selectedNodeId > 0)
                    {
                        if (testSetManager.TestSetFolder_Exists(projectId, selectedNodeId))
                        {
                            //Filter by specific Folder
                            folderId = selectedNodeId;
                        }
                        else
                        {
                            //Set to the Root Folder (i.e. no folder tasks only) and update the projectsetting
                            folderId = ManagerBase.NoneFilterValue;
                            SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                        }
                    }
                }

                //Simply insert the new item into the test set list
                int testSetId = testSetManager.Insert(
                    userId,
                    projectId,
                    folderId,
                    null,
                    userId,
                    null,
                    TestSet.TestSetStatusEnum.NotStarted,
                    "",
                    null,
                    null,
                    TestRun.TestRunTypeEnum.Manual,
                    null,
                    null);

                //We now need to populate the appropriate default custom properties
                TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId);
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);
                if (testSet != null)
                {
                    //If the artifact custom property row is null, create a new one and populate the defaults
                    if (artifactCustomProperty == null)
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, false);
                        artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestSet, testSetId, customProperties);
                        artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                    }
                    else
                    {
                        artifactCustomProperty.StartTracking();
                    }

                    //If we have filters currently applied to the view, then we need to set this new test set/folder to the same value
                    //(if possible) so that it will show up in the list
                    ProjectSettingsCollection filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST);
                    if (filterList.Count > 0)
                    {
                        testSet.StartTracking();
                        //We need to tell it to ignore any filtering by the ID, creation date since we cannot set that on a new item
                        List<string> fieldsToIgnore = new List<string>() { "TestSetId", "CreationDate" };
                        UpdateToMatchFilters(projectId, filterList, testSetId, testSet, artifactCustomProperty, fieldsToIgnore);
                        testSetManager.Update(testSet, userId);
                    }

                    //Save the custom properties
                    customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
                }

                return testSetId;
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
        /// <param name="testSetId">The id of the test set to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        public string RetrieveNameDesc(int? projectId, int testSetId, int? displayTypeId)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the test set business object
                TestSetManager testSetManager = new TestSetManager();

                //Now retrieve the specific test set/folder - handle quietly if it doesn't exist
                try
                {
                    //See if we have a test set or folder
                    string tooltip = "";
                    if (testSetId < 0)
                    {
                        //Test folder IDs are negative
                        int testFolderId = -testSetId;

                        TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testFolderId);

                        //See if we have any parent folders
                        List<TestSetFolderHierarchyView> parentFolders = testSetManager.TestSetFolder_GetParents(testSetFolder.ProjectId, testSetFolder.TestSetFolderId, false);
                        foreach (TestSetFolderHierarchyView parentFolder in parentFolders)
                        {
                            tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
                        }

                        tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testSetFolder.Name) + "</u>";
                        if (!String.IsNullOrEmpty(testSetFolder.Description))
                        {
                            tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testSetFolder.Description);
                        }
                    }
                    else
                    {
                        //First we need to get the test set itself
                        TestSetView testSetView = testSetManager.RetrieveById(null, testSetId);

                        //Next we need to get the list of successive parent folders
                        if (testSetView.TestSetFolderId.HasValue)
                        {
                            List<TestSetFolderHierarchyView> parentFolders = testSetManager.TestSetFolder_GetParents(testSetView.ProjectId, testSetView.TestSetFolderId.Value, true);
                            foreach (TestSetFolderHierarchyView parentFolder in parentFolders)
                            {
                                tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
                            }
                        }

                        //Now we need to get the test set itself
                        tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testSetView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TEST_SET, testSetId, true) + "</u>";
                        if (!String.IsNullOrEmpty(testSetView.Description))
                        {
                            tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testSetView.Description);
                        }

                        //See if we have any comments to append
                        IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(testSetId, Artifact.ArtifactTypeEnum.TestSet, false);
                        if (comments.Count() > 0)
                        {
                            IDiscussion lastComment = comments.Last();
                            tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
                                GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
                                GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
								Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
                                );
                        }
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the test set, but it has already been deleted on the server
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for test set");
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
        /// Updates the filters stored in the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="filters">The array of filters (name,value)</param>
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

            bool isInitialFilter = false;
            string result = base.UpdateFilters(userId, projectId, filters, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.TestSet, out isInitialFilter);
            return result;
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

            return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.TestSet, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, isShared, existingSavedFilterId, includeColumns);
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Delegate to the generic implementation
            return base.RetrieveFilters(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, includeShared);
        }

        /// <summary>
        /// Returns a list of test set folders for the current project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>Just the fields needed for a lookup / dropdown list</returns>
        public JsonDictionaryOfStrings RetrieveTestSetFolders(int projectId)
        {
            const string METHOD_NAME = "RetrieveTestSetFolders";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited is OK)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                TestSetManager testSetManager = new TestSetManager();
                List<TestSetFolderHierarchyView> testSetFolders = testSetManager.TestSetFolder_GetList(projectId);
                JsonDictionaryOfStrings lookupValues = ConvertLookupValues(testSetFolders.OfType<Entity>().ToList(), "TestSetFolderId", "Name", "IndentLevel");

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return lookupValues;
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
                    customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestSet, fieldName);
                }
                else
                {
                    //Toggle the status of the appropriate field name
                    ArtifactManager artifactManager = new ArtifactManager();
                    artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestSet, fieldName);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

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
                artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestSet, fieldName, width);
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
        /// Changes the order of columns in the test set list
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
                artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestSet, fieldName, newPosition);
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
        /// Exports a series of test sets/folders (and their children) from one project to another
        /// </summary>
        /// <param name="items">The list of test sets/folders being exported</param>
        /// <param name="destProjectId">The project we're exporting to</param>
        public void SortedList_Export(int destProjectId, List<string> items)
        {
            throw new NotImplementedException("This operation is not currently implemented");
        }

        /// <summary>
        /// Copies a set of test sets/folders
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items to copy</param>
        public void SortedList_Copy(int projectId, List<string> items)
        {
            const string METHOD_NAME = "SortedList_Copy";

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
                TestSetManager testSetManager = new TestSetManager();

                //Get the current folder
                //-1 = root folder
                int? currentFolderId = null;
                int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (selectedNodeId != -1)
                {
                    currentFolderId = selectedNodeId;
                }

                //Get the list of folders, not needed if moving to root
                List<TestSetFolderHierarchyView> testSetFolders = null;
                if (currentFolderId.HasValue)
                {
                    testSetFolders = testSetManager.TestSetFolder_GetList(projectId);
                }

                //Iterate through all the items to be copied
                foreach (string item in items)
                {
                    int artifactId;
                    if (Int32.TryParse(item, out artifactId))
                    {
                        //See if we have a folder or test set
                        if (artifactId > 0)
                        {
                            //Test Case
                            int testSetId = artifactId;

                            //Copy the single test set
                            testSetManager.TestSet_Copy(userId, projectId, testSetId, currentFolderId);
                        }
                        if (artifactId < 0)
                        {
                            //Test Folder
                            int testFolderId = -artifactId;

                            //Check to make sure we're not making it's parent either this folder
                            //or one of its children
                            if (currentFolderId.HasValue && testSetFolders != null && testSetFolders.Count > 0)
                            {
                                string folderIndent = testSetFolders.FirstOrDefault(f => f.TestSetFolderId == testFolderId).IndentLevel;
                                string newParentIndent = testSetFolders.FirstOrDefault(f => f.TestSetFolderId == currentFolderId.Value).IndentLevel;

                                if (newParentIndent.Length >= folderIndent.Length && newParentIndent.Substring(0, folderIndent.Length) == folderIndent)
                                {
                                    //Throw a meaningful exception
                                    throw new DataValidationExceptionEx(CreateSimpleValidationMessage(Resources.Messages.TestSetsService_CannotMoveFolderUnderItself));
                                }
                            }

                            //Copy the test set folder
                            testSetManager.TestSetFolder_Copy(userId, projectId, testFolderId, currentFolderId);
                        }
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
        /// Moves a test set/folder in the system
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items to move</param>
        public override void SortedList_Move(int projectId, List<string> items)
        {
            const string METHOD_NAME = "SortedList_Move";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                TestSetManager testSetManager = new TestSetManager();

                //Get the current folder
                //-1 = root folder
                int? currentFolderId = null;
                int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (selectedNodeId != -1)
                {
                    currentFolderId = selectedNodeId;
                }

                //Get the list of folders, not needed if moving to root
                List<TestSetFolderHierarchyView> testSetFolders = null;
                if (currentFolderId.HasValue)
                {
                    testSetFolders = testSetManager.TestSetFolder_GetList(projectId);
                }

                //Iterate through all the items to be moved
                foreach (string item in items)
                {
                    int artifactId;
                    if (Int32.TryParse(item, out artifactId))
                    {
                        //See if we have a folder or test set
                        if (artifactId > 0)
                        {
                            //Test Case
                            int testSetId = artifactId;

                            //Move the single test set
                            testSetManager.TestSet_UpdateFolder(testSetId, currentFolderId);
                        }
                        if (artifactId < 0)
                        {
                            //Test Folder
                            int testFolderId = -artifactId;
                            TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testFolderId);
                            if (testSetFolder != null)
                            {
                                //Check to make sure we're not making it's parent either this folder
                                //or one of its children
                                if (currentFolderId.HasValue && testSetFolders != null && testSetFolders.Count > 0)
                                {
                                    string folderIndent = testSetFolders.FirstOrDefault(f => f.TestSetFolderId == testFolderId).IndentLevel;
                                    string newParentIndent = testSetFolders.FirstOrDefault(f => f.TestSetFolderId == currentFolderId.Value).IndentLevel;

                                    if (newParentIndent.Length >= folderIndent.Length && newParentIndent.Substring(0, folderIndent.Length) == folderIndent)
                                    {
                                        //Throw a meaningful exception
                                        throw new DataValidationExceptionEx(CreateSimpleValidationMessage(Resources.Messages.TestSetsService_CannotMoveFolderUnderItself));
                                    }
                                }

                                //Move the test folder, need to make sure we don't create an infinite loop
                                testSetFolder.StartTracking();
                                testSetFolder.ParentTestSetFolderId = currentFolderId;
                                testSetManager.TestSetFolder_Update(testSetFolder);
                            }
                        }
                    }
                }

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
        /// Allows sorted lists with folders to focus on a specific item and open its containing folder
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="artifactId">Id of a test set (or negative for a folder)</param>
        /// <returns>The id of the folder (if any)</returns>
        public override int? SortedList_FocusOn(int projectId, int artifactId, bool clearFilters)
        {
            const string METHOD_NAME = "SortedList_FocusOn";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view test sets
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //See if we have a folder or test set
                TestSetManager testSetManager = new TestSetManager();
                if (artifactId > 0)
                {
                    int testSetId = artifactId;

                    //Retrieve this test set
                    TestSetView testSet = testSetManager.RetrieveById(projectId, testSetId);
                    if (testSet != null)
                    {
                        //Get the folder
                        int folderId = (testSet.TestSetFolderId.HasValue) ? testSet.TestSetFolderId.Value : -1;

                        //Unset the current filters and then set the current folder to this one
                        bool isInitialFilter = false;
                        string result = base.UpdateFilters(userId, projectId, null, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.TestSet, out isInitialFilter);
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, folderId);
                        return folderId;
                    }
                }
                if (artifactId < 0)
                {
                    int testFolderId = -artifactId;

                    //Retrieve this test folder
                    TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testFolderId);
                    if (testSetFolder != null)
                    {
                        //Unset the current filters and then set the current folder to this one
                        if (clearFilters)
                        {
                            bool isInitialFilter = false;
                            string result = base.UpdateFilters(userId, projectId, null, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.TestSet, out isInitialFilter);
                        }
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, testFolderId);
                        return testFolderId;
                    }
                }
                return null;
            }
            catch (ArtifactNotExistsException)
            {
                //Ignore, do not log
                return null;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Deletes a set of test sets
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items to delete</param>
        public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            const string METHOD_NAME = "SortedList_Delete";

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
                TestSetManager testSetManager = new TestSetManager();
                foreach (string item in items)
                {
                    int artifactId;
                    if (Int32.TryParse(item, out artifactId))
                    {
                        //See if we have a folder or test set
                        if (artifactId > 0)
                        {
                            //Test Case
                            int testSetId = artifactId;

                            //Delete the test set
                            try
                            {
                                testSetManager.MarkAsDeleted(userId, projectId, testSetId);
                            }
                            catch (ArtifactNotExistsException)
                            {
                                //Ignore any errors due to deleting a folder and some of its children at the same time
                            }
                        }
                        if (artifactId < 0)
                        {
                            //Test Folder
                            int testFolderId = -artifactId;

                            //Delete the folder
                            try
                            {
                                testSetManager.TestSetFolder_Delete(projectId, testFolderId, userId);
                            }
                            catch (ArtifactNotExistsException)
                            {
                                //Ignore any errors due to deleting a folder and some of its children at the same time
                            }
                        }
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

        #region Internal Methods

        /// <summary>
        /// Populates a data item from the entity
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="testSetFolder">The entity containing the data</param>
        protected void PopulateRow(SortedDataItem dataItem, TestSetFolder testSetFolder)
        {
            //Set the primary key (negative for folders)
            dataItem.PrimaryKey = -testSetFolder.TestSetFolderId;
            dataItem.Folder = true;

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                //The execution status id is not a real field on folders, so special case
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (testSetFolder.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the entity
                    PopulateFieldRow(dataItem, dataItemField, testSetFolder, null, null, false, PopulateEqualizer);
                }
            }
        }

        /// <summary>
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="filterReleaseId">Should we filter the equalizer to show only the data for the specific release/iteration</param>
        /// <param n/ame="dataItem">The dataitem to be populated</param>
        /// <param name="testSetView">The datarow containing the data</param>
        /// <param name="customProperties">The list of custom property definitions and values</param>
        /// <param name="editable">Does the data need to be in editable form?</param>
        /// <param name="artifactCustomProperty">The artifact's custom property data (if not provided as part of dataitem) - pass null if not used</param>
        protected void PopulateRow(SortedDataItem dataItem, TestSetView testSetView, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty, int? filterReleaseId = null)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = testSetView.TestSetId;
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, (DateTime)testSetView["ConcurrencyDate"]);

            //Specify if it has an attachment or not
            dataItem.Attachment = testSetView.IsAttachments;

            //Some fields are not editable for test sets
            List<string> readOnlyFields = new List<string>() { "CreationDate", "LastUpdateDate", "ActualDuration", "ExecutionDate", "EstimatedDuration" };

            //If we are displaying for a specific release, need to adjust the equalizer
            if (filterReleaseId.HasValue)
            {
                TestSetReleaseView testSetReleaseView = new TestSetManager().RetrieveByIdForRelease(testSetView.ProjectId, testSetView.TestSetId, filterReleaseId.Value);
                if (testSetReleaseView != null)
                {
                    testSetView.CountBlocked = testSetReleaseView.CountBlocked;
                    testSetView.CountCaution = testSetReleaseView.CountCaution;
                    testSetView.CountFailed = testSetReleaseView.CountFailed;
                    testSetView.CountNotApplicable = testSetReleaseView.CountNotApplicable;
                    testSetView.CountNotRun = testSetReleaseView.CountNotRun;
                    testSetView.CountPassed = testSetReleaseView.CountPassed;
                }
            }

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (testSetView.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, testSetView, customProperties, artifactCustomProperty, editable, PopulateEqualizer, null, null, readOnlyFields);

                    //Apply the conditional formatting to the Planned Date column (if displayed)
                    if (dataItemField.FieldName == "PlannedDate" && testSetView.PlannedDate.HasValue && testSetView.PlannedDate.Value < DateTime.UtcNow)
                    {
                        dataItemField.CssClass = "Warning";
                    }
                }
            }
        }

        /// <summary>
        /// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the test sets as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        /// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
        protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
        {
            //We need to dynamically add the various columns from the field list
            LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
            AddDynamicColumns(Artifact.ArtifactTypeEnum.TestSet, getLookupValues, projectId, projectTemplateId, userId, dataItem, filterList, PopulateEqualizerShape, returnJustListFields);
        }

        /// <summary>
        /// Used to populate the shape of the special compound fields used to display the information
        /// in the color-coded bar-chart 'equalizer' fields where different colors represent different values
        /// </summary>
        /// <param name="dataItemField">The field whose shape we're populating</param>
        /// <param name="fieldName">The field name we're handling</param>
        /// <param name="filterList">The list of filters</param>
        /// <param name="projectId">The project we're interested in</param>
        protected void PopulateEqualizerShape(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectId, int projectTemplateId)
        {
            //Check to see if this is a field we can handle
            if (fieldName == "ExecutionStatusId")
            {
                //Set the list of possible lookup values
                dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName))
                {
                    dataItemField.IntValue = (int)filterList[dataItemField.FieldName];
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
                TestSetManager testSetManager = new TestSetManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Standard field lookups
                if (lookupName == "TestSetStatusId")
                {
                    List<TestSetStatus> testSetStatuses = testSetManager.RetrieveStatuses();
                    lookupValues = ConvertLookupValues(testSetStatuses.OfType<Entity>().ToList(), "TestSetStatusId", "Name");
                }
                if (lookupName == "RecurrenceId")
                {
                    List<Recurrence> recurrences = testSetManager.RetrieveRecurrences();
                    lookupValues = ConvertLookupValues(recurrences.OfType<Entity>().ToList(), "RecurrenceId", "Name");
                }
                if (lookupName == "ReleaseId")
                {
                    ReleaseManager releaseManager = new ReleaseManager();
                    List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false, true);
                    lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
                }
                if (lookupName == "ExecutionStatusId")
                {
                    lookupValues = ConvertLookupValues(testSetManager.RetrieveExecutionStatusFiltersLookup());
                }
                if (lookupName == "CreatorId" || lookupName == "OwnerId")
                {
                    List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
                    lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
                }
                if (lookupName == "AutomationHostId")
                {
                    AutomationManager automationManager = new AutomationManager();
                    List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId);
                    lookupValues = ConvertLookupValues(automationHosts.OfType<DataModel.Entity>().ToList(), "AutomationHostId", "Name");
                }
                if (lookupName == "TestRunTypeId")
                {
                    TestRunManager testRunManager = new TestRunManager();
                    List<TestRunType> testRunTypes = testRunManager.RetrieveTypes();
                    lookupValues = ConvertLookupValues(testRunTypes.OfType<DataModel.Entity>().ToList(), "TestRunTypeId", "Name");
                }
                if (lookupName == "TestConfigurationSetId")
                {
                    TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
                    List<TestConfigurationSet> testConfigurationSets = testConfigurationManager.RetrieveSets(projectId);
                    lookupValues = ConvertLookupValues(testConfigurationSets.OfType<DataModel.Entity>().ToList(), "TestConfigurationSetId", "Name");
                }
                if (lookupName == "IsAutoScheduled")
                {
                    lookupValues = new JsonDictionaryOfStrings(GlobalFunctions.YesNoList());
                }

                //The custom property lookups
                int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
                if (customPropertyNumber.HasValue)
                {
                    CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, customPropertyNumber.Value, true);
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
        /// Populates the equalizer type graph for the test set execution status field
        /// </summary>
        /// <param name="dataItem">The data item</param>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="artifact">The data row</param>
        protected void PopulateEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
        {
            //See if we have a test set or folder

            if (artifact is TestSetView)
            {
                TestSetView testSet = (TestSetView)artifact;

                int passedCount = testSet.CountPassed;
                int failureCount = testSet.CountFailed;
                int cautionCount = testSet.CountCaution;
                int blockedCount = testSet.CountBlocked;
                int notRunCount = testSet.CountNotRun;
                int notApplicableCount = testSet.CountNotApplicable;

                //Calculate the percentages, handling rounding correctly
                int totalCount = passedCount + failureCount + cautionCount + blockedCount + notRunCount + notApplicableCount;
                int percentPassed = 0;
                int percentFailed = 0;
                int percentCaution = 0;
                int percentBlocked = 0;
                int percentNotRun = 0;
                int percentNotApplicable = 0;
                if (totalCount != 0)
                {
                    //Need check to handle divide by zero case
                    percentPassed = (int)Decimal.Round(((decimal)passedCount * (decimal)100) / (decimal)totalCount, 0);
                    percentFailed = (int)Decimal.Round(((decimal)failureCount * (decimal)100) / (decimal)totalCount, 0);
                    percentCaution = (int)Decimal.Round(((decimal)cautionCount * (decimal)100) / (decimal)totalCount, 0);
                    percentBlocked = (int)Decimal.Round(((decimal)blockedCount * (decimal)100) / (decimal)totalCount, 0);
                    percentNotRun = (int)Decimal.Round(((decimal)notRunCount * (decimal)100) / (decimal)totalCount, 0);
                    percentNotApplicable = (int)Decimal.Round(((decimal)notApplicableCount * (decimal)100) / (decimal)totalCount, 0);
                }

                //Populate the equalizer percentages
                dataItemField.EqualizerGreen = percentPassed;
                dataItemField.EqualizerRed = percentFailed;
                dataItemField.EqualizerYellow = percentBlocked;
                dataItemField.EqualizerOrange = percentCaution;
                dataItemField.EqualizerGray = 100 - (percentPassed + percentFailed + percentCaution + percentBlocked);
                if (dataItemField.EqualizerGray < 0)
                {
                    dataItemField.EqualizerGray = 0;
                }

                //Add a tooltip to the cell for the raw data
                dataItemField.Tooltip = "# " + Resources.Fields.Passed + "=" + passedCount.ToString() + ", # " + Resources.Fields.Failed + "=" + failureCount.ToString() + ", # " + Resources.Fields.Caution + "=" + cautionCount.ToString() + ", # " + Resources.Fields.Blocked + "=" + blockedCount.ToString() + ", # " + Resources.Fields.NotRun + "=" + notRunCount.ToString();

                //Add the total count to the folder total count
                dataItem.ChildCount = totalCount;
            }
            if (artifact is TestSetFolder)
            {
                TestSetFolder testSetFolder = (TestSetFolder)artifact;

                int passedCount = testSetFolder.CountPassed;
                int failureCount = testSetFolder.CountFailed;
                int cautionCount = testSetFolder.CountCaution;
                int blockedCount = testSetFolder.CountBlocked;
                int notRunCount = testSetFolder.CountNotRun;
                int notApplicableCount = testSetFolder.CountNotApplicable;

                //Calculate the percentages, handling rounding correctly
                int totalCount = passedCount + failureCount + cautionCount + blockedCount + notRunCount + notApplicableCount;
                int percentPassed = 0;
                int percentFailed = 0;
                int percentCaution = 0;
                int percentBlocked = 0;
                int percentNotRun = 0;
                int percentNotApplicable = 0;
                if (totalCount != 0)
                {
                    //Need check to handle divide by zero case
                    percentPassed = (int)Decimal.Round(((decimal)passedCount * (decimal)100) / (decimal)totalCount, 0);
                    percentFailed = (int)Decimal.Round(((decimal)failureCount * (decimal)100) / (decimal)totalCount, 0);
                    percentCaution = (int)Decimal.Round(((decimal)cautionCount * (decimal)100) / (decimal)totalCount, 0);
                    percentBlocked = (int)Decimal.Round(((decimal)blockedCount * (decimal)100) / (decimal)totalCount, 0);
                    percentNotRun = (int)Decimal.Round(((decimal)notRunCount * (decimal)100) / (decimal)totalCount, 0);
                    percentNotApplicable = (int)Decimal.Round(((decimal)notApplicableCount * (decimal)100) / (decimal)totalCount, 0);
                }

                //Populate the equalizer percentages
                dataItemField.EqualizerGreen = percentPassed;
                dataItemField.EqualizerRed = percentFailed;
                dataItemField.EqualizerYellow = percentBlocked;
                dataItemField.EqualizerOrange = percentCaution;
                dataItemField.EqualizerGray = 100 - (percentPassed + percentFailed + percentCaution + percentBlocked);
                if (dataItemField.EqualizerGray < 0)
                {
                    dataItemField.EqualizerGray = 0;
                }

                //Add a tooltip to the cell for the raw data
                dataItemField.Tooltip = "# " + Resources.Fields.Passed + "=" + passedCount.ToString() + ", # " + Resources.Fields.Failed + "=" + failureCount.ToString() + ", # " + Resources.Fields.Caution + "=" + cautionCount.ToString() + ", # " + Resources.Fields.Blocked + "=" + blockedCount.ToString() + ", # " + Resources.Fields.NotRun + "=" + notRunCount.ToString();

                //Add the total count to the test set total count
                dataItem.ChildCount = totalCount;
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
        /// Returns a list of test sets for display in the navigation bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="indentLevel">The indent level of the parent folder, or empty string for all items</param>
        /// <returns>List of test sets</returns>
        /// <param name="displayMode">
        /// The display mode of the navigation list:
        /// 1 = Filtered List
        /// 2 = All Items (no filters)
        /// 3 = Assigned to the Current User
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the test case business object
                TestSetManager testSetManager = new TestSetManager();

                //Create the array of data items
                List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

                //See if we have a folder to filter by
                //-1 = root folder
                int? folderId = null;
                int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (selectedNodeId != -1)
                {
                    folderId = selectedNodeId;
                }

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
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
                //Get the number of test cases in the project
                int artifactCount = testSetManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //**** Now we need to actually populate the rows of data to be returned ****

                //Get the requirements list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }
                List<TestSetView> testSetList = new List<TestSetView>(); //Default to empty list
                if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.AllItems)
                {
                    //All Items
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                    testSetList = testSetManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);

                }
                else if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Assigned)
                {
                    //Assigned to User
                    testSetList = testSetManager.RetrieveByOwnerId(userId, projectId);
                }
                else
                {
                    //Filtered List
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                    testSetList = testSetManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
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

                //Populate the test sets
                PopulateTestSets(testSetList, dataItems, "");

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
        /// Populates the list of Test Sets
        /// </summary>
        /// <param name="testSets">The TestSet list</param>
        /// <param name="dataItems">The list of nav-bar items</param>
        /// <param name="folderIndentLevel">The folder indent level (if there is one)</param>
        /// <param name="areTopLevel">Are these top-level (root) test sets</param>
        /// <returns>The last used indent level</returns>
        protected string PopulateTestSets(List<TestSetView> testSets, List<HierarchicalDataItem> dataItems, string folderIndentLevel, bool areTopLevel = false)
        {
            //Iterate through all the TestSets and populate the dataitem (only some columns are needed)
            string testSetIndentLevel = (areTopLevel) ? folderIndentLevel : folderIndentLevel + "AAA"; //Add on to the req indent level
            foreach (TestSetView testSet in testSets)
            {
                //Create the data-item
                HierarchicalDataItem dataItem = new HierarchicalDataItem();

                //Populate the necessary fields
                dataItem.PrimaryKey = testSet.TestSetId;
                dataItem.Indent = testSetIndentLevel;
                //dataItem.Alternate = testSet.IsDynamic;   //TODO: Uncomment once we support dynamic test sets

                //Name/Desc
                DataItemField dataItemField = new DataItemField();
                dataItemField.FieldName = "Name";
                dataItemField.TextValue = testSet.Name;
                dataItem.Summary = false;
                dataItem.Fields.Add("Name", dataItemField);

                //Add to the items collection
                dataItems.Add(dataItem);

                //Increment the indent level
                testSetIndentLevel = HierarchicalList.IncrementIndentLevel(testSetIndentLevel);
            }

            return testSetIndentLevel;
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

            try
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

                //Update the user's project settings
                bool changed = false;
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS);
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

        #region ITreeViewService Methods

        /// <summary>
        /// Deletes a testSet folder
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="nodeId">The node id of the folder to be deleted</param>
        public void TreeView_DeleteNode(int projectId, string nodeId)
        {
            const string METHOD_NAME = "TreeView_DeleteNode";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (need to have delete permissions)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            int testSetFolderId = 0;
            if (Int32.TryParse(nodeId, out testSetFolderId) && testSetFolderId > 0)
            {
                try
                {
                    //Delete the specified folder
                    TestSetManager testSetManager = new TestSetManager();
                    testSetManager.TestSetFolder_Delete(projectId, testSetFolderId, userId);

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                }
                catch (Exception exception)
                {
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns the parent node (if any) of the current node
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="nodeId">The node we're interested in</param>
        /// <returns>The parent node</returns>
        public string TreeView_GetParentNode(int projectId, string nodeId)
        {
            const string METHOD_NAME = "TreeView_GetParentNode";

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

            int testSetFolderId = 0;
            if (Int32.TryParse(nodeId, out testSetFolderId) && testSetFolderId > 0)
            {
                try
                {
                    string parentNodeId = "";
                    //Get the parent of the specified folder
                    TestSetManager testSetManager = new TestSetManager();
                    TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId);
                    if (testSetFolder != null && testSetFolder.ParentTestSetFolderId.HasValue)
                    {
                        parentNodeId = testSetFolder.ParentTestSetFolderId.ToString();
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return parentNodeId;
                }
                catch (ArtifactNotExistsException)
                {
                    return "";
                }
                catch (Exception exception)
                {
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                    throw;
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>Called when test sets or folders are dropped onto a folder in the treeview</summary>
        /// <param name="projectId">The current project</param>
        /// <param name="userId">The current user</param>
        /// <param name="artifactIds">The ids of the testSets</param>
        /// <param name="nodeId">The id of the folder</param>
        public void TreeView_DragDestination(int projectId, int[] artifactIds, int nodeId)
        {
            const string METHOD_NAME = "TreeView_DragDestination";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to modify testSets (limited view insufficient)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the folder id (or root if -1)
                int? folderId = null;
                if (nodeId > 0)
                {
                    folderId = nodeId;
                }

                //Make sure the folder exists
                TestSetManager testSetManager = new TestSetManager();
                if (folderId.HasValue)
                {
                    TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(folderId.Value);
                    if (testSetFolder == null)
                    {
                        //Folder does not exist
                        return;
                    }
                }

                //Get the list of folders, not needed if moving to root
                List<TestSetFolderHierarchyView> testSetFolders = null;
                if (folderId.HasValue)
                {
                    testSetFolders = testSetManager.TestSetFolder_GetList(projectId);
                }

                //Retrieve each artifact (test set or folder) in the list and move to the specified folder
                foreach (int artifactId in artifactIds)
                {
                    //See if we have a folder or test set
                    if (artifactId > 0)
                    {
                        //Test Case
                        int testSetId = artifactId;
                        testSetManager.TestSet_UpdateFolder(testSetId, folderId);
                    }
                    if (artifactId < 0)
                    {
                        //Test Folder
                        int testFolderId = -artifactId;

                        TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testFolderId);
                        if (testSetFolder != null)
                        {
                            //Check to make sure we're not making it's parent either this folder
                            //or one of its children
                            if (folderId.HasValue && testSetFolders != null && testSetFolders.Count > 0)
                            {
                                string folderIndent = testSetFolders.FirstOrDefault(f => f.TestSetFolderId == testFolderId).IndentLevel;
                                string newParentIndent = testSetFolders.FirstOrDefault(f => f.TestSetFolderId == folderId.Value).IndentLevel;

                                if (newParentIndent.Length >= folderIndent.Length && newParentIndent.Substring(0, folderIndent.Length) == folderIndent)
                                {
                                    //Throw a meaningful exception
                                    throw new InvalidOperationException(Resources.Messages.TestSetsService_CannotMoveFolderUnderItself);
                                }
                            }

                            //Move the test folder, need to make sure we don't create an infinite loop
                            testSetFolder.StartTracking();
                            testSetFolder.ParentTestSetFolderId = folderId;
                            testSetManager.TestSetFolder_Update(testSetFolder);
                        }
                    }
                }
            }
            catch (ArtifactNotExistsException)
            {
                //Fail quietly
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>Returns the tooltip for a node (used if not provided when node created)</summary>
        /// <param name="nodeId">The id of the node (test case folder)</param>
        /// <returns>The tooltip</returns>
        public string TreeView_GetNodeTooltip(string nodeId)
        {
            int testSetFolderId;
            if (Int32.TryParse(nodeId, out testSetFolderId))
            {
                TestSetManager testSetManager = new TestSetManager();
                TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId);

                if (testSetFolder != null)
                {
                    string tooltip = "";
                    //See if we have any parent folders
                    List<TestSetFolderHierarchyView> parentFolders = testSetManager.TestSetFolder_GetParents(testSetFolder.ProjectId, testSetFolder.TestSetFolderId, false);
                    foreach (TestSetFolderHierarchyView parentFolder in parentFolders)
                    {
                        tooltip += "<u>" + parentFolder.Name + "</u> &gt; ";
                    }

                    tooltip += "<u>" + GlobalFunctions.HtmlRenderAsPlainText(testSetFolder.Name) + "</u>";
                    if (!String.IsNullOrEmpty(testSetFolder.Description))
                    {
                        tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testSetFolder.Description);
                    }
                    return tooltip;
                }
            }
            return null;
        }

        /// <summary>Returns the list of testSet folders contained in a parent node</summary>
        /// <param name="userId">The current user</param>
        /// <param name="parentId">The id of the parent folder</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The list of treeview nodes to display</returns>
        public List<TreeViewNode> TreeView_GetNodes(int projectId, string parentId)
        {
            const string METHOD_NAME = "TreeView_GetNodes";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view testSets (limited view insufficient)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                List<TreeViewNode> nodes = new List<TreeViewNode>();

                //Get the list of project testSet folders from the business object
                TestSetManager testSetManager = new TestSetManager();

                //See if we need the root folder (folderId = 0)
                if (String.IsNullOrEmpty(parentId))
                {
                    nodes.Add(new TreeViewNode(0.ToString(), Resources.Main.Global_Root, null));
                }
                else
                {
                    int? parentFolderId = null;
                    if (!String.IsNullOrEmpty(parentId))
                    {
                        parentFolderId = Int32.Parse(parentId);
                        if (parentFolderId == 0)
                        {
                            //We want the direct children of the root, so set to NULL
                            parentFolderId = null;
                        }
                    }
                    List<TestSetFolder> testSetFolders = testSetManager.TestSetFolder_GetByParentId(projectId, parentFolderId);

                    foreach (TestSetFolder testSetFolder in testSetFolders)
                    {
                        nodes.Add(new TreeViewNode(testSetFolder.TestSetFolderId.ToString(), testSetFolder.Name, testSetFolder.Description));
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return nodes;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>Sets the currently selected node so that it can be persisted for future page loads</summary>
        /// <param name="nodeId">The id of the node to persist</param>
        /// <param name="projectId">The id of the project</param>
        public void TreeView_SetSelectedNode(int projectId, string nodeId)
        {
            const string METHOD_NAME = "TreeView_SetSelectedNode";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view testSets (limited view insufficient)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //We simply store this in a project setting
                int folderId = -1;  //Default to root folder
                if (!String.IsNullOrEmpty(nodeId))
                {
                    int nodeIdInt;
                    if (Int32.TryParse(nodeId, out nodeIdInt) && nodeIdInt > 0)
                    {
                        folderId = nodeIdInt;
                    }
                }
                SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, folderId);
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>Gets a comma-separated list of parent nodes that are to be expanded based on the selected node stored in the project settings collection. Used when the page is first loaded or when refresh is clicked</summary>
        /// <param name="userId">The id of the current user</param>
        /// <param name="projectId">The id of the project</param>
        public List<string> TreeView_GetExpandedNodes(int projectId)
        {
            const string METHOD_NAME = "TreeView_GetExpandedNodes";

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
                List<string> nodeList = new List<string>();
                //Get the currently selected node (if there is one)
                int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (selectedNodeId == -1)
                {
                    //We have the root node selected
                    nodeList.Insert(0, 0.ToString());
                }
                else
                {
                    //Get the list of all folders in the project and locate the selected item
                    TestSetManager testSetManager = new TestSetManager();
                    List<TestSetFolderHierarchyView> testSetFolders = testSetManager.TestSetFolder_GetList(projectId);
                    TestSetFolderHierarchyView testSetFolder = testSetFolders.FirstOrDefault(f => f.TestSetFolderId == selectedNodeId);

                    //Now iterate through successive parents to get the folder path
                    while (testSetFolder != null)
                    {
                        nodeList.Insert(0, testSetFolder.TestSetFolderId.ToString());
                        Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Added node : " + testSetFolder.TestSetFolderId + " to list");
                        if (testSetFolder.ParentTestSetFolderId.HasValue)
                        {
                            testSetFolder = testSetFolders.FirstOrDefault(f => f.TestSetFolderId == testSetFolder.ParentTestSetFolderId.Value);
                        }
                        else
                        {
                            testSetFolder = null;
                        }
                    }

                    //Finally add the root folder
                    nodeList.Insert(0, 0.ToString());
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return nodeList;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets all the test set folders in the treeview as a simple hierarchical lookup dictionary
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The datasource for the dropdown hierarchy control</returns>
        public JsonDictionaryOfStrings TreeView_GetAllNodes(int projectId)
        {
            const string METHOD_NAME = "TreeView_GetAllNodes";

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
                //Get the list of all folders in the project
                TestSetManager testSetManager = new TestSetManager();
                List<TestSetFolderHierarchyView> testSetFolders = testSetManager.TestSetFolder_GetList(projectId);

                //Convert to the necessary lookup
                JsonDictionaryOfStrings testSetFolderDic = ConvertLookupValues(testSetFolders.OfType<DataModel.Entity>().ToList(), "TestSetFolderId", "Name", "IndentLevel");

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return testSetFolderDic;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Adds a new node to the tree
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="name">The name of the new node</param>
        /// <param name="description">The description of the node</param>
        /// <param name="parentNodeId">The id of the parent node to add it under (optional)</param>
        /// <returns>The id of the new node</returns>
        public string TreeView_AddNode(int projectId, string name, string parentNodeId, string description)
        {
            const string METHOD_NAME = "TreeView_AddNode";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (need to have create permissions)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                int? parentTestSetFolderId = null;
                if (!String.IsNullOrEmpty(parentNodeId))
                {
                    int intValue;
                    if (Int32.TryParse(parentNodeId, out intValue))
                    {
                        parentTestSetFolderId = intValue;
                    }
                    else
                    {
                        throw new System.ServiceModel.FaultException(Resources.Messages.TestSetsService_TestSetFolderIdNotInteger);
                    }
                }

                if (String.IsNullOrWhiteSpace(name))
                {
                    throw new System.ServiceModel.FaultException(Resources.Messages.TestSetsService_TestSetFolderNameRequired);
                }
                else
                {
                    //Add the new folder and return the new node id
                    TestSetManager testSetManager = new TestSetManager();
                    int newTestSetFolderId = testSetManager.TestSetFolder_Create(name.Trim().SafeSubstring(0, 255), projectId, description, parentTestSetFolderId).TestSetFolderId;

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return newTestSetFolderId.ToString();
                }

            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing node in the tree
        /// </summary>
        /// <param name="nodeId">The id of the node to update</param>
        /// <param name="projectId">The id of the project</param>
        /// <param name="name">The name of the new node</param>
        /// <param name="parentNodeId">The id of the parent node to add it under (optional)</param>
        /// <param name="description">the description of the node</param>
        public void TreeView_UpdateNode(int projectId, string nodeId, string name, string parentNodeId, string description)
        {
            const string METHOD_NAME = "TreeView_UpdateNode";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (need to have modify all)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, Artifact.ArtifactTypeEnum.TestSet);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            int testSetFolderId = 0;
            if (Int32.TryParse(nodeId, out testSetFolderId) && testSetFolderId > 0)
            {
                try
                {
                    int? parentTestSetFolderId = null;
                    if (!String.IsNullOrEmpty(parentNodeId))
                    {
                        int intValue;
                        if (Int32.TryParse(parentNodeId, out intValue))
                        {
                            parentTestSetFolderId = intValue;
                        }
                        else
                        {
                            throw new System.ServiceModel.FaultException(Resources.Messages.TestSetsService_TestSetFolderIdNotInteger);
                        }
                    }

                    if (String.IsNullOrWhiteSpace(name))
                    {
                        throw new System.ServiceModel.FaultException(Resources.Messages.TestSetsService_TestSetFolderNameRequired);
                    }
                    else
                    {
                        //Update the existing folder (assuming that it exists)
                        TestSetManager testSetManager = new TestSetManager();
                        TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId);
                        if (testSetFolder != null)
                        {
                            testSetFolder.StartTracking();
                            testSetFolder.Name = name.Trim().SafeSubstring(0, 255);
                            testSetFolder.Description = description;
                            //Make sure you don't try and set a folder to be its own parent (!)
                            if (!parentTestSetFolderId.HasValue || parentTestSetFolderId != testSetFolderId)
                            {
                                testSetFolder.ParentTestSetFolderId = parentTestSetFolderId;
                            }
                            testSetManager.TestSetFolder_Update(testSetFolder);
                        }

                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                    }
                }
                catch (ArtifactNotExistsException)
                {
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format("Unable to update testSet folder '{0}' as it does not exist in the system ", nodeId));
                    //Fail quietly
                }
                catch (FolderCircularReferenceException)
                {
                    throw new InvalidOperationException(Resources.Messages.TestSetsService_CannotMoveFolderUnderItself);
                }
                catch (Exception exception)
                {
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                    throw;
                }
            }
        }

        #endregion
    }
}
