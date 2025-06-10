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

namespace Inflectra.SpiraTest.Web.Administration.Program
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Project Group Details Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProgramEditDetails_Title", "System-Workspaces/#viewedit-programs", "Admin_ProgramEditDetails_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.GroupOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class Edit : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.Program.Edit::";

        protected List<ProjectGroupRole> projectGroupRoles;
        protected ProjectGroup projectGroup;

        /// <summary>
        /// The URL to return to (depends on whether user is system admin or program admin)
        /// </summary>
        protected string ReturnUrl
        {
            get
            {
                if (UserIsAdmin)
                {
                    return "~/Administration/ProgramList.aspx";
                }
                else
                {
                    //Return to Program dashboard since there is no program admin dashboard currently
                    return UrlRoots.RetrieveGroupURL(UrlRoots.NavigationLinkEnum.ProjectGroupHome, ProjectGroupId.Value);
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

            //Reset the error message
            this.lblMessage.Text = "";

            //Make sure we have a project group id set
            //in theory should not happen unless someone spoofs the URL
            if (!ProjectGroupId.HasValue)
            {
                Response.Redirect(ReturnUrl, true);
                return;
            }

            //Set the back to list URL
            this.lnkBackToList.NavigateUrl = ReturnUrl;

            //Add the event handlers to the page
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.grdUserMembership.RowDataBound += new GridViewRowEventHandler(grdUserMembership_RowDataBound);
            this.btnMembershipAdd.Click += new DropMenuEventHandler(btnMembershipAdd_Click);
            this.btnMembershipDelete.Click += new DropMenuEventHandler(btnMembershipDelete_Click);
            this.btnMembershipSave.Click += new DropMenuEventHandler(btnMembershipSave_Click);
            this.grdProjectList.RowDataBound += GrdProjectList_RowDataBound;

            //Initially default to the user tab
            this.tclProjectGroupDetails.SelectedTab = pnlUsers.ClientID;

            //Only load the data once
            if (!IsPostBack)
            {
                //Instantiate the business classes
                ProjectGroupManager projectGroupManager = new ProjectGroupManager();

                //Retrieve the project group from the querystring if we have one (Edit/Update Mode)
                LoadProjectGroup();

                //Load the user membership
                LoadUserMembership();

                //Load the project list
                LoadProjectList();

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

                //Populate the form with the existing values (update case)
                this.lblProjectGroupName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(projectGroup.Name);
                this.txtName.Text = GlobalFunctions.HtmlRenderAsRichText(projectGroup.Name);
                this.txtDescription.Text = GlobalFunctions.HtmlRenderAsRichText(String.IsNullOrEmpty(projectGroup.Description) ? "" : projectGroup.Description);
                this.txtWebSite.Text = GlobalFunctions.HtmlRenderAsRichText(String.IsNullOrEmpty(projectGroup.Website) ? "" : projectGroup.Website);
                this.chkActiveYn.Checked = projectGroup.IsActive;
                this.chkDefaultYn.Checked = projectGroup.IsDefault;
                if (projectGroup.ProjectTemplateId.HasValue)
                {
                    try
                    {
                        this.ddlProjectTemplate.SelectedValue = projectGroup.ProjectTemplateId.Value.ToString();
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //Ignore, means value inactive
                    }
                }
                if (projectGroup.PortfolioId.HasValue)
                {
                    try
                    {
                        this.ddlPortfolio.SelectedValue = projectGroup.PortfolioId.Value.ToString();
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //Ignore, means value inactive
                    }
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Adds selective formatting to the project list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdProjectList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                ProjectView projectView = (ProjectView)e.Row.DataItem;
                //Change the color of the cell depending on the status
                if (projectView.IsActive)
                {
                    e.Row.Cells[4].CssClass = "bg-success";
                }
                else
                {
                    e.Row.Cells[4].CssClass = "bg-warning";
                }
            }
        }

        /// <summary>
        /// Gets the project group id from querystring and loads the main dataset if in edit mode
        /// </summary>
        protected void LoadProjectGroup()
        {
            //Retrieve the project group from the querystring
            Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();
            try
            {
                this.projectGroup = projectGroupManager.RetrieveById(ProjectGroupId.Value, true);
            }
            catch (ArtifactNotExistsException)
            {
            }
        }

        /// <summary>
        /// Updates the roles of users that are members of the group
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnMembershipSave_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnMembershipSave_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();

            //Retrieve the project group user membership list
            List<ProjectGroupUser> projectGroupUsers = projectGroupManager.RetrieveUserMembership(ProjectGroupId.Value);

            //Iterate through the dataset and check the drop-downs to see what's changed
            for (int i = 0; i < this.grdUserMembership.Rows.Count; i++)
            {
                DropDownListEx ddlProjectGroupRole = (DropDownListEx)this.grdUserMembership.Rows[i].Cells[3].FindControl("ddlProjectGroupRole");
                int newProjectGroupRoleId = Int32.Parse(ddlProjectGroupRole.SelectedValue);

                //Check to see if changed, and update if so
                int userId;
                if (Int32.TryParse(ddlProjectGroupRole.MetaData, out userId))
                {
                    ProjectGroupUser oldProjectGroupUser = projectGroupUsers.FirstOrDefault(p => p.UserId == userId);
                    if (oldProjectGroupUser != null && newProjectGroupRoleId != oldProjectGroupUser.ProjectGroupRoleId)
                    {
                        oldProjectGroupUser.StartTracking();
                        oldProjectGroupUser.MarkAsDeleted();

                        //Add a new item
                        ProjectGroupUser newProjectGroupUser = new ProjectGroupUser();
                        newProjectGroupUser.MarkAsAdded();
                        newProjectGroupUser.ProjectGroupId = ProjectGroupId.Value;
                        newProjectGroupUser.UserId = userId;
                        newProjectGroupUser.ProjectGroupRoleId = newProjectGroupRoleId;
                        projectGroupUsers.Add(newProjectGroupUser);
                    }
                }
            }

            //Update the bound dataset and reload (in case some roles were not allowed to be changed)
            projectGroupManager.SaveUserMembership(projectGroupUsers);
            LoadProjectGroup();
            LoadUserMembership();
            this.pnlUsers.DataBind();

            //Display a confirmation message
            this.lblMessage.Text = Resources.Messages.Admin_ProjectGroupDetails_MembershipUpdated;
            this.lblMessage.Type = MessageBox.MessageType.Information;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Removes any group members that have their checkboxes selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnMembershipDelete_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnMembershipDelete_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();

            //Retrieve the project group user membership dataset and project group role list lookup
            List<ProjectGroupUser> projectGroupUsers = projectGroupManager.RetrieveUserMembership(ProjectGroupId.Value);

            //Iterate through the dataset and check the drop-downs to see if any have their check-boxes checked
            for (int i = 0; i < this.grdUserMembership.Rows.Count; i++)
            {
                CheckBoxEx chkDeleteMembership = (CheckBoxEx)this.grdUserMembership.Rows[i].Cells[0].FindControl("chkDeleteMembership");

                //Check to see if checked
                int userId;
                if (chkDeleteMembership.Checked && Int32.TryParse(chkDeleteMembership.MetaData, out userId))
                {
                    ProjectGroupUser pgu = projectGroupUsers.FirstOrDefault(p => p.UserId == userId);
                    if (pgu != null)
                    {
                        pgu.MarkAsDeleted();
                    }
                }
            }

            //Delete the items in the dataset and reload (in case some roles were not allowed to be changed)
            projectGroupManager.SaveUserMembership(projectGroupUsers);
            LoadProjectGroup();
            LoadUserMembership();
            this.pnlUsers.DataBind();

            //Display a confirmation message
            this.lblMessage.Text = Resources.Messages.Admin_ProjectGroupDetails_MembershipRemoved;
            this.lblMessage.Type = MessageBox.MessageType.Information;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Redirects the user to the page that allows you to add new group members
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnMembershipAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnMembershipAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Get the project group id
            LoadProjectGroup();
            //Redirect to the Add Group Membership page
            Response.Redirect(UrlRoots.RetrieveGroupAdminUrl(ProjectGroupId.Value, "MembershipAdd"));

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Selects the project group role dropdown for each row in the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdUserMembership_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Locate the datarow and the drop-down control
            ProjectGroupUser projectGroupUser = (ProjectGroupUser)e.Row.DataItem;
            DropDownListEx ddlProjectGroupRole = (DropDownListEx)e.Row.Cells[5].FindControl("ddlProjectGroupRole");
            if (ddlProjectGroupRole != null)
            {
                //Set the selected value on the drop-down
                try
                {
                    ddlProjectGroupRole.SelectedValue = projectGroupUser.ProjectGroupRoleId.ToString();
                }
                catch (ArgumentOutOfRangeException)
                {
                    //Do nothing - in case the role was made inactive
                }
            }
        }

        /// <summary>
        /// Load the user membership
        /// </summary>
        protected void LoadUserMembership()
        {
            const string METHOD_NAME = "LoadUserMembership";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();

            //Retrieve the list of project roles
            this.projectGroupRoles = projectGroupManager.RetrieveRoles(true);

            //Retrieve the project user membership dataset
            this.grdUserMembership.DataSource = this.projectGroup.Users;
            
            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Load the project list
        /// </summary>
        protected void LoadProjectList()
        {
            const string METHOD_NAME = "LoadProjectList";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            ProjectManager projectManager = new ProjectManager();

            //Retrieve the project user membership dataset
            Hashtable filters = new Hashtable();
            filters.Add("ProjectGroupId", ProjectGroupId.Value);
            List<ProjectView> projects = projectManager.Retrieve(filters, null);
            this.grdProjectList.DataSource = projects;

            //Set the formats of the columns
            ((BoundFieldEx)this.grdProjectList.Columns[2]).DataFormatString = GlobalFunctions.FORMAT_DATE;
            ((BoundFieldEx)this.grdProjectList.Columns[5]).DataFormatString = GlobalFunctions.ARTIFACT_PREFIX_PROJECT + GlobalFunctions.FORMAT_ID;
            
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
            Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();

            //Retrieve the project group from the ID held in the querysting
            ProjectGroup projectGroup = projectGroupManager.RetrieveById(ProjectGroupId.Value);

            //Make the updates
            projectGroup.StartTracking();
            projectGroup.Name = GlobalFunctions.HtmlScrubInput(this.txtName.Text);
            if (this.txtDescription.Text == "")
            {
                projectGroup.Description = null;
            }
            else
            {
                projectGroup.Description = GlobalFunctions.HtmlScrubInput(this.txtDescription.Text);
            }
            if (this.txtWebSite.Text == "")
            {
                projectGroup.Website = null;
            }
            else
            {
                projectGroup.Website = GlobalFunctions.HtmlScrubInput(this.txtWebSite.Text);
            }

            if (String.IsNullOrEmpty(this.ddlProjectTemplate.SelectedValue))
            {
                projectGroup.ProjectTemplateId = null;
            }
            else
            {
                projectGroup.ProjectTemplateId = Int32.Parse(this.ddlProjectTemplate.SelectedValue);
            }

            if (String.IsNullOrEmpty(this.ddlPortfolio.SelectedValue))
            {
                projectGroup.PortfolioId = null;
            }
            else
            {
                projectGroup.PortfolioId = Int32.Parse(this.ddlPortfolio.SelectedValue);
            }

            //Check for certain business conditions
            if (!this.chkActiveYn.Checked && this.chkDefaultYn.Checked)
            {
                this.lblMessage.Text = Resources.Messages.Admin_ProjectGroupDetails_CannotMakeInactiveDefault;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }
            if (!this.chkDefaultYn.Checked && projectGroup.IsDefault)
            {
                this.lblMessage.Text = Resources.Messages.Admin_ProjectGroupDetails_NeedAtLeastOneDefault;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            projectGroup.IsActive = this.chkActiveYn.Checked;
            projectGroup.IsDefault = this.chkDefaultYn.Checked;
            try
            {
                projectGroupManager.Update(projectGroup);
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
            Response.Redirect(ReturnUrl, true);
        }
    }
}
