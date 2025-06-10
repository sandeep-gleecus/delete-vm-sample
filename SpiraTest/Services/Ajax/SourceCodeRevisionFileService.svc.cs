using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Activation;
using System.Text;
using System.Web;
using DiffPlex.DiffBuilder.Model;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Provides the web service used to interacting with the various client-side source code file/folder list AJAX components
	/// </summary>
	[
	AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
	]
	public class SourceCodeRevisionFileService : AjaxWebServiceBase, ISourceCodeRevisionFileService
    {
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService::";

        private const int NUMBER_ROWS_MAX = 500;

        #region Source Code Methods

        /// <summary>
        /// Updates the setting for whether to show unified or side by side view
        /// </summary>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="isUnified">Should we show unified view</param>
        public void SourceCode_UpdateDiffViewSetting(int projectId, bool isUnified)
        {
            const string METHOD_NAME = "SourceCode_RetrieveFilesForRevision";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }
            try
            {
                //Update the setting
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS);
                if (settings != null)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DIFF_MODE] = (isUnified) ? "unified" : "split";
                    settings.Save();
                }
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of files contained in a specific revision/commit
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="branchKey">The name of the branch</param>
        /// <param name="revisionKey">The revision's key</param>
        /// <param name="themeBaseUrl">The theme base URL</param>
        /// <returns>The list of files alphabetically, ignoring folder</returns>
        public List<DataItem> SourceCode_RetrieveFilesForRevision(int projectId, string branchKey, string revisionKey, string themeBaseUrl)
        {
            const string METHOD_NAME = "SourceCode_RetrieveFilesForRevision";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the manager for this project
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);

                int totalCount;
                List<SourceCodeFile> peerSourceCodeFiles = sourceCodeManager.RetrieveFilesForRevision(
                    revisionKey,
                    branchKey,
                    SourceCodeManager.FIELD_NAME,
                    true,
                    1,
                    NUMBER_ROWS_MAX,
                    null,
                    out totalCount);

                //Convert to ajax data objects
                List<DataItem> dataItems = new List<DataItem>();
                if (peerSourceCodeFiles != null && peerSourceCodeFiles.Count > 0)
                {
                    foreach (SourceCodeFile sourceCodeFile in peerSourceCodeFiles)
                    {
                        DataItem dataItem = new DataItem();
                        dataItems.Add(dataItem);

                        //Populate data
                        dataItem.PrimaryKey = sourceCodeFile.FileId;
                        dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionFileDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + HttpUtility.UrlEncode(revisionKey) + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + HttpUtility.UrlEncode(sourceCodeFile.FileKey));

                        //Name
                        DataItemField dataItemField = new DataItemField();
                        dataItemField.FieldName = SourceCodeManager.FIELD_NAME;
                        dataItemField.TextValue = sourceCodeFile.Name;
                        dataItemField.Tooltip = sourceCodeFile.FileKey;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //Filetype
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = "Filetype";
                        dataItemField.TextValue = themeBaseUrl + "Images/Filetypes/" + GlobalFunctions.GetFileTypeImage(sourceCodeFile.Name);
                        dataItemField.Tooltip = GlobalFunctions.GetFileTypeImage(sourceCodeFile.Name);
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return dataItems;
            }
            catch (ArtifactNotExistsException)
            {
                return null;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of the most recent revisions/commit for a file
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="branchKey">The name of the branch</param>
        /// <param name="fileKey">The file's key/path</param>
        /// <param name="themeBaseUrl">The theme base URL</param>
        /// <returns>The list of commits ordered by most recent first</returns>
        public List<DataItem> SourceCode_RetrieveRevisionsForFile(int projectId, string branchKey, string fileKey, string themeBaseUrl)
        {
            const string METHOD_NAME = "SourceCode_RetrieveRevisionsForFile";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the manager for this project
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);

                int totalCount;
                List<SourceCodeCommit> sourceCodeCommits = sourceCodeManager.RetrieveRevisionsForFile(
                    fileKey,
                    branchKey,
                    SourceCodeManager.FIELD_UPDATE_DATE,
                    false,
                    1,
                    NUMBER_ROWS_MAX,
                    null,
                    0,
                    out totalCount);

                //Convert to ajax data objects
                List<DataItem> dataItems = new List<DataItem>();
                if (sourceCodeCommits != null && sourceCodeCommits.Count > 0)
                {
                    foreach (SourceCodeCommit sourceCodeCommit in sourceCodeCommits)
                    {
                        DataItem dataItem = new DataItem();
                        dataItems.Add(dataItem);

                        //Populate data
                        dataItem.PrimaryKey = sourceCodeCommit.RevisionId;
                        dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionFileDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + HttpUtility.UrlEncode(sourceCodeCommit.Revisionkey) + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + HttpUtility.UrlEncode(fileKey));

                        //Name/Message
                        DataItemField dataItemField = new DataItemField();
                        dataItemField.FieldName = SourceCodeManager.FIELD_NAME;
                        dataItemField.Caption = sourceCodeCommit.Name;
                        dataItemField.TextValue = sourceCodeCommit.Revisionkey;
                        dataItemField.Tooltip = sourceCodeCommit.Message;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //Update Date
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = SourceCodeManager.FIELD_UPDATE_DATE;
                        dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, sourceCodeCommit.UpdateDate);
                        dataItemField.Tooltip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, sourceCodeCommit.UpdateDate);
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //AuthorName
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = SourceCodeManager.FIELD_AUTHOR;
                        dataItemField.TextValue = sourceCodeCommit.AuthorName;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //Action
                        dataItemField = new DataItemField();
                        dataItemField.FieldName = SourceCodeManager.FIELD_ACTION;
                        dataItemField.Tooltip = Resources.Fields.Revision;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                        //For certain actions, change the icon
                        string revisionIcon = "artifact-Revision.svg";
                        SourceCodeFileEntry sourceCodeFileEntry = sourceCodeCommit.Files.FirstOrDefault(f => f.FileKey == fileKey);
                        if (sourceCodeFileEntry != null)
                        {
                            if (sourceCodeFileEntry.Action == "Added")
                            {
                                revisionIcon = "artifact-Revision-Add.svg";
                            }
                            if (sourceCodeFileEntry.Action == "Deleted")
                            {
                                revisionIcon = "artifact-Revision-Delete.svg";
                            }
                            dataItemField.TextValue = themeBaseUrl + "Images/" + revisionIcon;
                            dataItemField.Tooltip = sourceCodeFileEntry.Action;
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return dataItems;
            }
            catch (ArtifactNotExistsException)
            {
                return null;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves the source code file by its key and returns as text (if possible)
        /// Gets the latest revision for that branch unless a revision key is provided
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="branchKey">The name of the current branch</param>
        /// <param name="fileKey">The key of the file</param>
        /// <param name="revisionKey">The key of the revision</param>
        /// <returns>The raw content</returns>
        public string SourceCodeFile_OpenText(int projectId, string branchKey, string fileKey, string revisionKey)
        {
            const string METHOD_NAME = "SourceCodeFile_OpenText";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the source code file as text
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                string text = sourceCodeManager.OpenFileAsText(fileKey, revisionKey, branchKey);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return text;
            }
            catch (ArtifactNotExistsException)
            {
                return null;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves the DIFF between two revisions of the same file as a side by side comparison
        /// </summary>
        /// <param name="branchKey">The name of the branch</param>
        /// <param name="currentRevisionKey">The current revision</param>
        /// <param name="fileKey">The key (path) to the file being compared</param>
        /// <param name="previousRevisionKey">The previous revision</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The difference object</returns>
        public TextDiff SourceCodeFile_OpenSideBySideTextDiff(int projectId, string branchKey, string fileKey, string currentRevisionKey, string previousRevisionKey)
        {
            const string METHOD_NAME = "SourceCodeFile_OpenSideBySideTextDiff";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the source code file as text
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                SideBySideDiffModel sideBySideDiffModel = sourceCodeManager.GenerateSideBySideDiffBetweenFileRevisions(fileKey, currentRevisionKey, previousRevisionKey, branchKey);

                //Covert to a data object
                TextDiff textDiff = new TextDiff();
                PopulationFunctions.PopulateDataItem(textDiff, sideBySideDiffModel);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return textDiff;
            }
            catch (ArtifactNotExistsException)
            {
                return null;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }


		/// <summary>
		/// Retrieves the DIFF between two revisions of the same file as unified view
		/// </summary>
		/// <param name="branchKey">The name of the branch</param>
		/// <param name="currentRevisionKey">The current revision</param>
		/// <param name="fileKey">The key (path) to the file being compared</param>
		/// <param name="previousRevisionKey">The previous revision</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The difference object</returns>
		public TextDiffPane SourceCodeFile_OpenUnifiedTextDiff(int projectId, string branchKey, string fileKey, string currentRevisionKey, string previousRevisionKey)
		{
			const string METHOD_NAME = "SourceCodeFile_OpenUnifiedTextDiff";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the source code file as text
				SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
				DiffPaneModel diffPaneModel = sourceCodeManager.GenerateUnifiedDiffBetweenFileRevisions(fileKey, currentRevisionKey, previousRevisionKey, branchKey);

				//Covert to a data object
				TextDiffPane textDiffPane = new TextDiffPane();
				PopulationFunctions.PopulateDataItem(textDiffPane, diffPaneModel);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return textDiffPane;
			}
			catch (ArtifactNotExistsException)
			{
				return null;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Retrieves the source code file by its key and returns the markdown as HTML for display inline
		/// Gets the latest revision for that branch unless a revision key is provided
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="attachmentId">The id of the file</param>
		/// <returns>The markdown converted to an HTML preview</returns>
		public string SourceCodeFile_OpenMarkdown(int projectId, string branchKey, string fileKey, string revisionKey)
        {
            const string METHOD_NAME = "SourceCodeFile_OpenMarkdown";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the source code file
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                string text = sourceCodeManager.OpenFileAsText(fileKey, revisionKey, branchKey);

                //Finally convert the markdown to HTML
                string html = GlobalFunctions.Md2Html(text);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return html;
            }
            catch (ArtifactNotExistsException)
            {
                return null;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Sets the current branch for the project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="branchKey">The name/path of the branch</param>
        public void SourceCode_SetSelectedBranch(int projectId, string branchKey)
        {
            const string METHOD_NAME = CLASS_NAME + "RetrievePaginationOptions()";
            Logger.LogEnteringEvent(METHOD_NAME);

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
                //Set the current branch
                SourceCodeManager.Set_UserSelectedBranch(userId, projectId, branchKey);

                Logger.LogExitingEvent(METHOD_NAME);
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
        }

        /// <summary>Returns a single source code file and revision record for use by the page binding</summary>
        /// <param name="branchKey">The name of the branch</param>
        /// <param name="fileKey">The path of the file</param>
        /// <param name="revisionKey">The key of the revision</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>A file data item</returns>
        public DataItem SourceCode_RetrieveDataItem(int projectId, string fileKey, string revisionKey, string branchKey)
        {
            const string METHOD_NAME = CLASS_NAME + "SourceCode_RetrieveDataItem";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business classes
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);

                //Create the data item record (no filter items)
                DataItem dataItem = new DataItem();

                //Retrieve the specific source code file
                SourceCodeFile sourceCodeFile = sourceCodeManager.RetrieveFileByKey(fileKey, branchKey);

                //Retrieve the specific revision
                SourceCodeCommit sourceCodeCommit = sourceCodeManager.RetrieveRevisionByKey(revisionKey);

                //Need to get previous revision of this file, if there is one
                SourceCodeCommit previousCommit = sourceCodeManager.RetrievePreviousRevision(sourceCodeFile.FileKey, sourceCodeCommit.RevisionId);

                //Populate the row data item
                PopulateRow(projectId, branchKey, dataItem, sourceCodeFile, sourceCodeCommit, previousCommit);
                
                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return dataItem;
            }
            catch (SourceCodeProviderArtifactPermissionDeniedException)
            {
                //Just return no data back
                return null;
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

        #endregion

        #region INavigationService Methods

        /// <summary>
        /// Returns a list of pagination options that the user can choose from
        /// </summary>
        /// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
        public JsonDictionaryOfStrings NavigationBar_RetrievePaginationOptions(int projectId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the size of pages returned and the currently selected page
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
        /// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
        public void NavigationBar_UpdatePagination(int projectId, int pageSize, int currentPage)
        {
            throw new NotImplementedException();
        }

        public List<HierarchicalDataItem> NavigationBar_RetrieveList(int projectId, string indentLevel, int displayMode, int? selectedItemId, int? containerId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the display settings used by the Navigation Bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="displayMode">Not used for this service</param>
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
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Update the user's project settings
                bool changed = false;
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS);
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
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
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
        /// <param name="branchKey">The name of the branch</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="sourceCodeFile">The source code file object</param>
        /// <param name="sourceCodeCommit">The source code commit object</param>
        /// <param name="sourceCodePreviousRevision">The previous commit</param>
		protected void PopulateRow(int projectId, string branchKey, DataItem dataItem, SourceCodeFile sourceCodeFile, SourceCodeCommit sourceCodeCommit, SourceCodeCommit sourceCodePreviousRevision = null)
		{
			//Set the primary key
			dataItem.PrimaryKey = sourceCodeFile.FileId;

            //Set the custom URL, depends on whether we are in the context of a revision or not
            dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("SourceCodeFileViewer.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + HttpUtility.UrlEncode(sourceCodeFile.FileKey) + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + HttpUtility.UrlEncode(sourceCodeCommit.Revisionkey) + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY + "=" + HttpUtility.UrlEncode(branchKey));

            //Source Code Files don't have an attachment flag
            dataItem.Attachment = false;

            //We need to add the various source code file fields to be displayed
            //File Name
            DataItemField dataItemField = new DataItemField();
            dataItemField.FieldName = "Filename";
			//The filetype image is passed as a 'tooltip'
			dataItemField.TextValue = sourceCodeFile.Name;
			dataItemField.Tooltip = "Filetypes/" + GlobalFunctions.GetFileTypeImage(sourceCodeFile.Name);
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //File Size
            dataItemField = new DataItemField();
            dataItemField.FieldName = SourceCodeManager.FIELD_SIZE;
			dataItemField.IntValue = sourceCodeFile.Size;
            dataItemField.TextValue = sourceCodeFile.Size.DisplayFileSizeBytes();
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //Commit Author
            dataItemField = new DataItemField();
            dataItemField.FieldName = SourceCodeManager.FIELD_AUTHOR;
			dataItemField.TextValue = sourceCodeCommit.AuthorName;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //Revision
            dataItemField = new DataItemField();
            dataItemField.FieldName = SourceCodeManager.FIELD_COMMIT;
            dataItemField.IntValue = sourceCodeCommit.RevisionId;
			dataItemField.TextValue = sourceCodeCommit.Revisionkey;
            dataItemField.Caption = sourceCodeCommit.Name;
            dataItemField.Tooltip = sourceCodeCommit.Message;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //Commit Date
            dataItemField = new DataItemField();
            dataItemField.FieldName = SourceCodeManager.FIELD_LASTUPDATED;
            dataItemField.Tooltip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(sourceCodeCommit.UpdateDate));
            dataItemField.Editable = false;
            dataItemField.Required = false;
            dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(sourceCodeCommit.UpdateDate));
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

            //Provide the file key as a field
            DataItemField fileKeyField = new DataItemField();
            fileKeyField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
            fileKeyField.FieldName = "FileKey";
            fileKeyField.TextValue = sourceCodeFile.FileKey;
            dataItem.Fields.Add(fileKeyField.FieldName, fileKeyField);

            //Provide the file type name as a field
            string imageUrl = "Filetypes/" + GlobalFunctions.GetFileTypeImage(sourceCodeFile.Name);
            Dictionary<string, string> fileTypeInfo = GlobalFunctions.GetFileTypeInformation(sourceCodeFile.Name);
            DataItemField fileTypeField = new DataItemField();
            fileTypeField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
            fileTypeField.FieldName = "FileType";
            fileTypeField.TextValue = fileTypeInfo["description"];
            fileTypeField.Tooltip = imageUrl;
            dataItem.Fields.Add(fileTypeField.FieldName, fileTypeField);

            //Provide the mime type as a field
            string mimeType = GlobalFunctions.GetFileMimeType(sourceCodeFile.Name);
            dataItem.Fields.Add("_MimeType", new DataItemField() { FieldName = "_MimeType", TextValue = mimeType });

            //Get the previous revision
            if (sourceCodePreviousRevision != null)
            {
                dataItemField = new DataItemField();
                dataItemField.FieldName = "PreviousRevision";
                dataItemField.IntValue = sourceCodePreviousRevision.RevisionId;
                dataItemField.TextValue = sourceCodePreviousRevision.Revisionkey;
                dataItemField.Caption = sourceCodePreviousRevision.Name;
                dataItemField.Tooltip = sourceCodePreviousRevision.Message;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            }
        }

        #endregion
    }
}
