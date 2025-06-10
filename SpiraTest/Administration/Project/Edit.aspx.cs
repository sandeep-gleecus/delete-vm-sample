using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Project Edit Page and handling all raised events
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProjectDetails_Title", "System-Workspaces/#viewedit-products", "Admin_ProjectDetails_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class Edit : AdministrationBase
	{
		protected SortedList<string, string> flagList;
		protected List<ProjectView> existingProjectList;
		private ProjectSettings projectSettings = null;


		private const string CLASS_NAME = "Web.Administration.Project.Edit::";

		#region Event Handlers

		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Reset the error message
			lblMessage.Text = "";

			//Get the ProjectSettings collection.
			if (ProjectId > 0)
				projectSettings = new ProjectSettings(ProjectId);

			//Add the event handlers to the page
			btnUpdate.Click += btnUpdate_Click;
			btnCancel.Click += btnCancel_Click;

			//Hide the Baselining switch, if this feature is not enabled
			grpBaseline.Visible = Common.Global.Feature_Baselines;

			//Add the client event handler to the background task process
			Dictionary<string, string> handlers = new Dictionary<string, string>();
			handlers.Add("succeeded", "ajxBackgroundProcessManager_success");
			ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

			//Set the url for the back button (depends on whether you are a project or system admin)
			if (UserIsAdmin)
			{
				lnkBackToList.NavigateUrl = "~/Administration/ProjectList.aspx";
				txtBackToList.Text = Resources.Main.Admin_ProjectDetails_BackToList;
			}
			else
			{
				lnkBackToList.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");
				txtBackToList.Text = Resources.Main.Admin_ProductDetails_BackToHome;
			}

			//Only load the data once
			if (!IsPostBack)
			{
				//Instantiate the business classes
				ProjectManager projectManager = new ProjectManager();
				ProjectGroupManager projectGroupManager = new ProjectGroupManager();
				TemplateManager templateManager = new TemplateManager();

				//Retrieve the project by its id
				DataModel.Project project;
				try
				{
					project = projectManager.RetrieveById(ProjectId);
				}
				catch (ArtifactNotExistsException)
				{
					//Project does not exist any more
					Response.Redirect("~/Administration/ProjectList.aspx", true);
					return;
				}

				//Retrieve any lookups
				flagList = projectManager.RetrieveFlagLookup();
				List<ProjectGroup> projectGroups = projectGroupManager.RetrieveActive();
				ddlProjectGroup.DataSource = projectGroups;
				DataModel.ProjectTemplate projectTemplate = templateManager.RetrieveById(project.ProjectTemplateId);
				List<DataModel.ProjectTemplate> activeTemplates = templateManager.RetrieveActive();

				//Process the list of templates - remove current template, and change the name to include the ID to make it easier to read
				activeTemplates.RemoveAll(template => template.ProjectTemplateId == project.ProjectTemplateId);
				List<DataModel.ProjectTemplate> displayTemplates = new List<DataModel.ProjectTemplate>();
				displayTemplates = activeTemplates.Select(template =>
				{
					template.Name = template.Name + " [PT:" + template.ProjectTemplateId + "]";
					return template;
				}).ToList();
				ddlTemplates.DataSource = displayTemplates;

				//Load the Baseling (and other future settigns).
				if (projectSettings != null)
				{
					this.chkBaseline.Checked = projectSettings.BaseliningEnabled;
					this.chkSearchNameAndDescription.Checked = projectSettings.FilterNameAndDescription;
				}

				//Databind the form
				DataBind();

				//Populate the form with the existing values (update case)
				lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(project.Name);
				txtName.Text = GlobalFunctions.HtmlRenderAsRichText(project.Name);
				dlgChangeTemplateProductName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(project.Name);
				txtDescription.Text = GlobalFunctions.HtmlRenderAsRichText(project.Description);
				txtWebSite.Text = GlobalFunctions.HtmlRenderAsRichText(project.Website);
				chkActiveYn.Checked = project.IsActive;
				ddlProjectGroup.SelectedValue = project.ProjectGroupId.ToString();
				txtProjectTemplate.Text = Microsoft.Security.Application.Encoder.HtmlEncode(projectTemplate.Name);
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Validates the form, and updates the project record</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "btnUpdate_Click()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!IsValid)
				return;

			//Instantiate the business class
			ProjectManager projectManager = new Business.ProjectManager();

			//Retrieve the project from the ID held in the querysting
			int projectId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]);
			DataModel.Project project = projectManager.RetrieveById(projectId);

			//Make the updates
			project.StartTracking();
			project.Name = GlobalFunctions.HtmlScrubInput(txtName.Text);
			if (string.IsNullOrEmpty(txtDescription.Text))
			{
				project.Description = null;
			}
			else
			{
				project.Description = GlobalFunctions.HtmlScrubInput(txtDescription.Text);
			}
			if (string.IsNullOrEmpty(txtWebSite.Text))
			{
				project.Website = null;
			}
			else
			{
				project.Website = GlobalFunctions.HtmlScrubInput(txtWebSite.Text);
			}

			bool ProjectWasActive = project.IsActive;
			bool ProjectIsNowActive = (chkActiveYn.Checked);

			project.ProjectGroupId = int.Parse(ddlProjectGroup.SelectedValue);
			project.IsActive = ProjectIsNowActive;
			projectManager.Update(project);

			//Save the Project's settings.
			if (projectSettings != null)
			{
				projectSettings.BaseliningEnabled = Common.Global.Feature_Baselines && chkBaseline.Checked;
				projectSettings.FilterNameAndDescription = this.chkSearchNameAndDescription.Checked;
				projectSettings.Save(UserId);
			}

			//Now check to make sure if the template has now become now active, that its template is / is made active
			//First check if the project was inactive but has now been switched to active
			if (!ProjectWasActive && ProjectIsNowActive)
			{
				TemplateManager templateManager = new TemplateManager();
				DataModel.ProjectTemplate projectTemplate = templateManager.RetrieveById(project.ProjectTemplateId);

				//If the project's template is currently inactive, we need to make it active
				if (!projectTemplate.IsActive)
				{
					//Make the updates
					projectTemplate.StartTracking();
					projectTemplate.IsActive = true;
					templateManager.Update(projectTemplate);
				}
			}

			//Now we need to force the context to update this info if we are logged in to this project
			SpiraContext.Current.ProjectName = project.Name;

			//Redirect back to Admin home page / project list
			if (UserIsAdmin)
			{
				Response.Redirect("~/Administration/ProjectList.aspx", true);
			}
			else
			{
				Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default"), true);
			}

			Logger.LogExitingEvent(METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>Redirects the user back to the administration home page when cancel clicked</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnCancel_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "btnCancel_Click()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (UserIsAdmin)
			{
				Response.Redirect("~/Administration/ProjectList.aspx", true);
			}
			else
			{
				Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default"), true);
			}

			Logger.LogExitingEvent(METHOD_NAME);
			Logger.Flush();
		}

		#endregion
	}
}
