using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Project Group Details Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_PortfolioCreate_Title", "System-Workspaces/#add-a-new-portfolio", "Admin_PortfolioCreate_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class PortfolioCreate : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.PortfolioCreate::";

        protected DataModel.Portfolio portfolio;
        protected int portfolioId;

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the error message
            this.lblMessage.Text = "";

            //Add the event handlers to the page
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.btnInsert.Click += new EventHandler(btnInsert_Click);

            //Only load the data once
            if (!IsPostBack)
            {
                //Databind the form
                this.DataBind();

                this.lblPortfolioName.Text = Resources.Main.Admin_PortfolioDetails_NewPortfolio;

                //Default to active=yes
                this.chkActiveYn.Checked = true;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Redirects the user back to the project group list when cancel clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnCancel_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            Response.Redirect("PortfolioList.aspx", true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Validates the form, and inserts the new project group
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnInsert_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnInsert_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            //Instantiate the business class
            Business.PortfolioManager portfolioManager = new Business.PortfolioManager();

            //Call the insert command
            portfolioManager.Portfolio_Insert(
                GlobalFunctions.HtmlScrubInput(this.txtName.Text),
                GlobalFunctions.HtmlScrubInput(this.txtDescription.Text),
                (this.chkActiveYn.Checked == true)
                );

            //Return to the portfolio list page
            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            Response.Redirect("PortfolioList.aspx", true);
        }
    }
}
