using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Project Document List and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Documents, "SiteMap_Documents", "Document-Management/#document-list")]
	public partial class Documents : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Documents::";

        #region Properties

        /// <summary>
        /// Returns the base URL for opening an attachment
        /// </summary>
        public string AttachmentOpenUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Attachment, ProjectId, -2));
            }
        }

        #endregion

        /// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Redirect if the user has limited view permissions for this artifact type. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

			//Populate the user and project id in the treeview
			this.trvFolders.ContainerId = this.ProjectId;
            this.trvFolders.PageUrlTemplate = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId, 0, GlobalFunctions.ARTIFACT_ID_TOKEN);

            //Specify the context for the quick-filters sidebar
            this.pnlQuickFilters.ProjectId = ProjectId;
            this.pnlQuickFilters.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlQuickFilters.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Specify the context for the folders sidebar
            this.pnlFolders.ProjectId = ProjectId;
            this.pnlFolders.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlFolders.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Specify the context for the tag cloud sidebar
            this.pnlTagCloud.ProjectId = ProjectId;
            this.pnlTagCloud.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlTagCloud.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

			//Populate the user and project id in the grid control
			this.grdDocumentList.ProjectId = this.ProjectId;
			this.grdDocumentList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId, -2);
            this.grdDocumentList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_DOCUMENT;
            this.grdDocumentList.FolderUrlTemplate = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId, 0, GlobalFunctions.ARTIFACT_ID_TOKEN);

            //See if we have a stored node that we need to populate, use the default otherwise
            //See if a folder was specified through the URL
            AttachmentManager attachmentManager = new AttachmentManager();
            int selectedFolder = 0; //Root
            if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_FOLDER_ID]))
            {
                int queryStringFolderId;
                if (Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_FOLDER_ID], out queryStringFolderId))
                {
                    //Check the folder exists - if not reset to root
                    selectedFolder = attachmentManager.ProjectAttachmentFolder_Exists(ProjectId, queryStringFolderId) ? queryStringFolderId : 0;

                    //In this case we also need to add it to the grid as a 'standard filter' so it does not get overriden by a saved setting
                    Dictionary<string, object> folderFilter = new Dictionary<string, object>();
                    folderFilter.Add(GlobalFunctions.SPECIAL_FILTER_FOLDER_ID, selectedFolder);
                    this.grdDocumentList.SetFilters(folderFilter);

                    //Update the user settings to mark this as the selected folder
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, selectedFolder);
                }
            }
            else
            {
                selectedFolder = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (selectedFolder < 1)
                {
                    //Makes sure that 'root' is selected
                    selectedFolder = attachmentManager.GetDefaultProjectFolder(ProjectId);
                }
                //If the folder does not exist reset to root and update settings
                else if (!attachmentManager.ProjectAttachmentFolder_Exists(ProjectId, selectedFolder))
                {
                    selectedFolder = attachmentManager.GetDefaultProjectFolder(ProjectId); //Makes sure that 'root' is selected
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                }
            }
			this.trvFolders.SelectedNodeId = selectedFolder.ToString();

            //See if we should have dragging (of documents to folders) enabled
            //Requires that the user has permissions to modify documents (not bulk edit)
            bool canDragDocuments = false;
            if (UserIsAdmin)
            {
                canDragDocuments = true;
            }
            else if (new ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.Modify) == Project.AuthorizationState.Authorized)
            {
                canDragDocuments = true;
            }
            this.trvFolders.AllowDragging = canDragDocuments;
            this.grdDocumentList.AllowDragging = canDragDocuments;

            //Instantiate business classes

            if (!IsPostBack)
			{
                //Populate the list of columns to show/hide and databind
                this.ddlShowHideColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.Document);
                this.ddlShowHideColumns.DataBind();

                //Populate the folder and document type dropdowns
                List<DocumentType> attachmentTypes = attachmentManager.RetrieveDocumentTypes(ProjectTemplateId, true);
                this.ddlDocType.DataSource = attachmentTypes;

                //Databind the page (needed for dropdowns and validators to work)
                this.DataBind();

                //Set the default document type
                int? defaultType = null;
                foreach (DocumentType attachmentType in attachmentTypes)
                {
                    if (attachmentType.IsDefault)
                    {
                        defaultType = attachmentType.DocumentTypeId;
                        break;
                    }
                }

                if (defaultType.HasValue)
                {
                    this.ddlDocType.SelectedValue = defaultType.Value.ToString();
                }

                //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
				this.btnEnterCatch.Attributes.Add("onclick", "return false;");
				this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            //Populate the tagcloud control (not stored in viewstate)
            List<ProjectTagFrequency> tags = attachmentManager.RetrieveTagFrequency(ProjectId);
            this.tagCloud.DataSource = tags;
            this.tagCloud.DataBind();

			//We need to add some client-code that handles the mouse-over event on the grid, launching the popup dialog box
            //and catches the dragging event from the document grid
			Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("focusOn", "grdDocumentList_focusOn");
            this.grdDocumentList.SetClientEventHandlers(handlers);

            //Client-side handlers on the treeview
            Dictionary<string, string> handlers2 = new Dictionary<string, string>();
            handlers2.Add("itemDropped", "trvFolders_dragEnd");
            this.trvFolders.SetClientEventHandlers(handlers2);

            //Add the paste event handler for the screenshot capture
            Dictionary<string, string> handlers3 = new Dictionary<string, string>();
            handlers3.Add("imagePaste", "ajxScreenshotCapture_imagePaste");
            this.ajxScreenshotCapture.SetClientEventHandlers(handlers3);

            //Reset the error messages
            this.lblMessage.Text = "";
            this.msgUploadMessage.Text = "";

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
	}
}
