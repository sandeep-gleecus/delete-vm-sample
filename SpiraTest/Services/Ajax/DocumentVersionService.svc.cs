using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.IO;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>Provides the web service used to interacting with the various client-side document management AJAX components</summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class DocumentVersionService : AjaxWebServiceBase, IDocumentVersionService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.DocumentVersionService::";

        /// <summary>
        /// Counts how many versions are attached to the document
        /// </summary>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="attachmentId">The ID of the attachment</param>
        /// <returns>The count of versions</returns>
        public int DocumentVersion_CountVersions(int projectId, int attachmentId)
        {
            const string METHOD_NAME = CLASS_NAME + "DocumentVersion_CountVersions";
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

                //Get the attachment for the specific attachment id
                Attachment attachment = attachmentManager.RetrieveById(attachmentId);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && attachment.EditorId != userId && attachment.AuthorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Now count the document versions
                int attachmentVersionsCount = attachmentManager.CountVersions(attachmentId);

                Logger.LogExitingEvent(METHOD_NAME);
                return attachmentVersionsCount;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Makes the specified document version the active one
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="attachmentVersionId">The id of the attachment version</param>
        public void DocumentVersion_MakeActive(int projectId, int attachmentVersionId)
        {
            const string METHOD_NAME = CLASS_NAME + "DocumentVersion_MakeActive";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited modify or full modify)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business classes
                AttachmentManager attachmentManager = new AttachmentManager();

                //Get the attachment for the specific attachment version id
                Attachment attachment = attachmentManager.RetrieveVersionById(attachmentVersionId).Attachment;

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && attachment.EditorId != userId && attachment.AuthorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Make the specified version the active one
                attachmentManager.SetCurrentVersion(projectId, attachment.AttachmentId, attachmentVersionId, userId);


                Logger.LogExitingEvent(METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Uploads a new document version (files only)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="attachmentId">The id of the document</param>
        /// <param name="filename">The filename of this version</param>
        /// <param name="description">A description of this version</param>
        /// <param name="version">The version name</param>
        /// <param name="encodedData">The file data (base-64 encoded)</param>
        /// <param name="makeActive">Should we make this the active version</param>
        /// <returns>The id of the new version</returns>
        public int DocumentVersion_UploadFile(int projectId, int attachmentId, string filename, string description, string version, string encodedData, bool makeActive)
        {
            const string METHOD_NAME = CLASS_NAME + "DocumentVersion_UploadFile";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited modify or full modify)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business classes
                AttachmentManager attachmentManager = new AttachmentManager();

                //Get the attachment for the specific attachment version id
                Attachment attachment = attachmentManager.RetrieveById(attachmentId);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && attachment.EditorId != userId && attachment.AuthorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

			    //Get the description from the form
                if (!String.IsNullOrEmpty(description))
                {
			        description = GlobalFunctions.HtmlScrubInput(description);
                }

                //Make sure we have uploaded file data
                if (String.IsNullOrEmpty(encodedData))
                {
                    throw new DataValidationException(Resources.Messages.FileUploadDialog_EmptyFileUploaded);
                }

			    //Get the filename from the provided path
			    string filenameWithoutPath = Path.GetFileName(filename);

                //Base64-decode the attachment data

                //Strip off the date:image/xxx;base64, section
                int index = encodedData.IndexOf("base64,");
                if (index == -1)
                {
                    throw new DataValidationException(Resources.Messages.FileUploadDialog_EmptyFileUploaded);
                }
                string base64data = encodedData.Substring(index + "base64,".Length);

                //Convert into bytes
                byte[] binaryData = Convert.FromBase64String(base64data);

			    //Upload the new attachment version
			    int attachmentVersionId = attachmentManager.InsertVersion(projectId, attachmentId, filename, description, userId, binaryData, version, makeActive);

                Logger.LogExitingEvent(METHOD_NAME);
                return attachmentVersionId;
            }
            catch (DataValidationException)
            {
                throw;
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
            catch (Exception exception)
            {
                //Another error occurred
                throw new DataValidationException(String.Format(Resources.Messages.FileUploadDialog_UploadErrorGeneral, exception.Message));
            }
        }


        /// <summary>
        /// Deletes the specified document version
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="attachmentVersionId">The id of the attachment version</param>
        /// <remarks>You cannot delete the active version</remarks>
        public void DocumentVersion_Delete(int projectId, int attachmentVersionId)
        {
            const string METHOD_NAME = CLASS_NAME + "DocumentVersion_Delete";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited modify or full modify)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Document);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business classes
                AttachmentManager attachmentManager = new AttachmentManager();

                //Get the attachment for the specific attachment version id
                Attachment attachment = attachmentManager.RetrieveVersionById(attachmentVersionId).Attachment;

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && attachment.EditorId != userId && attachment.AuthorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }
                attachmentManager.DeleteVersion(projectId, attachmentVersionId, userId);

                Logger.LogExitingEvent(METHOD_NAME);
            }
            catch (AttachmentDefaultVersionException)
            {
                //You can't delete the current version, so let the user know
                throw new DataValidationException (Resources.Messages.DocumentDetails_CannotDeleteActiveVersion);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of versions for a specific document
        /// </summary>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="attachmentId">The ID of the attachment we want the versions for</param>
        /// <returns></returns>
        public List<DataItem> DocumentVersion_RetrieveVersions(int projectId, int attachmentId)
        {
            const string METHOD_NAME = CLASS_NAME + "DocumentVersion_RetrieveVersions";
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

                //Get the attachment for the specific attachment id
                Attachment attachment = attachmentManager.RetrieveById(attachmentId);

                //Make sure the user is authorized for this item
                if (authorizationState == Project.AuthorizationState.Limited && attachment.EditorId != userId && attachment.AuthorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Now load in the various document versions
                List<AttachmentVersionView> attachmentVersions = attachmentManager.RetrieveVersions(attachmentId);

                //Convert to the needed data object
                List<DataItem> dataItems = new List<DataItem>();
                foreach (AttachmentVersionView attachmentVersion in attachmentVersions)
                {
                    DataItem dataItem = new DataItem();
                    dataItem.PrimaryKey = attachmentVersion.AttachmentVersionId;
                    DataItemField dataItemField;

                    //Filetype
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Filetype";
                    dataItemField.TextValue = "Filetypes/" + GlobalFunctions.GetFileTypeImage(attachmentVersion.Filename);
                    dataItemField.Tooltip = GlobalFunctions.GetFileTypeInformation(attachmentVersion.Filename)["description"];
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Filename
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Filename";
                    dataItemField.TextValue = attachmentVersion.Filename;
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Comments / Description
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Description";
                    dataItemField.TextValue = attachmentVersion.Description;
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Html;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Version Number
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "VersionNumber";
                    dataItemField.TextValue = attachmentVersion.VersionNumber;
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Size
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Size";
                    dataItemField.IntValue = attachmentVersion.Size;
                    dataItemField.TextValue = String.Format("{0:n00} KB", attachmentVersion.Size);
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Author
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "AuthorId";
                    dataItemField.IntValue = attachmentVersion.AuthorId;
                    dataItemField.TextValue = attachmentVersion.AuthorName;
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Lookup;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Upload Date
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "UploadDate";
                    dataItemField.DateValue = GlobalFunctions.LocalizeDate(attachmentVersion.UploadDate);
                    dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, attachmentVersion.UploadDate);
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.DateTime;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //IsCurrent
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "IsCurrent";
                    dataItemField.TextValue = (attachmentVersion.IsCurrent) ? "Y" : "N";
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Add the item
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(METHOD_NAME);
                return dataItems;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }
    }
}
