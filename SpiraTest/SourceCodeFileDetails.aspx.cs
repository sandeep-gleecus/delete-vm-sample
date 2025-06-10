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
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.SourceCode, null, "Source-Code/#source-code-file-details", "SourceCodeFileDetails_Title")]
	public partial class SourceCodeFileDetails : SourceCodePage
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.SourceCodeFileDetails::";
		
		protected string fileKey;
		private bool editingEnabled = true;
		protected string ArtifactTabName = null;

		#region Properties

		protected string SourceCodeFileViewerUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl("~/SourceCodeFileViewer.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + ProjectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "={0}&" + GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY + "={1}");
            }
        }

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

        /// <summary>
        /// The name of the current branch
        /// </summary>
        protected string BranchKey
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

			//Capture the passed file key from the querystring
			this.fileKey = (Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY]).Trim();

			//We need to make sure that the system is licensed for source code viewing use
			if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
			{
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidLicense, true);
			}

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

			//Populate the user and project id in the grid control
			this.grdSourceCodeRevisionList.ProjectId = this.ProjectId;
			this.grdSourceCodeRevisionList.DisplayTypeId = (int)Artifact.DisplayTypeEnum.SourceCodeFile_Revisions;

			//Register client events handlers on grid
			Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("loaded", "grdSourceCodeRevisionList_loaded");
            this.grdSourceCodeRevisionList.SetClientEventHandlers(handlers);

            //Reset the error message
            this.lblMessage.Text = "";

			//Load the data
			LoadAndBindData(fileKey);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

	
		/// <summary>
		/// Determines the url suffix to add
		/// </summary>
		protected string UrlSuffix
		{
			get
			{
				//Determine the destination page view (attachment, coverage, etc.)
				string urlSuffix = "";
				if (this.tclFileDetails.SelectedTab == this.pnlAssociations.ClientID)
				{
					urlSuffix = "&" + GlobalFunctions.PARAMETER_VIEW_ASSOCIATIONSSC + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
				}
				else if (this.tclFileDetails.SelectedTab == this.pnlRevisions.ClientID)
				{
					urlSuffix = "&" + GlobalFunctions.PARAMETER_VIEW_REVISIONS + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
				}

				return urlSuffix;
			}
		}

        /// <summary>
        /// Returns the url to either revision or file list
        /// </summary>
        protected string ReturnToFileListUrl
        {
            get
            {
                return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SourceCode, ProjectId);
            }
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

			//Specify the context for the sidebar
			this.pnlFolders.ProjectId = ProjectId;
			this.pnlFolders.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
			this.pnlFolders.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));
            this.navSourceCodeFiles.ProjectId = ProjectId;
            this.navSourceCodeFiles.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.navSourceCodeFiles.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

            //Set the context of the folder list
            this.trvFolders.ClientScriptServerControlId = this.navSourceCodeFiles.UniqueID;
            this.trvFolders.ContainerId = this.ProjectId;

            //Set link to return to either revision or file list
            this.navSourceCodeFiles.ListScreenUrl = ReturnToFileListUrl;
            this.btnBack.NavigateUrl = ReturnToFileListUrl;

			//First retrieve the passed in source code file
			try
			{
				//See if we've been passed in a branch key
				string branchKey = "";
				if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY]))
				{
					branchKey = Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY].Trim();
					SourceCodeManager.Set_UserSelectedBranch(UserId, ProjectId, branchKey);
				}

                //Get the list of branches for this project and add the event handler
                SourceCodeManager sourceCodeManager = new SourceCodeManager(this.ProjectId);

                //Get the current branch name 
                string currentBranchKey = SourceCodeManager.Get_UserSelectedBranch(UserId, ProjectId);
                if (String.IsNullOrEmpty(currentBranchKey))
                {
                    currentBranchKey = sourceCodeManager.RetrieveBranches().FirstOrDefault(b => b.IsDefault).BranchKey;
                }
                this.lblBranchName.Text = currentBranchKey;
                BranchKey = currentBranchKey;

                //Set the current branch and file key
                Dictionary<string, object> standardFilters = new Dictionary<string, object>();
                standardFilters.Add("BranchKey", currentBranchKey);
                standardFilters.Add("FileKey", fileKey);
                this.grdSourceCodeRevisionList.SetFilters(standardFilters);

                //Get the current source code folder key
                string selectedFolderKey = SourceCodeManager.Get_UserSelectedSourceFolder(this.UserId, this.ProjectId);
                if (!String.IsNullOrEmpty(selectedFolderKey))
                {
                    try
                    {
                        SourceCodeFolder sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey(selectedFolderKey, currentBranchKey);
                        if (sourceCodeFolder != null)
                        {
                            this.trvFolders.SelectedNodeId = sourceCodeFolder.FolderId.ToString();
                        }
                        else
                        {
                            //This folder does not exist in this branch, so unset the folder
                            SourceCodeManager.Set_UserSelectedSourceFolder(this.UserId, this.ProjectId, null);
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Do nothing as we will just not display a folder name,
                        //happens when page refreshes after the cache is initially invalidated
                    }
                }

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

                //Set the initial project/artifact context for the form manager
                this.ajxFormManager.ProjectId = ProjectId;
                this.ajxFormManager.PrimaryKey = sourceCodeFile.FileId;
                this.ajxFormManager.FolderPathUrlTemplate = "javascript:pnlFolderPath_click({art})";

                //Set the navigation bar selected item
                this.navSourceCodeFiles.SelectedItemId = sourceCodeFile.FileId;

                //Register client event handlers
                Dictionary<string, string> handlers = new Dictionary<string, string>();
                handlers.Add("loaded", "ajxFormManager_loaded_sourceCode");
                this.ajxFormManager.SetClientEventHandlers(handlers);

                //Add on the branch if necessary
                if (!String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY]))
				{
                    this.lnkFileName.NavigateUrl += "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY + "=" + Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY].Trim();
				}

				//On first load get the viewing option from the querystring
				this.grdSourceCodeRevisionList.AutoLoad = true;
				if (Request.QueryString[GlobalFunctions.PARAMETER_VIEW_REVISIONS] == GlobalFunctions.PARAMETER_VALUE_TRUE)
				{
					//Viewing Revisions
					tclFileDetails.SelectedTab = this.pnlRevisions.ClientID;
					this.grdSourceCodeRevisionList.AutoLoad = true;
				}
				if (Request.QueryString[GlobalFunctions.PARAMETER_VIEW_ASSOCIATIONSSC] == GlobalFunctions.PARAMETER_VALUE_TRUE)
				{
					//Viewing Associations
					tclFileDetails.SelectedTab = this.pnlAssociations.ClientID;
					this.grdSourceCodeRevisionList.AutoLoad = false;
				}

                //Configure the association panel
                this.tstAssociationPanel.MessageLabelHandle = this.lblMessage;
                this.tstAssociationPanel.ArtifactId = sourceCodeFile.FileId;
                this.tstAssociationPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.SourceCodeFile;
                this.tstAssociationPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.SourceCodeFile_Associations;

            }
            catch (SourceCodeProviderAuthenticationException)
			{
				this.lblMessage.Text = Resources.Messages.SourceCodeList_UnableToAuthenticate;
				this.lblMessage.Type = MessageBox.MessageType.Error;
				this.grdSourceCodeRevisionList.Visible = false;
			}
			catch (SourceCodeProviderLoadingException)
			{
				this.lblMessage.Text = Resources.Messages.SourceCodeList_UnableToLoadProvider;
				this.lblMessage.Type = MessageBox.MessageType.Error;
				this.grdSourceCodeRevisionList.Visible = false;
			}
			catch (SourceCodeProviderGeneralException)
			{
				this.lblMessage.Text = Resources.Messages.SourceCodeList_GeneralError;
				this.lblMessage.Type = MessageBox.MessageType.Error;
				this.grdSourceCodeRevisionList.Visible = false;
			}
			catch (SourceCodeCacheInvalidException)
			{
				this.lblMessage.Text = Resources.Messages.SourceCodeList_CacheNotReadyError;
				this.lblMessage.Type = MessageBox.MessageType.Error;
				this.grdSourceCodeRevisionList.Visible = false;
			}
		}
	}
}
