using System;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using System.Web;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// Simply redirects to the appropriate source code revision or file, converting the transient
    /// session ids into the permanent tokens that can be used as permanent URLs
    /// </summary>
    public partial class SourceCodeRedirect : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.SourceCodeRedirect::";

        /// <summary>
        /// Called when the page is first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //See if we have a file session id in the querystring
                if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_SESSION_ID]))
                {
                    int fileId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_SESSION_ID]);

                    //Get the matching real file key from the source code cached settings
                    SourceCodeManager sourceCode = new SourceCodeManager(this.ProjectId);
                    string currentBranchKey = SourceCodeManager.Get_UserSelectedBranch(UserId, ProjectId);
                    string fileKey = sourceCode.GetFileKeyForId(fileId, currentBranchKey);

                    //See if we have any other items to add
                    string urlSuffix = "";
                    //See if we have a revision key to also add to the URL
                    if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY]))
                    {
                        string revisionKey = Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY];
                        urlSuffix += "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + HttpUtility.UrlEncode(revisionKey);
                    }

                    //See if we have a branch key to also add to the URL
                    if (!String.IsNullOrEmpty(currentBranchKey))
                    {
                        urlSuffix += "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY + "=" + HttpUtility.UrlEncode(currentBranchKey);
                    }

                    //Now redirect to the real URL
                    Response.Redirect("SourceCodeFileDetails.aspx?" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + HttpUtility.UrlEncode(fileKey) + "&" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + ProjectId + urlSuffix, true);
                }

                //See if we have a revision session id in the querystring
                if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_SESSION_ID]))
                {
                    int revisionId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_SESSION_ID]);

                    //Get the matching real revision key from the source code cached settings
                    SourceCodeManager sourceCode = new SourceCodeManager(this.ProjectId);
                    string currentBranchKey = SourceCodeManager.Get_UserSelectedBranch(UserId, ProjectId);
                    string revisionKey = sourceCode.GetRevisionKeyForId(revisionId);

                    //See if we have any other items to add
                    string urlSuffix = "";

                    //See if we have a file key to also add to the URL
                    if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY]))
                    {
                        string fileKey = Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY];
                        urlSuffix += "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + HttpUtility.UrlEncode(fileKey);
                    }

                    //See if we have a specific tab to display
                    if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_VIEW_FILES]))
                    {
                        urlSuffix += "&" + GlobalFunctions.PARAMETER_VIEW_FILES + "=" + Request.QueryString[GlobalFunctions.PARAMETER_VIEW_FILES];
                    }
                    if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_VIEW_ASSOCIATIONSSC]))
                    {
                        urlSuffix += "&" + GlobalFunctions.PARAMETER_VIEW_ASSOCIATIONSSC + "=" + Request.QueryString[GlobalFunctions.PARAMETER_VIEW_ASSOCIATIONSSC];
                    }

                    //See if we have a branch key to also add to the URL
                    if (!String.IsNullOrEmpty(currentBranchKey))
                    {
                        urlSuffix += "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY + "=" + HttpUtility.UrlEncode(currentBranchKey);
                    }

                    //Finally perform the redirect
                    Response.Redirect("SourceCodeRevisionDetails.aspx?" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + HttpUtility.UrlEncode(revisionKey) + "&" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + ProjectId + urlSuffix, true);
                }

                //See if we have an artifact source code link id in the querystring
                if (!String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_ARTIFACT_LINK_ID]))
                {
                    int artifactSourceCodeId;
                    if (Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_ARTIFACT_LINK_ID], out artifactSourceCodeId))
                    {
                        try
                        {
                            SourceCodeManager sourceCode = new SourceCodeManager(ProjectId);
                            string fileKey = sourceCode.GetFileKeyForAssociation(artifactSourceCodeId);

                            //Now redirect to the real URL
                            Response.Redirect("SourceCodeFileDetails.aspx?" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + HttpUtility.UrlEncode(fileKey) + "&" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + ProjectId + "&" + GlobalFunctions.PARAMETER_VIEW_ASSOCIATIONSSC + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE, true);
                        }
                        catch (ArtifactNotExistsException)
                        {
                            //Just redirect to the Project Home with the message
                            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + HttpUtility.UrlEncode(Resources.Messages.SourceCodeRedirect_CacheInvalid), true);
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (SourceCodeCacheInvalidException)
            {
                //Redirect with the appropriate error messsage
                string errorMessage = Resources.Messages.SourceCodeList_CacheNotReadyError;
                Response.Redirect("~/SourceCodeList.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + errorMessage, true);
            }
        }
    }
}
