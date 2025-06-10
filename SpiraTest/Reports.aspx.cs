using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls.WebParts;
using Inflectra.SpiraTest.Web.UserControls.WebParts.Reports;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Reports Home Page and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Reports, "SiteMap_Reporting", "Reports-Center")]
	public partial class Reports : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Reports::";

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

            //Register event handlers
            this.ddlSelectRelease.SelectedIndexChanged += new EventHandler(ddlSelectRelease_SelectedIndexChanged);
            this.btnDesignView.Click += new EventHandler(btnDesignView_Click);
            this.btnBrowseView.Click += new EventHandler(btnBrowseView_Click);
            this.btnCustomize.Click += new EventHandler(btnCustomize_Click);
            this.prtManager.DisplayModeChanged += new WebPartDisplayModeEventHandler(prtManager_DisplayModeChanged);
            this.prtManager.WebPartAdded += new WebPartEventHandler(prtManager_WebPartAdded);
            this.prtManager.WebPartDrillDown += new WebPartEventHandler(prtManager_WebPartDrillDown);

            //Add the URL to the release hierarchical drop-down
            this.ddlSelectRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);

            //Only load the data once
            if (!IsPostBack) 
			{
                //Default the page to browse mode
                this.btnBrowseView.Visible = false;
                this.btnDesignView.Visible = true;
                this.btnCustomize.Visible = true;
                this.prtManager.DisplayMode = WebPartManager.BrowseDisplayMode;
                this.prtLeftZone.CssClass = "BrowseView";
                this.prtRightZone.CssClass = "BrowseView";
                this.prtTopZone.CssClass = "BrowseView";
                this.prtSideBar.CssClass = "BrowseView";

                //Display the page labels
                this.lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(this.ProjectName);

                //Populate the list of releases and databind. We include inactive ones and let the dropdown list filter by active
                //that ensures that a legacy filter is displayed even if it is no longer selectable now
                Business.ReleaseManager releaseManager = new Business.ReleaseManager();
                List<ReleaseView> releases = releaseManager.RetrieveByProjectId(ProjectId, false);
                this.ddlSelectRelease.DataSource = releases;
                this.ddlSelectRelease.DataBind();

                //Set the release drop-down, handing exceptions quietly (in case a release was made inactive)
                try
                {
                    int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                    if (releaseId < 1)
                    {
                        this.ddlSelectRelease.SelectedValue = "";
                    }
                    else
                    {
                        this.ddlSelectRelease.SelectedValue = releaseId.ToString();
                    }
                }
                catch (Exception)
                {
                    //This occurs if the release has been subsequently deleted. In which case we need to update
                    //the stored settings
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                }
            }

            Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>
        /// Called when the release filter drop-down-list is changed
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void ddlSelectRelease_SelectedIndexChanged(object sender, EventArgs e)
        {
            const string METHOD_NAME = "ddlSelectRelease_SelectedIndexChanged";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Capture the release id in project settings
            int releaseId;
            if (this.ddlSelectRelease.SelectedValue == "")
            {
                releaseId = -1;
            }
            else
            {
                releaseId = Int32.Parse(this.ddlSelectRelease.SelectedValue);
            }
            SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);

            //Now need to reload the widgets on the page
            ReloadWidgets(false);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Reloads all widgets on the page that implement IWebPartReloadable and sets the release dropdown
        /// </summary>
        /// <param name="setRelease">Should we update the release dropdown</param>
        public void ReloadWidgets(bool setRelease)
        {
            foreach (WebPart webPart in this.prtManager.WebParts)
            {
                if (webPart is GenericWebPart)
                {
                    //Get the child control
                    if (((GenericWebPart)webPart).ChildControl is IWebPartReloadable)
                    {
                        IWebPartReloadable webPartReloadable = (IWebPartReloadable)(((GenericWebPart)webPart).ChildControl);
                        webPartReloadable.LoadAndBindData();
                    }
                }
            }

            if (setRelease)
            {
                try
                {
                    int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                    if (releaseId == -1)
                    {
                        this.ddlSelectRelease.SelectedValue = "";
                    }
                    else
                    {
                        this.ddlSelectRelease.SelectedValue = releaseId.ToString();
                    }
                }
                catch (Exception)
                {
                    //Fail quietly
                }
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
                this.prtLeftZone.CssClass = "DesignView";
                this.prtRightZone.CssClass = "DesignView";
                this.prtTopZone.CssClass = "DesignView";
                this.prtSideBar.CssClass = "DesignView";
            }
            else
            {
                this.prtLeftZone.CssClass = "BrowseView";
                this.prtRightZone.CssClass = "BrowseView";
                this.prtTopZone.CssClass = "BrowseView";
                this.prtSideBar.CssClass = "BrowseView";
            }
        }
        
        
       /* /// <summary>
        /// We always want the web parts to be in 'Design' mode not Browse mode for this page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void prtManager_DisplayModeChanged(object sender, WebPartDisplayModeEventArgs e)
        {
            if (prtManager.DisplayMode == WebPartManager.BrowseDisplayMode)
            {
                prtManager.DisplayMode = WebPartManager.DesignDisplayMode;
            }
        }*/

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
        /// Called whenever the subtitle drilldown links are clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void prtManager_WebPartDrillDown(object sender, WebPartEventArgs e)
        {
            //See which webpart was clicked on and take the appropriate action
            if (e.WebPart != null && e.WebPart is GenericWebPart)
            {
                GenericWebPart genericWebPart = (GenericWebPart)e.WebPart;
                if (genericWebPart.ChildControl != null && genericWebPart.ChildControl is WebPartBase)
                {
                    //First the parts that are already on the page by default
                    //None

                    //Now, any parts in the declarative catalog need to be located by their type as there is no initial reference
                    //None
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

            if (Page.User.Identity.IsAuthenticated)
            {
                Business.UserManager userManager = new Business.UserManager();
                try
                {
                    DataModel.User user = userManager.GetUserByLogin(Page.User.Identity.Name);
                    //Make sure we have a project selected
                    if (user.Profile.LastOpenedProjectId.HasValue)
                    {
                        int projectId = user.Profile.LastOpenedProjectId.Value;
                        //Tell the web part manager that the settings are specific to this project
                        this.prtManager.DashboardInstanceId = projectId;
                    }
                }
                catch (ArtifactNotExistsException)
                {
                    //Do Nothing
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Called to validate that a webpart should be displayed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This can only be used to authorize "types" of webpart, not individual instances
        /// For that, the web part would need to be handle the authorization
        /// </remarks>
        protected void prtManager_AuthorizeWebPart(object sender, WebPartAuthorizationEventArgs e)
        {
            //Default to authorized
            e.IsAuthorized = true;

            //Store the base path of all the user controls
            //const string USER_CONTROL_BASE_PATH = "~/UserControls/WebParts/Reports/";
        }
	}
}
