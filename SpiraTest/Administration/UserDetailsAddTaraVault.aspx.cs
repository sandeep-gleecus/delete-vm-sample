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
    /// <summary>Displays the admin page for adding a user to TaraVault projects.</summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_UserDetailsAddProjectMembership_Title", "System-Users/#view-edit-users")]
    [AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
    public partial class UserDetailsAddTaraVault : AdministrationBase
    {
        private const string CLASS = "Web.Administration.UserDetailsAddTaraVault::";

        /// <summary>Loads the page</summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD = CLASS + "Page_Load()";
            Logger.LogEnteringEvent(METHOD);

            //Make sure we have a user id
            if (String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
                Response.Redirect("Default.aspx");
            int userId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);

            //Reset the error messages
            this.lblMessage.Text = "";

            //Add the event handlers
            this.btnSave.Click += this.btnSave_Click;
            this.btnBackToUserDetails.Click += this.btnMultiBack_Click;
            this.btnCancel.Click += this.btnMultiBack_Click;

            //Load the data..
            if (!IsPostBack)
            {
                LoadAndBindData(userId);

                //Add user avatar image
                this.imgUserAvatar.ImageUrl = UrlRewriterModule.ResolveUserAvatarUrl(userId, Page.Theme);
            }
            Logger.LogExitingEvent(CLASS + METHOD);
        }

        /// <summary>Loads the data on the page</summary>
        protected void LoadAndBindData(int userId)
        {
            const string METHOD = CLASS + "LoadAndBindData()";
            Logger.LogEnteringEvent(METHOD);

            //Get a list of all TaraVault-configured projects..
            List<DataModel.Project> allProjs = new VaultManager().Project_RetrieveTaraDefined();

            //Now, filter out projects that user is already defined for.
            List<DataModel.Project> projs = new List<DataModel.Project>();
            foreach (DataModel.Project proj in allProjs)
            {
                if (!proj.TaraVault.VaultUsers.Any(vu => vu.UserId == userId))
                    projs.Add(proj);
            }
            //Set the datasource and refresh it..
            this.grdProjectMembership.DataSource = projs;
            this.grdProjectMembership.DataBind();

            //Get User info for the label..
            //Set the name of the user on the page
            User user = new UserManager().GetUserById(userId);
            this.lblUserName.Text = user.FullName + " (" + user.UserName + ")";

            Logger.LogExitingEvent(CLASS + METHOD);
        }

        /// <summary>The user is cenceling action.</summary>
        /// <param name="sender">(Multiple)</param>
        /// <param name="e">EventArgs</param>
        void btnMultiBack_Click(object sender, EventArgs e)
        {
            BackToUserDetails();
        }

        /// <summary>Adds the selected projects to the user's membership</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnSave_Click(object sender, EventArgs e)
        {
            //Add the project(s) to user membership
            //Make sure we have a user id
            if (String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
                return;
            int userId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);

            //Loop through each row, and add the project if needed.
            VaultManager mgr = new VaultManager();
            foreach (GridViewRow gvr in this.grdProjectMembership.Rows)
            {
                //Only consider data rows that are visible
                if (gvr.RowType == DataControlRowType.DataRow)
                {
                    CheckBoxYnEx chkAdd = (CheckBoxYnEx)gvr.Cells[3].FindControl("chkAddProject");
                    if (chkAdd != null && chkAdd.Checked)
                    {
                        int projectId = (int)this.grdProjectMembership.DataKeys[gvr.RowIndex].Value;

                        //Add it with the manager..
                        mgr.User_AddToTaraVaultProject(userId, projectId);
                    }
                }
            }

            BackToUserDetails();
        }

        /// <summary>Returns back to the user details page</summary>
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