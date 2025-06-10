using System;
using System.Data;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Generic;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;
using System.Text;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// details of a particular document and handling updates
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Documents, null, "Document-Management/#document-details", "DocumentDetails_Title")]
	public partial class DocumentDetails : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.DocumentDetails::";

		protected int attachmentId;
		protected string ArtifactTabName = null;

		#region Properties

		/// <summary>
		/// Redirects back to the list page
		/// </summary>
		protected string ArtifactListPageUrl
        {
            get
            {
                //Redirect to the appropriate page
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId));
            }
        }

        /// <summary>
        /// Redirects to a different document
        /// </summary>
        /// <param name="attachmentId"></param>
        protected string ArtifactRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId, -2));
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

            //Capture the passed attachment id from the querystring
            this.attachmentId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_ATTACHMENT_ID]);

            //If we're explicitly passed a project, need to change to it
			if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]))
			{
				int artifactProjectId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]);
				VerifyArtifactProject(artifactProjectId, UrlRoots.NavigationLinkEnum.Documents, attachmentId);
			}

            //Configure the association panel
            this.tstAssociationPanel.MessageLabelHandle = this.lblMessage;
            this.tstAssociationPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Document;
            this.tstAssociationPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.Attachments;

			//Configure the Attachment panel
			this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;
			this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Document;
			this.tstAttachmentPanel.LoadAndBindData(true);
			this.tstAttachmentPanel.ArtifactId = this.attachmentId;


			//Set the permissions and action on the Add Comment button
			this.btnNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);
            this.btnNewComment.ClientScriptMethod = String.Format("add_comment('{0}')", this.txtNewComment.ClientID);

			//Set the project/artifact for the RTE so that we can upload screenshots
			this.txtDescription.Screenshot_ProjectId = ProjectId;
			this.txtDescription.Screenshot_ArtifactType = Artifact.ArtifactTypeEnum.Document;
			this.txtDescription.Screenshot_ArtifactId = this.attachmentId;
			this.txtNewComment.Screenshot_ProjectId = ProjectId;
			this.txtNewComment.Screenshot_ArtifactType = Artifact.ArtifactTypeEnum.Document;
			this.txtNewComment.Screenshot_ArtifactId = this.attachmentId;
			this.txtEditRichText.Screenshot_ProjectId = ProjectId;
			this.txtEditRichText.Screenshot_ArtifactType = Artifact.ArtifactTypeEnum.Document;
			this.txtEditRichText.Screenshot_ArtifactId = this.attachmentId;

			//Only load the data once
			if (!IsPostBack)
			{
				LoadAndBindData();
			}

			//Reset the error message
			this.lblMessage.Text = "";

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// This handles the upload of a new version of the document
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>The required fields are not handled by validators as the </remarks>
		void btnVersionUpload_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnVersionUpload_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure all validation occurred
			if (!Page.IsValid)
			{
				return;
			}

            try
            {
                //Capture the passed attachment id from the querystring
                this.attachmentId = System.Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_ATTACHMENT_ID]);

                //Make sure that we have a version number filled-out
                if (this.txtVersionNumber.Text.Trim() == "")
                {
                    this.lblMessage.Text = Resources.Messages.DocumentDetails_VersionNumberRequired;
                    return;
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                //The attachment folder does not exist
                this.lblMessage.Text = Resources.Messages.FileUploadDialog_DirectoryNotFound;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
            catch (System.Security.SecurityException)
            {
                //The attachment folder is not accessible
                this.lblMessage.Text = Resources.Messages.FileUploadDialog_DirectorySecurityError;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
            catch (Exception exception)
            {
                //Another error occurred
                this.lblMessage.Text = String.Format(Resources.Messages.FileUploadDialog_UploadErrorGeneral, exception);
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
		}

        /// <summary>
        /// Gets the artifact URL for the associated item
        /// </summary>
        /// <param name="artifactTypeId"></param>
        /// <param name="artifactId"></param>
        /// <returns></returns>
        protected string GetArtifactUrl(int artifactTypeId, int artifactId)
        {
            return UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)artifactTypeId, ProjectId, artifactId, GlobalFunctions.PARAMETER_TAB_ATTACHMENTS);
        }

		/// <summary>
		/// Loads and binds the document details
		/// </summary>
		private void LoadAndBindData()
		{
            const string METHOD_NAME = "LoadAndBindData";

			//First retrieve the passed in project attachment record
            Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
			ProjectAttachmentView projectAttachment = null;
			try
			{
                projectAttachment = attachmentManager.RetrieveForProjectById2(ProjectId, this.attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				//If the artifact doesn't exist let the user know nicely
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.DocumentDetails_DocumentNotExists, true);
			}

			//Make sure that the current project is set to the project in the document
			//this is important since the page may get loaded from an email notification URL
            VerifyArtifactProject(projectAttachment.ProjectId, UrlRoots.NavigationLinkEnum.Documents, attachmentId);

            //Specify if the current user created/owns this artifact (used for permission-checking)
            SpiraContext.Current.IsArtifactCreatorOrOwner = (projectAttachment.AuthorId == UserId || projectAttachment.EditorId == UserId);

            //Redirect if the user has limited view permissions and does not have this flag set. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View) == Project.AuthorizationState.Limited && !SpiraContext.Current.IsArtifactCreatorOrOwner)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

			//Load any lookups
			Business.UserManager userManager = new Business.UserManager();
            List<DocumentType> attachmentTypes = attachmentManager.RetrieveDocumentTypes(this.ProjectTemplateId, true);
            List<ProjectAttachmentFolderHierarchy> attachmentFolders = attachmentManager.RetrieveFoldersByProjectId(this.ProjectId);
            List<DataModel.User> users = userManager.RetrieveForProject(this.ProjectId);

			//Load the various drop-downs
            this.ddlDocType.DataSource = attachmentTypes;
            this.ddlDocFolder.DataSource = attachmentFolders;
            this.ddlCreatedBy.DataSource = users;
            this.ddlEditedBy.DataSource = users;

            //Store the folder for this project so we can pass to navigation control
            int folderId = projectAttachment.ProjectAttachmentFolderId;

            //Specify the context for the ajax form control and add client-side handlers
            this.ajxFormManager.ProjectId = this.ProjectId;
            this.ajxFormManager.PrimaryKey = this.attachmentId;
            this.ajxFormManager.ArtifactTypePrefix = Attachment.ARTIFACT_PREFIX;
            this.ajxFormManager.FolderPathUrlTemplate = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId, 0, GlobalFunctions.ARTIFACT_ID_TOKEN));
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("dataSaved", "ajxFormManager_dataSaved");
            handlers.Add("loaded", "ajxFormManager_loaded");
            this.ajxFormManager.SetClientEventHandlers(handlers);

            //Set the context on the workflow operations controls
            Dictionary<string, string> handlers2 = new Dictionary<string, string>();
            handlers2.Add("operationExecuted", "ajxWorkflowOperations_operationExecuted");
            this.ajxWorkflowOperations.ProjectId = ProjectId;
            this.ajxWorkflowOperations.PrimaryKey = this.attachmentId;
            this.ajxWorkflowOperations.SetClientEventHandlers(handlers2);

            //Specify the context for the navigation bar
            this.navDocumentList.ProjectId = ProjectId;
            this.navDocumentList.ContainerId = folderId;
            this.navDocumentList.ListScreenUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId);
            if (this.attachmentId != -1)
            {
                this.navDocumentList.SelectedItemId = this.attachmentId;
            }

            this.navDocumentList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
            this.navDocumentList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.navDocumentList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

            //Specify the context for the folders sidebar
            this.pnlFolders.ProjectId = ProjectId;
            this.pnlFolders.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlFolders.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

            // Set the context of the folders list
            this.trvFolders.ClientScriptServerControlId = this.navDocumentList.UniqueID;
            this.trvFolders.ContainerId = this.ProjectId;

            //See if we have a stored node that we need to populate, use zero(0) otherwise so that the root is selected by default
            int selectedFolder = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
            if (selectedFolder < 1)
            {
                selectedFolder = attachmentManager.GetDefaultProjectFolder(ProjectId); //Makes sure that 'default' folder is selected
            }
            //If the folder does not exist reset to root and update settings-
            else if (!attachmentManager.ProjectAttachmentFolder_Exists(this.ProjectId, selectedFolder))
            {
                selectedFolder = attachmentManager.GetDefaultProjectFolder(ProjectId); //Makes sure that 'default' folder is selected
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
            }
            this.trvFolders.SelectedNodeId = selectedFolder.ToString();

            //Set the context on the comment list 
            this.lstComments.ProjectId = ProjectId;
            
            //Specify the artifact type to retrieve the history log for
            this.tstHistoryPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Document;
            tstHistoryPanel.LoadAndBindData(true);

            //Add the various custom properties to the table of fields
            UnityCustomPropertyInjector.CreateControls(
                ProjectId,
                ProjectTemplateId,
                DataModel.Artifact.ArtifactTypeEnum.Document,
                this.customFieldsDefault,
                this.ajxFormManager,
                this.customFieldsUsers,
                this.customFieldsDates,
                this.customFieldsRichText
                );

			//Register client handlers on the tab control
			Dictionary<string, string> handlers3 = new Dictionary<string, string>();
			handlers3.Add("selectedTabChanged", "tclDocumentDetails_updateViewIframeHeight");
			this.tclDocumentDetails.SetClientEventHandlers(handlers3);

			//Databind the page
			this.DataBind();

            //See if a tab's been selected.
            if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
            {
				string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];
				switch (tabRequest)
                {
                    case GlobalFunctions.PARAMETER_TAB_PREVIEW:
                        tclDocumentDetails.SelectedTab = this.pnlPreview.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_EDIT:
						tclDocumentDetails.SelectedTab = this.pnlEdit.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
                        tclDocumentDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_VERSION:
						tclDocumentDetails.SelectedTab = this.pnlVersions.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_HISTORY:
                        tclDocumentDetails.SelectedTab = this.pnlHistory.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_ATTACHMENTS:
						tclDocumentDetails.SelectedTab = this.pnlAttachments.ClientID;
						this.ArtifactTabName = tabRequest;
						break;


					case GlobalFunctions.PARAMETER_TAB_ASSOCIATION:
                        tclDocumentDetails.SelectedTab = this.pnlAssociations.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

                    default:
                        tclDocumentDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
						break;
                }
            }

			//Specify the base URL in the navigation control
			this.navDocumentList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId, -2, ArtifactTabName);
		}
	}
}
