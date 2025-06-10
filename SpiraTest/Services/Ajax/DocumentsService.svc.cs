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
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>Provides the web service used to interacting with the various client-side document management AJAX components</summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class DocumentsService : SortedListServiceBase, IDocumentsService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService::";

        private const string SPECIAL_FIELD_PLAIN_TEXT_CONTENT = "_Text";
        private const string SPECIAL_FIELD_RICH_TEXT_CONTENT = "_Html";
		private const string SPECIAL_FIELD_DATA_CONTENT = "_Data"; // data to parse by a client side library (usually sent to client as JSON), for example, diagrams

		/* IDocumentService methods */

		/// <summary>
		/// Retrieves the raw document attachment by its ID
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="attachmentId">The id of the file</param>
		/// <returns>The raw content</returns>
		public byte[] Documents_OpenFile(int projectId, int attachmentId)
        {
            const string METHOD_NAME = "Documents_OpenFile";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized, limited is OK
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the attachment for the specific attachment id
                AttachmentManager attachmentManager = new AttachmentManager();
                ProjectAttachmentView projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && projectAttachmentView.EditorId != userId && projectAttachmentView.AuthorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Open the file
                byte[] binaryData;
                using (FileStream fileStream = attachmentManager.OpenById(attachmentId))
                {
                    //Read the file in.
                    binaryData = new byte[fileStream.Length];
                    fileStream.Read(binaryData, 0, (int)fileStream.Length);
                    fileStream.Close();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return binaryData;
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
        /// Retrieves the document attachment by its ID and returns as text (if possible)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="attachmentId">The id of the file</param>
        /// <returns>The raw content</returns>
        public string Documents_OpenText(int projectId, int attachmentId)
        {
            const string METHOD_NAME = "Documents_OpenText";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized, limited is OK
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the attachment for the specific attachment id
                AttachmentManager attachmentManager = new AttachmentManager();
                ProjectAttachmentView projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && projectAttachmentView.EditorId != userId && projectAttachmentView.AuthorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Open the file
                byte[] binaryData;
                using (FileStream fileStream = attachmentManager.OpenById(attachmentId))
                {
                    //Read the file in.
                    binaryData = new byte[fileStream.Length];
                    fileStream.Read(binaryData, 0, (int)fileStream.Length);
                    fileStream.Close();
                }

                //Convert into UTF8 text (htmlencode to prevent XSS attacks)
                string text;
                try
                {
                    text = UnicodeEncoding.UTF8.GetString(binaryData);
                }
                catch (Exception)
                {
                    text = null;
                }

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
        /// Retrieves the document attachment by its ID and returns the markdown as html preview
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="attachmentId">The id of the file</param>
        /// <returns>The html preview of the markdown</returns>
        public string Documents_OpenMarkdown(int projectId, int attachmentId)
        {
            const string METHOD_NAME = "Documents_OpenMarkdown";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized, limited is OK
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the attachment for the specific attachment id
                AttachmentManager attachmentManager = new AttachmentManager();
                ProjectAttachmentView projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && projectAttachmentView.EditorId != userId && projectAttachmentView.AuthorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Open the file
                byte[] binaryData;
                using (FileStream fileStream = attachmentManager.OpenById(attachmentId))
                {
                    //Read the file in.
                    binaryData = new byte[fileStream.Length];
                    fileStream.Read(binaryData, 0, (int)fileStream.Length);
                    fileStream.Close();
                }

                //Convert into UTF8 text (htmlencode to prevent XSS attacks)
                string text;
                try
                {
                    text = UnicodeEncoding.UTF8.GetString(binaryData);
                }
                catch (Exception)
                {
                    text = null;
                }

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
        /// Counts the number of documents (including source code associated with the specified artifacts)
        /// </summary>
        /// <param name="projectId">The project id</param>
        /// <param name="artifacts">The list of artifacts</param>
        /// <returns>The count</returns>
        public int Documents_Count(int projectId, List<ArtifactReference> artifacts)
        {
            const string METHOD_NAME = "Documents_Count";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited OK because we need it for the counts)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //We need to see if we have any attachments or not so that we can display the appropriate visual cue on the tab
                AttachmentManager attachmentManager = new AttachmentManager();
                int attachmentCount = 0;
                foreach (ArtifactReference artifactReference in artifacts)
                {
                    attachmentCount += attachmentManager.CountByArtifactId(projectId, artifactReference.ArtifactId, (Artifact.ArtifactTypeEnum)artifactReference.ArtifactTypeId, null, 0);
                }

                //Check that we are to include source code revisions
                if (Common.Global.SourceCode_IncludeInAssociationsAndDocuments)
                {
                    try
                    {
                        //For source code we only use the first (primary) artifact reference
                        SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                        List<SourceCodeFile> sourceCodeFiles = sourceCodeManager.RetrieveFilesForArtifact((Artifact.ArtifactTypeEnum)artifacts[0].ArtifactTypeId, artifacts[0].ArtifactId);
                        attachmentCount += sourceCodeFiles.Count;
                    }
                    catch (SourceCodeProviderLoadingException exception)
                    {
                        //Log only as trace
                        Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                    }
                }

                return attachmentCount;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Inserts a new file attachment into the system and associates it with the provided artifact</summary>
        /// <param name="filename">The filename of the attachment</param>
        /// <param name="description">An optional detailed description of the attachment</param>
        /// <param name="authorId">The uploader of the attachment</param>
        /// <param name="encodedData">The attachment itself in binary form using base64 encoding</param>
        /// <param name="artifactId">The id of the artifact to associate the attachment with</param>
        /// <param name="artifactTypeId">The type of artifact to associate the attachment with</param>
        /// <param name="projectId">The project we're uploading the document into</param>
        /// <param name="tags">The meta-tags to associate with the document</param>
        /// <param name="version">The name of the initial version of the document (optional)</param>
        /// <param name="documentTypeId">The type of document being attached (optional)</param>
        /// <param name="projectAttachmentFolderId">The project folder to put the document into (optional, current setting used otherwise)</param>
        /// <returns>The id of the attachment</returns>
        public int UploadFile(int projectId, string filename, string description, int authorId, string encodedData, int? artifactId, int? artifactTypeId, string version, string tags, int? documentTypeId, int? projectAttachmentFolderId)
        {
            const string METHOD_NAME = "UploadFile";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Validate some of the data first
                if (String.IsNullOrEmpty(filename))
                {
                    throw new DataValidationException(Resources.Messages.FileUploadDialog_FilenameRequired);
                }
                if (String.IsNullOrEmpty(encodedData))
                {
                    throw new DataValidationException("You need to provide the image data base64 encoded");
                }

                //Convert the artifact type
                Artifact.ArtifactTypeEnum artifactType = Artifact.ArtifactTypeEnum.None;
                if (artifactTypeId.HasValue)
                {
                    artifactType = (Artifact.ArtifactTypeEnum)artifactTypeId.Value;
                }

                //See if we have a folder to filter by
                //-1 = root folder
                int? folderId = null;
                AttachmentManager attachmentManager = new AttachmentManager();
                if (projectAttachmentFolderId.HasValue && projectAttachmentFolderId.Value > 0)
                {
                    // set the folder id and update the selected node (set to root folder if the folder does not exist)
                    int intValue = (int)(projectAttachmentFolderId.Value);
                    folderId = attachmentManager.ProjectAttachmentFolder_Exists(projectId, intValue) ? intValue : attachmentManager.GetDefaultProjectFolder(projectId);
                    this.TreeView_SetSelectedNode(projectId, folderId.ToString());
                }
                //If no folder is specified, see if we have a saved user setting
                else
                {
                    int selectedNode = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                    if (selectedNode > 0)
                    {
                        if (attachmentManager.ProjectAttachmentFolder_Exists(projectId, selectedNode))
                        {
                            //Filter by specific Folder
                            folderId = selectedNode;
                        }
                        else
                        {
                            //Set to the Root Folder (i.e. no folder tasks only) and update the projectsetting
                            folderId = attachmentManager.GetDefaultProjectFolder(projectId);
                            SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                        }
                    }
                }

                //Strip off the data:image/xxx;base64, section
                int index = encodedData.IndexOf("base64,");
                if (index == -1)
                {
                    throw new DataValidationException(Resources.Messages.FileUploadDialog_EmptyFileUploaded);
                }
                string base64data = encodedData.Substring(index + "base64,".Length);

                //Get the binary data from the base64 encoded string
                byte[] binaryData = System.Convert.FromBase64String(base64data);
                //Call the business object to upload the file attachment (optionally attaching to an artifact if specified)
                int attachmentId = attachmentManager.Insert(projectId, filename, description, authorId, binaryData, artifactId, artifactType, version, tags, documentTypeId, folderId, null);

                attachmentManager.SendCreationNotification(projectId, attachmentId, null, null);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return attachmentId;
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                //The attachment folder does not exist
                throw new DataValidationException(Resources.Messages.FileUploadDialog_DirectoryNotFound);
            }
            catch (System.Security.SecurityException)
            {
                //The attachment folder is not accessible
                throw new DataValidationException(Resources.Messages.FileUploadDialog_DirectorySecurityError);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Creates a new text document into the system (of the specified type)</summary>
        /// <param name="filename">The name of the file to be created</param>
        /// <param name="description">An optional detailed description of the attachment</param>
        /// <param name="authorId">The uploader of the attachment</param>
        /// <param name="projectId">The project we're uploading the document into</param>
        /// <param name="tags">The meta-tags to associate with the document</param>
        /// <param name="version">The name of the initial version of the document (optional)</param>
        /// <param name="documentTypeId">The type of document being attached (optional)</param>
        /// <param name="projectAttachmentFolderId">The project folder to put the document into (optional, current setting used otherwise)</param>
        /// <param name="format">The format of the text file (e.g. 'markdown' or 'html')</param>
        /// <returns>The id of the attachment</returns>
        public int Documents_CreateTextFile(int projectId, string filename, string description, int authorId, string version, string tags, int? documentTypeId, int? projectAttachmentFolderId, string format)
        {
            const string METHOD_NAME = "Documents_CreateTextFile";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Validate some of the data first
                if (String.IsNullOrEmpty(filename))
                {
                    throw new DataValidationException(Resources.Messages.FileUploadDialog_FilenameRequired);
                }
                if (format != "markdown" && format != "html" && format != "feature" && format != "diagram" && format != "orgchart" && format != "mindmap")
                {
                    throw new DataValidationException("The only accepted formats currently are: markdown, html, or feature");
                }

                //See if we have a folder to filter by
                //-1 = root folder
                int? folderId = null;
                AttachmentManager attachmentManager = new AttachmentManager();
                if (projectAttachmentFolderId.HasValue && projectAttachmentFolderId.Value > 0)
                {
                    // set the folder id and update the selected node (set to root folder if the folder does not exist)
                    int intValue = (int)(projectAttachmentFolderId.Value);
                    folderId = attachmentManager.ProjectAttachmentFolder_Exists(projectId, intValue) ? intValue : attachmentManager.GetDefaultProjectFolder(projectId);
                    this.TreeView_SetSelectedNode(projectId, folderId.ToString());
                }
                //If no folder is specified, see if we have a saved user setting
                else
                {
                    int selectedNode = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                    if (selectedNode > 0)
                    {
                        if (attachmentManager.ProjectAttachmentFolder_Exists(projectId, selectedNode))
                        {
                            //Filter by specific Folder
                            folderId = selectedNode;
                        }
                        else
                        {
                            //Set to the Root Folder (i.e. no folder tasks only) and update the projectsetting
                            folderId = attachmentManager.GetDefaultProjectFolder(projectId);
                            SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                        }
                    }
                }

                //Depending on the format, we may need to create an initial document
                string content = "";

                //Convert to bytes
                byte[] binaryData = System.Text.UnicodeEncoding.UTF8.GetBytes(content);

                //Call the business object to upload the file attachment (optionally attaching to an artifact if specified)
                int attachmentId = attachmentManager.Insert(projectId, filename, description, authorId, binaryData, null, Artifact.ArtifactTypeEnum.None, version, tags, documentTypeId, folderId, null);

                //Send new artifact notification email
                attachmentManager.SendCreationNotification(projectId, attachmentId, null, null);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return attachmentId;
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                //The attachment folder does not exist
                throw new DataValidationException(Resources.Messages.FileUploadDialog_DirectoryNotFound);
            }
            catch (System.Security.SecurityException)
            {
                //The attachment folder is not accessible
                throw new DataValidationException(Resources.Messages.FileUploadDialog_DirectorySecurityError);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Inserts a new URL attachment into the system and associates it with the provided artifact</summary>
        /// <param name="url">The URL link to be added</param>
        /// <param name="description">An optional detailed description of the attachment</param>
        /// <param name="authorId">The uploader of the attachment</param>
        /// <param name="artifactId">The id of the artifact to associate the attachment with</param>
        /// <param name="artifactTypeId">The type of artifact to associate the attachment with</param>
        /// <param name="projectId">The project we're uploading the document into</param>
        /// <param name="tags">The meta-tags to associate with the document</param>
        /// <param name="version">The name of the initial version of the document (optional)</param>
        /// <param name="documentTypeId">The type of document being attached (optional)</param>
        /// <param name="projectAttachmentFolderId">The project folder to put the document into (optional, current setting used otherwise)</param>
        /// <returns>The id of the attachment</returns>
        public int UploadUrl(int projectId, string url, string description, int authorId, int? artifactId, int? artifactTypeId, string version, string tags, int? documentTypeId, int? projectAttachmentFolderId)
        {
            const string METHOD_NAME = "UploadUrl";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Validate some of the data first
                if (String.IsNullOrEmpty(url))
                {
                    throw new DataValidationException(Resources.Messages.FileUploadDialog_URLRequired);
                }
                if (!Regex.IsMatch(url, GlobalFunctions.VALIDATION_REGEX_URL))
                {
                    throw new DataValidationException(Resources.Messages.FileUploadDialog_URLNotValid);
                }

                //Convert the artifact type
                Artifact.ArtifactTypeEnum artifactType = Artifact.ArtifactTypeEnum.None;
                if (artifactTypeId.HasValue)
                {
                    artifactType = (Artifact.ArtifactTypeEnum)artifactTypeId.Value;
                }



                //See if we have a folder to filter by
                //-1 = root folder
                int? folderId = null;
                AttachmentManager attachmentManager = new AttachmentManager();
                if (projectAttachmentFolderId.HasValue && projectAttachmentFolderId.Value > 0)
                {
                    // set the folder id and update the selected node (set to root folder if the folder does not exist)
                    int intValue = (int)(projectAttachmentFolderId.Value);
                    folderId = attachmentManager.ProjectAttachmentFolder_Exists(projectId, intValue) ? intValue : attachmentManager.GetDefaultProjectFolder(projectId);
                    this.TreeView_SetSelectedNode(projectId, folderId.ToString());
                }
                //If no folder is specified, see if we have a saved user setting
                else
                {
                    int selectedNode = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                    if (selectedNode > 0)
                    {
                        if (attachmentManager.ProjectAttachmentFolder_Exists(projectId, selectedNode))
                        {
                            //Filter by specific Folder
                            folderId = selectedNode;
                        }
                        else
                        {
                            //Set to the Root Folder (i.e. no folder tasks only) and update the projectsetting
                            folderId = attachmentManager.GetDefaultProjectFolder(projectId);
                            SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                        }
                    }
                }

                //Call the business object to upload the URL attachment (optionally attaching to an artifact if specified)
                int attachmentId = attachmentManager.Insert(projectId, url, description, authorId, artifactId, artifactType, version, tags, documentTypeId, folderId, null);

                attachmentManager.SendCreationNotification(projectId, attachmentId, null, null);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return attachmentId;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        #region ITreeViewService methods

        /// <summary>Called when a file is dropped onto a folder in the treeview</summary>
        /// <param name="projectId">The current project</param>
        /// <param name="userId">The current user</param>
        /// <param name="artifactIds">The ids of the attachments</param>
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

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Make sure the folder exists
                AttachmentManager attachmentManager = new AttachmentManager();
                ProjectAttachmentFolder folder = attachmentManager.RetrieveFolderById(nodeId);
                if (folder != null)
                {
                    //Retrieve each document in the list and move to the specified folder
                    foreach (int attachmentId in artifactIds)
                    {
                        //See if we have a folder or attachment
                        if (attachmentId > 0)
                        {
                            ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId);
                            projectAttachment.StartTracking();
                            projectAttachment.ProjectAttachmentFolderId = nodeId;
                            attachmentManager.Update(projectAttachment, userId);
                        }
                        else
                        {
                            //Document Folder
                            int documentFolderId = -attachmentId;
                            ProjectAttachmentFolder attachmentFolderBeingMoved = attachmentManager.RetrieveFolderById(documentFolderId);
                            if (attachmentFolderBeingMoved != null)
                            {
                                attachmentFolderBeingMoved.StartTracking();
                                //Make sure you don't try and set a folder to be its own parent (!)
                                if (nodeId != documentFolderId)
                                {
                                    attachmentFolderBeingMoved.ParentProjectAttachmentFolderId = nodeId;
                                }
                                attachmentManager.UpdateFolder(attachmentFolderBeingMoved);
                            }
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
        /// <param name="nodeId">The id of the node</param>
        /// <returns>The tooltip</returns>
        public string TreeView_GetNodeTooltip(string nodeId)
        {
            return null;
        }

        /// <summary>Returns the list of document folders contained in a parent node</summary>
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

            //Make sure we're authorized to view documents (limited view insufficient)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Document);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                List<TreeViewNode> nodes = new List<TreeViewNode>();

                //Get the list of project document folders from the business object
                AttachmentManager attachmentManager = new AttachmentManager();
                int? documentFolderId = null;
                if (!String.IsNullOrEmpty(parentId))
                {
                    documentFolderId = Int32.Parse(parentId);
                }
                List<ProjectAttachmentFolder> folders = attachmentManager.RetrieveFoldersByParentId(projectId, documentFolderId);

                foreach (ProjectAttachmentFolder folder in folders)
                {
                    nodes.Add(new TreeViewNode(folder.ProjectAttachmentFolderId.ToString(), folder.Name, folder.Name));
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

            //Make sure we're authorized to view documents (limited view insufficient)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Document);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //We simply store this in a project setting
                SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, Int32.Parse(nodeId));
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
                int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (selectedNodeId != -1)
                {
                    //Get the list of all folders in the project and locate the selected item
                    AttachmentManager attachmentManager = new AttachmentManager();
                    List<ProjectAttachmentFolderHierarchy> folders = attachmentManager.RetrieveFoldersByProjectId(projectId);
                    ProjectAttachmentFolderHierarchy folder = folders.FirstOrDefault(f => f.ProjectAttachmentFolderId == selectedNodeId);

                    //Now iterate through successive parents to get the folder path
                    while (folder != null)
                    {
                        nodeList.Insert(0, folder.ProjectAttachmentFolderId.ToString());
                        Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Added node : " + folder.ProjectAttachmentFolderId + " to list");
                        if (folder.ParentProjectAttachmentFolderId.HasValue)
                        {
                            folder = folders.FirstOrDefault(f => f.ProjectAttachmentFolderId == folder.ParentProjectAttachmentFolderId.Value);
                        }
                        else
                        {
                            folder = null;
                        }
                    }
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
        /// Gets all the nodes in the treeview as a simple hierarchical lookup dictionary
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The datasource for the dropdown hierarchy control</returns>
        public JsonDictionaryOfStrings TreeView_GetAllNodes(int projectId)
        {
            const string METHOD_NAME = "TreeView_GetNodes";

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
                List<TreeViewNode> nodes = new List<TreeViewNode>();

                //Get the list of project document folders from the business object
                AttachmentManager attachmentManager = new AttachmentManager();
                List<ProjectAttachmentFolderHierarchy> projectFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);

                JsonDictionaryOfStrings documentFoldersDic = new JsonDictionaryOfStrings();
                foreach (ProjectAttachmentFolderHierarchy projectFolder in projectFolders)
                {
                    if (!String.IsNullOrEmpty(projectFolder.IndentLevel) && !String.IsNullOrEmpty(projectFolder.Name))
                    {
                        documentFoldersDic.Add(projectFolder.ProjectAttachmentFolderId + "_" + ((projectFolder.IndentLevel).Length / 3) + "_N_N_Y", projectFolder.Name);
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return documentFoldersDic;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Adds a new document folder to the folder hierarchy
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="name">The name of the new node</param>
        /// <param name="description">Not used</param>
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

            //Make sure we're authorized (need to have document create)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, Artifact.ArtifactTypeEnum.Document);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                int? parentDocumentFolderId = null;
                if (String.IsNullOrEmpty(parentNodeId))
                {
                    throw new DataValidationException(Resources.Messages.Admin_DocumentFolderDetails_CannotInsertRoot);
                }
                else
                {
                    int intValue;
                    if (Int32.TryParse(parentNodeId, out intValue))
                    {
                        parentDocumentFolderId = intValue;
                    }
                    else
                    {
                        throw new System.ServiceModel.FaultException(Resources.Messages.DocumentsService_DocumentFolderIdNotInteger);
                    }
                }


                if (String.IsNullOrWhiteSpace(name))
                {
                    throw new DataValidationException(Resources.Messages.DocumentsService_DocumentFolderNameRequired);
                }
                else
                {
                    //Add the new folder and return the new node id
                    AttachmentManager attachmentManager = new AttachmentManager();
                    int newDocumentFolderId = attachmentManager.InsertProjectAttachmentFolder(projectId, name.Trim().SafeSubstring(0, 255), parentDocumentFolderId);

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return newDocumentFolderId.ToString();
                }

            }
            catch (DataValidationException exception)
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

        /// <summary>
        /// Updates an existing node in the tree
        /// </summary>
        /// <param name="nodeId">The id of the node to update</param>
        /// <param name="projectId">The id of the project</param>
        /// <param name="name">The name of the new node</param>
        /// <param name="description">Not used</param>
        /// <param name="parentNodeId">The id of the parent node to add it under (optional)</param>
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

            //Make sure we're authorized (need to have document bulk edit)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, Artifact.ArtifactTypeEnum.Document);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            int documentFolderId = 0;
            if (Int32.TryParse(nodeId, out documentFolderId) && documentFolderId > 0)
            {
                try
                {
                    int? parentDocumentFolderId = null;
                    if (!String.IsNullOrEmpty(parentNodeId))
                    {
                        int intValue;
                        if (Int32.TryParse(parentNodeId, out intValue))
                        {
                            parentDocumentFolderId = intValue;
                        }
                        else
                        {
                            throw new System.ServiceModel.FaultException(Resources.Messages.DocumentsService_DocumentFolderIdNotInteger);
                        }
                    }

                    if (String.IsNullOrWhiteSpace(name))
                    {
                        throw new DataValidationException(Resources.Messages.DocumentsService_DocumentFolderNameRequired);
                    }
                    else
                    {
                        AttachmentManager attachmentManager = new AttachmentManager();
                        if (parentDocumentFolderId.HasValue)
                        {
                            //We need to make sure that we're not creating any circular reference
                            //i.e. the parent folder can't be a child of the current folder
                            string itemIndentLevel = "";
                            string parentIndentLevel = "";
                            List<ProjectAttachmentFolderHierarchy> projectFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);
                            foreach (ProjectAttachmentFolderHierarchy projectFolder in projectFolders)
                            {
                                //Capture the item indent level if we have a match
                                if (projectFolder.ProjectAttachmentFolderId == documentFolderId)
                                {
                                    itemIndentLevel = projectFolder.IndentLevel;
                                }
                                //Capture the item indent level if we have a match
                                if (projectFolder.ProjectAttachmentFolderId == parentDocumentFolderId.Value)
                                {
                                    parentIndentLevel = projectFolder.IndentLevel;
                                }
                            }
                            if (parentIndentLevel.Length >= itemIndentLevel.Length)
                            {
                                if (parentIndentLevel.Substring(0, itemIndentLevel.Length) == itemIndentLevel)
                                {
                                    throw new DataValidationException(Resources.Messages.Admin_DocumentFolderDetails_CannotSetParentToYourChild);
                                }
                            }
                        }
                        else
                        {
                            //Make sure this is the project root that has no parent folder
                            List<ProjectAttachmentFolder> rootDocFolders = attachmentManager.RetrieveFoldersByParentId(projectId, null);
                            if (rootDocFolders.Count > 0 && rootDocFolders[0].ProjectAttachmentFolderId != documentFolderId)
                            {
                                throw new DataValidationException(Resources.Messages.Admin_DocumentFolderDetails_CannotInsertRoot);
                            }
                        }

                        //Update the existing folder (assuming that it exists)
                        ProjectAttachmentFolder projectAttachmentFolder = attachmentManager.RetrieveFolderById(documentFolderId);
                        if (projectAttachmentFolder != null)
                        {
                            projectAttachmentFolder.StartTracking();
                            projectAttachmentFolder.Name = name.Trim().SafeSubstring(0, 255);
                            //Make sure you don't try and set a folder to be its own parent (!)
                            if (!parentDocumentFolderId.HasValue || parentDocumentFolderId != documentFolderId)
                            {
                                projectAttachmentFolder.ParentProjectAttachmentFolderId = parentDocumentFolderId;
                            }
                            attachmentManager.UpdateFolder(projectAttachmentFolder);
                        }

                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                    }
                }
                catch (DataValidationException exception)
                {
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                    throw;
                }
                catch (ArtifactNotExistsException)
                {
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format("Unable to update document folder '{0}' as it does not exist in the system ", nodeId));
                    //Fail quietly
                }
                catch (Exception exception)
                {
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes a document folder
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

            //Make sure we're authorized (need to have document delete)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, Artifact.ArtifactTypeEnum.Document);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            int documentFolderId = 0;
            if (Int32.TryParse(nodeId, out documentFolderId) && documentFolderId > 0)
            {
                try
                {
                    //Make sure the folder is empty (have to use the admin screens in that case)
                    AttachmentManager attachmentManager = new AttachmentManager();
                    int count = attachmentManager.CountForProject(projectId, documentFolderId, null, 0);
                    if (count > 0)
                    {
                        //Will display it as a 'friendly message'
                        throw new DataValidationException(Resources.Messages.DocumentsService_DocumentFolderNotEmptyCannotDelete);
                    }

                    //Also check for child folders
                    count = attachmentManager.RetrieveFoldersByParentId(projectId, documentFolderId).Count;
                    if (count > 0)
                    {
                        //Will display it as a 'friendly message'
                        throw new DataValidationException(Resources.Messages.DocumentsService_DocumentFolderNotEmptyCannotDelete);
                    }

                    //Delete the specified folder
                    attachmentManager.DeleteFolder(projectId, documentFolderId);

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                }
                catch (ProjectDefaultAttachmentFolderException exception)
                {
                    //Will display it as a 'friendly message'
                    throw new DataValidationException(exception.Message);
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

            int documentFolderId = 0;
            if (Int32.TryParse(nodeId, out documentFolderId) && documentFolderId > 0)
            {
                try
                {
                    string parentNodeId = "";
                    //Get the parent of the specified folder
                    AttachmentManager attachmentManager = new AttachmentManager();
                    ProjectAttachmentFolder folder = attachmentManager.RetrieveFolderById(documentFolderId);
                    if (folder != null && folder.ParentProjectAttachmentFolderId.HasValue)
                    {
                        parentNodeId = folder.ParentProjectAttachmentFolderId.Value.ToString();
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

        #endregion

        #region IList interface methods

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
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS);
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
        /// Updates the filters stored in the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="filters">The array of filters (name,value)</param>
        /// <returns>Any error messages</returns>
        public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
        {
            const string METHOD_NAME = "UpdateFilters";

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

            //Call the base method with the appropriate settings collection
            return base.UpdateFilters(userId, projectId, filters, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.Document);
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
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return paginationDictionary;
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
                    customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Document, fieldName);
                }
                else
                {
                    //Toggle the status of the appropriate field name
                    ArtifactManager artifactManager = new ArtifactManager();
                    artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Document, fieldName);
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
                artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Document, fieldName, width);
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
        /// Changes the order of columns in the document list
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
                artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Document, fieldName, newPosition);
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

        #region ICommentService Methods

        /// <summary>
        /// Retrieves the list of comments associated with a document
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactId">The id of the document</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Create the new list of comments
                List<CommentItem> commentItems = new List<CommentItem>();

                //Get the document (to verify permissions) and also the comments
                AttachmentManager attachmentManager = new AttachmentManager();
                UserManager userManager = new UserManager();
                DiscussionManager discussion = new DiscussionManager();
                Attachment attachment = attachmentManager.RetrieveById(artifactId);
                List<IDiscussion> comments = discussion.Retrieve(artifactId, Artifact.ArtifactTypeEnum.Document).ToList();

                //Make sure the user is either the author or editor if limited permissions
                if (authorizationState == Project.AuthorizationState.Limited && attachment.AuthorId != userId && attachment.EditorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //See if we're sorting ascending or descending
                SortDirection sortDirection = (SortDirection)GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)SortDirection.Descending);

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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Update the setting
                SortDirection sortDirection = (SortDirection)sortDirectionId;
                SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)sortDirectionId);
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
        /// <param name="artifactId">The id of the document</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Delete the comment, making sure we have permissions
                DiscussionManager discussion = new DiscussionManager();
                IDiscussion comment = discussion.RetrieveById(commentId, Artifact.ArtifactTypeEnum.Document);
                //If the comment no longer exists do nothing
                if (comment != null && !comment.IsPermanent)
                {
                    if (comment.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin))
                    {
                        discussion.DeleteDiscussionId(commentId, Artifact.ArtifactTypeEnum.Document);
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
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
                int commentId = discussion.Insert(userId, artifactId, Artifact.ArtifactTypeEnum.Document, cleanedComment, projectId, false, true);

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

        #region ISortedListService methods

        /// <summary>
        /// Returns the latest information on a single document in the system
        /// </summary>
        /// <param name="userId">The user we're viewing the document as</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the attachment and custom property business objects
                AttachmentManager attachmentManager = new AttachmentManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

                //Get the attachment view record for the specific attachment id
                ProjectAttachmentView projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, artifactId);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && projectAttachment.EditorId != userId && projectAttachment.AuthorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, DataModel.Artifact.ArtifactTypeEnum.Document, true);

                //Finally populate the dataitem from the dataset
                if (projectAttachment != null)
                {
                    //See if we already have an artifact custom property row
                    if (artifactCustomProperty != null)
                    {
                        PopulateRow(dataItem, projectAttachment, artifactCustomProperty.CustomPropertyDefinitions, artifactCustomProperty, true, false);
                    }
                    else
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Document, true, false);
                        PopulateRow(dataItem, projectAttachment, customProperties, null, true, false);
                    }

					//See if we are allowed to bulk edit status (template setting)
					ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(projectTemplateId);
					if (!templateSettings.Workflow_BulkEditCanChangeStatus && dataItem.Fields.ContainsKey("DocumentStatusId"))
					{
						dataItem.Fields["DocumentStatusId"].Editable = false;
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
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="dataItems">The updated data records</param>
        /// <returns>The list of any validation messages</returns>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.Document);
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
                AttachmentManager attachmentManager = new AttachmentManager();
                //Load the custom property definitions (once, not per artifact)
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Document, false);

                foreach (SortedDataItem dataItem in dataItems)
                {
                    //Get the attachment id
                    int attachmentId = dataItem.PrimaryKey;

                    //Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
                    ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId);
                    if (projectAttachment != null)
                    {
                        projectAttachment.StartTracking();
                        Attachment attachment = projectAttachment.Attachment;
                        if (attachment != null)
                        {
                            ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, attachmentId, DataModel.Artifact.ArtifactTypeEnum.Document, false, customProperties);

                            //Create a new artifact custom property row if one doesn't already exist
                            if (artifactCustomProperty == null)
                            {
                                artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Document, attachmentId, customProperties);
                            }
                            else
                            {
                                artifactCustomProperty.StartTracking();
                            }

                            //Need to set the original date of this record to match the concurrency date
                            if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                            {
                                DateTime concurrencyDateTimeValue;
                                if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                                {
                                    attachment.ConcurrencyDate = concurrencyDateTimeValue;
                                    attachment.AcceptChanges();
                                }
                            }
                            attachment.StartTracking();

                            //Update the field values in both entities
                            List<string> fieldsToIgnore = new List<string>();
                            fieldsToIgnore.Add("UploadDate");
                            fieldsToIgnore.Add("EditedDate");
                            UpdateFields(validationMessages, dataItem, attachment, customProperties, artifactCustomProperty, projectId, attachmentId, 0, fieldsToIgnore);
                            UpdateFields(validationMessages, dataItem, projectAttachment, customProperties, null, projectId, attachmentId, 0, fieldsToIgnore);

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
                                //Extract changes for use in notifications
                                Dictionary<string, object> changes1 = projectAttachment.ExtractChanges();
                                Dictionary<string, object> changes2 = attachment.ExtractChanges();
                                ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

                                //Persist to database, catching any business exceptions and displaying them
                                try
                                {
                                    attachmentManager.Update(projectAttachment, userId);
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
                                    projectAttachment.ApplyChanges(changes1);
                                    attachment.ApplyChanges(changes2);

                                    new NotificationManager().SendNotificationForArtifact(projectAttachment, notificationCust, null, attachment);
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for " + projectAttachment.ArtifactToken);
                                }
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

        /// <summary>Returns a list of documents in the system for the specific user/project</summary>
        /// <param name="userId">The user we're viewing the documents as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">Any standard filters that need to be set (used when retrieving for a specific artifact)</param>
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
            //We can have limited permissions if an artifact is specified, so we check that later
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                AttachmentManager attachmentManager = new AttachmentManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Create the array of data items (including the first filter item)
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;

                //Now get the list of populated filters and the current sort
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST);
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "EditedDate DESC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");
                sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Document);

                //Create the filter item first - we can clone it later
                SortedDataItem filterItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
                dataItems.Add(filterItem);

                //See if we have standard filters specified and if they match retrieving by artifact rather than by folder
                //Also see if we're being asked to display for a single folder (or all folders)
                int artifactId = -1;
                AttachmentManager.DisplayMode displayMode = AttachmentManager.DisplayMode.CurrentFolder;
                DataModel.Artifact.ArtifactTypeEnum artifactType = DataModel.Artifact.ArtifactTypeEnum.None;
                //default the folder to the projects default (root) folder
                int? folderId = new AttachmentManager().GetDefaultProjectFolder(projectId);
                if (standardFilters != null)
                {
                    if (standardFilters.ContainsKey("ArtifactId") && standardFilters.ContainsKey("ArtifactType"))
                    {
                        artifactId = (int)GlobalFunctions.DeSerializeValue(standardFilters["ArtifactId"]);
                        artifactType = (DataModel.Artifact.ArtifactTypeEnum)GlobalFunctions.DeSerializeValue(standardFilters["ArtifactType"]);
                    }
                    if (standardFilters.ContainsKey("DisplayMode"))
                    {
                        int displayModeId = (int)GlobalFunctions.DeSerializeValue(standardFilters["DisplayMode"]);
                        displayMode = (AttachmentManager.DisplayMode)displayModeId;
                    }
                    //See if we have the folder id passed through as a filter
                    if (standardFilters.ContainsKey(GlobalFunctions.SPECIAL_FILTER_FOLDER_ID))
                    {
                        int? provisionalFolderId = (int)GlobalFunctions.DeSerializeValue(standardFilters[GlobalFunctions.SPECIAL_FILTER_FOLDER_ID]);
                        if (provisionalFolderId.HasValue && provisionalFolderId.Value > 0 && attachmentManager.ProjectAttachmentFolder_Exists(projectId, provisionalFolderId.Value))
                        {
                            folderId = provisionalFolderId.Value;
                        }
                    }
                }
                else
                {
                    //See if we have a folder to filter on, not applied if we have a standard filter
                    //because those screens don't display the folders on the left-hand side

                    //-1 = no filter
                    //0 = root folder
                    int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                    if (selectedNodeId > 1)
                    {
                        if (attachmentManager.ProjectAttachmentFolder_Exists(projectId, selectedNodeId))
                        {
                            //Filter by specific Folder
                            folderId = selectedNodeId;
                        }
                        else
                        {
                            //Set to the Root Folder (i.e. no folder items only) and update the projectsetting
                            folderId = attachmentManager.GetDefaultProjectFolder(projectId);
                            SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                        }
                    }
                }

                //If no artifact is specified, we do need full permissions
                if (artifactId < 1 && authorizationState == Project.AuthorizationState.Limited)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //If we're being asked for all folders, null the folder-id
                if (displayMode == AttachmentManager.DisplayMode.AllItems)
                {
                    folderId = null;
                }

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS);
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
                //Get the number of documents in the project or artifact
                int artifactCount;
                List<ProjectAttachmentView> projectAttachments = null;
                List<ProjectAttachmentView> sourceCodeDocuments = null;
                if (artifactType == DataModel.Artifact.ArtifactTypeEnum.None || artifactId == -1)
                {
                    artifactCount = attachmentManager.CountForProject(projectId, folderId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }
                else
                {
                    artifactCount = attachmentManager.CountByArtifactId(projectId, artifactId, artifactType, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                    //If we are displaying attachments during test execution, we need to also get the attachments associated with
                    //the test case and test step as well as the test run, so we need to merge those in
                    if (standardFilters != null)
                    {
                        foreach (KeyValuePair<string, string> standardFilter in standardFilters)
                        {
                            int prefixLength = "AdditionalArtifact_".Length;
                            if (SafeSubstring(standardFilter.Key, prefixLength) == "AdditionalArtifact_")
                            {
                                int additionalArtifactTypeId = Int32.Parse(standardFilter.Key.Substring(prefixLength, standardFilter.Key.Length - prefixLength));
                                int additionalArtifactId = (int)GlobalFunctions.DeSerializeValue(standardFilter.Value);
                                int additionalArtifactCount = attachmentManager.CountByArtifactId(projectId, additionalArtifactId, (DataModel.Artifact.ArtifactTypeEnum)additionalArtifactTypeId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                                artifactCount += additionalArtifactCount;
                            }
                        }
                    }

                    //If we are including source code documents, need to add them to the list
                    //Also make sure the current product license supports source code
					//PCS
                    if ((Common.License.LicenseProductName == LicenseProductNameEnum.ValidationMaster) && Common.Global.SourceCode_IncludeInAssociationsAndDocuments)
                    {
                        try
                        {
                            SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                            List<SourceCodeFile> sourceCodeFiles = sourceCodeManager.RetrieveFilesForArtifact(artifactType, artifactId, filterList);
                            sourceCodeDocuments = new List<ProjectAttachmentView>();
                            foreach (SourceCodeFile sourceCodeFile in sourceCodeFiles)
                            {
                                ProjectAttachmentView sourceCodeProjectAttachment = new ProjectAttachmentView();
                                //We make the 'attachment id' negative to avoid collisions with 'real' attachments in the TST_ATTACHMENT table
                                sourceCodeProjectAttachment.AttachmentId = -sourceCodeFile.ArtifactSourceCodeId.Value;
                                sourceCodeProjectAttachment.ConcurrencyDate = sourceCodeFile.LastUpdateDate;
                                sourceCodeProjectAttachment.ProjectId = projectId;
                                sourceCodeProjectAttachment.Filename = sourceCodeFile.Name;
                                //Source code is in bytes, documents are in KB so need to convert
                                sourceCodeProjectAttachment.Size = (int)(Math.Ceiling((decimal)sourceCodeFile.Size / 1024M));
                                sourceCodeProjectAttachment.AttachmentTypeId = (int)Attachment.AttachmentTypeEnum.SourceCode;
                                sourceCodeProjectAttachment.AttachmentTypeName = Resources.Main.SiteMap_SourceCode;
                                sourceCodeProjectAttachment.DocumentTypeId = -1;
                                sourceCodeProjectAttachment.DocumentTypeName = Resources.Main.SiteMap_SourceCode;
                                sourceCodeProjectAttachment.ProjectAttachmentFolderId = -1;
                                sourceCodeProjectAttachment.AuthorId = 1;
                                sourceCodeProjectAttachment.AuthorName = sourceCodeFile.AuthorName;
                                sourceCodeProjectAttachment.EditorId = 1;
                                sourceCodeProjectAttachment.EditorName = sourceCodeFile.AuthorName;
                                sourceCodeProjectAttachment.CurrentVersion = sourceCodeFile.RevisionName;
                                sourceCodeProjectAttachment.EditedDate = sourceCodeFile.LastUpdateDate;
                                sourceCodeProjectAttachment.UploadDate = sourceCodeFile.LastUpdateDate;
                                //We use the tags as a temporary place to store the 'file key' that is used in the custom url
                                sourceCodeProjectAttachment.Tags = sourceCodeFile.FileKey;

                                sourceCodeDocuments.Add(sourceCodeProjectAttachment);
                            }
                            artifactCount += sourceCodeDocuments.Count;
                        }
                        catch (SourceCodeProviderLoadingException)
                        {
                            //Ignore this one and don't log
                        }
                        catch (Exception exception)
                        {
                            //Log and continue
                            Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                        }
                    }
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

                //**** Now we need to actually populate the rows of data to be returned ****
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (artifactType == DataModel.Artifact.ArtifactTypeEnum.None || artifactId == -1)
                {
                    projectAttachments = attachmentManager.RetrieveForProject(projectId, folderId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                    //When viewing for a project we also need to include the direct folders/subfolders
                    //as long as viewing for current folder
                    if (folderId != null)
                    {
                        List<ProjectAttachmentFolder> documentFolders = attachmentManager.RetrieveFoldersByParentId(projectId, folderId);//, sortProperty, sortAscending, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                        //Iterate through all the document folders and populate the data items
                        foreach (ProjectAttachmentFolder documentFolder in documentFolders)
                        {
                            //We clone the template item as the basis of all the new items
                            SortedDataItem dataItem = filterItem.Clone();

                            //Now populate with the data
                            PopulateRow(dataItem, documentFolder);
                            dataItems.Add(dataItem);
                        }
                    }
                }
                else
                {
                    projectAttachments = attachmentManager.RetrieveByArtifactId(projectId, artifactId, artifactType, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                    //If we are displaying attachments during test execution, we need to also get the attachments associated with
                    //the test case and test step as well as the test run, so we need to merge those in
                    bool needToResort = false;
                    if (standardFilters != null)
                    {
                        foreach (KeyValuePair<string, string> standardFilter in standardFilters)
                        {
                            int prefixLength = "AdditionalArtifact_".Length;
                            if (SafeSubstring(standardFilter.Key, prefixLength) == "AdditionalArtifact_")
                            {
                                int additionalArtifactTypeId = Int32.Parse(standardFilter.Key.Substring(prefixLength, standardFilter.Key.Length - prefixLength));
                                int additionalArtifactId = (int)GlobalFunctions.DeSerializeValue(standardFilter.Value);
                                List<ProjectAttachmentView> additionalProjectAttachments = attachmentManager.RetrieveByArtifactId(projectId, additionalArtifactId, (DataModel.Artifact.ArtifactTypeEnum)additionalArtifactTypeId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                                projectAttachments.AddRange(additionalProjectAttachments);
                                needToResort = true;
                            }
                        }
                    }

                    //See if we need to merge in any source code documents
                    if (sourceCodeDocuments != null && sourceCodeDocuments.Count > 0)
                    {
                        projectAttachments.AddRange(sourceCodeDocuments);
                        needToResort = true;
                    }

                    //Now we need to resort
                    if (needToResort)
                    {
                        GenericSorter<ProjectAttachmentView> sorter = new GenericSorter<ProjectAttachmentView>();
                        projectAttachments = sorter.Sort(projectAttachments, sortProperty, sortAscending).ToList();
                    }
                }

                //Display the pagination information
                sortedData.CurrPage = currentPage;
                sortedData.PageCount = pageCount;
                sortedData.StartRow = startRow;

                //Display the sort information
                sortedData.SortProperty = sortProperty;
                sortedData.SortAscending = sortAscending;

                //Now get the list of custom property options and lookup values for this artifact type / project
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Document, true, false, true);

                //Iterate through all the documents and populate the dataitem
                foreach (ProjectAttachmentView projectAttachment in projectAttachments)
                {
                    //We clone the template item as the basis of all the new items
                    SortedDataItem dataItem = filterItem.Clone();

                    //Now populate with the data
                    PopulateRow(dataItem, projectAttachment, customProperties, null, false, false);
                    dataItems.Add(dataItem);

                    //If displaying for an artifact, if we have a URL, need to change the URL to the actual URL
                    if (artifactType != DataModel.Artifact.ArtifactTypeEnum.None && artifactId != -1 && projectAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
                    {
                        dataItem.CustomUrl = GlobalFunctions.FormNavigatableUrl(projectAttachment.Filename);
                    }
                }

                //Display the visible and total count of artifacts
                sortedData.VisibleCount = projectAttachments.Count;
                sortedData.TotalCount = artifactCount;

                //Also include the pagination info
                sortedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

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
        /// Allows sorted lists with folders to focus on a specific item and open its containing folder
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="artifactId">Id of an attachment (or negative for a folder)</param>
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

            //Make sure we're authorized to view documents
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //See if we have a folder or attachment
                AttachmentManager attachmentManager = new AttachmentManager();
                if (artifactId > 0)
                {
                    int attachmentId = artifactId;

                    //Retrieve this document
                    ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId);
                    if (projectAttachment != null)
                    {
                        //Get the folder
                        int folderId = projectAttachment.ProjectAttachmentFolderId;

                        //Unset the current filters and then set the current folder to this one
                        bool isInitialFilter = false;
                        string result = base.UpdateFilters(userId, projectId, null, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.Document, out isInitialFilter);
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, folderId);
                        return folderId;
                    }
                }
                if (artifactId < 0)
                {
                    int projectAttachmentFolderId = -artifactId;

                    //Retrieve this document folder
                    ProjectAttachmentFolder projectAttachmentFolder = attachmentManager.RetrieveFolderById(projectAttachmentFolderId);
                    if (projectAttachmentFolder != null)
                    {
                        //Unset the current filters and then set the current folder to this one
                        if (clearFilters)
                        {
                            bool isInitialFilter = false;
                            string result = base.UpdateFilters(userId, projectId, null, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.Document, out isInitialFilter);
                        }
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, projectAttachmentFolderId);
                        return projectAttachmentFolderId;
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
            return base.UpdateSort(userId, projectId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS);
        }

        /// <summary>
        /// Deletes a set of documents
        /// </summary>
        /// <param name="items">The items to delete</param>
        /// <param name="projectId">The current project</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Iterate through all the items to be deleted
                AttachmentManager attachmentManager = new AttachmentManager();
                foreach (string itemValue in items)
                {
                    //Get the attachment id
                    int attachmentId = Int32.Parse(itemValue);
                    attachmentManager.Delete(projectId, attachmentId);
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
            //This is handled separately since it involves file uploads and a popup dialog box
            throw new NotImplementedException();
        }

        /// <summary>Exports a set of documents to another project</summary>
        /// <param name="items">The items to export</param>
        /// <param name="destProjectId">The project to export them to</param>
        public void SortedList_Export(int destProjectId, List<string> items)
        {
            const string METHOD_NAME = "SortedList_Export()";
            Logger.LogEnteringEvent(METHOD_NAME);

            try
            {
                //Iterate through all the items to be exported
                AttachmentManager attachmentManager = new AttachmentManager();
                foreach (string itemValue in items)
                {
                    //Get the attachment id
                    int attachmentId = Int32.Parse(itemValue);

                    //We just put the exported document into the default (root) foler and default document type
                    attachmentManager.Export(attachmentId, destProjectId);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
            Logger.LogExitingEvent(METHOD_NAME);
        }

        public void SortedList_Copy(int projectId, List<string> items)
        {
            //Not used since copying is not allowed
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles page-specific operations
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="operation">The operation being performed</param>
        /// <param name="parameters">The parameters being passed</param>
        /// <returns></returns>
        public override string CustomOperationEx(int projectId, string operation, JsonDictionaryOfStrings parameters)
        {
            const string METHOD_NAME = "CustomOperationEx";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Extract the common parameters
                int artifactTypeId = Int32.Parse(parameters["artifactTypeId"]);
                DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;
                int artifactId = Int32.Parse(parameters["artifactId"]);

                //See which operating is being executed
                if (operation == "AddSourceCodeFile")
                {
                    //Extract the specific parameters
                    string[] fileIds = parameters["fileIds"].Split(',');
                    string comment = parameters["comment"];

                    SourceCodeManager sourceCode = new SourceCodeManager(projectId);
                    string branchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);
                    foreach (string fileId in fileIds)
                    {
                        int fileIdValue = Int32.Parse(fileId);
                        string fileKey = sourceCode.GetFileKeyForId(fileIdValue, branchKey);
                        try
                        {
                            sourceCode.AddFileAssociation(projectId, fileKey, artifactType, artifactId, DateTime.UtcNow, null, comment);
                        }
                        catch (EntityConstraintViolationException)
                        {
                            //Ignore any attempts to link an already-linked document
                        }
                    }
                }
                if (operation == "AddExistingDocument")
                {
                    //Make sure we're authorized (limited is OK, we check that later)
                    //need modify permissions of the artifact to delete an association to attachments
                    Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);

                    //Handle special case of a pending test run - where create permissions allows the removal of an attachment association
                    if ((DataModel.Artifact.ArtifactTypeEnum)artifactTypeId == DataModel.Artifact.ArtifactTypeEnum.TestRun)
                    {
                        //Need to see if the user is doing test execution or reviewing a completed test run
                        TestRunManager testRunManager = new TestRunManager();
                        TestRun testRun = testRunManager.RetrieveById2(artifactId);

                        if (testRun.TestRunsPendingId != null)
                        {
                            //If on test execution create authorization is needed for test runs (not modify)
                            authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
                        }

                    }

                    //Exit method if no authorization
                    if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }


                    //Extract the specific parameters
                    string[] attachmentIds = parameters["attachmentIds"].Split(',');
                    AttachmentManager attachmentManager = new AttachmentManager();
                    foreach (string attachmentId in attachmentIds)
                    {
                        int attachmentIdValue = Int32.Parse(attachmentId);
                        try
                        {
                            attachmentManager.InsertArtifactAssociation(projectId, attachmentIdValue, artifactId, artifactType);
                        }
                        catch (EntityConstraintViolationException)
                        {
                            //Ignore any attempts to link an already-linked document
                        }
                    }
                }

                //Indicate success
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return "";
            }
            catch (SourceCodeProviderGeneralException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Sets the current filter to the passed in tag name
        /// </summary>
        /// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
        public new string CustomOperation(int projectId, string operation, string value)
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
                //See which operation we have
                if (operation == "FilterOnTags")
                {
                    //Get the current filters from session
                    ProjectSettingsCollection savedFilters = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST);
                    int oldFilterCount = savedFilters.Count;
                    savedFilters.Clear(); //Clear the filters

                    //Now apply the tag filter
                    savedFilters.Add("Tags", value);
                    savedFilters.Save();
                }

                //Indicate success
                return "";
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
        /// <param name="attachmentId">The id of the attachment to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        public string RetrieveNameDesc(int? projectId, int attachmentId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the attachment business object
                AttachmentManager attachmentManager = new AttachmentManager();

                //Now retrieve the specific attachment - handle quietly if it doesn't exist
                try
                {
                    string tooltip = "";
                    //See if we have a document or folder (or source code file)
                    if (attachmentId < 0)
                    {
                        //On the attachment tab, these are source code folders, otherwise they are folders
                        if (displayTypeId.HasValue && displayTypeId == (int)(int)DataModel.Artifact.DisplayTypeEnum.Attachments && projectId.HasValue)
                        {
                            int sourceCodeAssociationId = -attachmentId;
                            SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId.Value);
                            ArtifactSourceCodeFile artifactSourceCodeFile = sourceCodeManager.RetrieveFileAssociation2(projectId.Value, sourceCodeAssociationId);
                            if (artifactSourceCodeFile != null)
                            {
                                tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(artifactSourceCodeFile.FileKey) + "</u>";
                            }
                        }
                        else
                        {
                            //Document folder IDs are negative
                            int folderId = -attachmentId;

                            ProjectAttachmentFolder attachmentFolder = attachmentManager.RetrieveFolderById(folderId);

                            //See if we have any parent folders
                            List<ProjectAttachmentFolderHierarchyView> parentFolders = attachmentManager.RetrieveParentFolders(attachmentFolder.ProjectId, attachmentFolder.ProjectAttachmentFolderId, false);
                            foreach (ProjectAttachmentFolderHierarchyView parentFolder in parentFolders)
                            {
                                tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
                            }

                            tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(attachmentFolder.Name) + "</u>";
                        }
                    }
                    else
                    {
                        //Get the folder path and name/description
                        if (projectId.HasValue)
                        {
                            Attachment attachment = attachmentManager.RetrieveById(attachmentId);
                            if (projectId.HasValue)
                            {
                                ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId.Value, attachmentId);
                                List<ProjectAttachmentFolderHierarchyView> parentFolders = attachmentManager.RetrieveParentFolders(projectAttachment.ProjectId, projectAttachment.ProjectAttachmentFolderId, true);
                                foreach (ProjectAttachmentFolderHierarchyView parentFolder in parentFolders)
                                {
                                    tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
                                }
                            }
                            if (String.IsNullOrWhiteSpace(attachment.Description))
                            {
                                tooltip += Microsoft.Security.Application.Encoder.HtmlEncode(attachment.Filename);
                            }
                            else
                            {
                                tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(attachment.Filename) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(attachment.Description);
                            }
                        }
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the attachment, but it has already been deleted on the server
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
        /// Handles custom list operations used by the document list screen, specifically removing
        /// documents from being attached to artifacts
        /// </summary>
        /// <param name="operation">
        /// The operation being executed:
        ///     RemoveFromArtifact - removes the document from the artifact specified
        /// </param>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="destId">The destination item id</param>
        /// <param name="items">The list of source items</param>
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
                int operationPrefixLength = "RemoveFromArtifact:".Length;
                if (SafeSubstring(operation, operationPrefixLength) == "RemoveFromArtifact:")
                {
                    //Get the artifact type from the operation name
                    int artifactTypeId = Int32.Parse(operation.Substring(operationPrefixLength, operation.Length - operationPrefixLength));

                    //Make sure we're authorized (limited is OK, we check that later)
                    //need modify permissions of the artifact to delete an association to attachments
                    Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);

                    //Handle special case of a pending test run - where create permissions allows the removal of an attachment association
                    if ((DataModel.Artifact.ArtifactTypeEnum)artifactTypeId == DataModel.Artifact.ArtifactTypeEnum.TestRun)
                    {
                        //Need to see if the user is doing test execution or reviewing a completed test run
                        TestRunManager testRunManager = new TestRunManager();
                        TestRun testRun = testRunManager.RetrieveById2(destId);

                        if (testRun.TestRunsPendingId != null)
                        {
                            //If on test execution create authorization is needed for test runs (not modify)
                            authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
                        }

                    }

                    //Exit method if no authorization
                    if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }

                    //Make sure the user is authorized for this item if they only have limited permissions
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        int ownerId = -1;
                        int creatorId = -1;
                        ArtifactManager artifactManager = new ArtifactManager();
                        ArtifactInfo artifactInfo = artifactManager.RetrieveArtifactInfo((DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, destId, projectId);
                        if (artifactInfo.OwnerId.HasValue)
                        {
                            ownerId = artifactInfo.OwnerId.Value;
                        }
                        if (artifactInfo.CreatorId.HasValue)
                        {
                            creatorId = artifactInfo.CreatorId.Value;
                        }

                        if (ownerId != userId && creatorId != userId)
                        {
                            throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                        }
                    }

                    //Iterate through all the passed in documents and remove them from the artifact
                    AttachmentManager attachmentManager = new AttachmentManager();
                    foreach (string item in items)
                    {
                        int attachmentId = Int32.Parse(item);
                        if (attachmentId < 0)
                        {
                            //We have a source code file attachment not a real one
                            int artifactSourceCodeId = -attachmentId;
                            SourceCodeManager sourceCode = new SourceCodeManager();
                            sourceCode.RemoveFileAssociation(artifactSourceCodeId);
                        }
                        else
                        {

                            attachmentManager.Delete(projectId, attachmentId, destId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException("Operation '" + operation + "' is not currently supported");
                }

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
            if (!CurrentUserId.HasValue)
            {
                throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.Document, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, isShared, existingSavedFilterId, includeColumns);
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
            if (!CurrentUserId.HasValue)
            {
                throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Delegate to the generic implementation
            return base.RetrieveFilters(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.Document, includeShared);
        }

        #region Internal Functions

        /// <summary>
        /// Verifies the digital signature on a workflow status change if it is required
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="originalStatusId">The original status</param>
        /// <param name="currentStatusId">The new status</param>
        /// <param name="signature">The digital signature</param>
        /// <param name="creatorId">The creator of the document</param>
        /// <param name="ownerId">The owner of the document</param>
        /// <returns>True for a valid signature, Null if no signature required and False if invalid signature</returns>
        protected bool? VerifyDigitalSignature(int workflowId, int originalStatusId, int currentStatusId, Signature signature, int authorId, int editorId)
        {
            const string METHOD_NAME = "VerifyDigitalSignature";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                DocumentWorkflowManager workflowManager = new DocumentWorkflowManager();
                DocumentWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, originalStatusId, currentStatusId);
                if (workflowTransition == null)
                {
                    //No transition possible, so return failure
                    return false;
                }
                if (!workflowTransition.IsSignatureRequired)
                {
                    //No signature required, so return null
                    return null;
                }

                //Make sure we have a signature at this point
                if (signature == null)
                {
                    return false;
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
                if (workflowTransition.IsExecuteByAuthor && authorId == userId)
                {
                    isAllowed = true;
                }
                else if (workflowTransition.IsExecuteByEditor && editorId == userId)
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

        /// <summary>Populates the 'shape' of the data item that will be used as a template for the retrieved data items</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the documents as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        /// <param name="allFields">Do we want all fields or just the ones displayed in the list view</param>
        /// <param name="projectTemplateId">the id of the project template</param>
        protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool allFields = false)
        {
            //We need to dynamically add the various columns from the field list
            LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
            AddDynamicColumns(Artifact.ArtifactTypeEnum.Document, getLookupValues, projectId, projectTemplateId, userId, dataItem, filterList, null, !allFields);
        }

        /// <summary>
        /// Gets the list of lookup values and names for a specific lookup
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
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
                AttachmentManager attachmentManager = new AttachmentManager();
                Business.UserManager userManager = new Business.UserManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                if (lookupName == "EditorId" || lookupName == "AuthorId")
                {
                    List<DataModel.User> users = userManager.RetrieveActiveByProjectId(projectId);
                    lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
                }
                if (lookupName == "DocumentTypeId")
                {
                    List<DocumentType> attachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, true);
                    lookupValues = ConvertLookupValues(attachmentTypes.OfType<DataModel.Entity>().ToList(), "DocumentTypeId", "Name");
                }
                if (lookupName == "DocumentStatusId")
                {
                    List<DocumentStatus> statuses = attachmentManager.DocumentStatus_Retrieve(projectTemplateId);
                    lookupValues = ConvertLookupValues(statuses.OfType<DataModel.Entity>().ToList(), "DocumentStatusId", "Name");
                }

                //The custom property lookups
                int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
                if (customPropertyNumber.HasValue)
                {
                    CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Document, customPropertyNumber.Value, true);
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
		/// Updates the editable textual content of a document item (i.e. adds a new version)
		/// </summary>
		/// <param name="attachmentManager">The attachment manager</param>
		/// <param name="workflowFields">The workflow field states</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="dataItem">The data item</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="mimeType">The mimetype of the content</param>
		protected void UpdateEditableContent(int userId, int projectId, DataItem dataItem, AttachmentManager attachmentManager, List<DocumentWorkflowField> workflowFields, string mimeType)
        {
            //See if the content is editable at this status
            bool isEditable = !workflowFields.Any(w => w.ArtifactField.Name == "DocumentVersions" && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive);
			bool isRichText = mimeType == "text/html";
			bool isPlainText = IsMimeTypePlainText(mimeType);
			bool isData = IsMimeTypeData(mimeType);

			string fieldName = isRichText ? SPECIAL_FIELD_RICH_TEXT_CONTENT : isPlainText ? SPECIAL_FIELD_PLAIN_TEXT_CONTENT : isData ? SPECIAL_FIELD_DATA_CONTENT : "";

			if (isEditable && dataItem.Fields.ContainsKey(fieldName))
            {
                //Make sure the context has actually changed
                string text = dataItem.Fields[fieldName].TextValue;
                string existingText = GetTextContentOfAttachment(attachmentManager, dataItem.PrimaryKey);
                if (text.Trim() != existingText.Trim())
                {
                    //Get the key values
                    string filename = dataItem.Fields["Filename"].TextValue;
                    string description = dataItem.Fields["Description"].TextValue;

                    //Calculate next version of this content - must specifiy this is in en-US because that is what we use to save version numbers (by default)
                    string version = attachmentManager.RetrieveMaxVersionNumber(dataItem.PrimaryKey);
					NumberStyles style = NumberStyles.AllowDecimalPoint;
					CultureInfo culture = CultureInfo.InvariantCulture;
					double result;
                    if (Double.TryParse(version, style, culture, out result))
                    {
                        //Increment the version by a major number - convert to a US formatted string 
                        version = (result + 1.0).ToString("0.0", culture);
                    }

                    //Get the binary data version of the content
                    byte[] binaryData = UnicodeEncoding.UTF8.GetBytes(text);

                    attachmentManager.InsertVersion(
                        projectId,
                        dataItem.PrimaryKey,
                        filename,
                        description,
                        userId,
                        binaryData,
                        version,
                        true
                        );
                }
            }
        }

        /// <summary>
        /// Populates the editable textual content of a document item
        /// </summary>
        /// <param name="attachmentManager">The attachment manager</param>
        /// <param name="workflowFields">The workflow field states</param>
        /// <param name="dataItem">The data item</param>
        /// <param name="mimeType">String of the mimetype</param>
        protected void PopulateEditableContent(DataItem dataItem, AttachmentManager attachmentManager, List<DocumentWorkflowField> workflowFields, string mimeType)
        {
            //See if the content is editable at this status
            bool isEditable = !workflowFields.Any(w => w.ArtifactField.Name == "DocumentVersions" && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive);
			//See if we have rich text or data content (if not then it must be text)
			bool isRichText = mimeType == "text/html";
			bool isData = IsMimeTypeData(mimeType);
			bool isPlainText = !isRichText && !isData;

			//Get the textual version of the attachment
			string text = GetTextContentOfAttachment(attachmentManager, dataItem.PrimaryKey);

            //Add the new content text field, we populate either rich text or plain text,
            //but we need to always create both fields so that the page works correctly
            dataItem.Fields.Add(SPECIAL_FIELD_PLAIN_TEXT_CONTENT, new DataItemField()
            {
                FieldName = SPECIAL_FIELD_PLAIN_TEXT_CONTENT,
                TextValue = isPlainText ? text : "",
                Editable = isEditable
            });
            dataItem.Fields.Add(SPECIAL_FIELD_RICH_TEXT_CONTENT, new DataItemField()
            {
                FieldName = SPECIAL_FIELD_RICH_TEXT_CONTENT,
                TextValue = isRichText ? text : "",
                Editable = isEditable
            });
			dataItem.Fields.Add(SPECIAL_FIELD_DATA_CONTENT, new DataItemField()
			{
				FieldName = SPECIAL_FIELD_DATA_CONTENT,
				TextValue = isData ? text : "",
				Editable = isEditable
			});
		}

        protected string GetTextContentOfAttachment(AttachmentManager attachmentManager, int attachmentId)
        {
            //Open the file
            byte[] binaryData;
            using (FileStream fileStream = attachmentManager.OpenById(attachmentId))
            {
                //Read the file in.
                binaryData = new byte[fileStream.Length];
                fileStream.Read(binaryData, 0, (int)fileStream.Length);
                fileStream.Close();
            }

            //Convert into UTF8 text
            string text = "";
            try
            {
                text = UnicodeEncoding.UTF8.GetString(binaryData);
            }
            catch (Exception)
            {
            }

            return text;
        }

        /// <summary>
        /// Populates a data item from the entity
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="documentFolder">The entity containing the data</param>
        protected void PopulateRow(SortedDataItem dataItem, ProjectAttachmentFolder documentFolder)
        {
            //Set the primary key (negative for folders)
            dataItem.PrimaryKey = -documentFolder.ProjectAttachmentFolderId;
            dataItem.Folder = true;

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (documentFolder.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the entity
                    PopulateFieldRow(dataItem, dataItemField, documentFolder, null, null, false, null);
                }

                if (dataItemField.FieldName == "Filename")
                {
                    //The folder icon
                    dataItemField.Tooltip = "Folder.svg";
                }
            }
        }

        /// <summary>
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="editable">Does the data need to be editable</param>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="projectAttachmentView">The entity containing the data</param>
        /// <param name="artifactCustomProperty">The artifact custom properties if available</param>
        /// <param name="customProperties">The custom property definitions</param>
        /// <param name="allFields">Do we want all fields or just what we need to display for the list view</param>
        protected void PopulateRow(SortedDataItem dataItem, ProjectAttachmentView projectAttachmentView, List<CustomProperty> customProperties, ArtifactCustomProperty artifactCustomProperty, bool editable, bool allFields, List<DocumentWorkflowField> workflowFields = null, List<DocumentWorkflowCustomProperty> workflowCustomProps = null)
        {
            //Set the primary key
            dataItem.PrimaryKey = projectAttachmentView.AttachmentId;
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, projectAttachmentView.ConcurrencyDate);

            //If this is a source code file, so need to pass a custom url
            if (projectAttachmentView.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.SourceCode)
            {
                string fileKey = projectAttachmentView.Tags;    //The file key is stored as a fake tag
                string url = "~/SourceCodeFileDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectAttachmentView.ProjectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + fileKey;
                dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(url);
            }
			//If this is a data mimetype then pass in a custom url to ALWAYS open the document view (in the attachment panel it will otherwise download the attachment which we do not want)
			else if (IsMimeTypeData(GlobalFunctions.GetFileTypeInformation(projectAttachmentView.Filename)["mimetype"]))
			{
				string url = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, projectAttachmentView.ProjectId, projectAttachmentView.ArtifactId));
				dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(url);
			}

            //We use this to specify if it's a file attachment or not
            dataItem.Attachment = (projectAttachmentView.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.File);

            //Convert the workflow lists into the type expected by the ListServiceBase function
            List<WorkflowField> workflowFields2 = DocumentWorkflowManager.ConvertFields(workflowFields);
            List<WorkflowCustomProperty> workflowCustomProps2 = DocumentWorkflowManager.ConvertFields(workflowCustomProps);

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                //The filetype field if requested is calculated and not from the entity
                if (fieldName == "Filetype")
                {
                    //Need to handle URLs separately (always displayed as HTML docs)
                    if (projectAttachmentView.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
                    {
                        dataItemField.TextValue = "Filetypes/Link.svg";
                        dataItemField.Tooltip = "HTML";
                    }
                    else
                    {
                        dataItemField.TextValue = "Filetypes/" + GlobalFunctions.GetFileTypeImage(projectAttachmentView.Filename);
                        dataItemField.Tooltip = GlobalFunctions.GetFileTypeInformation(projectAttachmentView.Filename)["description"];
                    }
                }
                else if (projectAttachmentView.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the entity
                    PopulateFieldRow(dataItem, dataItemField, projectAttachmentView, customProperties, artifactCustomProperty, editable, null, workflowFields2, workflowCustomProps2);

                    //Specify which fields are editable or not
                    if (fieldName == "UploadDate" ||
                        fieldName == "Size" ||
                        fieldName == "EditedDate" ||
                        fieldName == "CurrentVersion")
                    {
                        dataItemField.Editable = false;
                    }

                    //Certain fields do not allow null values
                    if (fieldName == "AuthorId" ||
                        fieldName == "DocumentTypeId" ||
                        fieldName == "EditorId")
                    {
                        dataItemField.Required = true;
                    }

                    //If we have size, need to explicitly set it's display name as KB
                    if (dataItemField.FieldName == "Size")
                    {
                        dataItemField.TextValue = dataItemField.IntValue.DisplayFileSizeKB();
                    }

                    //If we have the name/desc field then we need to set the image to the appropriate filetype
                    //which is passed in the tooltip field unless it's being used for an individual item lookuo
                    //in which case we need to pass the long description
                    if (dataItemField.FieldName == "Filename")
                    {
                        if (allFields)
                        {
                            if (String.IsNullOrWhiteSpace(projectAttachmentView.Description))
                            {
                                dataItemField.Tooltip = "";
                            }
                            else
                            {
                                dataItemField.Tooltip = projectAttachmentView.Description;
                            }
                        }
                        else
                        {
                            //Need to handle URLs separately (always displayed as HTML docs)
                            if (projectAttachmentView.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
                            {
                                dataItemField.Tooltip = "Filetypes/Link.svg";
                            }
                            else
                            {
                                dataItemField.Tooltip = "Filetypes/" + GlobalFunctions.GetFileTypeImage(dataItemField.TextValue);
                            }
                        }
                    }
                }
            }
        }

		/// <summary>
		/// Returns a true based on a mimetype to tell if a mimetype should be editable
		/// </summary>
		/// <param name="mimeType">The mimeType</param>
		protected bool IsMimeTypeEditable(string mimeType)
		{
			bool isEditable = false;
			if (mimeType.Contains("text/") || 
				mimeType.Equals("application/json") ||
				mimeType.Equals("application/xml") ||
				mimeType.Equals("application/x-diagram") ||
				mimeType.Equals("application/x-orgchart") ||
				mimeType.Equals("application/x-mindmap") ||
				mimeType.Equals("application/x-bat")
			)
			{
				isEditable = true;
			}
			return isEditable;
		}

		/// <summary>
		/// Returns a true based on a mimetype to tell if a mimetype is in the data format
		/// </summary>
		/// <param name="mimeType">The mimeType</param>
		protected bool IsMimeTypeData(string mimeType)
		{
			bool isData = false;
			if (mimeType.Equals("application/x-diagram") ||
				mimeType.Equals("application/x-orgchart") ||
				mimeType.Equals("application/x-mindmap")
			)
			{
				isData = true;
			}
			return isData;
		}

		/// <summary>
		/// Returns a true based on a mimetype to tell if a mimetype is a plain text format
		/// </summary>
		/// <param name="mimeType">The mimeType</param>
		protected bool IsMimeTypePlainText(string mimeType)
		{
			bool isPlainText = false;
			// is an application mimetpye that is really plain text
			if (mimeType.Equals("application/json") ||
				mimeType.Equals("application/xml") ||
				mimeType.Equals("application/x-bat") ||
				// or is a text mimetype that is NOT html
				(mimeType.Contains("text/") && mimeType != "text/html")
			)
			{
				isPlainText = true;
			}
			return isPlainText;
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
        /// Returns a list of documents for display in the navigation bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="indentLevel">Not used for documents since not hierarchical</param>
        /// <returns>List of documents</returns>
        /// <param name="displayMode">
        /// The display mode of the navigation list:
        /// 1 = Filtered List
        /// 2 = All Items (no filters)
        /// </param>
        /// <param name="containerId">not used</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business object
                AttachmentManager attachmentManager = new AttachmentManager();

                //Create the array of data items
                List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "EditedDate DESC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Also need to get the currently selected folder
                //default to the root folder
                int? folderId = null;
                int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (selectedNodeId < 1)
                {
                    //Root Folder (i.e. no folder documents only)
                    folderId = attachmentManager.GetDefaultProjectFolder(projectId);
                }
                else
                {
                    //Filter by specific Folder
                    folderId = selectedNodeId;
                }

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS);
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
                //Get the number of documents in the project
                int artifactCount = attachmentManager.CountForProject(projectId, folderId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //**** Now we need to actually populate the rows of data to be returned ****

                //Get the attachments list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }
                List<ProjectAttachmentView> attachments;
                if (displayMode == 2)
                {
                    //All Items
                    attachments = attachmentManager.RetrieveForProject(projectId, folderId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }
                else
                {
                    //Filtered List
                    attachments = attachmentManager.RetrieveForProject(projectId, folderId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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

                //Iterate through all the documents and populate the dataitem (only some columns are needed)
                string documentIndentLevel = "AAA";
                foreach (ProjectAttachmentView attachment in attachments)
                {
                    //Create the data-item
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();

                    //Populate the necessary fields
                    dataItem.PrimaryKey = attachment.AttachmentId;
                    dataItem.Indent = documentIndentLevel;
                    dataItem.Expanded = false;

                    //Name/Desc
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = attachment.Filename;
                    dataItem.Summary = false;
                    dataItem.Alternate = false;
                    dataItem.Fields.Add("Name", dataItemField);

                    //Need to handle URLs separately (always displayed as HTML docs)
                    if (attachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
                    {
                        dataItemField.Tooltip = "Images/Filetypes/Link.svg";
                    }
                    else
                    {
                        dataItemField.Tooltip = "Images/Filetypes/" + GlobalFunctions.GetFileTypeImage(dataItemField.TextValue);
                    }

                    //Add to the items collection
                    dataItems.Add(dataItem);

                    //Increment the indent level
                    documentIndentLevel = HierarchicalList.IncrementIndentLevel(documentIndentLevel);
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
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS);
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
        /// Retrieves the list of workflow operations for the current document
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="artifactId">The id of the document</param>
        /// <param name="typeId">The document type</param>
        /// <returns>The list of available workflow operations</returns>
        /// <remarks>Pass a specific type id if the user has changed the type of the document, but not saved it yet.</remarks>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Create the array of data items to store the workflow operations
                List<DataItem> dataItems = new List<DataItem>();

                //Get the list of available transitions for the current step in the workflow
                AttachmentManager attachmentManager = new AttachmentManager();
                DocumentWorkflowManager workflowManager = new DocumentWorkflowManager();
                ProjectAttachmentView projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, artifactId);
                int workflowId;
                if (typeId.HasValue)
                {
                    workflowId = workflowManager.Workflow_GetForDocumentType(typeId.Value);
                }
                else
                {
                    workflowId = workflowManager.Workflow_GetForDocumentType(projectAttachmentView.DocumentTypeId);
                }

                //Get the current user's role
                int projectRoleId = (SpiraContext.Current.ProjectRoleId.HasValue) ? SpiraContext.Current.ProjectRoleId.Value : -1;

                //Determine if the current user is the author or editor of the document
                bool isAuthor = false;
                if (projectAttachmentView.AuthorId == CurrentUserId.Value)
                {
                    isAuthor = true;
                }
                bool isOwner = false;
                if (projectAttachmentView.EditorId == CurrentUserId.Value)
                {
                    isOwner = true;
                }
                int statusId = projectAttachmentView.DocumentStatusId;
                List<DocumentWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusId, projectRoleId, isAuthor, isOwner);

                //Populate the data items list
                foreach (DocumentWorkflowTransition workflowTransition in workflowTransitions)
                {
                    //The data item itself
                    DataItem dataItem = new DataItem();
                    dataItem.PrimaryKey = (int)workflowTransition.WorkflowTransitionId;
                    dataItems.Add(dataItem);

                    //The WorkflowId field
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "WorkflowId";
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
                    dataItemField.IntValue = (int)workflowTransition.DocumentWorkflowId;
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
                    dataItemField.IntValue = (int)workflowTransition.InputDocumentStatusId;
                    dataItemField.TextValue = workflowTransition.InputDocumentStatus.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The OutputStatusId field
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "OutputStatusId";
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
                    dataItemField.IntValue = (int)workflowTransition.OutputDocumentStatusId;
                    dataItemField.TextValue = workflowTransition.OutputDocumentStatus.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The OutputStatusOpenYn field
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "OutputStatusOpenYn";
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
                    dataItemField.TextValue = (workflowTransition.OutputDocumentStatus.IsOpenStatus) ? "Y" : "N";
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The SignatureYn field (does it need a signature)
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "SignatureYn";
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
                    dataItemField.TextValue = (workflowTransition.IsSignatureRequired) ? "Y" : "N";
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

        #region IFormService Methods

        /// <summary>
        /// Deletes the current document and returns the ID of the item to redirect to (if any)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Load the current attachment so that we can get the folder id
                Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
                ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId, artifactId);

                int folderId = projectAttachment.ProjectAttachmentFolderId;

                //First we need to determine which attachment id to redirect the user to after the delete
                int? newAttachmentId = null;

                //Look through the current dataset to see what is the next attachment in the list
                //If we are the last one on the list then we need to simply use the one before

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "EditedDate DESC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS);
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
                //Get the number of documents in the project
                int artifactCount = attachmentManager.CountForProject(projectId, folderId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //Get the project document dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }

                List<ProjectAttachmentView> projectAttachments = attachmentManager.RetrieveForProject(projectId, folderId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                bool matchFound = false;
                int previousAttachmentId = -1;
                foreach (ProjectAttachmentView projectAttachmentView in projectAttachments)
                {
                    int testAttachmentId = projectAttachmentView.AttachmentId;
                    if (testAttachmentId == artifactId)
                    {
                        matchFound = true;
                    }
                    else
                    {
                        //If we found a match on the previous iteration, then we want to this (next) document
                        if (matchFound)
                        {
                            newAttachmentId = testAttachmentId;
                            break;
                        }

                        //If this matches the current incident, set flag
                        if (testAttachmentId == artifactId)
                        {
                            matchFound = true;
                        }
                        if (!matchFound)
                        {
                            previousAttachmentId = testAttachmentId;
                        }
                    }
                }
                if (!newAttachmentId.HasValue && previousAttachmentId != -1)
                {
                    newAttachmentId = previousAttachmentId;
                }

                //Next we need to delete the current attachment/document from the project
                attachmentManager.Delete(projectId, artifactId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return newAttachmentId;
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

        /// <summary>Returns a single attachment data record (all columns) for use by the FormManager control</summary>
        /// <param name="artifactId">The id of the current attachment</param>
        /// <returns>An attachment data item</returns>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business classes
                AttachmentManager attachmentManager = new AttachmentManager();
                DocumentWorkflowManager workflowManager = new DocumentWorkflowManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, true);

                //Need to add the empty column to capture any new comments added
                if (!dataItem.Fields.ContainsKey("NewComment"))
                {
                    dataItem.Fields.Add("NewComment", new DataItemField() { FieldName = "NewComment", Required = false, Editable = true, Hidden = false });
                }

                //Get the attachment for the specific attachment id
                ProjectAttachmentView projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, artifactId.Value);

                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, DataModel.Artifact.ArtifactTypeEnum.Document, true);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && projectAttachmentView.EditorId != userId && projectAttachmentView.AuthorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Get the list of workflow fields and custom properties
                int workflowId = workflowManager.Workflow_GetForDocumentType(projectAttachmentView.DocumentTypeId);

                int statusId = projectAttachmentView.DocumentStatusId;
                List<DocumentWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, statusId);
                List<DocumentWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, statusId);

                //See if we have any existing artifact custom properties for this row
                if (artifactCustomProperty == null)
                {
                    List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Document, true, false);
                    PopulateRow(dataItem, projectAttachmentView, customProperties, (ArtifactCustomProperty)null, true, true, workflowFields, workflowCustomProps);
                }
                else
                {
                    PopulateRow(dataItem, projectAttachmentView, artifactCustomProperty.CustomPropertyDefinitions, artifactCustomProperty, true, true, workflowFields, workflowCustomProps);
                }

                //See if we have any fields that are not part of the normal 'shape'
                if (artifactId.HasValue)
                {
                    //Populate the folder path as a special field
                    List<ProjectAttachmentFolderHierarchyView> parentFolders = attachmentManager.RetrieveParentFolders(projectAttachmentView.ProjectId, projectAttachmentView.ProjectAttachmentFolderId, true);
                    string pathArray = "[";
                    bool isFirst = true;
                    foreach (ProjectAttachmentFolderHierarchyView parentFolder in parentFolders)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            pathArray += ",";
                        }
                        pathArray += "{ \"name\": \"" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "\", \"id\": " + parentFolder.ProjectAttachmentFolderId + " }";
                    }
                    pathArray += "]";
                    dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath", TextValue = pathArray });

                    //Also we need to populate some additional fields used to display the file type, etc.
                    dataItem.Fields.Add("_AttachmentTypeId", new DataItemField() { FieldName = "_AttachmentTypeId", IntValue = projectAttachmentView.AttachmentTypeId });
					if (projectAttachmentView.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.File)
					{
						string mimeType = GlobalFunctions.GetFileMimeType(projectAttachmentView.Filename);
						dataItem.Fields.Add("_MimeType", new DataItemField() { FieldName = "_MimeType", TextValue = mimeType });

						//If we have textual content it can be edited inline, so we need to provide it in the special 'Content' field
						if (IsMimeTypeEditable(mimeType))
                        {
							PopulateEditableContent(dataItem, attachmentManager, workflowFields, mimeType);
                        }
                    }

                    //Also need to return back a special field to denote if the user is the owner or creator of the artifact
                    bool isArtifactCreatorOrOwner = (projectAttachmentView.EditorId == userId || projectAttachmentView.AuthorId == userId);
                    dataItem.Fields.Add("_IsArtifactCreatorOrOwner", new DataItemField() { FieldName = "_IsArtifactCreatorOrOwner", TextValue = isArtifactCreatorOrOwner.ToDatabaseSerialization() });
                }
                else
                {
                    //send a blank folder path object back so client knows this artifact has folders
                    dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath" });
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

        /// <summary>Saves a single attachment data item</summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="dataItem">The attachment to save</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Get the attachment id
            int attachmentId = dataItem.PrimaryKey;

            try
            {
                //Instantiate the business classes
                AttachmentManager attachmentManager = new AttachmentManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                DocumentWorkflowManager workflowManager = new DocumentWorkflowManager();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Load the custom property definitions (once, not per artifact)
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Document, false);

                //This service only supports updates, so we should get an attachment id that is valid

                //Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
                ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId);
                if (projectAttachment != null)
                {
                    projectAttachment.StartTracking();
                    Attachment attachment = projectAttachment.Attachment;
                    if (attachment != null)
                    {
                        //Make sure the user is authorized for this item if they only have limited permissions
                        if (authorizationState == Project.AuthorizationState.Limited && attachment.EditorId != userId && attachment.AuthorId != userId)
                        {
                            throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                        }

                        //Create a new artifact custom property row if one doesn't already exist
                        ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, attachmentId, DataModel.Artifact.ArtifactTypeEnum.Document, false, customProperties);
                        if (artifactCustomProperty == null)
                        {
                            artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Document, attachmentId, customProperties);
                        }
                        else
                        {
                            artifactCustomProperty.StartTracking();
                        }

						//For saving, need to use the current status and type of the dataItem which may be different to the one retrieved
						int currentStatusId = (dataItem.Fields["DocumentStatusId"].IntValue.HasValue) ? dataItem.Fields["DocumentStatusId"].IntValue.Value : -1;
						int originalStatusId = attachment.DocumentStatusId;
						int documentTypeId = (dataItem.Fields["DocumentTypeId"].IntValue.HasValue) ? dataItem.Fields["DocumentTypeId"].IntValue.Value : -1;

						//Get the list of workflow fields and custom properties
						int workflowId;
						if (documentTypeId < 1)
						{
							workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).DocumentWorkflowId;
						}
						else
						{
							workflowId = workflowManager.Workflow_GetForDocumentType(documentTypeId);
						}
						List<DocumentWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, currentStatusId);
						List<DocumentWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, currentStatusId);

						//Convert the workflow lists into the type expected by the ListServiceBase function
						List<WorkflowField> workflowFields2 = DocumentWorkflowManager.ConvertFields(workflowFields);
						List<WorkflowCustomProperty> workflowCustomProps2 = DocumentWorkflowManager.ConvertFields(workflowCustomProps);

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
									shouldVerifyDigitalSignature = attachment.ConcurrencyDate == concurrencyDateTimeValue;
								}
							}

							if (shouldVerifyDigitalSignature)
							{
								bool? valid = VerifyDigitalSignature(workflowId, originalStatusId, currentStatusId, signature, attachment.AuthorId, attachment.EditorId);
								if (valid.HasValue)
								{
									if (valid.Value)
									{
										//Add the meaning to the artifact so that it can be recorded
										attachment.SignatureMeaning = signature.Meaning;
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
                                attachment.ConcurrencyDate = concurrencyDateTimeValue;
                                attachment.AcceptChanges();
                            }
                        }

                        //Extract changes for use in notifications
                        Dictionary<string, object> changes1 = projectAttachment.ExtractChanges();
                        Dictionary<string, object> changes2 = attachment.ExtractChanges();

                        //Clone custom props for use in notifications
                        ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

                        //Now we can start tracking any changes
                        attachment.StartTracking();

                        //Update the field values, tracking changes
                        List<string> fieldsToIgnore = new List<string>();
                        fieldsToIgnore.Add("UploadDate");
                        fieldsToIgnore.Add("EditedDate");
                        fieldsToIgnore.Add("Comments");
                        fieldsToIgnore.Add("NewComment");
                        fieldsToIgnore.Add("CurrentVersion");

                        //Update the field values
                        UpdateFields(validationMessages, dataItem, attachment, customProperties, artifactCustomProperty, projectId, attachmentId, 0, fieldsToIgnore, workflowFields2, workflowCustomProps2);

                        //Check to see if a comment was required and if so, verify it was provided. It's not handled as part of 'UpdateFields'
                        //because there is no Comments field on the Document entity
                        if (workflowFields != null && workflowFields.Any(w => w.ArtifactField.Name == "Comments" && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
                        {
                            //Comment is required, so check that it's present
                            if (String.IsNullOrWhiteSpace(dataItem.Fields["NewComment"].TextValue))
                            {
                                AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = "NewComment", Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, Resources.Fields.Comment) });
                            }
                        }

                        //Check to see if a new version was required and if so, verify it was provided. It's not handled as part of 'UpdateFields'
                        //because there is no Version field on the Document entity
                        if (workflowFields != null && workflowFields.Any(w => w.ArtifactField.Name == "DocumentVersions" && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
                        {
							//Get most recent version uploaded to this document
							AttachmentVersionView attachmentVersion = attachmentManager.RetrieveVersionLastUploaded(attachmentId);
                            ProjectAttachmentView attachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId);


                            //New Version is required, so check that the last one provided was by the current user and uploaded recently
                            DateTime tooLongAgo = DateTime.UtcNow.AddHours(-1);
                            bool uploadedByUser = attachmentVersion.AuthorId == userId;
                            bool uploadedBeforeLastUpdate = attachmentVersion.UploadDate < attachmentView.EditedDate;
                            bool uploadedTooLongAgo = attachmentVersion.UploadDate < tooLongAgo;

							//If the document is editable on the page, and has been edited as part of this form_save operation, then we can use this as the new required version
							bool newEditedVersion = false;
							bool canLiveEdit = false;

							if (attachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.File)
							{
								string mimeType = GlobalFunctions.GetFileMimeType(attachment.Filename);
								if (IsMimeTypeEditable(mimeType))
								{
									canLiveEdit = true;

									//See if we have plain or rich text content (different fields)
									string fieldName = (mimeType == "text/html") ? SPECIAL_FIELD_RICH_TEXT_CONTENT : SPECIAL_FIELD_PLAIN_TEXT_CONTENT;
									//Check if the editable content has changed or not
									string text = dataItem.Fields[fieldName].TextValue;
									string existingText = GetTextContentOfAttachment(attachmentManager, dataItem.PrimaryKey);
									if (text.Trim() != existingText.Trim())
									{
										newEditedVersion = true;
									}
								}
							}

							//If the file can be edited and was can proceed as normal (the update/save at the end of the method will create the new version)
							if (canLiveEdit && newEditedVersion)
							{
								//No action
							}
							//Otherwise, if no version recently uploaded, break out with message
							else if (!uploadedByUser || uploadedBeforeLastUpdate || uploadedTooLongAgo)
                            {
								string message = canLiveEdit ? Resources.Messages.ListServiceBase_UploadOrEditRequired : Resources.Messages.ListServiceBase_UploadRequired;
								AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = "", Message = String.Format(message, Resources.Fields.Version) });
                            }
                        }

                        //Update the field values
                        UpdateFields(validationMessages, dataItem, attachment, customProperties, artifactCustomProperty, projectId, attachmentId, 0, fieldsToIgnore);
                        UpdateFields(validationMessages, dataItem, projectAttachment, customProperties, null, projectId, attachmentId, 0, fieldsToIgnore);

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

                        //Update the document and any custom properties
                        try
                        {
                            attachmentManager.Update(projectAttachment, userId);
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

                        //See if we have a new comment encoded in the list of fields
                        string notificationComment = null;
                        if (dataItem.Fields.ContainsKey("NewComment"))
                        {
                            string newComment = dataItem.Fields["NewComment"].TextValue;

                            if (!String.IsNullOrWhiteSpace(newComment))
                            {
                                new DiscussionManager().Insert(userId, attachmentId, Artifact.ArtifactTypeEnum.Document, newComment, DateTime.UtcNow, projectId, false, false);
                                notificationComment = newComment;
                            }
                        }

                        //Call notifications..
                        try
                        {
                            projectAttachment.ApplyChanges(changes1);
                            attachment.ApplyChanges(changes2);
                            projectAttachment.MarkAsModified();
                            attachment.MarkAsModified();
                            new NotificationManager().SendNotificationForArtifact(projectAttachment, notificationCust, null, attachment);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for " + projectAttachment.ArtifactToken);
                        }

                        //If we have editable text content, see if that was changed, which would result in a new version
                        if (attachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.File)
                        {
                            string mimeType = GlobalFunctions.GetFileMimeType(attachment.Filename);

                            //If we have textual content it can be edited inline, so we need to update it here,
                            //which will cause a new version to be added
                            if (IsMimeTypeEditable(mimeType))
                            {
                                UpdateEditableContent(userId, projectId, dataItem, attachmentManager, workflowFields, mimeType);
                            }
                        }
                    }
                }

                //Return back any messages. For success it should only contain a new artifact ID if we're inserting
                return validationMessages;
            }
            catch (ArtifactNotExistsException)
            {
                //Let the user know that the attachment no longer exists
                return CreateSimpleValidationMessage(Resources.Messages.DocumentDetails_DocumentNotExists);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Not implemented as we don't have workflows for documents currently
        /// </summary>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
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
                List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Document);
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Document, false);

                //Get the list of workflow fields and custom properties for the specified type and step
                DocumentWorkflowManager workflowManager = new DocumentWorkflowManager();
                int workflowId = workflowManager.Workflow_GetForDocumentType(typeId);
                List<DocumentWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, stepId);
                List<DocumentWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, stepId);

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

        #region IItemSelectorService Methods

        /// <summary>
        /// Reytrieves the list of existing documents (no folders)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="standardFilters"></param>
        /// <returns></returns>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business objects
                AttachmentManager attachmentManager = new AttachmentManager();

                //Create the array of data items (including the first filter item)
                ItemSelectorData itemSelectorData = new ItemSelectorData();
                List<DataItem> dataItems = itemSelectorData.Items;

                //Also need to get the currently selected folder
                //default to the root folder
                int? folderId = null;
                int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (selectedNodeId < 1)
                {
                    //Root Folder (i.e. no folder docs only)
                    folderId = attachmentManager.GetDefaultProjectFolder(projectId);
                }
                else
                {
                    //Filter by specific Folder
                    folderId = selectedNodeId;
                }

                //Get the document list for the folder
                List<ProjectAttachmentView> projectAttachments = attachmentManager.RetrieveForProject(projectId, folderId, "Filename", true, 1, Int32.MaxValue, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //Iterate through all the test cases and populate the dataitem (only some columns are needed)
                foreach (ProjectAttachmentView projectAttachment in projectAttachments)
                {
                    //Create the data-item
                    DataItem dataItem = new DataItem();

                    //Populate the necessary fields
                    dataItem.PrimaryKey = projectAttachment.AttachmentId;

                    //Attachment Id
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Id";
                    dataItemField.IntValue = projectAttachment.AttachmentId;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Filename
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Filename";
                    dataItemField.TextValue = projectAttachment.Filename;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Icon
                    //Need to handle URLs separately (always displayed as HTML docs)
                    if (projectAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
                    {
                        dataItemField.Tooltip = "Filetypes/Link.svg";
                    }
                    else
                    {
                        dataItemField.Tooltip = "Filetypes/" + GlobalFunctions.GetFileTypeImage(dataItemField.TextValue);
                    }

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
    }
}
