using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using System.Collections;
using System.Web.Security;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the admin page for adding new project membership to an existing user
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_UserDetailsAddProjectMembership_Title", "System-Users/#view-edit-users"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class UserDetailsAddProjectMembership : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.UserDetailsAddProjectMembership::";

        protected List<ProjectRole> projectRoles;
        protected List<ProjectUser> projectMembership;

        /// <summary>
        /// Loads the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            //Make sure we have a user id
            if (String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
            {
                //Admin home
                Response.Redirect("Default.aspx");
            }
            int userId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);

            //Reset the error messages
            this.lblMessage.Text = "";

            //Add the event handlers
            this.btnAdd.Click += new DropMenuEventHandler(btnAdd_Click);
            this.btnBackToUserDetails.Click += new EventHandler(btnBackToUserDetails_Click);
            this.btnCancel.Click += new DropMenuEventHandler(btnCancel_Click);
            this.grdProjectMembership.RowDataBound += new GridViewRowEventHandler(grdProjectMembership_RowDataBound);

            //Load the data once
            if (!IsPostBack)
            {
                //Load and bind data
                LoadAndBindData(userId);
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Removes any projects that the user is already a member of
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdProjectMembership_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Make sure we have a user id
            if (String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
            {
                //Admin home
                Response.Redirect("Default.aspx");
            }
            int userId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Get the data item
                ProjectView projectView = (ProjectView)e.Row.DataItem;

                //See if we're already a member
                if (this.projectMembership.Any(p => p.ProjectId == projectView.ProjectId && p.UserId == userId))
                {
                    //Hide the row
                    e.Row.Visible = false;
                }
            }
        }

        /// <summary>
        /// Loads the data on the page
        /// </summary>
        protected void LoadAndBindData(int userId)
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            Business.UserManager userManager = new Business.UserManager();
            ProjectManager projectManager = new ProjectManager();

            //Load the lookups
            this.projectRoles = projectManager.RetrieveProjectRoles(true);

            //Get the existing membership for the user
            this.projectMembership = projectManager.RetrieveProjectMembershipForUser(userId);

            //Now we need to get the list of active projects 
            Hashtable filters = new Hashtable();
            filters.Add("IsActive", true);
            List<ProjectView> projects = projectManager.Retrieve(filters, null);
            this.grdProjectMembership.DataSource = projects;
            this.grdProjectMembership.DataBind();            

            //Set the name of the user on the page
            DataModel.User user = userManager.GetUserById(userId);
            this.lblUserName.Text = user.FullName;

            //Set the default button to add (when enter pressed)
            this.Form.DefaultButton = this.btnAdd.UniqueID;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            BackToUserDetails();
        }

        void btnBackToUserDetails_Click(object sender, EventArgs e)
        {
            BackToUserDetails();
        }

        /// <summary>
        /// Adds the selected projects to the user's membership
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAdd_Click(object sender, EventArgs e)
        {
            //Add the project(s) to user membership
            //Make sure we have a user id
            if (String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
            {
                return;
            }
            int userId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);

            ProjectManager projectManager = new ProjectManager();
            //Iterate through the grid and add the appropriate projects
            foreach (GridViewRow gvr in this.grdProjectMembership.Rows)
            {
                //Only consider data rows that are visible
                if (gvr.RowType == DataControlRowType.DataRow && gvr.Visible)
                {
                    DropDownListEx dropDownList = (DropDownListEx)gvr.Cells[5].FindControl("ddlProjectRole");
                    if (dropDownList != null && !String.IsNullOrEmpty(dropDownList.SelectedValue))
                    {
                        int projectId = (int)this.grdProjectMembership.DataKeys[gvr.RowIndex].Value;
                        int projectRoleId = Int32.Parse(dropDownList.SelectedValue);

                        //Add the membership record
                        projectManager.InsertUserMembership(userId, projectId, projectRoleId);
                    }
                }
            }

            BackToUserDetails();
        }

        /// <summary>
        /// Returns back to the user details page
        /// </summary>
        protected void BackToUserDetails()
        {
            //Make sure we have a user id
            if (String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
            {
                //Admin home
                Response.Redirect("Default.aspx");
            }
            int userId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);
            Response.Redirect("UserDetailsEdit.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + userId, true);
        }
    }
}