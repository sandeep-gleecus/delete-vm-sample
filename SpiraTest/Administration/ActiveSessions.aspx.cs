using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Active Sessions Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ActiveSessions_Title", "System-Users/#active-sessions", "Admin_ActiveSessions_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class ActiveSessions : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ActiveSessions::";

        protected string productName = "";

        /// <summary>
        /// Sets up the page when first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Set the licensed product name (used in several places)
            this.productName = ConfigurationSettings.Default.License_ProductType;

            //Register the event handlers
            this.grdActiveSessions.RowCommand += new GridViewCommandEventHandler(grdActiveSessions_RowCommand);

            //Only load the data once
            if (!IsPostBack)
            {
                LoadActiveSessions();
            }

            //Reset the error message
            this.lblMessage.Text = "";
        }

        /// <summary>
        /// Loads the list of active user sessions
        /// </summary>
        protected void LoadActiveSessions()
        {
            //Load the list of active sessions and databind
            List<DataModel.User> activeSessions = Global.GetActiveUserSessions();
            this.grdActiveSessions.DataSource = activeSessions;
            this.grdActiveSessions.DataBind();

            //Load in the license key information
            Common.License.Load();

            if (Common.License.LicenseType == LicenseTypeEnum.Enterprise)
            {
                this.ltrLicenseUsage.Text = String.Format(Resources.Main.Admin_ActiveSessions_UsageEnterprise, activeSessions.Count);
            }
            else
            {
                this.ltrLicenseUsage.Text = String.Format(Resources.Main.Admin_ActiveSessions_UsageConcurrent, activeSessions.Count, Common.License.Number);
            }
        }

        /// <summary>
        /// This event handler ends the sessions belonging to the specified user
        /// </summary>
        /// <param name="sender">The object sending the event</param>
        /// <param name="e">The event handler arguments</param>
        private void grdActiveSessions_RowCommand(object source, GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdActiveSessions_RowCommand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Kill that user's session
            int userId = Int32.Parse((string)e.CommandArgument);
            Web.Global.KillUserSessions(userId, true);

            //Refresh the list of active sessions
            LoadActiveSessions();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
    }
}
