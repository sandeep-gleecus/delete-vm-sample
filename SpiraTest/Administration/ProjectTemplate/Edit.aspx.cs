using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
    /// <summary>
    /// This webform displays the 'Edit Project Template Page'
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProjectTemplate_Edit_Title", "System-Workspaces/#viewedit-templates", "Admin_ProjectTemplate_Edit_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class Edit : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.Edit::";

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

            //Reset the error message
            this.lblMessage.Text = "";

            //Add the event handlers to the page
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            //Set the url for the back button (depends on whether you are a template or system admin)
            if (UserIsAdmin)
            {
                this.lnkBackToList.NavigateUrl = "~/Administration/ProjectTemplateList.aspx";
            }
            else
            {
                this.lnkBackToList.NavigateUrl = UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");
            }

            //Only load the data once
            if (!IsPostBack)
            {
                //Instantiate the business classes
                TemplateManager templateManager = new TemplateManager();

                //Retrieve the project template by its id
                DataModel.ProjectTemplate projectTemplate = templateManager.RetrieveById(ProjectTemplateId);

                //Redirect if there's no project template selected, or cannot be retrieved
                if (ProjectTemplateId < 1 || projectTemplate == null)
                    Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProjectTemplate, true);

                //Databind the form
                this.DataBind();

                //Populate the form with the existing values (update case)
                this.lblProjectTemplateName.Text = GlobalFunctions.HtmlRenderAsPlainText(projectTemplate.Name);
                this.txtName.Text = GlobalFunctions.HtmlRenderAsRichText(projectTemplate.Name);
                this.txtDescription.Text = GlobalFunctions.HtmlRenderAsRichText(projectTemplate.Description);
                this.chkActiveYn.Checked = projectTemplate.IsActive;

				//Check if any projects are using this template - only need to do it if the template is already active (an inactive template has no projects)
				if (projectTemplate.IsActive)
				{
					ProjectManager projectManager = new ProjectManager();
					List<DataModel.Project> templateProjects = projectManager.RetrieveForTemplate(projectTemplate.ProjectTemplateId);
					if (templateProjects.Any())
					{
						this.chkActiveYn.Enabled = false;
					}
				}

				//show in the ui if bulk change of status is enabled/disabled
				ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(ProjectTemplateId);
				this.chkBulkChangeStatus.Checked = templateSettings.Workflow_BulkEditCanChangeStatus;
			}

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>
        /// Validates the form, and updates the project template record
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            //Instantiate the business class
            Business.TemplateManager templateManager = new Business.TemplateManager();
            DataModel.ProjectTemplate projectTemplate = templateManager.RetrieveById(ProjectTemplateId);

            //Make the updates
            projectTemplate.StartTracking();
            projectTemplate.Name = GlobalFunctions.HtmlScrubInput(this.txtName.Text);
            if (String.IsNullOrEmpty(this.txtDescription.Text))
            {
                projectTemplate.Description = null;
            }
            else
            {
                projectTemplate.Description = GlobalFunctions.HtmlScrubInput(this.txtDescription.Text);
            }

            //Only allow template to made inactive if it has no active products associated with it
            bool templateIsActive = true;
            //If the user is attempting to mark the template inactive then check the status of its products
            if (!this.chkActiveYn.Checked)
            {
                templateIsActive = false;

                ProjectManager projectManager = new ProjectManager();
                List<Inflectra.SpiraTest.DataModel.Project> projects = projectManager.RetrieveForTemplate(projectTemplate.ProjectTemplateId);

                if (projects.Count != 0)
                {
                    foreach (Inflectra.SpiraTest.DataModel.Project project in projects)
                    {
                        if (project.IsActive)
                        {
                            templateIsActive = true;
                            break;
                        }
                    }
                }
            }
            projectTemplate.IsActive = templateIsActive;
            templateManager.Update(projectTemplate);

			//show in the ui if bulk change of status is enabled/disabled
			ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(ProjectTemplateId);
			templateSettings.Workflow_BulkEditCanChangeStatus = this.chkBulkChangeStatus.Checked;
			templateSettings.Save();

			//Now we need to force the context to update this info if we are logged in to this project template
			SpiraContext.Current.ProjectTemplateName = projectTemplate.Name;

            //Redirect back to Admin home page / project list
            if (UserIsAdmin)
            {
                Response.Redirect("~/Administration/ProjectTemplateList.aspx", true);
            }
            else
            {
                Response.Redirect(UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default"), true);
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Redirects the user back to the administration home page when cancel clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnCancel_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            if (UserIsAdmin)
            {
                Response.Redirect("~/Administration/ProjectTemplateList.aspx", true);
            }
            else
            {
                Response.Redirect(UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default"), true);
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        #endregion

    }
}
