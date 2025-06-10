using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using System.Collections.Generic;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
    /// <summary>
    /// Displays the admin page for adding new users to a project
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProjectMembershipAdd_Title", "Product-Users/#product-membership", "Admin_ProjectMembershipAdd_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class ProjectMembershipAdd : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectMembershipAdd::";

        protected List<ProjectRole> projectRoles;

        /// <summary>
        /// Handles initialization that needs to come before Page_Onload is fired
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Configure the gridview filter column properties
            ((FilterSortFieldEx)this.grdUserManagement.Columns[6]).SubHeaderText = GlobalFunctions.ARTIFACT_PREFIX_USER;
            ((FilterSortFieldEx)this.grdUserManagement.Columns[6]).DataFormatString = GlobalFunctions.ARTIFACT_PREFIX_USER + GlobalFunctions.FORMAT_ID;
        }

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
            this.lblMessage.Text = "";

            //Add the event handlers
            this.btnFilter.Click += new DropMenuEventHandler(btnFilter_Click);
            this.btnClearFilters.Click += new DropMenuEventHandler(btnClearFilters_Click);
            this.btnAdd.Click += new DropMenuEventHandler(btnAdd_Click);
            this.btnCancel.Click += new DropMenuEventHandler(btnCancel_Click);
            this.grdUserManagement.PageIndexChanging += new GridViewPageEventHandler(grdUserManagement_PageIndexChanging);
            this.grdUserManagement.RowCommand += new GridViewCommandEventHandler(grdUserManagement_RowCommand);

            //Don't release on Postback
            if (!IsPostBack)
            {
                //Load and bind data
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Loads the user management datagrid and populates the filter controls if necessary
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Get the filter list and sort as local variables
            UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_FILTERS);
            string sortExpression = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, "Profile.FirstName ASC");
            int startingIndex = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, 0);
            int pageSize = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE, 15);
            this.grdUserManagement.PageSize = pageSize;

            //Now we need to update the dataset and re-databind the grid and its lookups
            Business.UserManager userManager = new Business.UserManager();
            Business.ProjectManager projectManager = new Business.ProjectManager();
            projectRoles = projectManager.RetrieveProjectRoles(true);

            //Get users not members
            int totalCount;
            List<User> users = userManager.GetUsers(filterList, sortExpression, 0, Int32.MaxValue, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out totalCount, false, false, ProjectId);
            this.grdUserManagement.DataSource = users;

            //Finally we need to repopulate the filter and sort controls
            this.grdUserManagement.Filters = filterList;
            this.grdUserManagement.SortExpression = sortExpression;

            try
            {
                this.grdUserManagement.PageIndex = startingIndex;
                this.grdUserManagement.DataBind();
            }
            catch (Exception)
            {
                startingIndex = 0;
                this.grdUserManagement.PageIndex = startingIndex;
                SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, startingIndex);
                this.grdUserManagement.DataBind();
            }

            //Set the name of the project on the page
            this.lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

            //Set the back URL
            this.lnkBackToList.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "ProjectMembership");

            //Set the default button to the filter (when enter pressed)
            this.Form.DefaultButton = this.btnFilter.UniqueID;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Returns to the project membership page when clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnCancel_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Redirect back to the project membership page
            Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "ProjectMembership"));

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles the event raised when the Add button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate business objects
            Business.ProjectManager projectManager = new Business.ProjectManager();

            //Loop through the rows in the datagrid
            foreach (GridViewRow gridViewRow in this.grdUserManagement.Rows)
            {
                if (gridViewRow.RowType == DataControlRowType.DataRow)
                {
                    //Get a handle to the drop-down component
                    DropDownListEx ddlProjectRole = (DropDownListEx)gridViewRow.Cells[5].FindControl("ddlProjectRole");
                    if (ddlProjectRole != null && ddlProjectRole.SelectedValue != "")
                    {
                        int userId = Int32.Parse(ddlProjectRole.MetaData);
                        int projectRoleId = Int32.Parse(ddlProjectRole.SelectedValue);

                        try
                        {
                            projectManager.InsertUserMembership(userId, ProjectId, projectRoleId);
                        }
                        catch (ProjectDuplicateMembershipRecordException)
                        {
                            //In theory this can't happen, but you may have people adding users concurrently
                            this.lblMessage.Text = Resources.Messages.Admin_ProjectMembershipAdd_UserAlreadyMember;
                            this.lblMessage.Type = MessageBox.MessageType.Error;
                            return;
                        }
                    }
                }
            }

            //Finally return to the project membership list
            Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "ProjectMembership"));

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles the click event on the item links in the datagrid
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void grdUserManagement_RowCommand(object source, GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdUserManagement_RowCommand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Redirect to the appropriate page
            if (e.CommandName == "Edit")
            {
                Response.Redirect("~/Administration/UserDetailsEdit.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + e.CommandArgument);
            }
            if (e.CommandName == "SortColumns")
            {
                //Update the sort
                SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, e.CommandArgument.ToString());
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles the event raised when the pagination page index is changed
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void grdUserManagement_PageIndexChanging(object source, GridViewPageEventArgs e)
        {
            const string METHOD_NAME = "grdUserManagement_PageIndexChanging";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Set the new page index
                this.grdUserManagement.PageIndex = e.NewPageIndex;
                UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_PAGINATION);
                paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW] = e.NewPageIndex;
                paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE] = this.grdUserManagement.PageSize;
                paginationSettings.Save();

                //Reload user list and Databind
                LoadAndBindData();
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// Handles the click event on the filter button
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnFilter_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnFilter_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_FILTERS);
            filterList.Clear();
            this.grdUserManagement.PageIndex = 0;

            //First we need to scan the list of columns and then update the saved filters
            this.grdUserManagement.Filters = filterList;
            this.grdUserManagement.UpdateFilters();
            filterList.Save();

            //Reload user list and Databind
            LoadAndBindData();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// Handles the click event on the clear filters button
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnClearFilters_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Clear the filters in stored settings
            UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_FILTERS);
            filterList.Clear();
            filterList.Save();
            UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_PAGINATION);
            paginationSettings.Clear();
            paginationSettings.Save();

            //Reload user list and Databind
            LoadAndBindData();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
    }
}
