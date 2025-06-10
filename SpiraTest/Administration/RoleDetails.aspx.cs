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
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Project Role Details Page and handling all raised events
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "SiteMap_EditRoleDetails", "System-Users/#view-edit-product-roles", "SiteMap_EditRoleDetails"),
	AdministrationLevelAttribute(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class RoleDetails : AdministrationBase
	{

        protected ProjectRole projectRole;
		protected SortedList<string, string> flagList;
		protected List<ArtifactType> artifactTypes;
        protected List<Permission> permissionsList;
		protected int projectRoleId;

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.RoleDetails::";

		/// <summary>Adds the dynamic fields to the permission data-grid</summary>
		private void AddDynamicColumns()
		{
			//Get the list of permissions, that will be used for the columns
			Business.ProjectManager projectManager = new Business.ProjectManager();
			permissionsList = projectManager.RetrievePermissions();

			//Iterate through the dataset, adding the columns
            for (int i = 0; i < permissionsList.Count; i++)
			{
				Inflectra.SpiraTest.Web.ServerControls.CheckBoxFieldEx checkBoxColumn = new Inflectra.SpiraTest.Web.ServerControls.CheckBoxFieldEx();
				checkBoxColumn.ItemStyle.CssClass = "Centered";
				checkBoxColumn.HeaderStyle.CssClass = "Centered";
				checkBoxColumn.DataKeyValue = permissionsList[i].PermissionId.ToString();
				string permissionName = (string)permissionsList[i].Name;
				string localizedPermissionName = Resources.Main.ResourceManager.GetString(permissionName);
				if (String.IsNullOrEmpty(localizedPermissionName))
				{
					checkBoxColumn.HeaderText = permissionName;
				}
				else
				{
					checkBoxColumn.HeaderText = localizedPermissionName;
				}

				checkBoxColumn.MetaDataField = "ArtifactTypeId";
				this.grdRolePermissions.Columns.Add(checkBoxColumn);
			}
		}

		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Reset the error message
			this.lblMessage.Text = "";

			//Add the event handlers
			this.grdRolePermissions.RowDataBound += new GridViewRowEventHandler(grdRolePermissions_RowDataBound);
			this.btnCancel.Click += new EventHandler(btnCancel_Click);
			this.btnUpdate.Click += new EventHandler(btnUpdate_Click);

			//Only load the data once
			if (!IsPostBack)
			{
				//Instantiate the business class
				Business.ProjectManager projectManager = new Business.ProjectManager();
				ArtifactManager artifactManager = new ArtifactManager();

				//Retrieve the project role using the querystring to get the id
				this.projectRoleId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ROLE_ID]);
                projectRole = projectManager.RetrieveRolePermissions(projectRoleId);

				//Retrieve any lookups
				this.flagList = projectManager.RetrieveFlagLookup();
                artifactTypes = artifactManager.ArtifactType_RetrieveAll(onlyThoseThatSupportCustomProperties:true);
				this.grdRolePermissions.DataSource = artifactTypes;

				//Databind the form
				this.DataBind();

				//Set the title of the page and the other form values
                this.lblProjectRoleName.Text = GlobalFunctions.HtmlRenderAsPlainText(projectRole.Name);
                this.txtName.Text = GlobalFunctions.HtmlRenderAsRichText(projectRole.Name);
                this.txtDescription.Text = GlobalFunctions.HtmlRenderAsRichText(projectRole.Description);
                this.chkActiveYn.Checked = projectRole.IsActive;
                this.chkAdminYn.Checked = projectRole.IsAdmin;
                this.chkTemplateAdminYn.Checked = projectRole.IsTemplateAdmin;
                this.chkLimitedView.Checked = projectRole.IsLimitedView;

				//Set the static checkboxes use for the non-artifact based permissions
				this.chkDiscussionsAdd.Checked = projectRole.IsDiscussionsAdd;
				this.chkSourceCodeView.Checked = projectRole.IsSourceCodeView;
				this.chkSourceCodeEdit.Checked = projectRole.IsSourceCodeEdit;

				//Disable the controls if we're editing project admin..
				if (this.projectRoleId == 1)
				{
					this.txtName.Enabled = false;
					this.txtDescription.Enabled = false;
					this.chkAdminYn.Enabled = false;
                    this.chkLimitedView.Enabled = false;
                    this.chkActiveYn.Enabled = false;
                    this.chkTemplateAdminYn.Enabled = false;
					this.chkDiscussionsAdd.Enabled = false;
					this.chkSourceCodeEdit.Enabled = false;
					this.chkSourceCodeView.Enabled = false;
					this.btnUpdate.Visible = false;
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// This event handler handles clicks on the Update button
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void btnUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			//Retrieve the project role using the querystring to get the id
			Business.ProjectManager projectManager = new Business.ProjectManager();
			this.projectRoleId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ROLE_ID]);
			projectRole = projectManager.RetrieveRolePermissions(projectRoleId);

			//Set the project role values from the form, handling null correctly
            projectRole.StartTracking();
			projectRole.Name = GlobalFunctions.HtmlScrubInput(this.txtName.Text);
			projectRole.Description = GlobalFunctions.HtmlScrubInput(this.txtDescription.Text);

			//Update the Yes/No dropdown values
			projectRole.IsActive = (this.chkActiveYn.Checked);
            projectRole.IsAdmin = (this.chkAdminYn.Checked);
            projectRole.IsLimitedView = (this.chkLimitedView.Checked);
            projectRole.IsTemplateAdmin = this.chkTemplateAdminYn.Checked;

            //Update the non-artifact related checkboxes
            projectRole.IsDiscussionsAdd = this.chkDiscussionsAdd.Checked;
			projectRole.IsSourceCodeView = this.chkSourceCodeView.Checked;
			projectRole.IsSourceCodeEdit = this.chkSourceCodeEdit.Checked;

			//Now we need to iterate through the permissions matrix and capture the changes
			foreach (GridViewRow gvr in this.grdRolePermissions.Rows)
			{
				//Ignore headers, footers, etc.
				if (gvr.RowType == DataControlRowType.DataRow)
				{
					//Iterate through the columns
					for (int j = 0; j < this.grdRolePermissions.Columns.Count; j++)
					{
						//Only look at check-box columns
						if (this.grdRolePermissions.Columns[j].GetType() == typeof(CheckBoxFieldEx))
						{
							CheckBoxFieldEx checkBoxColumn = (CheckBoxFieldEx)this.grdRolePermissions.Columns[j];
							CheckBoxEx checkBox = (CheckBoxEx)gvr.Cells[j].Controls[0];
							int artifactTypeId = Int32.Parse(checkBox.MetaData);
							int permissionId = Int32.Parse(checkBoxColumn.DataKeyValue);

							//Try and locate this entry in the dataset
							ProjectRolePermission projectRolePermissionRow = projectRole.RolePermissions.FirstOrDefault(p => p.ProjectRoleId == projectRoleId && p.ArtifactTypeId == artifactTypeId && p.PermissionId == permissionId);

							if (checkBox.Checked)
							{
								//If we have a checked box, see if it's already in the dataset
								if (projectRolePermissionRow == null)
								{
                                    ProjectRolePermission newRolePermission = new ProjectRolePermission();
                                    newRolePermission.ProjectRoleId = projectRoleId;
                                    newRolePermission.ArtifactTypeId = artifactTypeId;
                                    newRolePermission.PermissionId = permissionId;
                                    projectRole.RolePermissions.Add(newRolePermission);
								}
							}
							else
							{
								//If we don't have the checked box, make sure it's not in the list
								if (projectRolePermissionRow != null)
								{
									projectRolePermissionRow.MarkAsDeleted();
								}
							}
						}
					}
				}
			}

			//Persist the changes
			try
			{
				projectManager.UpdateProjectRole(new List<ProjectRole>() { projectRole });
			}
			catch (ProjectRoleNotDeactivatableException)
			{
				//Let the user know that they cannot make this role inactive
				this.lblMessage.Text = Resources.Messages.Admin_RoleDetails_CannotDeactivateProjectOwner;
				this.lblMessage.Type = MessageBox.MessageType.Error;
				return;
			}

			//Return to the role list screen
			Response.Redirect("~/Administration/RoleList.aspx", true);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// This event handler handles clicks on the Cancel button
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void btnCancel_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnCancel_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			Response.Redirect("~/Administration/RoleList.aspx", true);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>This event handler sets the checkboxes' state during databinding</summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void grdRolePermissions_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Don't touch headers, footers or subheaders
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				//Iterate through each of the check-box columns
				for (int i = 0; i < this.grdRolePermissions.Columns.Count; i++)
				{
					if (this.grdRolePermissions.Columns[i].GetType() == typeof(CheckBoxFieldEx))
					{
						CheckBoxFieldEx checkBoxColumn = (CheckBoxFieldEx)this.grdRolePermissions.Columns[i];
						if (checkBoxColumn.DataKeyValue != "")
						{
							//If we have a permission row then select the check-box
							CheckBoxEx checkBox = (CheckBoxEx)e.Row.Cells[i].Controls[0];

							//Disable it, if we're editing the project owner, and mark it disabled if we are.
							if (projectRoleId == 1)
								checkBox.Enabled = false;

							int permissionId = Int32.Parse(checkBoxColumn.DataKeyValue);
							int artifactTypeId = (int)((ArtifactType)(e.Row.DataItem)).ArtifactTypeId;

							//Now see if this should be checked for this project role, artifact type, permission combination
                            checkBox.Checked = projectRole.RolePermissions.Any(p => p.ProjectRoleId == this.projectRoleId && p.ArtifactTypeId == artifactTypeId && p.PermissionId == permissionId);
						}
					}
				}
			}
		}

		override protected void OnInit(EventArgs e)
		{
			//Add the dynamic columns to the role permission datagrid
			//Needs to be added in the OnInit method since adding to the
			//Page_Load() method will cause events to not work correctly
			AddDynamicColumns();
			base.OnInit(e);
		}
	}
}
