using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.UserControls.WebParts;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Project Group Home Page and handling all raised events
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.ProjectGroupHome, null, "Program-Homepage")]
	public partial class ProjectGroupHome2 : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ProjectGroupHome2::";

		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have a project group id
            if (ProjectGroupId.HasValue)
            {
                //Make sure we're authorized for this group
                ProjectGroupManager projectGroupManager = new ProjectGroupManager();
                if (!projectGroupManager.IsAuthorized(UserId, this.ProjectGroupId.Value))
                {
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "You are not authorized to view this Project Group!", true);
                }

                //Add the event handlers to the page
                this.btnDesignView.Click += new EventHandler(btnDesignView_Click);
                this.btnBrowseView.Click += new EventHandler(btnBrowseView_Click);
                this.btnCustomize.Click += new EventHandler(btnCustomize_Click);
                this.prtManager.DisplayModeChanged += new WebPartDisplayModeEventHandler(prtManager_DisplayModeChanged);
                this.prtManager.WebPartAdding += new WebPartAddingEventHandler(prtManager_WebPartAdding);
                this.prtManager.WebPartAdded += new WebPartEventHandler(prtManager_WebPartAdded);

                //Load any controls that are not in webparts
                if (!IsPostBack)
                {
                    //Display the project group name and id
                    try
                    {
                        ProjectGroup projectGroup = projectGroupManager.RetrieveById(this.ProjectGroupId.Value);
                        string projectGroupName = projectGroup.Name;
                        this.lblProjectGroupId.Text = GlobalFunctions.ARTIFACT_PREFIX_PROJECT_GROUP + ProjectGroupId.Value;
                        this.lblProjectGroupName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(projectGroupName);

                        //Update the page title/description with the project group id and name
                        MasterPages.Main mainMaster = ((MasterPages.Main)((MasterPages.Dashboard)((MasterPages.ProjectGroupDashboard)this.Master).Master).Master);
                        mainMaster.PageTitle = GlobalFunctions.ARTIFACT_PREFIX_PROJECT_GROUP + ProjectGroupId.Value + " - " + projectGroupName;
                        mainMaster.TstGlobalNavigation.BreadcrumbText = projectGroupName;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The Project Group selected no longer exists in the system.", true);
                    }

                    //Default the page to browse mode
                    this.btnBrowseView.Visible = false;
                    this.btnDesignView.Visible = true;
                    this.btnCustomize.Visible = true;
                    this.prtManager.DisplayMode = WebPartManager.BrowseDisplayMode;
                    this.prtLeftZone.CssClass = "BrowseView";
                    this.prtRightZone.CssClass = "BrowseView";
                    this.prtTopZone.CssClass = "BrowseView";
                    this.prtBottomZone.CssClass = "BrowseView";

                    //Add the URLs for the 3 versions of the home page
                    this.lnkGeneral.NavigateUrl = UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.ProjectGroupHome, this.ProjectGroupId.Value, "General");
                    this.lnkDevelopment.NavigateUrl = UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.ProjectGroupHome, this.ProjectGroupId.Value, "Dev");
                    this.lnkTesting.NavigateUrl = UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.ProjectGroupHome, this.ProjectGroupId.Value, "Test");

                    //See if we need to update the preferred version of the project group home page
                    string preferredHome = GetUserSetting(GlobalFunctions.USER_SETTINGS_PROJECT_GROUP_HOME_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
                    if (preferredHome != "Test")
                    {
                        SaveUserSetting(GlobalFunctions.USER_SETTINGS_PROJECT_GROUP_HOME_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "Test");
                    }
                }
            }
            else
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The specified project group is not valid.", true);
            }

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Switches the dashboard to catalog view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnCustomize_Click(object sender, EventArgs e)
		{
			this.btnBrowseView.Visible = false;
			this.btnDesignView.Visible = true;
			this.prtManager.DisplayMode = WebPartManager.CatalogDisplayMode;
		}

		/// <summary>
		/// Switches the dashboard to browse view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnBrowseView_Click(object sender, EventArgs e)
		{
			this.btnBrowseView.Visible = false;
			this.btnDesignView.Visible = true;
			this.prtManager.DisplayMode = WebPartManager.BrowseDisplayMode;
		}

		/// <summary>
		/// Switches the dashboard to design view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnDesignView_Click(object sender, EventArgs e)
		{
			this.btnBrowseView.Visible = true;
			this.btnDesignView.Visible = false;
			this.prtManager.DisplayMode = WebPartManager.EditDisplayMode;
		}

		/// <summary>
		/// Called when a new web part has been added
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void prtManager_WebPartAdded(object sender, WebPartEventArgs e)
		{
			//If implements reloadable, then fire the LoadAndBind() method
			if (((GenericWebPart)e.WebPart).ChildControl is IWebPartReloadable)
			{
				IWebPartReloadable reloadableWebPart = (IWebPartReloadable)(((GenericWebPart)e.WebPart).ChildControl);
				reloadableWebPart.LoadAndBindData();
			}
		}

		/// <summary>
		/// Changes the skin if the display mode is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void prtManager_DisplayModeChanged(object sender, WebPartDisplayModeEventArgs e)
		{
            if (this.prtManager.DisplayMode.AllowPageDesign)
            {
                this.prtTopZone.CssClass = "DesignView";
                this.prtLeftZone.CssClass = "DesignView";
                this.prtRightZone.CssClass = "DesignView";
                this.prtBottomZone.CssClass = "DesignView";
            }
            else
            {
                this.prtTopZone.CssClass = "BrowseView";
                this.prtLeftZone.CssClass = "BrowseView";
                this.prtRightZone.CssClass = "BrowseView";
                this.prtBottomZone.CssClass = "BrowseView";
            }
        }

		/// <summary>
		/// Prevents items in the declarative catalog from being added more than once (which is the default behavior)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void prtManager_WebPartAdding(object sender, WebPartAddingEventArgs e)
		{
			// get element being added - used a base class to check each control can/not be added multiple times
			WebPartBase addElement = (WebPartBase)((GenericWebPart)e.WebPart).ChildControl;

			// get the new element's control pathname
			string path = addElement.GetType().Name;

			// check pathname against each element currently in the document
			foreach (WebPart part in prtManager.WebParts)
			{
				if (part is GenericWebPart)
				{
					GenericWebPart genericPart = (GenericWebPart)part;
					if (genericPart.ChildControl.GetType().Name.Equals(path) && !genericPart.IsClosed)
					{
						// found that the web part control has already been added, tell user and bail out of loop
						e.Cancel = true;
                        this.lblMessage.Text = Resources.Messages.Global_CannotAddWidgetMoreThanOnce;
						this.lblMessage.Type = MessageBox.MessageType.Error;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Adds the project id to the web part manager during Init phase
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		protected void prtManager_Init(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "prtManager_Init";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Tell the web part manager that the settings are specific to this project group
            if (ProjectGroupId.HasValue)
            {
                this.prtManager.DashboardInstanceId = ProjectGroupId.Value;
            }

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Called to validate that a webpart should be displayed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void prtManager_AuthorizeWebPart(object sender, WebPartAuthorizationEventArgs e)
		{
			//Default to authorized
			e.IsAuthorized = true;

			//Store the base path of all the user controls
			const string USER_CONTROL_BASE_PATH = "~/UserControls/WebParts/ProjectGroupHome/";

			//Check to see if the current license supports the various displayed widgets
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.TestCase))
			{
				if (e.Path == USER_CONTROL_BASE_PATH + "TestExecutionStatus.ascx" || e.Path == USER_CONTROL_BASE_PATH + "RequirementsCoverage.ascx")
				{
					e.IsAuthorized = false;
				}
			}
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Task))
			{
				if (e.Path == USER_CONTROL_BASE_PATH + "TaskProgress.ascx")
				{
					e.IsAuthorized = false;
				}
			}
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Risk))
            {
                if (e.Path == USER_CONTROL_BASE_PATH + "OpenRisks.ascx")
                {
                    e.IsAuthorized = false;
                }
            }
        }
	}
}
