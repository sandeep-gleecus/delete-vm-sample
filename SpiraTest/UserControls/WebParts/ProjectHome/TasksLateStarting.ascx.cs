using System;
using System.Collections;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Attributes;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the list of project overview information
	/// </summary>
	public partial class TasksLateStarting : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TasksLateStarting::";

		#region User Configurable Properties

		/// <summary>
		/// Stores how many rows of data to display, default is unlimited
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
		DefaultValue(5)
		]
		public int RowsToDisplay
		{
			get
			{
				return this.rowsToDisplay;
			}
			set
			{
                int rowsToDisplayMax = 50;
                this.rowsToDisplay = value < rowsToDisplayMax ? value : rowsToDisplayMax;
				//Force the data to reload
				LoadAndBindData();
			}
		}
		protected int rowsToDisplay = 5;

		#endregion

		/// <summary>
		/// Loads the control data
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			try
			{
                //Webparts that are initially in the catalog need to have their subtitle set here
                this.Subtitle = Resources.Main.Global_ViewAll;

				//Add the event handlers
				this.grdLateStartingTasks.RowDataBound += new GridViewRowEventHandler(grdLateStartingTasks_RowDataBound);

				//Now load the content
                if (WebPartVisible)
                {
                    LoadAndBindData();
                }
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//Don't rethrow as this is loaded by an update panel and can't redirect to error page
				if (this.Message != null)
				{
					this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
					this.Message.Type = MessageBox.MessageType.Error;
				}
			}
		}

		/// <summary>
		/// Returns a handle to the interface
		/// </summary>
		/// <returns>IWebPartReloadable</returns>
		[ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
		public IWebPartReloadable GetReloadable()
		{
			return this;
		}

		/// <summary>
		/// Loads the control data
		/// </summary>
		public void LoadAndBindData()
		{
			//Get the release id from settings
			int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

            //Set the navigate url for the name field
            ((NameDescriptionFieldEx)this.grdLateStartingTasks.Columns[1]).NavigateUrlFormat = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -3);

			Business.TaskManager taskManager = new Business.TaskManager();
			Hashtable filters = new Hashtable();
			//Add a release filter if one is specified
			if (releaseId > 0)
			{
				filters.Add("ReleaseId", releaseId);
			}
			filters.Add("ProgressId", 2);   //Progress = Starting Late
            List<TaskView> lateStartingTasks = taskManager.Retrieve(ProjectId, "TaskPriorityName", true, 1, RowsToDisplay, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
            this.grdLateStartingTasks.DataSource = lateStartingTasks;
			this.grdLateStartingTasks.DataBind();
		}

		/// <summary>
		/// Adds the selective formatting to the task list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdLateStartingTasks_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Don't touch headers, footers or subheaders
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
                TaskView taskView = (TaskView)(e.Row.DataItem);

				//Next lets handle the color of the priority column
				string cssClass = "";
                if (taskView.TaskPriorityId.HasValue)
				{
                    cssClass = taskView.TaskPriorityColor;
				}
				e.Row.Cells[3].CssClass = cssClass;

                if (taskView.StartDate.HasValue)
                {
                    LabelEx lblStartDate = (LabelEx)e.Row.Cells[4].FindControl("lblStartDate");
                    lblStartDate.Text = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(taskView.StartDate.Value));
                    lblStartDate.ToolTip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(taskView.StartDate.Value));
                }
			}
		}
	}
}