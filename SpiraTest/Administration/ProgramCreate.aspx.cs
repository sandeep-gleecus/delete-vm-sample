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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProgramCreate_Title", "System-Workspaces/#viewedit-programs", "Admin_ProgramCreate_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class ProgramCreate : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProgramCreate::";

        protected List<ProjectGroupRole> projectGroupRoles;
        protected ProjectGroup projectGroup;
        protected int projectGroupId;

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
                //Load the various lookups
                TemplateManager templateManager = new TemplateManager();
                List<DataModel.ProjectTemplate> projectTemplates = templateManager.RetrieveActive();
				projectTemplates = projectTemplates.Select(template =>
				{
					template.Name = template.Name + " [PT:" + template.ProjectTemplateId + "]";
					return template;
				}).ToList();
				this.ddlProjectTemplate.DataSource = projectTemplates;

				if (Common.Global.Feature_Portfolios)
                {
                    PortfolioManager portfolioManager = new PortfolioManager();
                    List<DataModel.Portfolio> portfolios = portfolioManager.Portfolio_Retrieve(true);
                    this.ddlPortfolio.DataSource = portfolios;
                    this.portfolioFormGroup.Visible = true;
                }

                //Databind the form
                this.DataBind();

                this.lblProjectGroupName.Text = Resources.Main.Admin_ProjectGroupDetails_NewProjectGroup;

                //Default to active=yes, default = no (unless no other programs)
                List<ProjectGroup> existingPrograms = new ProjectGroupManager().RetrieveActive();
                this.chkActiveYn.Checked = true;
                this.chkDefaultYn.Checked = (existingPrograms.Count == 0);
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

            Response.Redirect("ProgramList.aspx", true);

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

            //See if we have a project template specified
            int? projectTemplateId = null;

            if (!String.IsNullOrEmpty(this.ddlProjectTemplate.SelectedValue))
            {
                projectTemplateId = Int32.Parse(this.ddlProjectTemplate.SelectedValue);
            }

            int? portfolioId = null;
            if (!String.IsNullOrEmpty(this.ddlPortfolio.SelectedValue))
            {
                portfolioId = Int32.Parse(this.ddlPortfolio.SelectedValue);
            }


            //Check for certain business conditions
            if (!this.chkActiveYn.Checked && this.chkDefaultYn.Checked)
            {
                this.lblMessage.Text = Resources.Messages.Admin_ProjectGroupDetails_CannotMakeInactiveDefault;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            //Instantiate the business class
            Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();

            //Call the insert command
            try
            {
                projectGroupManager.Insert(
                    GlobalFunctions.HtmlScrubInput(this.txtName.Text),
                    GlobalFunctions.HtmlScrubInput(this.txtDescription.Text),
                    GlobalFunctions.HtmlScrubInput(this.txtWebSite.Text),
                    (this.chkActiveYn.Checked == true),
                    (this.chkDefaultYn.Checked== true),
                    projectTemplateId,
                    UserId,
                    portfolioId
                    );
            }
            catch (ProjectGroupDefaultException)
            {
                this.lblMessage.Text = Resources.Messages.Admin_ProjectGroupDetails_CannotMakeDefaultInactive;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            //Return to the project group list page
            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            Response.Redirect("ProgramList.aspx", true);
        }
    }
}
