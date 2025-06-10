using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// Accepts screenshots posts and uploads into the system
    /// </summary>
    public class ScreenshotUpload : LocalizedHttpHandler
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ScreenshotUpload::";

        /// <summary>
        /// Processes the screenshot upload from ckEditor
        /// </summary>
        /// <param name="context">The HTTP context</param>
        public override void ProcessRequest(HttpContext context)
        {
            //First call the base functionality
            base.ProcessRequest(context);

            const string METHOD_NAME = CLASS_NAME + "ProcessRequest()";

            Logger.LogEnteringEvent(METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                //Forbidden
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                context.Response.End();
                return;
            }
            int userId = this.CurrentUserId.Value;

            //Get the project id, artifact type id and artifact id
            int projectId = -1;
            int artifactId = -1;
            int artifactTypeId = -1;
            if (!string.IsNullOrWhiteSpace(context.Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID]) && context.Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID].All<char>(char.IsNumber))
                artifactTypeId = int.Parse(context.Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID]);
            if (!string.IsNullOrWhiteSpace(context.Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_ID]) && context.Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_ID].All<char>(char.IsNumber))
                artifactId = int.Parse(context.Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_ID]);
            if (!string.IsNullOrWhiteSpace(context.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]) && context.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID].All<char>(char.IsNumber))
                projectId = int.Parse(context.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]);

            //Make sure we have at least a project id
            if (projectId < 0)
            {
                //Forbidden, project not valid
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                context.Response.End();
                return;
            }

            //Make sure we're authorized to upload a new document to this project
            ProjectManager projectManager = new Business.ProjectManager();
            ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, CurrentUserId.Value);
            if (projectUser == null)
            {
                //Forbidden
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                context.Response.End();
                return;
            }
            Project.AuthorizationState authorizationState = projectManager.IsAuthorized(projectUser.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.Create);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                //Forbidden
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                context.Response.End();
                return;
            }

            //Convert the artifact type
            Artifact.ArtifactTypeEnum artifactType = Artifact.ArtifactTypeEnum.None;
            if (artifactTypeId > 0)
            {
                artifactType = (Artifact.ArtifactTypeEnum)artifactTypeId;
            }

            //Set the response headers
            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.AddHeader("Cache-Control", "private, no-cache");

            // Check if the request contains multipart/form-data.
            if (!context.IsPostNotification && context.Request.Files.Count < 1)
            {
                Logger.LogErrorEvent(METHOD_NAME, null, "Not a post notification or file count < 1");
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.MethodNotAllowed;
                context.Response.StatusDescription = "Not a post notification or file count < 1";
                string errorJson = "{ 'uploaded': 0, 'error': { message: '" + GlobalFunctions.JSEncode("Not a post notification or file count < 1") + "' } }";
                context.Response.Write(errorJson);
                context.Response.End();
                return;
            }

            //Loop through each file and upload it..
            try
            {
                //Loop through the files
                string filename = "";
                string url = "";
                for (int i = 0; i < context.Request.Files.Count; i++)
                {
                    HttpPostedFile file = context.Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        int attachmentId = UploadFile(file, userId, projectId, artifactId, artifactType);

                        //Send back the correct URL to embed for this attachment as part of the
                        //HTTP OK response
                        string attachmentUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Attachment, projectId, attachmentId));

                        //If we have multiple files we only send back the first one
                        if (i == 0)
                        {
                            url = attachmentUrl;
                            filename = file.FileName;
                        }
                    }
                }

//              {
//                  "uploaded": 1,
//                  "fileName": "foo.jpg",
//                  "url": "/files/foo.jpg"
//              }

                string fileJson = "{ \"uploaded\": 1, \"fileName\": \"" + GlobalFunctions.JSEncode(filename) + "\", \"url\": \"" + GlobalFunctions.JSEncode(url) + "\" }";
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                context.Response.Write(fileJson);
            }
            catch (System.Exception e)
            {
                Logger.LogErrorEvent(METHOD_NAME, e, "Error parsing files from request.");
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                context.Response.StatusDescription = e.Message;
                string errorJson = "{ 'uploaded': 0, 'error': { message: '" + GlobalFunctions.JSEncode(e.Message) + "' } }";
                context.Response.Write(errorJson);
                context.Response.End();
            }
        }

        /// <summary>
        /// Uploads a file attachment if specified by the user
        /// </summary>
        protected int UploadFile(HttpPostedFile postedFile, int userId, int projectId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType)
        {
            const string IMAGE_FOLDER = "_Screenshots";
            const string DEFAULT_FILENAME = "image.png";

            //Get size of uploaded file
            int size = postedFile.ContentLength;

            //Allocate a buffer for reading of the file
            byte[] binaryData = new byte[size];

            //Extract the posted file into a byte array
            postedFile.InputStream.Read(binaryData, 0, size);

            //Get the filename from the provided path
            string filename = Path.GetFileName(postedFile.FileName);

            //If the filename is 'image.png' give it a more meaningful name
            //Otherwise all images will have the same name
            if (filename == DEFAULT_FILENAME && artifactType != Artifact.ArtifactTypeEnum.None && artifactType != Artifact.ArtifactTypeEnum.Placeholder)
            {
                filename = GlobalFunctions.GetPrefixForArtifactType(artifactType) + "-" + artifactId + "-Screenshot.png";
            }

            //Get the tags, folder and type
            string tags = null;
            int? typeId = null;
            int? folderId = null;

            //We create a special "_Screenshots" folder if it doesn't exist
            AttachmentManager attachmentManager = new AttachmentManager();
            ProjectAttachmentFolder folder = attachmentManager.RetrieveFolderByName(projectId, IMAGE_FOLDER);
            if (folder == null)
            {
                int rootFolderId = attachmentManager.GetDefaultProjectFolder(projectId);
                folderId = attachmentManager.InsertProjectAttachmentFolder(projectId, IMAGE_FOLDER, rootFolderId);
            }
            else
            {
                folderId = folder.ProjectAttachmentFolderId;
            }

            //Call the business object to upload the file attachment (optionally attaching to an artifact if specified)
            int attachmentId = attachmentManager.Insert(
                projectId,
                filename,
                null,
                userId,
                binaryData,
                artifactId,
                artifactType,
                null,
                tags,
                typeId,
                folderId,
                null
                );

            //Send the notification as well
            attachmentManager.SendCreationNotification(projectId, attachmentId, null, null);

            return attachmentId;
        }

        public override bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}