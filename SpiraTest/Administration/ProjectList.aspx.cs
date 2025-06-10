using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Page and handling all raised events
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_EditProjects_Title", "System-Workspaces/#viewedit-products", "Admin_EditProjects_Title")]
	[AdministrationLevelAttribute(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class ProjectList : AdministrationBase
	{
		protected SortedList<string, string> flagList;

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectList::";

		#region Event Handlers

		/// <summary>
		/// Handles initialization that needs to come before Page_Onload is fired
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			//Configure the gridview filter column properties
			((FilterSortFieldEx)this.grdProjectManagement.Columns[1]).FilterLookupDataSourceID = this.srcProjectGroups.UniqueID;
			((FilterSortFieldEx)this.grdProjectManagement.Columns[1]).NavigateUrlFormat = UrlRoots.RetrieveGroupAdminUrl(-3, "Edit");
			((FilterSortFieldEx)this.grdProjectManagement.Columns[2]).DataFormatString = GlobalFunctions.FORMAT_DATE;
			((FilterSortFieldEx)this.grdProjectManagement.Columns[3]).FilterLookupDataSourceID = this.srcProjectTemplates.UniqueID;
			((FilterSortFieldEx)this.grdProjectManagement.Columns[3]).NavigateUrlFormat = UrlRoots.RetrieveTemplateAdminUrl(-3, "Default");
			((FilterSortFieldEx)this.grdProjectManagement.Columns[4]).FilterLookupDataSourceID = this.srcActiveFlag.UniqueID;
			((FilterSortFieldEx)this.grdProjectManagement.Columns[5]).SubHeaderText = GlobalFunctions.ARTIFACT_PREFIX_PROJECT;
			((FilterSortFieldEx)this.grdProjectManagement.Columns[5]).DataFormatString = GlobalFunctions.ARTIFACT_PREFIX_PROJECT + GlobalFunctions.FORMAT_ID;
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

			//Reset the error messages
			this.lblMessage.Text = "";

			//Add the client event handler to the background task process
			Dictionary<string, string> handlers = new Dictionary<string, string>();
			handlers.Add("succeeded", "ajxBackgroundProcessManager_success");
			this.ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

			//Add the event handlers

			//Project Management List
			this.btnProjectListFilter.Click += new DropMenuEventHandler(btnProjectListFilter_Click);
			this.btnProjectListClearFilters.Click += new DropMenuEventHandler(btnProjectListClearFilters_Click);
			this.btnProjectListAdd.Click += new DropMenuEventHandler(btnProjectListAdd_Click);
			this.grdProjectManagement.PageIndexChanging += new GridViewPageEventHandler(grdProjectManagement_PageIndexChanging);
			this.grdProjectManagement.RowCommand += new GridViewCommandEventHandler(grdProjectManagement_RowCommand);
			this.grdProjectManagement.RowDataBound += new GridViewRowEventHandler(grdProjectManagement_RowDataBound);

			//Enable the span for TV Delete..
			ltlDeleteHasTV.Visible = (!ConfigurationSettings.Default.TaraVault_HasAccount && ConfigurationSettings.Default.TaraVault_UserLicense > 0 && Common.Global.Feature_TaraVault);

			//Only load the data once
			if (!IsPostBack)
			{
				//Instantiate the business objects
				Business.ProjectManager project = new Business.ProjectManager();

				//Retrieve the yes/no flag list
				this.flagList = project.RetrieveFlagLookup();

				LoadAndBind();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Highlights the row in the grid that matches the current project, and colors the status field
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdProjectManagement_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
			{
				ProjectView projectView = (ProjectView)e.Row.DataItem;
				//Change the color of the cell depending on the status
				if (projectView.IsActive)
				{
					e.Row.Cells[4].CssClass = "bg-success priority2";
				}
				else
				{
					e.Row.Cells[4].CssClass = "bg-warning priority2";
				}
				if (ProjectId > 0)
				{
					if (projectView.ProjectId == ProjectId)
					{
						e.Row.RowState = DataControlRowState.Selected;
					}
					else
					{
						e.Row.RowState = DataControlRowState.Normal;
					}
				}
			}
		}

		/// <summary>
		/// Called when the pagination buttons on the project management gridview are clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdProjectManagement_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			const string METHOD_NAME = "grdProjectManagement_PageIndexChanged";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Set the new page index
				this.grdProjectManagement.PageIndex = e.NewPageIndex;
				UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_PAGINATION);
				paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW] = e.NewPageIndex;
				paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE] = this.grdProjectManagement.PageSize;
				paginationSettings.Save();

				//Reload user list and Databind
				LoadAndBind();
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

		/// <summary>Handles the click events on the item links in the Project Management datagrid</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void grdProjectManagement_RowCommand(object source, GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = "grdProjectManagement_RowCommand";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Determine which command was clicked
			if (e.CommandName == "Select")
			{
				//Change the currently selected project
				//Simply redirect to the admin home page for this project
				int projectId = Int32.Parse((string)e.CommandArgument);
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Administration, projectId), true);
			}
			if (e.CommandName == "SortColumns")
			{
				//Update the sort
				SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, e.CommandArgument.ToString());
				LoadAndBind();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// Handles the click event on the project list filter button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnProjectListFilter_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnProjectListFilter_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_FILTERS);
			filterList.Clear();

			//Make sure that any date fields are in the correct format
			DateControl creationDateTextBox = (DateControl)((GridViewRow)this.grdProjectManagement.Controls[0].Controls[1]).Cells[2].FindControl("datCreationDateFilter");
			if (!GlobalFunctions.IsValidDate(creationDateTextBox.Text) && creationDateTextBox.Text != "")
			{
				lblMessage.Text = String.Format(Resources.Messages.Global_EnterValidDateValue, Resources.Fields.CreationDate);
				return;
			}

			//First we need to scan the list of columns and then update the saved filters
			this.grdProjectManagement.Filters = filterList;
			this.grdProjectManagement.UpdateFilters();
			filterList.Save();

			//Reload project list and Databind
			LoadAndBind();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// Handles the click event on the project list clear filters button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnProjectListClearFilters_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnProjectListClearFilters_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Clear the filters and pagination in stored settings
			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_FILTERS);
			filterList.Clear();
			filterList.Save();
			UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_PAGINATION);
			paginationSettings.Clear();
			paginationSettings.Save();

			//Reload project list and Databind
			LoadAndBind();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>Handles the event raised when the project management Add button is clicked</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnProjectListAdd_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "btnProjectListAdd_Click(obj,ImgEvtArgs)";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Transfer control to the project creation page
			Response.Redirect("~/Administration/ProjectCreate.aspx", true);

			Logger.LogExitingEvent(METHOD_NAME);
		}

		#endregion

		#region Functions Used Internally to the web form

		/// <summary>
		/// Loads the project management datagrid and populates the filter controls if necessary
		/// </summary>
		protected void LoadAndBind()
		{
			const string METHOD_NAME = "LoadAndBind";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Get the filter list as a local variable
			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_FILTERS);

			//Now we need to update the list and re-databind the grid and its lookups
			Business.ProjectManager project = new Business.ProjectManager();
			List<ProjectView> projectList = project.Retrieve(filterList, null);
			int startingIndex = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, 0);
			int pageSize = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE, 15);
			this.grdProjectManagement.PageSize = pageSize;

			//Get the current sort and apply using LINQ generic sorter
			GenericSorter<ProjectView> projectSorter = new GenericSorter<ProjectView>();
			string sortExpression = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");
			string[] sortElements = sortExpression.Split(' ');
			if (sortElements.Length > 1)
			{
				string sortBy = sortElements[0];
				bool sortAscending = (sortElements[1] == "ASC");
				projectList = (List<ProjectView>)projectSorter.Sort(projectList, sortBy, sortAscending);
			}
			else
			{
				projectList = (List<ProjectView>)projectSorter.Sort(projectList, sortExpression);
			}
			this.grdProjectManagement.DataSource = projectList;
			this.grdProjectManagement.SortExpression = sortExpression;

			//Finally we need to repopulate the filter controls
			this.grdProjectManagement.Filters = filterList;

			try
			{
				this.grdProjectManagement.PageIndex = startingIndex;
				this.grdProjectManagement.DataBind();
			}
			catch (Exception)
			{
				startingIndex = 0;
				//this.grdProjectManagement.CurrentPageIndex = startingIndex;
				SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, startingIndex);
				this.grdProjectManagement.DataBind();
			}

			//Finally set the ENTER key to fire the filter button
			this.Form.DefaultButton = this.btnProjectListFilter.UniqueID;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion
	}
}