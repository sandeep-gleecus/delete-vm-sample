using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_TaraVault_UserList", "System-Administration", "Admin_TaraVault_UserList")]
    [AdministrationLevelAttribute(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator | AdministrationLevelAttribute.AdministrationLevels.ProjectOwner)]
    public partial class TaraVault_ProjectUsers : AdministrationBase
    {
        private const string CLASS_NAME = "Web.Administration.Project.TaraVault_ProjectUsers::";

        protected List<DataModel.User> projectUsers = null;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            this.projectUsers = new UserManager().GetUsersAssignedToTaraVaultProject(ProjectId);
        }

        /// <summary>Hit when the page is first loaded.</summary>
        /// <param name="sender">Page</param>
        /// <param name="e">eventArgs</param>
        protected void Page_Load(object sender, EventArgs e)
		{
			//Set page stuffs..
			this.ltrProjectName.Text = this.ProjectName;
			HyperLink lnk = (HyperLink)this.grdUserList.FindControlRecursive("lnkActUsers");
			if (lnk != null) lnk.Visible = this.UserIsAdmin;
            this.lnkBackToTaraVault.NavigateUrl = UrlRoots.RetrieveProjectAdminUrl(ProjectId, "TaraVaultProjectSettings");

            //Register event handlers
            this.grdProjectUsers.RowCommand += GrdProjectUsers_RowCommand;
            this.grdUserList.RowDataBound += GrdUserList_RowDataBound;
            this.grdUserList.RowCommand += GrdUserList_RowCommand;

            if (!this.IsPostBack)
				this.LoadAndBindData();
		}

        /// <summary>
        /// Adds the user to the project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdUserList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "AddUserToProject" && e.CommandArgument != null)
            {
                //Get the user id we're adding
                int userId = Int32.Parse((string)e.CommandArgument);
                VaultManager vaultManager = new VaultManager();
                vaultManager.User_AddToTaraVaultProject(userId, ProjectId);

                //Reload the grid
                this.projectUsers = new UserManager().GetUsersAssignedToTaraVaultProject(ProjectId);
                LoadAndBindData();
            }
        }

        /// <summary>
        /// Removes the 'add to project' button if the user is already a member of the current project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdUserList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && this.projectUsers != null)
            {
                DataModel.User user = (DataModel.User)e.Row.DataItem;
                if (user != null)
                {
                    if (this.projectUsers.Any(p => p.UserId == user.UserId))
                    {
                        LinkButtonEx btnAddUser = (LinkButtonEx)e.Row.FindControl("btnAddUser");
                        if (btnAddUser != null)
                        {
                            btnAddUser.Visible = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the user from the project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdProjectUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "RemoveUserFromProject" && e.CommandArgument != null)
            {
                //Get the user id we're removing
                int userId = Int32.Parse((string)e.CommandArgument);
                VaultManager vaultManager = new VaultManager();
                vaultManager.User_RemoveFromTaraVaultProject(userId, ProjectId);

                //Reload the grid
                this.projectUsers = new UserManager().GetUsersAssignedToTaraVaultProject(ProjectId);
                LoadAndBindData();
            }
        }

        /// <summary>Loads the table of users.</summary>
        private void LoadAndBindData()
		{
            //Load the TV users
            VaultManager vaultManager = new VaultManager();
            List<DataModel.User> allUsers = vaultManager.User_RetrieveAllTVActive();
            this.grdUserList.DataSource = allUsers;
            this.grdUserList.DataBind();

            //Load the TV users for this project
            grdProjectUsers.DataSource = this.projectUsers;
            grdProjectUsers.DataBind();
        }
    }
}