using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the admin page for managing project templates in the system
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ViewEditProjectTemplates", "System-Workspaces/#viewedit-templates", "Admin_ViewEditProjectTemplates"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class ProjectTemplateList : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplateList::";

        /// <summary>
        /// Handles initialization that needs to come before Page_Onload is fired
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Configure the gridview filter column properties
            ((FilterSortFieldEx)this.grdProjectTemplates.Columns[1]).FilterLookupDataSourceID = this.srcActiveFlag.UniqueID;
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
            this.grdProjectTemplates.PageIndexChanging += new GridViewPageEventHandler(grdProjectTemplates_PageIndexChanging);
            this.grdProjectTemplates.RowCommand += new GridViewCommandEventHandler(grdProjectTemplates_RowCommand);
            this.grdProjectTemplates.RowDataBound += GrdProjectTemplates_RowDataBound;

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
        /// Adds row-specific formatting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdProjectTemplates_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                DataModel.ProjectTemplate projectGroup = (DataModel.ProjectTemplate)e.Row.DataItem;
                //Change the color of the cell depending on the status
                if (projectGroup.IsActive)
                {
                    e.Row.Cells[1].CssClass = "bg-success priority2";
                }
                else
                {
                    e.Row.Cells[1].CssClass = "bg-warning priority2";
                }
            }
        }

        /// <summary>
        /// Loads the project template grid and populates the filter controls if necessary
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Get the filter list as a local variable
            UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_FILTERS);

            //Now we need to update the dataset and re-databind the grid and its lookups
            TemplateManager templateManager = new TemplateManager();

            //Get the current sort
            string sortExpression = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");

            //Get the list of project templates
            List<DataModel.ProjectTemplate> projectGroups = templateManager.Retrieve(filterList, sortExpression);

            this.grdProjectTemplates.DataSource = projectGroups;
            int startingIndex = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, 0);
            int pageSize = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE, 15);
            this.grdProjectTemplates.PageSize = pageSize;
            this.grdProjectTemplates.SortExpression = sortExpression;

            //Finally we need to repopulate the filter controls
            this.grdProjectTemplates.Filters = filterList;

            try
            {
                this.grdProjectTemplates.PageIndex = startingIndex;
                this.grdProjectTemplates.DataBind();
            }
            catch (Exception)
            {
                startingIndex = 0;
                this.grdProjectTemplates.PageIndex = startingIndex;
                SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, startingIndex);
                this.grdProjectTemplates.DataBind();
            }

            //Set the default button to the filter (when enter pressed)
            this.Form.DefaultButton = this.btnFilter.UniqueID;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Returns the current sort expression
        /// </summary>
        /// <returns>The sort column and direction</returns>
        /// <remarks>Used from the actual ASPX page</remarks>
        protected string GetCurrentSortExpression()
        {
            return this.grdProjectTemplates.SortExpression;
        }

        /// <summary>
        /// Handles the event raised when the project template Add button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Transfer control to the project template create page
            //DISABLED IN 6.0 due to permissions not properly being in place yet
            //Response.Redirect("ProjectTemplateCreate.aspx", true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles the click event on the item links in the Project Group grid
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void grdProjectTemplates_RowCommand(object source, GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdProjectTemplates_RowCommand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Handle the appropriate event
            if (e.CommandName == "DeleteRow")
            {
                //Delete this project template
                int projectTemplateId = Int32.Parse((string)e.CommandArgument);
                TemplateManager templateManager = new TemplateManager();
                try
                {
                    templateManager.Delete(UserId, projectTemplateId);
                }
                catch (ProjectTemplateInUseException)
                {
                    //You cannot delete the default project template
                    this.lblMessage.Text = Resources.Messages.Admin_ProjectTemplates_CannotDeleteInUse;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                }

                //Now reload project template list and Databind
                LoadAndBindData();
            }
            if (e.CommandName == "SortColumns")
            {
                //Update the sort
                SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, e.CommandArgument.ToString());
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles the event raised when the project template list pagination page index is changed
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void grdProjectTemplates_PageIndexChanging(object source, GridViewPageEventArgs e)
        {
            const string METHOD_NAME = "grdProjectTemplates_PageIndexChanging";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Set the new page index
                this.grdProjectTemplates.PageIndex = e.NewPageIndex;
                UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_PAGINATION);
                paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW] = e.NewPageIndex;
                paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE] = this.grdProjectTemplates.PageSize;
                paginationSettings.Save();

                //Reload project template list and Databind
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
        /// <summary>
        /// Handles the click event on the project template list filter button
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnFilter_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnFilter_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_FILTERS);
            filterList.Clear();
            this.grdProjectTemplates.PageIndex = 0;

            //First we need to scan the list of columns and then update the saved filters
            this.grdProjectTemplates.Filters = filterList;
            this.grdProjectTemplates.UpdateFilters();
            filterList.Save();

            //Reload project template list and Databind
            LoadAndBindData();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles the click event on the project template list clear filters button
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnClearFilters_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Clear the filters in stored settings
            UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_FILTERS);
            filterList.Clear();
            filterList.Save();
            UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_PAGINATION);
            paginationSettings.Clear();
            paginationSettings.Save();

            //Reload user list and Databind
            LoadAndBindData();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
    }
}