namespace Inflectra.SpiraTest.Web.UserControls
{
	using System;
    using System.ComponentModel;
    using System.Collections.Generic;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.IO;
    using System.Linq;

	using Inflectra.SpiraTest.Business;
	
	using Inflectra.SpiraTest.Common;
	using Inflectra.SpiraTest.Web.Classes;
    using Inflectra.SpiraTest.Web.ServerControls;
    using Inflectra.SpiraTest.DataModel;

	/// <summary>
	///		This user control displays the attachment list and upload form used by the various artifact
	///		details pages. It is typically enclosed in a panel
	/// </summary>
	public partial class AttachmentPanel : ArtifactUserControlBase, IArtifactUserControl
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.AttachmentPanel::";

		//Viewstate keys
		protected const string ViewStateKey_CustomUploadControl = "CustomUploadControl";

		#region Properties

        /// <summary>
        /// Used to specify a list of secondary artifacts that we also want to display in the
        /// list of attachments. This is used primarily in the test run page where we want to display
        /// the attachments of the test run, test case and test step all in one grid, but only
        /// have the user able to delete or upload attachments related to the test run itself
        /// </summary>
        [
        Bindable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Data"),
        Description("Used to specify a list of secondary artifacts that we also want to display in the list of attachments")
        ]
        public List<AttachmentPanelAdditionalArtifact> AdditionalArtifacts
        {
            get
            {
                if (this.additionalArtifacts == null)
                {
                    this.additionalArtifacts = new List<AttachmentPanelAdditionalArtifact>();
                }
                return this.additionalArtifacts;
            }
        }
        List<AttachmentPanelAdditionalArtifact> additionalArtifacts = null;

        /// <summary>
        /// Returns the base URL for the document details page
        /// </summary>
        public string DocumentDetailsUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId, -2));
            }
        }
		
		#endregion Properties

		#region Event Handlers

		/// <summary>
		/// This sets up the user control upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

            //Populate the user and project id in the grid control
            this.grdAttachmentList.ProjectId = this.ProjectId;
            this.grdAttachmentList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_DOCUMENT;

            //Set the display type so that we know it's the list of documents for an artifact not the main documents list
            //This is important because negative rows are source code file attachments not folders
            this.grdAttachmentList.DisplayTypeId = (int)DataModel.Artifact.DisplayTypeEnum.Attachments;

            //Populate the user and project id in the treeviews
            this.ajxDocFolderSelector.ContainerId = this.ProjectId;
            this.ajxSourceCodeFolderSelector.ContainerId = this.ProjectId;
 
            //Populate the user and project id in the itemselector controls
            this.ajxDocFileSelector.ProjectId = this.ProjectId;
            this.ajxSourceCodeFileSelector.ProjectId = this.ProjectId;

			this.grdAttachmentList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Attachment, ProjectId, -2);

            //For the various controls, we need to set the artifact type for permissions
            //The Add/Add Existing/Remove are now done client-side to enable live loading support
            this.grdAttachmentList.Authorized_ArtifactType = this.ArtifactTypeEnum;

            //Make sure the user has permissions to attach files
            //This may be in addition to the artifact-specified permission
            ProjectManager projectManager = new ProjectManager();
            ProjectRole projectRole = projectManager.RetrieveRolePermissions(ProjectRoleId);

            //Add the client code for handling the display of URL/File/Screenshot attachment when radio buttons changed
            this.radFile.Attributes.Add("onclick", "radFile_click()");
            this.radUrl.Attributes.Add("onclick", "radUrl_click()");
            this.radScreenshot.Attributes.Add("onclick", "radScreenshot_click()");

            //Add the paste event handler for the screenshot capture
            Dictionary<string, string> handlers3 = new Dictionary<string, string>();
            handlers3.Add("imagePaste", "ajxScreenshotCapture_imagePaste");
            this.ajxScreenshotCapture.SetClientEventHandlers(handlers3);

            bool canViewSourceCode = false;
            bool canEditSourceCode = false;
            if (projectRole != null)
            {
                //See if we can edit/view source code as well
                canViewSourceCode = projectRole.IsSourceCodeView;
                canEditSourceCode = projectRole.IsSourceCodeEdit;
            }

            //The option to display source code attachments is only in SpiraPlan/Team
			//PCS
            if ((Common.License.LicenseProductName == LicenseProductNameEnum.ValidationMaster) && canViewSourceCode && Common.Global.SourceCode_IncludeInAssociationsAndDocuments)
            {
                List<VersionControlProject> versionControlProjects = new SourceCodeManager().RetrieveProjectSettings(this.ProjectId);
                if (versionControlProjects.Count > 0)
                {
                    //See if the user is allowed to edit source code
                    if (!canEditSourceCode)
                    {
                        this.ajxSourceCodeFileSelector.Visible = false;
                        this.ajxSourceCodeFolderSelector.Visible = false;
                        this.radSourceCode.Enabled = false;
                    }
                }
                else
                {
                    this.ajxSourceCodeFileSelector.Visible = false;
                    this.ajxSourceCodeFolderSelector.Visible = false;
                    this.radSourceCode.Enabled = false;
                }
            }
            else
            {
                this.ajxSourceCodeFileSelector.Visible = false;
                this.ajxSourceCodeFolderSelector.Visible = false;
                this.radSourceCode.Enabled = false;
            }
           
            //See if we have any attachments (and set the HasData flag) and also specify the standard
            //filters that need to be sent to the AJAX grid.
            if (!Page.IsPostBack)
            {
                LoadAndBindData(true);
            }
            //Specify if we need to auto-load the data (used if tab is initially visible)
            this.grdAttachmentList.AutoLoad = this.AutoLoad;

            //If the user has limited view permissions for documents, hide the document folder treeview
            if (!SpiraContext.Current.IsProjectAdmin && projectManager.IsAuthorized(SpiraContext.Current.ProjectRoleId.Value, Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                this.ajxDocFolderSelector.Visible = false;
            }

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}


		#endregion

		#region Other Methods

        /// <summary>
        /// Called by the enclosing page if the data has changed since Control.OnLoad
        /// </summary>
        /// <param name="dataBind"></param>
		public void LoadAndBindData (bool dataBind)
		{
            //Specify the artifact as a standard filter, need to handle special case of a test run step
            //that is not strictly speaking an artifact, but allows retrieval of attachments against
            //the test case, test run and test step all at once
            Dictionary<string, object> standardFilters = new Dictionary<string, object>();
            standardFilters.Add("ArtifactId", this.ArtifactId);
            standardFilters.Add("ArtifactType", (int)this.ArtifactTypeEnum);
            //See if we have any secondary artifacts we need to get the attachments for
            foreach (AttachmentPanelAdditionalArtifact additionalArtifact in this.AdditionalArtifacts)
            {
                standardFilters.Add("AdditionalArtifact_" + (int)additionalArtifact.ArtifactTypeEnum, additionalArtifact.ArtifactId);
            }
            this.grdAttachmentList.SetFilters(standardFilters);

            //Populate the list of attachment columns to show/hide and databind
            this.ddlShowHideColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.Document);
            this.ddlShowHideColumns.DataBind();

            //Permissions are now set client-side to allow for 'live loading'

            //Populate the folder and document type dropdowns
            AttachmentManager attachmentManager = new AttachmentManager();
            List<DocumentType> attachmentTypes = attachmentManager.RetrieveDocumentTypes(ProjectTemplateId, true);
            List<ProjectAttachmentFolderHierarchy> attachmentFolders = attachmentManager.RetrieveFoldersByProjectId(ProjectId);
            this.ddlDocType.DataSource = attachmentTypes;
            this.ddlDocType.DataSource = attachmentTypes;
            this.ddlDocFolder.DataSource = attachmentFolders;

            //Databind the page (needed for dropdowns and validators to work)
            this.DataBind();

            //Default the the file to checked
            this.radFile.Checked = true;

            //Reset messages
            this.msgUploadMessage.Text = "";
        }

  
		#endregion
	}

    /// <summary>
    /// Used to store a additional artifact that we want to include results for
    /// </summary>
    [
    ToolboxData("<{0}:AttachmentPanelAdditionalArtifact runat=server></{0}:AttachmentPanelAdditionalArtifact>")
    ]
    public class AttachmentPanelAdditionalArtifact : Control
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artifactType">The type of artifact</param>
        /// <param name="artifactId">The id of the artifact</param>
        public AttachmentPanelAdditionalArtifact(DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId)
        {
            this.ArtifactTypeEnum = artifactType;
            this.ArtifactId = artifactId;
        }

        /// <summary>
        /// The type of artifact
        /// </summary>
        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("The type of artifact"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public DataModel.Artifact.ArtifactTypeEnum ArtifactTypeEnum
        {
            get
            {
                if (ViewState["ArtifactType"] == null)
                {
                    return DataModel.Artifact.ArtifactTypeEnum.None;
                }
                else
                {
                    return (DataModel.Artifact.ArtifactTypeEnum)ViewState["ArtifactType"];
                }
            }
            set
            {
                ViewState["ArtifactType"] = value;
            }
        }

        /// <summary>
        /// The ID of artifact
        /// </summary>
        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("The ID of artifact"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public int ArtifactId
        {
            get
            {
                return (int)ViewState["ArtifactId"];
            }
            set
            {
                ViewState["ArtifactId"] = value;
            }
        }

    }
}
