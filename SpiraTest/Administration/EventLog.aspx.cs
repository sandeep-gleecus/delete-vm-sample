using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// Displays the admin page for viewing a list of logged system events
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "EventLog_TitleLong", "System/#event-log", "EventLog_TitleLong"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class EventLog : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.EventLog::";

		/// <summary>
		/// Handles initialization that needs to come before Page_Onload is fired
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			//Configure the gridview filter column properties
			((FilterSortFieldEx)grdEventLog.Columns[1]).FilterLookupDataSourceID = srcEventLogEntryTypes.UniqueID;
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
			lblMessage.Text = "";

			//Add the event handlers
			btnFilter.Click += new DropMenuEventHandler(btnFilter_Click);
			btnClearFilters.Click += new DropMenuEventHandler(btnClearFilters_Click);
			btnClearLog.Click += new DropMenuEventHandler(btnClearLog_Click);
			grdEventLog.PageIndexChanging += new GridViewPageEventHandler(grdEventLog_PageIndexChanging);
			grdEventLog.RowCommand += new GridViewCommandEventHandler(grdEventLog_RowCommand);
			grdEventLog.RowDataBound += new GridViewRowEventHandler(grdEventLog_RowDataBound);
			btnRefresh.Click += new EventHandler(btnRefresh_Click);

			//Load and bind data
			LoadAndBindData();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		void btnRefresh_Click(object sender, EventArgs e)
		{
			//Reload the data
			LoadAndBindData();
		}

		/// <summary>
		/// Clears the event log
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnClearLog_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnClearLog_Click";

			try
			{
				EventManager eventManager = new EventManager();
				eventManager.Clear(UserName);
				lblMessage.Text = Resources.Messages.EventLog_ClearLogSuccess;
				lblMessage.Type = MessageBox.MessageType.Information;
				LoadAndBindData();
			}
			catch (Exception exception)
			{
				lblMessage.Text = Resources.Messages.EventLog_ClearLogFailure;
				lblMessage.Type = MessageBox.MessageType.Error;
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}

		/// <summary>
		/// Adds coloration to the events in the event log grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdEventLog_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Make sure we have a data row
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				Event evt = (Event)e.Row.DataItem;
				string cssClass = "";
				switch ((EventLogEntryType)evt.EventTypeId)
				{
					case EventLogEntryType.Error:
						cssClass = "EventError priority1";
						break;
					case EventLogEntryType.Warning:
						cssClass = "EventWarning priority1";
						break;
					case EventLogEntryType.Information:
						cssClass = "EventInformational priority1";
						break;
					case EventLogEntryType.SuccessAudit:
						cssClass = "EventSuccessAudit priority1";
						break;
					case EventLogEntryType.FailureAudit:
						cssClass = "EventFailureAudit priority1";
						break;
				}
				e.Row.Cells[1].CssClass = cssClass;

				//We need to convert the datetime in the EventTimeUtc field to the local time for the current user
				DateTime localDate = GlobalFunctions.LocalizeDate(evt.EventTimeUtc);
				e.Row.Cells[0].Text = localDate.ToString("g");
			}
		}

		/// <summary>
		/// Handles the click event on the item links in the User Management datagrid
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void grdEventLog_RowCommand(object source, GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = "grdEventLog_RowCommand";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Handle the appropriate command
			if (e.CommandName == "SortColumns")
			{
				//Update the sort
				SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_EVENT_LOG_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, e.CommandArgument.ToString());
				LoadAndBindData();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the event raised when the event pagination page index is changed
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void grdEventLog_PageIndexChanging(object source, GridViewPageEventArgs e)
		{
			const string METHOD_NAME = "grdEventLog_PageIndexChanging";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Set the new page index
				grdEventLog.CurrentPageIndex = e.NewPageIndex;
				UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_EVENT_LOG_GENERAL);
				paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW] = e.NewPageIndex;
				paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE] = grdEventLog.PageSize;
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

			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_EVENT_LOG_FILTERS);
			filterList.Clear();
			grdEventLog.CurrentPageIndex = 0;

			//First we need to scan the list of columns and then update the saved filters
			grdEventLog.Filters = filterList;
			grdEventLog.UpdateFilters();
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
			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_EVENT_LOG_FILTERS);
			filterList.Clear();
			filterList.Save();
			UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_EVENT_LOG_GENERAL);
			paginationSettings.Clear();
			paginationSettings.Save();

			//If we have a filter in the querystring need to do a redirect to clear it out
			if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_EVENT_CATEGORY]))
			{
				Response.Redirect("EventLog.aspx");
			}
			else
			{
				//Reload user list and Databind
				LoadAndBindData();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Loads the event log datagrid and populates the filter controls if necessary
		/// </summary>
		protected void LoadAndBindData()
		{
			const string METHOD_NAME = "LoadAndBindData";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Get the filter list as a local variable
			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_EVENT_LOG_FILTERS);

			//See if we have a passed-in event log category to filter by
			if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_EVENT_CATEGORY]))
			{
				string category = Request.QueryString[GlobalFunctions.PARAMETER_EVENT_CATEGORY].Trim();
				if (!filterList.ContainsKey("EventCategory"))
				{
					filterList.Add("EventCategory", category);
				}
			}

			//Get the current sort
			string sortExpression = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_EVENT_LOG_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, "EventTimeUtc DESC");

			//Now we need to update the dataset and re-databind the grid and its lookups
			int totalCount;
			int pageIndex = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_EVENT_LOG_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, 0);
			int pageSize = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_EVENT_LOG_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE, 15);
			grdEventLog.PageSize = pageSize;
			Business.EventManager eventManager = new Business.EventManager();
			int itemIndex = pageIndex * grdEventLog.PageSize;
			List<Event> events = eventManager.GetEvents(filterList, sortExpression, itemIndex, grdEventLog.PageSize, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out totalCount);
			grdEventLog.DataSource = events;
			grdEventLog.VirtualItemCount = totalCount;

			//If we are running SmarteQM, need to rename any mentions of Spira to product neutral
			if (ConfigurationSettings.Default.License_ProductType == "SmarteQM")
			{
				foreach (Event evt in events)
				{
					if (!String.IsNullOrEmpty(evt.Message))
					{
						evt.Message = evt.Message.Replace("Inflectra.SpiraTest", "Application");
					}
					if (!String.IsNullOrEmpty(evt.ExceptionType))
					{
						evt.ExceptionType = evt.ExceptionType.Replace("Inflectra.SpiraTest", "Application");
					}
					if (!String.IsNullOrEmpty(evt.Details))
					{
						evt.Details = evt.Details.Replace("Inflectra.SpiraTest", "Application");
					}
				}
			}


			//Finally we need to populate the filter and sort display controls on the grid
			grdEventLog.SortExpression = sortExpression;
			grdEventLog.Filters = filterList;

			try
			{
				grdEventLog.CurrentPageIndex = pageIndex;
				grdEventLog.DataBind();
			}
			catch (Exception)
			{
				pageIndex = 0;
				grdEventLog.CurrentPageIndex = pageIndex;
				SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_EVENT_LOG_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, pageIndex);
				grdEventLog.DataBind();
			}

			//Set the default button to the filter (when enter pressed)
			Form.DefaultButton = btnFilter.UniqueID;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
	}
}