using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the admin page for approving/denying new user account requests
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "UserRequests_TitleLong", "System-Users/#pending-requests", "UserRequests_TitleLong"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class UserRequests : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.UserRequests::";

        /// <summary>
        /// Handles initialization that needs to come before Page_Onload is fired
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Configure the gridview filter column properties
            ((FilterSortFieldEx)this.grdUserManagement.Columns[4]).FilterLookupDataSourceID = this.srcActiveFlag.UniqueID;
            ((FilterSortFieldEx)this.grdUserManagement.Columns[5]).SubHeaderText = GlobalFunctions.ARTIFACT_PREFIX_USER;
            ((FilterSortFieldEx)this.grdUserManagement.Columns[5]).DataFormatString = GlobalFunctions.ARTIFACT_PREFIX_USER + GlobalFunctions.FORMAT_ID;
			((FilterSortFieldEx)this.grdUserManagement.Columns[6]).FilterLookupDataSourceID = this.srcActiveFlag.UniqueID;
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
            this.btnUserListFilter.Click += new DropMenuEventHandler(btnUserListFilter_Click);
            this.btnUserListClearFilters.Click += new DropMenuEventHandler(btnUserListClearFilters_Click);
            this.grdUserManagement.PageIndexChanging += new GridViewPageEventHandler(grdUserManagement_PageIndexChanging);
            this.grdUserManagement.RowCommand += new GridViewCommandEventHandler(grdUserManagement_RowCommand);

            //Load and bind data
            LoadAndBindData();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles the click event on the item links in the User Management datagrid
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void grdUserManagement_RowCommand(object source, GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdUserManagement_RowCommand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Handle the appropriate command
            if (e.CommandName == "ApproveUser")
            {
                SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;
                int userId = Int32.Parse((string)e.CommandArgument);
                provider.ApproveUser(userId);
                LoadAndBindData();
            }
            if (e.CommandName == "DeleteUser")
            {
                SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;
                int userId = Int32.Parse((string)e.CommandArgument);
                provider.DeleteUser(userId, true);
                LoadAndBindData();
            }
            if (e.CommandName == "SortColumns")
            {
                //Update the sort
                SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, e.CommandArgument.ToString());
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles the event raised when the user list pagination page index is changed
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
                this.grdUserManagement.CurrentPageIndex = e.NewPageIndex;
                UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_GENERAL);
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

        /// Handles the click event on the user list filter button
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnUserListFilter_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUserListFilter_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_FILTERS);
            filterList.Clear();
            this.grdUserManagement.CurrentPageIndex = 0;

            //First we need to scan the list of columns and then update the saved filters
            this.grdUserManagement.Filters = filterList;
            this.grdUserManagement.UpdateFilters();
            filterList.Save();

            //Reload user list and Databind
            LoadAndBindData();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// Handles the click event on the user list clear filters button
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnUserListClearFilters_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUserListClearFilters_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Clear the filters in stored settings
            UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_FILTERS);
            filterList.Clear();
            filterList.Save();
            UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_GENERAL);
            paginationSettings.Clear();
            paginationSettings.Save();

            //Reload user list and Databind
            LoadAndBindData();

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

            //Get the filter list as a local variable
            UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_FILTERS);

            //We only want unapproved users
            if (!filterList.ContainsKey("IsApproved"))
            {
                filterList["IsApproved"] = false;
            }

            //Get the current sort
            string sortExpression = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, "Profile.FirstName ASC");

            //Now we need to update the dataset and re-databind the grid and its lookups
            int totalCount;
            int pageIndex = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, 0);
            int pageSize = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE, 15);
            this.grdUserManagement.PageSize = pageSize;
            
            Business.UserManager userManager = new Business.UserManager();
            int itemIndex = pageIndex * this.grdUserManagement.PageSize;
            List<DataModel.User> users = userManager.GetUsers(filterList, sortExpression, itemIndex, this.grdUserManagement.PageSize, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out totalCount, true, true);
            this.grdUserManagement.DataSource = users;
            this.grdUserManagement.VirtualItemCount = totalCount;

            //Finally we need to populate the filter and sort display controls on the grid
            this.grdUserManagement.SortExpression = sortExpression;
            this.grdUserManagement.Filters = filterList;

            try
            {
                this.grdUserManagement.CurrentPageIndex = pageIndex;
                this.grdUserManagement.DataBind();
            }
            catch (Exception)
            {
                pageIndex = 0;
                this.grdUserManagement.CurrentPageIndex = pageIndex;
                SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, pageIndex);
                this.grdUserManagement.DataBind();
            }

            //Set the default button to the filter (when enter pressed)
            this.Form.DefaultButton = this.btnUserListFilter.UniqueID;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
    }
}
