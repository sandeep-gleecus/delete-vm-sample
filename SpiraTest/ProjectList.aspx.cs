using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.UserControls.WebParts;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.MasterPages;
using Microsoft.Security.Application;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// My Project List Page and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.MyPage, "SiteMap_MyPage", "User-Product-Management/#my-page")]
	public partial class ProjectList : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ProjectList::";

        /// <summary>
        /// Reloads all widgets on the page that implement IWebPartReloadable
        /// </summary>
        protected void ReloadWidgets()
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
        }

		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

			//See if we have any error message passed from the calling page
			if (Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE] != null)
			{
				this.lblMessage.Text = Encoder.HtmlEncode(Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE]);
                this.lblMessage.Type = MessageBox.MessageType.Error;
			}

            //Add the event handlers to the page
            this.btnDesignView.Click += new EventHandler(btnDesignView_Click);
            this.btnBrowseView.Click += new EventHandler(btnBrowseView_Click);
            this.btnCustomize.Click += new EventHandler(btnCustomize_Click);
            this.prtManager.DisplayModeChanged += new WebPartDisplayModeEventHandler(prtManager_DisplayModeChanged);
            this.radAllProjects.CheckedChanged +=new EventHandler(radProjectFilter_CheckedChanged);
            this.radCurrentProjects.CheckedChanged += new EventHandler(radProjectFilter_CheckedChanged);

            //Only load certain items on initial load
            if (!IsPostBack)
            {
                //Specify the user's name
                this.lblFullName.Text = Encoder.HtmlEncode(UserFullName);

                //Default the page to browse mode
                this.btnBrowseView.Visible = false;
                this.btnDesignView.Visible = true;
                this.btnCustomize.Visible = true;
                this.prtManager.DisplayMode = WebPartManager.BrowseDisplayMode;
                this.prtLeftZone.CssClass = "BrowseView";
                this.prtRightZone.CssClass = "BrowseView";

                //Set the project filter radio button, or hide if no project is set
                if (ProjectId < 1)
                {
                    divProjectFilter.Visible = false;
                }
                else
                {
                    divProjectFilter.Visible = true;
                    bool filterbyProject = GetUserSetting(GlobalFunctions.USER_SETTINGS_MY_PAGE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, false);
                    this.radAllProjects.Checked = !filterbyProject;
                    this.lblAllProjects.Attributes["data-checked"] = (filterbyProject) ? "" : "checked";
                    this.radCurrentProjects.Checked = filterbyProject;
                    this.lblCurrentProjects.Attributes["data-checked"] = (filterbyProject) ? "checked" : "";
                }
            }

            Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
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
            const string USER_CONTROL_BASE_PATH = "~/UserControls/WebParts/MyPage/";

            //Check to see if the current license supports the various displayed widgets
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.TestCase))
            {
                if (e.Path == USER_CONTROL_BASE_PATH + "TestCaseList.ascx")
                {
                    e.IsAuthorized = false;
                }
            }
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.TestSet))
            {
                if (e.Path == USER_CONTROL_BASE_PATH + "TestSetList.ascx")
                {
                    e.IsAuthorized = false;
                }
            }
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.TestRun))
            {
                if (e.Path == USER_CONTROL_BASE_PATH + "PendingTestRuns.ascx")
                {
                    e.IsAuthorized = false;
                }
            }
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Task))
            {
                if (e.Path == USER_CONTROL_BASE_PATH + "TaskList.ascx")
                {
                    e.IsAuthorized = false;
                }
            }
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Risk))
            {
                if (e.Path == USER_CONTROL_BASE_PATH + "AssignedRisks.ascx")
                {
                    e.IsAuthorized = false;
                }
            }
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Incident))
            {
                if (e.Path == USER_CONTROL_BASE_PATH + "AssignedIncidents.ascx" || e.Path == USER_CONTROL_BASE_PATH + "DetectedIncidents.ascx"
                    || e.Path == USER_CONTROL_BASE_PATH + "QuickLaunch.ascx")
                {
                    e.IsAuthorized = false;
                }
            }

            //Make sure we have instant messaging enabled and we have SpiraPlan/Team
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Message) || !ConfigurationSettings.Default.Message_Enabled)
            {
                if (e.Path == USER_CONTROL_BASE_PATH + "Contacts.ascx")
                {
                    e.IsAuthorized = false;
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
            }
            else
            {
                this.prtLeftZone.CssClass = "BrowseView";
                this.prtRightZone.CssClass = "BrowseView";
            }
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
        /// Switches the my page to filter by project or all projects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void radProjectFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (ProjectId > 0)
            {
                //Update the stored setting and reload page
                bool filterbyProject = this.radCurrentProjects.Checked;
                SaveUserSetting(GlobalFunctions.USER_SETTINGS_MY_PAGE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, filterbyProject);
                ReloadWidgets();

                //Update visual state of radio
                this.lblAllProjects.Attributes["data-checked"] = (filterbyProject) ? "" : "checked";
                this.lblCurrentProjects.Attributes["data-checked"] = (filterbyProject) ? "checked" : "";
            }
        }

		#endregion
	}
}
