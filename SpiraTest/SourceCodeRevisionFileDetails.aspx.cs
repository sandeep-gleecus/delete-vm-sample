using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Linq;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// details of a particular document and handling updates
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.SourceCodeRevisions, null, "Commits/#commit-file-details", "SourceCodeRevisionFileAction_Title")]
	public partial class SourceCodeRevisionFileDetails : SourceCodePage
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.SourceCodeRevisionFileDetails::";

		protected string fileKey;
		protected string revisionKey;
        protected string branchKey;
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

        protected string SourceCodeFileViewerUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl("~/SourceCodeFileViewer.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + ProjectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "={0}&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "={1}&" + GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY + "={2}");
            }
        }

        /// <summary>
        /// The diff mode ('unified' or 'split')
        /// </summary>
        protected string DiffMode
        {
            get;
            set;
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

            //We need to make sure that the system is licensed for source code viewing use
            if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidLicense, true);
            }

            //Make sure we have a file key and revision key specified
            if (String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY]) || String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY]))
            {
                Response.Redirect("~/" + ProjectId + "/SourceCode/List.aspx", true);
            }

            //Capture the passed file key from the querystring
            this.fileKey = (Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY]).Trim();
            this.revisionKey = (Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY]).Trim();

            //We need to determine what permissions the user has
            ProjectManager projectManager = new ProjectManager();
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

			//Reset the error message
			this.lblMessage.Text = "";

			//Load the data
			LoadAndBindData(fileKey);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Loads and binds the source code file details
		/// </summary>
		/// <param name="fileKey">The current file key</param>
		private void LoadAndBindData(string fileKey)
		{
			const string METHOD_NAME = "LoadAndBindData";

			//Make sure that the current project is set to the project associated with the file
			//this is important since the page may get loaded from an email notification URL
			//This project id is passed in the querystring
			int artifactProjectId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]);
			VerifyArtifactProject(artifactProjectId, UrlRoots.NavigationLinkEnum.SourceCode, 0);

			//Specify the context for the sidebars
			this.pnlSidebar.ProjectId = ProjectId;
			this.pnlSidebar.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
			this.pnlSidebar.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

            this.pnlSidebar2.ProjectId = ProjectId;
            this.pnlSidebar2.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlSidebar2.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

			//First retrieve the passed in source code file
			try
			{
				//See if we've been passed in a branch key
				this.branchKey = "";
				if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY]))
				{
					this.branchKey = Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY].Trim();
					SourceCodeManager.Set_UserSelectedBranch(UserId, ProjectId, branchKey);
				}

                //Get the current branch and load in the current file
				SourceCodeManager sourceCodeManager = new SourceCodeManager(this.ProjectId);
                string currentBranchKey = SourceCodeManager.Get_UserSelectedBranch(UserId, ProjectId);
                this.branchKey = currentBranchKey;
                this.lblBranchName.Text = currentBranchKey;
                SourceCodeFile sourceCodeFile = null;
				try
				{
					sourceCodeFile = sourceCodeManager.RetrieveFileByKey(this.fileKey, currentBranchKey);
				}
				catch (ArtifactNotExistsException)
				{
                    //If the artifact doesn't exist let the user know nicely
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SourceCode, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeFileDetails_FileNotExistsInBranch, true);
                }

                if (sourceCodeFile == null)
				{
                    //If the artifact doesn't exist let the user know nicely
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SourceCode, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeFileDetails_FileNotExistsInBranch, true);
                }

                //Load the passed-in revision file so we can display its name
                SourceCodeCommit sourceCodeRevision = null;
				try
				{
					sourceCodeRevision = sourceCodeManager.RetrieveRevisionByKey(revisionKey);
				}
				catch (ArtifactNotExistsException)
				{
                    //If the revision doesn't exist let the user know nicely
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SourceCode, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeRevisionDetails_RevisionNotExistsInBranch, true);
				}

                if (sourceCodeRevision == null || sourceCodeRevision.RevisionId < 0)
                {
                    //If the revision doesn't exist let the user know nicely
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SourceCode, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeRevisionDetails_RevisionNotExistsInBranch, true);
                }

                //Specify the diff mode (default to unified)
                DiffMode = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_DIFF_MODE, "unified");
            }
			catch (SourceCodeProviderAuthenticationException)
			{
				this.lblMessage.Text = Resources.Messages.SourceCodeList_UnableToAuthenticate;
				this.lblMessage.Type = MessageBox.MessageType.Error;
			}
			catch (SourceCodeProviderLoadingException)
			{
				this.lblMessage.Text = Resources.Messages.SourceCodeList_UnableToLoadProvider;
				this.lblMessage.Type = MessageBox.MessageType.Error;
			}
			catch (SourceCodeProviderGeneralException)
			{
				this.lblMessage.Text = Resources.Messages.SourceCodeList_GeneralError;
				this.lblMessage.Type = MessageBox.MessageType.Error;
			}
			catch (SourceCodeCacheInvalidException)
			{
				this.lblMessage.Text = Resources.Messages.SourceCodeList_CacheNotReadyError;
				this.lblMessage.Type = MessageBox.MessageType.Error;
			}
		}
	}
}
