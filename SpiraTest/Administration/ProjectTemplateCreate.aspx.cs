    using System;
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

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Project Template Creation Page and handling all raised events
	/// </summary>
	[
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProjectCreate_Title", "System-Workspaces/#viewedit-templates", "Admin_ProjectCreate_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class ProjectTemplateCreate : Inflectra.SpiraTest.Web.Administration.AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplateCreate::";

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

            // MAKE SURE PAGE CAN'T BE ACCESSED UNTIL PERMISSIONS FIXED IN 6.0
            Response.Redirect("ProjectTemplateList.aspx", true);

            //Reset the error message
            this.lblMessage.Text = "";

			//Add the event handlers to the page
			this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
			this.btnInsert.Click += new EventHandler(btnInsert_Click);
			this.radDefaultTemplate.CheckedChanged += new EventHandler(radProjectTemplate_CheckedChanged);
			this.radExistingTemplate.CheckedChanged += new EventHandler(radProjectTemplate_CheckedChanged);

			//Only load the data once
			if (!IsPostBack)
			{
                //Load the templates
                LoadTemplates();

                //Databind the form
                this.DataBind();

                this.lblProjectTemplateName.Text = Resources.Main.Admin_ProjectTemplateCreate_NewTemplate;

				//Enable and load the choice of templates
				this.radDefaultTemplate.Enabled = true;
				this.radExistingTemplate.Enabled = true;
				this.plcExistingTemplates.Visible = false;
                this.radDefaultTemplate.Checked = true;
				this.ddlExistingTemplates.DataBind();

				//Default to active=yes
				this.chkActiveYn.Checked = true;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

        private void LoadTemplates()
        {
            TemplateManager templateManager = new TemplateManager();
            List<DataModel.ProjectTemplate> projectTemplates = templateManager.RetrieveActive();
            this.ddlExistingTemplates.DataSource = projectTemplates;

            //Databind the dropdown
            this.ddlExistingTemplates.DataBind();
        }

        /// <summary>
        /// Validates the form, and inserts the new project template
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
            TemplateManager templateManager = new TemplateManager();

            //See if we are using the default template or an existing project as the template
            int? existingProjectTemplateId = null;
            if (this.radExistingTemplate.Checked && this.ddlExistingTemplates.SelectedValue != "")
			{
                //Get the id of the existing project template
                existingProjectTemplateId = Int32.Parse(this.ddlExistingTemplates.SelectedValue);
			}

            //Call the insert command
            templateManager.Insert(
                GlobalFunctions.HtmlScrubInput(this.txtName.Text),
                GlobalFunctions.HtmlScrubInput(this.txtDescription.Text),
                (this.chkActiveYn.Checked),
                existingProjectTemplateId
                );

            //Return to the template list page
            Response.Redirect("ProjectTemplateList.aspx");

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Redirects the user back to the template list page when cancel clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnCancel_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnCancel_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            Response.Redirect("ProjectTemplateList.aspx", true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles changes to the 'based on' radio button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void radProjectTemplate_CheckedChanged(object sender, EventArgs e)
		{
			const string METHOD_NAME = "radProjectTemplate_CheckedChanged";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Depending on what option is checked, enable or disable the template section
			if (this.radExistingTemplate.Checked)
			{
				this.plcExistingTemplates.Visible = true;
                this.lblDefaultTemplate.Attributes["data-checked"] = "";
                this.lblExistingTemplate.Attributes["data-checked"] = "checked";
            }
			else
			{
				this.plcExistingTemplates.Visible = false;
                this.lblDefaultTemplate.Attributes["data-checked"] = "checked";
                this.lblExistingTemplate.Attributes["data-checked"] = "";
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion
	}
}
