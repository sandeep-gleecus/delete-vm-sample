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
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration.Portfolio
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Portfolio Details Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_PortfolioEditDetails_Title", "System-Workspaces/#edit-a-portfolio", "Admin_PortfolioEditDetails_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class Edit : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.Portfolio.Edit::";

        protected DataModel.Portfolio portfolio;

        /// <summary>
        /// The URL to return to (depends on whether user is system admin or program admin)
        /// </summary>
        protected string ReturnUrl
        {
            get
            {
                if (UserIsAdmin)
                {
                    return "~/Administration/PortfolioList.aspx";
                }
                else
                {
                    //Return to Portfolio dashboard since there is no program admin dashboard currently
                    return UrlRoots.RetrievePortfolioURL(UrlRoots.NavigationLinkEnum.PortfolioHome, PortfolioId);
                }
            }
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

            //Make sure we're authorized to view portfolios
            if (!Common.Global.Feature_Portfolios)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Common.License.LicenseProductName + " does not support portfolios", true);
            }

            //Reset the error message
            this.lblMessage.Text = "";

            //Make sure we have a portfolio id set
            //in theory should not happen unless someone spoofs the URL
            if (PortfolioId < 1)
            {
                Response.Redirect(ReturnUrl, true);
                return;
            }

            //Set the back to list URL
            this.lnkBackToList.NavigateUrl = ReturnUrl;

            //Add the event handlers to the page
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.grdPrograms.RowDataBound += grdPrograms_RowDataBound;

            //Initially default to the programs tab
            this.tclPortfolioDetails.SelectedTab = pnlPrograms.ClientID;

            //Only load the data once
            if (!IsPostBack)
            {
                //Instantiate the business classes
                PortfolioManager portfolioManager = new PortfolioManager();

                //Retrieve the project group from the querystring if we have one (Edit/Update Mode)
                LoadPortfolio();

                //Load the program list
                LoadProgramList();

                //Databind the form
                this.DataBind();

                //Populate the form with the existing values (update case)
                this.lblPortfolioName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(portfolio.Name);
                this.txtName.Text = GlobalFunctions.HtmlRenderAsRichText(portfolio.Name);
                this.txtDescription.Text = GlobalFunctions.HtmlRenderAsRichText(String.IsNullOrEmpty(portfolio.Description) ? "" : portfolio.Description);
                this.chkActiveYn.Checked = portfolio.IsActive;
           }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Adds selective formatting to the program list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdPrograms_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                ProjectGroup projectView = (ProjectGroup)e.Row.DataItem;
                //Change the color of the cell depending on the status
                if (projectView.IsActive)
                {
                    e.Row.Cells[3].CssClass = "bg-success";
                }
                else
                {
                    e.Row.Cells[3].CssClass = "bg-warning";
                }
            }
        }

        /// <summary>
        /// Gets the project group id from querystring and loads the main dataset if in edit mode
        /// </summary>
        protected void LoadPortfolio()
        {
            //Retrieve the project group from the querystring
            Business.PortfolioManager portfolioManager = new Business.PortfolioManager();
            try
            {
                this.portfolio = portfolioManager.Portfolio_RetrieveById(PortfolioId);
            }
            catch (ArtifactNotExistsException)
            {
            }
        }

  
        /// <summary>
        /// Load the program list
        /// </summary>
        protected void LoadProgramList()
        {
            const string METHOD_NAME = "LoadProgramList";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();

            //Retrieve the program list
            Hashtable filters = new Hashtable();
            filters.Add("PortfolioId", PortfolioId);
            List<ProjectGroup> programs = projectGroupManager.Retrieve(filters, null);
            this.grdPrograms.DataSource = programs;

            //Set the formats of the columns
            ((BoundFieldEx)this.grdPrograms.Columns[4]).DataFormatString = GlobalFunctions.ARTIFACT_PREFIX_PROJECT_GROUP + GlobalFunctions.FORMAT_ID;
            
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

            Response.Redirect(ReturnUrl, true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Validates the form, and updates the project group record
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnSave_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            //Instantiate the business class
            Business.PortfolioManager portfolioManager = new Business.PortfolioManager();

            //Retrieve the project group from the ID held in the querysting
            DataModel.Portfolio portfolio = portfolioManager.Portfolio_RetrieveById(PortfolioId);

            //Make the updates
            portfolio.StartTracking();
            portfolio.Name = GlobalFunctions.HtmlScrubInput(this.txtName.Text);
            if (this.txtDescription.Text == "")
            {
                portfolio.Description = null;
            }
            else
            {
                portfolio.Description = GlobalFunctions.HtmlScrubInput(this.txtDescription.Text);
            }
            portfolio.IsActive = this.chkActiveYn.Checked;
            portfolioManager.Portfolio_Update(portfolio);

            //Return to the portfolio list page
            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            Response.Redirect(ReturnUrl, true);
        }
    }
}
