using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;

using Inflectra.SpiraTest.DataModel;
using System.Threading;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// Redirects the user to the document repository associated with a Rapise test file
    /// </summary>
    public class RepositoryRedirect : LocalizedHttpHandler
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.RepositoryRedirect::";

        /// <summary>
        /// Redirects to the Document Details page for the appropriate Rapise test file
        /// </summary>
        /// <param name="context"></param>
        public override void ProcessRequest(HttpContext context)
        {
            base.ProcessRequest(context);

            const string METHOD_NAME = "ProcessRequest";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Get the repository path from the querystring
                if (String.IsNullOrWhiteSpace(context.Request.QueryString[GlobalFunctions.PARAMETER_REPOSITORY_PATH]))
                {
                    //Redirect to Project Home if we aren't given a path
                    context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId), true);
                }
                string repositoryPath = context.Request.QueryString[GlobalFunctions.PARAMETER_REPOSITORY_PATH].Trim();

                //Now we need to get the folder associated with the path (format = Folder\Test.sstext)
                string[] elements = repositoryPath.Split('\\');
                if (elements.Length < 2)
                {
                    //Redirect to Project Home if we aren't given a valid path
                    context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId), true);
                }

                string documentFolderName = elements[elements.Length-2];
                string documentFileName = elements[elements.Length - 1];

                //Locate the appropriate document folder
                Business.AttachmentManager attachmentManager = new AttachmentManager();
                List<ProjectAttachmentFolderHierarchy> attachmentFolders = attachmentManager.RetrieveFoldersByProjectId(ProjectId);
                int documentId = -1;
                foreach (ProjectAttachmentFolderHierarchy attachmentFolder in attachmentFolders)
                {
                    if (attachmentFolder.Name == documentFolderName)
                    {
                        //Now get the documents in this folder
                        List<ProjectAttachmentView> projectAttachments = attachmentManager.RetrieveForProject(ProjectId, attachmentFolder.ProjectAttachmentFolderId, "Filename", true, 1, Int32.MaxValue, null, 0);
                        foreach (ProjectAttachmentView projectAttachment in projectAttachments)
                        {
                            if (projectAttachment.Filename == documentFileName)
                            {
                                documentId = projectAttachment.AttachmentId;
                                break;
                            }
                        }
                    }
                    if (documentId != -1)
                    {
                        break;
                    }
                }
                if (documentId == -1)
                {
                    //Redirect to Project Home if we aren't given a valid path
                    context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId), true);
                }
                else
                {
                    //Redirect to the appropriate document
                    context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId, documentId), true);
                }
            }
            catch (ThreadAbortException)
            {
                //Don't log thread aborted exceptions
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
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