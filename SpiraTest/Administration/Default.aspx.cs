using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using Microsoft.Security.Application;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Web.UserControls.WebParts;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Page and handling all raised events
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_Home_Title", "System-Administration", "Admin_Home_Title")]
	[AdministrationLevelAttribute(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class _Default : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration._Default::";

		#region Event Handlers

		/// <summary>
		/// Handles initialization that needs to come before Page_Onload is fired
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
		}

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

            //Set TaraVault activation banner, if required and the system is hosted
            if (!ConfigurationSettings.Default.TaraVault_HasAccount && ConfigurationSettings.Default.TaraVault_UserLicense > 0 && Common.Global.Feature_TaraVault)
            {
                // show the banner
                divNotActivated.Visible = true;

                // fill in the display message
                lblActivate.Text = String.Format(Resources.Messages.Admin_TaraVault_Active, Resources.Main.Global_Unlimited, ConfigurationSettings.Default.License_ProductType);
                
                //set the TaraVault logo url
                this.imgTaraVaultLogo.ImageUrl = "Images/" + "product-TaraVault.svg";
                this.imgTaraVaultLogo.AlternateText = "TaraVault";

                //Set the TaraVault activation button
                btnTaraVaultActivate.Click += btnTaraActivate_Click;
            }
            else
            {
                divNotActivated.Visible = false;
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


			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>Hit when the user clicks the 'Activate TaraVault' button.</summary>
        /// <param name="sender">btnTaraActivate</param>
        /// <param name="e">EventArgs</param>
        private void btnTaraActivate_Click(object sender, EventArgs e)
        {
            try
            {
                //Activate the system.
                new VaultManager().Account_Activate();
                Response.Redirect("~/Administration/TaraVault.aspx", true);
            }
            catch (Exception exception)
            {
                //Display the 'nice message'
                this.lblMessage.Text = Resources.Messages.Admin_TaraVault_UnableToActivate + " - " + exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
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
                    if (userControl is UserControls.WebParts.Administration.EventList)
                    {
                        //Redirect to the full event log
                        Response.Redirect("~/Administration/EventLog.aspx");
                    }
                    if (userControl is UserControls.WebParts.Administration.DataSynchronization)
                    {
                        //Redirect to the full event log
                        Response.Redirect("~/Administration/DataSynchronization.aspx");
                    }
                    if (userControl is UserControls.WebParts.Administration.LockedOutUsers)
                    {
                        //Redirect to the full event log
                        Response.Redirect("~/Administration/UserList.aspx");
                    }
                    if (userControl is UserControls.WebParts.Administration.UserRequests)
                    {
                        //Redirect to the full event log
                        Response.Redirect("~/Administration/UserRequests.aspx");
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
