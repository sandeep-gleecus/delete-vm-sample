using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Project Details Page and handling all raised events
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProjectCreate_Title", "System-Workspaces/#viewedit-products", "Admin_ProjectCreate_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class ProjectCreate : Inflectra.SpiraTest.Web.Administration.AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectCreate::";

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
			this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
			this.btnInsert.Click += new EventHandler(btnInsert_Click);
			this.radNewProject.CheckedChanged += new EventHandler(radProjectTemplate_CheckedChanged);
			this.radExistingProject.CheckedChanged += new EventHandler(radProjectTemplate_CheckedChanged);
			this.ddlExistingProjects.SelectedIndexChanged += DdlExistingProjects_SelectedIndexChanged;

			//Only load the data once
			if (!IsPostBack)
			{
				//Instantiate the business classes
				ProjectManager projectManager = new ProjectManager();
				ProjectGroupManager projectGroupManager = new ProjectGroupManager();

				//Retrieve any lookups
				List<ProjectGroup> projectGroups = projectGroupManager.RetrieveActive();
				List<ProjectView> existingProjects = projectManager.Retrieve(null, null);
				this.ddlProjectGroup.DataSource = projectGroups;
				this.ddlExistingProjects.DataSource = existingProjects;

				//Load the templates
				LoadTemplates();

				//Databind the form
				this.DataBind();

				this.lblProjectName.Text = Resources.Main.Admin_ProjectDetails_NewProject;

				//Enable and load the choice of templates
				this.radNewProject.Enabled = true;
				this.radExistingProject.Enabled = true;
				this.plcExistingProjects.Visible = false;
				this.lblProjectTemplate.Enabled = true;
				this.radNewProject.Checked = true;
				this.ddlExistingProjects.DataBind();

				//Default to active=yes and the default project group
				this.chkActiveYn.Checked = true;
				this.ddlProjectGroup.SelectedValue = projectGroupManager.GetDefault().ToString();

				//Hide the Baselining switch, if this feature is not enabled
				this.grpBaseline.Visible = Common.Global.Feature_Baselines;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		private void LoadTemplates()
		{
			TemplateManager templateManager = new TemplateManager();
			//See if we need to filter by current project
			List<DataModel.ProjectTemplate> projectTemplates = new List<DataModel.ProjectTemplate>();
			if (this.radNewProject.Checked)
			{
				projectTemplates = templateManager.RetrieveActive();
				projectTemplates = projectTemplates.Select(template =>
				{
					template.Name = template.Name + " [PT:" + template.ProjectTemplateId + "]";
					return template;
				}).ToList();
			}
			else
			{
				if (!String.IsNullOrEmpty(this.ddlExistingProjects.SelectedValue))
				{
					int existingProjectId = Int32.Parse(this.ddlExistingProjects.SelectedValue);
					projectTemplates = new List<DataModel.ProjectTemplate>() { templateManager.RetrieveForProject(existingProjectId) };
				}
			}
			this.ddlProjectTemplates.Items.Clear();

			this.ddlProjectTemplates.DataSource = projectTemplates;

			//Databind the dropdown
			this.ddlProjectTemplates.DataBind();

			//Add the 'new template' option
			this.ddlProjectTemplates.Items.Add(new ListItem() { Value = "", Text = Resources.Main.ProjectCreate_TemplateCreateNew });
		}

		/// <summary>
		/// Called when the project dropdown list is changed. Filters the list of templates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DdlExistingProjects_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadTemplates();
		}

		/// <summary>
		/// Validates the form, and inserts the new project
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
			Business.ProjectManager projectManager = new Business.ProjectManager();

			int newProjectId = 0;

			//See if we are using the default template or an existing project as the template
			if (this.radExistingProject.Checked && this.ddlExistingProjects.SelectedValue != "")
			{
				//The project group is not used in this case (taken from the existing project)

				//Get the id of the existing project and see if we should create a new template
				bool createNewTemplate = true;  //Create new template for project
				if (!String.IsNullOrEmpty(this.ddlProjectTemplates.SelectedValue))
				{
					createNewTemplate = false;
				}
				int existingProjectId = Int32.Parse(this.ddlExistingProjects.SelectedValue);

				//Call the create-from-existing project command
				newProjectId = projectManager.CreateFromExisting(
					GlobalFunctions.HtmlScrubInput(this.txtName.Text),
					GlobalFunctions.HtmlScrubInput(this.txtDescription.Text),
					GlobalFunctions.HtmlScrubInput(this.txtWebSite.Text),
					existingProjectId,
					(this.chkActiveYn.Checked),
					createNewTemplate
					);

				// set baselining using the new id just generated
				setBaselining(newProjectId);
			}
			else
			{
				//Get the project group id and template id
				int projectGroupId = Int32.Parse(this.ddlProjectGroup.SelectedValue);
				int? projectTemplateId = null;  //Create new template for project
				if (!String.IsNullOrEmpty(this.ddlProjectTemplates.SelectedValue))
				{
					projectTemplateId = Int32.Parse(this.ddlProjectTemplates.SelectedValue);
				}

				//Call the insert command
				newProjectId = projectManager.Insert(
					GlobalFunctions.HtmlScrubInput(this.txtName.Text),
					projectGroupId,
					GlobalFunctions.HtmlScrubInput(this.txtDescription.Text),
					GlobalFunctions.HtmlScrubInput(this.txtWebSite.Text),
					(this.chkActiveYn.Checked),
					projectTemplateId,
					UserId
					);

				// set baselining using the new id just generated
				setBaselining(newProjectId);
			}

			//Return to the admin home page
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Administration, newProjectId), true);

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

			Response.Redirect("ProjectList.aspx", true);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles changes to the project template radio button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void radProjectTemplate_CheckedChanged(object sender, EventArgs e)
		{
			const string METHOD_NAME = "radProjectTemplate_CheckedChanged";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Depending on what template is checked, enable or disable the project and project group section
			//Also only show templates for the selected project if based on existing
			if (this.radExistingProject.Checked)
			{
				this.plcExistingProjects.Visible = true;
				this.lblNewProject.Attributes["data-checked"] = "";
				this.lblExistingProject.Attributes["data-checked"] = "checked";
				this.plcProjectGroup.Visible = false;

			}
			else
			{
				this.plcExistingProjects.Visible = false;
				this.lblNewProject.Attributes["data-checked"] = "checked";
				this.lblExistingProject.Attributes["data-checked"] = "";
				this.plcProjectGroup.Visible = true;
			}

			//Update the templates
			LoadTemplates();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		private void setBaselining(int projectId)
		{
			// if baselining is enabled and has been turned on then set the project setting
			ProjectSettings projectSettings = new ProjectSettings(projectId);
			//Save the Project's Baseline setting.
			if (projectSettings != null)
			{
				projectSettings.BaseliningEnabled = Common.Global.Feature_Baselines && chkBaseline.Checked;
				projectSettings.Save(UserId);
			}
		}

		#endregion
	}
}
