using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Administration
{
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "SiteMap_ViewEditRoles", "System-Users/#view-edit-product-roles", "SiteMap_ViewEditRoles"),
    AdministrationLevelAttribute(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class RoleList : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.RoleList::";

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Add the Event Landers
            this.grdProjectRolesList.RowCommand += new GridViewCommandEventHandler(grdProjectRolesList_RowCommand);
            this.btnRoleAdd.Click += new DropMenuEventHandler(btnRoleAdd_Click);

            //Only load the data once
            if (!IsPostBack)
            {
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Loads the role management information
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            Business.ProjectManager projectManager = new Business.ProjectManager();

            //Retrieve the list of all project roles
            List<ProjectRole> projectRoles = projectManager.RetrieveProjectRoles(false);
            this.grdProjectRolesList.DataSource = projectRoles;

            //Databind the datagrid
            this.grdProjectRolesList.DataBind();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles the click events on the item links in the Project Role List datagrid
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void grdProjectRolesList_RowCommand(object source, GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdProjectRolesList_ItemCommand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Handle the different commands
            if (e.CommandName == "DeleteRole")
            {
                int projectRoleId = Int32.Parse(e.CommandArgument.ToString());

                //Delete the role and re-databind
                Business.ProjectManager projectManager = new Business.ProjectManager();
                projectManager.DeleteProjectRole(projectRoleId);
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles the click events on the add project role button
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnRoleAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnRoleAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Create a new project with a dummy role
            Business.ProjectManager projectManager = new Business.ProjectManager();
            int newProjectRoleId = projectManager.InsertProjectRole(Resources.Main.Admin_RoleList_NewProjectRole, null, false, true, true, true, false, true, false);

            //Now redirect to this role
            Response.Redirect("RoleDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ROLE_ID + "=" + newProjectRoleId, true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
    }
}
