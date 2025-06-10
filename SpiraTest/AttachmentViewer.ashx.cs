using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Text;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>This handler writes out the attachment binary stream with the appropriate MIME encoding</summary>
	public class AttachmentViewer : LocalizedHttpHandler
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.AttachmentViewer::";
		private const int BYTES_READATONCE = 2097152;

		public override bool IsReusable
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// This method generates the streamable image from the passed-in XML
		/// </summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		public override void ProcessRequest(HttpContext context)
		{
			//First call the base functionality
			base.ProcessRequest(context);

			const string METHOD_NAME = "ProcessRequest";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Increase the timeout so that we can render large attachments
                context.Server.ScriptTimeout = 600; // 10 minutes

                //Error number, for support debugging.
                int intErrorNum = -1;

                //Get the possible variables here.
                int intAttachVerId = -1;
                int intAttachId = -1;
                int intProjId = -1;
                if (!string.IsNullOrWhiteSpace(context.Request.QueryString[GlobalFunctions.PARAMETER_ATTACHMENT_VERSION_ID]) && context.Request.QueryString[GlobalFunctions.PARAMETER_ATTACHMENT_VERSION_ID].All<char>(char.IsNumber))
                    intAttachVerId = int.Parse(context.Request.QueryString[GlobalFunctions.PARAMETER_ATTACHMENT_VERSION_ID]);
                if (!string.IsNullOrWhiteSpace(context.Request.QueryString[GlobalFunctions.PARAMETER_ATTACHMENT_ID]) && context.Request.QueryString[GlobalFunctions.PARAMETER_ATTACHMENT_ID].All<char>(char.IsNumber))
                    intAttachId = int.Parse(context.Request.QueryString[GlobalFunctions.PARAMETER_ATTACHMENT_ID]);
                if (!string.IsNullOrWhiteSpace(context.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]) && context.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID].All<char>(char.IsNumber))
                    intProjId = int.Parse(context.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]);
                if (intProjId == -1)
                    intProjId = ProjectId;

                if (CurrentUserId.HasValue) //Verify they're a user.
                {
                    Attachment attachment = null;
                    AttachmentVersion attachmentVersion = null;
                    if (intProjId > -1) //Verify they are logged into or reference a project.
                    {
                        //Verify they have access to this project.
                        ProjectUserView projectUser = new ProjectManager().RetrieveUserMembershipById(intProjId, CurrentUserId.Value);

                        //Verify they belong to that project.
                        if (projectUser != null)
                        {
                            AttachmentManager attachmentManager = new AttachmentManager();
                            if (intAttachId > -1)
                            {
                                attachment = attachmentManager.RetrieveForProjectById(intProjId, intAttachId).Attachment;
                            }
                            else if (intAttachVerId > -1)
                            {
                                attachmentVersion = attachmentManager.RetrieveVersionById(intAttachVerId);
                                attachment = attachmentManager.RetrieveForProjectById(intProjId, attachmentVersion.AttachmentId).Attachment;

                                //In this case need to also verify that the project is allowed
                            }
                            else
                            {
                                intErrorNum = -1004;
                            }
                        }
                        else
                        {
                            intErrorNum = -1003;
                        }
                    }
                    else
                    {
                        intErrorNum = -1002;
                    }

                    //Now report to the user..
                    if (attachment != null)
                    {
                        //Forward to a URL if necessary.
                        if (attachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
                        {
                            context.Response.Redirect(GlobalFunctions.FormNavigatableUrl(attachment.Filename), true);
                            return;
                        }
                        else
                        {
                            //Set the MIME type and 'filename' for the attachment
                            context.Response.Buffer = false;
                            context.Response.ContentType = GlobalFunctions.GetFileMimeType(attachment.Filename);
                            context.Response.AddHeader("Content-Disposition", "inline; filename=\"" + attachment.Filename + "\"");
                            context.Response.Charset = "UTF-8"; //Only used for text

                            //Open up the attachment file for binary reading
                            //See if we have a specific version or just the latest version to open
                            FileStream fileStream = null;
                            if (attachmentVersion == null)
                            {
                                fileStream = new AttachmentManager().OpenById(attachment.AttachmentId);
                            }
                            else
                            {
                                fileStream = new AttachmentManager().OpenByVersionId(attachmentVersion.AttachmentVersionId);
                            }

                            if (fileStream != null)
                            {
                                //Add length header..
                                context.Response.AddHeader("Content-Length", fileStream.Length.ToString());

                                int numBytesReadPass = BYTES_READATONCE;
                                while (numBytesReadPass == BYTES_READATONCE)
                                {
                                    //Figure the size of our buffer..
                                    int numToGet = 0;
                                    if (fileStream.Length - fileStream.Position > BYTES_READATONCE)
                                        numToGet = BYTES_READATONCE;
                                    else
                                        numToGet = (int)(fileStream.Length - fileStream.Position);

                                    //Read the file in.
                                    byte[] byteArray = new byte[numToGet]; //2Mb groups.
                                    numBytesReadPass = fileStream.Read(byteArray, 0, numToGet);

                                    //Actually write out the attachment binary
                                    context.Response.BinaryWrite(byteArray);
                                }

                                //Exit the function.
                                return;
                            }
                            else
                            {
                                intErrorNum = -1006;
                            }
                        }
                    }
                    else
                    {
                        intErrorNum = -1005;
                    }
                }
                else
                {
                    intErrorNum = -1001;
                }

                //If we get this far, display an error.
                context.Response.ContentType = "text/plain";
                context.Response.Write(String.Format(Resources.Main.Attachment_NotExistError, intErrorNum));

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (ArtifactNotExistsException)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write(String.Format(Resources.Main.Attachment_NotExistError, -1007));
            }
            catch (HttpException)
            {
                //Happens if the user disconnects during download, don't log
                //as it clogs up the event log
            }
            catch (Exception exception)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write(String.Format(Resources.Main.Attachment_NotExistError, exception.Message));

                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
            }
		}
	}
}