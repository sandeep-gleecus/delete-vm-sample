using System;
using System.Linq;
using System.Data;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// Displays the admin page for managing project groups in the system
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProgramList_Title", "System-Workspaces/#viewedit-programs", "Admin_ProgramList_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class ProgramList : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProgramList::";

		/// <summary>
		/// Handles initialization that needs to come before Page_Onload is fired
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

            //Configure the gridview filter column properties
            ((FilterSortFieldEx)this.grdProjectGroups.Columns[2]).FilterLookupDataSourceID = this.srcActiveFlag.UniqueID;
			((FilterSortFieldEx)this.grdProjectGroups.Columns[3]).FilterLookupDataSourceID = this.srcActiveFlag.UniqueID;
            ((FilterSortFieldEx)this.grdProjectGroups.Columns[4]).FilterLookupDataSourceID = this.srcProjectTemplates.UniqueID;
            ((FilterSortFieldEx)this.grdProjectGroups.Columns[4]).NavigateUrlFormat = UrlRoots.RetrieveTemplateAdminUrl(-3, "Default");
            ((FilterSortFieldEx)this.grdProjectGroups.Columns[5]).SubHeaderText = GlobalFunctions.ARTIFACT_PREFIX_PROJECT_GROUP;
			((FilterSortFieldEx)this.grdProjectGroups.Columns[5]).DataFormatString = GlobalFunctions.ARTIFACT_PREFIX_PROJECT_GROUP + GlobalFunctions.FORMAT_ID;

            // only show and set portfolio column if the feature is enabled
            if (Common.Global.Feature_Portfolios)
            {
                this.grdProjectGroups.Columns[1].Visible = true;
                ((FilterSortFieldEx)this.grdProjectGroups.Columns[1]).FilterLookupDataSourceID = this.srcPortfolios.UniqueID;
                ((FilterSortFieldEx)this.grdProjectGroups.Columns[1]).NavigateUrlFormat = UrlRoots.RetrievePortfolioAdminUrl(-3, "Edit");
            }
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
            this.btnGroupListFilter.Click += new DropMenuEventHandler(btnGroupListFilter_Click);
            this.btnGroupListClearFilters.Click += new DropMenuEventHandler(btnGroupListClearFilters_Click);
			this.btnGroupListAdd.Click += new DropMenuEventHandler(btnGroupListAdd_Click);
			this.grdProjectGroups.PageIndexChanging += new GridViewPageEventHandler(grdProjectGroups_PageIndexChanging);
			this.grdProjectGroups.RowCommand += new GridViewCommandEventHandler(grdProjectGroups_RowCommand);
            this.grdProjectGroups.RowDataBound += GrdProjectGroups_RowDataBound;

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
        private void GrdProjectGroups_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                ProjectGroup projectGroup = (ProjectGroup)e.Row.DataItem;
                //Change the color of the cell depending on the status
                if (projectGroup.IsActive)
                {
                    e.Row.Cells[3].CssClass = "bg-success priority2";
                }
                else
                {
                    e.Row.Cells[3].CssClass = "bg-warning priority2";
                }
                if (projectGroup.IsDefault)
                {
                    e.Row.Cells[2].CssClass = "bg-info";
                }
            }
        }

        /// <summary>
        /// Loads the project group grid and populates the filter controls if necessary
        /// </summary>
        protected void LoadAndBindData()
		{
			const string METHOD_NAME = "LoadAndBindData";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Get the filter list as a local variable
			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_FILTERS);

			//Now we need to update the dataset and re-databind the grid and its lookups
			ProjectGroupManager projectGroupManager = new ProjectGroupManager();

            //Get the current sort
            string sortExpression = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");

            //Get the list of programs
            List<ProjectGroup> projectGroups = projectGroupManager.Retrieve(filterList, sortExpression, null);

            this.grdProjectGroups.DataSource = projectGroups;
			int startingIndex = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, 0);
            int pageSize = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE, 15);
            this.grdProjectGroups.PageSize = pageSize;
            this.grdProjectGroups.SortExpression = sortExpression;

			//Finally we need to repopulate the filter controls
			this.grdProjectGroups.Filters = filterList;

			try
			{
				this.grdProjectGroups.PageIndex = startingIndex;
				this.grdProjectGroups.DataBind();
			}
			catch (Exception)
			{
				startingIndex = 0;
				this.grdProjectGroups.PageIndex = startingIndex;
				SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, startingIndex);
				this.grdProjectGroups.DataBind();
			}

			//Set the default button to the filter (when enter pressed)
			this.Form.DefaultButton = this.btnGroupListFilter.UniqueID;

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
			return this.grdProjectGroups.SortExpression;
		}

		/// <summary>
		/// Handles the event raised when the project group Add button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnGroupListAdd_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnGroupListAdd_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Transfer control to the program create page
			Response.Redirect("ProgramCreate.aspx", true);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the click event on the item links in the Project Group grid
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void grdProjectGroups_RowCommand(object source, GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = "grdProjectGroups_RowCommand";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Handle the appropriate event
			if (e.CommandName == "DeleteRow")
			{
				//Delete this project group
				int projectGroupId = Int32.Parse((string)e.CommandArgument);
				ProjectGroupManager projectGroupManager = new ProjectGroupManager();
				try
				{
                    projectGroupManager.Delete(projectGroupId);
				}
				catch (ProjectGroupDefaultException)
				{
					//You cannot delete the default project group
					this.lblMessage.Text = Resources.Messages.Admin_ProjectGroups_CannotDeleteDefault;
					this.lblMessage.Type = MessageBox.MessageType.Error;
					return;
				}

				//Now reload project group list and Databind
				LoadAndBindData();
			}
			if (e.CommandName == "SortColumns")
			{
				//Update the sort
				SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, e.CommandArgument.ToString());
				LoadAndBindData();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the event raised when the project group list pagination page index is changed
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void grdProjectGroups_PageIndexChanging(object source, GridViewPageEventArgs e)
		{
			const string METHOD_NAME = "grdProjectGroups_PageIndexChanging";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Set the new page index
				this.grdProjectGroups.PageIndex = e.NewPageIndex;
				UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_PAGINATION);
				paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW] = e.NewPageIndex;
                paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE] = this.grdProjectGroups.PageSize;
				paginationSettings.Save();

				//Reload project group list and Databind
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
		/// Handles the click event on the project group list filter button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnGroupListFilter_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnGroupListFilter_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_FILTERS);
			filterList.Clear();
			this.grdProjectGroups.PageIndex = 0;

			//First we need to scan the list of columns and then update the saved filters
			this.grdProjectGroups.Filters = filterList;
			this.grdProjectGroups.UpdateFilters();
			filterList.Save();

			//Reload project group list and Databind
			LoadAndBindData();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the click event on the project group list clear filters button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnGroupListClearFilters_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnGroupListClearFilters_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Clear the filters in stored settings
			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_FILTERS);
			filterList.Clear();
			filterList.Save();
			UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_PAGINATION);
			paginationSettings.Clear();
			paginationSettings.Save();

			//Reload user list and Databind
			LoadAndBindData();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
	}
}
