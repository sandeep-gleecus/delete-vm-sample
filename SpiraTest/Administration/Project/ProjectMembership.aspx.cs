using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>
	/// Displays the admin page for managing project membership in the system
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProjectMembership_Title", "Product-Users/#product-membership", "Admin_ProjectMembership_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class ProjectMembership : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectMembership::";

		protected List<ProjectRole> projectRoles;

		/// <summary>
		/// Called when the page is first loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Reset the error messages
			lblMessage.Text = "";

			//Add the event handlers
			ddlFilterType.SelectedIndexChanged += new EventHandler(ddlFilterType_SelectedIndexChanged);
			btnProjectMembershipUpdate.Click += new DropMenuEventHandler(btnProjectMembershipUpdate_Click);
			btnProjectMembershipDelete.Click += new DropMenuEventHandler(btnProjectMembershipDelete_Click);
			btnProjectMembershipAdd.Click += new DropMenuEventHandler(btnProjectMembershipAdd_Click);
			grdUserMembership.RowDataBound += new GridViewRowEventHandler(grdUserMembership_RowDataBound);

			//Don't reload on Postback
			if (!IsPostBack)
			{
				//Load and bind data
				LoadAndBindData();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Selects the project role dropdown for each row in the grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdUserMembership_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Locate the datarow and the drop-down control
			ProjectUser projectUser = (ProjectUser)e.Row.DataItem;
			DropDownListEx ddlProjectRole = (DropDownListEx)e.Row.Cells[3].FindControl("ddlProjectRole");
			if (ddlProjectRole != null)
			{
				//Set the selected value on the drop-down
				try
				{
					ddlProjectRole.SelectedValue = projectUser.ProjectRoleId.ToString();
				}
				catch (ArgumentOutOfRangeException)
				{
					//Do nothing - in case the role was made inactive
				}
			}
		}

		/// <summary>
		/// Loads the project membership information
		/// </summary>
		protected void LoadAndBindData()
		{
			const string METHOD_NAME = "LoadAndBindData";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			if (ProjectId < 1)
			{
				Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject, true);
			}

			//Instantiate the business objects
			Business.ProjectManager projectManager = new Business.ProjectManager();

			//Retrieve the list of project roles
			projectRoles = projectManager.RetrieveProjectRoles(true);

			//Get the filter type for the active/all users dropdown
			string activeFilterType = ddlFilterType.SelectedValue;
			bool activeOnly = (activeFilterType == "allactive");

			//Retrieve the project user membership dataset
			List<ProjectUser> projectUsers = projectManager.RetrieveUserMembershipById(ProjectId, !activeOnly);
			grdUserMembership.DataSource = projectUsers;
			DataBind();

			//Populate any static fields
			lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
			lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the event raised when the project membership Update button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnProjectMembershipUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnProjectMembershipUpdate_Click()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//See if we have a project in session, if so retrieve the ID and name
			if (ProjectId > 0)
			{
				//Instantiate the business objects and capture the project id
				ProjectManager projectManager = new ProjectManager();

				//Figure out if we're 'Active Only'.
				string activeFilterType = ddlFilterType.SelectedValue;
				bool activeOnly = activeFilterType.Equals("allactive");

				//Retrieve the project user membership dataset and project role list lookup
				List<ProjectUser> projectUsers = projectManager.RetrieveUserMembershipById(ProjectId, !activeOnly);
				projectRoles = projectManager.RetrieveProjectRoles(true);

				//Iterate through the dataset and check the drop-downs to see what's changed
				for (int i = 0; i < projectUsers.Count; i++)
				{
					DropDownListEx projectRoleDropDown = (DropDownListEx)grdUserMembership.Rows[i].Cells[3].FindControl("ddlProjectRole");
					int newProjectRoleId = Int32.Parse(projectRoleDropDown.SelectedValue);

					//Check to see if changed, and update if so
					if (newProjectRoleId != projectUsers[i].ProjectRoleId)
					{
						projectUsers[i].StartTracking();
						projectUsers[i].ProjectRoleId = newProjectRoleId;
					}
				}

				//Update the bound dataset and reload (in case some roles were not allowed to be changed)
				projectManager.UpdateMembership(projectUsers);
				LoadAndBindData();

				//Display a confirmation message
				lblMessage.Text = Resources.Messages.Admin_ProjectMembership_Success;
				lblMessage.Type = MessageBox.MessageType.Information;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the event raised when the project membership Delete button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnProjectMembershipDelete_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnProjectMembershipDelete_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//See if we have a project in session, if so retrieve the ID and name
			if (ProjectId > 0)
			{
				//Instantiate the business objects and capture the project id
				Business.ProjectManager projectManager = new Business.ProjectManager();

                //Figure out if we're 'Active Only'.
                string activeFilterType = ddlFilterType.SelectedValue;
                bool activeOnly = activeFilterType.Equals("allactive");

                //Retrieve the project user membership dataset and project role list lookup
                List<ProjectUser> projectUsers = projectManager.RetrieveUserMembershipById(ProjectId, !activeOnly);
				projectRoles = projectManager.RetrieveProjectRoles(true);

				//Iterate through the dataset and check the drop-downs to see if any have their check-boxes checked
				for (int i = 0; i < projectUsers.Count; i++)
				{
					CheckBoxEx projectRoleCheckBox = (CheckBoxEx)grdUserMembership.Rows[i].Cells[0].FindControl("chkDeleteMembership");

					//Check to see if checked
					if (projectRoleCheckBox.Checked)
					{
						projectUsers[i].StartTracking();
						projectUsers[i].MarkAsDeleted();
					}
				}

				//Delete the items in the dataset and reload (in case some roles were not allowed to be changed)
				projectManager.UpdateMembership(projectUsers);
				LoadAndBindData();

				//Display a confirmation message
				lblMessage.Text = Resources.Messages.Admin_ProjectMembership_DeleteSuccess;
				lblMessage.Type = MessageBox.MessageType.Information;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the event raised when the project membership Add button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnProjectMembershipAdd_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnProjectMembershipAdd_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Redirect to the Add Membership page
			Response.Redirect("ProjectMembershipAdd.aspx");

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Changes the display of data
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
		{
			//Save the data and then reload
			LoadAndBindData();
		}
	}
}
