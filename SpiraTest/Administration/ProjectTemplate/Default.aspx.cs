using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.Security.Application;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.UserControls.WebParts;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
    /// <summary>
    /// This webform displays the Project Template Home Page
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProjectTemplate_Home_Title", "System-Administration", "Admin_ProjectTemplate_Home_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class Default : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.Default::";

        #region Event Handlers

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the error messages
            this.lblMessage.Text = "";

            //If no project template selected, bounce back to administration home
            if (ProjectTemplateId < 1)
            {
                Response.Redirect("~/Administration/", true);
                return;
            }

            //Register event handlers
            this.btnDesignView.Click += new EventHandler(btnDesignView_Click);
            this.btnBrowseView.Click += new EventHandler(btnBrowseView_Click);
            this.btnCustomize.Click += new EventHandler(btnCustomize_Click);
            this.prtManager.DisplayModeChanged += new WebPartDisplayModeEventHandler(prtManager_DisplayModeChanged);
            this.prtManager.WebPartAdded += new WebPartEventHandler(prtManager_WebPartAdded);
            this.prtManager.WebPartDrillDown += new WebPartEventHandler(prtManager_WebPartDrillDown);

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
            }

            //Finally, see if we have any error message passed from a calling page
            if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE]))
            {
                this.lblMessage.Text = Encoder.HtmlEncode(Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE]);
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }

            //Display the project template name
            this.lblProjectTemplateName.Text = Encoder.HtmlEncode(ProjectTemplateName);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Does any web part dashboard setup
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        protected void prtManager_Init(object sender, System.EventArgs e)
        {
            const string METHOD_NAME = "prtManager_Init";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Tell the web part manager that the settings are specific to this project template
            this.prtManager.DashboardInstanceId = ProjectTemplateId;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
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
                    WebPartBase userControl = (WebPartBase)genericWebPart.ChildControl;

                    //See what type of control we have
                    if (userControl is UserControls.WebParts.ProjectTemplateAdmin.ProjectTemplateOverview)
                    {
                        //Redirect to the project template edit page
                        Response.Redirect(UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Edit"));
                    }
                }
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
            //Store the base path of all the user controls
            const string USER_CONTROL_BASE_PATH = "~/UserControls/WebParts/ProjectTemplateAdmin/";

            //Default to authorized
            e.IsAuthorized = true;
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

        #endregion
    }
}