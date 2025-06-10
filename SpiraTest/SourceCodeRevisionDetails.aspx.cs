using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Data;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// details of a particular document and handling updates
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.SourceCodeRevisions, null, "Commits/#commit-details", "SourceCodeRevisionDetails_Title")]
    public partial class SourceCodeRevisionDetails : SourceCodePage
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.SourceCodeRevisionDetails::";
        
		protected string revisionKey = "";
        private bool editingEnabled = true;

		#region Properties

		/// <summary>
		/// Returns whether editing is allowed, accessed by controls on the web form
		/// </summary>
		protected bool EditingEnabled
        {
            get
            {
                return this.editingEnabled;
            }
        }

        #endregion

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Capture the passed revision key from the querystring
            this.revisionKey = (Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY]).Trim();

            //We need to make sure that the system is licensed for source code viewing use
            if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidLicense, true);
            }

            //We need to determine what permissions the user has
            Business.ProjectManager projectManager = new Business.ProjectManager();
            ProjectRole projectRole = projectManager.RetrieveRolePermissions(ProjectRoleId);

            if (projectRole == null)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidRole, true);
            }
            else
            {
                //See what role we have and enable the appropriate operations

                //View Source Code
                if (!projectRole.IsSourceCodeView)
                {
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidRole, true);
                }

                //Edit Source Code
                if (!projectRole.IsSourceCodeEdit)
                {
                    this.editingEnabled = false;
                }
            }

            //Populate the user and project id in the grid controls
            this.grdSourceCodeFileList.ProjectId = this.ProjectId;

            //Set the base url on the build link
            this.lnkBuild.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Builds, ProjectId, -2);

            //Reset the error message
            this.lblMessage.Text = "";

			//Load the data
			LoadAndBindData(revisionKey);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

         /// <summary>
        /// Returns the url to redirect back to the general list of revisions
        /// </summary>
        protected string ReturnToRevisionListUrl
        {
            get
            {
                return "~/" + ProjectId + "/SourceCodeRevision/List.aspx";
            }
        }

        /// <summary>
        /// Loads and binds the source code revision details
        /// </summary>
        /// <param name="revisionKey">The revision key</param>
        private void LoadAndBindData(string revisionKey)
        {
            //Make sure that the current project is set to the project associated with the revision
            //this is important since the page may get loaded from an email notification URL
            //This project id is passed in the querystring
            int artifactProjectId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]);
            VerifyArtifactProject(artifactProjectId, UrlRoots.NavigationLinkEnum.SourceCode, 0);

            //Set the header URLs
            //Either redirect back to the general list of revisions, or the one for a specific file key
            this.navRevisionList.ProjectId = this.ProjectId;
            this.navRevisionList.ListScreenUrl = ReturnToRevisionListUrl;
            this.navRevisionList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.navRevisionList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

            this.btnBack.NavigateUrl = ReturnToRevisionListUrl;

            try
            {
                //First retrieve the passed in source code branches and current revision
                SourceCodeManager sourceCodeManager = new SourceCodeManager(this.ProjectId);
				string currentBranchKey = SourceCodeManager.Get_UserSelectedBranch(UserId, ProjectId);

				//See if we have a branch specified in the URL, otherwise use settings
				if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY]))
				{
					string branchName = Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY];
					if (branchName != currentBranchKey)
					{
						//Update the branch as long as the new one exists
						SourceCodeBranch passedInBranch = sourceCodeManager.RetrieveBranches().FirstOrDefault(b => b.BranchKey == branchName);
						if (passedInBranch != null)
						{
							currentBranchKey = passedInBranch.BranchKey;
							SourceCodeManager.Set_UserSelectedBranch(UserId, ProjectId, currentBranchKey);
						}
					}
				}

                if (String.IsNullOrEmpty(currentBranchKey))
                {
                    currentBranchKey = sourceCodeManager.RetrieveBranches().FirstOrDefault(b => b.IsDefault).BranchKey;
                }
                this.lblBranchName.Text = currentBranchKey;

                //Set the current revision for the files list
                Dictionary<string, object> standardFilters = new Dictionary<string, object>();
                standardFilters.Add("RevisionKey", revisionKey);
                standardFilters.Add("BranchKey", currentBranchKey);
                this.grdSourceCodeFileList.SetFilters(standardFilters);

                SourceCodeCommit sourceCodeRevision = null;
                try
                {
                    sourceCodeRevision = sourceCodeManager.RetrieveRevisionByKey(this.revisionKey);
                }
                catch (SourceCodeProviderArtifactPermissionDeniedException)
                {
                    //If the user doesn't have permission to view, let them know nicely
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SourceCode, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeRevisionDetails_NotAuthorizedForRevision, true);
                }
                catch (ArtifactNotExistsException)
                {
                    //If the artifact doesn't exist let the user know nicely
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SourceCode, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeRevisionDetails_RevisionNotExistsInBranch, true);
                }

                if (sourceCodeRevision == null || sourceCodeRevision.RevisionId < 0)
                {
                    //If the artifact doesn't exist let the user know nicely
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SourceCode, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeRevisionDetails_RevisionNotExistsInBranch, true);
                }

                //If the revision does not exist in the current branch, need to also redirect
                if (!sourceCodeManager.DoesRevisionExistInBranch(currentBranchKey, revisionKey))
                {
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SourceCode, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeRevisionDetails_RevisionNotExistsInBranch, true);
                }

                //Set the initial project/artifact context for the form manager
                this.ajxFormManager.ProjectId = ProjectId;
                this.ajxFormManager.PrimaryKey = sourceCodeRevision.RevisionId;

                //Set the navigation bar selected item
                this.navRevisionList.SelectedItemId = sourceCodeRevision.RevisionId;

                //Register client event handlers
                Dictionary<string, string> handlers = new Dictionary<string, string>();
                handlers.Add("loaded", "ajxFormManager_loaded_sourceCode");
                this.ajxFormManager.SetClientEventHandlers(handlers);

                //Configure the association panel
                this.tstAssociationPanel.MessageLabelHandle = this.lblMessage;
                this.tstAssociationPanel.ArtifactId = sourceCodeRevision.RevisionId;
                this.tstAssociationPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.SourceCodeRevision;
                this.tstAssociationPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.SourceCodeRevision_Associations;
            }
            catch (SourceCodeProviderAuthenticationException)
            {
                this.lblMessage.Text = Resources.Messages.SourceCodeList_UnableToAuthenticate;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeFileList.Visible = false;
            }
            catch (SourceCodeProviderLoadingException)
            {
                this.lblMessage.Text = Resources.Messages.SourceCodeList_UnableToLoadProvider;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeFileList.Visible = false;
            }
            catch (SourceCodeProviderGeneralException)
            {
                this.lblMessage.Text = Resources.Messages.SourceCodeList_GeneralError;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeFileList.Visible = false;
            }
            catch (SourceCodeCacheInvalidException)
            {
                this.lblMessage.Text = Resources.Messages.SourceCodeList_CacheNotReadyError;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeFileList.Visible = false;
            }
        }
    }
}
